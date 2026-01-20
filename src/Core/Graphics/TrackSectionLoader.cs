using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Core.Graphics;

/// <summary>
/// Loads track sections from TRACK.TRS files.
/// Each section has:
/// - Position (X, Y, Z) - center of the section
/// - next/previous links for navigation  
/// - Face range (firstFace, numFaces) for mesh association
/// </summary>
public class TrackSectionLoader
{
    private readonly ILogger<TrackSectionLoader> _logger;
    private List<TrackSectionData> _sections = new();

    /// <summary>
    /// Raw track section data from TRACK.TRS file
    /// </summary>
    public struct TrackSectionData
    {
        public int NextJunction;      // -1 if no junction
        public int Previous;          // Index to previous section
        public int Next;              // Index to next section
        public int X;                 // Position X (PSX coords)
        public int Y;                 // Position Y (PSX coords)
        public int Z;                 // Position Z (PSX coords)
        public uint FirstFace;        // First face index in track mesh
        public ushort NumFaces;       // Number of faces for this section
        public ushort Flags;          // Section flags (JUMP, JUNCTION, etc)
    }

    public TrackSectionLoader(ILogger<TrackSectionLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads sections from TRACK.TRS file
    /// </summary>
    public void LoadSections(string filePath)
    {
        _sections.Clear();

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("[TRACK] TRACK.TRS file not found: {Path}", filePath);
            return;
        }

        try
        {
            byte[] data = File.ReadAllBytes(filePath);
            _logger.LogInformation("[TRACK] Loaded TRACK.TRS: {Bytes} bytes", data.Length);

            // Each TrackSection is 152 bytes (not 140!)
            // Structure: 6 int32 (24) + 116 padding + uint32 (4) + uint16 (2) + 4 padding + uint16 (2) = 152
            const int SECTION_SIZE = 152;
            int sectionCount = data.Length / SECTION_SIZE;

            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                for (int i = 0; i < sectionCount; i++)
                {
                    var section = ReadTrackSection(reader);
                    _sections.Add(section);
                }
            }

            _logger.LogInformation("[TRACK] Loaded {Count} track sections from {Path}", _sections.Count, filePath);

            // Calculate and log bounds
            if (_sections.Count > 0)
            {
                int minX = _sections.Min(s => s.X);
                int maxX = _sections.Max(s => s.X);
                int minY = _sections.Min(s => s.Y);
                int maxY = _sections.Max(s => s.Y);
                int minZ = _sections.Min(s => s.Z);
                int maxZ = _sections.Max(s => s.Z);

                _logger.LogInformation(
                    "[TRACK] Sections bounds (PSX coords): X[{MinX}, {MaxX}], Y[{MinY}, {MaxY}], Z[{MinZ}, {MaxZ}]",
                    minX, maxX, minY, maxY, minZ, maxZ);
                
                _logger.LogInformation(
                    "[TRACK] Sections bounds (world coords, scale 0.001): X[{MinX:F2}, {MaxX:F2}], Y[{MinY:F2}, {MaxY:F2}], Z[{MinZ:F2}, {MaxZ:F2}]",
                    minX * 0.001f, maxX * 0.001f, minY * 0.001f, maxY * 0.001f, minZ * 0.001f, maxZ * 0.001f);

                // Log first few sections for debugging
                _logger.LogDebug("[TRACK] First section: X={X}, Y={Y}, Z={Z}, Next={Next}", 
                    _sections[0].X, _sections[0].Y, _sections[0].Z, _sections[0].Next);
                
                if (_sections.Count > 1)
                {
                    _logger.LogDebug("[TRACK] Second section: X={X}, Y={Y}, Z={Z}, Next={Next}", 
                        _sections[1].X, _sections[1].Y, _sections[1].Z, _sections[1].Next);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("[TRACK] Error loading TRACK.TRS: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Reads a single track section from binary data
    /// Structure (152 bytes total):
    ///   0-3:   nextJunction (int32)
    ///   4-7:   previous (int32)
    ///   8-11:  next (int32)
    ///   12-15: x (int32)
    ///   16-19: y (int32)
    ///   20-23: z (int32)
    ///   24-139: padding (116 bytes)
    ///   140-143: firstFace (uint32)
    ///   144-145: numFaces (uint16)
    ///   146-149: padding (4 bytes)
    ///   150-151: flags (uint16)
    /// Total: 152 bytes
    /// </summary>
    private TrackSectionData ReadTrackSection(BinaryReader reader)
    {
        var section = new TrackSectionData();

        // Read fixed fields
        section.NextJunction = reader.ReadInt32();
        section.Previous = reader.ReadInt32();
        section.Next = reader.ReadInt32();
        section.X = reader.ReadInt32();
        section.Y = reader.ReadInt32();
        section.Z = reader.ReadInt32();

        // Skip 116 bytes of padding/unknown data
        reader.ReadBytes(116);

        // Read face data
        section.FirstFace = reader.ReadUInt32();
        section.NumFaces = reader.ReadUInt16();
        
        // Skip 4 bytes of padding
        reader.ReadBytes(4);
        
        // Read flags
        section.Flags = reader.ReadUInt16();

        return section;
    }

    /// <summary>
    /// Converts loaded sections to TrackSection objects for use in Track
    /// </summary>
    public void PopulateTrackSections(Track track)
    {
        track.Sections.Clear();
        
        const float TRACK_SCALE = 0.001f;  // PSX coords to world coords

        int sectionIndex = 0;
        foreach (var rawSection in _sections)
        {
            var scaledX = rawSection.X * TRACK_SCALE;
            var scaledY = rawSection.Y * TRACK_SCALE;
            var scaledZ = rawSection.Z * TRACK_SCALE;

            var section = new TrackSection
            {
                Center = new Vec3(scaledX, scaledY, scaledZ),
                SectionNumber = track.Sections.Count,
                FaceStart = (int)rawSection.FirstFace,
                FaceCount = rawSection.NumFaces,
                Flags = rawSection.Flags,
                Prev = null,
                Next = null,
                Junction = null
            };

            // Log first few sections for debugging
            if (sectionIndex < 3)
            {
                _logger.LogInformation(
                    "[TRACK] Section {Index}: PSX=({X}, {Y}, {Z}) -> World=({ScaledX:F2}, {ScaledY:F2}, {ScaledZ:F2}), Next={Next}",
                    sectionIndex, rawSection.X, rawSection.Y, rawSection.Z, 
                    scaledX, scaledY, scaledZ, rawSection.Next);
            }

            track.Sections.Add(section);
            sectionIndex++;
        }

        // Link sections together using Next indices
        for (int i = 0; i < _sections.Count; i++)
        {
            if (_sections[i].Next >= 0 && _sections[i].Next < track.Sections.Count)
            {
                track.Sections[i].Next = track.Sections[_sections[i].Next];
            }
            
            if (_sections[i].Previous >= 0 && _sections[i].Previous < track.Sections.Count)
            {
                track.Sections[i].Prev = track.Sections[_sections[i].Previous];
            }
        }

        _logger.LogInformation("[TRACK] Populated Track with {Count} sections from TRS file", track.Sections.Count);
    }

    /// <summary>
    /// Gets all loaded sections
    /// </summary>
    public List<TrackSectionData> GetSections() => _sections;
}

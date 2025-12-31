using ImGuiNET;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Tools.Core;
using WipeoutRewrite.Tools.Managers;
using SysVector2 = System.Numerics.Vector2;
using SysVector4 = System.Numerics.Vector4;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Diagnostic panel that displays detailed information about TRV (vertices) and TRF (faces) files.
/// Useful for understanding track geometry structure and debugging track loading issues.
/// </summary>
public class TrackDataPanel : ITrackDataPanel
{
    private readonly ILogger<TrackDataPanel> _logger;
    private readonly IScene _scene;
    private bool _isVisible = true;
    
    // Pagination for vertices
    private int _vertexPageSize = 50;
    private int _vertexCurrentPage = 0;
    
    // Pagination for faces
    private int _facePageSize = 20;
    private int _faceCurrentPage = 0;
    
    // Selected item for detailed view
    private int _selectedVertexIndex = -1;
    private int _selectedFaceIndex = -1;

    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    public TrackDataPanel(ILogger<TrackDataPanel> logger, IScene scene)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
    }

    public void Render()
    {
        if (!_isVisible) return;

        ImGui.SetNextWindowSize(new SysVector2(800, 600), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Track Data Inspector (TRV/TRF)", ref _isVisible))
        {
            var track = _scene.ActiveTrack;
            var trackLoader = _scene.TrackLoader;

            if (track == null || trackLoader == null)
            {
                ImGui.TextDisabled("No track loaded");
                ImGui.Text("Load a track from the Track Viewer panel to inspect its data.");
            }
            else
            {
                RenderTrackDataTabs(track, trackLoader);
            }
        }
        ImGui.End();
    }

    private void RenderTrackDataTabs(ITrack track, TrackLoader loader)
    {
        // Tab bar
        if (ImGui.BeginTabBar("TrackDataTabs"))
        {
            if (ImGui.BeginTabItem("Overview"))
            {
                RenderOverviewTab(track, loader);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Vertices (TRV)"))
            {
                RenderVerticesTab(loader);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Faces (TRF)"))
            {
                RenderFacesTab(loader);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Statistics"))
            {
                RenderStatisticsTab(track, loader);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void RenderOverviewTab(ITrack track, TrackLoader loader)
    {
        ImGui.Text($"Track: {track.Name}");
        ImGui.Separator();

        ImGui.Text("File Information:");
        ImGui.Indent();
        
        var vertices = GetVertices(loader);
        var faces = GetFaces(loader);
        
        if (vertices != null)
        {
            ImGui.BulletText($"TRV (Vertices): {vertices.Count} vertices");
            ImGui.SameLine();
            ImGui.TextDisabled($"({vertices.Count * 16} bytes)");
        }
        else
        {
            ImGui.BulletText("TRV (Vertices): Not loaded");
        }

        if (faces != null)
        {
            ImGui.BulletText($"TRF (Faces): {faces.Count} faces (quads)");
            ImGui.SameLine();
            ImGui.TextDisabled($"({faces.Count * 20} bytes)");
        }
        else
        {
            ImGui.BulletText("TRF (Faces): Not loaded");
        }

        ImGui.BulletText($"Sections: {track.Sections.Count}");
        
        ImGui.Unindent();
        ImGui.Separator();

        ImGui.Text("Data Structure:");
        ImGui.Indent();
        ImGui.BulletText("TRV Format: 4x int32 per vertex (X, Y, Z, padding)");
        ImGui.BulletText("TRF Format: 20 bytes per face");
        ImGui.Indent();
        ImGui.Text("- 4x int16: Vertex indices");
        ImGui.Text("- 3x int16: Normal (X, Y, Z)");
        ImGui.Text("- 1x uint8: Texture ID");
        ImGui.Text("- 1x uint8: Flags");
        ImGui.Text("- 1x uint32: Color (RGBA)");
        ImGui.Unindent();
        ImGui.Unindent();

        ImGui.Separator();
        ImGui.TextColored(new SysVector4(0.7f, 0.7f, 1.0f, 1.0f), 
            "Navigate to other tabs to inspect individual vertices and faces.");
    }

    private void RenderVerticesTab(TrackLoader loader)
    {
        var vertices = GetVertices(loader);
        if (vertices == null || vertices.Count == 0)
        {
            ImGui.TextDisabled("No vertices loaded");
            return;
        }

        ImGui.Text($"Total Vertices: {vertices.Count}");
        ImGui.Separator();

        // Pagination controls
        int totalPages = (vertices.Count + _vertexPageSize - 1) / _vertexPageSize;
        _vertexCurrentPage = Math.Clamp(_vertexCurrentPage, 0, totalPages - 1);

        ImGui.Text($"Page {_vertexCurrentPage + 1} of {totalPages}");
        ImGui.SameLine();
        if (ImGui.Button("<< Prev##verts") && _vertexCurrentPage > 0)
            _vertexCurrentPage--;
        ImGui.SameLine();
        if (ImGui.Button("Next >>##verts") && _vertexCurrentPage < totalPages - 1)
            _vertexCurrentPage++;
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.InputInt("Page Size##verts", ref _vertexPageSize);
        _vertexPageSize = Math.Clamp(_vertexPageSize, 10, 200);

        ImGui.Separator();

        // Vertex table
        if (ImGui.BeginTable("VerticesTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Y", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Length", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableHeadersRow();

            int startIdx = _vertexCurrentPage * _vertexPageSize;
            int endIdx = Math.Min(startIdx + _vertexPageSize, vertices.Count);

            for (int i = startIdx; i < endIdx; i++)
            {
                var v = vertices[i];
                ImGui.TableNextRow();
                
                ImGui.TableSetColumnIndex(0);
                bool isSelected = (_selectedVertexIndex == i);
                if (ImGui.Selectable($"{i}##vertex", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                {
                    _selectedVertexIndex = i;
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"{v.X:F2}");

                ImGui.TableSetColumnIndex(2);
                ImGui.Text($"{v.Y:F2}");

                ImGui.TableSetColumnIndex(3);
                ImGui.Text($"{v.Z:F2}");

                ImGui.TableSetColumnIndex(4);
                float length = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
                ImGui.Text($"{length:F2}");
            }

            ImGui.EndTable();
        }

        // Selected vertex details
        if (_selectedVertexIndex >= 0 && _selectedVertexIndex < vertices.Count)
        {
            ImGui.Separator();
            ImGui.Text("Selected Vertex Details:");
            var v = vertices[_selectedVertexIndex];
            ImGui.Indent();
            ImGui.Text($"Index: {_selectedVertexIndex}");
            ImGui.Text($"Position: ({v.X:F3}, {v.Y:F3}, {v.Z:F3})");
            float length = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
            ImGui.Text($"Distance from origin: {length:F3}");
            ImGui.Unindent();
        }
    }

    private void RenderFacesTab(TrackLoader loader)
    {
        var faces = GetFaces(loader);
        if (faces == null || faces.Count == 0)
        {
            ImGui.TextDisabled("No faces loaded");
            return;
        }

        ImGui.Text($"Total Faces: {faces.Count}");
        ImGui.Separator();

        // Pagination controls
        int totalPages = (faces.Count + _facePageSize - 1) / _facePageSize;
        _faceCurrentPage = Math.Clamp(_faceCurrentPage, 0, totalPages - 1);

        ImGui.Text($"Page {_faceCurrentPage + 1} of {totalPages}");
        ImGui.SameLine();
        if (ImGui.Button("<< Prev##faces") && _faceCurrentPage > 0)
            _faceCurrentPage--;
        ImGui.SameLine();
        if (ImGui.Button("Next >>##faces") && _faceCurrentPage < totalPages - 1)
            _faceCurrentPage++;
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.InputInt("Page Size##faces", ref _facePageSize);
        _facePageSize = Math.Clamp(_facePageSize, 10, 100);

        ImGui.Separator();

        // Face table
        if (ImGui.BeginTable("FacesTable", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Vertices", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("Normal", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("Texture", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Flags", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Color", ImGuiTableColumnFlags.WidthFixed, 80);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableHeadersRow();

            int startIdx = _faceCurrentPage * _facePageSize;
            int endIdx = Math.Min(startIdx + _facePageSize, faces.Count);

            for (int i = startIdx; i < endIdx; i++)
            {
                var f = faces[i];
                ImGui.TableNextRow();
                
                ImGui.TableSetColumnIndex(0);
                bool isSelected = (_selectedFaceIndex == i);
                if (ImGui.Selectable($"{i}##face", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                {
                    _selectedFaceIndex = i;
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"[{f.VertexIndices[0]},{f.VertexIndices[1]},{f.VertexIndices[2]},{f.VertexIndices[3]}]");

                ImGui.TableSetColumnIndex(2);
                ImGui.Text($"({f.NormalX:F1},{f.NormalY:F1},{f.NormalZ:F1})");

                ImGui.TableSetColumnIndex(3);
                ImGui.Text($"{f.TextureId}");

                ImGui.TableSetColumnIndex(4);
                ImGui.Text($"0x{f.Flags:X2}");

                ImGui.TableSetColumnIndex(5);
                var color = new SysVector4(f.Color.R / 255f, f.Color.G / 255f, f.Color.B / 255f, 1.0f);
                ImGui.ColorButton($"##color{i}", color, ImGuiColorEditFlags.NoAlpha, new SysVector2(40, 20));

                ImGui.TableSetColumnIndex(6);
                ImGui.Text(GetFaceTypeString(f.Flags));
            }

            ImGui.EndTable();
        }

        // Selected face details
        if (_selectedFaceIndex >= 0 && _selectedFaceIndex < faces.Count)
        {
            ImGui.Separator();
            ImGui.Text("Selected Face Details:");
            var f = faces[_selectedFaceIndex];
            ImGui.Indent();
            ImGui.Text($"Index: {_selectedFaceIndex}");
            ImGui.Text($"Vertex Indices: [{f.VertexIndices[0]}, {f.VertexIndices[1]}, {f.VertexIndices[2]}, {f.VertexIndices[3]}]");
            ImGui.Text($"Normal: ({f.NormalX:F3}, {f.NormalY:F3}, {f.NormalZ:F3})");
            ImGui.Text($"Texture ID: {f.TextureId}");
            ImGui.Text($"Flags: 0x{f.Flags:X2} ({Convert.ToString(f.Flags, 2).PadLeft(8, '0')})");
            ImGui.Indent();
            RenderFaceFlags(f.Flags);
            ImGui.Unindent();
            ImGui.Text($"Color: RGBA({f.Color.R}, {f.Color.G}, {f.Color.B}, {f.Color.A})");
            ImGui.Unindent();
        }
    }

    private void RenderStatisticsTab(ITrack track, TrackLoader loader)
    {
        var vertices = GetVertices(loader);
        var faces = GetFaces(loader);

        ImGui.Text("Track Geometry Statistics:");
        ImGui.Separator();

        if (vertices != null && vertices.Count > 0)
        {
            ImGui.Text("Vertex Statistics:");
            ImGui.Indent();

            // Calculate bounds
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (var v in vertices)
            {
                minX = Math.Min(minX, v.X);
                maxX = Math.Max(maxX, v.X);
                minY = Math.Min(minY, v.Y);
                maxY = Math.Max(maxY, v.Y);
                minZ = Math.Min(minZ, v.Z);
                maxZ = Math.Max(maxZ, v.Z);
            }

            ImGui.Text($"Total Vertices: {vertices.Count}");
            ImGui.Text($"Bounds X: [{minX:F2}, {maxX:F2}] (width: {maxX - minX:F2})");
            ImGui.Text($"Bounds Y: [{minY:F2}, {maxY:F2}] (height: {maxY - minY:F2})");
            ImGui.Text($"Bounds Z: [{minZ:F2}, {maxZ:F2}] (depth: {maxZ - minZ:F2})");
            ImGui.Text($"Center: ({(minX + maxX) / 2:F2}, {(minY + maxY) / 2:F2}, {(minZ + maxZ) / 2:F2})");

            ImGui.Unindent();
            ImGui.Separator();
        }

        if (faces != null && faces.Count > 0)
        {
            ImGui.Text("Face Statistics:");
            ImGui.Indent();

            // Count face types by flags
            int trackFaces = 0, weaponFaces = 0, boostFaces = 0, wallFaces = 0;
            var textureUsage = new Dictionary<byte, int>();

            foreach (var f in faces)
            {
                if ((f.Flags & 0x01) != 0) trackFaces++;
                if ((f.Flags & 0x02) != 0 || (f.Flags & 0x08) != 0) weaponFaces++;
                if ((f.Flags & 0x20) != 0) boostFaces++;
                if ((f.Flags & 0x01) == 0) wallFaces++;

                if (!textureUsage.ContainsKey(f.TextureId))
                    textureUsage[f.TextureId] = 0;
                textureUsage[f.TextureId]++;
            }

            ImGui.Text($"Total Faces: {faces.Count}");
            ImGui.Text($"Track Surface Faces: {trackFaces}");
            ImGui.Text($"Weapon Pad Faces: {weaponFaces}");
            ImGui.Text($"Boost Pad Faces: {boostFaces}");
            ImGui.Text($"Wall Faces: {wallFaces}");
            ImGui.Text($"Unique Textures Used: {textureUsage.Count}");

            ImGui.Separator();
            ImGui.Text("Top 10 Most Used Textures:");
            ImGui.Indent();
            var sortedTextures = textureUsage.OrderByDescending(kvp => kvp.Value).Take(10);
            foreach (var tex in sortedTextures)
            {
                ImGui.BulletText($"Texture {tex.Key}: {tex.Value} faces ({(tex.Value * 100.0f / faces.Count):F1}%)");
            }
            ImGui.Unindent();

            ImGui.Unindent();
            ImGui.Separator();
        }

        ImGui.Text($"Track Sections: {track.Sections.Count}");
        if (faces != null && track.Sections.Count > 0)
        {
            ImGui.Indent();
            float avgFacesPerSection = (float)faces.Count / track.Sections.Count;
            ImGui.Text($"Average Faces per Section: {avgFacesPerSection:F1}");
            ImGui.Unindent();
        }
    }

    private string GetFaceTypeString(byte flags)
    {
        var types = new List<string>();
        if ((flags & 0x01) != 0) types.Add("Track");
        if ((flags & 0x02) != 0) types.Add("Weapon");
        if ((flags & 0x04) != 0) types.Add("Flip");
        if ((flags & 0x08) != 0) types.Add("Weapon2");
        if ((flags & 0x10) != 0) types.Add("Unknown");
        if ((flags & 0x20) != 0) types.Add("Boost");

        return types.Count > 0 ? string.Join(", ", types) : "Wall";
    }

    private void RenderFaceFlags(byte flags)
    {
        if ((flags & 0x01) != 0) ImGui.BulletText("TRACK (0x01) - Track surface");
        if ((flags & 0x02) != 0) ImGui.BulletText("WEAPON (0x02) - Weapon pad");
        if ((flags & 0x04) != 0) ImGui.BulletText("FLIP (0x04) - Flip texture UV");
        if ((flags & 0x08) != 0) ImGui.BulletText("WEAPON_2 (0x08) - Weapon pad variant");
        if ((flags & 0x10) != 0) ImGui.BulletText("UNKNOWN (0x10) - Unknown flag");
        if ((flags & 0x20) != 0) ImGui.BulletText("BOOST (0x20) - Boost pad");
        
        if (flags == 0)
            ImGui.BulletText("WALL (0x00) - Wall/barrier");
    }

    private List<TrackLoader.TrackVertex>? GetVertices(TrackLoader loader)
    {
        return loader.LoadedVertices;
    }

    private List<TrackLoader.TrackFace>? GetFaces(TrackLoader loader)
    {
        return loader.LoadedFaces;
    }
}

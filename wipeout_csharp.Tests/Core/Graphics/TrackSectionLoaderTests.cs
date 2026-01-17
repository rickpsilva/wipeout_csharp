using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Core.Graphics;
using Xunit;

namespace WipeoutRewrite.Tests.Core.Graphics;

public class TrackSectionLoaderTests
{
    [Fact]
    public void LoadSections_MissingFile_LeavesEmptySet()
    {
        var loader = new TrackSectionLoader(NullLogger<TrackSectionLoader>.Instance);
        loader.LoadSections(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".trs"));

        Assert.Empty(loader.GetSections());
    }

    [Fact]
    public void LoadSections_ReadsDataAndPopulatesTrack()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".trs");

        try
        {
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                WriteSection(writer, nextJunction: -1, previous: -1, next: 1, x: 1000, y: 2000, z: 3000, firstFace: 42, numFaces: 3, flags: 0xAAAA);
                WriteSection(writer, nextJunction: -1, previous: 0, next: -1, x: -1000, y: -2000, z: 0, firstFace: 99, numFaces: 4, flags: 0xBBBB);
            }

            var loader = new TrackSectionLoader(LoggerFactory.Create(b => { }).CreateLogger<TrackSectionLoader>());

            loader.LoadSections(tempPath);

            var sections = loader.GetSections();
            Assert.Equal(2, sections.Count);
            Assert.Equal(1, sections[0].Next);
            Assert.Equal(0, sections[1].Previous);

            var track = new Track(LoggerFactory.Create(b => { }).CreateLogger<Track>());

            loader.PopulateTrackSections(track);

            Assert.Equal(2, track.Sections.Count);

            Assert.InRange(track.Sections[0].Center.X, 0.999f, 1.001f);
            Assert.InRange(track.Sections[0].Center.Y, 1.999f, 2.001f);
            Assert.InRange(track.Sections[0].Center.Z, 2.999f, 3.001f);
            Assert.Equal(42, track.Sections[0].FaceStart);
            Assert.Equal(3, track.Sections[0].FaceCount);

            Assert.Equal(track.Sections[1], track.Sections[0].Next);
            Assert.Equal(track.Sections[0], track.Sections[1].Prev);
            Assert.Equal(99, track.Sections[1].FaceStart);
            Assert.Equal(4, track.Sections[1].FaceCount);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static void WriteSection(BinaryWriter writer, int nextJunction, int previous, int next, int x, int y, int z, uint firstFace, ushort numFaces, ushort flags)
    {
        writer.Write(nextJunction);
        writer.Write(previous);
        writer.Write(next);
        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
        writer.Write(new byte[116]);
        writer.Write(firstFace);
        writer.Write(numFaces);
        writer.Write(new byte[4]);
        writer.Write(flags);
    }
}
using System;
using System.Buffers.Binary;
using System.IO;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Graphics;
using Xunit;

namespace WipeoutRewrite.Tests.Core.Graphics;

public class TrackLoaderTests
{
    [Fact]
    public void LoadVertices_MissingFile_Throws()
    {
        var loader = new TrackLoader(LoggerFactory.Create(b => { }).CreateLogger<TrackLoader>());
        Assert.Throws<FileNotFoundException>(() => loader.LoadVertices(Path.Combine(Path.GetTempPath(), "track_missing.trv")));
    }

    [Fact]
    public void ConvertToMesh_BuildsTrianglesFromTrackFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            string trvPath = Path.Combine(tempDir, "track.trv");
            string trfPath = Path.Combine(tempDir, "track.trf");

            File.WriteAllBytes(trvPath, BuildVerticesBytes());
            File.WriteAllBytes(trfPath, BuildFaceBytes());

            var loader = new TrackLoader(LoggerFactory.Create(b => { }).CreateLogger<TrackLoader>());

            loader.LoadVertices(trvPath);
            loader.LoadFaces(trfPath);

            var mesh = loader.ConvertToMesh();

            Assert.Equal(4, mesh.Vertices.Length);
            Assert.Equal(2, mesh.Primitives.Count);

            var tri0 = Assert.IsType<FT3>(mesh.Primitives[0]);
            var tri1 = Assert.IsType<FT3>(mesh.Primitives[1]);

            Assert.Equal((byte)0x11, tri0.Color.r);
            Assert.Equal((byte)0x22, tri0.Color.g);
            Assert.Equal((byte)0x33, tri0.Color.b);
            Assert.Equal(255, tri0.Color.a);
            Assert.Equal(tri0.Color, tri1.Color);

            Assert.Equal((byte)0, tri0.UVs[0].u);   // uv[2]
            Assert.Equal((byte)128, tri0.UVs[2].u); // uv[0]
            Assert.Equal((byte)0, tri1.UVs[0].u);   // uv[2]
            Assert.Equal((byte)128, tri1.UVs[1].u); // uv[0]

            Assert.All(mesh.Normals, n =>
            {
                Assert.InRange(n.X, 0.999f, 1.001f);
                Assert.InRange(n.Y, -0.001f, 0.001f);
                Assert.InRange(n.Z, -0.001f, 0.001f);
            });
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static byte[] BuildVerticesBytes()
    {
        using var stream = new MemoryStream();
        WriteVertex(stream, 0, 0, 0);
        WriteVertex(stream, 1000, 0, 0);
        WriteVertex(stream, 0, 1000, 0);
        WriteVertex(stream, 0, 0, 1000);
        return stream.ToArray();
    }

    private static byte[] BuildFaceBytes()
    {
        using var stream = new MemoryStream();
        WriteInt16BE(stream, 0);
        WriteInt16BE(stream, 1);
        WriteInt16BE(stream, 2);
        WriteInt16BE(stream, 3);

        WriteInt16BE(stream, 4096); // normal.x = 1.0f
        WriteInt16BE(stream, 0);
        WriteInt16BE(stream, 0);

        stream.WriteByte(5);  // texture ID
        stream.WriteByte(0);  // flags

        WriteUInt32BE(stream, 0x11223344);

        return stream.ToArray();
    }

    private static void WriteVertex(Stream stream, int x, int y, int z)
    {
        WriteInt32BE(stream, x);
        WriteInt32BE(stream, y);
        WriteInt32BE(stream, z);
        WriteInt32BE(stream, 0); // padding
    }

    private static void WriteInt16BE(Stream stream, short value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        stream.Write(buffer);
    }

    private static void WriteInt32BE(Stream stream, int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        stream.Write(buffer);
    }

    private static void WriteUInt32BE(Stream stream, uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        stream.Write(buffer);
    }
}
using Xunit;
using WipeoutRewrite.Core.Graphics;
using Microsoft.Extensions.Logging.Abstractions;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for 3D model structures and loading.
/// Tests mesh creation, primitives, and mock loader functionality.
/// </summary>
public class ModelTests
{
    [Fact]
    public void Mesh_Constructor_ShouldInitializeWithName()
    {
        var mesh = new Mesh("TestShip");
        
        Assert.Equal("TestShip", mesh.Name);
        Assert.NotNull(mesh.Vertices);
        Assert.NotNull(mesh.Normals);
        Assert.NotNull(mesh.Primitives);
        Assert.Empty(mesh.Vertices);
        Assert.Empty(mesh.Normals);
        Assert.Empty(mesh.Primitives);
    }

    [Fact]
    public void FT3_Constructor_ShouldSetType()
    {
        var primitive = new FT3();
        
        Assert.Equal(PrimitiveType.FT3, primitive.Type);
        Assert.NotNull(primitive.CoordIndices);
        Assert.Equal(3, primitive.CoordIndices.Length);
        Assert.NotNull(primitive.UVs);
        Assert.Equal(3, primitive.UVs.Length);
    }

    [Fact]
    public void GT3_Constructor_ShouldSetType()
    {
        var primitive = new GT3();
        
        Assert.Equal(PrimitiveType.GT3, primitive.Type);
        Assert.NotNull(primitive.CoordIndices);
        Assert.Equal(3, primitive.CoordIndices.Length);
        Assert.NotNull(primitive.Colors);
        Assert.Equal(3, primitive.Colors.Length);
    }

    [Fact]
    public void F3_Constructor_ShouldSetType()
    {
        var primitive = new F3();
        
        Assert.Equal(PrimitiveType.F3, primitive.Type);
        Assert.NotNull(primitive.CoordIndices);
        Assert.Equal(3, primitive.CoordIndices.Length);
    }

    [Fact]
    public void ModelLoader_CreateMockShipModel_ShouldReturnValidMesh()
    {
        var loader = new ModelLoader(NullLogger<ModelLoader>.Instance);
        
        var mesh = loader.CreateMockShipModel("Feisar");
        
        Assert.NotNull(mesh);
        Assert.Equal("Feisar", mesh.Name);
        Assert.NotEmpty(mesh.Vertices);
        Assert.NotEmpty(mesh.Normals);
        Assert.NotEmpty(mesh.Primitives);
    }

    [Fact]
    public void ModelLoader_CreateMockShipModel_ShouldHaveCorrectVertexCount()
    {
        var loader = new ModelLoader();
        
        var mesh = loader.CreateMockShipModel("TestShip");
        
        // Mock model has 8 vertices
        Assert.Equal(8, mesh.Vertices.Length);
    }

    [Fact]
    public void ModelLoader_CreateMockShipModel_ShouldHaveTriangles()
    {
        var loader = new ModelLoader();
        
        var mesh = loader.CreateMockShipModel("TestShip");
        
        // Should have multiple triangles
        Assert.True(mesh.Primitives.Count > 0);
        Assert.All(mesh.Primitives, p => Assert.IsType<FT3>(p));
    }

    [Fact]
    public void ModelLoader_CreateMockShipModel_VerticesShouldFormShipShape()
    {
        var loader = new ModelLoader();
        
        var mesh = loader.CreateMockShipModel("TestShip");
        
        // Check nose is at front (positive Z)
        var nose = mesh.Vertices[0];
        Assert.Equal(384, nose.Z);
        
        // Check radius is calculated
        Assert.True(mesh.Radius > 0);
    }

    [Fact]
    public void FT3_CoordIndices_ShouldReferenceValidVertices()
    {
        var loader = new ModelLoader();
        var mesh = loader.CreateMockShipModel("TestShip");
        
        foreach (var primitive in mesh.Primitives)
        {
            if (primitive is FT3 ft3)
            {
                // All coord indices should be within vertex array bounds
                Assert.True(ft3.CoordIndices[0] >= 0 && ft3.CoordIndices[0] < mesh.Vertices.Length);
                Assert.True(ft3.CoordIndices[1] >= 0 && ft3.CoordIndices[1] < mesh.Vertices.Length);
                Assert.True(ft3.CoordIndices[2] >= 0 && ft3.CoordIndices[2] < mesh.Vertices.Length);
            }
        }
    }

    [Fact]
    public void FT3_UVs_ShouldBeInValidRange()
    {
        var loader = new ModelLoader();
        var mesh = loader.CreateMockShipModel("TestShip");
        
        foreach (var primitive in mesh.Primitives)
        {
            if (primitive is FT3 ft3)
            {
                // UV coordinates should be 0-255 (byte range)
                Assert.InRange(ft3.UVs[0].u, 0, 255);
                Assert.InRange(ft3.UVs[0].v, 0, 255);
                Assert.InRange(ft3.UVs[1].u, 0, 255);
                Assert.InRange(ft3.UVs[1].v, 0, 255);
                Assert.InRange(ft3.UVs[2].u, 0, 255);
                Assert.InRange(ft3.UVs[2].v, 0, 255);
            }
        }
    }

    [Fact]
    public void ModelLoader_LoadFromPrmFile_ShouldThrowNotImplemented()
    {
        var loader = new ModelLoader();
        
        Assert.Throws<NotImplementedException>(() => 
            loader.LoadFromPrmFile("wipeout/common/allsh.prm"));
    }
}


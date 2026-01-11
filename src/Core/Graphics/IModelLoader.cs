using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Core.Graphics;

/// <summary>
/// Interface for loading and parsing PRM model files.
/// </summary>
public interface IModelLoader
{
    /// <summary>
    /// Creates a simple mock model for testing when PRM loading fails
    /// </summary>
    Mesh CreateMockModel(string name);

    /// <summary>
    /// Creates a mock model with scaled geometry - Wipeout-style futuristic racer
    /// </summary>
    Mesh CreateMockModelScaled(string name, float scale = 1.0f);

    /// <summary>
    /// Scans a PRM file and returns a list of object names and their indices.
    /// This is useful for displaying available models in a PRM file without loading them all.
    /// </summary>
    List<(int index, string name)> GetObjectsInPrmFile(string filepath);

    /// <summary>
    /// Load a PRM file and return a Mesh.
    /// Directly ported from objects_load() in object.c
    /// </summary>
    Mesh LoadFromPrmFile(string filepath, int objectIndex = 0);

    /// <summary>
    /// Load ALL objects from a PRM file and return them as a list.
    /// This matches the behavior of objects_load() in object.c which returns a linked list of ALL objects.
    /// Useful for scene.prm and sky.prm files that contain multiple models.
    /// </summary>
    List<Mesh> LoadAllObjectsFromPrmFile(string filepath);
}

using Microsoft.Extensions.Logging;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Infrastructure.Assets;

namespace WipeoutRewrite.Core.Entities;

/// <summary>
/// GameObjectCollection: Manages a collection of game objects (ships, props, etc.).
/// Handles initialization, rendering, and updates for all objects in the collection.
/// </summary>
public class GameObjectCollection : IGameObjectCollection
{
    public List<GameObject> GetAll { get; private set; } = new();

    private readonly IGameObjectFactory _gameObjectFactory;
    private readonly ILogger<GameObjectCollection> _logger;

    public GameObjectCollection(ILogger<GameObjectCollection> logger,
                 IGameObjectFactory gameObjectFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameObjectFactory = gameObjectFactory ?? throw new ArgumentNullException(nameof(gameObjectFactory));
    }

    #region methods

    public void Clear()
    {
        GetAll.Clear();
    }

    /// <summary>
    /// Get all objects from a specific category.
    /// </summary>
    public List<GameObject> GetByCategory(GameObjectCategory category)
    {
        return GetAll.Where(o => o.Category == category).ToList();
    }

    /// <summary>
    /// Get a specific object by name.
    /// </summary>
    public GameObject? GetByName(string name)
    {
        return GetAll.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all available categories in the collection.
    /// </summary>
    public List<GameObjectCategory> GetCategories()
    {
        return GetAll.Select(o => o.Category).Distinct().OrderBy(c => c).ToList();
    }

    public void Init(TrackSection? startSection)
    {
        // Find assets root directory
        string? assetsRoot = null;
        foreach (var path in AssetPaths.AssetSearchPaths)
        {
            if (Directory.Exists(path))
            {
                assetsRoot = path;
                _logger.LogInformation("[GameObjectCollection] Found assets at: {Path}", Path.GetFullPath(path));
                break;
            }
        }

        if (assetsRoot == null)
        {
            _logger.LogWarning("[GameObjectCollection] Could not find assets directory");
            return;
        }

        int globalObjectId = 0;

        // Load common objects with categories
        string commonPath = System.IO.Path.Combine(assetsRoot, "common");
        if (System.IO.Directory.Exists(commonPath))
        {
            // Ships (8 objects from allsh.prm)
            globalObjectId = LoadCategorizedPrm(commonPath, "allsh.prm", GameObjectCategory.Ship, 8, globalObjectId);

            // UI/Menu
            // msdos.prm contains 4 objects: [0]=championship, [1]=msdos, [2]=single_race, [3]=options/extru1
            globalObjectId = LoadCategorizedPrm(commonPath, "msdos.prm", GameObjectCategory.MsDos, 4, globalObjectId);
            globalObjectId = LoadCategorizedPrm(commonPath, "teams.prm", GameObjectCategory.Teams, 1, globalObjectId);
            // alopt.prm contains 5 objects: stopwatch, save, load, headphones, cd
            globalObjectId = LoadCategorizedPrm(commonPath, "alopt.prm", GameObjectCategory.Options, 5, globalObjectId);

            // Weapons
            globalObjectId = LoadCategorizedPrm(commonPath, "miss.prm", GameObjectCategory.Weapon, 1, globalObjectId);
            globalObjectId = LoadCategorizedPrm(commonPath, "mine.prm", GameObjectCategory.Weapon, 1, globalObjectId);
            globalObjectId = LoadCategorizedPrm(commonPath, "ebolt.prm", GameObjectCategory.Weapon, 1, globalObjectId);

            // Pickups
            globalObjectId = LoadCategorizedPrm(commonPath, "alcol.prm", GameObjectCategory.Pickup, 1, globalObjectId);
            globalObjectId = LoadCategorizedPrm(commonPath, "shld.prm", GameObjectCategory.Pickup, 1, globalObjectId);

            // Props/Obstacles
            globalObjectId = LoadCategorizedPrm(commonPath, "rescu.prm", GameObjectCategory.Prop, 1, globalObjectId);
            globalObjectId = LoadCategorizedPrm(commonPath, "rock.prm", GameObjectCategory.Obstacle, 1, globalObjectId);
            globalObjectId = LoadCategorizedPrm(commonPath, "pad1.prm", GameObjectCategory.Prop, 1, globalObjectId);

            // Pilot (pilot.prm has 8 pilots, leeg.prm has 2 objects)
            globalObjectId = LoadCategorizedPrm(commonPath, "pilot.prm", GameObjectCategory.Pilot, 8, globalObjectId);
            globalObjectId = LoadCategorizedPrm(commonPath, "leeg.prm", GameObjectCategory.Pilot, 2, globalObjectId);
        }

        // Load tracks (sky.prm and scene.prm for each track)
        for (int trackNum = 1; trackNum <= 14; trackNum++)
        {
            string trackFolder = System.IO.Path.Combine(assetsRoot, $"track{trackNum:D2}");
            if (System.IO.Directory.Exists(trackFolder))
            {
                var category = (GameObjectCategory)Enum.Parse(typeof(GameObjectCategory), $"Track{trackNum:D2}");

                // Load sky.prm for this track
                globalObjectId = LoadCategorizedPrm(trackFolder, "sky.prm", category, 1, globalObjectId, $"Track{trackNum:D2}_Sky");

                // Load scene.prm for this track
                globalObjectId = LoadCategorizedPrm(trackFolder, "scene.prm", category, 1, globalObjectId, $"Track{trackNum:D2}_Scene");
            }
        }

        _logger.LogInformation("[GameObjectCollection] Loaded {Count} game objects total", GetAll.Count);
        _logger.LogInformation("[GameObjectCollection] Categories: {Categories}",
            string.Join(", ", GetAll.Select(o => o.Category).Distinct().OrderBy(c => c)));
    }

    public void Renderer()
    {
        // Stub: original draws ships. Preview layer should call Draw() on instances.
        foreach (var s in GetAll)
            s.Draw();
    }

    public void ResetExhaustPlumes()
    {
        foreach (var s in GetAll)
            s.ResetExhaustPlume();
    }

    public void Update()
    {
        foreach (var s in GetAll)
            s.Update();
    }

    private int LoadCategorizedPrm(string directory, string fileName, GameObjectCategory category,
                                   int objectCount, int startId, string? customNamePrefix = null)
    {
        string prmPath = System.IO.Path.Combine(directory, fileName);
        if (!System.IO.File.Exists(prmPath))
        {
            _logger.LogDebug("[GameObjectCollection] PRM file not found: {Path}", fileName);
            return startId;
        }

        string baseName = customNamePrefix ?? System.IO.Path.GetFileNameWithoutExtension(fileName);

        for (int i = 0; i < objectCount; i++)
        {
            var model = _gameObjectFactory.CreateModel() as GameObject;
            if (model != null)
            {
                model.Name = objectCount > 1 ? $"{baseName}_{i}" : baseName;
                model.Category = category;
                model.SetGameObjectId(i); // Use internal setter instead of reflection
                model.LoadModelFromPath(prmPath, i);

                GetAll.Add(model);
                _logger.LogInformation("[GameObjectCollection] [{Category}] Loaded object {CategoryIndex}: {Name}",
                                     category, i, model.Name);
                startId++;
            }
        }

        return startId;
    }

    #endregion 
}
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Infrastructure.Assets;

namespace WipeoutRewrite.Core.Entities;

/// <summary>
/// Manages a collection of game objects (ships, props, etc.).
/// Handles initialization, rendering, and updates for all objects in the collection.
/// </summary>
public class GameObjectCollection : IGameObjectCollection
{
    public List<GameObject> GetAll { get; private set; } = new();

    private readonly IGameObjectFactory _gameObjectFactory;
    private readonly ILogger<GameObjectCollection> _logger;

    public GameObjectCollection(ILogger<GameObjectCollection> logger, IGameObjectFactory gameObjectFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameObjectFactory = gameObjectFactory ?? throw new ArgumentNullException(nameof(gameObjectFactory));
    }

    public void Clear() => GetAll.Clear();

    public List<GameObject> GetByCategory(GameObjectCategory category) =>
        GetAll.Where(o => o.Category == category).ToList();

    public GameObject? GetByName(string name) =>
        GetAll.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public List<GameObjectCategory> GetCategories() =>
        GetAll.Select(o => o.Category).Distinct().OrderBy(c => c).ToList();

    public void Init(TrackSection? startSection)
    {
        var assetsRoot = AssetPaths.AssetSearchPaths.FirstOrDefault(p => Directory.Exists(p));
        if (assetsRoot == null)
        {
            _logger.LogWarning("[GameObjectCollection] Could not find assets directory");
            return;
        }

        _logger.LogInformation("[GameObjectCollection] Found assets at: {Path}", Path.GetFullPath(assetsRoot));

        int id = LoadCommonAssets(assetsRoot);
        LoadTrackAssets(assetsRoot, ref id);

        _logger.LogInformation("[GameObjectCollection] Loaded {Count} objects | Categories: {Cats}",
            GetAll.Count, string.Join(", ", GetCategories()));
    }

    public void Renderer() => GetAll.ForEach(obj => obj.Draw());
    public void ResetExhaustPlumes() => GetAll.ForEach(obj => obj.ResetExhaustPlume());
    public void Update() => GetAll.ForEach(obj => obj.Update());

    private int LoadCommonAssets(string assetsRoot)
    {
        string commonPath = Path.Combine(assetsRoot, "common");
        if (!Directory.Exists(commonPath))
            return 0;

        int id = 0;
        id = LoadAsset(commonPath, "allsh.prm", GameObjectCategory.Ship, 8, id);
        id = LoadAsset(commonPath, "msdos.prm", GameObjectCategory.MsDos, 4, id);
        id = LoadAsset(commonPath, "teams.prm", GameObjectCategory.Teams, 1, id);
        id = LoadAsset(commonPath, "alopt.prm", GameObjectCategory.Options, 5, id);
        id = LoadAsset(commonPath, "miss.prm", GameObjectCategory.Weapon, 1, id);
        id = LoadAsset(commonPath, "mine.prm", GameObjectCategory.Weapon, 1, id);
        id = LoadAsset(commonPath, "ebolt.prm", GameObjectCategory.Weapon, 1, id);
        id = LoadAsset(commonPath, "alcol.prm", GameObjectCategory.Pickup, 1, id);
        id = LoadAsset(commonPath, "shld.prm", GameObjectCategory.Pickup, 1, id);
        id = LoadAsset(commonPath, "rescu.prm", GameObjectCategory.Prop, 1, id);
        id = LoadAsset(commonPath, "rock.prm", GameObjectCategory.Obstacle, 1, id);
        id = LoadAsset(commonPath, "pad1.prm", GameObjectCategory.Prop, 1, id);
        id = LoadAsset(commonPath, "pilot.prm", GameObjectCategory.Pilot, 8, id);
        id = LoadAsset(commonPath, "leeg.prm", GameObjectCategory.Pilot, 2, id);
        
        return id;
    }

    private void LoadTrackAssets(string assetsRoot, ref int id)
    {
        for (int trackNum = 1; trackNum <= 14; trackNum++)
        {
            string trackPath = Path.Combine(assetsRoot, $"track{trackNum:D2}");
            if (!Directory.Exists(trackPath))
                continue;

            var category = (GameObjectCategory)Enum.Parse(typeof(GameObjectCategory), $"Track{trackNum:D2}");
            id = LoadAsset(trackPath, "sky.prm", category, 1, id, $"Track{trackNum:D2}_Sky");
            id = LoadAsset(trackPath, "scene.prm", category, 1, id, $"Track{trackNum:D2}_Scene");
        }
    }

    private int LoadAsset(string directory, string fileName, GameObjectCategory category,
                          int objectCount, int startId, string? customNamePrefix = null)
    {
        string prmPath = Path.Combine(directory, fileName);
        if (!File.Exists(prmPath))
            return startId;

        string baseName = customNamePrefix ?? Path.GetFileNameWithoutExtension(fileName);

        for (int i = 0; i < objectCount; i++)
        {
            if (_gameObjectFactory.CreateModel() is GameObject model)
            {
                model.Name = objectCount > 1 ? $"{baseName}_{i}" : baseName;
                model.Category = category;
                model.SetGameObjectId(startId + i);
                model.LoadModelFromPath(prmPath, i);
                GetAll.Add(model);
            }
        }

        return startId + objectCount;
    }
}
using System;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Assets;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using WipeoutRewrite.Factory;

namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// GameObjectCollection: Manages a collection of game objects (ships, props, etc.).
    /// Handles initialization, rendering, and updates for all objects in the collection.
    /// </summary>
    public class GameObjectCollection : IGameObjectCollection
    {
        private readonly ILogger<GameObjectCollection> _logger;

        private readonly IGameObjectFactory _gameObjectFactory;

        public List<GameObject> GetAll { get; private set; } = new();

        private const int NumberOfShips = 8;

        public GameObjectCollection(ILogger<GameObjectCollection> logger,
                     IGameObjectFactory gameObjectFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameObjectFactory = gameObjectFactory ?? throw new ArgumentNullException(nameof(gameObjectFactory)); 
        }

        
        public void Init(TrackSection? startSection)
        { 
            //Call this when Race is started
            for (int i = 0; i < NumberOfShips; i++)
            {
                var model = _gameObjectFactory.CreateModel() as GameObject;
                if (model != null)
                {
                    model.Load(i);
                    GetAll.Add(model);
                }
            }
            
        }

        public void Renderer()
        {
            // Stub: original draws ships. Preview layer should call Draw() on instances.
             foreach (var s in GetAll)
                s.Draw();
        }

        public void Update()
        {
            foreach (var s in GetAll)
                s.Update();
        }

        public void ResetExhaustPlumes()
        {
            foreach (var s in GetAll)
                s.ResetExhaustPlume();
        }

        public void Clear()
        {
            GetAll.Clear();
        }
    }
}

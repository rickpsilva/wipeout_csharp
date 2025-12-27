using System.Collections.Generic;

namespace WipeoutRewrite.Core.Entities
{
    public interface IGameObjectCollection
    {
        List<GameObject> GetAll { get; }
        void Init(TrackSection? startSection);
        void Renderer();
        void Update();
        void ResetExhaustPlumes();
        void Clear();
        
        // Category-based access
        List<GameObject> GetByCategory(GameObjectCategory category);
        GameObject? GetByName(string name);
        List<GameObjectCategory> GetCategories();
    }
}
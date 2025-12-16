using System.Collections.Generic;

namespace WipeoutRewrite.Core.Entities
{
    public interface IShips
    {
        List<ShipV2> AllShips { get; }
        void ShipsInit(TrackSection? startSection);
        void ShipsRenderer();
        void ShipsUpdate();
        void ShipsResetExhaustPlumes();
        void Clear();
    }
}
using System.Collections.Generic;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Factory
{
    public interface IShipFactory
    {
        IShipV2 CreateShip();
    }
}
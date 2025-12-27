using System.Collections.Generic;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Factory
{
    public interface IGameObjectFactory
    {
        IGameObject CreateModel();
    }
}
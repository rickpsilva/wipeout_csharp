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
    /// ShipV2: versão simplificada de entidade de nave que apenas lida
    /// com o carregamento do PRM (modelo) e exposição do `Mesh` carregado.
    /// Útil para ferramentas de preview e testes rápidos.
    /// </summary>
    public class Ships : IShips
    {
        private readonly ILogger<Ships> _logger;

        private readonly IShipFactory _shipFactory;

        public List<ShipV2> AllShips { get; private set; } = new();

        private const int NumberOfShips = 8;

        public Ships(ILogger<Ships> logger,
                     IShipFactory shipFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _shipFactory = shipFactory ?? throw new ArgumentNullException(nameof(shipFactory)); 
        }

        
        public void ShipsInit(TrackSection? startSection)
        { 
            //Call this when Race is started
            for (int i = 0; i < NumberOfShips; i++)
            {
                var ship = _shipFactory.CreateShip() as ShipV2;
                if (ship != null)
                {
                    ship.ShipLoad(i);
                    AllShips.Add(ship);
                }
            }
            
        }

        public void ShipsRenderer()
        {
            // Stub: original draws ships. Preview layer should call Draw() on instances.
             foreach (var s in AllShips)
                s.Draw();
        }

        public void ShipsUpdate()
        {
            foreach (var s in AllShips)
                s.Update();
        }

        public void ShipsResetExhaustPlumes()
        {
            foreach (var s in AllShips)
                s.ResetExhaustPlume();
        }

        public void Clear()
        {
            AllShips.Clear();
        }
    }
}

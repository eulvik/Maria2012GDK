using System.Linq;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.Maria.GridContracts;

namespace TestMapApp
{
    public class GridLayerViewModel
    {
        public IMariaGridLayer GridLayer { get; private set; }

        public GridLayerViewModel(IMariaGridLayer gridLayer)
        {
            GridLayer = gridLayer;
            GridLayer.LayerInitialized += OnLayerInitialized;
        }

        private void OnLayerInitialized()
        {
        }
    }
}
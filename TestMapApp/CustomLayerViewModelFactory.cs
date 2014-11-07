using TPG.Maria.CustomContracts;

namespace TestMapApp
{
    public class CustomLayerViewModelFactory : IMariaCustomLayerFactory
    {
        private MapViewModel _trackLayer;

        public CustomLayerViewModelFactory(MapViewModel _trackLayer)
        {
            // TODO: Complete member initialization
            this._trackLayer = _trackLayer;
        }
        public TPG.GeoFramework.Contracts.Geo.Layer.IGeoLayerViewModel New(
            TPG.GeoFramework.Contracts.Geo.Context.IGeoContext geoContext,
            TPG.GeoFramework.Contracts.Geo.Context.IGeoNavigator geoNavigator,
            TPG.GeoFramework.Contracts.Geo.Control.IGeoControlViewModel geoControlViewModel,
            TPG.GeoUnits.IGeoUnitsSetting geoUnitsSetting)
        {
            var vm =  new CustomViewModel
            {
                GeoContext = geoContext,
                GeoNavigator = geoNavigator
            };
            vm._trackLayer = _trackLayer;
            return vm;
        }
    }
}
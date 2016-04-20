using TPG.Maria.CustomContracts;

namespace TestMapApp2_0
{
    public class CustomLayerViewModelFactory : IMariaCustomLayerFactory
    {
        public TPG.GeoFramework.Contracts.Geo.Layer.IGeoLayerViewModel New(
            TPG.GeoFramework.Contracts.Geo.Context.IGeoContext geoContext,
            TPG.GeoFramework.Contracts.Geo.Context.IGeoNavigator geoNavigator,
            TPG.GeoFramework.Contracts.Geo.Control.IGeoControlViewModel geoControlViewModel,
            TPG.GeoUnits.IGeoUnitsSetting geoUnitsSetting)
        {
            return new CustomViewModel
            {
                GeoContext = geoContext,
                GeoNavigator = geoNavigator
            };
        }
    }
}
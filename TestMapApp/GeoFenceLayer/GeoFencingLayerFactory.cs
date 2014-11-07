using MariaGeoFencing.GeoFenceLayer;
using TPG.GeoFramework.Contracts.Geo.Context;
using TPG.GeoFramework.Contracts.Geo.Control;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.GeoUnits;
using TPG.Maria.CustomContracts;

namespace MariaCustomLayerClient.CustomLayer
{
    public class GeoFencingLayerFactory : IMariaCustomLayerFactory
    {
        #region Implementation of IMariaCustomLayerFactory

        public IGeoLayerViewModel New(IGeoContext geoContext, 
                                      IGeoNavigator geoNavigator,
                                      IGeoControlViewModel geoControlViewModel, 
                                      IGeoUnitsSetting geoUnitsSetting = null)
        {
            return new GeoFencingLayer
                       {
                           GeoContext = geoContext,
                           GeoNavigator = geoNavigator
                       };
        }

        #endregion
    }
}

using TPG.GeoFramework.Contracts.Geo.Context;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.GeoFramework.Core;

namespace MariaGeoFencing.GeoFenceLayer
{
    public class GeoFencingViewFactory : IGeoLayerViewFactory
    {
        private GeoFencingView _instance;

        #region Implementation of IGeoLayerViewFactory
        public IGeoLayerView New()
        {
            return _instance ?? (_instance = new GeoFencingView());
        }

        #endregion
    }
}

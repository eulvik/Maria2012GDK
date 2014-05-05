using TPG.GeoFramework.Contracts.Geo.Layer;

namespace TestMapApp
{
    public class CustomViewFactory : IGeoLayerViewFactory
    {
        private CustomView _instance;

        public IGeoLayerView New()
        {
            return _instance ?? (_instance = new CustomView());
        }
    }
}
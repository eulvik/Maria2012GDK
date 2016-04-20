using TPG.GeoFramework.Contracts.Geo.Layer;

namespace TestMapApp2_0
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
using TPG.GeoFramework.Contracts.EventManager;
using TPG.GeoFramework.Core;

namespace TestMapApp
{
    public class CustomViewModel : GeoLayerViewModel
    {
        private readonly CustomView _view;
        
        public CustomViewModel()
        {
            GeoLayerViewFactory = new CustomViewFactory();
            _view = (CustomView)GeoLayerViewFactory.New();
        }

        public override void HandleInputEvent(GeoInputEventArgs inputEventArgs)
        {
            //Do input handling here
        }

        public override void Update()
        {
            _view.Render(GeoContext);
        }
    }
}
using TPG.GeoFramework.Contracts.EventManager;
using TPG.GeoFramework.Core;
using TPG.GeoUnits;
using TPG.Maria.TrackContracts;
using TPG.Maria.TrackLayer;

namespace TestMapApp
{
    public class CustomViewModel : GeoLayerViewModel
    {
        private readonly CustomView _view;
        public MapViewModel _trackLayer;

        public CustomViewModel()
        {
            GeoLayerViewFactory = new CustomViewFactory();
            _view = (CustomView)GeoLayerViewFactory.New();
        }

        public override void HandleInputEvent(GeoInputEventArgs inputEventArgs)
        {
            //Do input handling here
        }

        GeoPos? lastPos;
        public override void Update()
        {
            _view.Render(GeoContext, _trackLayer.Destination);
        }
    }
}
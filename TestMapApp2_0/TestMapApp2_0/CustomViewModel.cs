using System;
using System.Windows.Threading;
using TPG.GeoFramework.Contracts.EventManager;
using TPG.GeoFramework.Core;

namespace TestMapApp2_0
{
    public class CustomViewModel : GeoLayerViewModel
    {
        private readonly CustomView _view;
        
        public CustomViewModel()
        {
            GeoLayerViewFactory = new CustomViewFactory();
            _view = (CustomView)GeoLayerViewFactory.New();
            new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (sender, args) =>
            {
                _view.Generate();
                SetDirty(true);
            }, Dispatcher.CurrentDispatcher);
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
using System.Windows.Controls;
using System.Windows.Media;
using TPG.GeoFramework.Contracts.Geo.Context;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.GeoFramework.Core;

namespace TestMapApp
{
    public class CustomView : Grid, IGeoLayerView
    {
        private readonly VisualHost _visualHost;
        private readonly DrawingVisual _drawingVisual;

        public CustomView()
        {
            _visualHost = new VisualHost { IsHitTestVisible = false };
            _drawingVisual = new DrawingVisual();
            _visualHost.Children.Add(_drawingVisual);
            Children.Add(_visualHost);
        }

        public IGeoContext GeoContext { get; set; }

        public void Generate()
        {
            DrawingContext dc = _drawingVisual.RenderOpen();
            Render(dc);
            dc.Close();
        }

        private void Render(DrawingContext dc)
        {
            //Do you rendering stuff here.
        }
    }
}
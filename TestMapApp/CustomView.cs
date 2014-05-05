using System.Globalization;
using System.Windows;
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

        public void Generate()
        {

        }

        public void Render(IGeoContext geoContext)
        {
            DrawingContext dc = _drawingVisual.RenderOpen();
            dc.DrawLine(new Pen(new SolidColorBrush(Colors.HotPink), 5), new Point(0, 0), new Point(geoContext.Viewport.Width, geoContext.Viewport.Height));
            dc.DrawText(new FormattedText("FFI Layer", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 22, new SolidColorBrush(Colors.HotPink)), 
                geoContext.Viewport.CenterPoint);
            dc.Close();
        }
    }
}
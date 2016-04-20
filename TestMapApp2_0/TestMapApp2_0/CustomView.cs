using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TPG.GeoFramework.Contracts.Geo.Context;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.GeoFramework.Core;

namespace TestMapApp2_0
{
    public class CustomView : Grid, IGeoLayerView
    {
        private readonly VisualHost _visualHost;
        private readonly DrawingVisual _drawingVisual;
        private double _angle;

        public CustomView()
        {
            _visualHost = new VisualHost { IsHitTestVisible = false };
            _drawingVisual = new DrawingVisual();
            _visualHost.Children.Add(_drawingVisual);
            Children.Add(_visualHost);
        }

        public void Generate()
        {
            _angle += 0.5;
            if (_angle > 360)
                _angle = 0;
        }

        public void Render(IGeoContext geoContext)
        {
            DrawingContext dc = _drawingVisual.RenderOpen();
            dc.DrawLine(new Pen(new SolidColorBrush(Colors.HotPink), 5), new Point(0, 0), new Point(geoContext.Viewport.Width, geoContext.Viewport.Height));
            dc.PushTransform(new RotateTransform(_angle, geoContext.Viewport.CenterPoint.X, geoContext.Viewport.CenterPoint.Y));
            dc.DrawText(new FormattedText("ClientDrawGeoLayer", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 22, new SolidColorBrush(Colors.HotPink)), 
                geoContext.Viewport.CenterPoint);
            dc.Pop();
            dc.Close();
        }
    }
}
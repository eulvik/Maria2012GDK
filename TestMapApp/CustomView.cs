using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private int frame = 0;
        public void Render(IGeoContext geoContext)
        {
            frame++;
            DrawingContext dc = _drawingVisual.RenderOpen();
            //dc.DrawLine(new Pen(new SolidColorBrush(Colors.HotPink), 5), new Point(0, 0), new Point(geoContext.Viewport.Width, geoContext.Viewport.Height));
            //dc.DrawText(new FormattedText("El33t", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 22, new SolidColorBrush(Colors.HotPink)), 
            //    geoContext.Viewport.CenterPoint);
            //dc.Close();

            int w = 480 / 5;
            int h = 384 / 4;

            frame = frame % 20;
            int x = frame % 5;
            int y = frame / 5;
            FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var imageUri = new Uri(Path.Combine("file:\\", fi.DirectoryName, "Explosion2.png"));
            BitmapSource bSource = new BitmapImage(imageUri);

            UInt32[] pixels = new UInt32[w*h];

            bSource.CopyPixels(new Int32Rect(x * w, y * h, w, h), pixels, w*4, 0);
            var pic = BitmapSource.Create(w,h,96,96,bSource.Format, null, pixels, w*4);
            dc.DrawImage(pic, new Rect(300, 300, 200, 200));
            dc.Close();
        }

        bool render = true;
        internal void Render(IGeoContext GeoContext, TPG.GeoUnits.GeoPos? pos)
        {
            render = !render;
            if (!render)
            {
                return;
            }
            frame++;
            DrawingContext dc = _drawingVisual.RenderOpen();
            int w = 480 / 5;
            int h = 384 / 4;

            frame = frame % 20;
            int x = frame % 5;
            int y = frame / 5;
            FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var imageUri = new Uri(Path.Combine("file:\\", fi.DirectoryName, "Explosion2.png"));
            BitmapSource bSource = new BitmapImage(imageUri);

            UInt32[] pixels = new UInt32[w * h];

            bSource.CopyPixels(new Int32Rect(x * w, y * h, w, h), pixels, w * 4, 0);
            var pic = BitmapSource.Create(w, h, 96, 96, bSource.Format, null, pixels, w * 4);
            Point posXy;
            GeoContext.Viewport.LatLonToXY(pos.Value, out posXy);
            dc.DrawImage(pic, new Rect(posXy.X-64, posXy.Y - 64, 128, 128));
            dc.Close();
        }
    }
}
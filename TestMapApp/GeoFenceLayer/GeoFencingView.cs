using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using MariaGeoFencing.Utilities;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.GeoFramework.Core;
using TPG.GeoFramework.GeoFencingServiceInterfaces;

namespace MariaGeoFencing.GeoFenceLayer
{
    public class GeoFencingView : Grid, IGeoLayerView
    {
        private readonly DrawingVisual _drawingVisual;
        private readonly VisualHost _visualHost;

        

        public GeoFencingView()
        {
            _visualHost = new VisualHost { IsHitTestVisible = false };
            _drawingVisual = new DrawingVisual();

            _visualHost.Children.Add(_drawingVisual);

            Children.Add(_visualHost);

            VisalisationList = new List<VisualData>();
        }

        public List<VisualData> VisalisationList { get; private set; }

        #region Implementation of IGeoLayerView
        public void Generate()
        {
            var dc = _drawingVisual.RenderOpen();

            Render(dc);

            dc.Close();
        }
        #endregion



        private void Render(DrawingContext dc)
        {
            // Custom layer draw code goes here, if any...
            if (VisalisationList != null)
            {
                foreach (var element in VisalisationList)
                {
                    if (element.Type == VisualInfoType.AnnotationLine || 
                        element.Type == VisualInfoType.EventPossitions)
                    {
                        dc.DrawLine(element.LinePen,
                            GeoFencePossitionHelper.LatLonPosToPoint(element.PtStart),
                            GeoFencePossitionHelper.LatLonPosToPoint(element.PtEnd));
                    }
                    else if (element.Type == VisualInfoType.AnnotationEllipsis)
                    {
                        dc.DrawEllipse(element.FillBrush, element.LinePen,
                            GeoFencePossitionHelper.LatLonPosToPoint(element.PtStart), 75, 50);
                    }
                }
            }
        }
    }
}

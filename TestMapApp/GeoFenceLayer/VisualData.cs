using System.Windows.Media;
using TPG.GeoFramework.GeoFencingServiceInterfaces;

namespace MariaGeoFencing.GeoFenceLayer
{
    public enum VisualInfoType
    {
        EventPossitions,
        AnnotationEllipsis,
        AnnotationLine
    }

    public class VisualData
    {
        public Pen LinePen { get; set; }
        public Brush FillBrush { get; set; }

        public VisualInfoType Type { get; set; }
        public LatLonPos PtStart { get; set; }
        public LatLonPos PtEnd { get; set; }

        public NotificationLevel Level  { get; set; }
    }
}

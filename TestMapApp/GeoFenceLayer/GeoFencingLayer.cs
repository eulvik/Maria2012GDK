using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MariaGeoFencing.Utilities;
using TPG.GeoFramework.Contracts.EventManager;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.GeoFramework.Core;
using TPG.GeoFramework.GeoFencingServiceInterfaces;

namespace MariaGeoFencing.GeoFenceLayer
{
    public class GeoFencingLayer : GeoLayerViewModel
    {
        #region Locals
        private readonly GeoFencingView _view;
        #endregion Locals

        #region Properties
        public List<VisualData> VisalisationList{get { return _view.VisalisationList; }}
        #endregion Properties

        public GeoFencingLayer()
        {
            GeoLayerViewFactory = new GeoFencingViewFactory();
            _view = GeoLayerViewFactory.New() as GeoFencingView;

            ClipMargins = new ClipMargins 
                              {
                                  ClipLeftMargin = 0,
                                  ClipTopMargin = 0,
                                  ClipRightMargin = 0,
                                  ClipBottomMargin = 0
                              };

            VisibleChanged += OnVisibleChanged;
        }

        private void OnVisibleChanged()
        {
            System.Diagnostics.Trace.WriteLine("GeoFencingLayer => OnVisibleChanged");

            _view.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
            _view.Generate();
        }
      
        public void Clear()
        {
            VisalisationList.Clear();
        }
        
        public void UpdateVisualisation(IEnumerable<CustomNotificationDef> list, bool assosiateWithTrack)
        {
            if (list == null)
                return;
            
            foreach (var element in list)
            {
                VisalisationList.Add(
                    new VisualData
                    {
                        Type = VisualInfoType.EventPossitions,
                        Level = element.Def.Level,
                        PtStart = element.Def.FirstTrackPos,
                        PtEnd = element.Def.LastTrackPos,
                        LinePen = PenFromDefinition(element.Def, VisualInfoType.EventPossitions)
                    });

                if (element.CurrentTrackPos != null && assosiateWithTrack)
                {
                    VisalisationList.AddRange(new[]
                    {
                        new VisualData
                        {
                            Type = VisualInfoType.AnnotationLine,
                            Level = element.Def.Level,
                            PtStart = element.Def.FirstTrackPos,
                            PtEnd = element.CurrentTrackPos,
                            LinePen = PenFromDefinition(element.Def, VisualInfoType.AnnotationLine)
                        },
                        new VisualData
                        {
                            Type = VisualInfoType.AnnotationLine,
                            Level = element.Def.Level,
                            PtStart = element.Def.LastTrackPos,
                            PtEnd = element.CurrentTrackPos,
                            LinePen = PenFromDefinition(element.Def, VisualInfoType.AnnotationLine)
                        }
                    });
                }
            }
        }

        private Pen PenFromDefinition(NotificationDef def, VisualInfoType type)
        {
            var color = Colors.DimGray;
            var width = 2;
            var dash = DashStyles.Solid;

            if (def.GetUserDataTagValue(GeoFenceDefs.OwnerName, GeoFenceDefs.StateKey) != GeoFenceDefs.AcknowledgedValue)
            {
                switch (def.Level)
                {
                    case NotificationLevel.Low:
                        color = Colors.LimeGreen;
                        break;
                    case NotificationLevel.Medium:
                        color = Colors.Yellow;
                        break;
                    case NotificationLevel.High:
                        color = Colors.Red;
                        break;
                }
            }
            if (type == VisualInfoType.EventPossitions)
                width = 4;
            else
                dash = DashStyles.Dash;

            return new Pen(new SolidColorBrush(color), width) { DashStyle = dash };
        }

        #region Overrides of GeoLayerViewModel

        /// <summary>
        /// Called to forward events to the layer.
        /// </summary>
        /// <param name="inputEventArgs"/>
        /// <remarks>
        /// If the layer handles the event, it must set the <c>Handled</c> property to true.
        /// </remarks>
        public override void HandleInputEvent(GeoInputEventArgs inputEventArgs)
        {
            _view.Generate();                    
        }

        /// <summary>
        /// Event called at regular intervals to update contents of the layer.
        /// </summary>
        public override void Update()
        {
            // Periodiscally update: refresh view when required....
            _view.Generate();
        }

        #endregion

 
    }
}

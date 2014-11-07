using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using MariaGeoFencing.Utilities;
using TPG.DrawObjects.Contracts.GenericObjects;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI.Primitives;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI.Types;
using TPG.GeoFramework.Core;
using TPG.GeoUnits;
using TPG.Maria.Common;
using TPG.Maria.Contracts;
using TPG.Maria.DrawObjectContracts;

namespace MariaGeoFencing.ViewModels
{
    public class DrawObjectViewModel : ViewModelBase
    {
        private readonly MariaWindowViewModel _parent;
        private readonly IMariaDrawObjectLayer _drawObjectLayer;
        private ICreationWorkflow _activeCreationWorkflow;
        private ulong _objCnt = 0;

        public DrawObjectViewModel(IMariaDrawObjectLayer drawObjectLayer, MariaWindowViewModel parent)
        {
            SingleStore = false;
            _parent = parent;

            _drawObjectLayer = drawObjectLayer;
            _drawObjectLayer.LayerInitialized += DrawObjectLayerOnLayerInitialized;
            _drawObjectLayer.ServiceConnected += OnDrawObjectLayerServiceConnected;

            _drawObjectLayer.ExtendedDrawObjectLayer.ActiveCreationWorkflowCompleted += OnExtendedDrawObjectLayerActiveCreationWorkflowCompleted;
        }
        private void DrawObjectLayerOnLayerInitialized()
        {
            _drawObjectLayer.DefaultStyleXml = "<styleset><stylecategory name=\"DrawObjects\"/></styleset>";

            DrawObjectServices = new ObservableCollection<IMariaService>
            {
                new MariaService(GeoFenceDefs.DefaultGeoshapeServiceName)
            };

            _parent.DisplayFilters.Add(_drawObjectLayer.DisplayFilter);

            if (_drawObjectLayer != null && _drawObjectLayer.GenericCreationWorkflows != null && _drawObjectLayer.GenericCreationWorkflows.Count > 0)
            {
                foreach (var gwf in _drawObjectLayer.GenericCreationWorkflows.Reverse())
                    _drawObjectLayer.CreationWorkflows.Insert(0, gwf);
            }
        }

        private void OnDrawObjectLayerServiceConnected(object sender, MariaServiceEventArgs args)
        {
            if (DrawObjectServices.Count > 0)
            {
                var activeName = _parent.StoreId;
                ActiveDrawObjectService = DrawObjectServices[0];

                if (SingleStore)
                {
                    DrawObjectServiceStores = new ObservableCollection<string> { activeName };
                }
                else
                {
                    DrawObjectServiceStores =
                        new ObservableCollection<string>(_drawObjectLayer.GetDrawObjectServiceStores());


                    if (!DrawObjectServiceStores.Contains(activeName))
                    {
                        DrawObjectServiceStores.Add(activeName);
                    }
                }

                ActiveDrawObjectServiceStore = activeName;
            }
        }

        #region Connection properties
        public bool SingleStore { get; set; }

        public ObservableCollection<string> DrawObjectServiceStores
        {
            get { return _drawObjectLayer.DrawObjectServiceStores; }
            set
            {
                _drawObjectLayer.DrawObjectServiceStores = value;
                NotifyPropertyChanged(() => DrawObjectServiceStores);
            }
        }
        public ObservableCollection<IMariaService> DrawObjectServices
        {
            get { return _drawObjectLayer.DrawObjectServices; }
            set
            {
                _drawObjectLayer.DrawObjectServices = value;
                NotifyPropertyChanged(() => DrawObjectServices);
            }
        }
        public string ActiveDrawObjectServiceStore
        {
            get { return _drawObjectLayer.ActiveDrawObjectServiceStore; }
            set
            {
                _drawObjectLayer.ActiveDrawObjectServiceStore = value;
                NotifyPropertyChanged(() => ActiveDrawObjectServiceStore);
            }
        }
        public IMariaService ActiveDrawObjectService
        {
            get { return _drawObjectLayer.ActiveDrawObjectService; }
            set
            {
                _drawObjectLayer.ActiveDrawObjectService = value;
                NotifyPropertyChanged(() => ActiveDrawObjectService);
            }
        }
        #endregion Connection properties
        public ICreationWorkflow ActiveCreationWorkflow
        {
            get { return _activeCreationWorkflow; }
            set
            {
                if (_activeCreationWorkflow != null)
                    _activeCreationWorkflow.IsActive = false;

                _activeCreationWorkflow = value;

                if (_activeCreationWorkflow != null)
                {
                    _activeCreationWorkflow.IsActive = true;
                    _parent.Cursor = Cursors.Cross;
                }

                NotifyPropertyChanged(() => ActiveCreationWorkflow);
            }
        }

        public ICommand AddObjects {get { return new DelegateCommand(x => AddMiscObjects()); }}
        public ICommand EditSelected { get { return _drawObjectLayer.EditPointsCommand; } }
        public ICommand RemoveObject { get { return _drawObjectLayer.DeleteDrawObjectCommand; } }
       

        #region DrawObjectStoreDisplay

        public ObservableCollection<IMariaLayer> DrawObjectStoreDisplay
        {
            get { return new ObservableCollection<IMariaLayer>(); }
        }
        #endregion DrawObjectStoreDisplay
 
        private void OnExtendedDrawObjectLayerActiveCreationWorkflowCompleted(object sender, ActiveWorkflowCompletedEventArgs args)
        {
            ActiveCreationWorkflow = null;
            _parent.Cursor = null;

            if (_drawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds.Count() != 1)
                return;

            var simpleObject = _drawObjectLayer.GetDrawObjectFromStore(_drawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds.First());
            if (simpleObject.GenericDataFields == null)
                return;

            simpleObject.GenericDataFields.FontForegroundColor = Colors.Black;
            simpleObject.GenericDataFields.FontName = "Arial";
            simpleObject.GenericDataFields.FontSize = 10;
            simpleObject.GenericDataFields.FillForegroundColor = Colors.Transparent;

            _drawObjectLayer.UpdateStore(simpleObject);
        }

        #region Object helpers
        
        private void AddMiscObjects()
        {
            switch (_objCnt++%6)
            {
                case 0:
                    AddPolyLine();
                    break;
                case 1:
                    AddRecttangle();
                    break;
                case 2:
                    AddRangeCircle();
                    break;
                case 3:
                    AddEllipse();
                    break;
                case 4:
                    AddFanArea();
                    break;
                case 5:
                    AddText();
                    break;
            }
        }
        
        public void DoCleanObjects()
        {
            _drawObjectLayer.ExtendedDrawObjectLayer.DeleteAll();
        }

        private void AddText()
        {
            var text = _drawObjectLayer.DrawObjectFactory.CreateText("Text Object");

            text.Points = new[] { RandomProvider.GetRandomGeoPoint(_drawObjectLayer.GeoContext.Viewport.GeoRect)};

            text.DataFields.AlphaFactor = RandomProvider.GetRandomDouble(0.5, 1.0);
            text.DataFields.Name = "TextObject-" + DateTime.UtcNow.ToString(GeoFenceDefs.DateTimeFormat);
            text.DataFields.DrawDepth = 4;
            text.DataFields.RotationAngle = 0;
            text.DataFields.ExtraFields.Add("MyTag", "MyTagValue");

            text.GenericDataFields.LineColor = Colors.Red;
            text.GenericDataFields.FillBackgroundColor = Colors.BlueViolet;
            text.GenericDataFields.FillForegroundColor = Colors.Yellow;
            text.GenericDataFields.FontForegroundColor = Colors.Black;
            text.GenericDataFields.FontBackgroundColor = Colors.Orange;

            text.GenericDataFields.LineWidth = 4;
            text.GenericDataFields.LineDashStyle = new List<double>(); // DashStyles.Solid

            _drawObjectLayer.UpdateStore(text);
        }
        private void AddPolyLine()
        {
            var rect = _drawObjectLayer.GeoContext.Viewport.GeoRect;
            ISimpleDrawObject obj = _drawObjectLayer.DrawObjectFactory.CreatePolyLine();
            obj.Points = new[]
                             {
                                 RandomProvider.GetRandomGeoPoint(rect),
                                 RandomProvider.GetRandomGeoPoint(rect),
                                 RandomProvider.GetRandomGeoPoint(rect)
                             };

            obj.DataFields.Name = "PolyLineObject-" + DateTime.UtcNow.ToString(GeoFenceDefs.DateTimeFormat);
            obj.DataFields.DrawDepth = 4;
            obj.DataFields.RotationAngle = 0;
            obj.DataFields.ExtraFields.Add("MyTag", "MyTagValue");

            obj.GenericDataFields.LineColor = Colors.Brown;
            obj.GenericDataFields.LineWidth = 4;
            obj.GenericDataFields.LineDashStyle = null; // DashStyles.Solid
            _drawObjectLayer.UpdateStore(obj);
        }
        private void AddEllipse()
        {
            var rect = _drawObjectLayer.GeoContext.Viewport.GeoRect;
            rect.Center = RandomProvider.GetRandomPosition(rect);
            rect.InflateRelative(0.1);

            var obj = _drawObjectLayer.DrawObjectFactory.CreateEllipse();

            if (obj.GetType().IsSubclassOf(typeof(IEllipse)))
                return;

            var ellipse = (IEllipse)obj;
            ellipse.CentrePoint = RandomProvider.GetRandomGeoPoint(rect);
            ellipse.FirstConjugateDiameterPoint = RandomProvider.GetRandomGeoPoint(rect);
            ellipse.SecondConjugateDiameterPoint = RandomProvider.GetRandomGeoPoint(rect);

            obj.DataFields.Name = "EllipsisObject-" + DateTime.UtcNow.ToString(GeoFenceDefs.DateTimeFormat);
            obj.DataFields.DrawDepth = 4;
            obj.DataFields.RotationAngle = 0;
            obj.DataFields.ExtraFields.Add("MyTag", "MyTagValue");

            obj.GenericDataFields.LineColor = Colors.DarkOrange;
            obj.GenericDataFields.LineWidth = 4;
            obj.GenericDataFields.LineDashStyle = new List<double>();
            obj.GenericDataFields.Fill = true;
            obj.GenericDataFields.FillStyle = FillStyle.Cross;
            obj.GenericDataFields.FillBackgroundColor = Color.FromArgb(150, 255, 215, 0); //Colors.Gold;
            obj.GenericDataFields.FillForegroundColor = Colors.Yellow;

            _drawObjectLayer.UpdateStore(obj);
        }
        private void AddFanArea()
        {
            var rect = _drawObjectLayer.GeoContext.Viewport.GeoRect;
            rect.InflateRelative(0.9);
            rect.Center = RandomProvider.GetRandomPosition(rect);
            rect.InflateRelative(0.3);

            var obj = _drawObjectLayer.DrawObjectFactory.CreateFanArea();

            if (obj.GetType().IsSubclassOf(typeof(IFanArea)))
                return;

            var ptCenter = RandomProvider.GetRandomPosition(rect);
            var ptOuter =  RandomProvider.GetRandomPosition(rect);
            var br = Earth.BearingRange(ptCenter, ptOuter);

            var fan = (IFanArea)obj;
            fan.VertexPoint = new GeoPoint(){Latitude = ptCenter.Lat, Longitude = ptCenter.Lon};

            fan.MaximumRange = br.Range; 
            fan.MinimumRange = br.Range/(RandomProvider.GetRandomInt(2,8));
            fan.OrientationAngle = (float) br.Bearing;
            fan.SectorSizeAngle = (float)RandomProvider.GetRandomDouble(45.0, 135);

            obj.DataFields.Name = "Fan Object-" + DateTime.UtcNow.ToString(GeoFenceDefs.DateTimeFormat);
            obj.DataFields.DrawDepth = 4;
            obj.DataFields.RotationAngle = 0;
            obj.DataFields.ExtraFields.Add("MyTag", "MyTagValue");

            obj.GenericDataFields.LineColor = Colors.Green;
            obj.GenericDataFields.LineWidth = 4;
            obj.GenericDataFields.LineDashStyle = new List<double>();
            obj.GenericDataFields.Fill = true;
            obj.GenericDataFields.FillStyle = FillStyle.Solid;
            obj.GenericDataFields.FillForegroundColor = Colors.LawnGreen;

            _drawObjectLayer.UpdateStore(obj);
        }
        private void AddRecttangle()
        {
            var rect = _drawObjectLayer.GeoContext.Viewport.GeoRect;
            var obj = _drawObjectLayer.DrawObjectFactory.CreatePolyArea();

            var pt1 = RandomProvider.GetRandomGeoPoint(rect);
            var pt2 = RandomProvider.GetRandomGeoPoint(rect);
            obj.Points = new[]
            {
                pt1, new GeoPoint {Latitude = pt2.Latitude, Longitude = pt1.Longitude},
                pt2, new GeoPoint {Latitude = pt1.Latitude, Longitude = pt2.Longitude}
            };

            obj.DataFields.Name = "RecttangleObject-" + DateTime.UtcNow.ToString(GeoFenceDefs.DateTimeFormat);
            obj.DataFields.DrawDepth = 4;
            obj.DataFields.RotationAngle = 0;
            obj.DataFields.ExtraFields.Add("MyTag", "MyTagValue");

            obj.GenericDataFields.LineColor = Colors.DeepPink;
            obj.GenericDataFields.LineWidth = 4;
            obj.GenericDataFields.LineDashStyle = new List<double>();

            _drawObjectLayer.UpdateStore(obj);
        }
        private void AddTriangle()
        {
            var rect = _drawObjectLayer.GeoContext.Viewport.GeoRect;

            var obj = _drawObjectLayer.DrawObjectFactory.CreatePolyArea();
            obj.Points = new[]
                                  {
                                      RandomProvider.GetRandomGeoPoint(rect),
                                      RandomProvider.GetRandomGeoPoint(rect),
                                      RandomProvider.GetRandomGeoPoint(rect)
                                  };

            obj.DataFields.Name = "TriangleObject-" + DateTime.UtcNow.ToString(GeoFenceDefs.DateTimeFormat);
            obj.DataFields.DrawDepth = 4;
            obj.DataFields.RotationAngle = 0;
            obj.DataFields.ExtraFields.Add("MyTag", "MyTagValue");

            obj.GenericDataFields.LineColor = Colors.Red;
            obj.GenericDataFields.LineWidth = 4;
            obj.GenericDataFields.LineDashStyle = new List<double>(); // DashStyles.Solid

            _drawObjectLayer.UpdateStore(obj);
        }

        private void AddRangeCircle()
        {
            var rect = _drawObjectLayer.GeoContext.Viewport.GeoRect;
            rect.InflateRelative(0.8);
            rect.Center = RandomProvider.GetRandomPosition(rect);
            rect.InflateRelative(0.25);

            var obj = _drawObjectLayer.DrawObjectFactory.CreateRangeRings();

            obj.DataFields.Name = "RangeCircle-" + DateTime.UtcNow.ToString(GeoFenceDefs.DateTimeFormat); 
            var ptCenter = RandomProvider.GetRandomPosition(rect);
            var ptOuter =  RandomProvider.GetRandomPosition(rect);
            var br = Earth.BearingRange(ptCenter, ptOuter);

            var rangeRings = (IRangeRings)obj;
            rangeRings.VertexPoint = new GeoPoint(){Latitude = ptCenter.Lat, Longitude = ptCenter.Lon};

            var cnt = RandomProvider.GetRandomInt(3,8);
            rangeRings.MaximumRange = br.Range;
            rangeRings.NumberOfRadials = cnt;
            rangeRings.RangeBetweenRings = (int)(br.Range / cnt);

            obj.GenericDataFields.LineColor = Colors.Black;
            obj.GenericDataFields.LineWidth = 2;
            obj.GenericDataFields.LineDashStyle = new List<double>(); // DashStyles.Solid

            _drawObjectLayer.UpdateStore(obj);

        }
        #endregion Object helpers
    }
}

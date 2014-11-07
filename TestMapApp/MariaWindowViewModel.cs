using MariaGeoFencing.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using TestMapApp.Annotations;
using TPG.GeoFramework.Contracts.Geo.Tool;
using TPG.Maria.Contracts;
using TPG.Maria.CustomLayer;
using TPG.Maria.DrawObjectLayer;
using TPG.Maria.MapContracts;
using TPG.Maria.MapLayer;
using TPG.Maria.TrackContracts;
using TPG.Maria.TrackLayer;

namespace TestMapApp
{
    public class MariaWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IMariaLayer> Layers { get; set; }  

        public MapViewModel MapViewModel { get; set; }
        private readonly IMariaMapLayer _mapLayer;

        public TrackViewModel TrackViewModel { get; set; }
        private readonly IMariaTrackLayer _trackLayer;

        public DrawObjectViewModel DrawObjectViewModel { get; set; }

        private readonly CustomLayer<CustomViewModel> _customLayer;
        
        private DispatcherTimer _timer;
        public MariaWindowViewModel()
        {
            Layers = new ObservableCollection<IMariaLayer>();

            _mapLayer = new MapLayer();
            MapViewModel = new MapViewModel(_mapLayer);
            Layers.Add(_mapLayer);

            _trackLayer = new TrackLayer();
            TrackViewModel = new TrackViewModel(_trackLayer);
            Layers.Add(_trackLayer);

            var drawObjectLayer = new DrawObjectLayer(true);
            DrawObjectViewModel = new DrawObjectViewModel(drawObjectLayer);
            Layers.Add(drawObjectLayer);

            _customLayer = new CustomLayer<CustomViewModel>(new CustomLayerViewModelFactory(MapViewModel));
            Layers.Add(_customLayer);

            _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500)};
            _timer.Tick += IsMapWorking;
            _timer.Start();
        }

        private void IsMapWorking(object sender, EventArgs e)
        {
            ShowProgress = (MapViewModel.MapLayer.TileCacheManager.TileDataReader.WorkQueueSize > 0);
        }


        #region Tools

        private ObservableCollection<IGeoTool> _tools;
        

        public ObservableCollection<IGeoTool> Tools
        {
            private get { return _tools; }
            set
            {
                _tools = value;
            }
        }

        private IGeoTool GetToolByName(string name)
        {
            return Tools != null ? Tools.FirstOrDefault(tool => tool.ToolName == name) : null;
        }

        private void RefreshTools()
        {
            OnPropertyChanged("IsZoomToolActive");
            OnPropertyChanged("IsDistanceToolActive");
            OnPropertyChanged("IsMapFeatureQueryToolActive");
        }

        private IGeoTool _activeTool;
        public IGeoTool ActiveTool
        {
            get { return _activeTool; }
            set
            {
                _activeTool = value;
                RefreshTools();
            }
        }

        private bool _showProgress;
        public bool ShowProgress
        {
            get
            {
                return _showProgress;
            }
            set
            {
                _showProgress = value;
                OnPropertyChanged("ShowProgress");
            }
        }
      
        public bool IsZoomToolActive
        {
            get
            {
                var tool = GetToolByName("ZoomTool");
                return ActiveTool == tool && tool != null;
            }
            set
            {
                ActiveTool = value ? GetToolByName("ZoomTool") : null;
                OnPropertyChanged("IsZoomToolActive");
            }
        }

        public bool IsDistanceToolActive
        {
            get
            {
                var tool = GetToolByName("DistanceTool");
                return ActiveTool == tool && tool != null;
            }
            set
            {
                ActiveTool = value ? GetToolByName("DistanceTool") : null;

                OnPropertyChanged("IsDistanceToolActive");
            }
        }

        public bool IsMapFeatureQueryToolActive
        {
            get
            {
                var tool = GetToolByName("IsMapFeatureQueryToolActive");
                return ActiveTool == tool && tool != null;
            }
            set
            {
                ActiveTool = value ? GetToolByName("MapFeatureQueryTool") : null;

                OnPropertyChanged("IsMapFeatureQueryToolActive");
            }
        }
        #endregion



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
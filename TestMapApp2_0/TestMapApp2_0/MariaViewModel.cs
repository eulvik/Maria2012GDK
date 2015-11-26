using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TestMapApp2_0.Annotations;
using TPG.GeoFramework.Contracts.Geo.Tool;
using TPG.Maria.Contracts;
using TPG.Maria.DrawObjectLayer;
using TPG.Maria.MapLayer;
using TPG.Maria.TrackLayer;

namespace TestMapApp2_0
{
    public class MariaViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<IGeoTool> _tools;

        public MapViewModel MapViewModel { get; set; }

        public TrackViewModel TrackViewModel { get; set; }


        public ObservableCollection<IMariaLayer> Layers { get; set; }

        public MariaViewModel()
        {
            Layers = new ObservableCollection<IMariaLayer>();
            var mapLayer = new MapLayer();
            MapViewModel = new MapViewModel(mapLayer);
            Layers.Add(mapLayer);

            var trackLayer = new TrackLayer();
            TrackViewModel = new TrackViewModel(trackLayer);
            Layers.Add(trackLayer);

            var drawObjectLayer = new DrawObjectLayer(false)
            {
                InitializeCreationWorkflows = true,
                InitializeGenericCreationWorkflows = true,
                InitializeTacticalCreationWorkflows = false
            };

            DrawObjectViewModel = new DrawObjectViewModel(drawObjectLayer);
            Layers.Add(drawObjectLayer);
        }

        public DrawObjectViewModel DrawObjectViewModel { get; set; }


        public ObservableCollection<IGeoTool> Tools
        {
            private get { return _tools; }
            set
            {
                _tools = value;
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
                OnPropertyChanged(nameof(IsZoomToolActive));
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

                OnPropertyChanged(nameof(IsDistanceToolActive));
            }
        }

        private IGeoTool GetToolByName(string name)
        {
            return Tools != null ? Tools.FirstOrDefault(tool => tool.ToolName == name) : null;
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

        private void RefreshTools()
        {
            OnPropertyChanged(nameof(IsZoomToolActive));
            OnPropertyChanged(nameof(IsDistanceToolActive));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
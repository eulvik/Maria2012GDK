using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TestMapApp.Annotations;
using TPG.GeoFramework.Map.Core.Contracts;
using TPG.GeoFramework.MapServiceInterfaces;
using TPG.GeoUnits;
using TPG.Maria.MapContracts;
using TPG.Maria.MapLayer;

namespace TestMapApp
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private IMariaMapLayer _miniMapLayer;
        public IMariaMapLayer MapLayer { get; private set; }

        public IMariaMapLayer MiniMapLayer
        {
            get { return _miniMapLayer; }
            set
            {
                _miniMapLayer = value;
                OnPropertyChanged("MiniMapLayer");
            }
        }

        public ObservableCollection<string> ActiveMapNames { get; set; }

        public ObservableCollection<IRasterLayerData> LayerDatas { get; set; }

        public GeoPos CenterPosition { get; set; }

        public double Scale
        {
            get
            {
                return MapLayer.GeoContext.CenterScale;
            }
            set
            {
                MapLayer.GeoContext.CenterScale = value;
            }
        }

        public ObservableCollection<Bookmark> Bookmarks { get; private set; }

        public MapViewModel(IMariaMapLayer mapLayer)
        {
            Bookmarks = new ObservableCollection<Bookmark>();

            ActiveMapNames = new ObservableCollection<string>();

            LayerDatas = new ObservableCollection<IRasterLayerData>();

            MapLayer = mapLayer;
            MapLayer.LayerInitialized += OnMapLayerInitialized;

            MiniMapLayer = new MapLayer(MapLayer.MapCatalogServiceClient);
        }

        private void OnMapLayerInitialized()
        {
            Scale = 100000;
            CenterPosition = new GeoPos(60, 10);

            MapLayer.ExtendedMapLayer.FetchVectorLabels = false;
            MapLayer.ExtendedMapLayer.MapRenderSimple = false;
            UpdateData();

            ActiveMapNames.Clear();
            foreach (string activeMapName in MapLayer.ActiveMapNames)
                ActiveMapNames.Add(activeMapName);
            MapLayer.ActiveMapName = ActiveMapNames.First();
            MiniMapLayer.ActiveMapName = ActiveMapNames.First();
        }

        

        public string ActiveMapName
        {
            get { return MapLayer.ActiveMapName; }
            set
            {
                MapLayer.ActiveMapName = value;
                UpdateData();
            }
        }
        private void UpdateData()
        {
            Bookmarks.Clear();
            foreach (Bookmark bookmark in MapLayer.Bookmarks)
                Bookmarks.Add(bookmark);

            LayerDatas.Clear();
            foreach (IRasterLayerData rasterLayerData in MapLayer.MapDataLayers)
                LayerDatas.Add(rasterLayerData);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System.Collections.ObjectModel;
using TPG.GeoFramework.Map.Core.Contracts;
using TPG.GeoFramework.MapServiceInterfaces;
using TPG.GeoUnits;
using TPG.Maria.MapContracts;

namespace TestMapApp
{
    public class MapViewModel
    {
        public IMariaMapLayer MapLayer { get; private set; }

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
        }

        private void OnMapLayerInitialized()
        {
            Scale = 100000;
            CenterPosition = new GeoPos(60, 10);

            MapLayer.ActiveMapName = "NorgeRaster";
            MapLayer.ExtendedMapLayer.FetchVectorLabels = false;
            MapLayer.ExtendedMapLayer.MapRenderSimple = false;
            UpdateData();

            ActiveMapNames.Clear();
            foreach (string activeMapName in MapLayer.ActiveMapNames)
                ActiveMapNames.Add(activeMapName);
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
    }
}
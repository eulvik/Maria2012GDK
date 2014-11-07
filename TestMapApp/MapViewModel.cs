using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TestMapApp.Annotations;
using TPG.GeoFramework.Core;
using TPG.GeoFramework.Map.Core.Contracts;
using TPG.GeoFramework.MapServiceInterfaces;
using TPG.GeoUnits;
using TPG.Maria.MapContracts;
using TPG.Maria.MapLayer;

namespace TestMapApp
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public IMariaMapLayer MapLayer { get; private set; }

        public ObservableCollection<string> ActiveMapNames { get; set; }

        public ObservableCollection<IRasterLayerData> LayerDatas { get; set; }

        public GeoPos CenterPosition { get; set; }
        private string _trackUpdateUri;
        public string TrackUpdateUri 
        {
            get
            {
                return _trackUpdateUri;
            }
            set
            {
                _trackUpdateUri = value;
                OnPropertyChanged("TrackUpdateUri");
            }
        }

        public ICommand ApplyCmnd { get { return new DelegateCommand(x => Apply(x)); } }
        public string Address { get; set; }
        private void Apply(object param)
        {
            string url = string.Format(@"http://mariamapserver.teleplan.no:9005/webgeoloc/json/find/matrikkel/{0}?lat=60&lon=10&facets=false&maxreturnhits=1", Address);
            url = Uri.EscapeUriString(url);
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string str = sr.ReadToEnd();
            var jsonObj = JsonConvert.DeserializeObject<RootObject>(str);

            if (jsonObj.Matches.Count == 0)
                return;

            var pos = jsonObj.Matches.First().PlaceName.Position;
            Destination = new GeoPos(pos.Lat, pos.Lon);
            MapLayer.GeoContext.CenterPosition = new GeoPos(pos.Lat, pos.Lon);
            string trackUpdateUri = string.Format(@"http://mariamapserver.teleplan.no/hacksim/reroute/7?stops={0},{1}", 
                pos.Lon.ToString(CultureInfo.InvariantCulture), pos.Lat.ToString(CultureInfo.InvariantCulture));
            trackUpdateUri = Uri.EscapeUriString(trackUpdateUri);
            TrackUpdateUri = trackUpdateUri;

            System.Net.WebRequest req2 = System.Net.WebRequest.Create(trackUpdateUri);
            System.Net.WebResponse resp2 = req2.GetResponse();
            StreamReader sr2 = new StreamReader(resp2.GetResponseStream());
            string str2 = sr2.ReadToEnd();
            SetSpeed(int.Parse(param.ToString()));

            Task.Factory.StartNew(() => DelayedUpdate());
        }

        private void DelayedUpdate()
        {
            Thread.Sleep(1500);
            SetSymbol();
        }

        public ICommand SetSymbolCommand
        {
            get
            {
                return new DelegateCommand(x => SetSymbol());
            }
        }
        private void SetSymbol()
        {
            string url = @"http://mariamapserver.teleplan.no/hacksim/setsymbol/7/Team3l33t/elite";
            url = Uri.EscapeUriString(url);
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string str = sr.ReadToEnd();
        }

        private void SetSpeed(double kmh)
        {
            string uri = string.Format(@"http://mariamapserver.teleplan.no/hacksim/setspeed/7/{0}", (Math.Floor(kmh/0.036)/100).ToString(CultureInfo.InvariantCulture));
            uri = Uri.EscapeUriString(uri);
            System.Net.WebRequest req = System.Net.WebRequest.Create(uri);
            System.Net.WebResponse resp = req.GetResponse();
        }

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
            Destination = new GeoPos(60, 10);
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

            MapLayer.ExtendedMapLayer.FetchVectorLabels = false;
            MapLayer.ExtendedMapLayer.MapRenderSimple = false;
            UpdateData();

            ActiveMapNames.Clear();
            foreach (string activeMapName in MapLayer.ActiveMapNames)
                ActiveMapNames.Add(activeMapName);
            MapLayer.ActiveMapName = "Norge";// ActiveMapNames.First();
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

        public GeoPos? Destination { get; set; }
    }
}
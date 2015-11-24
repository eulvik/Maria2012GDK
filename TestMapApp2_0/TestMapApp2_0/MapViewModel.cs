using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TestMapApp2_0.Annotations;
using TPG.GeoFramework.MapServiceInterfaces.CatalogServiceData;
using TPG.GeoUnits;
using TPG.Maria.MapContracts;

namespace TestMapApp2_0
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly IMariaMapLayer _mapLayer;
        private GeoPos _centerPosition;
        private double _centerScale;
        private ObservableCollection<MapTemplate> _maps = new ObservableCollection<MapTemplate>();

        public MapViewModel(IMariaMapLayer mapLayer)
        {
            _mapLayer = mapLayer;
            _mapLayer.LayerInitialized += OnLayerInitialized;
        }

        private void OnLayerInitialized()
        {
            foreach (var activeMapTemplate in _mapLayer.ActiveMapTemplates)
                Maps.Add(activeMapTemplate);

            CurrentMap = _mapLayer.ActiveMapTemplates.First();
            CenterPosition = new GeoPos(60, 10);
            CenterScale = 1000000;
        }

        public ObservableCollection<MapTemplate> Maps
        {
            get
            {
                return _maps;
            }
            set
            {
                _maps = value; 
                OnPropertyChanged(nameof(Maps));
            }
        }

        public MapTemplate CurrentMap
        {
            get { return _mapLayer.ActiveMapTemplate; }
            set
            {
                _mapLayer.ActiveMapTemplate = value;
                OnPropertyChanged(nameof(CurrentMap));
            }
        }

        public GeoPos CenterPosition
        {
            get { return _centerPosition; }
            set
            {
                _centerPosition = value; 
                OnPropertyChanged(nameof(CenterPosition));
            }
        }

        public double CenterScale
        {
            get { return _centerScale; }
            set
            {
                _centerScale = value; 
                OnPropertyChanged(nameof(CenterScale));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
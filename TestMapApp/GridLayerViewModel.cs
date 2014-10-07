using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using TestMapApp.Annotations;
using TPG.GeoFramework.Contracts.Geo.Layer;
using TPG.GeoFramework.GridLayer.Contracts;
using TPG.GeoFramework.GridLayer.Grids;
using TPG.Maria.GridContracts;

namespace TestMapApp
{
    public class GridLayerViewModel : INotifyPropertyChanged
    {
        public IMariaGridLayer GridLayer { get; private set; }

        public GridLayerViewModel(IMariaGridLayer gridLayer)
        {
            GridLayer = gridLayer;
            GridLayer.LayerInitialized += OnLayerInitialized;
        }

        private void OnLayerInitialized()
        {
            GridLayer.Grids.Add(new CGRSGrid());
            var utmGrid = GridLayer.Grids.FirstOrDefault(g => g is UTMGrid);
            if (utmGrid != null)
            {
                var grid = utmGrid as UTMGrid;
                grid.MGRS = true;
            }
            SelectedGrid = GridLayer.Grids.First();
        }

        private IGridRenderer _selectedGrid;
        public IGridRenderer SelectedGrid
        {
            get { return _selectedGrid; }
            set
            {
                foreach (IGridRenderer gridRenderer in GridLayer.Grids)
                {
                    if (gridRenderer != value)
                        gridRenderer.Visible = false;
                    else
                        gridRenderer.Visible = true;
                    _selectedGrid = value;
                    OnPropertyChanged("SelectedGrid");
                }
            }
        
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
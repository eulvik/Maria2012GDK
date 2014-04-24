using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TestMapApp.Annotations;
using TPG.GeoFramework.Contracts;
using TPG.GeoFramework.Core;
using TPG.GeoFramework.StyleCore.Condition;
using TPG.GeoFramework.TrackCore;
using TPG.GeoFramework.TrackCore.Contracts;
using TPG.GeoFramework.TrackLayer.Contracts.Selection;
using TPG.Maria.Common;
using TPG.Maria.Contracts;
using TPG.Maria.TrackContracts;

namespace TestMapApp
{
    public class TrackViewModel : INotifyPropertyChanged
    {
        private string _styleXml;
        
        public IMariaTrackLayer TrackLayer { get; set; }

        public ObservableCollection<ITrackData> SelectedTrackDatas { get; set; }

        public string StyleXml
        {
            get { return _styleXml; }
            set
            {
                _styleXml = value;
                OnPropertyChanged("StyleXml");
            }
        }

        public TrackViewModel(IMariaTrackLayer trackLayer)
        {
            TrackLayer = trackLayer;
            TrackLayer.LayerInitialized += OnLayerInitialized;

            SelectedTrackDatas = new ObservableCollection<ITrackData>();
        }

        

        private void OnLayerInitialized()
        {
            TrackLayer.TrackLists = new ObservableCollection<string>()
                                    {
                                        "ais.test"
                                    };
            TrackLayer.TrackServices = new ObservableCollection<IMariaService>
                                    {
                                        new MariaService("TrackService")
                                    };

            
            TrackLayer.ActiveTrackService = TrackLayer.TrackServices.First();
            TrackLayer.ActiveTrackList = TrackLayer.TrackLists.First();

            TrackLayer.ExtendedTrackLayer.TrackSelectionChanged += 
                HandleTrackSelectionChanged;

            StyleXml = TrackLayer.StyleXml;
        }

        private void HandleTrackSelectionChanged(object sender, 
            TrackSelectionChangedEventArgs args)
        {
            foreach (ItemId deselectedTrack in args.DeselectedTracks)
            {
                ITrackData actualToRemove = SelectedTrackDatas.
                    FirstOrDefault(td => td.TrackItemId == deselectedTrack);
                if (actualToRemove != null)
                    SelectedTrackDatas.Remove(actualToRemove);
            }

            foreach (ItemId selectedTrack in args.SelectedTracks)
            {
                ITrackData toAdd = TrackLayer.
                    GetTrackData(selectedTrack.InstanceId).
                    FirstOrDefault();
                if(toAdd != null)
                    SelectedTrackDatas.Add(toAdd);
            }
        }

        private DelegateCommand _applyStyleCommand;
        public ICommand ApplyStyleCommand
        {
            get
            {
                if (_applyStyleCommand == null)
                    _applyStyleCommand = new DelegateCommand(ApplyStyle);

                return _applyStyleCommand;
            }
        }

        private void ApplyStyle(object obj)
        {
            TrackLayer.StyleXml = StyleXml;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, 
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MariaGeoFencing.GeoFenceLayer;
using MariaGeoFencing.Utilities;
using TPG.DrawObjects.Contracts.DrawObjectState;
using TPG.GeoFramework.Core;
using TPG.GeoFramework.GeoFencingClient;
using TPG.GeoFramework.GeoFencingServiceInterfaces;
using TPG.GeoFramework.Style.Core.Contracts.Condition;
using TPG.GeoFramework.StyleCore.Condition;
using TPG.GeoFramework.TrackLayer.Contracts.Selection;
using TPG.Maria.Contracts;
using TPG.Maria.CustomLayer;
using TPG.Maria.DrawObjectContracts;
using TPG.Maria.TrackContracts;

namespace MariaGeoFencing.ViewModels
{
    public class GeoFenceViewModel : ViewModelBase
    {
        #region Locals
        private bool initInProgress;
        private CustomNotificationDef _selectedNotificationItem;

        private string _statusRuleCon;
        private string _statusDataMgrCon;
        private string _statusNotificationCon;

        private readonly MariaWindowViewModel _parent;

        private readonly CustomLayer<GeoFencingLayer> _customLayer;
        private GeoFencingLayer _geoFencingLayer;

        private readonly IMariaDrawObjectLayer _drawObjectLayer;
        private readonly IMariaTrackLayer _trackLayer;
      
        private NotificationQueryResult _lastNewNotifications;
        private NotificationQueryResult _lastAllNotifications;
        #endregion Locals

        #region Properties
        public GeofencingServiceManager ServiceManager { get; set; }

        #region Commands
        public ICommand PollCmnd { get { return new DelegateCommand(x => PollNotifications()); } }
        public ICommand AcknwledgeCmnd { get { return new DelegateCommand(x => Acknwledge()); } }
        public ICommand ResetCmnd { get { return new DelegateCommand(x => Reset()); } }
        public ICommand CheckConnectionCmnd { get { return new DelegateCommand(x => CheckConnection()); } }
        #endregion Commands

        #region Lists & Collections
        public ObservableCollection<string> RulesCollection 
        { 
            get
            {
                var result = new ObservableCollection<string>();
                if (ServiceManager == null)
                    result.Add("--");
                else
                {
                    foreach (var rule in ServiceManager.FenceRuleClient.GetRules())
                    {
                        result.Add(rule.ToString());
                    }
                }
                return result;
            } 
        }

        public CustomNotificationDef SelectedNotificationItem
        {
            get { return _selectedNotificationItem; }
            set
            {
                _selectedNotificationItem = value;

                UpdateCurrentProp();
                UpdateVisualisation();
            }
        }

        public ObservableCollection<CustomNotificationDef> DisplayListItems { get; private set; }
        public ObservableCollection<CustomNotificationDef> DisplayNewListItems { get; private set; }
        #endregion Lists & Collections


        #region Notification details
        public int NotificationGeneration
        {
            get
            {
                if (_lastNewNotifications == null)
                    return 0;

                return _lastNewNotifications.Generation;
            }
        }

        public string NotificationId
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.Id.ToString()
                    : "-";
            }
        }
        public string TimeTriggered
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.TimeTriggered.ToShortTimeString()
                    : "-";
            }
        }
        public string Level
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.Level.ToString()
                    : "-";
            }
        }
        public string Interaction
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.Interaction.ToString()
                    : "-";
            }
        }
        public string Tags
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.GetTagsString()
                    : "-";
            }
        }
        public string TrackId
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.TrackId.ToString()
                    : "-";
            }
        }
        public string FirstTrackPos
        {
            get
            {
                return SelectedNotificationItem != null
                    ? GeoFencePossitionHelper.GetPosString(SelectedNotificationItem.Def.FirstTrackPos)
                    : "-";
            }
        }
        public string LastTrackPos
        {
            get
            {
                return SelectedNotificationItem != null
                    ? GeoFencePossitionHelper.GetPosString(SelectedNotificationItem.Def.LastTrackPos)
                    : "-";
            }
        }
        public string TrackCondition
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.SourceTriggerRule.TrackConditionXml
                    : "-";
            }
        }
        public string TrackInfo
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.GetTrackInfoString()
                    : "-";
            }
        }

        public string GeoShapeId
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.GeoShapeId.ToString()
                    : "-";
            }
        }
        public string GeoShapeCenterPos
        {
            get
            {
                return SelectedNotificationItem != null
                    ? GeoFencePossitionHelper.GetPosString(SelectedNotificationItem.Def.GeoShapeCenter)
                    : "-";
            }
        }
        public string GeoShapeCondition
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.SourceTriggerRule.GeoFenceConditionXml
                    : "-";
            }
        }
        public string GeoShapeInfo
        {
            get
            {
                return SelectedNotificationItem != null
                    ? SelectedNotificationItem.Def.GetGeoShapeInfoString()
                    : "-";
            }
        }

        public int Filtered
        {
            get
            {
                if (_lastAllNotifications == null || _lastAllNotifications.FilteredNotificationIds.Count <= 0)
                    return 0;
                return _lastAllNotifications.FilteredNotificationIds.Count;
            }
        }
        public int Deleted
        {
            get
            {
                if (_lastAllNotifications == null || _lastAllNotifications.DeletedNotificationIds.Count <= 0)
                    return 0;
                return _lastAllNotifications.FilteredNotificationIds.Count;
            }
        }
        #endregion Notification details

        #region Status
        public string StatusRuleCon
        {
            get
            {
                if (ServiceManager == null )
                    return _statusRuleCon;
                
                if (ServiceManager.FenceRuleClient == null ||
                    !ServiceManager.FenceRuleClient.Connected)
                        return  _statusRuleCon + " No connection";
                
                return "Connected";
            }
            private set
            {
                _statusRuleCon = value;
                NotifyPropertyChanged(() => StatusRuleCon);
            }
        }
        public string StatusDataMgrCon
        {
            get
            {
                if (ServiceManager == null)
                    return _statusDataMgrCon;

                if (ServiceManager.DataManagerClient == null ||
                    !ServiceManager.DataManagerClient.Connected)
                    return _statusDataMgrCon + " No connection";

                return "Connected";
            }
            private set
            {
                _statusDataMgrCon = value;
                NotifyPropertyChanged(() => StatusDataMgrCon);
            }
        }
        public string StatusNotificationCon
        {
            get
            {
                if (ServiceManager == null)
                    return _statusNotificationCon;
                if (ServiceManager.NotificationClient == null ||
                    !ServiceManager.NotificationClient.Connected)
                    return _statusNotificationCon + " No connection"; ;
                return "Connected";
            }
            set
            {
                _statusNotificationCon = value;
                NotifyPropertyChanged(() => StatusNotificationCon);
            }
        }
        #endregion Status
        
        #region Filter 
        public string QueryCondition
        {
            get
            {
                if (_lastAllNotifications == null || _lastAllNotifications.Notifications.Count <= 0)
                    return "--";
                return _lastAllNotifications.QueryConditionXml;
            }
        }      
        
        private bool _supressLowLevel;
        public bool SupressLowLevel
        {
            get { return _supressLowLevel; }
            set
            {
                _supressLowLevel = value;
                NotifyPropertyChanged(() => SupressLowLevel);
                PollNotifications();
            }
        }
        private bool _includeAllNew;
        public bool IncludeAllNew
        {
            get { return _includeAllNew; }
            set
            {
                _includeAllNew = value;
                NotifyPropertyChanged(() => IncludeAllNew);

                PollNotifications();
            }
        }
        private bool _supressAcknowledged;
        public bool SupressAcknowledged
        {
            get { return _supressAcknowledged; }
            set
            {
                _supressAcknowledged = value;
                NotifyPropertyChanged(() => SupressAcknowledged);

                PollNotifications();
            }
        }
        #endregion Filter 
        
        #region Visualization 
        private bool _show;
        private bool _selectedNotification;
        private bool _selectedTracksOrGeoShapes;
 
        public bool Show
        {
            get { return _show; }
            set
            {
                _show = value;
                NotifyPropertyChanged(() => Show);
                UpdateVisualisation();
            }
        }
        public bool SelectedNotification
        {
            get { return _selectedNotification; }
            set
            {
                _selectedNotification = value;
                
                if (value)
                {
                    _selectedTracksOrGeoShapes = false;
                }
                else
                    SelectedNotificationItem = null;

                NotifyPropertyChanged(() => SelectedNotification); 
                NotifyPropertyChanged(() => SelectedTracksOrGeoShapes);
                NotifyPropertyChanged(() => DisplayListItems);
                NotifyPropertyChanged(() => DisplayNewListItems);

                PollNotifications();
            }
        }
        public bool SelectedTracksOrGeoShapes
        {
            get { return _selectedTracksOrGeoShapes; }
            set
            {
                _selectedTracksOrGeoShapes = value;
                if (value)
                {
                    _selectedNotification = false;
                }

                NotifyPropertyChanged(() => SelectedNotification);
                NotifyPropertyChanged(() => SelectedTracksOrGeoShapes);

                PollNotifications();
            }
        }
        
        #endregion Visualization
        #endregion Properties

        #region Constructors & initialization
        public GeoFenceViewModel(MariaWindowViewModel parent, IMariaTrackLayer trackLayer, IMariaDrawObjectLayer drawObjectLayer, CustomLayer<GeoFencingLayer> customLayer)
        {
            StatusRuleCon = "-";
            StatusDataMgrCon = "-";
            StatusNotificationCon = "-";

            DisplayListItems = new ObservableCollection<CustomNotificationDef>();
            DisplayNewListItems = new ObservableCollection<CustomNotificationDef>();

            _parent = parent;
            _trackLayer = trackLayer;
            _drawObjectLayer = drawObjectLayer;
            _customLayer = customLayer;
            _geoFencingLayer = _customLayer.Instance;

            _drawObjectLayer.ServiceConnected += OnDataSourceServicesReady;
            _drawObjectLayer.ExtendedDrawObjectLayer.LayerSelectionChanged += OnGeoshapeSelectionChanged;
            _drawObjectLayer.ExtendedDrawObjectLayer.LayerChanged += DrawObjectLayerOnLayerChanged;

            _trackLayer.ServiceConnected += OnDataSourceServicesReady;
            _trackLayer.ExtendedTrackLayer.TrackSelectionChanged += OnTrackSelectionChanged;
            _trackLayer.ExtendedTrackLayer.CurrentTrackDisplayItemsChanged += CurrentTrackDisplayItemsChanged;

            _customLayer.LayerInitialized += OnGeoFenceLayerInitialized;
        }

        void CurrentTrackDisplayItemsChanged()
        {
          //Trace.Write(". ");
        }

        private void DrawObjectLayerOnLayerChanged(object sender, DataStoreChangedEventArgs args)
        {
            Trace.WriteLine("Change: GeoShapes: " + args.AffectedDrawObjects.Count() + " Action:" + args.DataStoreChangedAction);
        }

        private void OnGeoFenceLayerInitialized()
        {
            Show = false;
            SelectedTracksOrGeoShapes = false;
            SelectedNotification = false;
            Show = true;

            _geoFencingLayer = _customLayer.Instance;

            GeoFencePossitionHelper.GeoContext = _geoFencingLayer.GeoContext;
        }


        private void OnDataSourceServicesReady(object sender, MariaServiceEventArgs args)
        {
            if ((!initInProgress) && _trackLayer.IsInitialized && _drawObjectLayer.IsInitialized)
            {
                initInProgress = true;
                ServiceManager = new GeofencingServiceManager(
                    "GeoFencingService/FenceRule",
                    "GeoFencingService/DataManager",
                    "GeoFencingService/NotificationHandling");

                ServiceManager.ServiceRestarted += OnServiceManagerServiceRestarted;

                StatusDataMgrCon = "Connecting...";
                StatusRuleCon = "Connecting...";
                StatusNotificationCon = "Connecting...";

                ServiceManager.WaitUntilReady().ContinueWith(a => InitiateServerInfo());
            }
        }

        private void InitiateServerInfo()
        {
            StatusDataMgrCon = "Init completed";
            StatusRuleCon = "Init completed";
            StatusNotificationCon = "Init completed";


            ServiceManager.AddDataSource(_parent.TrackViewModel.ActiveTrackService.Name, _parent.StoreId,
                                         Guid.Parse(GeoFenceDefs.TrackDataSourceDefinitionID), GenericObjectType.Track);

            ServiceManager.AddDataSource(_parent.DrawObjectViewModel.ActiveDrawObjectService.Name, _parent.StoreId,
                                         Guid.Parse(GeoFenceDefs.GeoShapeDataSourceDefinitionID), GenericObjectType.GeoShape);

            ServiceManager.FenceRuleClient.AddRule(GeoFencingRuleHelper.SimpleCrossingRule());
            ServiceManager.FenceRuleClient.AddRule(GeoFencingRuleHelper.SimpleEnteringRule());
            ServiceManager.FenceRuleClient.AddRule(GeoFencingRuleHelper.SimpleLeavingRule());
            NotifyPropertyChanged(() => RulesCollection);
        }
        #endregion Constructors & initialization

        void OnServiceManagerServiceRestarted(object sender, TPG.ServiceModel.EventArgs.ServiceRestartedEventArgs args)
        {
            ServiceManager.WaitUntilReady().ContinueWith(a => InitiateServerInfo());
        }

        private void OnGeoshapeSelectionChanged(object sender, DrawObjectSelectionChangedEventArgs args)
        {
            PollNotifications();
        }

        private void OnTrackSelectionChanged(object sender, TrackSelectionChangedEventArgs args)
        {
            PollNotifications();
        }

        private void CheckConnection()
        {
            NotifyPropertyChanged(() => StatusRuleCon);
            NotifyPropertyChanged(() => StatusDataMgrCon);
            NotifyPropertyChanged(() => StatusNotificationCon);
        }

        private void Reset()
        {
            var stat = MessageBox.Show(
                "Are you sure you want to restore all notificationes for the service?",
                "Reset reqested!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            
            if (stat == MessageBoxResult.Yes)
            {
                ServiceManager.NotificationClient.RestoreNotificationLog();
                PollNotifications();
            }
        }

        private void Acknwledge()
        {
            if (SelectedNotificationItem != null)
            {
                ServiceManager.NotificationClient.SetNotificationUserData(SelectedNotificationItem.Def.Id,
                    new UserDataDefinition { Owner = GeoFenceDefs.OwnerName, Key = GeoFenceDefs.StateKey, Value = GeoFenceDefs.AcknowledgedValue });

                PollNotifications();
            }
        }

        private void PollNotifications()
        {
            if (ServiceManager == null ||
                ServiceManager.NotificationClient == null ||
                !ServiceManager.NotificationClient.Connected)
                return;

            var conditionXml = DisplayCondition();

            var lastGeneration = NotificationGeneration;
            _lastNewNotifications = ServiceManager.NotificationClient.GetNotifications("", lastGeneration);

            DisplayNewListItems.Clear();
            if (NotificationGeneration > lastGeneration && _lastNewNotifications != null )
            {
                foreach (var item in _lastNewNotifications.Notifications)
                {
                    var rec = CreateNotificationRecord(item);
                    DisplayNewListItems.Add(rec);
                }
            }

            _lastAllNotifications = ServiceManager.NotificationClient.GetNotifications(conditionXml, -1);

            DisplayListItems.Clear();
            if (_lastAllNotifications != null)
            {
                foreach (var item in _lastAllNotifications.Notifications)
                {
                    DisplayListItems.Add(CreateNotificationRecord(item));
                }
            }
            
            UpdateNotificationProperties();
            UpdateVisualisation();
        }

        private CustomNotificationDef CreateNotificationRecord(NotificationDef item)
        {
            var rec = new CustomNotificationDef(item);

            var tracks = _trackLayer.GetTrackData(item.TrackId.ObjectId);

            if (tracks != null && tracks.Length > 0 &&  tracks[0].Pos.HasValue)
                rec.CurrentTrackPos = new LatLonPos(tracks[0].Pos.Value.Lat, tracks[0].Pos.Value.Lon);
            return rec;
        }

        private void UpdateVisualisation()
        {
            if (_geoFencingLayer == null)
                return;

            _geoFencingLayer.Clear();

            if (!Show)
                return;

            if (SelectedNotification)
            {
                if (SelectedNotificationItem == null)
                    return;

                _geoFencingLayer.UpdateVisualisation(new List<CustomNotificationDef> {SelectedNotificationItem}, true);
            }
            else
            {
                if (DisplayListItems == null)
                    return;

                _geoFencingLayer.UpdateVisualisation( DisplayListItems, SelectedTracksOrGeoShapes);
            }
        }

        private string DisplayCondition()
        {
            var conditionXml = "";
            var compMainCondition = new CompositeCondition {Operator = GroupOperator.And};

            if (SupressLowLevel)
            {
                var compSubCondition = new CompositeCondition {Operator = GroupOperator.Or};
                compSubCondition.Children.Add(new FieldCondition("Level", NotificationLevel.Low.ToString(),
                    FieldOperator.NEq));

                if (IncludeAllNew)
                    compSubCondition.Children.Add(new AgeCondition {Operator = FieldOperator.Lt, Age = 60.0});

                conditionXml = GeoFencingFilterHelper.GenericFielsConditionFilter(compSubCondition);

                compMainCondition.Children.Add(compSubCondition);
            }

            if (SupressAcknowledged)
            {
                compMainCondition.Children.Add(new FieldCondition(GeoFenceDefs.StateUserData,
                    GeoFenceDefs.AcknowledgedValue, FieldOperator.NEq));
                conditionXml = GeoFencingFilterHelper.GenericFielsConditionFilter(compMainCondition);
            }

            if (Show && SelectedTracksOrGeoShapes)
            {
                // First handle selected tracks, if any....
                var compTrackCondition = new CompositeCondition {Operator = GroupOperator.And};
                var compTrackCollection = new CompositeCondition {Operator = GroupOperator.Or};
                var selTracks = _trackLayer.ExtendedTrackLayer.Selected;
                if (selTracks.Count > 0)
                {
                    foreach (var id in selTracks)
                    {
                        compTrackCollection.Children.Add(new FieldCondition("trackid", id.InstanceId, FieldOperator.Eq));
                    }
                }
                compTrackCondition.Children.Add(new FieldCondition("tracklistid", _parent.StoreId, FieldOperator.Eq));
                compTrackCondition.Children.Add(compTrackCollection);

                // Then handle selected Geo Shapes, if any
                var compDrawObjCondition = new CompositeCondition {Operator = GroupOperator.And};

                var selectedDrawObjects = _drawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds;
                var compDrawObjCollection = new CompositeCondition {Operator = GroupOperator.Or};
                if (selectedDrawObjects.Any())
                {
                    foreach (var id in selectedDrawObjects)
                    {
                        compDrawObjCollection.Children.Add(new FieldCondition("geoshapeid", id, FieldOperator.Eq));
                    }
                }

                compDrawObjCondition.Children.Add(new FieldCondition("geoshapelistid", _parent.StoreId, FieldOperator.Eq));
                compDrawObjCondition.Children.Add(compDrawObjCollection);

                // Add all 
                var compSelectedTracksOrGeoShapes = new CompositeCondition {Operator = GroupOperator.Or};
                compSelectedTracksOrGeoShapes.Children.Add(compTrackCondition);
                compSelectedTracksOrGeoShapes.Children.Add(compDrawObjCondition);
                compMainCondition.Children.Add(compSelectedTracksOrGeoShapes);

                conditionXml = GeoFencingFilterHelper.GenericFielsConditionFilter(compMainCondition);
            }

            return conditionXml;
        }

        private void UpdateCurrentProp()
        {
            NotifyPropertyChanged(() => SelectedNotificationItem);
            NotifyPropertyChanged(() => NotificationId);
            NotifyPropertyChanged(() => TimeTriggered);
            NotifyPropertyChanged(() => Interaction);
            NotifyPropertyChanged(() => Level);
            NotifyPropertyChanged(() => Tags);
            NotifyPropertyChanged(() => TrackId);
            NotifyPropertyChanged(() => FirstTrackPos);
            NotifyPropertyChanged(() => LastTrackPos);
            NotifyPropertyChanged(() => TrackCondition);
            NotifyPropertyChanged(() => TrackInfo);
            NotifyPropertyChanged(() => GeoShapeId);
            NotifyPropertyChanged(() => GeoShapeCenterPos);
            NotifyPropertyChanged(() => GeoShapeCondition);
            NotifyPropertyChanged(() => GeoShapeInfo);
        }

        private void UpdateNotificationProperties()
        {
            UpdateCurrentProp();

            NotifyPropertyChanged(() => DisplayListItems);
            NotifyPropertyChanged(() => DisplayNewListItems);
            NotifyPropertyChanged(() => NotificationGeneration);
            NotifyPropertyChanged(() => Filtered);
            NotifyPropertyChanged(() => Deleted);
            NotifyPropertyChanged(() => QueryCondition);

            NotifyPropertyChanged(() => SelectedTracksOrGeoShapes);
        }
    }
}

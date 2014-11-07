﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TPG.GeoFramework.Core;
using TPG.GeoFramework.LocationServiceClient;
using TPG.GeoFramework.LocationServiceClient.Search;
using TPG.GeoFramework.LocationServiceInterfaces;
using TPG.GeoFramework.SearchLayer;
using TPG.GeoFramework.SearchLayer.Contracts;
using TPG.Maria.SearchContracts;
using TPG.ServiceModel;

namespace MariaSearch.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly IMariaSearchLayer _searchLayer;

        private readonly ILocationServiceClient _locationServiecClient;
        private readonly LocationProvider _geoLocationProvider;

        private CollectionView _searchMatchView;

        #region Properties

        public ICommand OnSearchCmd { get { return new DelegateCommand(x => DoSearch()); } }
        public ICommand OnClearCmd { get { return new DelegateCommand(x => DoClearSearch()); } }
        public ICommand OnClearFacetsCmd { get { return new DelegateCommand(x => DoClearFacet()); } }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set 
            {
                _searchText = value;
                NotifyPropertyChanged(() => SearchText);
            }
        }

        public ObservableCollection<ISearchMatch> SearchMatches
        {
            get { return _searchLayer.SearchMatches; }
        }

        public ISearchMatch SelectedSearchMatch
        {
            get { return _searchLayer.SelectedSearchMatch; }
            set
            {
                if (value != null )
                    _searchLayer.GeoNavigator.CenterPosition = value.Position;

                _searchLayer.SelectedSearchMatch = value;
            }
        }

        public int MaxHitCount { get; set; }

        public bool ShowSearchMatchMark
        {
            get { return _searchLayer.ShowSearchMatchMark; }
            set 
            {
                _searchLayer.ShowSearchMatchMark = value;
                NotifyPropertyChanged(() => ShowSearchMatchMark);
            }
        }
        public bool ShowSearchMatchName 
        {
            get { return _searchLayer.ShowSearchMatchName; }
            set 
            {
                _searchLayer.ShowSearchMatchName = value;
                NotifyPropertyChanged(() => ShowSearchMatchName);
            }
        }

        public ObservableCollection<ExtendedSearchFacet> Facets { get; private set; }
        public ObservableCollection<ExtendedSearchFacet> SelectedFacets { get; private set; }
        public ExtendedSearchFacet SelectedFacet
        {
            set
            {
                if (value != null)
                    _searchLayer.SelectedFacets.Add(value.Facet);
                else
                     _searchLayer.SelectedFacets.Clear();
            }
        }
        public int MinimumFacetOccurenceFilter
        {
            get { return _searchLayer.MinimumFacetOccurenceFilter; }
            set
            {
                _searchLayer.MinimumFacetOccurenceFilter = value;
                NotifyPropertyChanged(() => MinimumFacetOccurenceFilter);
            }
        }

        public ObservableCollection<LocationDatabaseInfo> LocationDbInfo { get; private set; }
        
        public LocationDatabaseInfo ActiveLocationDbInfo
        {
            get { return _geoLocationProvider.ActiveDatabaseInfo; }
            set
            {
                _geoLocationProvider.ActiveDatabaseInfo = value;
                NotifyPropertyChanged(() => ActiveLocationDbInfo);

                DoSearch();
            }
        }
        #endregion Properties

        #region Initialization
        public SearchViewModel(IMariaSearchLayer searchLayer)
        {
            _searchLayer = searchLayer;
            _searchLayer.LayerInitialized += SearchLayerOnLayerInitialized;

            LocationDbInfo = new ObservableCollection<LocationDatabaseInfo>();
        
            ILocationServiceFactory locationServiceFactory = new LocationServiceFactory(new BindingFactory(), new EndpointAddressFactory());
            _locationServiecClient = locationServiceFactory.New("LocationService");
            _locationServiecClient.Connect(2000);

            _geoLocationProvider = new LocationProvider
                                       {
                                           LocationServiceClient = _locationServiecClient
                                       };
        }

        private void SearchLayerOnLayerInitialized()
        {
            var nvc = System.Configuration.ConfigurationManager.GetSection("LocationConfig") as NameValueCollection;

            if (nvc == null)
            {
                MessageBox.Show("Could not provide Location Service info... ", "Connection error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
           
            
            var activeDb = nvc.Get("DefaultDatabase");
            var dbInfo = _geoLocationProvider.GetLocationDatabaseInfo();

            if (dbInfo == null)
            {
                MessageBox.Show("Could not provide Location Database Info... ", "Database error!",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            foreach (var dbi in dbInfo)
            {
                LocationDbInfo.Add(dbi);

                if (dbi.DataSource.Contains(activeDb))
                    ActiveLocationDbInfo = dbi;
            }

            if (_geoLocationProvider.ActiveDatabaseInfo == null)
            {
                MessageBox.Show("Could not provide Active Database Info... ", "Database error!",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            _searchLayer.SearchProviders.Add(_geoLocationProvider);
            _searchLayer.SearchCompleted += SearchLayerOnSearchCompleted;

            NotifyPropertyChanged(() => LocationDbInfo);

            _searchMatchView = (CollectionView)CollectionViewSource.GetDefaultView(SearchMatches);
            _searchLayer.SearchMatchSelectionChanged += OnSearchMatchSelectionChanged;
            _searchMatchView.CurrentChanged += OnCurrentChanged;

            MaxHitCount = 50;
            MinimumFacetOccurenceFilter = 1;
            ShowSearchMatchMark = true;
            ShowSearchMatchName = true;
            NotifyPropertyChanged(() => ShowSearchMatchMark);
            NotifyPropertyChanged(() => ShowSearchMatchName);

            Facets = new ObservableCollection<ExtendedSearchFacet>();
            SelectedFacets = new ObservableCollection<ExtendedSearchFacet>();

            _searchLayer.Facets.CollectionChanged += OnFacetsCollectionChanged;
            _searchLayer.SelectedFacets.CollectionChanged += OnSelectedFacetsCollectionChanged;
        }

        void OnCurrentChanged(object sender, EventArgs e)
        {
            if (_searchMatchView.CurrentItem == null)
                return;

            SelectedSearchMatch = _searchMatchView.CurrentItem as ISearchMatch;
            NotifyPropertyChanged(() => SelectedSearchMatch);
        }
        void OnSearchMatchSelectionChanged(object sender, SearchMatchSelectionChangedEventArgs args)
        {
            _searchMatchView.MoveCurrentTo(args.SelectedSearchMatch);

            NotifyPropertyChanged(() => SelectedSearchMatch);
        }
        
        private void OnSelectedFacetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleFacetCollectionChanged(e, SelectedFacets);
            NotifyPropertyChanged(() => SelectedFacets);
        }
        private void OnFacetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleFacetCollectionChanged(e, Facets);
            NotifyPropertyChanged(() => Facets);
        }
        private void HandleFacetCollectionChanged(NotifyCollectionChangedEventArgs e,
            ObservableCollection<ExtendedSearchFacet> facets)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                facets.Clear();
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var obj in e.NewItems)
                {
                    var facet = (ISearchFacet) obj;
                    if (facet == null)
                        continue;

                    var extended = new ExtendedSearchFacet(facet);
                    facets.Add(extended);
                }
            }
        }

        private void SearchLayerOnSearchCompleted(object sender, EventArgs args)
        {
            NotifyPropertyChanged(() => SearchMatches);           
            NotifyPropertyChanged(() => Facets);
            NotifyPropertyChanged(() => SelectedFacets);
        }

        #endregion Initialization

        private void DoClearSearch()
        {
            _searchLayer.Clear();
            SearchText = "";

        }
        private void DoClearFacet()
        {
            SelectedFacet = null;
            SelectedFacets.Clear();

            DoSearch();
        }

        public void DoSearch()
        {           
            if (String.IsNullOrWhiteSpace(SearchText))
            {
                DoClearSearch();
                return;
            }

            var query = new SearchQuery
            {
                SearchText = SearchText,
                MaxHitCount = MaxHitCount,
                MaxInternalHits = MaxHitCount,
                CenterPosition = _searchLayer.GeoContext.CenterPosition,
                AutoWildcard = false,
                ExtractFacets = true,
                Facets = ConvertList(SelectedFacets)
            };

            _searchLayer.Search(query);
        }

        private List<ISearchFacet> ConvertList(IEnumerable<ExtendedSearchFacet> extendedList)
        {
            var list = new List<ISearchFacet>();
            foreach (var extFacet in extendedList)
            {
                list.Add(extFacet.Facet);
            }
            return list;
        }
    }
}
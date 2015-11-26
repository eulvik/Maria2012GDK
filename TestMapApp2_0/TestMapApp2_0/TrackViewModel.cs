using System;
using System.IO;
using TestMapApp2_0.Properties;
using TPG.GeoFramework.Contracts;
using TPG.GeoFramework.TrackCore;
using TPG.GeoFramework.TrackCore.Contracts;
using TPG.GeoUnits;
using TPG.Maria.Common;
using TPG.Maria.Contracts;
using TPG.Maria.TrackContracts;

namespace TestMapApp2_0
{
    public class TrackViewModel
    {
        private IMariaTrackLayer _trackLayer;
        private MariaService _trackService;

        public TrackViewModel(IMariaTrackLayer trackLayer)
        {
            _trackLayer = trackLayer;
            _trackLayer.LayerInitialized += OnLayerInitialized;
            _trackLayer.ServiceConnected += OnTrackServiceConnected;
        }

        private void OnTrackServiceConnected(object sender, MariaServiceEventArgs args)
        {
            _trackLayer.ActiveTrackList = "eSmart";
            _trackLayer.StyleXml = Resources.TrackStyle;
            for (int i = 0; i < 10; i++)
            {
                ITrackData d = new TrackData(new ItemId("eSmart", Guid.NewGuid().ToString()), new GeoPos(60, 10+(i*0.1)), null, null);
                d.Fields["status"] = "out";
                d.Fields["symbolcode"] = "someCode";
                d.Fields["Symbology"] = "eSmart";
               _trackLayer.SetTrackData(d);
            }
        }


        private void OnLayerInitialized()
        {
            File.WriteAllText(@"C:\temp\TrackStyle.xml", _trackLayer.StyleXml);
            _trackService = new MariaService("TrackService");
            _trackLayer.ActiveTrackService = _trackService;
            var symbolProvider = new BitmapSymbolProvider();
            _trackLayer.SymbolProviders[symbolProvider.GetProviderType()] = symbolProvider;
        }
    }
}

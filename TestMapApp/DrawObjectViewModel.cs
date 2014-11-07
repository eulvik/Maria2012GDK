using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using TPG.DrawObjects.Contracts.GenericObjects;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI.Primitives;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI.Types;
using TPG.GeoFramework.Core;
using TPG.GeoUnits;
using TPG.Maria.Common;
using TPG.Maria.Contracts;
using TPG.Maria.DrawObjectContracts;

namespace MariaGeoFencing.ViewModels
{
    public class DrawObjectViewModel : ViewModelBase
    {
        private readonly IMariaDrawObjectLayer _drawObjectLayer;
        private ICreationWorkflow _activeCreationWorkflow;
        private ulong _objCnt = 0;
        

        public DrawObjectViewModel(IMariaDrawObjectLayer drawObjectLayer)
        {
            _drawObjectLayer = drawObjectLayer;
            _drawObjectLayer.LayerInitialized += DrawObjectLayerOnLayerInitialized;
        }
        private void DrawObjectLayerOnLayerInitialized()
        {
            _drawObjectLayer.DrawObjectServices = new ObservableCollection<IMariaService>
            {
                new MariaService("DrawObjectService")
            };
            _drawObjectLayer.ServiceConnected += OnServiceConnected;
        }

        private void OnServiceConnected(object sender, MariaServiceEventArgs args)
        {
            _drawObjectLayer.ActiveDrawObjectService = _drawObjectLayer.DrawObjectServices.First();
            _drawObjectLayer.ActiveDrawObjectServiceStore = "test";
        }

        
       

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TPG.DrawObjects.Contracts.DrawObjectState;
using TPG.DrawObjects.Contracts.GenericObjects;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI;
using TPG.DrawObjects.Contracts.SimpleDrawObjectAPI.Primitives;
using TPG.GeoFramework.Core;
using TPG.GeoFramework.Style.Core.Contracts.Condition;
using TPG.GeoFramework.Style.Core.Contracts.Sorting;
using TPG.GeoFramework.StyleCore.Condition;
using TPG.Maria.DrawObjectContracts;

namespace TestMapApp
{
    public class DrawObjectViewModel
    {
        public IMariaDrawObjectLayer DrawObjectLayer { get; private set; }
        public ObservableCollection<string> DrawObjectsInView { get; private set; }
        public ObservableCollection<string> DrawObjectsQueried { get; private set; }

        public DrawObjectViewModel(IMariaDrawObjectLayer drawObjectLayer)
        {
            DrawObjectsInView = new ObservableCollection<string>();
            DrawObjectLayer = drawObjectLayer;
            DrawObjectsQueried = new ObservableCollection<string>();

            DrawObjectLayer.LayerInitialized += OnLayerInitialized;
        }

        private void OnLayerInitialized()
        {
            //DrawObjectLayer.ActiveDrawObjectService =
            //    new MariaService("DrawObjectService") { WaitForConnection = true };
            //DrawObjectLayer.ActiveDrawObjectServiceStore = "test";
            
            DrawObjectLayer.ExtendedDrawObjectLayer.SetSymbolProvider("WaypointSymbology", new BitmapFileSymbolProvider());

            DrawObjectLayer.GeoContext.ViewportChanged += ViewportChanged;

            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            var files = Directory.GetFiles("Data");
            foreach (var file in files)
            {
                if (Path.GetExtension(file).Contains("xml"))
                {
                    string xml = File.ReadAllText(file);
                    DrawObjectLayer.UpdateStore(xml);
                }
            }

            DrawObjectLayer.ExtendedDrawObjectLayer.LayerChanged += OnLayerChanged;
            DrawObjectLayer.ExtendedDrawObjectLayer.ActiveCreationWorkflowCompleted += CreateionWorkflowCompleted;
            StyleXml = DrawObjectLayer.StyleXml;

        }

        private DelegateCommand _createWaypointCommand;
        public ICommand CreateWaypointCommand
        {
            get
            {
                if (_createWaypointCommand == null)
                    _createWaypointCommand =
                        new DelegateCommand(CreateWaypoint);

                return _createWaypointCommand;
            }
        }

        private void CreateWaypoint(object obj)
        {
            ISimpleDrawObject symbol = DrawObjectLayer.DrawObjectFactory.CreateSymbolInView(DrawObjectLayer.GeoContext.Viewport.Center);
            symbol.GenericDataFields.SymbolCode = "waypoint.jpg";
            symbol.GenericDataFields.SymbolType = "WaypointSymbology";
            DrawObjectLayer.UpdateStore(symbol);
        }

        private void CreateionWorkflowCompleted(object sender, ActiveWorkflowCompletedEventArgs args)
        {

            if (DrawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds.Any())
            {
                try
                {
                    string selectedId = DrawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds.First();
                    ISimpleDrawObject simpleDrawObject = DrawObjectLayer.GetDrawObjectFromStore(selectedId);
                    simpleDrawObject.DataFields.ExtraFields.Add("DrawingLayerId", Guid.NewGuid().ToString());
                    simpleDrawObject.DataFields.ExtraFields.Add("Visible", "true");
                    DrawObjectLayer.UpdateStore(simpleDrawObject);
                }
                catch (Exception ex)
                {

                }
            }

        }

        private void OnLayerChanged(object sender, DataStoreChangedEventArgs args)
        {
            if (args.DataStoreChangedAction == DataStoreChangeAction.Delete)
                DeleteDrawObjects(args.AffectedDrawObjects);
            else if (args.DataStoreChangedAction == DataStoreChangeAction.Update)
            {
                UpdateDrawObjects(args.AffectedDrawObjects);
            }
        }

        private void UpdateDrawObjects(IEnumerable<string> affectedDrawObjects)
        {
            foreach (var affectedDrawObject in affectedDrawObjects)
            {
                string file = CreateDrawObjectFileString(affectedDrawObject);
                if (File.Exists(file))
                    File.Delete(file);
                File.WriteAllText(file, DrawObjectLayer.GetDrawObjectXMLFromStore(affectedDrawObject));
            }
        }

        private void DeleteDrawObjects(IEnumerable<string> affectedDrawObjects)
        {
            foreach (var affectedDrawObject in affectedDrawObjects)
            {
                string file = CreateDrawObjectFileString(affectedDrawObject);
                if(File.Exists(file))
                    File.Delete(file);
            }
        }

        private string CreateDrawObjectFileString(string instanceId)
        {
            return string.Format("Data/{0}.xml", instanceId);
        }

        private void ViewportChanged(object sender, EventArgs args)
        {
            
            DrawObjectsInView.Clear();
            List<string> inView = DrawObjectLayer.ExtendedDrawObjectLayer.GetVisibleObjectIds();
            foreach (var idInView in inView)
            {
                DrawObjectsInView.Add(idInView);
            }
        }

        private string _selectDrawObject;
        public string SelectDrawObject
        {
            get { return _selectDrawObject; }
            set
            {
                _selectDrawObject = value;
                DrawObjectLayer.ExtendedDrawObjectLayer.Select(_selectDrawObject, true);
                DrawObjectLayer.ExtendedDrawObjectLayer.EnsureWithinMapView(_selectDrawObject);
            }
        }

        private DelegateCommand _creationWorkflowActivatedCommand;
        public ICommand CreationWorkflowActivatedCommand
        {
            get
            {
                if (_creationWorkflowActivatedCommand == null)
                    _creationWorkflowActivatedCommand = 
                        new DelegateCommand(ActivateDelegateCommand);

                return _creationWorkflowActivatedCommand;
            }
        }

        private void ActivateDelegateCommand(object obj)
        {
            var creationWorkflow = obj as ICreationWorkflow;
            if (creationWorkflow == null)
                return;

            DrawObjectLayer.ExtendedDrawObjectLayer.
                ActivateCreationWorkflow(creationWorkflow.ObjectTypeId);
        }

        private string _currentSite;
        public string CurrentSite
        {
            get { return _currentSite; }
            set
            {
                _currentSite = value;
                ICondition filter = new FieldCondition("Layer", _currentSite, FieldOperator.Eq);
                DrawObjectLayer.DisplayFilter.Filter = filter;
            }
        }


        private DelegateCommand _applyNewStyleCommand;
        public ICommand ApplyNewStyleCommand
        {
            get
            {
                if (_applyNewStyleCommand == null)
                    _applyNewStyleCommand = new DelegateCommand(ApplyStyle);

                return _applyNewStyleCommand;
            }
        }

        private void ApplyStyle(object obj)
        {
            DrawObjectLayer.StyleXml = StyleXml;
        }

        public string StyleXml { get; set; }

        public string A { get; set; }
        public string R { get; set; }
        public string G { get; set; }
        public string B { get; set; }
        public string W { get; set; }

        private DelegateCommand _centerToSelectedCommand;
        public ICommand CenterToSelectedCommand
        {
            get
            {
                if (_centerToSelectedCommand == null)
                    _centerToSelectedCommand = new DelegateCommand(CenterToSelected, 
                        CanCenterToSelected);

                return _centerToSelectedCommand;
            }
        }

        private bool CanCenterToSelected(object obj)
        {
            return DrawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds.Any();
        }

        private void CenterToSelected(object obj)
        {
            DrawObjectLayer.ExtendedDrawObjectLayer.EnsureWithinMapView(
                DrawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds.ToArray());
        }




        private DelegateCommand _disableSelectedCommand;
        public ICommand DisableSelectedCommand
        {
            get
            {
                if (_disableSelectedCommand == null)
                    _disableSelectedCommand = new DelegateCommand(DisableSelected);

                return _disableSelectedCommand;
            }
        }

        private void DisableSelected(object obj)
        {
            foreach (string selectedDrawObjectId in DrawObjectLayer.ExtendedDrawObjectLayer.SelectedDrawObjectIds)
            {
                ISimpleDrawObject simpleDrawObject = DrawObjectLayer.GetDrawObjectFromStore(selectedDrawObjectId);
                simpleDrawObject.DataFields.ExtraFields["myDisabled"] = "true";
                DrawObjectLayer.UpdateStore(simpleDrawObject);
            }
            DrawObjectLayer.ExtendedDrawObjectLayer.DisabledFilter = new FieldCondition("myDisabled", "true", FieldOperator.Eq);
        }


        private DelegateCommand _saveDrawObjectsCommand;
        public ICommand SaveDrawObjectsCommand
        {
            get
            {
                if(_saveDrawObjectsCommand == null)
                    _saveDrawObjectsCommand = new DelegateCommand(SaveDrawObjects);

                return _saveDrawObjectsCommand;
            }
            
        }

        private void SaveDrawObjects(object obj)
        {
            foreach (var drawObjectId in DrawObjectLayer.ExtendedDrawObjectLayer.DrawObjectIds)
            {
                var xml = DrawObjectLayer.GetDrawObjectXMLFromStore(drawObjectId);
                File.WriteAllText(string.Format("Data/{0}.xml", drawObjectId), xml);
            }
        }

        private DelegateCommand _performQueryCommand;
        public ICommand PerformQueryCommand 
        {
            get
            {
                if (_performQueryCommand == null)
                    _performQueryCommand = new DelegateCommand(PerformQuery);

                return _performQueryCommand;
            }
            
        }

        private void PerformQuery(object obj)
        {
            DrawObjectsQueried.Clear();
            FieldCondition fc = new FieldCondition("Site", "Site10", FieldOperator.Eq);

            var drawObjects = DrawObjectLayer.GetSortedDrawObjectsFromStore(fc, new List<SortInfoItem>(), 0, int.MaxValue);
            foreach (var drawObject in drawObjects.SortedDrawObjects)
            {
                DrawObjectsQueried.Add(drawObject.Id);
            }
        }

        public void SetDashStyle()
        {
            var drawObjects = DrawObjectLayer.GetSortedDrawObjectsFromStore(null, null, 0, int.MaxValue);
            foreach (var dObj in drawObjects.SortedDrawObjects)
            {
                dObj.GenericDataFields.LineDashStyle.Add(1.0);
                dObj.GenericDataFields.LineDashStyle.Add(2.0);
                DrawObjectLayer.UpdateStore(dObj);
            }
        }
    }
}
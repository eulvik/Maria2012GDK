using System.Windows.Input;
using TPG.GeoFramework.Core;
using TPG.Maria.DrawObjectContracts;
using TPG.Maria.DrawObjectLayer;

namespace TestMapApp2_0
{
    public class DrawObjectViewModel
    {
        public IMariaDrawObjectLayer DrawObjectLayer { get; }

        public DrawObjectViewModel(IMariaDrawObjectLayer drawObjectLayer)
        {
            DrawObjectLayer = drawObjectLayer;
            DrawObjectLayer.LayerInitialized += OnLayerInitialized;
        }

        private void OnLayerInitialized()
        {
            
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

            DrawObjectLayer.ExtendedDrawObjectLayer.ActivateCreationWorkflow(creationWorkflow.ObjectTypeId);
        }
    }
}

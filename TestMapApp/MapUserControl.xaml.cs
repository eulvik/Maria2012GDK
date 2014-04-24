using System;
using System.Windows;
using System.Windows.Controls;

namespace TestMapApp
{
    /// <summary>
    /// Interaction logic for MapUserControl.xaml
    /// </summary>
    public partial class MapUserControl : UserControl, IDisposable
    {
        public MapUserControl()
        {
            InitializeComponent();
            DataContext = new MariaWindowViewModel();
        }

        private void MapUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void MapUserControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            MariaCtrl.Dispose();
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
        }

        private void MapUserControl_OnDrop(object sender, DragEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as MariaWindowViewModel;
            if (dc == null)
                return;

            dc.DrawObjectViewModel.SetDashStyle();

        }

        public void Dispose()
        {
            MariaCtrl.Dispose();
        }
    }
}

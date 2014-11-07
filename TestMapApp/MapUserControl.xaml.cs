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




        public void Dispose()
        {
            MariaCtrl.Dispose();
        }
    }
}

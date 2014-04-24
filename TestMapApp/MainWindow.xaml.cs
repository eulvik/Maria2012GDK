using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TestMapApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            MapUserCtrl.Dispose();
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}

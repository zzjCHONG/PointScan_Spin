using Simscop.Spindisk.Core.ViewModels;
using System.Windows;

namespace Simscop.Spindisk.WPF.Views
{
    /// <summary>
    /// Interaction logic for BaseCameraView.xaml
    /// </summary>
    public partial class BaseCameraView
    {
        public BaseCameraView()
        {
            InitializeComponent();

            scanView.DataContext = scanVm;
        }

        ScanView scanView = new();
        ScanViewModel scanVm = new();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            scanView.Show();
            scanView.Topmost = true;
            scanView.Topmost = false;
        }
    }
}

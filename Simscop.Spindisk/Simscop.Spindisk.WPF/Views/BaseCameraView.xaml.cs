using Simscop.Spindisk.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

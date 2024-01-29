using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CameraView.xaml
    /// </summary>
    public partial class CameraView : Window
    {
        public CameraView()
        {
            InitializeComponent();
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void ExposureTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double value;

            if (double.TryParse(ExposureTextBox.Text, out value))
            {
                if (value < 10.00)
                {
                    ExposureTextBox.Text = "10.00";
                }
                else if (value > 1000.00)
                {
                    ExposureTextBox.Text = "1000.00";
                }
            }
            else
            {
                ExposureTextBox.Text = "";
            }
        }
    }
}

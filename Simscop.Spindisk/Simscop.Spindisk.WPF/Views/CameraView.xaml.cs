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

            if (int.TryParse(ExposureTextBox.Text, out int value))
            {
                if (value <= 20)
                {
                    ExposureTextBox.Text = "20";
                }
                else if (value >= 1000)
                {
                    ExposureTextBox.Text = "1000";
                }
            }
            else
            {
                ExposureTextBox.Text = "";
            }
        }
    }
}

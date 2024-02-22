using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
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
using System.Windows.Shapes;

namespace Simscop.Spindisk.WPF.Views
{
    /// <summary>
    /// Interaction logic for ScanView.xaml
    /// </summary>
    public partial class ScanView : Window
    {
        public ScanView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTabItem = ScanTabControl.SelectedItem as TabItem;

            if (selectedTabItem != null)
            {
                if (selectedTabItem.Name == "XYScanItem")
                {
                    this.Height = 530;
                }
                else if (selectedTabItem.Name == "ZScanItem")
                {
                    this.Height = 340;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send<string>(SteerMessage.Setting);
        }
    }
}

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
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl? tabControl = sender as TabControl;
            if (tabControl != null && tabControl.SelectedItem != null)
            {
                TabItem? selectedTab = tabControl.SelectedItem as TabItem;
                if (selectedTab != null)
                {

                    if (selectedTab.Header.ToString() == "常规对焦")
                    {

                    }
                    else if (selectedTab.Header.ToString() == "自定义对焦")
                    {

                    }
                }
            }
        }

    }
}

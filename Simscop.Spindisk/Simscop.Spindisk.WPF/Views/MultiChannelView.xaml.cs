using System.ComponentModel;
using System.Windows;

namespace Simscop.Spindisk.WPF.Views
{
    /// <summary>
    /// MultiChannelSave.xaml 的交互逻辑
    /// </summary>
    public partial class MultiChannelView : Window
    {
        public MultiChannelView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}

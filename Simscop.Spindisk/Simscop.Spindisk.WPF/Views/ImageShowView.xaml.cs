using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.ViewModels;

namespace Simscop.Spindisk.WPF.Views
{
    /// <summary>
    /// ImageShowView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageShowView : Window
    {
        public ImageShowView()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<PopupWindowMessage>(this, ((s, m) => { this.Show(); }));
        }


    }
}

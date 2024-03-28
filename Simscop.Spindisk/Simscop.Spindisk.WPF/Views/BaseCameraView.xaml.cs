using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

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
            InitializeBlinkStoryboard();
            scanView.DataContext = scanVm;

            WeakReferenceMessenger.Default.Register<CameraCaptureStatusMessage>(this, ((s, m) => 
            {
                if (m.IsBlinking)
                {
                    _blinkStoryboard?.Begin();
                }
                else
                {
                    _blinkStoryboard?.Stop();
                }
            }));
        }

        private Storyboard _blinkStoryboard;
        ScanView scanView = new();
        ScanViewModel scanVm = new();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            scanView.Show();
            scanView.Topmost = true;
            scanView.Topmost = false;
        }

        private void InitializeBlinkStoryboard()
        {
            _blinkStoryboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                AutoReverse = true,
                Duration = TimeSpan.FromSeconds(0.2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(animation, dot);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            _blinkStoryboard.Children.Add(animation);
        }
    }
}

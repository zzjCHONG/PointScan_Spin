using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using System.Threading.Tasks;
using System.Windows;

namespace Simscop.Spindisk.Core.ViewModels
{

    public partial class ConnectStateViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isCameraConnected = false;

        [ObservableProperty]
        private bool _isSteerConnected = false;

        [ObservableProperty]
        private bool _isLaserConnected = false;

        [ObservableProperty]
        private bool _isSpinConnected = false;

        public ConnectStateViewModel()
        {
            //获取各连接返回信息
            WeakReferenceMessenger.Default.Register<CameraConnectMessage>(this, (o, m) =>
            {
                IsCameraConnected = m.IsConnected;
                CameraText(m.ConnectState);
            });
            WeakReferenceMessenger.Default.Register<LaserConnectMessage>(this, (o, m) =>
            {
                IsLaserConnected = m.IsConnected;
                LaserText(m.ConnectState);
            });
            WeakReferenceMessenger.Default.Register<SteerConnectMessage>(this, (o, m) =>
            {
                IsSteerConnected = m.IsConnected;
                SteerText(m.ConnectState);
            });
            WeakReferenceMessenger.Default.Register<SpinConnectMessage>(this, (o, m) =>
            {
                IsSpinConnected = m.IsConnected;
                SpinText(m.ConnectState);
            });

            //初始化各硬件
            Task.Run(Init);
        }

        [ObservableProperty]
        private string _cameraTextContext = "Connecting...";
        private string CameraText(string connectState)
        {
            return CameraTextContext = connectState;
        }

        [ObservableProperty]
        private string _laserTextContext = "Connecting...";
        private string LaserText(string connectState)
        {
            return LaserTextContext = connectState;
        }

        [ObservableProperty]
        private string _steerTextContext = "Connecting...";
        private string SteerText(string connectState)
        { 
            return SteerTextContext = connectState;
        }

        [ObservableProperty]
        private string _spinTextContext = "Connecting...";
        private string SpinText(string connectState)
        {
            return SpinTextContext = connectState;
        }

        void Init()
        {
            Task.Run(() =>
            {
                WeakReferenceMessenger.Default.Send<LaserInitMessage>(new LaserInitMessage());
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Send<SteerInitMessage>(new SteerInitMessage());
            });

            Task.Run(() =>
            {
                WeakReferenceMessenger.Default.Send<SpinInitMessage>(new SpinInitMessage());
            });

            Task.Run(() =>
            {
                WeakReferenceMessenger.Default.Send<CameraInitMessage>(new CameraInitMessage());
            });

        }
    }
}

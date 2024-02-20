using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Simscop.Spindisk.Core.ViewModels
{

    public partial class ConnectStateViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isCameraConnected = false;
        private bool IsCameraConnecting = false;

        [ObservableProperty]
        private bool _isSteerConnected = false;
        private bool IsSteerConnecting = false;

        [ObservableProperty]
        private bool _isLaserConnected = false;
        private bool IsLaserConnecting = false;

        [ObservableProperty]
        private bool _isSpinConnected = false;
        private bool IsSpinConnecting = false;

        public ConnectStateViewModel()
        {
            WeakReferenceMessenger.Default.Register<CameraConnectMessage>(this, (o, m) =>
            {
                IsCameraConnected = m.IsConnected;
                IsCameraConnecting = m.IsConnecting;
                CameraText(m.ConnectState);
            });
            WeakReferenceMessenger.Default.Register<LaserConnectMessage>(this, (o, m) =>
            {
                IsLaserConnected = m.IsConnected;
                IsLaserConnecting = m.IsConnecting;
                LaserText(m.ConnectState);
            });
            WeakReferenceMessenger.Default.Register<SteerConnectMessage>(this, (o, m) =>
            {
                IsSteerConnected = m.IsConnected;
                IsSteerConnecting = m.IsConnecting;
                SteerText(m.ConnectState);
            });
            WeakReferenceMessenger.Default.Register<SpinConnectMessage>(this, (o, m) =>
            {
                IsSpinConnected = m.IsConnected;
                IsSpinConnecting = m.IsConnecting;
                SpinText(m.ConnectState);
            });

            Task.Run(Init);
        }

        [ObservableProperty]
        private string _cameraTextContext = "Connecting...";
        private string CameraText(string connectState)
        {
            //return CameraTextContext= IsCameraConnected ? "连接完成！" : "初始化失败！";
            return CameraTextContext = connectState;
        }

        [ObservableProperty]
        private string _laserTextContext = "Connecting...";
        private string LaserText(string connectState)
        {
            //return LaserTextContext = IsLaserConnected ? "连接完成！" : "初始化失败！";
            return LaserTextContext = connectState;
        }

        [ObservableProperty]
        private string _steerTextContext = "Connecting...";
        private string SteerText(string connectState)
        {
            //return SteerTextContext = IsSteerConnected ? "连接完成！" : "初始化失败！";
            return SteerTextContext = connectState;
        }

        [ObservableProperty]
        private string _spinTextContext = "Connecting...";
        private string SpinText(string connectState)
        {
            //return SpinTextContext = IsSpinConnected ? "连接完成！" : "初始化失败！";
            return SpinTextContext = connectState;
        }

        void Init()
        {
            Task.Run(() =>
            {
                WeakReferenceMessenger.Default.Send<LaserInitMessage>(new LaserInitMessage(true));
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Send<SteerInitMessage>(new SteerInitMessage(true));
            });

            Task.Run(() =>
            {
                WeakReferenceMessenger.Default.Send<SpinInitMessage>(new SpinInitMessage(true));
            });

            Task.Run(() =>
            {
                WeakReferenceMessenger.Default.Send<CameraInitMessage>(new CameraInitMessage(true));
            });

        }
    }
}

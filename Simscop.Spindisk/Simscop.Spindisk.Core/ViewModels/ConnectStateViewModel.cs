using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;
using System.Diagnostics;
using System.Threading.Tasks;

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
                IsCameraConnected = m.isConnected;
                IsCameraConnecting = m.isConnecting;
                CameraText();
            });
            WeakReferenceMessenger.Default.Register<LaserConnectMessage>(this, (o, m) =>
            {
                IsLaserConnected = m.isConnected;
                IsLaserConnecting = m.isConnecting;
                LaserText();
            });
            WeakReferenceMessenger.Default.Register<SteerConnectMessage>(this, (o, m) =>
            {
                IsSteerConnected = m.isConnected;
                IsSteerConnecting = m.isConnecting;
                SteerText();
            });
            WeakReferenceMessenger.Default.Register<SpinConnectMessage>(this, (o, m) =>
            {
                IsSpinConnected = m.isConnected;
                IsSpinConnecting = m.isConnecting;
                SpinText();
            });

            Task.Run(Init);          
        }

        [ObservableProperty]
        private string _cameraTextContext = "连接中...";
        private string CameraText()
        {
           return CameraTextContext= IsCameraConnected ? "连接完成！" : "初始化失败！";
        }

        [ObservableProperty]
        private string _laserTextContext = "连接中...";
        private string LaserText()
        {
            return LaserTextContext = IsLaserConnected ? "连接完成！" : "初始化失败！";
        }

        [ObservableProperty]
        private string _steerTextContext = "连接中...";
        private string SteerText()
        {
            return SteerTextContext = IsSteerConnected ? "连接完成！" : "初始化失败！";
        }

        [ObservableProperty]
        private string _spinTextContext = "连接中...";
        private string SpinText()
        {
            return SpinTextContext = IsSpinConnected ? "连接完成！" : "初始化失败！";
        }

        void Init()
        {
            Task.Run(() => 
            {
                WeakReferenceMessenger.Default.Send<LaserInitMessage>(new LaserInitMessage(true));
            });

            Task.Run(() =>
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

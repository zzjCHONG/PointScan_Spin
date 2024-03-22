using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.API.Thorlabs;
using Simscop.Spindisk.Core.Messages;

namespace Simscop.Spindisk.Core.ViewModels;

public partial class SpinViewModel : ObservableObject
{
    private string ConnectState = string.Empty;

    private ELL6 eLL6Front;

    private ELL6 eLL6Back;

    public SpinViewModel()
    {
        WeakReferenceMessenger.Default.Register<SpindiskMessage, string>(this, nameof(SpindiskMessage), (r, m) => SetMode(m.Mode));

        WeakReferenceMessenger.Default.Register<SpinInitMessage>(this, (o, m) => Init());
    }

    [ObservableProperty]
    private bool _isConnected = false;
    [ObservableProperty]
    private bool _isConnecting = true;

    private void Init()
    {
        eLL6Front = new(true);
        eLL6Back = new(false);
        IsConnecting = true;
        bool eLL6FrontEnable = eLL6Front.Connect(out var msgFront);
        bool eLL6BackEnable = eLL6Back.Connect(out var msgBack);
        IsConnected = eLL6FrontEnable && eLL6BackEnable;
        ConnectState = $"滑轨1:{msgFront};滑轨2：{msgBack}";
        IsConnecting = false;

        if (IsConnected)
        {
            //设置并到达初始位置
            eLL6Front.Home(true);
            eLL6Back.Home(false);
        }
    }

    partial void OnIsConnectingChanged(bool value)
    {
        if (!value)
            WeakReferenceMessenger.Default.Send<SpinConnectMessage>(new SpinConnectMessage(IsConnected, value, ConnectState));
    }

    private void SetMode(int mode)
    {
        if (!IsConnected) return;

        switch (mode)
        {
            case 0:
                eLL6Front.ForWard();
                break;
            case 1:
                eLL6Front.BackWard();
                break;
            case 2:
                eLL6Back.ForWard();
                break;
            case 3:
                eLL6Back.BackWard();
                break;
            default:
                break;
        }
    }

    ~SpinViewModel()
    {
        eLL6Front.Disconnect();
        eLL6Back.Disconnect();
        eLL6Front = null;
        eLL6Back = null;
    }
}
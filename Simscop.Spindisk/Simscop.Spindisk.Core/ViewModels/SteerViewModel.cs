using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Simscop.Spindisk.Core.ViewModels;

/// <summary>
/// 舵机操作
/// </summary>
public partial class SteerViewModel : ObservableObject
{
    private readonly ASIMotor _motor = new();

    private readonly DispatcherTimer _timer;

    public SteerViewModel()
    {

        _timer = new DispatcherTimer(priority: DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(0.1),
        };
        Task.Run(() =>
        {
            IsConnected = _motor.InitializeMotor();
            _timer.Tick += _timer_Tick;
            if (IsConnected) _timer.Start();

        });

        WeakReferenceMessenger.Default.Register<string, string>(this, SteerMessage.MoveX, (s, e) =>
        {
            var value = double.Parse(e);
            _motor.SetXPosition(value);
        });

        WeakReferenceMessenger.Default.Register<string, string>(this, SteerMessage.MoveY, (s, e) =>
        {
            var value = double.Parse(e);
            _motor.SetYPosition(value);
        });

        WeakReferenceMessenger.Default.Register<string, string>(this, SteerMessage.MoveZ, (s, e) =>
        {
            var value = double.Parse(e);
            _motor.SetZPosition(value);
        });
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
        _motor.ReadPosition();

        X = _motor.X;
        Y = _motor.Y;
        Z = _motor.Z;

        //queue.Take();
        //XEnable = _motor is { XEnabled: true, XAction: false, XException: false };
        //YEnable = _motor is { YEnabled: true, YAction: false, YException: false };
        //ZEnable = _motor is { ZEnabled: true, ZAction: false, ZException: false };
    }

    [ObservableProperty]
    private double _x = 0;

    [ObservableProperty]
    private bool _xEnable = true;

    [ObservableProperty]
    private double _y = 0;

    [ObservableProperty]
    private bool _yEnable = true;

    [ObservableProperty]
    private double _z = 0;

    [ObservableProperty]
    private bool _zEnable = true;

    [ObservableProperty]
    private double _xyStep = 100;

    [ObservableProperty]
    private double _zStep = 10;

    [ObservableProperty]
    private bool _isConnected = false;

    [RelayCommand]
    void PositionToZero()
        => _motor.ResetPosition();

    public void MoveX(double step)
        => _motor.SetXOffset(step);

    public void MoveY(double step)
        => _motor.SetYOffset(step);

    public void MoveZ(double step)
        => _motor.SetZOffset(step);

    public void MoveToX(double value)
        => _motor.SetXPosition(value);

    public void MoveToY(double value)
        => _motor.SetYPosition(value);

    public void MoveToZ(double value)
        => _motor.SetZPosition(value);

    //private BlockingCollection<object> queue = new BlockingCollection<object>();

}
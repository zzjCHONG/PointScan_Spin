using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Simscop.Spindisk.Core.ViewModels;

/// <summary>
/// 舵机操作
/// </summary>
public partial class SteerViewModel : ObservableObject
{
    private readonly ASIMotor _motor;

    private readonly DispatcherTimer _timer;

    private ConcurrentQueue<Func<bool>> taskQueue;

    public SteerViewModel()
    {
        _motor = new();
        GlobalValue.GlobalMotor = _motor;

        _timer = new DispatcherTimer(priority: DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(0.1),
        };
        _timer.Tick += _timer_Tick;

        WeakReferenceMessenger.Default.Register<SteerInitMessage>(this, (o, m) => 
        { 
            if (m.isPreInit) SteerInit(); 
        });
    }

    partial void OnIsConnectingChanged(bool value)
    {
        if (!value)
            WeakReferenceMessenger.Default.Send<SteerConnectMessage>(new SteerConnectMessage(IsConnected, value));
    }

    private void SteerInit()
    {
        IsConnecting = true;
        IsConnected = _motor.InitializeMotor();   
        if (IsConnected) _timer.Start();
        IsConnecting = false;

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

        WeakReferenceMessenger.Default.Register<string>(SteerMessage.MotorReceive, (s, e) =>
        {
            WeakReferenceMessenger.Default.Send<ASIMotor, string>(_motor, SteerMessage.Motor);
        });

        _ = Queue();
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

    async Task Queue()
    {
        taskQueue = new ConcurrentQueue<Func<bool>>();

        await Task.Run(async () =>
        {
            while (true)
            {
                if (taskQueue.TryDequeue(out Func<bool> taskFunc))
                {
                    taskFunc.Invoke();
                }

                await Task.Delay(100);
            }
        });

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

    [ObservableProperty]
    private bool _isConnecting=true;

    [RelayCommand]
    void PositionToZero()
        => _motor.ResetPosition();

    [RelayCommand]
    void Focus()
    {
        Thread.Sleep(100);

        Task.Run(() =>
        {
            focus = AutoFocus.Create();
            focus.FirstCount = 5;
            focus.FirstStep = 10;
            focus.SeccondCount = 5;
            focus.SecondStep = 1;
            focus.Threshold = 0.02;
            focus.CropSize = 0.5;
            focus.Focus();
        });
    }

    void MicroFocus()
    {
        Thread.Sleep(100);

        Task.Run(() =>
        {
            focus = AutoFocus.Create();
            focus.FirstCount = 4;
            focus.FirstStep = 5;
            focus.SeccondCount = 5;
            focus.SecondStep = 1;
            focus.Threshold = 0.02;
            focus.CropSize = 0.5;
            focus.Focus();
        });
    }

    AutoFocus focus;

    public void MoveX(double step)
        => taskQueue.Enqueue(() => _motor.SetXOffset(step));

    public void MoveY(double step)
        => taskQueue.Enqueue(() => _motor.SetYOffset(step));

    public void MoveZ(double step)
        => taskQueue.Enqueue(() => _motor.SetZOffset(step));

    public void MoveToX(double value)
        => _motor.SetXPosition(value);

    public void MoveToY(double value)
        => _motor.SetYPosition(value);

    public void MoveToZ(double value)
        => _motor.SetZPosition(value);


    //private BlockingCollection<object> queue = new BlockingCollection<object>();

}
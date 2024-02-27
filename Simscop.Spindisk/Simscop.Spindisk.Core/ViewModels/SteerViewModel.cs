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

    SteerViewModel steerVM;

    public SteerViewModel()
    {
        _motor = new();

        GlobalValue.GlobalMotor = _motor;

        _timer = new DispatcherTimer(priority: DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(0.1),
        };

        WeakReferenceMessenger.Default.Register<SteerInitMessage>(this, (o, m) =>
        {
            if (m.IsPreInit) SteerInit();
        });
    }

    [ObservableProperty]
    private bool _isConnecting = true;

    partial void OnIsConnectingChanged(bool value)
    {
        if (!value)
            WeakReferenceMessenger.Default.Send<SteerConnectMessage>(new SteerConnectMessage(IsConnected, value, ConnectState));
    }

    private string ConnectState = string.Empty;

    private void SteerInit()
    {
        IsConnecting = true;
        IsConnected = _motor.InitializeMotor(out ConnectState);
        _timer.Tick += _timer_Tick;
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

        
        WeakReferenceMessenger.Default.Send<ASIMotor, string>(_motor, SteerMessage.Splice);

        WeakReferenceMessenger.Default.Register<string>(SteerMessage.Setting, (s, e) =>
        {
            CustomSeccondCount = GlobalValue.CustomFocus.SeccondCount;
            SetCustomFocus();
        });

        ReSetFocus();

        ReSetCustomFocus();

        _ = Queue();
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
        _motor.ReadPosition();
        GlobalValue.GlobalMotor = _motor;
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
    private bool _isUnFocusing = true;
    
    #region 自动对焦参数
    [ObservableProperty]
    private int _firstCount = 5;

    [ObservableProperty]
    private int _firstStep = 5;

    [ObservableProperty]
    private int _seccondCount = 5;

    [ObservableProperty]
    private int _secondStep = 1;

    [ObservableProperty]
    private double _cropSize = 0.5;

    [ObservableProperty]
    private double _threshold = 0.02;

    [ObservableProperty]
    private int _customFirstCount = 0;

    [ObservableProperty]
    private int _customFirstStep = 0;

    [ObservableProperty]
    private int _customSeccondCount = 5;

    [ObservableProperty]
    private int _customSecondStep = 1;

    [ObservableProperty]
    private double _customCropSize = 0.5;

    [ObservableProperty]
    private double _customThreshold = 0.02;

    #endregion

    [RelayCommand]
    void SetFocus()
    {
        GlobalValue.GeneralFocus = AutoFocus.Create();
        GlobalValue.GeneralFocus.FirstCount = FirstCount;
        GlobalValue.GeneralFocus.FirstStep = FirstStep;
        GlobalValue.GeneralFocus.SeccondCount = SeccondCount;
        GlobalValue.GeneralFocus.SecondStep = SecondStep;
        GlobalValue.GeneralFocus.Threshold = Threshold;
        GlobalValue.GeneralFocus.CropSize = CropSize;
    }

    [RelayCommand]
    void ReSetFocus()
    {
        GlobalValue.GeneralFocus = AutoFocus.Create();
        FirstCount = 5;
        FirstStep = 5;
        SeccondCount = 5;
        SecondStep = 1;
        Threshold = 0.02;
        CropSize = 0.5;
        GlobalValue.GeneralFocus.FirstCount = FirstCount;
        GlobalValue.GeneralFocus.FirstStep = FirstStep;
        GlobalValue.GeneralFocus.SeccondCount = SeccondCount;
        GlobalValue.GeneralFocus.SecondStep = SecondStep;
        GlobalValue.GeneralFocus.Threshold = Threshold;
        GlobalValue.GeneralFocus.CropSize = CropSize;
    }

    [RelayCommand]
    void SetCustomFocus()
    {
        GlobalValue.CustomFocus = AutoFocus.Create();
        GlobalValue.CustomFocus.FirstCount = CustomFirstCount;
        GlobalValue.CustomFocus.FirstStep = CustomFirstStep;
        GlobalValue.CustomFocus.SeccondCount = CustomSeccondCount;
        GlobalValue.CustomFocus.SecondStep = CustomSecondStep;
        GlobalValue.CustomFocus.Threshold = CustomThreshold;
        GlobalValue.CustomFocus.CropSize = CustomCropSize;
    }

    [RelayCommand]
    void ReSetCustomFocus()
    {
        GlobalValue.CustomFocus = AutoFocus.Create();
        CustomFirstCount = 0;
        CustomFirstStep = 0;
        CustomSeccondCount = 5;
        CustomSecondStep = 1;
        CustomThreshold = 0.02;
        CustomCropSize = 0.5;
        GlobalValue.CustomFocus.FirstCount = CustomFirstCount;
        GlobalValue.CustomFocus.FirstStep = CustomFirstStep;
        GlobalValue.CustomFocus.SeccondCount = CustomSeccondCount;
        GlobalValue.CustomFocus.SecondStep = CustomSecondStep;
        GlobalValue.CustomFocus.Threshold = CustomThreshold;
        GlobalValue.CustomFocus.CropSize = CustomCropSize;
    }

    [RelayCommand]
    void PositionToZero()
        => _motor.ResetPosition();

    [RelayCommand]
    async Task Focus()
    {
        Thread.Sleep(100);
        IsUnFocusing = false;

        await Task.Run(() =>
        {
            GlobalValue.GeneralFocus.Focus();
        });

        IsUnFocusing = true;
    }

    void CustomFocus()
    {
        Thread.Sleep(100);

        Task.Run(() =>
        {
            GlobalValue.CustomFocus.Focus();
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
    ~SteerViewModel()
    {
        _motor.UnInitializeMotor();
    }

}
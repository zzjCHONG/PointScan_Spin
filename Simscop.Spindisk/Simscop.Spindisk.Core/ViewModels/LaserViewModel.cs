﻿using System;
using System.Diagnostics;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.API;
using Simscop.API.TopLaser;
using Simscop.Spindisk.Core.Messages;

namespace Simscop.Spindisk.Core.ViewModels;

/**
 * 蓝色，红色，绿色，紫色
 */

public class TestLaser : ILaser
{
    public bool Init()
    {
        Debug.WriteLine($"LASER: -> INIT");
        return true;
    }

    public bool SetStatus(int count, bool status)
    {
        Debug.WriteLine($"LASER: -> SetStatus:{count} - {status}");
        return true;
    }

    public bool GetStatus(int count, out bool status)
    {
        status = false;
        Debug.WriteLine($"LASER: -> GetStatus:{count} - {status}");
        return true;
    }

    public bool SetPower(int count, int value)
    {
        Debug.WriteLine($"LASER: -> SetPower:{count} - {value}");
        return true;
    }

    public bool GetPower(int count, out int value)
    {
        value = 10;
        Debug.WriteLine($"LASER: -> GetPower:{count} - {value}");
        return true;
    }

    public string GetConnectState()
    {
        Debug.WriteLine($"LASER:-> TestLaser.GetConnectState");

        return "TestLaser init completed!";
    }

    public bool DisConnect()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 激光通道操作
/// </summary>
public partial class LaserViewModel : ObservableObject
{
    public ILaser? Laser { get; set; }

    public LaserViewModel()
    {
        // todo 这里初始化laser并且准备好Laser本身的数据
        //Laser = new TopticaLaser();
        //Laser = new BogaoLaser();
        Laser = new TestLaser();
        GlobalValue.GlobalLaser = Laser;

        WeakReferenceMessenger.Default.Register<LaserInitMessage>(this, (o, m) => LaserInit());
    }

    [ObservableProperty]
    private bool _isConnected = false;

    [ObservableProperty]
    private bool _isConnecting = true;

    partial void OnIsConnectingChanged(bool value)
    {
        if (!value)
            WeakReferenceMessenger.Default.Send<LaserConnectMessage>(new LaserConnectMessage(IsConnected, value, Laser.GetConnectState()));
    }

    void LaserInit()
    {
        IsConnecting = true;
        IsConnected = Laser.Init();
        IsConnecting = false;
        if (IsConnected)
        {
            if (Laser.GetStatus(0, out var aStatus) && Laser.GetStatus(1, out var bStatus)
                && Laser.GetStatus(2, out var cStatus) && Laser.GetStatus(3, out var dStatus))
            {
                ChannelAEnable = aStatus;
                ChannelBEnable = bStatus;
                ChannelCEnable = cStatus;
                ChannelDEnable = dStatus;
            }
            else throw new Exception("Laser get status error.");
            Thread.Sleep(100);
            if (Laser.GetPower(0, out var aPower) && Laser.GetPower(1, out var bPower) &&
                Laser.GetPower(2, out var cPower) && Laser.GetPower(3, out var dPower))
            {
                ChannelAValue = aPower;
                ChannelBValue = bPower;
                ChannelCValue = cPower;
                ChannelDValue = dPower;
            }
            else throw new Exception("Laser get power error.");
        }
    }

    [ObservableProperty]
    private int _channelAValue = 25;

    partial void OnChannelAValueChanged(int value)
        => DialogRun(() => Laser.SetPower(0, value));

    [ObservableProperty]
    private bool _channelAEnable = false;

    partial void OnChannelAEnableChanged(bool value)
    {
        DialogRun(() => Laser.SetStatus(0, value));
        WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(0, value), nameof(LaserMessage));
        if (!value) return;

        WeakReferenceMessenger.Default.Send<SpindiskMessage, string>(new SpindiskMessage(0), nameof(SpindiskMessage));

        WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(0));

        ChannelBEnable = false;
        ChannelCEnable = false;
        ChannelDEnable = false;

        WeakReferenceMessenger.Default.Send<CurrentDispalyChannelEnableMessage>(new CurrentDispalyChannelEnableMessage(ChannelAEnable, ChannelBEnable, ChannelCEnable, ChannelDEnable));

    }

    [ObservableProperty]
    private int _channelBValue = 89;

    partial void OnChannelBValueChanged(int value)
        => DialogRun(() => Laser.SetPower(1, value));

    [ObservableProperty]
    private bool _channelBEnable = false;

    partial void OnChannelBEnableChanged(bool value)
    {
        DialogRun(() => Laser.SetStatus(1, value));
        WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(1, value), nameof(LaserMessage));
        if (!value) return;

        WeakReferenceMessenger.Default.Send<SpindiskMessage, string>(new SpindiskMessage(1), nameof(SpindiskMessage));

        WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(1));

        ChannelAEnable = false;
        ChannelCEnable = false;
        ChannelDEnable = false;

        WeakReferenceMessenger.Default.Send<CurrentDispalyChannelEnableMessage>(new CurrentDispalyChannelEnableMessage(ChannelAEnable, ChannelBEnable, ChannelCEnable, ChannelDEnable));
    }

    [ObservableProperty]
    private int _channelCValue = 53;

    partial void OnChannelCValueChanged(int value)
        => DialogRun(() => Laser.SetPower(2, value));

    [ObservableProperty]
    private bool _channelCEnable = false;

    partial void OnChannelCEnableChanged(bool value)
    {
        DialogRun(() => Laser.SetStatus(2, value));
        WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(2, value), nameof(LaserMessage));

        if (!value) return;
        WeakReferenceMessenger.Default.Send<SpindiskMessage, string>(new SpindiskMessage(2), nameof(SpindiskMessage));
        WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(2));

        ChannelAEnable = false;
        ChannelBEnable = false;
        ChannelDEnable = false;

        WeakReferenceMessenger.Default.Send<CurrentDispalyChannelEnableMessage>(new CurrentDispalyChannelEnableMessage(ChannelAEnable, ChannelBEnable, ChannelCEnable, ChannelDEnable));

    }

    [ObservableProperty]
    private int _channelDValue = 9;

    partial void OnChannelDValueChanged(int value)
        => DialogRun(() => Laser.SetPower(3, value));

    [ObservableProperty]
    private bool _channelDEnable = false;

    partial void OnChannelDEnableChanged(bool value)
    {
        DialogRun(() => Laser.SetStatus(3, value));
        WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(3, value), nameof(LaserMessage));

        if (!value) return;
        WeakReferenceMessenger.Default.Send<SpindiskMessage, string>(new SpindiskMessage(3), nameof(SpindiskMessage));
        WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(3));

        ChannelAEnable = false;
        ChannelBEnable = false;
        ChannelCEnable = false;

        WeakReferenceMessenger.Default.Send<CurrentDispalyChannelEnableMessage>(new CurrentDispalyChannelEnableMessage(ChannelAEnable, ChannelBEnable, ChannelCEnable, ChannelDEnable));
    }

    void DialogRun(Func<bool> func)
    {
        if (!func.Invoke())
        {
            //MessageBox.Show("Error laser command");
        }
    }

    ~LaserViewModel()
    {
        Laser?.DisConnect();
        Laser = null;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;

namespace Simscop.Spindisk.Core.ViewModels;

/**
 * Note 之后可以添加功能：
 *  自动识别串口并选择
 */

/// <summary>
/// 转盘操作
/// </summary>
public partial class SpinViewModel : ObservableObject
{
    private string ConnectState=string.Empty;

    public SpinViewModel()
    {
        ComList = Simscop.API.Helper.SerialHelper.GetAllCom();
        WeakReferenceMessenger.Default.Register<SpindiskMessage, string>(this, nameof(SpindiskMessage), (r, m) =>SetMode(m.Mode));
        WeakReferenceMessenger.Default.Register<SpinInitMessage>(this, (o, m) =>
        {
            if (ComList == null || !ComList.Contains(ComName))
            {
                ConnectState = $"The serial port {ComName} is not found";
                //MessageBox.Show(ConnectState);
                WeakReferenceMessenger.Default.Send<SpinConnectMessage>(new SpinConnectMessage(false, false, ConnectState));
                return;
            }
            if (m.IsPreInit) ConnectCom();
        });
    }

    [ObservableProperty]
    private bool _spinViewEnabled = true;

    private void DelaySpinViewEnabled(int value)
    {
        Task.Run(() =>
        {
            SpinViewEnabled = false;
            Task.Delay(value * 1000).Wait();
            SpinViewEnabled = true;
        });
    }

    partial void OnIsConnectedChanged(bool value)
    {
        if (value)
            ConnectState = "Initialize spin completed!";
        WeakReferenceMessenger.Default.Send<SpinConnectMessage>(new SpinConnectMessage(value, IsConnecting, ConnectState));
    }

    partial void OnIsConnectingChanged(bool value)
    {   
        if (!IsConnected)
        {
            ConnectState = $"The serial port {ComName} is disconnected or in use.";
            WeakReferenceMessenger.Default.Send<SpinConnectMessage>(new SpinConnectMessage(IsConnected, value, ConnectState));
        }         
    }

    [ObservableProperty]
    private bool _spinControlEnabled = false;

    public List<string> ComList { get; set; }

    [ObservableProperty]
    private string _comName = "COM6";

    [ObservableProperty]
    private bool _isConnected = false;

    [ObservableProperty]
    private bool _isConnectEnable = true;

    [ObservableProperty]
    private bool _isConnecting = true;

    [RelayCommand]
     async void ConnectCom()
    {
        IsConnectEnable = false;
        SpinControlEnabled = false;
        try
        {
            if (!IsConnected)
            {
                IsConnecting= true;
                IsConnected = await XLight.Connect(ComName);
                IsConnecting = false;
               
                if (IsConnected)
                {                
                    XLight.LoadAllFlag();
                    SpiningIndex = XLight.FlagD;
                    DichroicIndex = XLight.FlagC - 1;
                    EmissionIndex = XLight.FlagB - 1;
                    ExcitationIndex = XLight.FlagA - 1;
                    DiskEnable = XLight.FlagN == 1;

                    SpinControlEnabled = true;
                }

                IsConnectEnable = true;
            }
            else
            {
                XLight.Disconnect();
                IsConnected = false;
                SpinControlEnabled = false;
            }
        }
        catch (Exception e)
        {
            IsConnectEnable = true;
            IsConnected = false;
            SpinControlEnabled = false;
            MessageBox.Show("旋转台：接口出现错误，连接失败");
        }
        finally
        {
            IsConnectEnable = true;
        }

    }

    [RelayCommand]
    void Reset()
    {
        XLight.Reset();
        DelaySpinViewEnabled(SpiningIndex == 1 ? 6 : 3);

        SpiningIndex = 0;
        DiskEnable = false;
        DichroicIndex = 0;
        EmissionIndex = 0;
        ExcitationIndex = 0;
    }

    [ObservableProperty]
    private bool _diskEnable = false;

    [RelayCommand]
    void SetDisk()
    {
        // TODO 这里之后要修改成为从串口获取实时数据来抉择是否完成后面的任务
        try
        {
            if (XLight.FlagD == 0) SpiningIndex = 1;
            XLight.SetDisk(DiskEnable ? (uint)1 : (uint)0);
            DelaySpinViewEnabled(2);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            DiskEnable = false;
        }
    }

    #region Spining

    public List<string> SpiningCollection => new List<string>()
    {
        "宽场","转盘"
    };

    [ObservableProperty]
    private uint _spiningIndex = 0;

    partial void OnSpiningIndexChanged(uint value)
    {
        if (XLight.FlagD == value) return;

        XLight.SetSpining(value);
        DelaySpinViewEnabled(4);

    }

    #endregion

    #region Dichroic

    public List<string> DichroicCollection => new List<string>()
    {
        "DM488","DM532","DM561","DM640","DM405"
    };

    [ObservableProperty]
    private uint _dichroicIndex = 0;

    partial void OnDichroicIndexChanged(uint value) => XLight.SetDichroic(value + 1);

    #endregion

    #region Emission

    public List<string> EmissionCollection => new List<string>()
    {
        "DAPI","BFP","FITC","TRITC","CY5","Custom1","Custom2","Custom3"
    };

    [ObservableProperty]
    private uint _emissionIndex = 0;

    partial void OnEmissionIndexChanged(uint value) => XLight.SetEmission(value + 1);

    #endregion

    #region Excitation

    public List<string> ExcitationCollection => new List<string>()
    {
        "405nm","445nm","488nm","532nm","561nm","640nm","Custom1","Custom2"
    };

    [ObservableProperty]
    private uint _excitationIndex = 0;

    partial void OnExcitationIndexChanged(uint value) => XLight.SetExcitation(value + 1);

    #endregion

    void SetMode(int mode)
    {
        if (!IsConnected) return;

        switch (mode)
        {
            case 0:
                DichroicIndex = 4;
                EmissionIndex = 0;
                ExcitationIndex = 0;
                break;
            case 1:
                DichroicIndex = 0;
                EmissionIndex = 2;
                ExcitationIndex = 2;
                break;
            case 2:
                DichroicIndex = 1;
                EmissionIndex = 3;
                ExcitationIndex = 3;
                break;
            case 3:
                DichroicIndex = 3;
                EmissionIndex = 4;
                ExcitationIndex = 5;
                break;
            default:
                break;

        }
    }

    //void SetMode(int mode)
    //{
    //    if (!IsConnected)return;
        
    //    switch (mode)
    //    {
    //        case 0:
    //            DichroicIndex = 4;
    //            EmissionIndex = 0;
    //            ExcitationIndex = 0;
    //            break;
    //        case 1:
    //            DichroicIndex = 2;
    //            EmissionIndex = 2;
    //            ExcitationIndex = 2;
    //            break;
    //        case 2:
    //            DichroicIndex = 1;
    //            EmissionIndex = 5;
    //            ExcitationIndex = 3;
    //            break;
    //        case 3:
    //            DichroicIndex = 2;
    //            EmissionIndex = 4;
    //            ExcitationIndex = 5;
    //            break;
    //        default:
    //            break;

    //    }
    //}
}
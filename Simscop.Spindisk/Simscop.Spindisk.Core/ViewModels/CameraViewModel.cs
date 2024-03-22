using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;
using Simscop.Spindisk.Core.Models.NIDevice;
using Simscop.Spindisk.Core.NICamera;

namespace Simscop.Spindisk.Core.ViewModels;

public partial class CameraViewModel : ObservableObject
{
    ICamera camera { get; set; }

    Config Config { get; set; }

    public List<string> ResolutionsRatio { get; set; } = new()
    {
        "50*50",
        "100*100",
        "200*200",
        "300*300",
    };

    public List<string> WaveSelection { get; set; } = new()
    {
        "锯齿波",
        "三角波",
    };

    [ObservableProperty]
    private int _resolutionsRatioSelectIndex = 3;
    [ObservableProperty]
    private int _waveSelectionSelectIndex = 0;
    [ObservableProperty]
    private List<string> _devicesList =new();
    [ObservableProperty]
    private int _deviceSelectIndex = 0;
    [ObservableProperty]
    private int _voltageSweepRangeUpperLimit = 0;
    [ObservableProperty]
    private int _voltageSweepRangeLowerLimit = 0;
    [ObservableProperty]
    private int _pixelDwelTime = 0;
    partial void OnResolutionsRatioSelectIndexChanged(int value)
    {
        int X = 0;
        int Y = 0;
        int index = ResolutionsRatioSelectIndex;
        switch (index)
        {
            case 0:
                X = 50 ;
                Y = 50 ;
                break;
            case 1:
                X = 100;
                Y = 100 ;
                break;
            case 2:
                X= 200;
                Y= 200 ;
                break;
            case 3:
                X = 300;
                Y = 300;
                break;
        }
        Config.Write(ConfigSettingEnum.XPixelFactor, X);
        Config.Write(ConfigSettingEnum.YPixelFactor, Y);
    }
    partial void OnWaveSelectionSelectIndexChanged(int value)
    {
        Config.Write(ConfigSettingEnum.WaveMode, value);//0为锯齿波，1为三角波
    }
    partial void OnDeviceSelectIndexChanged(int value)
    {
        string deviceName = DevicesList[value];
        Config.Write(ConfigSettingEnum.DeviceName, deviceName);
    }
    partial void OnDevicesListChanged(List<string> value)
    {
        Config.Write(ConfigSettingEnum.DeviceName, DevicesList[DeviceSelectIndex]);
    }
    partial void OnVoltageSweepRangeUpperLimitChanged(int value)
    {
        Config.Write(ConfigSettingEnum.maxXV, value);
        Config.Write(ConfigSettingEnum.maxYV, value);
    }
    partial void OnVoltageSweepRangeLowerLimitChanged(int value)
    {
        Config.Write(ConfigSettingEnum.minXV, value);
        Config.Write(ConfigSettingEnum.minYV, value);
    }
    partial void OnPixelDwelTimeChanged(int value)
    {
        Config.Write(ConfigSettingEnum.PixelDwelTime, value);
    }

    public CameraViewModel()
    {
        Config = new();
        camera = new NICam();
        //camera = new TestCamera();
        //Camera = new Andor();
        GlobalValue.GlobalCamera = camera;

        WeakReferenceMessenger.Default.Register<SaveFrameModel, string>(this, MessageManage.SaveCurrentCapture, (s, e) =>
        throw new Exception("当前方法不准使用了"));

        WeakReferenceMessenger.Default.Register<string, string>(this, MessageManage.SaveACapture, (s, e) =>
        {
            if (IsCapture)
                GlobalValue.CurrentFrame?.SaveImage(e);
        });

        WeakReferenceMessenger.Default.Register<CameraInitMessage>(this, (o, m) => CameraInit());
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotInit))]
    private bool _isInit = false;
    public bool IsNotInit => !IsInit;

    [ObservableProperty]
    private bool _isCapture = false;

    [ObservableProperty]
    private bool _isCaptureOpposite = false;

    [ObservableProperty]
    private bool _isConnecting = true;

    [ObservableProperty]
    private bool _isNoBusy = true;

    [ObservableProperty]
    private bool _isStartAcquisition = false;

    partial void OnIsCaptureChanged(bool value)
    {
        IsCaptureOpposite=!value;
        WeakReferenceMessenger.Default.Send<CameraControlEnableMessage>(new CameraControlEnableMessage(value));
    }

    partial void OnIsConnectingChanged(bool value)
    {
        if (!value)
            WeakReferenceMessenger.Default.Send<CameraConnectMessage>(new CameraConnectMessage(IsInit, value, camera.GetConnectState()));
    }

    partial void OnIsStartAcquisitionChanged(bool value)//显示
    {
        if (IsStartAcquisition)
        {
            Task.Run(() =>
            {
                while (IsStartAcquisition)
                {
                    try
                    {
                        if (camera.Capture(out var mat))
                        {
                            GlobalValue.CurrentFrame = mat.Clone();
                            WeakReferenceMessenger.Default.Send<DisplayFrame, string>(new DisplayFrame()
                            {
                                Image = mat,
                            }, "Display");
                        }
                    }
                    catch (Exception)
                    { }
                }
            });
        }
    }

    [RelayCommand]
    void BtnInit()//按键
    {
        IsNoBusy = false;
        if (!IsInit)
        {
            Task.Run(() =>
            {
                if (CameraInit())
                {
                    IsCapture = camera.StartCapture();
                    IsStartAcquisition = true;
                }
                IsNoBusy = true;
            });
        }
        else if (IsInit && !IsCapture)
        {
            IsCapture = camera.StartCapture();
            IsStartAcquisition = true;
            IsNoBusy = true;
        }
        else if (IsCapture)
        {
            IsCapture = !camera.StopCapture();
            IsStartAcquisition = false;
            IsNoBusy = true;
        }
        else { }
    }

    bool CameraInit()
    {
        IsConnecting = true;
        IsInit = camera.Init(out List<string> devices);

        IsConnecting = false;
        if (IsInit)
        {
            DevicesList = devices;
            WaveSelectionSelectIndex = Config.WaveMode;
            VoltageSweepRangeLowerLimit =(int)Config.MinXV;
            VoltageSweepRangeUpperLimit =(int)Config.MaxXV;
            PixelDwelTime=(int)Config.PixelDwelTime;
            switch (Config.XPixelFactor)
            {
                case 50:
                    ResolutionsRatioSelectIndex = 0;
                    break;
                case 100:
                    ResolutionsRatioSelectIndex = 1;
                    break;
                case 200:
                    ResolutionsRatioSelectIndex = 2;
                    break;
                case 300:
                    ResolutionsRatioSelectIndex = 3;
                    break;
                    default:
                    break;
            }

            //camera.GetExposure(out var exposure);
            //Exposure = Math.Floor(exposure * 1000.0);
            //camera.GetFrameRate(out var frameRate);
            //FrameRate = frameRate;
            return true;
        }
        else
        {
            IsNoBusy = false;//初始化不成功
            return false;
        }
    }

    #region Old-Andor

    public List<string> ImageModes { get; set; } = new()
    {
        "NONE"
    };

    public List<string> RoiModeLite { get; set; } = new()
    {
        "NONE"
    };

    [ObservableProperty]
    private double _exposure = 0;//[10,30000]，ms

    [ObservableProperty]
    private double _frameRate = 0;//随曝光&采样率实时变化

    [ObservableProperty]
    private bool isExposureSettingEnable=true;

    [RelayCommand]
    void SetExposure()
    {
        IsExposureSettingEnable = false;
        camera.SetExposure(Exposure);
        camera.GetFrameRate(out var rate);
        FrameRate = rate;
        IsExposureSettingEnable = true;
    }

    #endregion

    #region File

    [ObservableProperty]
    private string _root = "";

    [ObservableProperty]
    private string _fileName = "";

    [ObservableProperty]
    private bool _timeSuffix = true;

    [RelayCommand]
    void QuickSaveFile()
    {
        var name = $"{FileName}" +
                   $"{(!string.IsNullOrEmpty(FileName) && TimeSuffix ? "_" : "")}" +
                   $"{(TimeSuffix ? $"{DateTime.Now:yyyyMMdd_HH_mm_ss}" : "")}";

        var dlg = new SaveFileDialog()
        {
            Title = "存储图片",
            FileName = name,
            Filter = "TIF|*.tif",
            DefaultExt = ".tif",
        };
     
        if (dlg.ShowDialog() == DialogResult.OK)
            WeakReferenceMessenger.Default
                .Send<string, string>(dlg.FileName, MessageManage.SaveACapture);
    }

    [RelayCommand]
    void LoadSaveDirectory()
    {
        var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() == DialogResult.OK)
            Root = dialog.SelectedPath;
    }

    [RelayCommand]
    void SaveFile()
    {
        var name = $"{FileName}" +
                   $"{(!string.IsNullOrEmpty(FileName) && TimeSuffix ? "_" : "")}" +
                   $"{(TimeSuffix ? $"{DateTime.Now:yyyyMMdd_HH_mm_ss}" : "")}";

        WeakReferenceMessenger.Default
            .Send<string, string>(Path.Join(Root, name), MessageManage.SaveACapture);
    }

    #endregion
}

public class TestCamera : ICamera
{
    private readonly Mat[] _imgs;

    public int Count { get; set; }

    public int Total { get; set; }

    public TestCamera()
    {
        //Simscop.Spindisk.Core.Models.NIDevice.TestExample testExample = new();
        //testExample.Test();

        var paths = new List<string>()
        {
            @"C:\\Users\\simscop\\Desktop\\3-11\\00\\1.tif",
            //"C:/Users/DELL/Desktop/Y_-15800_X_-1800.TIF",
            //@"C:\\Users\\Administrator\\Pictures\\Camera Roll\\1.jpg"
        };

        Total = paths.Count;

        _imgs = paths
            .Select(path => Cv2.ImRead(path, ImreadModes.AnyDepth)).ToArray();

        //var count = 0;
        //while (true)
        //{
        //    Thread.Sleep(300); 

        //    WeakReferenceMessenger.Default.Send<DisplayFrame, string>(new DisplayFrame()
        //    {
        //        Image = imgs[(count++) % 4]
        //    }, "Display");
        //}
    }

    public bool Init()
    {
        Debug.WriteLine($"Camera-> TestCamera.Init");
        Thread.Sleep(1);
        return true;
    }

    public bool StartCapture()
    {
        Debug.WriteLine($"Camera-> TestCamera.StartCapture");
        return true;
    }

    public bool Capture(out Mat mat)
    {
        mat = _imgs[Count++ % Total];

        return true;
    }

    public bool SaveCapture(string path)
    {
        Debug.WriteLine($"Camera-> TestCamera.SaveCapture {path}");
        return true;
    }

    public bool GetExposure(out double exposure)
    {
        Debug.WriteLine($"Camera-> TestCamera.GetExposure");

        exposure = 0.1;
        return true;
    }

    public bool SetExposure(double exposure)
    {
        Debug.WriteLine($"Camera-> TestCamera.SetExposure");

        exposure = 100;
        return true;
    }

    public bool StopCapture()
    {
        Debug.WriteLine($"Camera-> TestCamera.StopCapture");
        return true;
    }

    public bool GetFrameRate(out double frameRate)
    {
        Debug.WriteLine($"-> TestCamera.GetFrameRate");

        frameRate = 100;
        return true;
    }

    public string GetConnectState()
    {
        Debug.WriteLine($"Camera-> TestCamera.GetConnectState");

        return "TestCamera init completed!";
    }

    public bool AcqStartCommand()
    {
        return true;
    }

    public bool AcqStopCommand()
    {
        return true;
    }

    public bool Init(out List<string> devices)
    {
        devices = new List<string>();
        Debug.WriteLine($"Camera-> TestCamera.Init");
        Thread.Sleep(1);
        return true;
    }
}
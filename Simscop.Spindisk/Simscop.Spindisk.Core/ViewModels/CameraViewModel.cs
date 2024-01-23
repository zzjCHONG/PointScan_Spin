using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;

namespace Simscop.Spindisk.Core.ViewModels;

public class TestCamera : ICamera
{
    private Mat[] _imgs;

    public int Count { get; set; }

    public int Total { get; set; }

    public TestCamera()
    {
        var paths = new List<string>()
        {
            @"E:\.test\BPAE405.tif",
            @"E:\.test\BPAE488.tif",
            @"E:\.test\BPAE525.tif",

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
        Debug.WriteLine($"-> TestCamera.Init");
        Thread.Sleep(1);
        return true;
    }

    public bool StartCapture()
    {
        Debug.WriteLine($"-> TestCamera.StartCapture");
        return true;
    }

    public bool Capture(out Mat mat)
    {
        mat = _imgs[Count++ % Total];

        return true;
    }

    public bool SaveCapture(string path)
    {
        Debug.WriteLine($"-> TestCamera.SaveCapture {path}");
        return true;
    }

    public bool GetExposure(out double exposure)
    {
        Debug.WriteLine($"-> TestCamera.GetExposure");

        exposure = 100;
        return true;
    }

    public bool SetExposure(double exposure)
    {
        Debug.WriteLine($"-> TestCamera.SetExposure");

        exposure = 100;
        return true;
    }

    public bool StopCapture()
    {
        Debug.WriteLine($"-> TestCamera.StopCapture");
        return true;
    }
}

public partial class CameraViewModel : ObservableObject
{
    public ICamera Camera { get; set; }

    public List<string> ResolutionsLite { get; set; } = new()
    {
        "2560 * 2160"
    };

    public List<string> ImageModes { get; set; } = new()
    {
        "NONE"
    };

    public List<string> RoiModeLite { get; set; } = new()
    {
        "NONE"
    };

    private DispatcherTimer _timer;

    public Mat? CurrentFrame { get; set; }
    public CameraViewModel()
    {
        //Camera = new TestCamera();
        Camera = new TestCamera();

        //var value = TimeSpan.FromSeconds(0.1);//Exposure / 2 / 1000
        //_timer = new DispatcherTimer(DispatcherPriority.Render)
        //{
        //    Interval = value,
        //};
        //_timer.Tick += (s, e) =>
        //{
        //    Task.Run(() =>
        //    {
        //        if (Camera.Capture(out var mat))
        //        {
        //            var img = new Mat();
        //            mat.MinMaxIdx(out var min, out var max);
        //            (((mat - min) / (max - min)) * 256).ToMat().ConvertTo(img, MatType.CV_8UC1);

        //            WeakReferenceMessenger.Default.Send<DisplayFrame, string>(new DisplayFrame()
        //            {
        //                Image = mat,
        //            }, "Display");
        //        }
        //    });
        //};

        WeakReferenceMessenger.Default.Register<SaveFrameModel, string>(this, MessageManage.SaveCurrentCapture,
            (s, e) => throw new Exception("当前方法不准使用了"));

        WeakReferenceMessenger.Default.Register<string, string>(this, MessageManage.SaveACapture, (s, e) =>
        {
            //if (!IsInit || !IsCapture) return;
            if (IsCapture)
            {
                CurrentFrame?.SaveImage(e);
            }

        });
    }

    [ObservableProperty]
    private bool _isCapture = false;

    [ObservableProperty]
    private double _exposure = 500;//0.01

    partial void OnExposureChanged(double value)
    {
        //var isTick = _timer.IsEnabled;
        //_timer.Stop();
        //_timer.Interval = TimeSpan.FromSeconds(value / 2 / 1000);

        //if (isTick) _timer.Start();

        Camera.SetExposure(value);

    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotInit))]
    private bool _isInit = false;

    public bool IsNotInit => !IsInit;

    [ObservableProperty]
    private bool _isNoBusy = true;

    [ObservableProperty]
    private bool _isStartAcquisition = false;

    partial void OnIsStartAcquisitionChanged(bool value)
    {
        if (IsStartAcquisition)
        {
            Task.Run(() =>
            {
                while (IsStartAcquisition)
                {
                    try
                    {
                        if (!Camera.Capture(out var mat)) continue;

                        CurrentFrame = mat.Clone();

                        WeakReferenceMessenger.Default.Send<DisplayFrame, string>(new DisplayFrame()
                        {
                            Image = mat,
                        }, "Display");

                        Thread.Sleep(1000);
                    }
                    catch (Exception) { }
                }

            });
        }
    }

    [RelayCommand]
    void Init()
    {
        IsNoBusy = false;

        if (!IsInit)
        {
            Task.Run(() =>
            {
                IsInit = Camera.Init();
                IsCapture = Camera.StartCapture();

                IsStartAcquisition = true;
                //_timer.Start();
                IsNoBusy = true;
            });
        }
        else if (IsInit && !IsCapture)
        {
            IsCapture = Camera.StartCapture();
            //_timer.Interval = TimeSpan.FromSeconds(Exposure / 2 / 1000);
            //_timer.Start();
            IsStartAcquisition = true;

            IsNoBusy = true;
        }
        else if (IsCapture)
        {
            IsCapture = !Camera.StopCapture();
            //_timer.Stop();
            IsStartAcquisition = false;
            IsNoBusy = true;
        }
        else { }
    }

    [RelayCommand]
    void SetExposure() => Camera.SetExposure(Exposure);

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
// https://blog.walterlv.com/post/wpf-high-performance-bitmap-rendering.html

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lift.Core.ImageArray.Extensions;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Simscop.API;
using Simscop.Lib.ImageExtension;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;

namespace Simscop.Spindisk.Core.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    #region 不再修改

    [ObservableProperty]
    private BitmapFrame? _imageFirst;

    [ObservableProperty]
    private BitmapFrame? _imageSecond;

    [ObservableProperty]
    private BitmapFrame? _imageThird;

    [ObservableProperty]
    private BitmapFrame? _imageFourth;

    [ObservableProperty] private Mat? _matFirst;
    [ObservableProperty] private Mat? _matSecond;
    [ObservableProperty] private Mat? _matThird;
    [ObservableProperty] private Mat? _matFourth;

    [ObservableProperty]
    private bool _enableFirst = false;

    [ObservableProperty]
    private bool _enableSecond = false;

    [ObservableProperty]
    private bool _enableThird = false;

    [ObservableProperty]
    private bool _enableFourth = false;

    partial void OnEnableFirstChanged(bool value) => SwitchEnable(value, 1);
    partial void OnEnableSecondChanged(bool value) => SwitchEnable(value, 2);
    partial void OnEnableThirdChanged(bool value) => SwitchEnable(value, 3);
    partial void OnEnableFourthChanged(bool value) => SwitchEnable(value, 4);

    void SwitchEnable(bool value, int index)
    {
        if (!value) return;

        EnableFirst = index == 1;
        EnableSecond = index == 2;
        EnableThird = index == 3;
        EnableFourth = index == 4;

        Debug.WriteLine($"{EnableFirst} - {EnableSecond} - {EnableThird} - {EnableFourth}");
    }

    private Mat? _currentFrame = null;

    private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);

    public List<string> Colors { get; set; } = new()
    {
        "Gray",
        "Green",
        "Red",
        "Blue",
        "Purple",
        "Autumn",
        "Bone",
        "Jet",
        "Winter",
        "Rainbow",
        "Ocean",
        "Summer",
        "Spring",
        "Cool",
        "Hsv",
        "Pink",
        "Hot",
        "Parula",
        "Magma",
        "Inferno",
        "Plasma",
        "Viridis",
        "Cividis",
        "Twilight",
    };

    Mat ConverterImage(Mat mat, int mode)
        => mode switch
        {
            0 => ToGray(mat),
            1 => ToGreen(mat),
            2 => ToRed(mat),
            3 => ToBlue(mat),
            4 => ToPurple(mat),
            5 => Apply(mat, ColormapTypes.Autumn),
            6 => Apply(mat, ColormapTypes.Bone),
            7 => Apply(mat, ColormapTypes.Jet),
            8 => Apply(mat, ColormapTypes.Winter),
            9 => Apply(mat, ColormapTypes.Rainbow),
            10 => Apply(mat, ColormapTypes.Ocean),
            11 => Apply(mat, ColormapTypes.Summer),
            12 => Apply(mat, ColormapTypes.Spring),
            13 => Apply(mat, ColormapTypes.Cool),
            14 => Apply(mat, ColormapTypes.Hsv),
            15 => Apply(mat, ColormapTypes.Pink),
            16 => Apply(mat, ColormapTypes.Hot),
            17 => Apply(mat, ColormapTypes.Parula),
            18 => Apply(mat, ColormapTypes.Magma),
            19 => Apply(mat, ColormapTypes.Inferno),
            20 => Apply(mat, ColormapTypes.Plasma),
            21 => Apply(mat, ColormapTypes.Viridis),
            22 => Apply(mat, ColormapTypes.Cividis),
            23 => Apply(mat, ColormapTypes.Twilight),
            24 => Apply(mat, ColormapTypes.TwilightShifted),

            _ => throw new Exception()
        };

    Mat ConverterFormat(Mat mat)
    {
        if (mat.Type() == MatType.CV_8UC1)
            return mat;

        if (mat.Type() == MatType.CV_8UC3)
        {
            var dst = new Mat();
            Cv2.CvtColor(mat, dst, ColorConversionCodes.RGB2GRAY);
            return mat;
        }
        else
            return mat.ToU8();
    }

    Mat Apply(Mat mat, ColormapTypes type)
    {
        mat = ConverterFormat(mat);
        var dst = new Mat();
        Cv2.ApplyColorMap(mat, dst, type);
        return dst;
    }

    Mat ToGray(Mat mat)
    {
        return mat;
    }

    Mat ToGreen(Mat mat)
    {
        mat = ConverterFormat(mat);
        var dst = new Mat();
        Cv2.ApplyColorMap(mat, dst, ColorMaps.Green);
        return dst;
    }

    Mat ToRed(Mat mat)
    {
        mat = ConverterFormat(mat);
        var dst = new Mat();
        Cv2.ApplyColorMap(mat, dst, ColorMaps.Red);
        return dst;
    }

    Mat ToBlue(Mat mat)
    {
        mat = ConverterFormat(mat);
        var dst = new Mat();
        Cv2.ApplyColorMap(mat, dst, ColorMaps.Blue);
        return dst;
    }

    Mat ToPurple(Mat mat)
    {
        mat = ConverterFormat(mat);
        var dst = new Mat();
        Cv2.ApplyColorMap(mat, dst, ColorMaps.Pruple);
        return dst;
    }

    [ObservableProperty] private int _colorFirst = 0;
    [ObservableProperty] private int _colorSecond = 0;
    [ObservableProperty] private int _colorThird = 0;
    [ObservableProperty] private int _colorFourth = 0;

    partial void OnColorFirstChanged(int value)
    {
        if (MatFirst is null) return;
        ImageFirst = BitmapFrame.Create(ConverterImage(MatFirst, ColorFirst).ToBitmapSource());
    }

    partial void OnColorSecondChanged(int value)
    {
        if (MatSecond is null) return;
        ImageSecond = BitmapFrame.Create(ConverterImage(MatSecond, ColorSecond).ToBitmapSource());
    }

    partial void OnColorThirdChanged(int value)
    {
        if (MatThird is null) return;
        ImageThird = BitmapFrame.Create(ConverterImage(MatThird, ColorThird).ToBitmapSource());
    }

    partial void OnColorFourthChanged(int value)
    {
        if (MatFourth is null) return;
        ImageFourth = BitmapFrame.Create(ConverterImage(MatFourth, ColorFourth).ToBitmapSource());
    }

    #endregion

    public ShellViewModel()
    {
        WeakReferenceMessenger.Default.Register<LaserMessage, string>(this, nameof(LaserMessage), (s, m) =>
        {
            switch (m.Index)
            {
                case 0:
                    EnableFirst = m.Status;
                    break;
                case 1:
                    EnableSecond = m.Status;
                    break;
                case 2:
                    EnableThird = m.Status;
                    break;
                case 3:
                    EnableFourth = m.Status;
                    break;
            }

            Debug.WriteLine($"ShellViewModel {m.Index} - {m.Status}");
        });


        WeakReferenceMessenger.Default.Register<DisplayFrame, string>(this, "Display", (s, m) =>
        {

            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (m.Image is not { } img) return;

                _currentFrame= img.Clone();

                //img = img.Normalize();


                if (EnableFirst)
                {
                    MatFirst = m.Image;
                    ImageFirst = BitmapFrame.Create(ConverterImage(img, ColorFirst).ToBitmapSource());
                }

                if (EnableSecond)
                {
                    MatSecond = m.Image;
                    ImageSecond = BitmapFrame.Create(ConverterImage(img, ColorSecond).ToBitmapSource());
                }

                if (EnableThird)
                {
                    MatThird = m.Image;
                    ImageThird = BitmapFrame.Create(ConverterImage(img, ColorThird).ToBitmapSource());
                }

                if (EnableFourth)
                {
                    MatFourth = m.Image;
                    ImageFourth = BitmapFrame.Create(ConverterImage(img, ColorFourth).ToBitmapSource());
                }

            });

            
        });

        //WeakReferenceMessenger.Default.Register<SaveFrameModel, string>(this, MessageManage.SaveCurrentCapture,
        //    (s, e) =>
        //    {
        //        if (_currentFrame == null) return;
        //        e.Dump();

        //        var mat = _currentFrame.Clone();
        //        foreach (var path in e.Paths)
        //        {
        //            mat.SaveImage(path);
        //        }
        //    });

        //WeakReferenceMessenger.Default.Register<string, string>(this, MessageManage.SaveACapture, (s, e) =>
        //{
        //    if (_currentFrame == null) return;
        //    var mat = _currentFrame.Clone();
        //    mat.SaveImage(e);
        //});

        //InitCamera();
    }


    private Andor _andor;

    void InitCamera()
    {
        _andor = new Andor();

        _andor.InitializeSdk();
        _andor.InitializeCamera(0);


        _andor.StartCapture();

        Task.Run(() =>
        {
            while (true)
            {
                var mat = _andor.GetCurrentFrame();

                WeakReferenceMessenger.Default.Send<DisplayFrame, string>(new DisplayFrame()
                {
                    Image = mat
                }, "Display");

            }

            _andor.StopCapture();
            _andor.UnInitializeCamera();
            _andor.UninitializeSdk();
        });


    }

}


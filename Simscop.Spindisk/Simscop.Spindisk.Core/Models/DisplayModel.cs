using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Simscop.Lib.ImageExtension;

namespace Simscop.Spindisk.Core.Models;


/// <summary>
/// 图像显示过程中的抽象对象
///
/// gamma 先处理，其他后处理
/// </summary>
public partial class DisplayModel : ObservableObject
{
    [ObservableProperty]
    private BitmapFrame? _frame;

    [ObservableProperty]
    private Mat _original = new();

    /// <summary>
    /// gamma等处理对象
    ///
    /// 承载是否归一化和最大最小处理
    /// </summary>
    [ObservableProperty]
    private Mat _u8 = new();

    /// <summary>
    /// 这个结果是所有处理后才有的
    /// </summary>
    [ObservableProperty]
    private Mat _display = new();

    [ObservableProperty]
    private double _contrast = 1;

    [ObservableProperty]
    private int _brightness = 0;

    [ObservableProperty]
    private double _gamma = 1;

    [ObservableProperty]
    private bool _norm = true;

    [ObservableProperty]
    private int _min = 0;

    [ObservableProperty]
    private int _max = 255;

    /// <summary>
    /// 实际最小值
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ContrastThreshold))]
    private int _threshold = 255;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int ContrastThreshold => Threshold / 2;

    [ObservableProperty]
    private int _colorMode = 0;

    void Init()
    {
        if (Norm)
        {
            Min = 0;
            Max = Threshold;
            Threshold = 255;

            U8 = Original.To8U(1);
        }
        else
        {
            ResetU8();
        }
    }

    void ResetU8()
    {
        if (!ValidMat(Original)) return;

        var depth = Original.Depth();
        switch (depth)
        {
            case 0:
                Threshold = 255;
                break;
            case 2:
                Threshold = 65535;
                break;
            default:
                Norm = true;
                return;
        }

        var temp = new Mat();

        //Original.MinMaxLoc(out double minval, out double maxval);
        //if (Min > minval) Min = (int)minval;
        //if (Max < maxval) Max = (int)maxval;


        U8 = (((Original.RangeIn(Min, Max) - Min) / (Max - Min)) * 255).ToMat().To8U();
    }

    partial void OnOriginalChanged(Mat value) => Init();

    partial void OnNormChanged(bool value) => Init();

    // U8改变之后要生成Display
    partial void OnU8Changed(Mat value)
        => Display = U8.Gamma(Gamma).Adjust(Contrast, Brightness);

    partial void OnDisplayChanged(Mat value)
        => TempFrameDo();

    partial void OnColorModeChanged(int value)
        => TempFrameDo();

    partial void OnMinChanged(int value)
    {
        if (Norm) return;
        ResetU8();
    }

    partial void OnMaxChanged(int value)
    {
        if (Norm) return;
        ResetU8();
    }

    partial void OnContrastChanged(double value)
        => TempDisplayDo();

    partial void OnBrightnessChanged(int value)
        => TempDisplayDo();

    partial void OnGammaChanged(double value)
        => TempDisplayDo();

    bool ValidMat(Mat mat)
        => mat.Cols != 0 || mat.Rows != 0;

    void TempDisplayDo()
    {
        if (ValidMat(Display))
            Display = U8.Gamma(Gamma).Adjust(Contrast, Brightness);
    }

    void TempFrameDo()
    {
        if (ValidMat(Display))
            Frame = BitmapFrame.Create(Display.ApplyColor(ColorMode).ToBitmapSource());
    }

    [RelayCommand]
    void SetAsDefault()
    {
        Norm = true;
        Contrast = 1;
        Brightness = 0;
        Gamma = 1;
        ColorMode = 0;
    }

}
public static class MatExtension
{
    /// <summary>
    /// 转换任意xC1数据类型成为64FC1
    /// </summary>c
    /// <param name="mat"></param>
    /// <returns></returns>
    public static Mat? To64F(this Mat mat)
    {
        if (mat.Channels() != 1) return null;
        var temp = new Mat(mat.Rows, mat.Cols, MatType.CV_64FC1);
        mat.ConvertTo(temp, MatType.CV_64FC1, 1, 0);
        return temp;
    }

    /// <summary>
    /// 范围外值为0
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Mat RangeIn(this Mat mat, double min, double max)
    {
        var mask = new Mat();
        Cv2.InRange(mat, new Scalar(min, min, min), new Scalar(max, max, max), mask);

        var result = new Mat();
        Cv2.BitwiseAnd(mat, mat, result, mask);

        return result;
    }

    /// <summary>
    /// 原始数据
    /// 这里处理也是只针对单通道的数据
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="mode">
    /// 转换方式：
    /// 0 - 不做额外处理，使用标准的转换
    /// 1 - 最大最小归一化
    /// 2 - 等比例放缩
    /// </param>
    /// <returns></returns>
    public static Mat To8U(this Mat mat, int mode = 0)
    {
        var u8 = new Mat();

        if (mat.Channels() != 1) return u8;
        if (mat.Depth() == 0) return mat;

        var depth = mat.Depth();

        switch (mode)
        {
            // 直接转换
            case 0:
                mat.ConvertTo(u8, MatType.CV_8UC1, 1, 0);
                return u8;
            case 1:
                var mat64 = mat.To64F();
                mat64!.MinMaxLoc(out var min, out double max);
                (((mat64 - min) / (max - min)) * 255)
                    .ToMat()
                    .ConvertTo(u8, MatType.CV_8UC1);
                return u8;
                break;

            case 2:
                break;
            default:
                break;
        }

        return u8;
    }

    /// <summary>
    /// 将一系列的8UC1数据格式计算平均值
    /// </summary>
    /// <param name="mats"></param>
    /// <returns></returns>
    public static Mat? Average(this List<Mat> mats)
    {
        if (!mats.All(item => item.Depth() == 0 && item.Channels() == 1))
            return null;

        if (mats.Count < 2)
            return mats.Count == 1 ? mats[0] : null;

        var average = new Mat(mats[0].Rows, mats[0].Cols, MatType.CV_64FC1, new Scalar(0));
        average = mats.Aggregate(average, (current, mat) => (Mat)(current + mat.To64F()!));

        average.GetArray(out double[] data);

        average /= mats.Count;

        var u8 = new Mat();
        average.ConvertTo(u8, MatType.CV_8UC1);
        return u8;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static Mat Max(this Mat mat, Mat match)
    {
        var temp = new Mat();
        Cv2.Max(mat, match, temp);
        return temp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mats"></param>
    /// <returns></returns>
    public static Mat? Max(this List<Mat> mats)
    {
        if (!mats.All(item => item.Depth() == 0 && item.Channels() == 1))
            return null;

        if (mats.Count < 2)
            return mats.Count == 1 ? mats[0] : null;

        var max = new Mat(mats[0].Rows, mats[0].Cols, MatType.CV_8UC1, new Scalar(0));
        max = mats.Aggregate(max, (current, mat) => (Mat)(current.Max(mat)));
        return max;
    }

    /// <summary>
    /// 伪彩设置
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Mat Apply(this Mat mat, Mat color)
    {
        var res = new Mat();
        Cv2.ApplyColorMap(mat, res, color);
        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Mat Apply(this Mat mat, ColormapTypes type)
    {
        var dst = new Mat();
        Cv2.ApplyColorMap(mat, dst, type);
        return dst;
    }


    /// <summary>
    /// 调整图像的亮度和对比度
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="contrast"></param>
    /// <param name="brightness"></param>
    /// <returns></returns>
    public static Mat Adjust(this Mat mat, double contrast, int brightness)
    {
        if (Math.Abs(contrast - 1) < 0.00001 && brightness == 0) return mat;
        var result = new Mat();
        mat.ConvertTo(result, -1, contrast, brightness);
        return result;
    }

    /// <summary>
    /// 设置图像的gamma值
    /// </summary>
    /// <param name="img"></param>
    /// <param name="gamma"></param>
    /// <returns></returns>
    public static Mat Gamma(this Mat img, double gamma)
    {
        if (img.Type() != MatType.CV_8UC1) return img;

        if (Math.Abs(gamma - 1) < 0.00001) return img;

        var lut = new Mat(1, 256, MatType.CV_8U);
        for (var i = 0; i < 256; i++)
            lut.Set<byte>(0, i, (byte)(Math.Pow(i / 255.0, gamma) * 255.0));

        var output = new Mat();
        Cv2.LUT(img, lut, output);

        return output;
    }

    /// <summary>
    /// 多个三通道图片合并
    /// </summary>
    /// <param name="mats"></param>
    /// <returns></returns>
    public static Mat? MergeChannel3(this List<Mat> mats)
    {
        if (mats.Any(item => item.Channels() != 3))
            return null;

        var ch1 = new List<Mat>();
        var ch2 = new List<Mat>();
        var ch3 = new List<Mat>();

        foreach (var channels in mats.Select(mat => mat.Split()))
        {
            ch1.Add(channels[0]);
            ch2.Add(channels[1]);
            ch3.Add(channels[2]);
        }

        var averCh1 = ch1.Max();
        var averCh2 = ch2.Max();
        var averCh3 = ch3.Max();

        var res = new Mat();
        Cv2.Merge(new Mat[] { averCh1!, averCh2!, averCh3! }, res);

        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static Mat ApplyColor(this Mat mat, int mode)
        => mode switch
        {
            0 => mat,
            1 => mat.Apply(ColorMaps.Green),
            2 => mat.Apply(ColorMaps.Red),
            3 => mat.Apply(ColorMaps.Blue),
            4 => mat.Apply(ColorMaps.Pruple),
            5 => mat.Apply(ColormapTypes.Autumn),
            6 => mat.Apply(ColormapTypes.Bone),
            7 => mat.Apply(ColormapTypes.Jet),
            8 => mat.Apply(ColormapTypes.Winter),
            9 => mat.Apply(ColormapTypes.Rainbow),
            10 => mat.Apply(ColormapTypes.Ocean),
            11 => mat.Apply(ColormapTypes.Summer),
            12 => mat.Apply(ColormapTypes.Spring),
            13 => mat.Apply(ColormapTypes.Cool),
            14 => mat.Apply(ColormapTypes.Hsv),
            15 => mat.Apply(ColormapTypes.Pink),
            16 => mat.Apply(ColormapTypes.Hot),
            17 => mat.Apply(ColormapTypes.Parula),
            18 => mat.Apply(ColormapTypes.Magma),
            19 => mat.Apply(ColormapTypes.Inferno),
            20 => mat.Apply(ColormapTypes.Plasma),
            21 => mat.Apply(ColormapTypes.Viridis),
            22 => mat.Apply(ColormapTypes.Cividis),
            23 => mat.Apply(ColormapTypes.Twilight),
            24 => mat.Apply(ColormapTypes.TwilightShifted),
            _ => throw new Exception()
        };

    public static List<string> Colors { get; set; } = new()
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
}

//public static class ColorMaps
//{
//    private static Mat? _gray = null;
//    public static Mat Gray
//    {
//        get
//        {
//            if (_gray != null) return _gray!;

//            _gray = new Mat(256, 1, MatType.CV_8UC3);
//            for (var i = 0; i < 256; i++)
//                _gray.Set(i, 0, new Vec3b((byte)i, (byte)i, (byte)i));
//            return _gray!;
//        }
//    }

//    private static Mat? _green = null;
//    public static Mat Green
//    {
//        get
//        {
//            if (_green != null) return _green!;

//            _green = new Mat(256, 1, MatType.CV_8UC3);
//            for (var i = 0; i < 256; i++)
//                _green.Set(i, 0, new Vec3b((byte)0, (byte)i, (byte)0));
//            return _green!;
//        }
//    }



//    private static Mat? _red = null;
//    public static Mat Red
//    {
//        get
//        {
//            if (_red != null) return _red!;

//            _red = new Mat(256, 1, MatType.CV_8UC3);
//            for (var i = 0; i < 256; i++)
//                _red.Set(i, 0, new Vec3b((byte)0, (byte)0, (byte)i));
//            return _red!;
//        }
//    }

//    private static Mat? _blue = null;
//    public static Mat Blue
//    {
//        get
//        {
//            if (_blue != null) return _blue!;

//            _blue = new Mat(256, 1, MatType.CV_8UC3);
//            for (var i = 0; i < 256; i++)
//                _blue.Set(i, 0, new Vec3b((byte)i, (byte)0, (byte)0));
//            return _blue!;
//        }
//    }


//    private static Mat? _purple = null;
//    public static Mat Pruple
//    {
//        get
//        {
//            if (_purple == null)
//            {
//                _purple = new Mat(256, 1, MatType.CV_8UC3);
//                for (var i = 0; i < 256; i++)
//                    _purple.Set(i, 0, new Vec3b((byte)i, (byte)0, (byte)i));

//            }
//            return _purple!;
//        }
//    }
//}
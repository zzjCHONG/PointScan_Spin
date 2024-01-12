using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Models;
using Simscop.API;

using DhyanaObject = Simscop.API.Dhyana;
using Simscop.API.Native;
using System.Windows.Threading;

namespace Simscop.Spindisk.Core.ViewModels;

/**
 * NOTE
 * 
 * SDk Initialize Pass
 * Camera Connect Pass
 * Capture Start and Stop Pass
 * Exposure max/min/set/get
 *
 * DispatcherTimer DispatcherPriority
 *
 * BUG
 * 
 * 1. ROI设置有问题
 * 2. 曝光设置有问题
 *
 *
 * FIX
 * ROI 和 曝光问题已经解决，原因在于对API的set和get命令的时间反应不同
 * 
 * 
 */


/**
 * 常量参数 TODO
 * 
 * 曝光的最大，最小和步进 完成
 * 将Capture拆开
 * SetBrightness这个参数不太确作用
 *
 * 
 */

public partial class CameraViewModel : ObservableObject
{

    #region 引用的ViewModel

    [ObservableProperty]
    private SaveFrameViewModel _saveFrameVm = new SaveFrameViewModel();

    #endregion


    private const double DefaultFrameInterval = 200;

    private const double PeriodSurplus = 10;

    private const int AutoExposureInterval = 50;

    [ObservableProperty]
    private double _globalTimerPeriod = DefaultFrameInterval + PeriodSurplus;

    partial void OnGlobalTimerPeriodChanged(double value)
    {
        if (!(value > 10)) return;

        _frameTimer.Interval = TimeSpan.FromMilliseconds(value + PeriodSurplus);
        //_levelTimer.Interval = TimeSpan.FromMilliseconds(value + PeriodSurplus);
    }

    private static DispatcherPriority FrameTimerPriority
        => DispatcherPriority.Render;

    /// <summary>
    /// 获取frame的定时器
    /// </summary>
    private readonly DispatcherTimer _frameTimer = new(priority: FrameTimerPriority);

    /// <summary>
    /// 色阶使用自动初始化器，但是因为显示问题，他的所有值更改应该依据Frame更改来驱动
    /// </summary>
    private readonly DispatcherTimer _levelTimer = new(priority: FrameTimerPriority)
    {
        Interval = TimeSpan.FromMilliseconds(1000)
    };

    public CameraViewModel()
    {
        _frameTimer.Tick += (s, m) =>
        {
            Task.Run(() =>
            {
                var display = new DisplayFrame();

                if (DhyanaObject.GetCurrentFrame((int)(GlobalTimerPeriod))
                        && Frame2Bytes(ref display, DhyanaObject.CurrentFrame))

                    WeakReferenceMessenger.Default.Send<DisplayFrame, string>(display, "Display");

            });
        };

        _levelTimer.Tick += (s, m) =>
        {
            Task.Run(() =>
            {
                if (!IsAutoLeftLevel && !IsAutoRightLevel) _levelTimer.Stop();

                if (IsAutoLeftLevel)
                {
                    DhyanaObject.GetLeftLevels(out var lv);
                    LeftLevel = lv;
                }

                if (IsAutoRightLevel)
                {
                    DhyanaObject.GetRightLevels(out var rv);
                    RightLevel = rv;
                }
            });
        };


        RefreshValue();
    }

    ~CameraViewModel()
    {
        DhyanaObject.UnInitializeCamera();
        DhyanaObject.UninitializeSdk();
    }

    /// <summary>
    /// 初始化赋值参数和获取参数值
    /// 曝光，最大最小
    /// 
    /// </summary>
    void RefreshValue()
    {
        RefreshExposure();
        RefreshRoi();

    }

    void RefreshExposure()
    {
        // Expousure
        DhyanaObject.GetExposureAttr(out var attr);

        ExposureModel.MinExposure = attr.dbValMin;
        ExposureModel.MaxExposure = attr.dbValMax;
        ExposureModel.DefaultExposure = attr.dbValDft;
        ExposureModel.StepExposure = attr.dbValStep;
    }

    private void RefreshRoi()
    {
        TUCAM_ROI_ATTR attr = default;
        DhyanaObject.GetRoi(ref attr);

        RoiModel.HOffset = attr.nHOffset;
        RoiModel.VOffset = attr.nVOffset;
        RoiModel.Width = attr.nWidth;
        RoiModel.Height = attr.nHeight;

        RoiEnabled = attr.bEnable;
    }

    /// <summary>
    /// 自动设置相机属性
    /// 这个功能只在相机连接之后完成
    /// </summary>
    void AutoLoadOnCameraConnected()
    {
        Exposure = 100;
    }

    void AutoLoadOnCapture()
    {
        DhyanaObject.SetHistc(true);
        DhyanaObject.SetAutolevels();

        if (DhyanaObject.GetGamma(out var gamma)) Gamma = gamma;
        if (DhyanaObject.GetContrast(out var contrast)) Contrast = contrast;

        IsAutoExposure = true;
    }

    public DhyanaInfoModel DhyanaInfo { get; set; } = new();

    /// <summary>
    /// 更新某些参数暂停Capture
    /// </summary>
    /// <param name="action"></param>
    private void SetValueWithCapture(Action action)
    {
        Task.Run(() =>
        {
            if (IsCapture)
            {
                StopCapture();
                action();
                StartCapture();
            }
            else action();
        });
    }

    #region Enabled

    /// <summary>
    /// 除Camer以外，控制他们所有的相机的Connected
    /// 用来标定实际结果
    /// </summary>
    [ObservableProperty]
    private bool _cameraConnected = false;

    /// <summary>
    /// 只控制相机按钮
    /// </summary>
    [ObservableProperty]
    private bool _cameraConnecting = true;

    /// <summary>
    /// 仅仅代表Camera这个按钮的情况
    /// </summary>
    [ObservableProperty]
    private bool _cameraFlag = false;

    #endregion

    [RelayCommand]
    void OpenCameraAndCapture()
    {
        switch (CameraConnected)
        {
            case false:
                Task.Run(() =>
                {
                    CameraFlag = true;
                    GenerateConnectCameraTask().Wait();
                    IsCapture = true;
                    CaptureFrame();
                });
                break;
            case true when !IsCapture:
                IsCapture = true;
                CaptureFrame();
                break;
            case true when IsCapture:
                IsCapture = false;
                CaptureFrame();
                break;
            default:
                break;
        }
    }

    #region Camera

    //TODO 这里之后要记得写一个控件，在True和False之间切换会有图像切换

    internal Task GenerateConnectCameraTask()
        => Task.Run(() =>
        {
            CameraConnecting = false;

            if (CameraFlag)
            {

                if (!DhyanaObject.InitializeSdk())
                {
                    MessageBox.Show("初始化SDK出错");
                    throw new NotSupportedException();
                }

                CameraConnected = DhyanaObject.InitializeCamera(0);
                CameraFlag = CameraConnected;

                if (!CameraConnected) MessageBox.Show("相机链接失败");
                else AutoLoadOnCameraConnected();
            }

            else
            {
                if (IsCapture)
                {
                    IsCapture = false;
                    StopCapture();
                }


                DhyanaObject.UnInitializeCamera();
                DhyanaObject.UninitializeSdk();

                CameraConnected = false;
            }

            CameraConnecting = true;
        });

    [RelayCommand]
    void ConnectCamera() => GenerateConnectCameraTask();




    #endregion

    #region Capture

    [ObservableProperty]
    private bool _isCapture = false;


    [RelayCommand]
    void CaptureFrame()
    {
        if (IsCapture) StartCapture();
        else StopCapture();
    }

    /// <summary>
    /// 开始捕获屏幕
    /// </summary>
    private void StartCapture()
    {
        try
        {
            RefreshExposure();
            DhyanaObject.StartCapture();

            _frameTimer.Start();

            _levelTimer.Start();

            AutoLoadOnCapture();
        }
        catch (Exception e)
        {
            //MessageBox.Show(e.ToString());
            throw new Exception("", e);

            IsCapture = false;
        }
    }

    /// <summary>
    /// 停止捕获屏幕
    /// </summary>
    private void StopCapture()
    {
        try
        {
            _levelTimer.Stop();
            _frameTimer.Stop();
            DhyanaObject.StopCapture();
        }
        catch (Exception e)
        {
            //MessageBox.Show(e.ToString());
            throw new Exception("", e);
            IsCapture = false;
        }

    }

    /// <summary>
    /// 转换软件里面的Frame格式，这里之后可以用来自定义处理数据，然后存储图片和视频也要在这里做一个简单的修订
    /// NOTE 这里的数据结构依据Capture那里，格式为RAW格式
    /// </summary>
    /// <param name="display"></param>
    /// <param name="frame"></param>
    /// <returns></returns>
    private bool Frame2Bytes(ref DisplayFrame display, TUCAM_FRAME frame)
    {
        try
        {
            if (frame.pBuffer == IntPtr.Zero) return false;

            int width = frame.usWidth;
            int height = frame.usHeight;
            int stride = (int)frame.uiWidthStep;

            var size = (int)(frame.uiImgSize + frame.usHeader);
            var raw = new byte[size];
            var actualRaw = new byte[frame.uiImgSize];

            // 要位移
            Marshal.Copy(frame.pBuffer, raw, 0, size);

            Buffer.BlockCopy(raw, frame.usHeader, actualRaw, 0, (int)frame.uiImgSize);


            display.Height = height;
            display.Width = width;
            display.Stride = stride;
            display.FrameObject = actualRaw;

            display.Depth = frame.ucDepth;
            display.Channels = frame.ucChannels;

        }
        catch (Exception e)
        {
            //MessageBox.Show(e.Message);
            throw new Exception("", e);

            return false;
        }
        return true;
    }

    #endregion

    #region Resolution

    public List<string> Resolutions => Dhyana.Resolutions;

    public List<string> ResolutionsLite => new List<string>()
    {
        "2048", "2048(E)", "1024", "512"
    };

    [ObservableProperty]
    private int _resolutionIndex = 0;

    partial void OnResolutionIndexChanged(int value)
    {
        SetValueWithCapture(() => DhyanaObject.SetResolution(value));

        RefreshExposure();
    }

    #endregion

    #region Exposure

    /// <summary>
    /// 公用Exposure
    /// </summary>
    [ObservableProperty]
    private double _exposure = 0;

    partial void OnExposureChanged(double value)
    {
        GlobalTimerPeriod = value;

        if (IsAutoExposure) return;

        ExposureModel.Exposure = value;
        DhyanaObject.SetExposure(value);

    }

    /// <summary>
    /// 输入框里面的Exposure
    /// </summary>
    [ObservableProperty]
    private ExposureModel _exposureModel = new();

    [RelayCommand]
    void SetExposure() => Exposure = ExposureModel.Exposure;

    [ObservableProperty]
    private bool _isAutoExposure = false;

    partial void OnIsAutoExposureChanged(bool value)
    {
        DhyanaObject.SetAutoExposure(value);

        Task.Run(() =>
        {
            while (IsAutoExposure && IsCapture)
            {
                double val = -1;
                DhyanaObject.GetExposure(ref val);
                GlobalTimerPeriod = val;

                Exposure = val;
                ExposureModel.Exposure = val;

                Task.Delay(AutoExposureInterval);
            }
        });
    }

    // TODO 这个方法肯定得重写的
    [RelayCommand]
    async Task SetOnceExposure()
    {
        await Task.Run(() =>
        {
            DhyanaObject.SetAutoExposure(true);

            double oldValue = -1;
            double newValue = -2;

            do
            {
                oldValue = newValue;
                DhyanaObject.GetExposure(ref newValue);

                Exposure = newValue;
                //ExposureModel.Exposure = newValue;

                Task.Delay(3000);

            } while (Math.Abs(oldValue - newValue) > 0.01);

            DhyanaObject.SetAutoExposure(false);
        });
    }


    #endregion

    #region ROI

    public List<string> RoiMode => new List<string>()
    {
        "None","1024 X 1024","512 X 512","256 X 256","128 X 128"
    };

    public List<string> RoiModeLite => new List<string>()
    {
        "None","1024","512","256","128"
    };

    [ObservableProperty]
    private uint _roiModeIndex = 0;

    [ObservableProperty]
    private RoiModel _roiModel = new();

    partial void OnRoiModeIndexChanged(uint value)
        => SetValueWithCapture(() =>
        {
            switch (value)
            {
                case 0:
                    DhyanaObject.UnSetRoi();
                    break;

                default:
                    var width = 1024 / (int)Math.Pow(2, value - 1);
                    var height = 1024 / (int)Math.Pow(2, value - 1);
                    DhyanaObject.SetRoi(width, height, 0, 0);
                    RoiEnabled = true;
                    break;
            }

            RefreshRoi();
        });


    [ObservableProperty]
    private bool _roiEnabled = false;

    [RelayCommand]
    void SetRoi()
        => SetValueWithCapture(() =>
        {
            RoiModeIndex = 0;
            if (!RoiEnabled)
                DhyanaObject.UnSetRoi();
            else
            {
                DhyanaObject.SetRoi(RoiModel.Width, RoiModel.Height, RoiModel.HOffset, RoiModel.VOffset);
                RefreshRoi();
            }

        });



    #endregion

    #region Histogram

    private void OnAutoLevelChanged()
    {
        _levelTimer.Stop();

        if(IsCapture)
            _levelTimer.Start();

        switch (IsAutoLeftLevel)
        {
            case true when !IsAutoRightLevel:
                DhyanaObject.SetAutolevels(1);
                break;
            case false when IsAutoRightLevel:
                DhyanaObject.SetAutolevels(2);
                break;
            case true when IsAutoRightLevel:
                DhyanaObject.SetAutolevels(3);
                break;
            default:
                DhyanaObject.SetAutolevels(0);
                _levelTimer.Stop();
                break;
        }
    }


    /// <summary>
    /// 是否开启直方统计 这个默认每次启动相机都去启动
    /// 
    /// </summary>
    [ObservableProperty]
    private bool _isHistc = true;

    partial void OnIsHistcChanged(bool value) => DhyanaObject.SetHistc(value);

    /// <summary>
    ///
    /// </summary>
    [ObservableProperty]
    private bool _isAutoLeftLevel = true;

    partial void OnIsAutoLeftLevelChanged(bool value) => OnAutoLevelChanged();

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]

    private bool _isAutoRightLevel = true;

    partial void OnIsAutoRightLevelChanged(bool value) => OnAutoLevelChanged();


    /// <summary>
    /// 8bit  - 254
    /// 16bit - 65534
    /// </summary>
    [ObservableProperty]
    private double _leftLevel = 0;

    partial void OnLeftLevelChanged(double value)
    {
        if (!IsAutoLeftLevel) DhyanaObject.SetLeftLevels(value);
    }

    /// <summary>
    /// 8bit  - 254
    /// 16bit - 65534
    /// </summary>
    [ObservableProperty]
    private double _rightLevel = 0;

    partial void OnRightLevelChanged(double value)
    {
        if (!IsAutoRightLevel) DhyanaObject.SetRightLevels(value);
    }
    #endregion

    #region Noise

    public List<string> NoiseModes => new List<string>()
    {
        "Off","Low","Medium","High"
    };

    [ObservableProperty]
    private uint _noiseModeIndex = 3;

    partial void OnNoiseModeIndexChanged(uint value)
        => DhyanaObject.SetNoiseLevel(value);

    #endregion

    #region Orientation

    [ObservableProperty]
    private bool _horizontal = false;

    partial void OnHorizontalChanged(bool value)
        => DhyanaObject.SetHorizontal(value);


    [ObservableProperty]
    private bool _vertical = false;

    partial void OnVerticalChanged(bool value)
        => DhyanaObject.SetHorizontal(value);

    #endregion

    #region ImageProperty

    public List<string> ImageModes => Dhyana.ImageModeRename;

    [ObservableProperty]
    private uint _imageModeIndex = 0;

    partial void OnImageModeIndexChanged(uint value)
    {
        switch (ImageModeIndex)
        {
            // 这部分完全去按照操作手册里面给的来写
            case 0:
                GlobalGain = 0;
                ImageMode = 1;
                break;
            case 1:
                GlobalGain = 0;
                ImageMode = 2;
                break;
            case 2:
                GlobalGain = 1;
                ImageMode = 3;
                break;
            case 3:
                GlobalGain = 2;
                ImageMode = 4;
                break;
            case 4:
                // NOTE 这里不确定如果等于2是个什么情况
                GlobalGain = 1;
                ImageMode = 5;
                break;
            default:
                break;
        }
    }

    [ObservableProperty]
    private int _globalGain = 0;

    partial void OnGlobalGainChanged(int value)
        => DhyanaObject.SetGlobalGain(GlobalGain);

    [ObservableProperty]
    private int _imageMode = 0;

    partial void OnImageModeChanged(int value)
        => DhyanaObject.SetImageMode(ImageMode);

    [ObservableProperty]
    private double _gamma = 0;

    partial void OnGammaChanged(double value)
        => DhyanaObject.SetGamma(Gamma);

    [ObservableProperty]
    private double _contrast = 0;

    partial void OnContrastChanged(double value)
        => DhyanaObject.SetContrast(Contrast);
    #endregion

    #region fans

    /**
     * Note 关于风扇和温度，这里有一些概念问题亟待了解，暂时不写
     */

    #endregion

    #region ImageOutput

    //[ObservableProperty]
    //private SaveFrameModel _saveFrameModel = new();

    //[ObservableProperty]
    //private int _savePictureCount = 1;

    //[ObservableProperty]
    //private int _savePictureInterval = 5;

    //private int _saveCountFlag = 0;

    ///// <summary>
    ///// 当前依旧是调用的API，后期可能会对图片进行增删改
    ///// </summary>
    //[RelayCommand]
    //async Task SavePictures()
    //{
    //    _saveCountFlag = 0;

    //    DispatcherTimer timer = new(priority: DispatcherPriority.Background)
    //    {
    //        Interval = TimeSpan.FromSeconds((double)SavePictureInterval),
    //    };

    //    timer.Tick += (sender, args) =>
    //    {
    //        if (_saveCountFlag < SavePictureCount)
    //        {
    //            SaveOneFrame();
    //            _saveCountFlag++;
    //        }
    //        else
    //        {
    //            timer.Stop();
    //        }
    //    };

    //    await Task.Run(() =>
    //    {
    //        timer.Start();

    //        var span = SavePictureInterval == 0 ? 1000 : SavePictureInterval * 1000;

    //        while (timer.IsEnabled)
    //        {
    //            Task.Delay(span).Wait();
    //        }
    //    });
    //}

    //void SaveOneFrame()
    //{
    //    try
    //    {
    //        var suffix = SaveFrameModel.IsTimeSuffix ? $"_{DateTime.Now:yyyyMMdd_HH_mm_ss}" : "";
    //        var path = $"{SaveFrameModel.Root}\\{SaveFrameModel.Name}{suffix}";

    //        if (SaveFrameModel.IsRaw)
    //            DhyanaObject.SaveCurrentFrame(path, 0);
    //        if (SaveFrameModel.IsTif)
    //            DhyanaObject.SaveCurrentFrame(path, 1);
    //        if (SaveFrameModel.IsPng)
    //            DhyanaObject.SaveCurrentFrame(path, 2);
    //        if (SaveFrameModel.IsJpg)
    //            DhyanaObject.SaveCurrentFrame(path, 3);
    //        if (SaveFrameModel.IsBmp)
    //            DhyanaObject.SaveCurrentFrame(path, 4);
    //    }
    //    catch (Exception e)
    //    {
    //        //MessageBox.Show(e.ToString());
    //        throw new Exception("", e);

    //    }
    //}

    //[ObservableProperty]
    //private float _videoFps = 25;

    //[RelayCommand]
    //private void SaveVideo()
    //{
    //    try
    //    {
    //        var suffix = SaveFrameModel.IsTimeSuffix ? $"_{DateTime.Now:yyyyMMdd_HH_mm_ss}" : "";
    //        string path = $"{SaveFrameModel.Root}\\{SaveFrameModel.Name}{suffix}.avi";

    //        var interval = Exposure + 100;
    //        DhyanaObject.SaveVideo((int)interval, VideoFps, path);
    //    }
    //    catch (Exception e)
    //    {
    //        //MessageBox.Show(e.ToString());
    //        throw new Exception("", e);

    //    }
    //}

    #endregion
}
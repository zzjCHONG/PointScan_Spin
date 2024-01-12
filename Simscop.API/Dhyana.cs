using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Simscop.API.Native;

namespace Simscop.API;

/**
 * NOTE
 * 
 * 有个重大问题，如果，如果部分数据不用static，而是直接放到对象中
 * 它居然会偷默默的给回收掉！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
 * 
 * 关于GC的问题后期看书再优化
 * 
 * 关于兼容性问题和稳定性问题，在ViewModels中处理而不是API中
 *
 *
 * 关于需要刷新Capture的函数测试结果:
 *
 *
 */

/// <summary>
/// 仅仅只是400BSI V3
/// </summary>
public static class Dhyana
{
    private static bool AssertRet(TUCAMRET ret, bool assertInit = true, bool assertConnect = true)
    {
        StackTrace st = new StackTrace(true);

        if (assertInit && !IsInitialized()) return false;

        if (assertConnect && !IsConnected()) return false;

        if (ret != TUCAMRET.TUCAMRET_SUCCESS)
        {
            Debug.WriteLine($"[ERROR] [{st?.GetFrame(1)?.GetMethod()?.Name}] {ret}");
            return false;
        }

        Debug.WriteLine($"[INFO] [{st?.GetFrame(1)?.GetMethod()?.Name}] {ret}");

        return true;
    }

    #region core

    private static TUCAM_INIT _sdk = new TUCAM_INIT();

    private static TUCAM_OPEN _camera;

    public static bool InitializeSdk()
    {
        IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

        _sdk.uiCamCount = 0;
        _sdk.pstrConfigPath = strPath;

        if (!AssertRet(TUCamAPI.TUCAM_Api_Init(ref _sdk), assertConnect: false)) return false;

        if (_sdk.uiCamCount == 0)
        {
            Debug.WriteLine("No camera found");
            return false;
        }

        return true;
    }

    public static bool UninitializeSdk()
        => AssertRet(TUCamAPI.TUCAM_Api_Uninit(), false, false);

    public static bool InitializeCamera(uint cameraId)
    {
        _camera.uiIdxOpen = cameraId;
        return AssertRet(TUCamAPI.TUCAM_Dev_Open(ref _camera), true, false);
    }

    public static bool UnInitializeCamera()
        => AssertRet(TUCamAPI.TUCAM_Dev_Close(_camera.hIdxTUCam), false, false);

    #endregion

    #region assert

    //TODO 
    // 判断是否连接相机
    // 判断是否初始化

    private static bool IsInitialized()
    {
        if (_sdk.uiCamCount == 0)
        {
            Debug.WriteLine("No camera found");
            return false;
        }

        return true;
    }

    private static bool IsConnected()
    {
        if (_camera.hIdxTUCam == IntPtr.Zero)
        {
            Debug.WriteLine("No camera connected");
            return false;
        }

        return true;
    }


    #endregion

    #region Trigger

    /**
     * TODO
     * 触发器常用功能
     */

    #endregion

    #region Capa

    /// <summary>
    /// 支持的分辨率，对应0-3
    /// </summary>
    public static readonly List<string> Resolutions = new List<string>()
    {
        "2048x2048(Normal)", "2048x2048(Enhance)", "1024x1024(2x2Bin)", "512x512(4x4Bin)"
    };

    /// <summary>
    /// 设置分辨率
    /// </summary>
    /// <param name="resId"></param>
    /// <returns></returns>
    public static bool SetResolution(int resId)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_RESOLUTION, resId));

    /// <summary>
    /// 获取分辨率
    /// </summary>
    /// <param name="resId"></param>
    /// <returns></returns>
    public static bool GetResolution(ref int resId)
        => AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_RESOLUTION, ref resId));

    /// <summary>
    /// 设置是否水平翻转
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetHorizontal(bool value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_HORIZONTAL, value ? 1 : 0));

    /// <summary>
    /// 获取是否水平翻转
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetHorizontal(out bool value)
    {
        int val = 0;
        value = false;

        bool rec = AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_HORIZONTAL, ref val));

        value = val == 0;
        return rec;
    }

    /// <summary>
    /// 设置是否垂直翻转
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetVertical(bool value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_VERTICAL, value ? 1 : 0));

    /// <summary>
    /// 获取是否垂直翻转
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetVertical(out bool value)
    {
        int val = 0;
        value = false;

        bool rec = AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_VERTICAL, ref val));

        value = val == 0;
        return rec;
    }

    /// <summary>
    /// 风扇模式，对应0-3
    /// </summary>
    public static List<string> FanGear = new List<string>()
    {
        "High", "Medium", "Low", "Off"
    };

    /// <summary>
    /// 设置风扇转速
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetFanGear(int value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_FAN_GEAR, value));

    /// <summary>
    /// 获取风扇转速
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetFanGear(ref int value)
        => AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_FAN_GEAR, ref value));


    /// <summary>
    /// 是否开启直方图数据统计
    /// 只有开启了才能够设置自动色阶
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetHistc(bool value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
                       (int)TUCAM_IDCAPA.TUIDC_HISTC, value ? 1 : 0));

    /// <summary>
    /// 获取直方图数据统计是否开启
    /// </summary>
    /// <returns></returns>
    public static bool GetHistc()
    {
        int val = 0;
        bool rec = AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
                       (int)TUCAM_IDCAPA.TUIDC_HISTC, ref val));
        return rec && val == 1;
    }

    /// <summary>
    /// 自动色阶模式，对应0-3
    /// </summary>
    public static readonly List<string> Levels = new List<string>()
    {
        "Disable Auto Levels",
        "Auto Left Levels",
        "Auto Right Levels",
        "Auto Levels",
    };

    /// <summary>
    /// 设置自动色阶设置
    /// 需要先开启直方图统计
    /// </summary>
    /// <param name="value">
    /// 0 - "Disable Auto Levels"
    /// 1 - "Auto Left Levels"
    /// 2 - "Auto Right Levels"
    /// 3 - "Auto Levels"
    /// </param>
    /// <returns></returns>
    public static bool SetAutolevels(int value = 3)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ATLEVELS, value));

    /// <summary>
    /// 获取自动色阶模式
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetAutolevels(ref int value)
        => AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ATLEVELS, ref value));


    /// <summary>
    /// 自动曝光模式 对应1-5
    /// </summary>
    public static readonly List<string> ImageMode = new List<string>()
    {
        "CMS", "HDR", "HighSpeedHG", "HighSpeedLG", "GlobalReset"
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly List<string> ImageModeRename = new List<string>()
    {
        "High Sensitivity","High Dynamic","High Speed HG","High Speed LG","Global Reset"
    };

    /**
     * 对增益模式的描述
     * CMS
     * 12Bit Mode=1 Gain=0
     * HDR
     * 16Bit Mode=2 Gain=0
     * HighGain
     * 11Bit Mode=2 Gain=1
     * 12Bit(High Speed) Mode=3 Gain=1
     * 12Bit(Global Reset) Mode=5 Gain=1
     * LowGain
     * 11Bit Mode=2 Gain=2
     * 12Bit(High Speed) Mode=4 Gain=2
     * 12Bit(Global Reset) Mode=5 Gain=2
     */

    /// <summary>
    /// 设置图片模式，这个和增益模式无关
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetImageMode(int value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_IMGMODESELECT, value));

    public static bool GetImageMode(ref int value)
        => AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_IMGMODESELECT, ref value));

    public static bool SetLedEnable(bool value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
                       (int)TUCAM_IDCAPA.TUIDC_ENABLELED, value ? 1 : 0));

    public static bool SetTimestampEnable(bool value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ENABLETIMESTAMP, value ? 1 : 0));

    public static bool SetTriggerOutEnable(bool value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ENABLETRIOUT, value ? 1 : 0));

    public static readonly List<string> RollingScanMode = new List<string>()
    {
        "Off","线路延时","缝隙高度"
    };

    public static bool SetRollingScanMode(int value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ROLLINGSCANMODE, value));

    public static bool GetRollingScanMode(ref int value)
        => AssertRet(TUCamAPI.TUCAM_Capa_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ROLLINGSCANMODE, ref value));

    // TODO TUIDC_ROLLINGSCANLTD TUIDC_ROLLINGSCANSLIT TUIDC_ROLLINGSCANDIR TUIDC_ROLLINGSCANRESET


    /// <summary>
    /// 设置自动曝光状态
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetAutoExposure(bool value)
        => AssertRet(TUCamAPI.TUCAM_Capa_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ATEXPOSURE, value ? 1 : 0));

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Obsolete("这个方法不适合当前相机", true)]
    public static bool SetOnceExposure()
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ATEXPOSURE, 2, 0));

    /// <summary>
    /// 设置自动曝光类型
    /// 0 - 居中曝光
    /// 1 - 居右曝光
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetAutoExposureMode(int value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDCAPA.TUIDC_ATEXPOSURE_MODE, value, 0));
    #endregion

    #region Prop

    /// <summary>
    /// 全局增益模式，对应0-5
    /// </summary>
    public static readonly List<string> GlobalGain = new List<string>()
    {
        "HDR" ,"High gain" ,"Low gain" ,"HDR - Raw" ,"High gain - Raw","Low gain - Raw"
    };


    /// <summary>
    /// 设置增益模式，和SetImageMode配合使用
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetGlobalGain(int value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
                       (int)TUCAM_IDPROP.TUIDP_GLOBALGAIN, value, 0));

    /// <summary>
    /// 获取增益
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetGlobalGain(ref double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_GLOBALGAIN, ref value, 0));


    /// <summary>
    /// 设置曝光
    /// 这个设置的最小值是0，但是最大值和步进都需要从接口获取
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetExposure(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_EXPOSURETM, value, 0));

    /// <summary>
    /// 获取曝光值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetExposure(ref double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_EXPOSURETM, ref value, 0));

    /// <summary>
    /// 设置自动曝光
    /// 范围20-255，步进1
    /// 自动曝光生效前提为启动自动曝光模式且模式为居中自动曝光
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetBrightness(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_BRIGHTNESS, value, 0));

    /// <summary>
    /// 获取自动曝光值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetBrightness(ref double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_BRIGHTNESS, ref value, 0));

    // TODO TUIDP_BLACKLEVEL

    /// <summary>
    /// 设置黑电平值
    /// 范围1-8191，步进1
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetBlackLevel(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_BLACKLEVEL, value, 0));

    /// <summary>
    /// 获取黑电平值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetBlackLevel(ref double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_BLACKLEVEL, ref value, 0));

    /// <summary>
    /// 设置相机温度
    /// 范围0-100，步进1
    /// 实际温度为-50℃到50℃
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetTemperature(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_TEMPERATURE, value, 0));

    /// <summary>
    /// 获取相机温度
    /// 范围从0-100
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetTemperature(ref double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_TEMPERATURE, ref value, 0));

    /// <summary>
    /// 设置降噪等级
    /// 可选为0 1 2 3
    /// 数值越大，降噪强度越大
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetNoiseLevel(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_NOISELEVEL, value, 0));

    /// <summary>
    /// 获取降噪等级
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetNoiseLevel(ref double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_NOISELEVEL, ref value, 0));

    /// <summary>
    /// 设置伽马值
    /// 范围0-255
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetGamma(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_GAMMA, value, 0));

    /// <summary>
    /// 获取伽马
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetGamma(out double value)
    {
        value = 0;
        return AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_GAMMA, ref value, 0));
    }

    /// <summary>
    /// 设置对比度
    /// 范围0-255
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetContrast(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_CONTRAST, value, 0));

    /// <summary>
    /// 获取对比度
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetContrast(out double value)
    {
        value = 0;
        return AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_CONTRAST, ref value, 0));
    }


    /// <summary>
    /// 设置左色阶
    /// 范围在8bit为1-255
    /// 范围在16bit为1-65535
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetLeftLevels(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_LFTLEVELS, value, 0));

    /// <summary>
    /// 获取左色阶
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetLeftLevels(out double value)
    {
        value = 0;
        return AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_LFTLEVELS, ref value, 0));
    }

    /// <summary>
    /// 设置右色阶
    /// 范围在8bit为1-255
    /// 范围在16bit为1-65535
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetRightLevels(double value)
        => AssertRet(TUCamAPI.TUCAM_Prop_SetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_RGTLEVELS, value, 0));

    /// <summary>
    /// 获取右色阶
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetRightLevels(out double value)
    {
        value = 0;
        return AssertRet(TUCamAPI.TUCAM_Prop_GetValue(_camera.hIdxTUCam,
            (int)TUCAM_IDPROP.TUIDP_RGTLEVELS, ref value, 0));
    }

    #endregion

    #region PropAttr

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="attr"></param>
    /// <param name="prop"></param>
    /// <returns></returns>
    private static bool GetPropAttr(out TUCAM_PROP_ATTR attr, TUCAM_IDPROP prop)
    {
        attr = default;

        attr.nIdxChn = 0;
        attr.idProp = (int)prop;

        return AssertRet(TUCamAPI.TUCAM_Prop_GetAttr(_camera.hIdxTUCam, ref attr));
    }

    /// <summary>
    /// 获取曝光时间属性参数
    /// </summary>
    /// <param name="attr"></param>
    /// <returns></returns>
    public static bool GetExposureAttr(out TUCAM_PROP_ATTR attr)
        => GetPropAttr(out attr, TUCAM_IDPROP.TUIDP_EXPOSURETM);

    #endregion

    #region Capture

    private static readonly byte[] _captureFrameFormat = new[]
{
        (byte)TUFRM_FORMATS.TUFRM_FMT_RAW,
        (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl,
        (byte)TUFRM_FORMATS.TUFRM_FMT_RGB888,
    };


    public static TUCAM_FRAME _captureFrame = new TUCAM_FRAME()
    {
        pBuffer = IntPtr.Zero,
        ucFormat = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl,
        uiRsdSize = 1,
    };

    public static TUCAM_FRAME CurrentFrame => _captureFrame;

    /// <summary>
    /// 开始捕获
    ///
    /// </summary>
    /// <param name="frameFormat">
    /// 0 - RAW
    /// 1 - USUAL
    /// 2 - RGB888
    /// </param>
    /// <returns></returns>
    public static bool StartCapture(uint frameFormat = 0)
    {
        _captureFrame.ucFormat = _captureFrameFormat[frameFormat];
        _captureFrame.pBuffer = IntPtr.Zero;
        _captureFrame.uiRsdSize = 1;

        TUCamAPI.TUCAM_Buf_Alloc(_camera.hIdxTUCam, ref _captureFrame);
        return AssertRet(TUCamAPI.TUCAM_Cap_Start(_camera.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool StopCapture()
        => AssertRet(TUCamAPI.TUCAM_Buf_AbortWait(_camera.hIdxTUCam)) &&
           AssertRet(TUCamAPI.TUCAM_Cap_Stop(_camera.hIdxTUCam)) &&
           AssertRet(TUCamAPI.TUCAM_Buf_Release(_camera.hIdxTUCam));


    public static bool GetCurrentFrame(int interval = 1000)
        => AssertRet(TUCamAPI.TUCAM_Buf_WaitForFrame(_camera.hIdxTUCam, ref _captureFrame, interval));
    #endregion

    #region Extensions

    /**
     * TODO
     * 捕获Frame开启和关闭
     * 视频录制导出
     * Frame导出
     * Bin设置
     */


    /// <summary>
    /// 
    /// </summary>
    private static TUCAM_ROI_ATTR _cameraRoiAttr = default;

    /// <summary>
    /// 设置数据ROI
    /// NOTE ： 这个是SDK层面的设置，所以，修改后会更改相机实际输出数据部分
    /// </summary>
    /// <param name="width">宽</param>
    /// <param name="height">高</param>
    /// <param name="hOffset">水平偏移</param>
    /// <param name="vOffset">垂直偏移</param>
    /// <returns></returns>
    public static bool SetRoi(int width = 0, int height = 0, int hOffset = 0, int vOffset = 0)
    {
        _cameraRoiAttr.bEnable = true;

        _cameraRoiAttr.nHOffset = (int)((hOffset >> 2) << 2);
        _cameraRoiAttr.nVOffset = (int)((vOffset >> 2) << 2);
        _cameraRoiAttr.nWidth = (int)((width >> 2) << 2);
        _cameraRoiAttr.nHeight = (int)((height >> 2) << 2);

        return AssertRet(TUCamAPI.TUCAM_Cap_SetROI(_camera.hIdxTUCam, _cameraRoiAttr)) && GetRoi(ref _cameraRoiAttr);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool UnSetRoi()
    {
        _cameraRoiAttr.bEnable = false;
        return AssertRet(TUCamAPI.TUCAM_Cap_SetROI(_camera.hIdxTUCam, _cameraRoiAttr)) && GetRoi(ref _cameraRoiAttr);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool GetRoi(ref TUCAM_ROI_ATTR attr)
        => AssertRet(TUCamAPI.TUCAM_Cap_GetROI(_camera.hIdxTUCam, ref attr));

    /// <summary>
    /// 
    /// </summary>
    public static readonly List<string> PictureType = new List<string>()
    {
        "RAW","TIF","PNG","JPG","BMP"
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly List<TUIMG_FORMATS> SaveFormatList = new List<TUIMG_FORMATS>()
    {
        TUIMG_FORMATS.TUFMT_RAW,
        TUIMG_FORMATS.TUFMT_TIF,
        TUIMG_FORMATS.TUFMT_PNG,
        TUIMG_FORMATS.TUFMT_JPG,
        TUIMG_FORMATS.TUFMT_BMP,
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="typeId"></param>
    /// <returns></returns>
    public static bool SaveCurrentFrame(string path, int typeId)
    {
        TUCAM_FRAME capture = default;

        // NOTE 这里非常离谱，他们路径只支持\\这种写法
        path = path.Replace("/", "\\");

        TUCAM_FILE_SAVE fSave;

        fSave.pstrSavePath = Marshal.StringToHGlobalAnsi(path);
        fSave.pFrame = Marshal.AllocHGlobal(Marshal.SizeOf(capture));
        Marshal.StructureToPtr(capture, fSave.pFrame, true);

        var fmt = (int)SaveFormatList[typeId];

        TUCAM_FRAME frame = default;

        frame.pBuffer = IntPtr.Zero;
        frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
        frame.uiRsdSize = 1;

        // Format RAW
        if (typeId == 0)
        {
            fmt &= ~(int)TUIMG_FORMATS.TUFMT_RAW;
            fSave.nSaveFmt = (int)TUIMG_FORMATS.TUFMT_RAW;
            // Get RAW data
            frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_RAW;


            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_CopyFrame(_camera.hIdxTUCam, ref frame))
            {
                var recall = TUCamAPI.TUCAM_File_SaveImage(_camera.hIdxTUCam, fSave);
                if (recall != TUCAMRET.TUCAMRET_SUCCESS)
                {
                    return false;
                }

                return true;
            }

        }

        // Format other
        if (0 != fmt)
        {
            fSave.nSaveFmt = (int)SaveFormatList[typeId];

            // Get other format data
            frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_CopyFrame(_camera.hIdxTUCam, ref frame))
                return TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_File_SaveImage(_camera.hIdxTUCam, fSave);
        }
        return false;
    }

    public static void SaveVideo(int interval=1000, float fps = 25f, string path = "a.avi")
    {
        int nTimes = 50;

        TUCAM_FRAME frame = default;
        TUCAM_REC_SAVE rec = default;

        frame.pBuffer = IntPtr.Zero;
        frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
        frame.uiRsdSize = 1;

        TUCamAPI.TUCAM_Buf_Alloc(_camera.hIdxTUCam, ref frame);

        if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(_camera.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE))
        {
            rec.fFps = fps;
            rec.nCodec = 0;
            rec.pstrSavePath = Marshal.StringToHGlobalAnsi(path);

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Rec_Start(_camera.hIdxTUCam, rec))
            {
                for (int i = 0; i < nTimes; ++i)
                {
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(_camera.hIdxTUCam, ref frame, interval))
                        TUCamAPI.TUCAM_Rec_AppendFrame(_camera.hIdxTUCam, ref frame);
                }

                TUCamAPI.TUCAM_Rec_Stop(_camera.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_AbortWait(_camera.hIdxTUCam);
            TUCamAPI.TUCAM_Cap_Stop(_camera.hIdxTUCam);
        }

        TUCamAPI.TUCAM_Buf_Release(_camera.hIdxTUCam);
    }

    #endregion

}
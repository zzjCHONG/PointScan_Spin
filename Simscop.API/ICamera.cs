using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace Simscop.API;

public interface ICamera
{
    /// <summary>
    /// 输出连接状态
    /// </summary>
    /// <returns></returns>
    public string GetConnectState();

    /// <summary>
    /// 相机初始化
    /// note: Deinit的代码写到析构函数中
    /// </summary>
    /// <returns></returns>
    public bool Init();

    /// <summary>
    /// 开始采集
    /// </summary>
    /// <returns></returns>
    public bool StartCapture();

    /// <summary>
    /// 采集一帧图像
    /// </summary>
    /// <returns></returns>
    public bool Capture(out Mat mat);

    /// <summary>
    /// 保存一帧的图像
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool SaveCapture(string path);

    /// <summary>
    /// 获取当前曝光
    /// </summary>
    /// <returns></returns>
    public bool GetExposure(out double exposure);

    /// <summary>
    /// 获取当前帧数
    /// </summary>
    /// <returns></returns>
    public bool GetFrameRate(out double frameRate);

    /// <summary>
    /// 设置曝光
    /// </summary>
    /// <param name="exposure"></param>
    /// <returns></returns>
    public bool SetExposure(double exposure);

    /// <summary>
    /// 停止采集
    /// </summary>
    /// <returns></returns>
    public bool StopCapture();

    /// <summary>
    /// 开启
    /// </summary>
    /// <returns></returns>
    public bool AcqStartCommand();

    /// <summary>
    /// 关闭
    /// </summary>
    /// <returns></returns>
    public bool AcqStopCommand();
}
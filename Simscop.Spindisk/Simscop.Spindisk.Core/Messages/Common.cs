using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.Messages;

public static class MessageManage
{
    /// <summary>
    /// 更改颜色
    /// </summary>
    public const string ChangedColorMap = "ChangedColorMapMessage";

    /// <summary>
    /// 保存当前的capture
    /// </summary>
    public const string SaveCurrentCapture = "SaveCurrentCaptureMessage";

    /// <summary>
    /// 只保存一个，只支持TIF
    /// </summary>
    public const string SaveACapture = "SaveACaptureMessage";//原图

    /// <summary>
    /// 当前显示frame数据
    /// </summary>
    public const string DisplayFrame = "DisplayFrameMessage";
}

public static class SteerMessage
{
    public const string MoveX = "MoveXMessage";

    public const string StatusX = "StatusXMessage";

    public const string MoveY = "MoveYMessage";

    public const string StatusY = "StatusYMessage";

    public const string MoveZ = "MoveZMessage";

    public const string StatusZ = "StatusZMessage";

    public const string Motor = "MotorMessage";

    public const string MotorReceive = "MotorReceiveMessage";

    public static string? GetValue(string name)
    {
        var type = typeof(SteerMessage);

        var fields = type.GetFields();

        return (from field in fields let value = field.GetRawConstantValue() 
            where field.Name == name select (string?)Convert.ToString(value)).FirstOrDefault();
    }
}

public record LaserMessage(int Index, bool Status);

public record SpindiskMessage(int Mode);


/// <summary>
/// 双击全屏相关逻辑
/// </summary>
/// <param name="Index"></param>
public record MainDisplayMessage(int Index);

/// <summary>
/// 设备初始化
/// 相机
/// </summary>
/// <param name="isPreInit"></param>
public record CameraInitMessage(bool isPreInit);

public record LaserInitMessage(bool isPreInit);

public record SteerInitMessage(bool isPreInit);

public record SpinInitMessage(bool isPreInit);

public record CameraConnectMessage(bool isConnected, bool isConnecting);

public record LaserConnectMessage(bool isConnected, bool isConnecting);

public record SteerConnectMessage(bool isConnected, bool isConnecting);

public record SpinConnectMessage(bool isConnected, bool isConnecting);

/// <summary>
/// 多通道采集-伪彩通道
/// </summary>
/// <param name="codeModel"></param>
public record MultiChannelSaveMessage(int codeModel);


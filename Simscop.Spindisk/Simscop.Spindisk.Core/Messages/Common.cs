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
    public const string SaveCurrentCapture = "SaveCurrentCaptureMessage";//存储，暂未使用

    /// <summary>
    /// 只保存一个，只支持TIF
    /// </summary>
    public const string SaveACapture = "SaveACaptureMessage";//实时原图

    /// <summary>
    /// 当前显示frame数据
    /// </summary>
    public const string DisplayFrame = "DisplayFrameMessage";//实时处理图像
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

    public const string Setting = "SettingMessage";

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


public record CameraInitMessage(bool IsPreInit);

public record LaserInitMessage(bool IsPreInit);

public record SteerInitMessage(bool IsPreInit);

public record SpinInitMessage(bool IsPreInit);

public record CameraConnectMessage(bool IsConnected, bool IsConnecting);

public record LaserConnectMessage(bool IsConnected, bool IsConnecting);

public record SteerConnectMessage(bool IsConnected, bool IsConnecting);

public record SpinConnectMessage(bool IsConnected, bool IsConnecting);

/// <summary>
/// 存图
/// 非实时画面
/// </summary>
/// <param name="channel"></param>
/// <param name="isOriginalImage"></param>
public record CameraSaveMessage(int channel, bool isOriginalImage,string filename);

/// <summary>
/// 多通道采集-伪彩通道
/// </summary>
/// <param name="codeModel"></param>
public record MultiChannelColorMessage(int codeModel);

/// <summary>
/// 多通道采集-激光通道触发
/// </summary>
/// <param name="channel"></param>
/// <param name="isEnable"></param>
public record MultiChannelLaserMessage(int channel,bool isEnable);

public record MultiChannelMergeMessage();




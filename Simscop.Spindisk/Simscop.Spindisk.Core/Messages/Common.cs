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

    public const string Splice = "SpliceMessage";

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

#region 初始化
/// <summary>
/// 相机初始化
/// </summary>
/// <param name="IsPreInit"></param>
public record CameraInitMessage();

/// <summary>
/// 激光初始化
/// </summary>
/// <param name="IsPreInit"></param>
public record LaserInitMessage();

/// <summary>
/// 电动台初始化
/// </summary>
/// <param name="IsPreInit"></param>
public record SteerInitMessage();

/// <summary>
/// 旋转台初始化
/// </summary>
/// <param name="IsPreInit"></param>
public record SpinInitMessage();
#endregion

#region 连接状态
/// <summary>
/// 相机连接状态
/// </summary>
/// <param name="IsConnected"></param>
/// <param name="IsConnecting"></param>
/// <param name="ConnectState"></param>
public record CameraConnectMessage(bool IsConnected, bool IsConnecting, string ConnectState);

/// <summary>
/// 激光连接状态
/// </summary>
/// <param name="IsConnected"></param>
/// <param name="IsConnecting"></param>
public record LaserConnectMessage(bool IsConnected, bool IsConnecting, string ConnectState);

/// <summary>
/// 电动台连接状态
/// </summary>
/// <param name="IsConnected"></param>
/// <param name="IsConnecting"></param>
public record SteerConnectMessage(bool IsConnected, bool IsConnecting, string ConnectState);

/// <summary>
/// 旋转台连接状态
/// </summary>
/// <param name="IsConnected"></param>
/// <param name="IsConnecting"></param>
public record SpinConnectMessage(bool IsConnected, bool IsConnecting, string ConnectState);
#endregion

/// <summary>
/// 双击全屏相关逻辑
/// </summary>
/// <param name="Index"></param>
public record MainDisplayMessage(int Index);

/// <summary>
/// 存图
/// 非实时画面
/// </summary>
/// <param name="Channel"></param>
/// <param name="IsOriginalImage"></param>
public record CameraSaveMessage(int Channel, bool IsOriginalImage,string Filename);

/// <summary>
/// 控件Enable
/// 激光通道开关控制：存图对应频道
/// </summary>
/// <param name="Channel"></param>
/// <param name="IsEnable"></param>
public record ChannelControlEnableMessage(int Channel,bool IsEnable);
/// <summary>
/// 当前界面显示
/// </summary>
public record CurrentDispalyChannelEnableMessage(bool IsFirstDisplayEnabled, bool IsSecondDisplayEnabled, bool IsThirdDisplayEnabled, bool IsFourthDisplayEnabled);

/// <summary>
/// 控件Enable
/// 相机开关控制：存图+多通道采集
/// </summary>
public record CameraControlEnableMessage(bool IsEnable);

/// <summary>
/// 多通道采集-多通道合并
/// </summary>
public record MultiChannelMergeMessage(string Filename, bool IsFirstEnabled, bool IsSecondEnabled, bool IsThirdEnabled, bool IsFourthEnabled);

/// <summary>
/// 多通道采集-打开图像窗体
/// </summary>
/// <param name="Filename"></param>
public record MultiChannelOpenImageWindowMwssage(string Filename);

/// <summary>
/// 电动台对焦动画状态
/// </summary>
public record SteerAnimationStateMessage(int Mode);

/// <summary>
/// 像素坐标变化
/// </summary>
/// <param name="X"></param>
/// <param name="Y"></param>
public record CurrentPositionMessage(int X, int Y);

/// <summary>
/// 反算电动台坐标
/// </summary>
/// <param name="X"></param>
/// <param name="Y"></param>
public record MappingMoveMessage(double X, double Y);

/// <summary>
/// 多通道合并弹窗
/// </summary>
public record PopupWindowMessage();
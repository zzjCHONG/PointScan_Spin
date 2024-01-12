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
    public const string SaveACapture = "SaveACaptureMessage";

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

    public static string? GetValue(string name)
    {
        var type = typeof(SteerMessage);

        var fields = type.GetFields();

        return (from field in fields let value = field.GetRawConstantValue() 
            where field.Name == name select (string?)Convert.ToString(value)).FirstOrDefault();
    }
}
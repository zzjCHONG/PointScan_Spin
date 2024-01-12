using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Documents;

namespace Simscop.API.Native.Mshot;

public static class Motor
{
    private const string DllName = "SerialCom.dll";

    public static MshotErrorCode ErrorMessage { get; set; } = MshotErrorCode.NONE;

    private static bool LoadErrorMessage(Func<int> action)
    {
        if (action() == 1) return true;

        var code = GetError();
        ErrorMessage = Enum.IsDefined(typeof(MshotErrorCode), code) ? (MshotErrorCode)code : MshotErrorCode.NO_DEFINE;

        return false;
    }

    /// <summary>
    /// 获取版本
    /// </summary>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "GetDllVersion", CallingConvention = CallingConvention.StdCall)]
    public static extern uint GetDLLVersion();

    /// <summary>
    /// 打开驱控设备(可以自动查找设备)
    /// 0 关闭设备，为1 打开设备（串口方式打开）
    /// </summary>
    /// <param name="isOpen">是否打开</param>
    /// <returns>
    /// 若串口成功打开，返回1，否则返回-1
    /// </returns>
    [DllImport(DllName, EntryPoint = "DLL_OpenQK", CallingConvention = CallingConvention.StdCall)]
    public static extern int OpenQk(int isOpen);

    public static bool OpenQk(bool isOpen)
        => LoadErrorMessage(() => OpenQk(isOpen ? 2 : 0));

    /// <summary>
    /// 打开串口
    /// 串口号（1, 2, 3...）, 波特率（115200, ...）
    /// </summary>
    /// <param name="isOpen"></param>
    /// <returns>
    /// 若串口成功打开，返回1
    /// </returns>
    [DllImport(DllName, EntryPoint = "DLL_OpenQK", CallingConvention = CallingConvention.StdCall)]
    public static extern int OpenCom(int isOpen);

    /// <summary>
    /// 设置操作轴
    /// </summary>
    /// <param name="axis">
    /// </param>
    [DllImport(DllName, EntryPoint = "SetControlAxis", CallingConvention = CallingConvention.StdCall)]
    public static extern void SetControlAxis(int axis);

    /// <summary>
    /// 设置操作轴
    /// </summary>
    /// <param name="axis">
    /// </param>
    [DllImport(DllName, EntryPoint = "SetControlAxis", CallingConvention = CallingConvention.StdCall)]
    public static extern void SetControlAxis(MshotAxis axis);

    /// <summary>
    /// 读取指定轴号的位置数据
    /// </summary>
    /// <param name="address">
    /// 目标轴号(1~9)
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_ReadPosition", CallingConvention = CallingConvention.StdCall)]
    public static extern int ReadPosition(uint address);

    /// <summary>
    /// 控制器使能与失能设置
    /// </summary>
    /// <param name="address">
    /// Address为要设置的目标轴号(1~9)
    /// </param>
    /// <param name="kg">
    /// KG为控制字符当KG='K'时控制器使能，当KG=‘G’时，马达失能
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_AxisEnable", CallingConvention = CallingConvention.StdCall)]
    public static extern int AxisEnable(uint address, char kg);

    public static bool AxisEnable(uint address, bool isEnable)
        => LoadErrorMessage(() => AxisEnable(address, isEnable ? 'K' : 'G'));

    /// <summary>
    /// 相对移动(以当前平台当前位置为参考进行移动)
    /// </summary>
    /// <param name="address">
    /// 驱控器地址(1~9)
    /// </param>
    /// <param name="position">
    /// 移动距离(单位count，为位置反馈的最小分辨率)（数值正负代表向哪个方向移动）
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_PositionRelativeMove", CallingConvention = CallingConvention.StdCall)]
    public static extern int PositionRelativeMove(uint address, int position);

    /// <summary>
    /// 绝对移动(以所设零点位置为参考进行移动)
    /// </summary>
    /// <param name="address">
    /// 驱控器地址(1~9)
    /// </param>
    /// <param name="position">
    /// 移动目标位置值（单位count，为位置反馈的最小分辨率)（数值正负代表目标点位于0点正方向还是负方向）
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_PositionAbsoluteMove", CallingConvention = CallingConvention.StdCall)]
    public static extern int PositionAbsoluteMove(uint address, int position);

    /// <summary>
    /// 设置位移台当前位置为0点
    /// </summary>
    /// <param name="address">
    /// 驱控器地址(1~9)
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_SetZeroPosition", CallingConvention = CallingConvention.StdCall)]
    public static extern int SetZeroPosition(uint address);

    /// <summary>
    /// JOG模式运动
    /// </summary>
    /// <param name="address">
    /// Address 驱控器地址(1~9)
    /// </param>
    /// <param name="cmd">
    /// CMD 运动命令 ‘L’左运行，‘R’右运行，‘T’停止运行
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "JOG_Move", CallingConvention = CallingConvention.StdCall)]
    public static extern int JogMove(uint address, char cmd);

    /// <summary>
    /// 读取最近一次软件配置错误信息
    /// </summary>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_GetError", CallingConvention = CallingConvention.StdCall)]
    public static extern int GetError();

    /// <summary>
    /// 速度设置（最低分辨率0.1mm/s，速度设置范围0.1~50mm/s）
    /// </summary>
    /// <param name="address">
    /// ddress驱控器地址(1~9)
    /// </param>
    /// <param name="type">
    /// 速度类型（'V'普通定位速度，'J'Jog速度，'S'脉冲扫描速度，'F'开机找index速度）
    /// </param>
    /// <param name="mmPerS">
    /// mmPerS速度设置值
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_SetSpeed", CallingConvention = CallingConvention.StdCall)]
    public static extern int SetSpeed(uint address, char type, double mmPerS);

    /// <summary>
    /// 速度设置（最低分辨率0.1mm/s，速度设置范围0.1~50mm/s）
    /// </summary>
    /// <param name="address">
    /// ddress驱控器地址(1~9)
    /// </param>
    /// <param name="type">
    /// 速度类型（'V'普通定位速度，'J'Jog速度，'S'脉冲扫描速度，'F'开机找index速度）
    /// </param>
    /// <param name="mmPerS">
    /// mmPerS速度设置值
    /// </param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_SetSpeed", CallingConvention = CallingConvention.StdCall)]
    public static extern int SetSpeed(uint address, MshotSpeed type, double mmPerS);

    /// <summary>
    /// 速度设置（最低分辨率10000counts/s，速度设置范围30000~600000 counts/s）
    /// </summary>
    /// <param name="address"></param>
    /// <param name="type"></param>
    /// <param name="countsPerS"></param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_SetSpeed2", CallingConvention = CallingConvention.StdCall)]
    public static extern int SetSpeed(uint address, char type, uint countsPerS);

    /// <summary>
    /// 速度设置（最低分辨率10000counts/s，速度设置范围30000~600000 counts/s）
    /// </summary>
    /// <param name="address"></param>
    /// <param name="type"></param>
    /// <param name="countsPerS"></param>
    /// <returns></returns>
    [DllImport(DllName, EntryPoint = "DLL_SetSpeed2", CallingConvention = CallingConvention.StdCall)]
    public static extern int SetSpeed(uint address, MshotSpeed type, uint countsPerS);

    /// <summary>
    /// 读取设置速度
    /// </summary>
    /// <param name="address"></param>
    /// <param name="type"></param>
    /// <returns>mm/s</returns>
    [DllImport(DllName, EntryPoint = "DLL_GetSpeed", CallingConvention = CallingConvention.StdCall)]
    public static extern double GetSpeed(uint address, char type);

    /// <summary>
    /// 读取设置速度
    /// </summary>
    /// <param name="address"></param>
    /// <param name="type"></param>
    /// <returns>mm/s</returns>
    [DllImport(DllName, EntryPoint = "DLL_GetSpeed", CallingConvention = CallingConvention.StdCall)]
    public static extern double GetSpeed(uint address, MshotSpeed type);

    /// <summary>
    /// 读取设置速度
    /// </summary>
    /// <param name="address"></param>
    /// <param name="type"></param>
    /// <returns>counts/s</returns>
    [DllImport(DllName, EntryPoint = "DLL_GetSpeed2", CallingConvention = CallingConvention.StdCall)]
    public static extern int GetSpeed2(uint address, char type);

    /// <summary>
    /// 读取设置速度
    /// </summary>
    /// <param name="address"></param>
    /// <param name="type"></param>
    /// <returns>counts/s</returns>
    [DllImport(DllName, EntryPoint = "DLL_GetSpeed2", CallingConvention = CallingConvention.StdCall)]
    public static extern int GetSpeed2(uint address, MshotSpeed type);

    /// <summary>
    /// 查询指定轴的运动状态
    /// </summary>
    /// <param name="address"></param>
    /// <param name="statusNum">
    /// StatusName （1查询控制器是否使能，2查询是否运动，3查询马达是否接入，4查询控制器是否有异常反馈）
    /// </param>
    /// <returns>
    /// 1是，0否，-1 设置失败
    /// </returns>
    [DllImport(DllName, EntryPoint = "DLL_GetAxisStatus", CallingConvention = CallingConvention.StdCall)]
    public static extern int GetAxisStatus(uint address, int statusNum);

    public static bool GetAxisStatus(uint address, MshotAxisStatus status)
        => LoadErrorMessage(() => GetAxisStatus(address, (int)status));

}
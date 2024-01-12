using System;
using System.Collections.Generic;
using System.Text;

namespace Simscop.API.Native.Mshot;

public enum MshotErrorCode
{
    NONE = -1,
    NO_DEFINE= -2,

    // 清除错误信息1
    CLEAR = 0,
    // 马达相关错误1
    MOTOR = 991,
    // 没有检测到马达连接1
    NO_MOTOR = 992,
    // 没有使能1
    NO_ENABLE = 993,
    // 没有设备连接1
    NO_CONNECT = 994,
    // 所查询的目标位置没有最新内容1
    NO_CONTENT = 995,
    // 输入参数不符合要求1
    INPUT_PARAMETER = 996,
    // DLL_OpenCom 串口打开失败1
    COM_OPEN_FAILED = 997,
    // DLL_CloseCom 串口关闭失败(指定的com号无法打开)1
    COM_CLOSE_FAILED = 998,
    // ComClose 串口处于关闭状态1
    COM_CLOSED = 999,
    // 控制器收到命令，但没有正常设置（可能由于控制器处于异常状态或和其他命令设置有冲突）1
    RESPONSE_BUT_FAIL = 1000,
    // 设置超时1
    SET_OVER_TIME = 1001,
    // Socket初始化失败1
    SOCKET_INIT_FAILED = 1002,
    // Socket链接失败1
    SOCKET_LINK_FAILED = 1003,
    // Socket1
    SOCKET_RECEIVED = 1004,
    // 驱控器返回指令设置错误1
    QKCMD_SET_ERROR = 1099,
    // 操作轴号为无效轴号1
    INVALID_AIXS_ADDRESS = 1100,
    // 没有查询到有效设备1
    NO_VALID_DEVICE = 1101,
}

[Flags]
public enum MshotAxis
{
    ONE = 0x01,
    TWO = 0x02,
    THREE = 0x04,
    ALL = ONE + TWO + THREE,
}

/// <summary>
/// Type速度类型（'V'普通定位速度，'J'Jog速度，'S'脉冲扫描速度，'F'开机找index速度）
/// </summary>
public enum MshotSpeed
{
    V = 'V',
    JOG = 'J',
    SPURT = 'S',
    FIND = 'F',
}

/// <summary>
/// 1查询控制器是否使能，2查询是否运动，3查询马达是否接入，4查询控制器是否有异常反馈
/// </summary>
public enum MshotAxisStatus
{
    ENABLE = 1,
    ACTION = 2,
    MOTOR = 3,
    CONTROL = 4,
}
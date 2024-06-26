﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Simscop.API;

public interface ILaser
{
    /// <summary>
    /// 输出连接状态
    /// </summary>
    /// <returns></returns>
    public string GetConnectState();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Init();

    public bool DisConnect();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public bool SetStatus(int count,bool status);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public bool GetStatus(int count ,out bool status);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count">
    /// 激光通道个数
    /// </param>
    /// <param name="value">
    /// 0 - 100 这个是强度百分比
    /// </param>
    /// <returns></returns>
    public bool SetPower(int count, int value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool GetPower(int count, out int value);
}
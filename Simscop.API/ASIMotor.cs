﻿using Simscop.API.ASI;

namespace Simscop.API;

public class ASIMotor
{
    Motor _motor;

    public bool InitializeMotor(out string errMsg)
    {
        _motor = new Motor();

        if (!_motor.OpenCom(out errMsg)) return false;
        errMsg = "Initialize steer completed!";
        return true;
    }

    public void ReadPosition()
    {
        _motor.ReadPosition();
    }

    #region Position

    public double X
        => _motor.x;

    public double Y
        => _motor.y;

    public double Z
        => _motor.z;

    #endregion

    //#region Enable

    //public bool XEnabled => Motor.GetAxisStatus(XAddress, MshotAxisStatus.ENABLE);

    //public bool YEnabled => Motor.GetAxisStatus(YAddress, MshotAxisStatus.ENABLE);

    //public bool ZEnabled => Motor.GetAxisStatus(ZAddress, MshotAxisStatus.ENABLE);

    //#endregion

    //#region Action 舵机状态

    //public bool XAction
    //    => Motor.GetAxisStatus(XAddress, MshotAxisStatus.ACTION);

    //public bool YAction
    //    => Motor.GetAxisStatus(YAddress, MshotAxisStatus.ACTION);

    //public bool ZAction
    //    => Motor.GetAxisStatus(ZAddress, MshotAxisStatus.ACTION);

    //#endregion

    //#region Exception

    //public bool XException
    //    => Motor.GetAxisStatus(XAddress, MshotAxisStatus.CONTROL);

    //public bool YException
    //    => Motor.GetAxisStatus(YAddress, MshotAxisStatus.CONTROL);

    //public bool ZException
    //    => Motor.GetAxisStatus(ZAddress, MshotAxisStatus.CONTROL);

    //#endregion

    #region 相对位移

    public bool SetOffset(Motor.Axis axis, double value)
    {
        if (_motor.MoveRelative(axis, value)) return true;



        return false;
    }

    public bool SetXOffset(double value)
        => SetOffset(Motor.Axis.X, value);

    public bool SetYOffset(double value)
        => SetOffset(Motor.Axis.Y, value);

    public bool SetZOffset(double value)
        => SetOffset(Motor.Axis.Z, value);

    #endregion

    #region 绝对路径
    public bool SetPosition(Motor.Axis axis, double value)
    {
        if (_motor.MoveAbsolute(axis, value)) return true;


        return false;
    }

    public bool SetXPosition(double value)
        => SetPosition(Motor.Axis.X, value);

    public bool SetYPosition(double value)
        => SetPosition(Motor.Axis.Y, value);

    public bool SetZPosition(double value)
        => SetPosition(Motor.Axis.Z, value);

    public bool SetXYPosition(double x, double y) => _motor.MoveXYAbsolute(x, y);

    public void Stop() => _motor.StopMove();

    #endregion

    public void ResetPosition()
    {
        _motor.Home();
    }

    public bool UnInitializeMotor()
    {
        if (!_motor.CloseCom()) return false;
        return true;
    }
}
using OpenCvSharp;
using Simscop.API.Bogao;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Simscop.API;

public class BogaoLaser : ILaser
{
    Laser _laser;

    public BogaoLaser()
    {
        _laser = new Laser();
    }

    public string GetConnectState() => _laser._connectState;

    public bool GetPower(int count, out int value) => _laser.GetPower(count + 1, out value);

    public bool GetStatus(int count, out bool status) => _laser.GetStatus(count + 1,out status);

    public bool Init()=> _laser.OpenCom();

    public bool SetPower(int count, int value) => _laser.SetPower(count + 1, value);

    public bool SetStatus(int count, bool status)
    {
        if (status)
            return _laser.OpenChannel(count+1);
        else return _laser.CloseChannel(count+1);
    }
}

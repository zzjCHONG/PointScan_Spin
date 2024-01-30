using OpenCvSharp;
using Simscop.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core;

public static class GlobalValue
{
    public static Mat? CurrentFrame;

    public static ASIMotor? GlobalMotor;

    public static ICamera? GlobalCamera;

    public static ILaser? GlobalLaser;

    public static XLight? GlobalSpin;
}
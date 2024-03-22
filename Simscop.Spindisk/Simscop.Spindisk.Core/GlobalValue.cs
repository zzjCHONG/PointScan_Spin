using OpenCvSharp;
using Simscop.API;
using Simscop.Spindisk.Core.Models;
using Simscop.Spindisk.Core.ViewModels;

namespace Simscop.Spindisk.Core;

public static class GlobalValue
{
    public static Mat? CurrentFrame;

    //public static ASIMotor? GlobalMotor;
    public static MshotMotor? GlobalMotor;

    public static ICamera? GlobalCamera;

    public static ILaser? GlobalLaser;

    public static XLight? GlobalSpin;

    public static AutoFocus? GeneralFocus;

    public static AutoFocus? CustomFocus;

    public static ShellViewModel GlobalShellViewModel;
}
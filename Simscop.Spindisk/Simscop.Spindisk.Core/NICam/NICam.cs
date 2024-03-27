using OpenCvSharp;
using Simscop.API;
using System.Collections.Generic;

namespace Simscop.Spindisk.Core.Models.NIDevice
{
    public class NICam : ICamera
    {
        NIImplementation _NICam = new();

        public bool AcqStartCommand() => _NICam.AcqStartCommand();

        public bool AcqStopCommand() => _NICam.AcqStopCommand();

        public bool Capture(out Mat mat) => _NICam.Capture(out mat);

        public string GetConnectState() => _NICam.GetConnectState();

        public bool GetExposure(out double exposure) => _NICam.GetExposure(out exposure);

        public bool GetFrameRate(out double frameRate) => _NICam.GetFrameRate(out frameRate);

        public bool Init(out List<string> devices)=>_NICam.Init(out devices);

        public bool SaveCapture(string path) => _NICam.SaveCapture(path);

        public bool SetExposure(double exposure) => _NICam.SetExposure(exposure);

        public bool StartCapture() => _NICam.StartCapture();

        public bool StopCapture() => _NICam.StopCapture();

        public bool Init() => true;

    }
}

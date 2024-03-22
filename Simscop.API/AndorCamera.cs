using OpenCvSharp;
using Simscop.API.AndorCam;
using System.Collections.Generic;

namespace Simscop.API
{
    public class AndorCamera : ICamera
    {
        Andor _andor=new Andor();

        public string GetConnectState() => _andor.ConnectState;

        public bool Capture(out Mat mat) => _andor.Capture(out mat);

        public bool GetExposure(out double exposure) => _andor.GetExpose(out exposure);

        public bool GetFrameRate(out double frameRate) => _andor.GetFrameRate(out frameRate);

        public bool Init() => _andor.InitializeSdk() && _andor.InitializeCamera();

        public bool SaveCapture(string path) => _andor.SaveSingleFrame(path);

        public bool SetExposure(double exposure) => _andor.SetExposure(exposure);

        public bool StartCapture() => _andor.StartAcquisition();

        public bool StopCapture() => _andor.StopAcquisition();

        public bool AcqStartCommand() => _andor.AcqStartCommand();

        public bool AcqStopCommand() => _andor.AcqStopCommand();

        public bool Init(out List<string> devices)
        {
            throw new System.NotImplementedException();
        }

        ~AndorCamera()
        {
            _andor.UnInitializeCamera();
            _andor.UninitializeSdk();
        }
    }
}

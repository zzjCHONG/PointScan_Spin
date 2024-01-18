using OpenCvSharp;

namespace Simscop.API
{
    public class AndorIntImp : ICamera
    {
        Andor _andor = new Andor();
        public bool Capture(out Mat mat) => _andor.Capture(out mat);

        public bool GetExposure(out double exposure) => _andor.GetExpose(out exposure);

        public bool Init() => _andor.InitializeSdk() && _andor.InitializeCamera();

        public bool SaveCapture(string path) => _andor.SaveSingleFrame(path);

        public bool SetExposure(double exposure) => _andor.SetExposure(exposure);

        public bool StartCapture() => _andor. StartAcquisition();

        public bool StopCapture() => _andor.StopAcquisition();

        ~AndorIntImp()
        {
            _andor.UnInitializeCamera();
            _andor.UninitializeSdk();
        }
        
    }
}

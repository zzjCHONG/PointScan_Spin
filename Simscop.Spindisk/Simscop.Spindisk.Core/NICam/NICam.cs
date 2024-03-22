using OpenCvSharp;
using Simscop.API;
using Simscop.Spindisk.Core.NICamera;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.Models.NIDevice
{
    public class NICam : ICamera
    {
        NI _NICam = new();

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

    public class NI
    {
        public Card? Card { get; set; }
           
        public NI()
        {
            Card = new Card();
        }

        public bool Init(out List<string> devices)
        {
            devices=new List<string>(); 
            if (Card == null) return false;
            if (!Card.GetDevices(out devices) || devices.Count == 0)
            {
                _connectState = "NICamera no found";
                return false;
            }
            _connectState = "Initialize NICamera completed";
            return true;
        }

        public bool Capture(out Mat mat)
        {
            mat=new Mat();
            if (Card == null) return false;
            Card.StartTask(1);
            Card.StartTask(0);
            Card.WaitUntilDone(0, 0);
            Card.WaitUntilDone(1, 0);

            if (!Card.ReadImage(out List<Mat> mats) || mats.Count == 0) return false;

            mat = mats[0];

            CurrentFrameforSaving?.Dispose();//save
            CurrentFrameforSaving = mat;

            Card.StopTask(0);
            Card.StopTask(1);
            return true;
        }

        public bool StartCapture()
        {
            Card?.RefrashWavePara();
            if (Card == null) return false;
            if (!Card.CreateAiTask()) return false;
            if (!Card.CreateAoTask()) return true;
            return true;
        }

        public bool StopCapture()
        {
            Task.Run(() =>
            {
                Card?.DisposeTask(0);
                Card?.DisposeTask(1);
            });
            return true;
        }

        #region 

        private Mat? CurrentFrameforSaving { get; set; }
        private string _connectState = string.Empty;
        public bool SaveCapture(string path)
        {
            if (CurrentFrameforSaving == null || CurrentFrameforSaving.Cols == 0 || CurrentFrameforSaving.Rows == 0)
            {
                Debug.WriteLine("Get Frame Error.————————Save");
                return false;
            }
           if(!CurrentFrameforSaving.SaveImage(path))return false;
            return true;
        }
        public string GetConnectState() => _connectState;
        public bool GetExposure(out double exposure)
        {
            exposure = 0;
            return true;
        }
        public bool GetFrameRate(out double frameRate)
        {
            frameRate = 0;
            return true;
        }
        public bool SetExposure(double exposure) => true;
        public bool AcqStartCommand() => true;
        public bool AcqStopCommand() => true;

        #endregion
    }


}

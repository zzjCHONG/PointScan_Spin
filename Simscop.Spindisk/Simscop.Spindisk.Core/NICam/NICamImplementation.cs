using OpenCvSharp;
using Simscop.Spindisk.Core.NICamera;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.Models.NIDevice
{
    public class NIImplementation
    {
        public Card? Card { get; set; }

        public NIImplementation()
        {
            Card = new Card();
        }

        private static bool isStopTask = true;

        public bool Init(out List<string> devices)
        {
            devices = new List<string>();
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
            mat = new Mat();
            List<Mat> mats = new List<Mat>();
            try
            {
                int times = 0;
                while (!isStopTask && times < 1)
                {
                    times++;
                    if (Card == null) return false;
                    if (!Card.StartTask(1)) return false;
                    if (!Card.StartTask(0)) return false;
                    if (!Card.WaitUntilDone(0, 0)) return false;
                    if (!Card.WaitUntilDone(1, 0)) return false;

                    if (Card.ImageDataOutput(out int x, out int y, out List<short[]> resultArrayList))
                    {
                        for (int i = 0; i < resultArrayList.Count; i++)
                        {
                            Mat matImage = new Mat(x, y, MatType.CV_16SC1);
                            Marshal.Copy(resultArrayList[i], 0, matImage.Data, x * y);
                            mats.Add(matImage);
                        }
                        if (mats.Count > 0) mat = mats[0];
                        CurrentFrameforSaving?.Dispose();
                        CurrentFrameforSaving = mat;
                    }

                    if (!Card.StopTask(0)) return false;
                    if (!Card.StopTask(1)) return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool StartCapture()
        {
            do
            {
                Thread.Sleep(100);
            } while (!isStopTask);
            isStopTask=false;//确保disposetask完成了再允许开启，待修改

            if (Card == null) return false;
            Card.RefrashWavePara();
            if (!Card.CreateAiTask()) return false;
            if (!Card.CreateAoTask()) return false;
            return true;
        }

        public bool StopCapture()
        {
            Task.Run(() =>
            {
                Card?.DisposeTask(0);
                Card?.DisposeTask(1);
                isStopTask = true;
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
            if (!CurrentFrameforSaving.SaveImage(path)) return false;
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

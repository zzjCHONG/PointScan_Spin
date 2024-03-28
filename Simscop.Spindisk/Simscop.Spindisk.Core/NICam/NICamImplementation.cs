using OpenCvSharp;
using Simscop.Spindisk.Core.NICamera;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.Models.NIDevice
{
    public class NIImplementation
    {
        private static bool isStopTask = true;
        private readonly object lockObject = new();
        static int times = 0;
        private string _connectState = string.Empty;
        public Card? Card { get; set; }
        private Mat? CurrentFrameforSaving { get; set; }
        public NIImplementation()
        {
            Config = new Config();
            Card = new Card();
        }
   
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
            if (isStopTask) return false;
            List<Mat> mats = new List<Mat>();
            try
            {
                
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
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool StartCapture()
        {
            if (times != 0)
            {
                lock (lockObject)
                {
                    while (!isStopTask)
                    {
                        if (Monitor.Wait(lockObject))
                        {
                            Debug.WriteLine("等待结束");
                            return false;
                            
                        }     
                    }          
                }
            }
            times++;
            isStopTask = false;
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
                lock (lockObject)
                {
                    isStopTask = true;            
                    Monitor.Pulse(lockObject);
                }
            });

            return true;
        }

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

        #region 

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

        private Config? Config { get; set; }
        public bool SetResolutionsRatio(int x, int y)
        {
            Config?.Write(ConfigSettingEnum.XPixelFactor, x);
            Config?.Write(ConfigSettingEnum.YPixelFactor, y);
            return true;
        }
        public bool SetWave(int value)
        {
            Config?.Write(ConfigSettingEnum.WaveMode, value);//0为锯齿波，1为三角波
            return true;
        }
        public bool SetDevice(string deviceName)
        {
            Config?.Write(ConfigSettingEnum.DeviceName, deviceName);
            return true;
        }
        public bool SetVoltageSweepRangeUpperLimit(int value)
        {
            Config?.Write(ConfigSettingEnum.maxXV, value);
            Config?.Write(ConfigSettingEnum.maxYV, value);
            return true;
        }
        public bool SetVoltageSweepRangeLowerLimit(int value)
        {
            Config?.Write(ConfigSettingEnum.minXV, value);
            Config?.Write(ConfigSettingEnum.minYV, value);
            return true;
        }
        public bool SetPixelDwelTime(int value)
        {
            Config?.Write(ConfigSettingEnum.PixelDwelTime, value);
            return true;
        }
        #endregion
    }
}

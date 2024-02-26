using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace Simscop.API
{

    public class Andor : ICamera
    {
        AndorImplemented _andor = new AndorImplemented();

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


        ~Andor()
        {
            _andor.UnInitializeCamera();
            _andor.UninitializeSdk();
        }
    }

    class AndorImplemented
    {
        internal string ConnectState = string.Empty;
        private static int Hndl = 0;
        private static int NumberDevices = 0;
        private static int ImageSizeBytes;

        #region AssertRet
        private bool AssertRet(int ret, bool assertInit = true, bool assertConnect = true)
        {
            if (assertInit && !IsInitialized()) return false;
            if (assertConnect && !IsConnected()) return false;

            var st = new StackTrace(true);
            var msg = AndorErrorCodeEnum.NO_DEFINE;
            msg = Enum.IsDefined(typeof(AndorErrorCodeEnum), ret) ? (AndorErrorCodeEnum)ret : AndorErrorCodeEnum.NO_DEFINE;
            if (ret != (int)AndorErrorCodeEnum.AT_SUCCESS)
            {
                ConnectState = $"{ret}-{msg}";
                Debug.WriteLine($"[ERROR] [{st?.GetFrame(1)?.GetMethod()?.Name}]{ConnectState}");
                return false;
            }
            return true;
        }

        private bool IsInitialized()
        {
            if (NumberDevices != 0) return true;
            ConnectState = "No camera found";
            Debug.WriteLine(ConnectState);
            return false;
        }

        private bool IsConnected()
        {
            if (Hndl == 0 || ImageSizeBytes == 0)
            {
                ConnectState = "No camera connected";
                Debug.WriteLine(ConnectState);
                return false;
            }
            return true;
        }
        #endregion

        #region Init
        /// <summary>
        /// 初始化Sdk
        /// </summary>
        /// <returns></returns>
        public bool InitializeSdk()
        {
            if (!AssertRet(AndorAPI.InitialiseLibrary(), false, false))
            {
                Debug.WriteLine("InitialiseLibrary Error");
                return false;
            }
            if (!AssertRet(AndorAPI.GetInt(1, "Device Count", ref NumberDevices), false, false)) return false;
            ConnectState = "InitializeSdk completed!";
            Debug.WriteLine(ConnectState);
            return true;
        }

        /// <summary>
        /// 释放SDK
        /// </summary>
        /// <returns></returns>
        public bool UninitializeSdk()
            => AssertRet(AndorAPI.FinaliseLibrary(), false, false);

        /// <summary>
        /// 初始化相机
        /// </summary>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        public bool InitializeCamera(int cameraId = 0)
        {
            if (Hndl > 0) return true;//已经成功获得相机句柄

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            if (!AssertRet(AndorAPI.Open(cameraId, ref Hndl), assertConnect: false))
            {
                Debug.WriteLine("OpenCamera Error");
                return false;
            }
            stopwatch.Stop();
            Debug.WriteLine($"Open camera cost {stopwatch.ElapsedMilliseconds}ms");
            if (!AssertRet(AndorAPI.GetInt(Hndl, "imageSizeBytes", ref ImageSizeBytes), assertConnect: false)) return false;

            //初始设置
            //Electronic Shuttering Mode：默认Rolling
            //Trigger Mode：无外接触发设备 Internal
            SetSpuriousNoiseFilter();//消除噪声
            SetPixelEncoding(PixelEncodingEnum.Mono16);//图像格式
            //SetOverlap();//曝光可以在之前一次曝光读出时就开始，高帧频，最小曝光时间将加至读出时间。当曝光时间小于读出时间,Overlap 模式不适用。添加后延时更高
            SetSimplePreAmpGainControl(SimplePreAmpGainControlEnum.LowNoiseandHighWellCapacity);//追求高动态范围选 16-bit; 追求采集速度选12-bit
            SetPixelReadoutRate(PixelReadoutRateEnum.OneHundredMHz);//采样率，只有100和280mhz
            SetCycleMode(CycleModeEnum.Continuous);//采集方式-连续触发
            SetExposure(50);//曝光

            AcqStartCommand();
            AcqStopCommand();//预热

            ConnectState = "Initialize camera completed!";
            Debug.WriteLine(ConnectState);

            return true;
        }

        /// <summary>
        /// 释放相机
        /// </summary>
        /// <returns></returns>
        public bool UnInitializeCamera()
            => AssertRet(AndorAPI.Close(Hndl), false, false);
        #endregion

        #region Setting

        private const double MaxExposure = 30;
        private const double MinExposure = 1.0 / 1000 / 10;//0.0001.unit,S

        /// <summary>
        /// 获取曝光值
        /// </summary>
        /// <param name="exposure"></param>
        /// <returns></returns>
        public bool GetExpose(out double exposure)
        {
            exposure = 0;
            bool isReadable = false;
            if (!AssertRet(AndorAPI.IsReadable(Hndl, "ExposureTime", ref isReadable))) return false;
            if (isReadable)
                if (!AssertRet(AndorAPI.GetFloat(Hndl, "ExposureTime", ref exposure))) return false;
            return true;
        }

        /// <summary>
        /// 获得帧率
        /// </summary>
        /// <param name="frameRate"></param>
        /// <returns></returns>
        public bool GetFrameRate(out double frameRate)
        {
            frameRate = 0;
            bool isReadable = false;
            if (!AssertRet(AndorAPI.IsReadable(Hndl, "FrameRate", ref isReadable))) return false;
            if (isReadable)
                if (!AssertRet(AndorAPI.GetFloat(Hndl, "FrameRate", ref frameRate))) return false;
            return true;
        }

        /// <summary>
        /// 设置曝光值
        /// </summary>
        /// <param name="exposure"></param>
        /// <returns></returns>
        public bool SetExposure(double exposure)
        {
            AcqStopCommand();
            exposure = exposure / 1000.0;
            exposure = exposure > MaxExposure ? MaxExposure : exposure;
            exposure = exposure < MinExposure ? MinExposure : exposure;

            if (!AssertRet(AndorAPI.SetFloat(Hndl, "ExposureTime", exposure))) return false;
            double max = 0;
            if (!AssertRet(AndorAPI.GetFloatMax(Hndl, "FrameRate", ref max))) return false;
            if (!AssertRet(AndorAPI.SetFloat(Hndl, "FrameRate", max))) return false;

            AcqStartCommand();
            return true;
        }

        /// <summary>
        /// 设置像素编码类型
        /// </summary>
        /// <param name="pixelEncoding"></param>
        /// <returns></returns>
        private bool SetPixelEncoding(PixelEncodingEnum pixelEncoding)
        {
            //AcqStopCommand();
            if (!AssertRet(AndorAPI.SetEnumString(Hndl, "PixelEncoding", pixelEncoding.ToString()))) return false;
            if (!AssertRet(AndorAPI.GetInt(Hndl, "imageSizeBytes", ref ImageSizeBytes))) return false;
            //AcqStartCommand();
            return true;
        }

        /// <summary>
        /// 设置采样率
        /// </summary>
        /// <param name="pixelReadoutRate"></param>
        /// <returns></returns>
        private bool SetPixelReadoutRate(PixelReadoutRateEnum pixelReadoutRate)
        {
            //AcqStopCommand();
            if (!AssertRet(AndorAPI.SetEnumIndex(Hndl, "PixelReadoutRate", (int)pixelReadoutRate))) return false;
            //AcqStartCommand();
            return true;
        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="cycleMode"></param>
        /// <returns></returns>
        private bool SetCycleMode(CycleModeEnum cycleMode)
        {
            //AcqStartCommand();
            if (!AssertRet(AndorAPI.SetEnumString(Hndl, "CycleMode", cycleMode.ToString()))) return false;
            //AcqStopCommand();
            return true;
        }

        /// <summary>
        /// 设置灵敏度与动态范围 
        /// Sensitivity/Dynamic Range；追求高动态范围请选 16-bit/追求采集速度请选 12-bit
        /// </summary>
        /// <param name="simplePreAmpGainControl"></param>
        /// <returns></returns>
        private bool SetSimplePreAmpGainControl(SimplePreAmpGainControlEnum simplePreAmpGainControl)
        {
            //AcqStopCommand();
            if (!AssertRet(AndorAPI.SetEnumIndex(Hndl, "SimplePreAmpGainControl", (int)simplePreAmpGainControl))) return false;
            //AcqStartCommand();
            return true;
        }

        /// <summary>
        /// 设置噪声滤波
        /// </summary>
        /// <returns></returns>
        private bool SetSpuriousNoiseFilter()
        {
            //AcqStartCommand();
            if (!AssertRet(AndorAPI.SetBool(Hndl, "SpuriousNoiseFilter", true))) return false;
            //AcqStopCommand();
            return true;
        }

        private bool SetOverlap()
        {
            //AcqStartCommand();
            if (!AssertRet(AndorAPI.SetBool(Hndl, "Overlap", true))) return false;
            //AcqStopCommand();
            return true;
        }

        #region Setting Test
        /// <summary>
        /// EnumTest
        /// </summary>
        /// <returns></returns>
        public bool EnumSettingDemo()
        {
            int i_retCode = 0;
            string PixelReadoutRate = "PixelReadoutRate";

            int i_index = 0;
            i_retCode = AndorAPI.GetEnumerated(Hndl, PixelReadoutRate, ref i_index);
            StringBuilder szValue = new StringBuilder(64);
            i_retCode = AndorAPI.GetEnumeratedstring(Hndl, PixelReadoutRate, i_index, szValue, 64);
            i_retCode = AndorAPI.SetEnumeratedString(Hndl, PixelReadoutRate, "200 MHz");

            string feature = "CycleMode";
            int value = 0;
            AndorAPI.GetEnumIndex(Hndl, feature, ref value);
            AndorAPI.SetEnumIndex(Hndl, feature, value);

            int count = 0;
            AndorAPI.GetEnumCount(Hndl, feature, ref count);

            int indextoGetEnum = 1;
            StringBuilder stringBuilder = new StringBuilder(64);
            AndorAPI.GetEnumStringByIndex(Hndl, feature, indextoGetEnum, stringBuilder, 64);
            AndorAPI.SetEnumString(Hndl, feature, stringBuilder.ToString());

            int AvailableIndex = 0;
            bool isAvailable = false;
            AndorAPI.IsEnumIndexAvailable(Hndl, feature, AvailableIndex, ref isAvailable);

            int IndexImplemented = 0;
            bool isImplemented = false;
            AndorAPI.IsEnumIndexImplemented(Hndl, feature, IndexImplemented, ref isImplemented);

            return true;
        }

        /// <summary>
        /// 获得Enum参数
        /// </summary>
        /// <returns></returns>
        public bool LoopGetEnum()
        {
            string feature = "CycleMode";

            //PixelReadoutRate,PixelEncoding,ElectronicShutteringMode,FanSpeed,PreAmpGainControl,SensorReadoutMode，
            //TemperatureControl，TemperatureStatus，ShutterOutputMode，SimplePreAmpGainControl，TriggerMode，CycleMode

            //int index = 0;
            //bool isAvailable = false;
            //if (!AssertRet(AndorAPI.IsEnumIndexAvailable(Hndl, feature, index, ref isAvailable))) return false;
            //bool isImplemented = false;
            //if (!AssertRet(AndorAPI.IsEnumIndexImplemented(Hndl, feature, index, ref isImplemented))) return false;

            int count = 0;
            if (!AssertRet(AndorAPI.GetEnumCount(Hndl, feature, ref count))) return false;

            StringBuilder szValue = new StringBuilder(64);
            StringBuilder szValuebyIndex = new StringBuilder(64);
            Debug.WriteLine(feature);
            for (int i = 0; i < count; i++)
            {
                //if (!AssertRet(AndorAPI.GetEnumeratedstring(Hndl, feature, i, szValue, 64))) return false;
                //if (!AssertRet(AndorAPI.GetEnumStringByIndex(Hndl, feature, i, szValuebyIndex, 64))) return false;
                AndorAPI.GetEnumeratedstring(Hndl, feature, i, szValue, 64);
                Debug.WriteLine($"{i}-{szValue.ToString()}");
                //Debug.WriteLine(szValuebyIndex.ToString());
            }
            return true;
        }

        /// <summary>
        /// 传感器冷却
        /// 降低噪音--无法消除亮点（热像素）
        /// </summary>
        /// <returns></returns>
        public bool SetsSensorCooling()
        {
            int recode = -1;
            recode = AndorAPI.SetBool(Hndl, "SensorCooling", true);
            int temperatureCount = 0;
            recode = AndorAPI.GetEnumCount(Hndl, "TemperatureControl", ref temperatureCount);
            AndorAPI.SetEnumIndex(Hndl, "TemperatureControl", temperatureCount - 1);
            int temperatureStatusIndex = 0;
            StringBuilder temperatureStatus = new StringBuilder(256);
            int times = 0;
            do
            {
                times++;
                recode = AndorAPI.GetEnumIndex(Hndl, "TemperatureStatus", ref temperatureStatusIndex);
                recode = AndorAPI.GetEnumStringByIndex(Hndl, "TemperatureStatus", temperatureStatusIndex,
                temperatureStatus, 256);
                if (temperatureStatus.ToString() == "Cooler Off") return false;
                Debug.WriteLine($"{times}---{temperatureStatus}");
                Thread.Sleep(2000);
            }
            while (string.Compare("Stabilised", temperatureStatus.ToString()) != 0);
            return true;
        }
        #endregion  

        #endregion

        #region Save
        public Mat? CurrentFrameforSaving { get; set; }

        /// <summary>
        /// 单张存图
        /// </summary>
        /// <returns></returns>
        public bool SaveSingleFrame(string path)
        {
            if (CurrentFrameforSaving == null || CurrentFrameforSaving.Cols == 0 || CurrentFrameforSaving.Rows == 0)
                Debug.WriteLine("Get Frame Error.————————Save");

            if (!MatSave(CurrentFrameforSaving, path)) return false;

            return true;
        }

        /// <summary>
        /// MatSave in Andor
        /// Rotate&&Flip
        /// 非tif格式，存图异常
        /// </summary>
        /// <param name="matImg"></param>
        /// <param name="imageFilepath"></param>
        /// <returns></returns>
        private bool MatSave(Mat? matImg, string imageFilepath)
        {
            try
            {
                if (string.IsNullOrEmpty(imageFilepath)) return false;
                if (matImg == null) return false;

                //旋转
                Mat matImgRotate = new Mat(matImg.Height, matImg.Width, matImg.Type());
                Cv2.Rotate(matImg, matImgRotate, RotateFlags.Rotate180);
                Mat matImgFlip = new Mat(matImg.Height, matImg.Width, matImg.Type());
                Cv2.Flip(matImgRotate, matImgFlip, FlipMode.Y);

                //图像水平&垂直分辨率、压缩比
                ImwriteFlags flags = ImwriteFlags.TiffCompression;
                ImwriteFlags dpix = ImwriteFlags.TiffXDpi;
                ImwriteFlags dpiy = ImwriteFlags.TiffYDpi;
                ImageEncodingParam[] encodingParams = new ImageEncodingParam[] { new ImageEncodingParam(dpix, 96), new ImageEncodingParam(dpiy, 96), new ImageEncodingParam(flags, 1) };

                //if (!Directory.Exists(imageFilepath)) Directory.CreateDirectory(imageFilepath);
                string imageFile = System.IO.Path.Combine(imageFilepath, $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}.tif");
                if (!Cv2.ImWrite(imageFile, matImgFlip, encodingParams)) return false;

                Debug.WriteLine("MatImage save:" + imageFile);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MatImage save error:" + ex.Message);
                return false;
            }
        }

        #endregion

        #region Capture

        private const int QueueCount = 5;
        private static int CapQueIndex = 0;
        private const int ImageHeight = 2160;
        private const int ImageWidth = 2560;
        public static byte[]?[] AlignedBuffers;
        private static IntPtr GlobalFramePtr = IntPtr.Zero;
        private int times = 0;

        /// <summary>
        /// 图像捕获
        /// </summary>
        /// <param name="matImg"></param>
        /// <returns></returns>
        public bool Capture(out Mat matImg)
        {
            matImg = new Mat();
            if (!GetCircularFrame(out matImg, interval: 1000)) return false;
            matImg.MinMaxLoc(out double min, out double max);
            if (matImg == null || min == 0 || max == 0)
            {
                Debug.WriteLine($"matImg is null.min {min} - max {max}");
                return false;
            }
            CurrentFrameforSaving?.Dispose();
            CurrentFrameforSaving = matImg;
            return true;
        }

        /// <summary>
        /// 循环获得图像
        /// </summary>
        /// <param name="pixelEncoding"></param>
        /// <param name="matImg"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private bool GetCircularFrame(out Mat matImg, PixelEncodingEnum pixelEncoding = PixelEncodingEnum.Mono16, uint interval = unchecked(0xFFFFFFFF))
        {
            matImg = null;
            byte[]? imageBytes = new byte[ImageSizeBytes];
            GCHandle handle = GCHandle.Alloc(imageBytes, GCHandleType.Pinned);
            try
            {
                //1-获取buffer
                GlobalFramePtr = new IntPtr(imageBytes.Length);
                GlobalFramePtr = handle.AddrOfPinnedObject();
                int bufferSize = 0;
                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Start();
                if (!AssertRet(AndorAPI.WaitBuffer(Hndl, ref GlobalFramePtr, ref bufferSize, interval)))//断联，超时，连续多次触发
                {
                    Debug.WriteLine("Camera recontect...");
                    AcqStopCommand();
                    AcqStartCommand();
                    return false;
                }
                //stopwatch.Stop();
                //Debug.WriteLine($"WaitBuffer cost time:{stopwatch.ElapsedMilliseconds}ms");

                //2-转换Mat
                MatType matType = new MatType();
                switch (pixelEncoding)
                {
                    case PixelEncodingEnum.Mono8:
                        matType = MatType.CV_8UC1;
                        break;
                    case PixelEncodingEnum.Mono12:
                        matType = MatType.CV_16UC1;
                        break;
                    case PixelEncodingEnum.Mono12Packed:
                        matType = MatType.CV_16UC1;
                        break;
                    case PixelEncodingEnum.Mono16:
                        matType = MatType.CV_16UC1;
                        break;
                    case PixelEncodingEnum.Mono32:
                        matType = MatType.CV_32SC1;
                        break;
                    default:
                        Debug.WriteLine("Invalid pixel format.");
                        return false;
                }
                matImg = new Mat(ImageHeight, ImageWidth, matType);
                Marshal.Copy(GlobalFramePtr, imageBytes, 0, imageBytes.Length);
                Marshal.Copy(imageBytes, 0, matImg.Data, imageBytes.Length);

                //matImg.Flip(FlipMode.X);
                //Cv2.Flip(matImg, matImg, FlipMode.X);//图像翻转

                //4-Re-queue the buffers
                if (AlignedBuffers == null)
                {
                    Debug.WriteLine("AlignedBuffers is null");
                    return false;
                }

                if (!AssertRet(AndorAPI.QueueBuffer(Hndl, AlignedBuffers[CapQueIndex % QueueCount], ImageSizeBytes))) return false;

                CapQueIndex++;
                if (CapQueIndex > 750)
                    CapQueIndex = 0;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetCurrentFrame Error:" + ex.Message);
                return false;
            }
            finally
            {
                imageBytes = null;
                handle.Free();
            }
        }

        /// <summary>
        /// 开始捕获
        /// </summary>
        /// <returns></returns>
        public bool StartAcquisition()
        {
            int numberOfBuffers = QueueCount;
            byte[][]? AcqBuffers = new byte[numberOfBuffers][];
            AlignedBuffers = new byte[numberOfBuffers][];
            int bytesNum = ImageSizeBytes + 7;
            for (int i = 0; i < numberOfBuffers; i++)
            {
                AcqBuffers[i] = new byte[bytesNum];
                AlignedBuffers[i] = new byte[bytesNum];
                Buffer.BlockCopy(AcqBuffers[i % numberOfBuffers], 0, AlignedBuffers[i], 0, bytesNum);

                if (!AssertRet(AndorAPI.QueueBuffer(Hndl, AlignedBuffers[i], ImageSizeBytes))) return false;
            }
            AcqStartCommand();
            AcqBuffers = null;
            return true;
        }

        /// <summary>
        /// 停止捕获
        /// </summary>
        /// <param name="imageSizeBytes"></param>
        /// <returns></returns>
        public bool StopAcquisition()
        {
            try
            {
                if (!AcqStopCommand())
                {
                    Debug.WriteLine("StopAcquisition failed. ");

                    System.Windows.MessageBox.Show("相机停止采集失败，相机或被意外关闭。请确认！", "错误", MessageBoxButton.OK);
                    return false;
                }                   
                if (!AssertRet(AndorAPI.Flush(Hndl))) return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                AlignedBuffers = null;
                
            }
        }

        /// <summary>
        /// 开始采集命令
        /// </summary>
        /// <returns></returns>
        public bool AcqStartCommand() => AssertRet(AndorAPI.Command(Hndl, "Acquisition Start"));

        /// <summary>
        /// 停止采集命令
        /// </summary>
        /// <returns></returns>
        public bool AcqStopCommand() => AssertRet(AndorAPI.Command(Hndl, "Acquisition Stop"));

        #endregion

    }
}

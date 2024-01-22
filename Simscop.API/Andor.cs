using OpenCvSharp;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Simscop.API
{

    public class Andor : ICamera
    {
        AndorImplemented _andor = new AndorImplemented();
        public bool Capture(out Mat mat) => _andor.Capture(out mat);

        public bool GetExposure(out double exposure) => _andor.GetExpose(out exposure);

        public bool Init() => _andor.InitializeSdk() && _andor.InitializeCamera();

        public bool SaveCapture(string path) => _andor.SaveSingleFrame(path);

        public bool SetExposure(double exposure) => _andor.SetExposure(exposure);

        public bool StartCapture() => _andor.StartAcquisition();

        public bool StopCapture() => _andor.StopAcquisition();

        ~Andor()
        {
            _andor.UnInitializeCamera();
            _andor.UninitializeSdk();
        }
    }

    class AndorImplemented
    {
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
                Debug.WriteLine($"[ERROR] [{st?.GetFrame(1)?.GetMethod()?.Name}] {ret}-{msg}");
                return false;
            }

            Debug.WriteLine($"[INFO] [{st?.GetFrame(1)?.GetMethod()?.Name}] {ret}-{msg}");
            return true;
        }
        private bool IsInitialized()
        {
            if (NumberDevices != 0) return true;
            Debug.WriteLine("No camera found");
            return false;
        }
        private bool IsConnected()
        {
            if (Hndl == 0 || ImageSizeBytes == 0)
            {
                Debug.WriteLine("No camera connected");
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

            Debug.WriteLine("InitializeSdk completed!");
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
            if (!AssertRet(AndorAPI.Open(cameraId, ref Hndl), assertConnect: false)) throw new Exception("OpenCamera Error");
            if (!AssertRet(AndorAPI.GetInt(Hndl, "imageSizeBytes", ref ImageSizeBytes), assertConnect: false)) return false;

            //初始设置
            SetPixelEncoding(PixelEncodingEnum.Mono16);//默认格式Mono12PACKED
            SetPixelReadoutRate(100);
            SetCycleMode(CycleModeEnum.Continuous);//必需
            SetExposure(50);
            Debug.WriteLine("InitializeCamera completed!");
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
        private const double MinExposure = 1.0 / 1000 / 10;//0.0001

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
                if (!AssertRet(AndorAPI.GetFloat(Hndl, "Exposure Time", ref exposure))) return false;
            Debug.WriteLine($"++++++++++++++++++++++++++++ExposureTime-         GetExpose{exposure}");

            double rate = 0;
            if (!AssertRet(AndorAPI.GetFloat(Hndl, "FrameRate", ref rate))) return false;
            Debug.WriteLine($"++++++++++++++++++++++++++++FrameRate-         FrameRate{rate}");

            return true;
        }

        /// <summary>
        /// 设置曝光值
        /// </summary>
        /// <param name="exposure"></param>
        /// <returns></returns>
        public bool SetExposure(double exposure)
        {
            exposure = exposure / 1000.0;

            exposure = exposure > MaxExposure ? MaxExposure : exposure;
            exposure = exposure < MinExposure ? MinExposure : exposure;

            double rateold = 0;
            if (!AssertRet(AndorAPI.GetFloat(Hndl, "FrameRate", ref rateold))) return false;
            Debug.WriteLine($"++++++++++++++++++++++++++++FrameRate-         old{rateold}");

            //设置曝光
            bool isWritable = false;
            if (!AssertRet(AndorAPI.IsWritable(Hndl, "ExposureTime", ref isWritable))) return false;
            if (isWritable)
                if (!AssertRet(AndorAPI.SetFloat(Hndl, "Exposure Time", exposure))) return false;
            Debug.WriteLine($"++++++++++++++++++++++++++++ExposureTime-            now{exposure}");

            double max = 0;
            if (!AssertRet(AndorAPI.GetFloatMax(Hndl, "FrameRate", ref max))) return false;
            Debug.WriteLine($"++++++++++++++++++++++++++++FrameRate--         max{max}");

            if (!AssertRet(AndorAPI.SetFloat(Hndl, "FrameRate",  max))) return false;
            Debug.WriteLine($"++++++++++++++++++++++++++++FrameRate--       Set  max{max}");

            double rate = 0;
            if (!AssertRet(AndorAPI.GetFloat(Hndl, "FrameRate", ref rate))) return false;
            Debug.WriteLine($"++++++++++++++++++++++++++++FrameRate-            new{rate}");

            Debug.WriteLine("####################################Set Compelte!");
            return true;
        }

        /// <summary>
        /// 设置像素编码类型
        /// </summary>
        /// <param name="pixelEncoding"></param>
        /// <returns></returns>
        private bool SetPixelEncoding(PixelEncodingEnum pixelEncoding)
        {
            bool isWritable = false;
            if (!AssertRet(AndorAPI.IsWritable(Hndl, "PixelEncoding", ref isWritable))) return false;
            if (isWritable)
                if (!AssertRet(AndorAPI.SetEnumeratedString(Hndl, "PixelEncoding", pixelEncoding.ToString()))) return false;
            if (!AssertRet(AndorAPI.GetInt(Hndl, "imageSizeBytes", ref ImageSizeBytes))) return false;

            return true;
        }

        /// <summary>
        /// 设置采样率
        /// </summary>
        /// <param name="pixelReadoutRate"></param>
        /// <returns></returns>
        private bool SetPixelReadoutRate(int pixelReadoutRate)
        {
            bool isWritable = false;
            if (!AssertRet(AndorAPI.IsWritable(Hndl, "PixelReadoutRate", ref isWritable))) return false;
            if (isWritable)
                if (!AssertRet(AndorAPI.SetEnumeratedString(Hndl, "PixelReadoutRate", $"{pixelReadoutRate} MHz"))) return false;
            return true;
        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="cycleMode"></param>
        /// <returns></returns>
        public bool SetCycleMode(CycleModeEnum cycleMode)
        {
            bool isWritable = false;
            if (!AssertRet(AndorAPI.IsWritable(Hndl, "PixelReadoutRate", ref isWritable))) return false;
            if (isWritable)
                if (!AssertRet(AndorAPI.SetEnumeratedString(Hndl, "CycleMode", cycleMode.ToString()))) return false;
            return true;
        }
        #endregion

        #region Save
        public Mat? CurrentFrameforSaving { get; set; }

        /// <summary>
        /// 单张存图
        /// </summary>
        /// <returns></returns>
        public bool SaveSingleFrame(string path)
        {
            Debug.WriteLine("##Save");
            //if (!Capture(out Mat? matImg)) return false;
            if (CurrentFrameforSaving == null || CurrentFrameforSaving.Cols == 0 || CurrentFrameforSaving.Rows == 0)
                Debug.WriteLine("Get Frame Error.————————Save");

            if (!MatSave(CurrentFrameforSaving, path)) return false;
            Debug.WriteLine("***********************************************Save complete!");

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
                string imageFile = Path.Combine(imageFilepath, $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}.tif");
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

        private const int QueueCount = 15;
        private static int QueueIndex = 0;
        private const int ImageHeight = 2160;
        private const int ImageWidth = 2560;
        public static byte[][]? AlignedBuffers;
        private static IntPtr GlobalFramePtr = IntPtr.Zero;

        /// <summary>
        /// 图像捕获
        /// </summary>
        /// <param name="matImg"></param>
        /// <returns></returns>
        public bool Capture(out Mat? matImg)
        {
            matImg = new Mat();
            Debug.WriteLine("##Cupture");

            //获取图像
            if (!GetCircularFrame(out matImg, PixelEncodingEnum.Mono16, 10000)) return false;

            CurrentFrameforSaving?.Dispose();
            CurrentFrameforSaving = matImg;

            if (!GetExpose(out double exposure)) return false;

            Debug.WriteLine("-----------------------------------------------");

            return true;
        }

        /// <summary>
        /// 循环获得图像
        /// </summary>
        /// <param name="pixelEncoding"></param>
        /// <param name="matImg"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private bool GetCircularFrame( out Mat? matImg, PixelEncodingEnum pixelEncoding = PixelEncodingEnum.Mono16, uint interval = unchecked(0xFFFFFFFF))
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            matImg = null;
            byte[]? imageBytes = new byte[ImageSizeBytes];
            GCHandle handle = GCHandle.Alloc(imageBytes, GCHandleType.Pinned);
            try
            {
                GlobalFramePtr = new IntPtr(imageBytes.Length);
                GlobalFramePtr = handle.AddrOfPinnedObject();
                int bufferSize = 0;
                stopwatch.Start();
                AndorAPI.WaitBuffer(Hndl, ref GlobalFramePtr, ref bufferSize, interval);
                stopwatch.Stop();
                Debug.WriteLine($"WaitBuffer taste time:{stopwatch.ElapsedMilliseconds}ms");

                MatType matType = new MatType();
                switch (pixelEncoding)
                {
                    case PixelEncodingEnum.Mono8:
                        matType = MatType.CV_8UC1;
                        break;
                    case PixelEncodingEnum.Mono12:
                        matType = MatType.CV_16UC1;
                        break;
                    case PixelEncodingEnum.Mono12PACKED:
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

                matImg.MinMaxLoc(out double min, out double max);
                Debug.WriteLine($"{min}-----{max}");

                AndorAPI.QueueBuffer(Hndl, AlignedBuffers[QueueIndex % QueueCount], ImageSizeBytes);
                QueueIndex++;

                //if (QueueIndex % ((QueueCount - 1) * 10) == 0)
                //{
                //    StopAcquisition();
                //    StartAcquisition();
                //}

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

        public bool AcquisitionStart() => AssertRet(AndorAPI.Command(Hndl, "AcquisitionStart"));
        public bool AcquisitionStop()=> AssertRet(AndorAPI.Command(Hndl, "Acquisition Stop"));

        /// <summary>
        /// 开始捕获
        /// </summary>
        /// <returns></returns>
        public bool StartAcquisition()
        {
            int numberOfBuffers = QueueCount;
            byte[][]? AcqBuffers = new byte[numberOfBuffers][];
            AlignedBuffers = new byte[numberOfBuffers][];
            for (int i = 0; i < numberOfBuffers; i++)
            {
                AcqBuffers[i] = new byte[ImageSizeBytes + 7];
                AlignedBuffers[i] = new byte[ImageSizeBytes + 7];
                Buffer.BlockCopy(AcqBuffers[i % numberOfBuffers], 0, AlignedBuffers[i], 0, ImageSizeBytes + 7);

                if (!AssertRet(AndorAPI.QueueBuffer(Hndl, AlignedBuffers[i], ImageSizeBytes))) return false;
            }

            Debug.WriteLine("##AcquisitionStart");
            AssertRet(AndorAPI.Command(Hndl, "AcquisitionStart"));

            return true;
        }

        /// <summary>
        /// 停止捕获
        /// </summary>
        /// <param name="imageSizeBytes"></param>
        /// <returns></returns>
        public bool StopAcquisition()
        {
            Debug.WriteLine("##AcquisitionStop");
            AssertRet(AndorAPI.Command(Hndl, "Acquisition Stop"));

            Debug.WriteLine("##Flush");
            if (!AssertRet(AndorAPI.Flush(Hndl))) return false;

            AlignedBuffers = null;//新增数组的释放

            return true;
        }

        #endregion

    }
}

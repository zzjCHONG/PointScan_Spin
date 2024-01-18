using OpenCvSharp;
using System;
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

    public class AndorImplemented
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
            if (AndorAPI.InitialiseLibrary() != (int)AndorErrorCodeEnum.AT_SUCCESS)
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
            SetCycleMode(CycleModeEnum.Continuous);

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
        public bool GetExpose(out double exposure)
        {
            exposure = 0;

            bool isReadable = false;
            if (!AssertRet(AndorAPI.IsReadable(Hndl, "ExposureTime", ref isReadable))) return false;

            if(isReadable)
                if (!AssertRet(AndorAPI.GetFloat(Hndl, "Exposure Time",ref exposure))) return false;
            return true;
        }

        /// <summary>
        /// 设置曝光值
        /// </summary>
        /// <param name="exposure"></param>
        /// <returns></returns>
        public bool SetExposure(double exposure)
        {
            double exposureTran = exposure / 1000;

            double maxValue = 0;
            if (!AssertRet(AndorAPI.GetFloatMax(Hndl, "ExposureTime", ref maxValue))) return false;
            double minValue = 0;
            if (!AssertRet(AndorAPI.GetFloatMin(Hndl, "ExposureTime", ref minValue))) return false;
            if (exposureTran > maxValue || exposureTran < minValue)
            {
                Debug.WriteLine($"Exposure exposure-{exposureTran} is out range:[{minValue},{maxValue}]");
                return false;
            }
            bool isWritable = false;
            if (!AssertRet(AndorAPI.IsWritable(Hndl, "ExposureTime", ref isWritable))) return false;

            if (isWritable)
                if (!AssertRet(AndorAPI.SetFloat(Hndl, "Exposure Time", exposureTran))) return false;

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
                if (!AssertRet(AndorAPI.SetEnumeratedString(Hndl, "PixelReadoutRate", $"{pixelReadoutRate} MHz")))
                    return false;

            return true;

        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="cycleMode"></param>
        /// <returns></returns>
        private bool SetCycleMode(CycleModeEnum cycleMode)
        {
            bool isWritable = false;
            if (!AssertRet(AndorAPI.IsWritable(Hndl, "PixelReadoutRate", ref isWritable))) return false;
            if (isWritable)
                if (!AssertRet(AndorAPI.SetEnumeratedString(Hndl, "CycleMode", cycleMode.ToString()))) 
                    return false;
            return true;
        }
        #endregion

        #region Save

        /// <summary>
        /// 单张存图
        /// </summary>
        /// <returns></returns>
        public bool SaveSingleFrame(string path)
        {
            Debug.WriteLine("##Save");
            if (!Capture(out Mat? matImg)) return false;

            if (matImg == null || matImg.Cols == 0 || matImg.Rows == 0)
                Debug.WriteLine("Get Frame Error.————————Save");

            //matImg.Normalize();
            //matImg.MinMaxLoc(out double min, out double max);
            //Debug.WriteLine($"************Save:{min}-----{max}");

            if (!MatSave(matImg, path)) return false;
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

        /// <summary>
        /// 图像捕获
        /// </summary>
        /// <param name="matImg"></param>
        /// <returns></returns>
        public bool Capture(out Mat? matImg)
        {
            Debug.WriteLine("##Cupture");

            //停止捕获
            StopAcquisition();

            matImg = new Mat();

            //开始捕获
            StartAcquisition();

            //获取图像
            GetCurrentFrame(PixelEncodingEnum.Mono16, out matImg);

            ////停止捕获
            //StopAcquisition();

            return true;
        }

        /// <summary>
        /// 开始捕获
        /// </summary>
        /// <returns></returns>
        public bool StartAcquisition()
        {
            int NumberOfBuffers = 10;
            byte[][]? AcqBuffers = new byte[NumberOfBuffers][];
            byte[][]? AlignedBuffers = new byte[NumberOfBuffers][];
            for (int i = 0; i < NumberOfBuffers; i++)
            {
                AcqBuffers[i] = new byte[ImageSizeBytes + 7];
                AlignedBuffers[i] = (byte[])Array.CreateInstance(typeof(byte), ImageSizeBytes + 7);
                Buffer.BlockCopy(AcqBuffers[i % NumberOfBuffers], 0, AlignedBuffers[i], 0, ImageSizeBytes + 7);

                if (!AssertRet(AndorAPI.QueueBuffer(Hndl, AlignedBuffers[i], ImageSizeBytes))) return false;
            }

            Debug.WriteLine("##AcquisitionStart");
            AssertRet(AndorAPI.Command(Hndl, "AcquisitionStart"));

            AcqBuffers = null;
            AlignedBuffers=null;

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

            return true;
        }

        /// <summary>
        /// 获得当前帧
        /// </summary>
        /// <param name="pixelEncoding"></param>
        /// <param name="matImg"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool GetCurrentFrame(PixelEncodingEnum pixelEncoding, out Mat matImg, uint interval = unchecked(0xFFFFFFFF))
        {
            try
            {
                matImg = new Mat();
                GetFrameBytes(PixelEncodingEnum.Mono16, out byte[]? imageBytes);

                Bytes2Mat(imageBytes, pixelEncoding, out matImg);

                if (matImg == null || matImg.Cols == 0 || matImg.Rows == 0)
                    Debug.WriteLine("Get Frame Error.");//throw new Exception("Get Frame Error.");

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("GetCurrentFrame Error:" + ex.Message);
            }
        }

        /// <summary>
        /// Byte[] to Mat
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="matImg"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private bool Bytes2Mat(byte[] imageBytes, PixelEncodingEnum pixelEncoding, out Mat matImg)
        {
            int imageHeight = 0;
            AndorAPI.GetInt(Hndl, "AOI Height", ref imageHeight);
            int imageWidth = 0;
            AndorAPI.GetInt(Hndl, "AOI Width", ref imageWidth);
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
                    throw new ArgumentException("Invalid pixel format.");
            }
            matImg = new Mat(imageHeight, imageWidth, matType);
            Marshal.Copy(imageBytes, 0, matImg.Data, imageBytes.Length);

            return true;
        }

        /// <summary>
        /// 获得当前帧
        /// </summary>
        /// <param name="imageSizeBytes"></param>
        /// <param name="imageBytes"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private bool GetFrameBytes(PixelEncodingEnum pixelEncoding, out byte[] imageBytes, uint interval = unchecked(0xFFFFFFFF))
        {
            try
            {
                //    byte[]? imageBytesTemp = new byte[ImageSizeBytes];

                imageBytes = new byte[ImageSizeBytes];
                IntPtr imgPtr = Marshal.AllocHGlobal(imageBytes.Length);
                if (!AssertRet(AndorAPI.WaitBuffer(Hndl, ref imgPtr, ref ImageSizeBytes, interval))) return false;

                Marshal.Copy(imgPtr, imageBytes, 0, imageBytes.Length);
                //Marshal.Copy(imgPtr, imageBytesTemp, 0, imageBytes.Length);
                //Buffer.BlockCopy(imageBytesTemp, 0, imageBytes, 0, imageBytesTemp.Length);
                //imageBytesTemp = null;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("GetCurrentFrame Error:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取缓存直至接收到完整图像
        /// </summary>
        /// <param name="isContinuous"></param>
        /// <param name="matImg"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private bool WaitBuffer(bool isContinuous, out Mat matImg, uint interval = unchecked(0xFFFFFFFF))
        {
            matImg = new Mat();

            if (isContinuous) SetCycleMode(CycleModeEnum.Continuous);
            GetFrameBytes(PixelEncodingEnum.Mono16, out byte[]? imageBytes);
            Bytes2Mat(imageBytes, PixelEncodingEnum.Mono16, out matImg);
            if (matImg == null || matImg.Cols == 0 || matImg.Rows == 0)
                Debug.WriteLine("Get Frame Error.");

            int NumberOfBuffers = 10;
            for (int i = 0; i < NumberOfBuffers; i++)
            {
                if (!AssertRet(AndorAPI.QueueBuffer(Hndl, new byte[ImageSizeBytes], ImageSizeBytes))) return false;
            }

            return true;
        }

        #endregion

    }
}



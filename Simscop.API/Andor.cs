using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Simscop.API;

public class Andor
{
    private BackgroundWorker LiveWorker = null;
    private static int Hndl = 0;
    private static int NumberDevices = 0;
    private PixelEncodingEnum PixelEncoding = PixelEncodingEnum.Mono16;

    private int imageSizeBytes;
    public int ImageSizeBytes => imageSizeBytes;

    private string imageFilePath = string.Empty;
    public string ImageFilePath
    {
        get => imageFilePath;
        set => imageFilePath = value;
    }

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
        if (Hndl == 0 || imageSizeBytes == 0)
        {
            Debug.WriteLine("No camera connected");
            return false;
        }
        return true;
    }
    #endregion

    /// <summary>
    /// 初始化Sdk
    /// </summary>
    /// <returns></returns>
    public bool InitializeSdk()
    {
        if (AndorAPI.InitialiseLibrary() != (int)AndorErrorCodeEnum.AT_SUCCESS) return false;

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
    public bool InitializeCamera(int cameraId)
    {
        if (!AssertRet(AndorAPI.Open(cameraId, ref Hndl), assertConnect: false)) return false;
        if (!AssertRet(AndorAPI.GetInt(Hndl, "imageSizeBytes", ref imageSizeBytes), assertConnect: false)) return false;

        Debug.WriteLine("InitializeCamera completed!");
        return true;
    }

    /// <summary>
    /// 释放相机
    /// </summary>
    /// <returns></returns>
    public bool UnInitializeCamera()
        => AssertRet(AndorAPI.Close(Hndl), false, false);

    /// <summary>
    /// 开始捕获
    /// </summary>
    /// <returns></returns>
    public bool StartCapture()
    {
        byte[] userBuffer = new byte[imageSizeBytes];
        if (!AssertRet(AndorAPI.QueueBuffer(Hndl, userBuffer, imageSizeBytes))) return false;

        if (!AssertRet(AndorAPI.Command(Hndl, "AcquisitionStart"))) return false;

        return true;
    }

    /// <summary>
    /// 停止捕获
    /// </summary>
    /// <param name="imageSizeBytes"></param>
    /// <returns></returns>
    public bool StopCapture()
    {
        if (!AssertRet(AndorAPI.Command(Hndl, "Acquisition Stop"))) return false;

        if (!AssertRet(AndorAPI.Flush(Hndl))) return false;

        return true;
    }

    /// <summary>
    /// 获得当前帧
    /// </summary>
    /// <param name="imageSizeBytes"></param>
    /// <param name="imageBytes"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public bool GetCurrentFrame(ref byte[] imageBytes, uint interval = unchecked(0xFFFFFFFF))
    {
        IntPtr imgPtr = Marshal.AllocHGlobal(imageBytes.Length);
        if (!AssertRet(AndorAPI.WaitBuffer(Hndl, ref imgPtr, ref imageSizeBytes, interval))) return false;
        Marshal.Copy(imgPtr, imageBytes, 0, imageBytes.Length);
        return true;
    }

    public Mat? GetCurrentFrame()
    {
        //获取图像->byte[] buffer
        byte[] pBuf = new byte[imageSizeBytes];
        if (!GetCurrentFrame(ref pBuf)) return null;

        Mat matImg = new Mat();
        Bytes2Mat(pBuf, ref matImg);

        return matImg;

    }

    /// <summary>
    /// Imagebyte[] to Mat
    /// </summary>
    /// <param name="imageBytes"></param>
    /// <param name="matImg"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool Bytes2Mat(byte[] imageBytes, ref Mat matImg)
    {
        int imageHeight = 0;
        AndorAPI.GetInt(Hndl, "AOI Height", ref imageHeight);
        int imageWidth = 0;
        AndorAPI.GetInt(Hndl, "AOI Width", ref imageWidth);
        MatType matType = new MatType();
        switch (PixelEncoding)
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
    /// MatImgSave in Andor
    /// Rotate&&Flip
    /// 非tif格式，存图异常
    /// </summary>
    /// <param name="matImg"></param>
    /// <param name="imageFileName"></param>
    /// <returns></returns>
    public bool MatSave(Mat matImg, string imageFileName)
    {
        try
        {
            if (string.IsNullOrEmpty(imageFileName)) return false;

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

            Cv2.ImWrite(imageFileName, matImgFlip, encodingParams);

            Debug.WriteLine("MatImage save:" + imageFileName);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MatImage save error:" + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 设置曝光值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetExposure(double value)
    {
        double maxValue = 0;
        if (!AssertRet(AndorAPI.GetFloatMax(Hndl, "ExposureTime", ref maxValue))) return false;
        double minValue = 0;
        if (!AssertRet(AndorAPI.GetFloatMin(Hndl, "ExposureTime", ref minValue))) return false;

        if (value > maxValue || value < minValue)
        {
            Debug.WriteLine($"Exposure value-{value} is out range:[{minValue},{maxValue}]");
            return false;
        }

        if (!AssertRet(AndorAPI.SetFloat(Hndl, "Exposure Time", value))) return false;
        return true;
    }

    /// <summary>
    /// 设置像素编码类型
    /// </summary>
    /// <param name="pixelEncoding"></param>
    /// <returns></returns>
    public bool SetPixelEncoding(PixelEncodingEnum pixelEncoding)
    {
        if (!AssertRet(AndorAPI.SetEnumeratedString(Hndl, "PixelEncoding", pixelEncoding.ToString()))) return false;
        if (AndorAPI.GetInt(Hndl, "imageSizeBytes", ref imageSizeBytes) != (int)AndorErrorCodeEnum.AT_SUCCESS) return false;
        PixelEncoding = pixelEncoding;

        return true;
    }

    /// <summary>
    /// 设置采样率
    /// </summary>
    /// <param name="pixelReadoutRate"></param>
    /// <returns></returns>
    public bool SetPixelReadoutRate(int pixelReadoutRate)
    {
        if (!AssertRet(AndorAPI.SetEnumeratedString(Hndl, "PixelReadoutRate", $"{pixelReadoutRate} MHz")))
            return false;

        return true;

    }

    /// <summary>
    /// 设置触发模式
    /// </summary>
    /// <param name="cycleMode"></param>
    /// <returns></returns>
    public bool SetCycleMode(CycleModeEnum cycleMode)
        => AssertRet(AndorAPI.SetEnumeratedString(Hndl, "CycleMode", cycleMode.ToString()));

    /// <summary>
    /// 单张采集
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public bool SaveCurrentFrame()
    {
        //开始捕获
        StartCapture();

        //获取图像->byte[] buffer
        byte[] pBuf = new byte[imageSizeBytes];
        if (!GetCurrentFrame(ref pBuf)) return false;

        Mat matImg = new Mat();
        Bytes2Mat(pBuf, ref matImg);

        //Show
        //.......

        //Save
        string imageFile = Path.Combine(imageFilePath, $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}.tif");
        MatSave(matImg, imageFile);

        //停止捕获
        StopCapture();

        return true;
    }

    /// <summary>
    /// Live存图
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void LiveSave_DoWork(object sender, DoWorkEventArgs e)
    {
        int times = 0;
        while (!LiveWorker.CancellationPending)
        {
            times++;
            SetExposure(times);

            ////开始捕获
            StartCapture();

            //获取图像
            byte[] pBuf = new byte[imageSizeBytes];
            if (!GetCurrentFrame(ref pBuf)) return;

            Mat matImg = new Mat();
            Bytes2Mat(pBuf, ref matImg);

            //Show
            //.......

            //Save
            string imageFileName = Path.Combine(imageFilePath, $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}.tif");
            MatSave(matImg, imageFileName);

            //停止捕获
            StopCapture();

            Thread.Sleep(5);
        }
    }

    /// <summary>
    /// 开始采集
    /// </summary>
    public void StartLive()
    {
        LiveWorker = new BackgroundWorker();
        LiveWorker.DoWork += LiveSave_DoWork;
        LiveWorker.WorkerSupportsCancellation = true;
        LiveWorker.RunWorkerAsync();
    }

    /// <summary>
    /// 结束采集
    /// </summary>
    public void StopLive()
    {
        LiveWorker.CancelAsync();
        LiveWorker.Dispose();
    }

    #region demo
    /// <summary>
    /// 多张采集并存图
    /// 按张数存储
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="imageFileName"></param>
    /// <param name="pixelEncoding"></param>
    /// <param name="numberOfBuffers"></param>
    /// <param name="numberOfFrames">存图的张数</param>
    /// <returns></returns>
    private bool SaveLiveImages()
    {
        if (!SetPixelEncoding(PixelEncodingEnum.Mono16)) return false;
        if (!SetPixelReadoutRate(100)) return false;
        if (!SetExposure(0.01)) return false;
        if (!SetCycleMode(CycleModeEnum.Continuous)) return false;

        //开始捕获
        int numberOfBuffers = 5;
        byte[][] acqBuffers = new byte[numberOfBuffers][];
        byte[][] alignedBuffers = new byte[numberOfBuffers][];
        for (int i = 0; i < numberOfBuffers; i++)
        {
            acqBuffers[i] = new byte[imageSizeBytes + 7];
            alignedBuffers[i] = (byte[])(Array.CreateInstance(typeof(byte), imageSizeBytes + 7));
            Buffer.BlockCopy(acqBuffers[i % numberOfBuffers], 0, alignedBuffers[i], 0, imageSizeBytes + 7);

            if (!AssertRet(AndorAPI.QueueBuffer(Hndl, alignedBuffers[i], imageSizeBytes))) return false;
        }
        if (!AssertRet(AndorAPI.Command(Hndl, "AcquisitionStart"))) return false;

        //获取图像
        int numberOfFrames = 50;
        for (int i = 0; i < numberOfFrames; i++)
        {
            byte[] pBuf = new byte[0];
            if (!GetCurrentFrame(ref pBuf)) return false;

            string imageFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AndorImage", $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}.tif");
            SaveCVImage(Hndl, pBuf, PixelEncoding, imageFileName);
            //Application specific data processing goes here..

            if (!AssertRet(AndorAPI.QueueBuffer(Hndl, alignedBuffers[i % numberOfBuffers], imageSizeBytes))) return false;
        }

        //停止捕获
        StopCapture();

        //buffer清除缓存
        for (int i = 0; i < numberOfBuffers; i++)
        {
            acqBuffers[i] = null;
        }
        alignedBuffers = null;
        acqBuffers = null;

        return true;
    }
    /// <summary>
    /// 存图
    /// 增加180旋转&&水平翻转
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="buffer"></param>
    /// <param name="pixelEncoding"></param>
    /// <param name="imageFileName"></param>
    /// <returns></returns>
    private bool SaveCVImage(int Hndl, byte[] buffer, PixelEncodingEnum pixelEncoding, string imageFileName)
    {
        try
        {
            int ImageHeight = 0;
            AndorAPI.GetInt(Hndl, "AOI Height", ref ImageHeight);
            int ImageWidth = 0;
            AndorAPI.GetInt(Hndl, "AOI Width", ref ImageWidth);
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
            Mat matImg = new Mat(ImageHeight, ImageWidth, matType);
            Marshal.Copy(buffer, 0, matImg.Data, buffer.Length);

            //旋转
            Mat matImgRotate = new Mat((int)ImageHeight, (int)ImageWidth, MatType.CV_16UC1);
            Cv2.Rotate(matImg, matImgRotate, RotateFlags.Rotate180);
            Mat matImgFlip = new Mat((int)ImageHeight, (int)ImageWidth, MatType.CV_16UC1);
            Cv2.Flip(matImgRotate, matImgFlip, FlipMode.Y);

            //图像水平&垂直分辨率、压缩比
            ImwriteFlags flags = ImwriteFlags.TiffCompression;
            ImwriteFlags dpix = ImwriteFlags.TiffXDpi;
            ImwriteFlags dpiy = ImwriteFlags.TiffYDpi;
            ImageEncodingParam[] encodingParams = new ImageEncodingParam[] { new ImageEncodingParam(dpix, 96), new ImageEncodingParam(dpiy, 96), new ImageEncodingParam(flags, 1) };

            Cv2.ImWrite(imageFileName, matImgFlip, encodingParams);
            Debug.WriteLine("Image save:" + imageFileName);

            return true;
        }
        catch (Exception ex)
        {

            Debug.WriteLine(ex.Message);
            return false;
        }
    }
    #endregion

}

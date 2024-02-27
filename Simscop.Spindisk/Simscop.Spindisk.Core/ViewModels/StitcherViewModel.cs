using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using System.Threading;
using Simscop.Spindisk.Core.Models;
using System.Windows;
using Simscop.Spindisk.Core.Messages;
using Simscop.API;
using System.Windows.Media.Imaging;

namespace Simscop.Spindisk.Core.ViewModels;

/// <summary>
/// note 图像中心点的位置定义为当前电动台给的坐标
/// </summary>
public partial class StitcherViewModel : ObservableObject
{
    ASIMotor _motor;
    public StitcherViewModel()
    {
        WeakReferenceMessenger.Default.Register<ASIMotor, string>(this, SteerMessage.Splice, (s, e) =>
        {
            _motor = e;
        });

        WeakReferenceMessenger.Default.Register<MappingMoveMessage, string>(this, nameof(MappingMoveMessage), (s, e) =>
        {
            Task.Run(() =>
            {
                _motor.Stop();
                SetPos(e.X, e.Y);
            });
        });

        if (cancellationTokenSource != null) cancellationTokenSource.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// 网格生成的第一个点
    /// </summary>
    [ObservableProperty]
    private StagePoint _point1 = new();

    /// <summary>
    /// 每次移动到指定位置后的采样前等待时间，一般设置为曝光时间的两倍
    /// </summary>
    [ObservableProperty]
    private int _waitAcquisitionTime = 200;

    /// <summary>
    /// 网格生成的第二个点
    /// </summary>
    [ObservableProperty]
    private StagePoint _point2 = new();

    /// <summary>
    /// 当前是否是忙碌状态
    /// </summary>
    [ObservableProperty]
    private bool _isBusy = false;

    /// <summary>
    /// 进度条，值0-100
    /// </summary>
    [ObservableProperty]
    private int _progress = 0;

    partial void OnIsBusyChanged(bool value)
    {
        Progress = 0;

        //true->开始采集
        if (value)
        {
            Debug.WriteLine("开始任务");

            Task.Run(() =>
            {
                while (true)
                {
                    if (Progress == 100) break;
                    Progress += 10;
                    Thread.Sleep(500);
                }
                IsFinish = true;
                IsBusy = false;
            });

        }

        //!false & !false->采集过程中停止采集
        if (!value && !IsFinish)
        {
            Debug.WriteLine("取消任务");
        }

        //!false & ture->完成采集后自动停止
        if (!value && IsFinish)
        {
            IsFinish = false;

            Debug.WriteLine("任务完成，回归初始状态");
        }
    }

    /// <summary>
    /// 每个像素点对应的实际坐标差值
    /// </summary>
    [ObservableProperty]
    private double _perPixel2Unit = 0.67;

    /// <summary>
    /// 图像尺寸
    /// </summary>
    [ObservableProperty]
    private (int Width, int Height) _imageSize = new(2560, 2160);

    [ObservableProperty]
    private (double X, double Y)? _posStart;

    [ObservableProperty]
    private DisplayModel _display = new DisplayModel();

    [ObservableProperty]
    private BitmapFrame? _frame;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    private Mat? _stitchMat;

    /// <summary>
    /// 是否完成任务
    /// </summary>
    public bool IsFinish = false;

    /// <summary>
    /// 设置当前电动台位置为点1
    /// </summary>
    [RelayCommand]
    void SetCurrentAsPoint1()
    {
        Point1.X = _motor.X;
        Point1.Y = _motor.Y;
    }

    /// <summary>
    /// 设置当前电动台位置为点2
    /// </summary>
    [RelayCommand]
    void SetCurrentAsPoint2()
    {
        Point2.X = _motor.X;
        Point2.Y = _motor.Y;
    }

    /// <summary>
    /// 从xy位置获取一张图像
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    Mat GetCeil(double x, double y)
    {
        SetPos(x, y);
        Thread.Sleep(WaitAcquisitionTime);
        var mat = new Mat();
        Application.Current.Dispatcher.Invoke(() =>
        {
            mat = GlobalValue.GlobalShellViewModel.DisplayCurrent.Original;
        });
        return mat;
    }

    /// <summary>
    /// 移动电动台，直到移动到指定位置
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    bool SetPos(double x, double y)
    {
        var xPos = Math.Round(x, 2);
        var yPos = Math.Round(y, 2);

        int timeoutMilliseconds = 4000;
        int maxIterations = 10;

        DateTime startTime = DateTime.Now;
        int iterationCount = 0;

        //do
        //{
        //    _motor.SetXYPosition(xPos, yPos);
        //    _motor.ReadPosition();
        //} while (Math.Abs(_motor.X - xPos) > 10 && Math.Abs(_motor.Y - yPos) > 10);

        _motor.ReadPosition();

        if(Math.Abs(_motor.X - xPos) > 10)
        {
            do
            {
                _motor.SetXPosition(xPos);
                Thread.Sleep(500);
                _motor.ReadPosition();
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                {
                    Debug.WriteLine("SetPosition operation timed out");
                    break;
                }
                if (iterationCount > maxIterations)
                {
                    Debug.WriteLine("Exceeded maximum iterations");
                    break;
                }
            } while (Math.Abs(_motor.X - xPos) > 10);
        }

        if(Math.Abs(_motor.Y - yPos) > 10)
        {
            do
            {
                _motor.SetYPosition(yPos);
                Thread.Sleep(500);
                _motor.ReadPosition();
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                {
                    Debug.WriteLine("SetPosition operation timed out");
                    break;
                }
                if (iterationCount > maxIterations)
                {
                    Debug.WriteLine("Exceeded maximum iterations");
                    break;
                }
            } while (Math.Abs(_motor.Y - yPos) > 10);
        }
        

        try
        {
            while (Math.Abs(_motor.X - xPos) > 1 || Math.Abs(_motor.Y - yPos) > 1)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                {
                    throw new TimeoutException("SetPosition operation timed out");
                }

                if (iterationCount > maxIterations)
                {
                    throw new Exception("Exceeded maximum iterations");
                }

                iterationCount++;

                Thread.Sleep(10);
            }

            return true;
        }
        catch (Exception ex)
        {
            //MessageBox.Show("位移台出现错误，停止拼接");
            return false;
        }
    }

    /// <summary>
    /// 这里写的是开始采集的示例程序
    /// </summary>
    void Start(CancellationToken cancellationToken)
    {
        var sWidth = ImageSize.Width * PerPixel2Unit;
        var sHeight = ImageSize.Height * PerPixel2Unit;

        var cols = (int)Math.Ceiling(Math.Abs(Point1.X - Point2.X) / sWidth);
        var rows = (int)Math.Ceiling(Math.Abs(Point1.Y - Point2.Y) / sHeight);
        cols = (cols == 0) ? 1 : cols;
        rows = (rows == 0) ? 1 : rows;
        // note 之类直接强行要求第一个点左上方，第二个右上方

        double startX = Math.Min(Point1.X, Point2.X);
        double startY = Math.Min(Point1.Y, Point2.Y);

        PosStart = (startX, startY);

        StitchMat = new Mat(new OpenCvSharp.Size(cols * ImageSize.Width, rows * ImageSize.Height)
            , MatType.CV_16UC1
            , new Scalar(0));

        int times = rows * cols;
        int single = (int)Math.Ceiling(100.0 / times);

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                Progress += single;
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("Stitcher return");
                    // 如果请求取消，则停止处理
                    return;
                }

                var index = i % 2 == 0 ?
                    (startX + j * sWidth, startY + i * sHeight)
                    : (startX + (cols - 1 - j) * sWidth, startY + i * sHeight);

                var mat = GetCeil(index.Item1, index.Item2);

                var pRow = i * ImageSize.Height;
                var pCol = i % 2 == 0
                    ? j * ImageSize.Width
                    : ((cols - 1 - j)) * ImageSize.Width;

                mat.CopyTo(StitchMat[new OpenCvSharp.Rect(
                    pCol,
                    pRow,
                    mat.Width,
                    mat.Height)]);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Display.Original = StitchMat.Clone();
                });
            }

        }    
    }

    private CancellationTokenSource cancellationTokenSource;

    [RelayCommand]
    void StartScan()
    {
        Task.Run(() =>
        {
            Start(cancellationTokenSource.Token);
        });
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using Simscop.API;
using Simscop.API.ASI;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace Simscop.Spindisk.Core.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private CancellationTokenSource? _cancelToken;

    public const double TimeInterval = 1500;

    [ObservableProperty]
    private string _root = "";

    [RelayCommand]
    void SelectRoot()
    {
        var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() == DialogResult.OK)
            Root = dialog.SelectedPath;
    }

    [ObservableProperty]
    private double _span = 0.1;

    [ObservableProperty]
    private double _xStart = 0;

    [ObservableProperty]
    private double _xEnd = 0;

    [ObservableProperty]
    private double _xStep = 0;

    [ObservableProperty]
    private bool _xYEnable = true;

    [ObservableProperty]
    private double _yStart = 0;

    [ObservableProperty]
    private double _yEnd = 0;

    [ObservableProperty]
    private double _yStep = 0;

    [ObservableProperty]
    private double _zStart = 0;

    [ObservableProperty]
    private double _zEnd = 0;

    [ObservableProperty]
    private double _zStep = 0;

    [ObservableProperty]
    private bool _zEnable = true;

    [ObservableProperty]
    private double _percent = 0;

    partial void OnPercentChanged(double value)
    {
        if (value == 0) Title = "自动扫描";

        if (Percent > 1) Percent = 1;

        Title = $"自动扫描 ({value:F2} %)";
    }

    [ObservableProperty]
    private string _title = $"自动扫描";

    double result = 0;

    void StartScanXY()
    {
        if (string.IsNullOrEmpty(Root))
        {
            MessageBox.Show("请先设置存储路径");
            return;
        }

        var flags = new string[]
        {
            "X","Y","Z"
        };


        void EnableAction(bool value) => GetType().GetProperty($"XYEnable")!.SetValue(this, value);

        var xStartValue = (double)GetType().GetProperty("XStart")!.GetValue(this)!;
        var xEndValue = (double)GetType().GetProperty("XEnd")!.GetValue(this)!;
        var xStepValue = (double)GetType().GetProperty("XStep")!.GetValue(this)!;

        var yStartValue = (double)GetType().GetProperty("YStart")!.GetValue(this)!;
        var yEndValue = (double)GetType().GetProperty("YEnd")!.GetValue(this)!;
        var yStepValue = (double)GetType().GetProperty("YStep")!.GetValue(this)!;

        var xMessage = SteerMessage.GetValue($"Move{flags[0]}")!;

        var yMessage = SteerMessage.GetValue($"Move{flags[1]}")!;

        

        _cancelToken = new CancellationTokenSource();

        Task.Run(async () =>
        {
            EnableAction(false);
            Percent = 0;

            // 同号满足条件
            if (xStepValue * (xEndValue - xStartValue) <= 0 | xStepValue == 0 | yStepValue * (yEndValue - yStartValue) <= 0 | yStepValue == 0)
            {
                EnableAction(true);
                MessageBox.Show("参数设置有误");
                return;
            }




            var yPos = yStartValue;

            var xForwardPos = xStartValue;

            var xReversePos = xEndValue;

            var xCount = (int)Math.Ceiling((xEndValue - xStartValue) / xStepValue);

            var yCount = (int)Math.Ceiling((yEndValue - yStartValue) / yStepValue);

            double value = 1;

            WeakReferenceMessenger.Default.Send<string, string>(yStartValue.ToString(CultureInfo.InvariantCulture), yMessage);
            Thread.Sleep(2000);
            WeakReferenceMessenger.Default.Send<string, string>(xStartValue.ToString(CultureInfo.InvariantCulture), xMessage);
            Thread.Sleep(2000);
            do
            {
                for (int j = 0; j <= yCount; j++)
                {
                    Debug.WriteLine($"[INFO] {flags[1]} -> {yPos}");
                    WeakReferenceMessenger.Default.Send<string, string>(yPos.ToString(CultureInfo.InvariantCulture), yMessage);
                    if (value % 2 == 0)
                    {
                        xReversePos = result; 
                        for(int i = xCount; i >= 0; i --)
                        {
                            Debug.WriteLine($"[INFO] {flags[0]} -> {xReversePos}");
                            WeakReferenceMessenger.Default.Send<string, string>(xReversePos.ToString(CultureInfo.InvariantCulture), xMessage);

                            Thread.Sleep((int)(Span * 1000));

                            await MicroFocus();

                            var xPath = System.IO.Path.Join(Root, $"{flags[1]}_{yPos}_{flags[0]}_{xReversePos}.TIF");
                            WeakReferenceMessenger.Default
                            .Send<string, string>(xPath, MessageManage.SaveACapture);

                            xReversePos -= xStepValue;
                            if(_cancelToken.IsCancellationRequested) break;
                        }
                        value++;
                        xReversePos = result;
                    }
                    else
                    {
                        for (int i = 0; i <= xCount; i++)
                        {
                            
                            Debug.WriteLine($"[INFO] {flags[0]} -> {xForwardPos}");
                            WeakReferenceMessenger.Default.Send<string, string>(xForwardPos.ToString(CultureInfo.InvariantCulture), xMessage);
                            
                            Thread.Sleep((int)(Span * 1000));

                            await MicroFocus();

                            var xPath = System.IO.Path.Join(Root, $"{flags[1]}_{yPos}_{flags[0]}_{xForwardPos}.TIF");
                            WeakReferenceMessenger.Default
                            .Send<string, string>(xPath, MessageManage.SaveACapture);
                            xForwardPos += xStepValue;
                            if (_cancelToken.IsCancellationRequested) break;
                        }
                        result = xForwardPos - xStepValue;
                        value++;
                        xForwardPos = xStartValue;
                    }
                    yPos += yStepValue;
                }
                if (_cancelToken.IsCancellationRequested) break;
            } while (value < yCount);
 
            EnableAction(true);
        });
    }

    async Task Focus()
    {
        Thread.Sleep(100);

        await Task.Run(() =>
        {
            focus = AutoFocus.Create();
            focus.FirstCount = 5;
            focus.FirstStep = 10;
            focus.SeccondCount = 5;
            focus.SecondStep = 1;
            focus.Threshold = 0.02;
            focus.CropSize = 0.5;
            focus.Focus();
        });
    }

    async Task MicroFocus()
    {
        Thread.Sleep(100);

        await Task.Run(() =>
        {
            focus = AutoFocus.Create();
            focus.FirstCount = 0;
            focus.FirstStep = 5;
            focus.SeccondCount = 3;
            focus.SecondStep = 1;
            focus.Threshold = 0.02;
            focus.CropSize = 0.2;
            focus.Focus();
        });
    }

    AutoFocus focus;

    void StartScan(uint mode)
    {

        if (string.IsNullOrEmpty(Root))
        {
            MessageBox.Show("请先设置存储路径");
            return;
        }

        var flags = new string[]
        {
        "X","Y","Z"
        };

        var flag = flags[mode];

        void EnableAction(bool value) => GetType().GetProperty($"{flag}Enable")!.SetValue(this, value);

        var startValue = (double)GetType().GetProperty($"{flag}Start")!.GetValue(this)!;
        var endValue = (double)GetType().GetProperty($"{flag}End")!.GetValue(this)!;
        var stepValue = (double)GetType().GetProperty($"{flag}Step")!.GetValue(this)!;

        var message = SteerMessage.GetValue($"Move{flag}")!;

        _cancelToken = new CancellationTokenSource();

        List<Mat> stack = new List<Mat>();

        Task.Run(() =>
        {
            EnableAction(false);
            Percent = 0;

            // 同号满足条件
            if (stepValue * (endValue - startValue) <= 0 | stepValue == 0)
            {
                EnableAction(true);
                MessageBox.Show("参数设置有误");
                return;
            }

            var pos = startValue;

            var count = (int)Math.Ceiling((endValue - startValue) / stepValue);
            var step = 0;

            WeakReferenceMessenger.Default.Send<string, string>(pos.ToString(CultureInfo.InvariantCulture), message);
            Thread.Sleep(3000);
            do
            {
                Debug.WriteLine($"[INFO] {flag} -> {pos}");
                WeakReferenceMessenger.Default.Send<string, string>(pos.ToString(CultureInfo.InvariantCulture), message);

                Thread.Sleep((int)(Span * 1000));

                var path = System.IO.Path.Join(Root, $"{flag}_{pos}.TIF");
                WeakReferenceMessenger.Default.Send<string, string>(path, MessageManage.SaveACapture);

                stack.Add(GlobalValue.CurrentFrame);

                pos += stepValue;

                pos = stepValue > 0 ? Math.Min(pos, endValue) : Math.Max(pos, endValue);

                Percent = (double)++step / count * 100;
                if (_cancelToken.IsCancellationRequested)
                {
                    stack.Clear();
                    break;
                };
            } while (step <= count && !_cancelToken.IsCancellationRequested);

            var route = Path.Join(Root, $"Start_{startValue}_End_{endValue}_Step_{stepValue}.TIF");
            if(stack!=null) Cv2.ImWrite(route, stack);


            if (_cancelToken.IsCancellationRequested)
                Percent = 0;

            EnableAction(true);
        });
    }

    [RelayCommand]
    public void StartScanX()
        => StartScanXY();

    [RelayCommand]
    public void StartScanY()
        => StartScanXY();

    [RelayCommand]
    public void StartScanZ()
        => StartScan(2);

    [RelayCommand]
    void StopScan()
        => _cancelToken?.Cancel();
}
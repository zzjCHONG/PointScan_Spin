﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using Simscop.API;
using Simscop.API.ASI;
using Simscop.API.Native.Mshot;
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
    private double _xYSpan = 0.5;

    [ObservableProperty]
    private double _zSpan = 0.1;

    [ObservableProperty]
    private double _xStart = 0;

    [ObservableProperty]
    private double _xEnd = 0;

    [ObservableProperty]
    private double _xYStep = 0;

    [ObservableProperty]
    private bool _xYEnable = true;

    [ObservableProperty]
    private double _yStart = 0;

    [ObservableProperty]
    private double _yEnd = 0;

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

    [ObservableProperty]
    private bool _isFocus = false;

    partial void OnIsFocusChanged(bool value)
    {
        Debug.WriteLine("IsFouce =" +value);
    }

    [ObservableProperty]
    private String _isXYStart = "开始扫描";

    [ObservableProperty]
    private String _isZStart = "开始扫描";

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

        var yStartValue = (double)GetType().GetProperty("YStart")!.GetValue(this)!;
        var yEndValue = (double)GetType().GetProperty("YEnd")!.GetValue(this)!;
        var stepValue = (double)GetType().GetProperty("XYStep")!.GetValue(this)!;

        var xMessage = SteerMessage.GetValue($"Move{flags[0]}")!;

        var yMessage = SteerMessage.GetValue($"Move{flags[1]}")!;

        

        _cancelToken = new CancellationTokenSource();

        Task.Run(async () =>
        {
            EnableAction(false);
            Percent = 0;
            IsXYStart = "停止采集";

            // 同号满足条件
            if (stepValue * (xEndValue - xStartValue) <= 0 | stepValue == 0 | stepValue * (yEndValue - yStartValue) <= 0 | stepValue == 0)
            {
                EnableAction(true);
                MessageBox.Show("参数设置有误");
                return;
            }

            var yPos = yStartValue;

            var xForwardPos = xStartValue;

            var xReversePos = xEndValue;

            var xCount = (int)Math.Ceiling((xEndValue - xStartValue) / stepValue);

            var yCount = (int)Math.Ceiling((yEndValue - yStartValue) / stepValue);

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

                            Thread.Sleep((int)(XYSpan * 1000));

                            if (IsFocus)
                            {
                                await CustomFocus();
                            };

                            var xPath = System.IO.Path.Join(Root, $"{flags[1]}_{yPos}_{flags[0]}_{xReversePos}.TIF");
                            WeakReferenceMessenger.Default
                            .Send<string, string>(xPath, MessageManage.SaveACapture);

                            xReversePos -= stepValue;
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
                            
                            Thread.Sleep((int)(XYSpan * 1000));

                            if (IsFocus)
                            {
                                await CustomFocus();
                            }

                            var xPath = System.IO.Path.Join(Root, $"{flags[1]}_{yPos}_{flags[0]}_{xForwardPos}.TIF");
                            WeakReferenceMessenger.Default
                            .Send<string, string>(xPath, MessageManage.SaveACapture);
                            xForwardPos += stepValue;
                            if (_cancelToken.IsCancellationRequested) break;
                        }
                        result = xForwardPos - stepValue;
                        value++;
                        xForwardPos = xStartValue;
                    }
                    yPos += stepValue;
                }
                if (_cancelToken.IsCancellationRequested) break;
            } while (value < yCount);
 
            EnableAction(true);
            IsXYStart = "开始采集";
        });
    }

    async Task CustomFocus()
    {
        Thread.Sleep(100);

        await Task.Run(() =>
        {
            GlobalValue.CustomFocus.Focus();
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
            IsZStart = "停止采集";

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

                Thread.Sleep((int)(ZSpan * 1000));

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
            IsZStart = "开始采集";
        });
    }

    void SetStartPos(uint mode)
    {
        var flags = new string[]
        {
        "XY","Z"
        };

        var flag = flags[mode];

        if (flag == "XY")
        {
            XStart = GlobalValue.GlobalMotor.X;
            YStart = GlobalValue.GlobalMotor.Y;
        }
        else if (flag == "Z")
        {
            ZStart = GlobalValue.GlobalMotor.Z;
        }
    }

    void SetEndPos(uint mode)
    {
        var flags = new string[]
        {
        "XY","Z"
        };

        var flag = flags[mode];

        if (flag == "XY")
        {
            XEnd = GlobalValue.GlobalMotor.X;
            YEnd = GlobalValue.GlobalMotor.Y;
        }
        else if (flag == "Z")
        {
            ZEnd = GlobalValue.GlobalMotor.Z;
        }
    }

    [RelayCommand]
    public void StartScan()
        => StartScanXY();

    [RelayCommand]
    public void StartScanZ()
        => StartScan(2);

    [RelayCommand]
    void StopScan()
        => _cancelToken?.Cancel();

    [RelayCommand]
    void SetXYStartPoint() => SetStartPos(0);

    [RelayCommand]
    void SetZStartPoint() => SetStartPos(1);

    [RelayCommand]
    void SetXYEndPoint() => SetEndPos(0);

    [RelayCommand]
    void SetZEndPoint() => SetEndPos(1);
}
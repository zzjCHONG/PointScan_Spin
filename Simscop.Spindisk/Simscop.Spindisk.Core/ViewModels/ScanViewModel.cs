using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    private bool _xEnable = true;

    [ObservableProperty]
    private double _yStart = 0;

    [ObservableProperty]
    private double _yEnd = 0;

    [ObservableProperty]
    private double _yStep = 0;

    [ObservableProperty]
    private bool _yEnable = true;

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

    void StartScan(uint mode)
    {
        //if (string.IsNullOrEmpty(Root))
        //{
        //    MessageBox.Show("请先设置存储路径");
        //    return;
        //}

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

                pos += stepValue;

                pos = stepValue > 0 ? Math.Min(pos, endValue) : Math.Max(pos, endValue);

                Percent = (double)++step / count * 100;
            } while (step <= count && !_cancelToken.IsCancellationRequested);

            if (_cancelToken.IsCancellationRequested)
                Percent = 0;

            EnableAction(true);
        });
    }

    [RelayCommand]
    public void StartScanX()
        => StartScan(0);

    [RelayCommand]
    public void StartScanY()
        => StartScan(1);

    [RelayCommand]
    public void StartScanZ()
        => StartScan(2);

    [RelayCommand]
    void StopScan()
        => _cancelToken?.Cancel();
}
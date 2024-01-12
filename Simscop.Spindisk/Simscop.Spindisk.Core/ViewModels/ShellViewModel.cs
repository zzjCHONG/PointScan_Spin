// https://blog.walterlv.com/post/wpf-high-performance-bitmap-rendering.html


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Simscop.Lib.ImageExtension;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;


namespace Simscop.Spindisk.Core.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private ImageSource? _imageFirst;

    //private WriteableBitmap _wbBitmap;

    private Mat? _currentFrame = null;

    private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);

    public ShellViewModel()
    {
        WeakReferenceMessenger.Default.Register<DisplayFrame, string>(this, "Display", (s, m) =>
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                m.ToMat(out var bitmap);
                var dst = new Mat();
                Cv2.ApplyColorMap(bitmap, dst, ColorMaps.Green);

                _currentFrame = dst;
                var source = dst.ToBitmapSource();
                ImageFirst = source;
            });
        });

        WeakReferenceMessenger.Default.Register<SaveFrameModel, string>(this, MessageManage.SaveCurrentCapture,
            (s, e) =>
            {
                if (_currentFrame == null) return;
                e.Dump();

                var mat = _currentFrame.Clone();
                foreach (var path in e.Paths)
                {
                    mat.SaveImage(path);
                }
            });

        WeakReferenceMessenger.Default.Register<string, string>(this,MessageManage.SaveACapture, (s, e) =>
        {
            if (_currentFrame == null) return;
            var mat = _currentFrame.Clone();
            mat.SaveImage(e);
        });
    }
}


// https://blog.walterlv.com/post/wpf-high-performance-bitmap-rendering.html

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lift.Core.ImageArray.Extensions;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;

namespace Simscop.Spindisk.Core.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    #region 不再修改

    [ObservableProperty]
    private DisplayModel _displayFirst = new();

    [ObservableProperty]
    private DisplayModel _displaySecond = new();

    [ObservableProperty]
    private DisplayModel _displayThird = new();

    [ObservableProperty]
    private DisplayModel _displayFourth = new();

    [ObservableProperty]
    private DisplayModel _displayCurrent = new();

    [ObservableProperty]
    private bool _enableFirst = false;

    [ObservableProperty]
    private bool _enableSecond = false;

    [ObservableProperty]
    private bool _enableThird = false;

    [ObservableProperty]
    private bool _enableFourth = false;

    [ObservableProperty]
    private bool _isCameraCapture=false;

    partial void OnEnableFirstChanged(bool value) => SwitchEnable(value, 1);
    partial void OnEnableSecondChanged(bool value) => SwitchEnable(value, 2);
    partial void OnEnableThirdChanged(bool value) => SwitchEnable(value, 3);
    partial void OnEnableFourthChanged(bool value) => SwitchEnable(value, 4);

    void SwitchEnable(bool value, int index)
    {
        if (!value) return;

        EnableFirst = index == 1;
        EnableSecond = index == 2;
        EnableThird = index == 3;
        EnableFourth = index == 4;

        DisplayCurrent = index switch
        {
            1 => DisplayFirst,
            2 => DisplaySecond,
            3 => DisplayThird,
            4 => DisplayFourth,
            _ => throw new Exception()
        };

        WeakReferenceMessenger.Default.Send<ChannelControlEnableMessage>(new ChannelControlEnableMessage(index, value));
    }

    #endregion

    public ShellViewModel()
    {
        //切换页面
        WeakReferenceMessenger.Default.Register<LaserMessage, string>(this, nameof(LaserMessage), (s, m) =>
        {
            switch (m.Index)
            {
                case 0:
                    EnableFirst = m.Status;
                    break;
                case 1:
                    EnableSecond = m.Status;
                    break;
                case 2:
                    EnableThird = m.Status;
                    break;
                case 3:
                    EnableFourth = m.Status;
                    break;
            }
            Debug.WriteLine($"ShellViewModel {m.Index} - {m.Status}");
        });

        //接收原图
        WeakReferenceMessenger.Default.Register<DisplayFrame, string>(this, "Display", (s, m) =>
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (m.Image is not { } img) return;
                if (!(EnableFirst || EnableSecond || EnableThird || EnableFourth)) return;

                //Cv2.Flip(img, img, FlipMode.X);
                DisplayCurrent.Original = img.Clone();

            });
        });

        //非实时画面存图--存图界面使用
        WeakReferenceMessenger.Default.Register<CameraSaveMessage>(this, (s, m) =>
        {
            if (m.IsOriginalImage)
            {
                switch (m.Channel)
                {
                    case 0:

                        if (!DisplayFirst.Original.Empty())
                            DisplayFirst.Original.SaveImage(m.Filename);//原图
                        break;
                    case 1:
                        if (!DisplaySecond.Original.Empty())
                            DisplaySecond.Original.SaveImage(m.Filename);
                        break;
                    case 2:
                        if (!DisplayThird.Original.Empty())
                            DisplayThird.Original.SaveImage(m.Filename);
                        break;
                    case 3:
                        if (!DisplayFourth.Original.Empty())
                            DisplayFourth.Original.SaveImage(m.Filename);
                        break;
                    default: break;
                }
            }
            else
            {
                switch (m.Channel)
                {
                    case 0:
                        if (DisplayFirst.Frame == null)
                        {
                            MessageBox.Show("通道1无对应图像！");
                            return;
                        }
                        DisplayFirst.Frame.Clone().ToMat().SaveImage(m.Filename);//处理图
                        break;
                    case 1:
                        if (DisplaySecond.Frame == null)
                        {
                            MessageBox.Show("通道2无对应图像！");
                            return;
                        }
                        DisplaySecond.Frame.Clone().ToMat().SaveImage(m.Filename);
                        break;
                    case 2:
                        if (DisplayThird.Frame == null)
                        {
                            MessageBox.Show("通道3无对应图像！");
                            return;
                        }
                        DisplayThird.Frame.Clone().ToMat().SaveImage(m.Filename);
                        break;
                    case 3:
                        if (DisplayFourth.Frame == null)
                        {
                            MessageBox.Show("通道4无对应图像！");
                            return;
                        }
                        DisplayFourth.Frame.Clone().ToMat().SaveImage(m.Filename);
                        break;
                    default: break;
                }
            }
        });

        //多通道N+1合并
        WeakReferenceMessenger.Default.Register<MultiChannelMergeMessage>(this, (s, m) =>
        {
            List<Mat> matList = new List<Mat>();
            if (DisplayFirst.Frame != null)
            {
                var mat1 = DisplayFirst.Frame.Clone().ToMat();
                if (mat1.Channels() != 3)
                    Cv2.CvtColor(mat1, mat1, ColorConversionCodes.GRAY2BGR);
                matList.Add(mat1);
            }
            if (DisplaySecond.Frame != null)
            {
                var mat2 = DisplaySecond.Frame.Clone().ToMat();
                if (mat2.Channels() != 3)
                    Cv2.CvtColor(mat2, mat2, ColorConversionCodes.GRAY2BGR);
                matList.Add(mat2);
            }
            if (DisplayThird.Frame != null)
            {
                var mat3 = DisplayThird.Frame.Clone().ToMat();
                if (mat3.Channels() != 3)
                    Cv2.CvtColor(mat3, mat3, ColorConversionCodes.GRAY2BGR);
                matList.Add(mat3);
            }
            if (DisplayFourth.Frame != null)
            {
                var mat4 = DisplayFourth.Frame.Clone().ToMat();
                if (mat4.Channels() != 3)
                    Cv2.CvtColor(mat4, mat4, ColorConversionCodes.GRAY2BGR);
                matList.Add(mat4);
            }
            var merge = MatsExtension.MergeChannelAsMax(matList);
            merge.SaveImage(m.Filename);

        });

        //相机是否正常取像
        WeakReferenceMessenger.Default.Register<CameraControlENableMessage>(this, (s, m) =>
        {
            IsCameraCapture = m.IsEnable;
        });
    }

}
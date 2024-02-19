// https://blog.walterlv.com/post/wpf-high-performance-bitmap-rendering.html

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
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

                DisplayCurrent.Original = img.Clone();
            });
        });

        //存图
        WeakReferenceMessenger.Default.Register<CameraSaveMessage>(this, (s, m) =>
        {
            if (File.Exists(m.filename)) File.Delete(m.filename);//覆盖

            if (m.isOriginalImage)
            {
                switch (m.channel)
                {
                    case 0:
                        
                        if (!DisplayFirst.Original.Empty())
                            DisplayFirst.Original.SaveImage(m.filename);
                        break;
                    case 1:
                        if (!DisplaySecond.Original.Empty())
                            DisplaySecond.Original.SaveImage(m.filename);
                        break;
                    case 2:
                        if (!DisplayThird.Original.Empty())
                            DisplayThird.Original.SaveImage(m.filename);
                        break;
                    case 3:
                        if (!DisplayFourth.Original.Empty())
                            DisplayFourth.Original.SaveImage(m.filename);
                        break;
                    default: break;
                }
            }
            else
            {
                switch (m.channel)
                {
                    case 0:
                        if (DisplayFirst.Frame != null)
                            DisplayFirst.Frame.Clone().ToMat().SaveImage(m.filename);
                        break;
                    case 1:
                        if (DisplaySecond.Frame != null)
                            DisplaySecond.Frame.Clone().ToMat().SaveImage(m.filename);
                        break;
                    case 2:
                        if (DisplayThird.Frame != null)
                            DisplayThird.Frame.Clone().ToMat().SaveImage(m.filename);
                        break;
                    case 3:
                        if (DisplayFourth.Frame != null)
                            DisplayFourth.Frame.Clone().ToMat().SaveImage(m.filename);
                        break;
                    default: break;
                }
            }    
        });


        //伪彩通道修改
        WeakReferenceMessenger.Default.Register<MultiChannelColorMessage>(this, (s, m) =>
        {
            DisplayCurrent.ColorMode = m.codeModel;
        });

        //WeakReferenceMessenger.Default.Register<string, string>(this, MessageManage.DisplayFrame, (s, m) =>
        //{
        //    if (File.Exists(m)) File.Delete(m);//覆盖
        //    if (DisplayCurrent.Frame != null)
        //        DisplayCurrent.Frame.Clone().ToMat().SaveImage(m);//多通道存图
        //});

        WeakReferenceMessenger.Default.Register<MultiChannelMergeMessage>(this, (s, m) =>
        {
            List<Mat> mats = new List<Mat>();
            if (DisplayFirst.Frame != null)
                mats.Add(DisplayFirst.Frame.Clone().ToMat());
            if (DisplaySecond.Frame != null)
                mats.Add(DisplaySecond.Frame.Clone().ToMat());
            if (DisplayThird.Frame != null)
                mats.Add(DisplayThird.Frame.Clone().ToMat());
            if (DisplayFourth.Frame != null)
                mats.Add(DisplayFourth.Frame.Clone().ToMat());

            //DisplayModel.MergeChannel3();
        });

    }
}
// https://blog.walterlv.com/post/wpf-high-performance-bitmap-rendering.html

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using Simscop.API;
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


        WeakReferenceMessenger.Default.Register<DisplayFrame, string>(this, "Display", (s, m) =>
        {
            Application.Current?.Dispatcher.Invoke(() =>
            { 
                if (m.Image is not { } img) return;
                if (!(EnableFirst || EnableSecond || EnableThird || EnableFourth)) return;

                DisplayCurrent.Original = img;
            });
        });
    }
}
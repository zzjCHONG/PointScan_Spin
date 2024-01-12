using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Simscop.Spindisk.Core.ViewModels;

/**
 * 蓝色，红色，绿色，紫色
 */

/// <summary>
/// 激光通道操作
/// </summary>
public partial class LaserViewModel:ObservableObject
{
    [ObservableProperty] 
    private int _channelAValue = 25;

    [ObservableProperty] 
    private bool _channelAEnable = false;

    [ObservableProperty]
    private int _channelBValue = 89;

    [ObservableProperty]
    private bool _channelBEnable = false;

    [ObservableProperty]
    private int _channelCValue = 53;

    [ObservableProperty]
    private bool _channelCEnable = false;

    [ObservableProperty]
    private int _channelDValue = 9;

    [ObservableProperty]
    private bool _channelDEnable = false;
}
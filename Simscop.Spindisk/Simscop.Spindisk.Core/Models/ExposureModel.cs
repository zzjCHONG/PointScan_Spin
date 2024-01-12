using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Simscop.Spindisk.Core.Models;

/**
 *
 *
 * 最大最小值是固定的 最大10s，最小就是行周期；
High Sensitivity与Global Reset模式的行周期是11.2us，High Dynamic模式的行周期为6.6us，High Speed模式的行周期为7.2us；
 */

public partial class ExposureModel : ObservableObject
{
    [ObservableProperty]
    private double _exposure = 0;

    [ObservableProperty]
    private double _maxExposure=0;

    [ObservableProperty]
    private double _minExposure = 0;

    [ObservableProperty]
    private double _defaultExposure=0;

    [ObservableProperty] 
    private double _stepExposure = 0;

    [ObservableProperty]
    private double _expMin = 0;

    [ObservableProperty]
    private double _expSec = 0;

    [ObservableProperty]
    private double _expMs = 0;

    [ObservableProperty]
    private double _expUs = 0;

    /// <summary>
    /// 从单个时间计算出Exposure
    /// </summary>
    public void ToExposure()
    {

    }

    /// <summary>
    /// 从Exposure值计算出单个时间
    /// </summary>
    public void FromTime()
    {

    }
}
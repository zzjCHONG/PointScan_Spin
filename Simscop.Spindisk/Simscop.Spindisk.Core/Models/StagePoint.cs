using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.Models;

/// <summary>
/// 
/// </summary>
public partial class StagePoint : ObservableObject
{
    [ObservableProperty]
    private double _x = 0;

    [ObservableProperty]
    private double _y = 0;

    [ObservableProperty]
    private double _z = 0;

}

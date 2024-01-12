using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Simscop.Spindisk.Core.Models;

public partial class RoiModel:ObservableObject
{
    [ObservableProperty]
    private int _hOffset = 0;

    [ObservableProperty]
    private int _vOffset = 0;

    [ObservableProperty]
    private int _width = 0;

    [ObservableProperty]
    private int _height = 0;
}
using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Simscop.Fake.Spindisk;

internal partial class SpinViewModel : ObservableObject
{
   
    public List<string> ComList => new()
    {
        "COM1","COM2","COM3","COM4","COM5","COM6",
    };

    [ObservableProperty] private int _comIndex = 1;

    public List<string> Spining => new List<string>()
    {
        "Position1","Position2"
    };

    [ObservableProperty] private int _spiningIndex = 1;

    public List<string> Dichroic => new List<string>()
    {
        "Position1","Position2","Position3","Position4","Position5"
    };

    [ObservableProperty] private int _dichroicIndex = 1;

    public List<string> Emission=>new List<string>()
    {
        "Position1","Position2","Position3","Position4","Position5","Position6","Position7","Position8"
    };

    [ObservableProperty] private int _emissionIndex = 1;

    public List<string> Excitation => new List<string>()
    {
        "Position1","Position2","Position3","Position4","Position5","Position6","Position7","Position8"
    };

    [ObservableProperty] private int _excitationIndex = 1;

    [ObservableProperty] private bool _isComConnected = false;

    [ObservableProperty] private bool _isSpining = false;


}
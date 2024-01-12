using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Simscop.Spindisk.Core.Models;

public partial class SaveFrameModel : ObservableObject
{
    [ObservableProperty]
    private bool _isRaw = false;

    [ObservableProperty]
    private bool _isTif = true;

    [ObservableProperty]
    private bool _isPng = false;

    [ObservableProperty]
    private bool _isJpg = false;

    [ObservableProperty]
    private bool _isBmp = false;

    [ObservableProperty]
    private string _root = @"C:\TEMP";

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private bool _isTimeSuffix = true;

    [ObservableProperty]
    private int _count = 1;

    [ObservableProperty]
    private int _interval = 10;

    public void CopyFrom(SaveFrameModel model)
    {
        IsRaw = model.IsRaw;
        IsTif = model.IsTif;
        IsPng = model.IsPng;
        IsJpg = model.IsJpg;
        IsBmp = model.IsBmp;
        Root = model.Root;
        Name = model.Name;
        IsTimeSuffix = model.IsTimeSuffix;
        Count = model.Count;
        Interval = model.Interval;
    }

    public List<string> Paths { get; set; } = new List<string>();

    public void Dump()
    {
        var name = $"{Name}" +
                   $"{(!string.IsNullOrEmpty(Name) && IsTimeSuffix ? "_" : "")}" +
                   $"{(IsTimeSuffix ? $"{DateTime.Now:yyyyMMdd_HH_mm_ss}" : "")}";

        //if (IsRaw) Paths.Add(System.IO.Path.Join(Root, $"{name}.RAW"));
        if (IsTif) Paths.Add(System.IO.Path.Join(Root, $"{name}.TIF"));
        if (IsPng) Paths.Add(System.IO.Path.Join(Root, $"{name}.PNG"));
        if (IsJpg) Paths.Add(System.IO.Path.Join(Root, $"{name}.JPG"));
        if (IsBmp) Paths.Add(System.IO.Path.Join(Root, $"{name}.BMP"));
    }
}
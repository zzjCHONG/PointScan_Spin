using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.ViewModels;

public class ExampleModel
{
    public string Name { get; set; } = "";

    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public double? Exposure { get; set; }

    public double? LeftLevel { get; set; }

    public double? Contrast { get; set; }

    public double? Gamma { get; set; }

    public double? Start { get; set; }

    public double? Stop { get; set; }

    public double? Step { get; set; } = 1;


    static double? ConveterDouble(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return double.TryParse(value, out var result) ? result : null;

    }

    public static bool LoadFrom(string path, out List<ExampleModel> models)
    {
        var file = System.IO.File.ReadAllLines(path)[1..];

        var result = new List<string>();

        foreach (var line in file)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                result.Add(line);
            }
        }

        models = new List<ExampleModel>();
        foreach (var item in result)
        {
            var data = item.Split(',');

            if (data.Length < 4) continue;

            try
            {
                var m = new ExampleModel
                {
                    Name = data[0],
                    X = double.Parse(data[1]),
                    Y = double.Parse(data[2]),
                    Z = double.Parse(data[3]),

                    Exposure = data.Length >= 5 ? ConveterDouble(data[4]) : null,
                    LeftLevel = data.Length >= 6 ? ConveterDouble(data[5]) : null,
                    Gamma = data.Length >= 7 ? ConveterDouble(data[6]) : null,
                    Contrast = data.Length >= 8 ? ConveterDouble(data[7]) : null,
                    Start = data.Length >= 9 ? ConveterDouble(data[8]) : null,
                    Stop = data.Length >= 10 ? ConveterDouble(data[9]) : null,
                };
                models.Add(m);
            }
            catch (Exception)
            {
                continue;
            }

        }

        return true;
    }

    public override string ToString()
        => Name;
}


public partial class ExampleViewModel : ObservableObject
{
    [ObservableProperty]
    private List<ExampleModel>? _models;

    partial void OnModelsChanged(List<ExampleModel>? value)
        => Model = value?.FirstOrDefault();

    [ObservableProperty]
    private ExampleModel? _model;

    [ObservableProperty]
    private string _path = "";

    [RelayCommand]
    async Task OpenSelectFileDialog()
    {
        var dialog = new OpenFileDialog();
        dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

        if (dialog.ShowDialog() == true)
        {
            var filePath = dialog.FileName;

            Path = filePath;
            Models = await Task.Run(() =>
            {
                ExampleModel.LoadFrom(filePath, out var models);
                return models;
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public CameraViewModel? CameraVM { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ScanViewModel? ScanVM { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public SteerViewModel? SteerVM { get; set; }


    [RelayCommand]
    void AppendConfig()
    {
        if (Model == null) return;

        SteerVM?.MoveToX(Model.X);
        SteerVM?.MoveToY(Model.Y);
        SteerVM?.MoveToZ(Model.Z);

        if (CameraVM == null || !CameraVM.CameraConnected) return;
        CameraVM.IsAutoExposure = false;
        if (Model.Exposure != null) CameraVM.Exposure = (double)Model.Exposure;

        CameraVM.IsAutoLeftLevel = false;
        if (Model.LeftLevel != null) CameraVM.LeftLevel = (double)Model.LeftLevel;

        if (Model.Gamma != null) CameraVM.Gamma = (double)Model.Gamma;
        if (Model.Contrast != null) CameraVM.Contrast = (double)Model.Contrast;

        if (ScanVM == null) return;
        if (Model.Start != null) ScanVM.ZStart = (double)Model.Start;
        if (Model.Stop != null) ScanVM.ZEnd = (double)Model.Stop;
        ScanVM.ZStep = ScanVM.ZStart > ScanVM.ZEnd ? -1 : 1;
    }

}


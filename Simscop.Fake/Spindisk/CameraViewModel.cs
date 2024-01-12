using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using Simscop.Spindisk.Core.Models;
using CommunityToolkit.Mvvm.Input;
using Simscop.API.Native;
using Simscop.API;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Simscop.Fake.Spindisk;

internal partial class CameraViewModel : ObservableObject
{
    public DhyanaInfoModel DhyanaInfo { get; set; } = new DhyanaInfoModel();
    
    [ObservableProperty]
    private bool _isConnectedCamera = true;

    [RelayCommand]
    async Task ConnectCamera() { }

    [RelayCommand]
    void CaptureFrame() { }

    [RelayCommand]
    void SetExposure() => Exposure = ExposureModel.Exposure;

    [RelayCommand]
    async Task SetOnceExposure() { }

    [RelayCommand]
    void SetRoi() { }

    [RelayCommand]
    async Task SavePictures() { }

    [RelayCommand]
    private void SaveVideo() { }

    [ObservableProperty]
    private bool _isCapture = true;

    [ObservableProperty]
    private double _frameTimerInterval = 1;

    [ObservableProperty]
    private double _exposure = 20;

    [ObservableProperty]
    private ExposureModel _exposureModel = new()
    {
        MaxExposure = 10000,
        MinExposure = 0,
        StepExposure = 1000,
    };

    [ObservableProperty]
    private bool _isAutoExposure = false;

    public List<string> Resolutions => Dhyana.Resolutions;

    

    [ObservableProperty]
    private int _resolutionIndex = 0;

    public List<string> RoiMode => new List<string>()
    {
        "UnSet","2048 X 2048","1024 X 1024","512 X 512","256 X 256","128 X 128"
    };

    [ObservableProperty]
    private uint _roiModeIndex = 0;

    [ObservableProperty]
    private RoiModel _roiModel = new();

    [ObservableProperty]
    private bool _roiEnabled = false;

    [ObservableProperty]
    private bool _isHistc = true;

    [ObservableProperty]
    private bool _isAutoLeftLevel = false;

    [ObservableProperty]
    private bool _isAutoRightLevel = false;

    [ObservableProperty]
    private int _leftLevel = 0;

    [ObservableProperty]
    private int _rightLevel = 0;
    
    [ObservableProperty]
    private bool _horizontal = false;

    [ObservableProperty]
    private bool _vertical = false;
    
    public List<string> NoiseModes => new List<string>()
    {
        "0","1","2","3"
    };

    [ObservableProperty]
    private uint _noiseModeIndex = 0;
    
    public List<string> ImageModes => Dhyana.ImageModeRename;

    [ObservableProperty]
    private uint _imageModeIndex = 0;
 
    [ObservableProperty]
    private int _globalGain = 0;
 
    [ObservableProperty]
    private int _imageMode = 0;

    [ObservableProperty]
    private int _gamma = 0;

     [ObservableProperty]
    private int _contrast = 0;
 
    [ObservableProperty]
    private SaveModel _saveModel = new();

    [ObservableProperty]
    private int _savePictureCount = 1;

    [ObservableProperty]
    private int _savePictureInterval = 5;

    [ObservableProperty]
    private float _videoFps = 25;


    [ObservableProperty] private bool _isStart = false;

    [RelayCommand]
    private void Fake()
    {
        IsStart = !IsStart;
    }
}
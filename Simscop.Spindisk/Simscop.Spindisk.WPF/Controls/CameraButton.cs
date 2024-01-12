using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simscop.Spindisk.WPF.Controls;

public class CameraButton:Button
{
    public static readonly DependencyProperty StartImageProperty = DependencyProperty.Register(
        nameof(StartImage), typeof(ImageSource), typeof(CameraButton), new PropertyMetadata(default(ImageSource)));

    public ImageSource StartImage
    {
        get => (ImageSource)GetValue(StartImageProperty);
        set => SetValue(StartImageProperty, value);
    }

    public static readonly DependencyProperty StopImageProperty = DependencyProperty.Register(
        nameof(StopImage), typeof(ImageSource), typeof(CameraButton), new PropertyMetadata(default(ImageSource)));

    public ImageSource StopImage
    {
        get => (ImageSource)GetValue(StopImageProperty);
        set => SetValue(StopImageProperty, value);
    }

    public static readonly DependencyProperty IsStartProperty = DependencyProperty.Register(
        nameof(IsStart), typeof(bool), typeof(CameraButton), new PropertyMetadata(default(bool)));

    public bool IsStart
    {
        get => (bool)GetValue(IsStartProperty);
        set => SetValue(IsStartProperty, value);
    }
}
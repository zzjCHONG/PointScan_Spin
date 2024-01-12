using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simscop.Spindisk.WPF.Controls;

public class ObjectiveRadioButton : RadioButton
{
    public static readonly DependencyProperty UnCheckImageProperty = DependencyProperty.Register(
        nameof(UnCheckImage), typeof(ImageSource), typeof(ObjectiveRadioButton), new PropertyMetadata(default(ImageSource)));

    public ImageSource UnCheckImage
    {
        get => (ImageSource)GetValue(UnCheckImageProperty);
        set => SetValue(UnCheckImageProperty, value);
    }

    public static readonly DependencyProperty CheckImageProperty = DependencyProperty.Register(
        nameof(CheckImage), typeof(ImageSource), typeof(ObjectiveRadioButton), new PropertyMetadata(default(ImageSource)));

    public ImageSource CheckImage
    {
        get => (ImageSource)GetValue(CheckImageProperty);
        set => SetValue(CheckImageProperty, value);
    }

    public static readonly DependencyProperty TopLabelProperty = DependencyProperty.Register(
        nameof(TopLabel), typeof(string), typeof(ObjectiveRadioButton), new PropertyMetadata(default(string)));

    public string TopLabel
    {
        get => (string)GetValue(TopLabelProperty);
        set => SetValue(TopLabelProperty, value);
    }

    public static readonly DependencyProperty BottomLabelProperty = DependencyProperty.Register(
        nameof(BottomLabel), typeof(string), typeof(ObjectiveRadioButton), new PropertyMetadata(default(string)));

    public string BottomLabel
    {
        get => (string)GetValue(BottomLabelProperty);
        set => SetValue(BottomLabelProperty, value);
    }
}
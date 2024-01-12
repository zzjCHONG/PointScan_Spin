using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Simscop.UI.Controls;


public class ExtendWindow : Window
{
    public static readonly DependencyProperty IsLeftButtonEnableProperty = DependencyProperty.Register(
        nameof(IsLeftButtonEnable), typeof(bool), typeof(ExtendWindow), new PropertyMetadata(default(bool)));

    public bool IsLeftButtonEnable
    {
        get => (bool)GetValue(IsLeftButtonEnableProperty);
        set => SetValue(IsLeftButtonEnableProperty, value);
    }

    public static readonly DependencyProperty IsMiddleButtonEnableProperty = DependencyProperty.Register(
        nameof(IsMiddleButtonEnable), typeof(bool), typeof(ExtendWindow), new PropertyMetadata(default(bool)));

    public bool IsMiddleButtonEnable
    {
        get => (bool)GetValue(IsMiddleButtonEnableProperty);
        set => SetValue(IsMiddleButtonEnableProperty, value);
    }

    public static readonly DependencyProperty IsRightButtonEnableProperty = DependencyProperty.Register(
        nameof(IsRightButtonEnable), typeof(bool), typeof(ExtendWindow), new PropertyMetadata(default(bool)));

    public bool IsRightButtonEnable
    {
        get => (bool)GetValue(IsRightButtonEnableProperty);
        set => SetValue(IsRightButtonEnableProperty, value);
    }
}
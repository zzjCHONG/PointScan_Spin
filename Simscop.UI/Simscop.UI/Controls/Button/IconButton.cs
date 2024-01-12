using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Simscop.UI.Controls;


public class IconButton : Button
{
    public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
        nameof(IconHeight), typeof(double), typeof(IconButton), new PropertyMetadata(default(double)));

    public double IconHeight
    {
        get => (double)GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
        nameof(IconWidth), typeof(double), typeof(IconButton), new PropertyMetadata(default(double)));

    public double IconWidth
    {
        get => (double)GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    public static readonly DependencyProperty IconPathProperty = DependencyProperty.Register(
        nameof(IconPath), typeof(Geometry), typeof(IconButton), new PropertyMetadata(default(Geometry)));

    public Geometry IconPath
    {
        get => (Geometry)GetValue(IconPathProperty);
        set => SetValue(IconPathProperty, value);
    }

    public static readonly DependencyProperty IconThicknessProperty = DependencyProperty.Register(
        nameof(IconThickness), typeof(Thickness), typeof(IconButton), new PropertyMetadata(default(Thickness)));

    public Thickness IconThickness
    {
        get => (Thickness)GetValue(IconThicknessProperty);
        set => SetValue(IconThicknessProperty, value);
    }
}
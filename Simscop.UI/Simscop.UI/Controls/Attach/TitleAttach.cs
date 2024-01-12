using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Simscop.UI.Common;

namespace Simscop.UI.Controls.Attach;

/// <summary>
/// 类似于TextBlock和Label
/// </summary>
public class TitleAttach
{
    // Note 0-Left 1-Top 2-Right 3-Bottom
    public static readonly DependencyProperty TitlePlacementProperty = DependencyProperty.RegisterAttached(
        "TitlePlacement", typeof(int), typeof(TitleAttach), new FrameworkPropertyMetadata(IntBox.Zero, FrameworkPropertyMetadataOptions.Inherits));

    public static void SetTitlePlacement(DependencyObject element, int value)
        => element.SetValue(TitlePlacementProperty, (int)value);

    public static int GetTitlePlacement(DependencyObject element)
        => (int)element.GetValue(TitlePlacementProperty);

    #region Title

    public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
        "Title", typeof(string), typeof(TitleAttach), new PropertyMetadata(default(string)));

    public static void SetTitle(DependencyObject element, string value)
        => element.SetValue(TitleProperty, value);

    public static string GetTitle(DependencyObject element)
        => (string)element.GetValue(TitleProperty);

    public static readonly DependencyProperty TitleWidthProperty = DependencyProperty.RegisterAttached(
        "TitleWidth", typeof(GridLength), typeof(TitleAttach), new FrameworkPropertyMetadata(GridLength.Auto, FrameworkPropertyMetadataOptions.Inherits));

    public static void SetTitleWidth(DependencyObject element, GridLength value) => element.SetValue(TitleWidthProperty, value);

    public static GridLength GetTitleWidth(DependencyObject element) => (GridLength)element.GetValue(TitleWidthProperty);

    #endregion

    #region Color

    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
        "Background", typeof(Brush), typeof(TitleAttach), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetBackground(DependencyObject element, Brush value)
        => element.SetValue(BackgroundProperty, value);

    public static Brush GetBackground(DependencyObject element)
        => (Brush)element.GetValue(BackgroundProperty);


    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
        "Foreground", typeof(Brush), typeof(TitleAttach), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetForeground(DependencyObject element, Brush value)
        => element.SetValue(ForegroundProperty, value);

    public static Brush GetForeground(DependencyObject element)
        => (Brush)element.GetValue(ForegroundProperty);

    #endregion

    #region Border

    public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.RegisterAttached(
        "BorderBrush", typeof(Brush), typeof(TitleAttach), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetBorderBrush(DependencyObject element, Brush value)
        => element.SetValue(BorderBrushProperty, value);

    public static Brush GetBorderBrush(DependencyObject element)
        => (Brush)element.GetValue(BorderBrushProperty);

    #endregion

    #region Alignment

    public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.RegisterAttached(
        "HorizontalAlignment", typeof(HorizontalAlignment), typeof(TitleAttach), new FrameworkPropertyMetadata(default(HorizontalAlignment), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetHorizontalAlignment(DependencyObject element, HorizontalAlignment value)
        => element.SetValue(HorizontalAlignmentProperty, value);

    public static HorizontalAlignment GetHorizontalAlignment(DependencyObject element)
        => (HorizontalAlignment)element.GetValue(HorizontalAlignmentProperty);

    public static readonly DependencyProperty VerticalAlignmentProperty = DependencyProperty.RegisterAttached(
        "VerticalAlignment", typeof(VerticalAlignment), typeof(TitleAttach), new FrameworkPropertyMetadata(default(VerticalAlignment), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetVerticalAlignment(DependencyObject element, VerticalAlignment value)
        => element.SetValue(VerticalAlignmentProperty, value);

    public static VerticalAlignment GetVerticalAlignment(DependencyObject element)
        => (VerticalAlignment)element.GetValue(VerticalAlignmentProperty);


    #endregion

    #region Size

    public static readonly DependencyProperty MarginOnTheLeftProperty = DependencyProperty.RegisterAttached(
        "MarginOnTheLeft", typeof(Thickness), typeof(TitleAttach), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetMarginOnTheLeft(DependencyObject element, Thickness value)
        => element.SetValue(MarginOnTheLeftProperty, value);

    public static Thickness GetMarginOnTheLeft(DependencyObject element)
        => (Thickness)element.GetValue(MarginOnTheLeftProperty);

    public static readonly DependencyProperty MarginOnTheTopProperty = DependencyProperty.RegisterAttached(
        "MarginOnTheTop", typeof(Thickness), typeof(TitleAttach), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetMarginOnTheTop(DependencyObject element, Thickness value)
        => element.SetValue(MarginOnTheTopProperty, value);

    public static Thickness GetMarginOnTheTop(DependencyObject element)
        => (Thickness)element.GetValue(MarginOnTheTopProperty);

    public static readonly DependencyProperty PaddingProperty = DependencyProperty.RegisterAttached(
        "Padding", typeof(Thickness), typeof(TitleAttach), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.Inherits));

    public static void SetPadding(DependencyObject element, Thickness value) => element.SetValue(PaddingProperty, value);

    public static Thickness GetPadding(DependencyObject element) => (Thickness)element.GetValue(PaddingProperty);

    public static readonly DependencyProperty MinHeightProperty =
        DependencyProperty.RegisterAttached("MinHeight", typeof(double), typeof(TitleAttach), new PropertyMetadata(DoubleBox.Zero));

    public static double GetMinHeight(DependencyObject obj) => (double)obj.GetValue(MinHeightProperty);

    public static void SetMinHeight(DependencyObject obj, double value) => obj.SetValue(MinHeightProperty, value);

    public static readonly DependencyProperty MinWidthProperty =
        DependencyProperty.RegisterAttached("MinWidth", typeof(double), typeof(TitleAttach), new PropertyMetadata(DoubleBox.Zero));

    public static double GetMinWidth(DependencyObject obj) => (double)obj.GetValue(MinWidthProperty);

    public static void SetMinWidth(DependencyObject obj, double value) => obj.SetValue(MinWidthProperty, value);

    #endregion

}
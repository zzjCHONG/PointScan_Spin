using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Simscop.UI.Common;

namespace Simscop.UI.Controls.Attach;

/// <summary>
/// 所有控件都能用的部分
/// </summary>
public class ValueAttach
{
    #region Border

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
        "CornerRadius", typeof(CornerRadius), typeof(ValueAttach), new PropertyMetadata(default(CornerRadius)));

    public static void SetCornerRadius(DependencyObject element, CornerRadius value)
        => element.SetValue(CornerRadiusProperty, value);

    public static CornerRadius GetCornerRadius(DependencyObject element)
        => (CornerRadius)element.GetValue(CornerRadiusProperty);

    #endregion

    #region BrushAttach

    public static readonly DependencyProperty AttachBrushOneProperty = DependencyProperty.RegisterAttached(
        "AttachBrushOne", typeof(Brush), typeof(ValueAttach), new PropertyMetadata(default(Brush)));

    public static void SetAttachBrushOne(DependencyObject element, Brush value)
        => element.SetValue(AttachBrushOneProperty, value);

    public static Brush GetAttachBrushOne(DependencyObject element)
        => (Brush)element.GetValue(AttachBrushOneProperty);

    public static readonly DependencyProperty AttachBrushTwoProperty = DependencyProperty.RegisterAttached(
        "AttachBrushTwo", typeof(Brush), typeof(ValueAttach), new PropertyMetadata(default(Brush)));

    public static void SetAttachBrushTwo(DependencyObject element, Brush value)
        => element.SetValue(AttachBrushTwoProperty, value);

    public static Brush GetAttachBrushTwo(DependencyObject element)
        => (Brush)element.GetValue(AttachBrushTwoProperty);

    public static readonly DependencyProperty AttachBrushThreeProperty = DependencyProperty.RegisterAttached(
        "AttachBrushThree", typeof(Brush), typeof(ValueAttach), new PropertyMetadata(default(Brush)));

    public static void SetAttachBrushThree(DependencyObject element, Brush value)
        => element.SetValue(AttachBrushThreeProperty, value);

    public static Brush GetAttachBrushThree(DependencyObject element)
        => (Brush)element.GetValue(AttachBrushThreeProperty);

    #endregion

    #region CheckSwitchAttach

    /// <summary>
    /// 选中时展示的元素
    /// </summary>
    public static readonly DependencyProperty CheckedElementProperty = DependencyProperty.RegisterAttached(
        "CheckedElement", typeof(object), typeof(ValueAttach), new PropertyMetadata(default(object)));

    public static void SetCheckedElement(DependencyObject element, object value) => element.SetValue(CheckedElementProperty, value);

    public static object GetCheckedElement(DependencyObject element) => element.GetValue(CheckedElementProperty);

    /// <summary>
    /// 是否隐藏元素
    /// </summary>
    public static readonly DependencyProperty HideUncheckedElementProperty = DependencyProperty.RegisterAttached(
        "HideUncheckedElement", typeof(bool), typeof(ValueAttach), new PropertyMetadata(BoolBox.False));

    public static void SetHideUncheckedElement(DependencyObject element, bool value) => element.SetValue(HideUncheckedElementProperty, (bool)value);

    public static bool GetHideUncheckedElement(DependencyObject element) => (bool)element.GetValue(HideUncheckedElementProperty);

    #endregion

}
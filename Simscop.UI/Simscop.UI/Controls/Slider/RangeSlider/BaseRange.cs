// Adapted from https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/Primitives/RangeBase.cs

using Simscop.UI.Common;
using System.Windows.Controls;
using System.Windows;


//BaseRange.cs 中定义了多个依赖属性，这些属性用于描述控件的范围、位置及行为。以下是 BaseRange 中比较重要的依赖属性及其作用：

//1. Minimum：控件能够滚动或滑动的最小值。
//2. Maximum：控件能够滚动或滑动的最大值。
//3. Value：控件的当前值。
//4. LargeChange：大步长，指使用 Page Up 或 Page Down 键触发的滚动距离。
//5. SmallChange：小步长，指使用向上或向下箭头按键触发的滚动距离。
//6. IsDirectionReversed：指定控件的方向是否可以反转。
//7. Orientation：指定控件是水平滚动条（Horizontal），还是垂直滚动条（Vertical）。 

//这些依赖属性可以让开发人员配置 WPF 滑动控件的特性和行为，例如选择一个滑块 Slider 的最小值、最大值、当前值，以及滑块移动时的步长大小等。同时，这些属性还可以帮助开发人员实现自己的自定义滑动控件，因为它们提供了处理数值范围和值的方法。


//BaseRange.cs 中定义了多个事件，这些事件用于描述控件的交互和状态变化。以下是 BaseRange 中比较重要的事件及其作用：

//1. ValueChanged：当控件的 Value 属性值发生变化时触发，可以用于实现控件数据的双向绑定等相关功能。
//2. Scroll：当用户在滚动区域滚动鼠标轮或使用触摸板时触发，可以用于实现一些特定的滚动效果，例如反转方向滚动等。
//3. PreviewMouseLeftButtonDown 和 PreviewMouseLeftButtonUp：当用户按下或释放滚动条上的鼠标左键（或触摸屏幕）时触发，可以用于处理用户更加复杂的手势操作。
//4. Thumb.DragStarted 和 Thumb.DragCompleted：每次当 thumb 变量开始拖曳和拖曳结束时，这两个事件都会触发，可以用于处理控件拖动的开始和结束状态。

//这些事件可以帮助开发者实现滑动控件的交互，根据事件触发时的参数，开发人员可以实现滚动条的自定义行为、特定动画效果、记录控件操作日志等相关功能。


// ReSharper disable once CheckNamespace
namespace Simscop.UI.Controls;

public record RangeValue(double Start, double End);

/// <summary>
/// 
/// </summary>
public class BaseRange : Control
{
    #region 上下限

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(BaseRange),
        new PropertyMetadata(DoubleBox.Hundred, OnMaximumChanged, CoerceMaximum));

    private static object CoerceMaximum(DependencyObject d, object basevalue)
        => DoubleBox.Max(((BaseRange)d).Minimum, basevalue);


    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BaseRange br) return;

        br.CoerceValue(StartValueProperty);
        br.CoerceValue(EndValueProperty);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(double), typeof(BaseRange),
        new PropertyMetadata(DoubleBox.Zero, OnMinimumChanged, CoerceMinimum));

    private static object CoerceMinimum(DependencyObject d, object basevalue)
        => DoubleBox.Min(((BaseRange)d).Maximum, basevalue);

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BaseRange br) return;

        br.CoerceValue(MaximumProperty);
        br.CoerceValue(StartValueProperty);
        br.CoerceValue(EndValueProperty);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }


    #endregion

    #region 起始终止值

    private static object CoerceValue(DependencyObject d, object basevalue)
    {
        if (d is not BaseRange br || basevalue is not double bv) return basevalue;

        if (br.Minimum > bv)
            return br.Minimum;

        return br.Maximum < bv ? br.Maximum : basevalue;
    }

    protected virtual void OnValueChanged(RangeValue oldValue, RangeValue newValue) => RaiseEvent(
        new RoutedPropertyChangedEventArgs<RangeValue>(oldValue, newValue) { RoutedEvent = ValueChangedEvent });


    public static readonly DependencyProperty StartValueProperty = DependencyProperty.Register(
        nameof(StartValue), typeof(double), typeof(BaseRange),
        new FrameworkPropertyMetadata(DoubleBox.Zero,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
            OnStartValueChanged,
            CoerceValue));

    private static void OnStartValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BaseRange br) return;
        br.OnValueChanged(new RangeValue((double)e.OldValue, br.EndValue), 
            new RangeValue((double)e.NewValue, br.EndValue));
    }

    public double StartValue
    {
        get => (double)GetValue(StartValueProperty);
        set => SetValue(StartValueProperty, value);
    }

    public static readonly DependencyProperty EndValueProperty = DependencyProperty.Register(
        nameof(EndValue), typeof(double), typeof(BaseRange),
        new FrameworkPropertyMetadata(DoubleBox.Zero,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
            OnEndValueChanged,
            CoerceValue));

    private static void OnEndValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BaseRange br) return;
        br.OnValueChanged(new RangeValue( br.StartValue, (double)e.OldValue)
            ,new RangeValue(br.StartValue,(double)e.NewValue));
    }

    public double EndValue
    {
        get => (double)GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    #endregion

    #region 步进,表示大的步进范围和小的步进范围

    public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(
        nameof(SmallChange), typeof(double), typeof(BaseRange), new PropertyMetadata(DoubleBox.One));

    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(
        nameof(LargeChange), typeof(double), typeof(BaseRange), new PropertyMetadata(DoubleBox.Ten));

    public double LargeChange
    {
        get => (double)GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    #endregion

    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
        RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<RangeValue>), typeof(BaseRange));

    public event RoutedPropertyChangedEventHandler<RangeValue> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }
}
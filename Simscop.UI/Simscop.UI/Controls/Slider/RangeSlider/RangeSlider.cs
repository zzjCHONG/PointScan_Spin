using Simscop.UI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;



// ReSharper disable once CheckNamespace
namespace Simscop.UI.Controls;

[DefaultEvent("ValueChanged"), DefaultProperty("Value")]
[TemplatePart(Name = ElementTrack, Type = typeof(Track))]
public class RangeSlider : BaseRange
{
    private const string ElementTrack = "PART_Track";

    #region Initialize

    private void MoveToNextTick(double direction, bool isStart, bool isCenter = false, object? parameter = null)
    {
        if (DoubleBox.AreClose(direction, 0)) return;

        if (isCenter)
            switch (parameter)
            {
                case null:
                    {
                        var pt = Mouse.GetPosition(_track);
                        var newValue = _track.ValueFromPoint(pt);
                        if (DoubleBox.IsValid(newValue))
                        {
                            isStart = (StartValue + EndValue) / 2 > newValue;
                            if (!isStart)
                            {
                                direction = -Math.Abs(direction);
                            }
                        }

                        break;
                    }
                case bool parameterValue:
                    isStart = parameterValue;
                    break;
            }

        var value = isStart ? StartValue : EndValue;
        var next = SnapToTick(Math.Max(Minimum, Math.Min(Maximum, value + direction)));
        var greaterThan = direction > 0;

        // If the snapping brought us back to value, find the next tick point
        if (DoubleBox.AreClose(next, value) &&
            !(greaterThan && DoubleBox.AreClose(value, Maximum)) &&
            !(!greaterThan && DoubleBox.AreClose(value, Minimum)))
        {
            // If ticks collection is available, use it.
            // Note that ticks may be unsorted.
            if (Ticks is { Count: > 0 })
            {
                foreach (var tick in Ticks.Where(tick
                             => greaterThan
                                && DoubleBox.GreaterThan(tick, value)
                                && (DoubleBox.LessThan(tick, next)
                                    || DoubleBox.AreClose(next, value))
                                || !greaterThan && DoubleBox.LessThan(tick, value)
                                                && (DoubleBox.GreaterThan(tick, next) || DoubleBox.AreClose(next, value))))
                    next = tick;
            }
            else if (DoubleBox.GreaterThan(TickFrequency, 0.0))
            {
                // Find the current tick we are at
                var tickNumber = Math.Round((value - Minimum) / TickFrequency);

                if (greaterThan)
                    tickNumber += 1.0;
                else
                    tickNumber -= 1.0;

                next = Minimum + tickNumber * TickFrequency;
            }
        }

        // Update if we've found a better value
        if (!DoubleBox.AreClose(next, value))
            SetCurrentValue(isStart ? StartValueProperty : EndValueProperty, next);

    }

    public static RoutedCommand IncreaseLarge { get; private set; }

    public static RoutedCommand IncreaseSmall { get; private set; }

    public static RoutedCommand DecreaseLarge { get; private set; }

    public static RoutedCommand DecreaseSmall { get; private set; }

    public static RoutedCommand CenterLarge { get; private set; }

    public static RoutedCommand CenterSmall { get; private set; }

    public RangeSlider()
    {
        CommandBindings.Add(new CommandBinding(IncreaseLarge, OnIncreaseLarge));
        CommandBindings.Add(new CommandBinding(IncreaseSmall, OnIncreaseSmall));

        CommandBindings.Add(new CommandBinding(DecreaseLarge, OnDecreaseLarge));
        CommandBindings.Add(new CommandBinding(DecreaseSmall, OnDecreaseSmall));

        CommandBindings.Add(new CommandBinding(CenterLarge, OnCenterLarge));
        CommandBindings.Add(new CommandBinding(CenterSmall, OnCenterSmall));
    }

    private void OnCenterSmall(object sender, ExecutedRoutedEventArgs e) => (sender as RangeSlider)?.OnCenterSmall(e.Parameter);
    protected virtual void OnCenterSmall(object parameter) => MoveToNextTick(SmallChange, false, true);


    private void OnCenterLarge(object sender, ExecutedRoutedEventArgs e) => (sender as RangeSlider)?.OnCenterLarge(e.Parameter);
    protected virtual void OnCenterLarge(object parameter) => MoveToNextTick(LargeChange, false, true);


    private void OnDecreaseSmall(object sender, ExecutedRoutedEventArgs e) => (sender as RangeSlider)?.OnDecreaseSmall();
    protected virtual void OnDecreaseSmall() => MoveToNextTick(-SmallChange, true);


    private void OnDecreaseLarge(object sender, ExecutedRoutedEventArgs e) => (sender as RangeSlider)?.OnDecreaseLarge();
    protected virtual void OnDecreaseLarge() => MoveToNextTick(-LargeChange, true);


    private void OnIncreaseSmall(object sender, ExecutedRoutedEventArgs e) => (sender as RangeSlider)?.OnIncreaseSmall();
    protected virtual void OnIncreaseSmall() => MoveToNextTick(SmallChange, false);

    private void OnIncreaseLarge(object sender, ExecutedRoutedEventArgs e) => (sender as RangeSlider)?.OnIncreaseLarge();
    protected virtual void OnIncreaseLarge() => MoveToNextTick(LargeChange, false);


    static RangeSlider()
    {
        IncreaseLarge = new RoutedCommand(nameof(IncreaseLarge), typeof(RangeSlider));
        IncreaseSmall = new RoutedCommand(nameof(IncreaseSmall), typeof(RangeSlider));

        DecreaseLarge = new RoutedCommand(nameof(DecreaseLarge), typeof(RangeSlider));
        DecreaseSmall = new RoutedCommand(nameof(DecreaseSmall), typeof(RangeSlider));

        CenterLarge = new RoutedCommand(nameof(CenterLarge), typeof(RangeSlider));
        CenterSmall = new RoutedCommand(nameof(CenterSmall), typeof(RangeSlider));

        MinimumProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(DoubleBox.Zero, FrameworkPropertyMetadataOptions.AffectsMeasure));
        MaximumProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(DoubleBox.Hundred, FrameworkPropertyMetadataOptions.AffectsMeasure));
        StartValueProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(DoubleBox.Zero, FrameworkPropertyMetadataOptions.AffectsMeasure));
        EndValueProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(DoubleBox.Zero, FrameworkPropertyMetadataOptions.AffectsMeasure));

        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragStartedEvent, new DragStartedEventHandler(OnThumbDragStarted));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));

        EventManager.RegisterClassHandler(typeof(RangeSlider), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;

        var slider = (RangeSlider)sender;

        // When someone click on the Slider's part, and it's not focusable
        // Slider need to take the focus in order to process keyboard correctly
        if (!slider.IsKeyboardFocusWithin)
        {
            e.Handled = slider.Focus() || e.Handled;
        }

        if (slider._track.ThumbStart.IsMouseOver)
        {
            slider._track.ThumbStart.StartDrag();
            slider._thumbCurrent = slider._track.ThumbStart;
        }

        if (slider._track.ThumbEnd.IsMouseOver)
        {
            slider._track.ThumbEnd.StartDrag();
            slider._thumbCurrent = slider._track.ThumbEnd;
        }
    }

    private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e) => (sender as RangeSlider)?.OnThumbDragCompleted(e);
    protected virtual void OnThumbDragCompleted(DragCompletedEventArgs e)
    {
        if (e.OriginalSource is not Thumb thumb) return;
        var isStart = thumb.Equals(_track.ThumbStart);

        if (isStart) return;
        if (!thumb.Equals(_track.ThumbEnd)) return;

    }

    private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e) => (sender as RangeSlider)?.OnThumbDragDelta(e);
    protected virtual void OnThumbDragDelta(DragDeltaEventArgs e)
    {
        if (e.OriginalSource is not Thumb thumb) return;
        var isStart = thumb.Equals(_track.ThumbStart);
        if (!isStart)
        {
            if (!thumb.Equals(_track.ThumbEnd)) return;
        }

        // Convert to Track's co-ordinate
        OnThumbDragDelta(_track, isStart, e);
    }

    private void OnThumbDragDelta(RangeTrack? track, bool isStart, DragDeltaEventArgs e)
    {
        if (track == null || track?.ThumbStart == null | _track?.ThumbEnd == null) return;

        var newValue = (isStart ? StartValue : EndValue) + track?.ValueFromDistance(e.HorizontalChange, e.VerticalChange);
        if (newValue is { } nv && DoubleBox.IsValid(newValue)) UpdateValue(nv, isStart);
    }

    private static void OnThumbDragStarted(object sender, DragStartedEventArgs e) => (sender as RangeSlider)?.OnThumbDragStarted(e);

    protected virtual void OnThumbDragStarted(DragStartedEventArgs e)
    {
        // Show AutoToolTip if needed.
        if (e.OriginalSource is not RangeThumb thumb) return;

        _thumbCurrent = thumb;
        _originThumbPoint = Mouse.GetPosition(_thumbCurrent);
        _thumbCurrent.StartDrag();
    }

    #endregion

    private RangeThumb? _thumbCurrent;

    private RangeTrack _track;

    private Point _originThumbPoint;

    private Point _previousScreenCoordPosition;

    public override void OnApplyTemplate()
    {
        _thumbCurrent = null;

        base.OnApplyTemplate();

        _track = (GetTemplateChild(ElementTrack) as RangeTrack)!;

    }

    #region 依赖属性

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(RangeSlider), new PropertyMetadata(default(Orientation)));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty IsDirectionReversedProperty = DependencyProperty.Register(
        nameof(IsDirectionReversed), typeof(bool), typeof(RangeSlider), new PropertyMetadata(false));

    public bool IsDirectionReversed
    {
        get => (bool)GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, (bool)value);
    }

    public static readonly DependencyProperty DelayProperty
        = RepeatButton.DelayProperty.AddOwner(typeof(RangeSlider), new FrameworkPropertyMetadata(GetKeyboardDelay()));

    public int Delay
    {
        get => (int)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    internal static int GetKeyboardDelay()
    {
        var delay = SystemParameters.KeyboardDelay;
        if (delay is < 0 or > 3)
            delay = 0;
        return (delay + 1) * 250;
    }

    public static readonly DependencyProperty IntervalProperty
        = RepeatButton.IntervalProperty.AddOwner(typeof(RangeSlider), new FrameworkPropertyMetadata(GetKeyboardSpeed()));

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    internal static int GetKeyboardSpeed()
    {
        var speed = SystemParameters.KeyboardSpeed;
        if (speed < 0 || speed > 31)
            speed = 31;
        return (31 - speed) * (400 - 1000 / 30) / 31 + 1000 / 30;
    }

    public static readonly DependencyProperty IsSnapToTickEnabledProperty = DependencyProperty.Register(
        nameof(IsSnapToTickEnabled), typeof(bool), typeof(RangeSlider), new PropertyMetadata(false));

    public bool IsSnapToTickEnabled
    {
        get => (bool)GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, (bool)value);
    }

    public static readonly DependencyProperty TickPlacementProperty = DependencyProperty.Register(
        nameof(TickPlacement), typeof(TickPlacement), typeof(RangeSlider), new PropertyMetadata(default(TickPlacement)));

    public TickPlacement TickPlacement
    {
        get => (TickPlacement)GetValue(TickPlacementProperty);
        set => SetValue(TickPlacementProperty, value);
    }

    public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
        nameof(TickFrequency), typeof(double), typeof(RangeSlider), new PropertyMetadata(DoubleBox.One));

    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public static readonly DependencyProperty TicksProperty = DependencyProperty.Register(
        nameof(Ticks), typeof(DoubleCollection), typeof(RangeSlider), new PropertyMetadata(new DoubleCollection()));

    public DoubleCollection Ticks
    {
        get => (DoubleCollection)GetValue(TicksProperty);
        set => SetValue(TicksProperty, value);
    }

    public static readonly DependencyProperty IsMoveToPointEnabledProperty = DependencyProperty.Register(
        nameof(IsMoveToPointEnabled), typeof(bool), typeof(RangeSlider), new PropertyMetadata(false));

    public bool IsMoveToPointEnabled
    {
        get => (bool)GetValue(IsMoveToPointEnabledProperty);
        set => SetValue(IsMoveToPointEnabledProperty, (bool)value);
    }
    #endregion

    #region Override

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (IsMoveToPointEnabled && _track is { ThumbStart: { IsMouseOver: false }, ThumbEnd.IsMouseOver: false })
        {
            // Here we need to determine whether it's closer to the starting point or the end point. 
            var pt = e.MouseDevice.GetPosition(_track);
            UpdateValue(pt);
            e.Handled = true;
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    private void UpdateValue(Point point)
    {
        var newValue = _track.ValueFromPoint(point);

        var isStart = (StartValue + EndValue) / 2 > newValue;
        UpdateValue(newValue, isStart);
    }

    private void UpdateValue(double value, bool isStart)
    {
        var snappedValue = SnapToTick(value);

        if (isStart)
        {
            if (DoubleBox.AreClose(snappedValue, StartValue)) return;

            var start = Math.Max(Minimum, Math.Min(Maximum, snappedValue));
            if (start > EndValue)
            {
                SetCurrentValue(StartValueProperty, EndValue);
                SetCurrentValue(EndValueProperty, start);
                _track.ThumbStart.CancelDrag();
                _track.ThumbEnd.StartDrag();
                _thumbCurrent = _track.ThumbEnd;
            }
            else
                SetCurrentValue(StartValueProperty, start);

        }
        else
        {
            if (DoubleBox.AreClose(snappedValue, EndValue)) return;

            var end = Math.Max(Minimum, Math.Min(Maximum, snappedValue));
            if (end < StartValue)
            {
                SetCurrentValue(EndValueProperty, StartValue);
                SetCurrentValue(StartValueProperty, end);
                _track.ThumbEnd.CancelDrag();
                _track.ThumbStart.StartDrag();
                _thumbCurrent = _track.ThumbStart;
            }
            else
                SetCurrentValue(EndValueProperty, end);

        }
    }

    private double SnapToTick(double value)
    {
        if (!IsSnapToTickEnabled) return value;

        var previous = Minimum;
        var next = Maximum;

        if (Ticks is { Count: > 0 })
        {
            foreach (var tick in Ticks)
            {
                if (DoubleBox.AreClose(tick, value))
                    return value;


                if (DoubleBox.LessThan(tick, value) && DoubleBox.GreaterThan(tick, previous))
                {
                    previous = tick;
                }
                else if (DoubleBox.GreaterThan(tick, value) && DoubleBox.LessThan(tick, next))
                {
                    next = tick;
                }
            }
        }
        else if (DoubleBox.GreaterThan(TickFrequency, 0.0))
        {
            previous = Minimum + Math.Round((value - Minimum) / TickFrequency) * TickFrequency;
            next = Math.Min(Maximum, previous + TickFrequency);
        }

        return DoubleBox.GreaterThanOrClose(value, (previous + next) * 0.5) ? next : previous;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_thumbCurrent == null) return;
        if (e.MouseDevice.LeftButton != MouseButtonState.Pressed) return;
        if (!_thumbCurrent.IsDragging) return;

        var thumbCoordPosition = e.GetPosition(_thumbCurrent);
        var screenCoordPosition = PointFromScreen(thumbCoordPosition);

        if (screenCoordPosition == _previousScreenCoordPosition) return;
        _previousScreenCoordPosition = screenCoordPosition;
        _thumbCurrent.RaiseEvent(new DragDeltaEventArgs(thumbCoordPosition.X - _originThumbPoint.X,
            thumbCoordPosition.Y - _originThumbPoint.Y));
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        _thumbCurrent = null;
    }

    #endregion
}
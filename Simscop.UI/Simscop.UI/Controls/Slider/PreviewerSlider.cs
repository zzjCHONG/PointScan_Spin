using System;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Simscop.UI.Commands.Args;
using Simscop.UI.Interactivity;

// ReSharper disable once CheckNamespace
namespace Simscop.UI.Controls;


[TemplatePart(Name = TrackKey, Type = typeof(Track))]
[TemplatePart(Name = ThumbKey, Type = typeof(FrameworkElement))]
public class PreviewSlider : Slider
{
    private const string TrackKey = "PART_Track";

    private const string ThumbKey = "PART_Thumb";

    private AdornerContainer? _adorner;

    private FrameworkElement? _previewContent;

    private FrameworkElement? _thumb;

    private TranslateTransform? _transform;

    private Track? _track;

    /// <summary>
    /// Content
    /// </summary>
    public static readonly DependencyProperty PreviewContentProperty = DependencyProperty.Register(
        nameof(PreviewContent), typeof(object), typeof(PreviewSlider), new PropertyMetadata(default(object)));

    public object PreviewContent
    {
        get => GetValue(PreviewContentProperty);
        set => SetValue(PreviewContentProperty, value);
    }

    /// <summary>
    /// Content Offset
    /// </summary>
    public static readonly DependencyProperty PreviewContentOffsetProperty = DependencyProperty.Register(
        nameof(PreviewContentOffset), typeof(double), typeof(PreviewSlider), new PropertyMetadata(9.0));

    public double PreviewContentOffset
    {
        get => (double)GetValue(PreviewContentOffsetProperty);
        set => SetValue(PreviewContentOffsetProperty, value);
    }

    public static readonly DependencyProperty PreviewPositionProperty = DependencyProperty.RegisterAttached(
        "PreviewPosition", typeof(double), typeof(PreviewSlider), new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.Inherits));

    public static void SetPreviewPosition(DependencyObject? element, double value)
        => element?.SetValue(PreviewPositionProperty, value);

    public static double GetPreviewPosition(DependencyObject? element)
        => (double)(element?.GetValue(PreviewPositionProperty) ?? (double)0);

    public double PreviewPosition
    {
        get => GetPreviewPosition(_previewContent);
        set => SetPreviewPosition(_previewContent, value);
    }

    /// <summary>
    /// 值改变事件
    /// </summary>
    public static readonly RoutedEvent PreviewPositionChangedEvent =
        EventManager.RegisterRoutedEvent("PreviewPositionChanged", RoutingStrategy.Bubble,
            typeof(EventHandler<FunctionEventArgs<double>>), typeof(PreviewSlider));

    /// <summary>
    /// 值改变事件
    /// </summary>
    public event EventHandler<FunctionEventArgs<double>> PreviewPositionChanged
    {
        add => AddHandler(PreviewPositionChangedEvent, value);
        remove => RemoveHandler(PreviewPositionChangedEvent, value);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_previewContent == null) return;
        //if (e.LeftButton != MouseButtonState.Pressed) return;

        var p = e.GetPosition(_adorner);
        var maximum = Maximum;
        var minimum = Minimum;

        if (_thumb == null || _track == null || _transform == null) return;

        if (Orientation == Orientation.Horizontal)
        {

            var pos = !IsDirectionReversed
                ? (e.GetPosition(this).X - _thumb.ActualWidth * 0.5) / _track.ActualWidth * (maximum - minimum) + minimum
                : (1 - (e.GetPosition(this).X - _thumb.ActualWidth * 0.5) / _track.ActualWidth) * (maximum - minimum) + minimum;
            if (pos > maximum || pos < 0)
            {
                if (_thumb.IsMouseCaptureWithin)
                {
                    PreviewPosition = Value;
                }
                return;
            }

            _transform.X = p.X - _previewContent.ActualWidth * 0.5;
            _transform.Y = _thumb.TranslatePoint(new Point(), _adorner).Y - _previewContent.ActualHeight - PreviewContentOffset;

            PreviewPosition = _thumb.IsMouseCaptureWithin ? Value : pos;
        }
        else
        {
            var pos = !IsDirectionReversed
                ? (1 - (e.GetPosition(this).Y - _thumb.ActualHeight * 0.5) / _track.ActualHeight) * (maximum - minimum) + minimum
                : (e.GetPosition(this).Y - _thumb.ActualHeight * 0.5) / _track.ActualHeight * (maximum - minimum) + minimum;
            if (pos > maximum || pos < 0)
            {
                if (_thumb.IsMouseCaptureWithin)
                {
                    PreviewPosition = Value;
                }
                return;
            }

            _transform.X = _thumb.TranslatePoint(new Point(), _adorner).X - _previewContent.ActualWidth - PreviewContentOffset;
            _transform.Y = p.Y - _previewContent.ActualHeight * 0.5;

            PreviewPosition = _thumb.IsMouseCaptureWithin ? Value : pos;
        }

        RaiseEvent(new FunctionEventArgs<double>(PreviewPositionChangedEvent, this)
        {
            Info = PreviewPosition
        });
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (_adorner != null) return;
        //if (e.LeftButton != MouseButtonState.Pressed) return;


        var layer = AdornerLayer.GetAdornerLayer(this);
        if (layer == null) return;
        _adorner = new AdornerContainer(layer)
        {
            Child = _previewContent
        };
        layer.Add(_adorner);
    }


    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        if (_adorner != null) return;
        //if (e.LeftButton != MouseButtonState.Pressed) return;


        var layer = AdornerLayer.GetAdornerLayer(this);
        if (layer == null) return;
        _adorner = new AdornerContainer(layer)
        {
            Child = _previewContent
        };
        layer.Add(_adorner);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        var layer = AdornerLayer.GetAdornerLayer(this);
        if (layer != null && _adorner != null)
        {
            layer.Remove(_adorner);
        }
        else if (_adorner is { Parent: AdornerLayer parent })
        {
            parent.Remove(_adorner);
        }

        if (_adorner == null) return;

        _adorner.Child = null;
        _adorner = null;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var contentControl = new ContentControl
        {
            DataContext = this
        };
        contentControl.SetBinding(ContentControl.ContentProperty, new Binding(PreviewContentProperty.Name) { Source = this });
        _previewContent = contentControl;

        _track = Template.FindName(TrackKey, this) as Track;
        _thumb = Template.FindName(ThumbKey, this) as FrameworkElement;

        if (_previewContent == null) return;

        _transform = new TranslateTransform();

        _previewContent.HorizontalAlignment = HorizontalAlignment.Left;
        _previewContent.VerticalAlignment = VerticalAlignment.Top;
        _previewContent.RenderTransform = _transform;
    }
}
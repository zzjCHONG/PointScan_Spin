using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Simscop.UI.Controls.Attach;

/**
 * TODO
 * Title
 * ButtonEnable
 *
 */

public class WindowAttach
{
    /**
     * Close
     * Min
     * Restore
     * Max
     *
     * Drag
     *
     */

    #region 三大金刚的功能部分
    // ! 这个属性不包含关闭
    public static readonly DependencyProperty TitleBarButtonStateProperty = DependencyProperty.RegisterAttached(
           "TitleBarButtonState", typeof(WindowState?), typeof(WindowAttach), new PropertyMetadata(null, OnButtonStateChanged));
    private static void OnButtonStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (Button)d;

        if (e.OldValue is WindowState)
        {
            button.Click -= StateButton_Click;

        }

        if (e.NewValue is WindowState)
        {
            button.Click -= StateButton_Click;
            button.Click += StateButton_Click;
        }
    }

    private static void StateButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (DependencyObject)sender;
        var window = Window.GetWindow(button);
        var state = GetTitleBarButtonState(button);
        if (window != null && state != null)
        {
            window.WindowState = state.Value;
        }
    }

    public static WindowState? GetTitleBarButtonState(DependencyObject element)
        => (WindowState?)element.GetValue(TitleBarButtonStateProperty);

    public static void SetTitleBarButtonState(DependencyObject element, WindowState? value)
        => element.SetValue(TitleBarButtonStateProperty, value);

    // ! 关闭按钮
    public static readonly DependencyProperty IsTitleBarCloseButtonProperty = DependencyProperty.RegisterAttached(
           "IsTitleBarCloseButton", typeof(bool), typeof(WindowAttach),
           new PropertyMetadata(default(bool), OnIsCloseButtonChanged));

    private static void OnIsCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (Button)d;

        if (e.OldValue is true)
        {
            button.Click -= CloseButton_Click;
        }

        if (e.NewValue is true)
        {
            button.Click -= CloseButton_Click;
            button.Click += CloseButton_Click;
        }
    }

    private static void CloseButton_Click(object sender, RoutedEventArgs e)
        => Window.GetWindow((DependencyObject)sender)?.Close();


    public static bool GetIsTitleBarCloseButton(DependencyObject element)
        => (bool)element.GetValue(IsTitleBarCloseButtonProperty);

    public static void SetIsTitleBarCloseButton(DependencyObject element, bool value)
        => element.SetValue(IsTitleBarCloseButtonProperty, value);
    #endregion
}


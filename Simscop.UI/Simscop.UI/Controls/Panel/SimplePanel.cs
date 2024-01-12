using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

// ReSharper disable once InvalidXmlDocComment
/**
 * https://www.codeproject.com/Articles/1034445/Understanding-MeasureOverride-and-ArrangeOverride
 *
 * Measure方法自顶而下，递归调用各子控件的Measure方法，Measure方法会把该控件所需的大小控件存在desired size属性中，
 * 控件根据各子控件的desired size 属性确定自身空间大小，并返回自己的desired size
 * Arrange方法发生在Measure中，传入Measure方法计算到的大小，利用控件的位置设置分配子控件的位置
 * 简单来说，这两个方法一个管大小，一个管布局，都需要调用子类的Measure和Measure
 */

// ReSharper disable once CheckNamespace
namespace Simscop.UI.Controls;

public class SimplePanel : Panel
{
    /// <summary>
    /// 传入父容器分配的可用空间，返回该容器根据其子元素大小计算确定的在布局过程中所需的大小。
    /// 用于计算本身及其子控件的大小
    /// </summary>
    /// <param name="availableSize"></param>
    /// <returns></returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var maxSize = new Size();

        foreach (UIElement child in InternalChildren)
            if (child != null)
            {
                child.Measure(availableSize);
                maxSize.Width = Math.Max(maxSize.Width, child.DesiredSize.Width);
                maxSize.Height = Math.Max(maxSize.Height, child.DesiredSize.Height);
            }


        return maxSize;
    }

    /// <summary>
    /// 传入父容器最终分配的控件大小，返回使用的实际大小
    /// 用于布局本身及其子控件的位置和大小
    /// </summary>
    /// <param name="finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in InternalChildren)
            child?.Arrange(new Rect(finalSize));

        return finalSize;
    }
}
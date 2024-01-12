using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

// ReSharper disable once CheckNamespace
namespace Simscop.UI.Common.Converter;

public abstract class BaseMultiValueConvert<T> : MarkupExtension, IMultiValueConverter
    where T : class, new()
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetTypes"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();



    public override object ProvideValue(IServiceProvider serviceProvider)
        => new T();
}
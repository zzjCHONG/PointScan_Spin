using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

// ReSharper disable once CheckNamespace
namespace Simscop.UI.Common.Converter;

public abstract class BaseSignletonMultiValueConvert<T> : MarkupExtension, IMultiValueConverter
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


    /// <summary>
    /// 
    /// </summary>
    private static T? _instance = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
        => _instance ??= new T();
}
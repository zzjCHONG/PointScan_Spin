using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simscop.UI.Common;


public static class VisibilityBox
{
    public static object Get(object? value)
        => value switch
        {
            Visibility.Visible => Visibility.Visible,
            Visibility.Hidden => Visibility.Visible,
            Visibility.Collapsed => Visibility.Visible,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
}

public static class DoubleBox
{
    public static readonly double Epsilon = double.Epsilon;

    public static readonly double Threshold = 1E-06;

    public static readonly double Zero01 = 0.01;

    public static readonly double Zero1 = 0.1;

    public static readonly double Zero = .0;

    public static readonly double One = 1.0;

    public static readonly double Ten = 10.0;

    public static readonly double Hundred = 100.0;

    public static readonly double NaN = double.NaN;

    public static object Max(object v1, object v2)
        => (v1 is double d1 && v2 is double d2) ? Math.Max(d1, d2) : v1;

    public static object Min(object v1, object v2)
        => (v1 is double d1 && v2 is double d2) ? Math.Min(d1, d2) : v1;


    /// <summary>
    /// 判断是否为有意义的double
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValid(object? value)
        => value is double val && !double.IsNaN(val) && !double.IsInfinity(val);

    public static bool IsNegative(object value)
        => IsValid(value) && (double)value < 0;

    public static bool IsNonNegative(object value)
        => IsValid(value) && (double)value >= 0;

    public static bool IsPositive(object value)
        => IsValid(value) && (double)value > 0;

    public static bool IsNonPositive(object value)
        => IsValid(value) && (double)value <= 0;

    public static bool AreClose(object value1, object value2)
        => value1 is double v1 && value2 is double v2 && (value1 == value2 || Math.Abs(v1 - v2) < Threshold);

    /// <summary>
    /// value1 less than value2
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static bool LessThan(object value1, object value2)
        => value1 is double v1 && value2 is double v2 && v1 < v2 && !AreClose(value1, value2);

    /// <summary>
    /// value1 greater than value2
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static bool GreaterThan(object value1, object value2)
        => value1 is double v1 && value2 is double v2 && v1 > v2 && !AreClose(value1, value2);

    /// <summary>
    /// value1 greater or close value2
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static bool GreaterThanOrClose(object value1, object value2)
        => value1 is double v1 && value2 is double v2 && (!(v1 <= v2) || AreClose(value1, value2));
}

public static class BoolBox
{
    public static readonly bool True = true;

    public static readonly bool False = false;
    public static bool Reverse(object value) => !((bool)value);
}

public static class IntBox
{
    public static readonly int Zero = 0;

    public static readonly int One = 1;

    public static readonly int Ten = 10;

    public static readonly int Hundred = 100;

    public static bool IsNegative(object value)
        => (int)value < 0;

    public static bool IsNonNegative(object value)
        => (int)value >= 0;

    public static bool IsPositive(object value)
        => (int)value > 0;

    public static bool IsNonPositive(object value)
        => (int)value <= 0;
}
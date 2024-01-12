using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simscop.UI.Common.Converter;

public class Uint2Brushes : BaseValueConvert<Uint2Brushes>
{
    public Brush BrushOne { get; set; } = Brushes.Red;
    public Brush BrushTwo { get; set; } = Brushes.Red;
    public Brush BrushThree { get; set; } = Brushes.Red;
    public Brush BrushFour { get; set; } = Brushes.Red;
    public Brush BrushFive { get; set; } = Brushes.Red;

    private Brush GetIndex(uint value)
        => value switch
        {
            1=>BrushOne , 2=>BrushTwo, 3=>BrushThree, 4=>BrushFour, 5 => BrushFive,
            _ => throw new NotImplementedException()
        };

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            bool val => val ? BrushOne : BrushTwo,
            uint val => GetIndex(val),
            int val => GetIndex((uint)val),
            _ => value
        };
}
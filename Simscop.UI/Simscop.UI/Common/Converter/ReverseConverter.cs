using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simscop.UI.Common.Converter;

public class ReverseConverter : BaseValueConvert<ReverseConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            bool bValue => !bValue,

            _ => value
        };
}
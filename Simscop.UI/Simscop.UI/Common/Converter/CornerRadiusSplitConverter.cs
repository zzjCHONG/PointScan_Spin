using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simscop.UI.Common.Converter;

public class CornerRadiusSplitConverter:BaseValueConvert<CornerRadiusSplitConverter>
{
    public string Radius { get; set; } = "";

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not CornerRadius cornerRadius) return value;
 
        var arr = Radius.Split(',');
        if (arr.Length != 4) return cornerRadius;

        return new CornerRadius(
            arr[0].Equals("1") ? cornerRadius.TopLeft : 0,
            arr[1].Equals("1") ? cornerRadius.TopRight : 0,
            arr[2].Equals("1") ? cornerRadius.BottomRight : 0,
            arr[3].Equals("1") ? cornerRadius.BottomLeft : 0);
    }
}
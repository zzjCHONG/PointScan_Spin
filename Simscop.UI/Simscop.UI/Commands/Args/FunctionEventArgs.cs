using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Simscop.UI.Commands.Args;

public class FunctionEventArgs<T> : RoutedEventArgs
{
    public FunctionEventArgs(T info)
    {
        Info = info;
    }

    public FunctionEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public T? Info { get; set; }
}

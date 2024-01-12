using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;

namespace Simscop.Spindisk.Core.Common;

public static class LogHelper
{
    public static void SendDeubg(object obj, string msg)
        => Debug.WriteLine("this is a debug");

    }
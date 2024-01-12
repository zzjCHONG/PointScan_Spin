using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.Common;

public static class ValueHelper
{
    public static string DefaultSaveDirectory => System.IO.Directory.GetCurrentDirectory();

    public static string GetTime() => System.DateTime.Now.ToString("yyyyMMdd_HH_mm_ss");

    public const string EmptyString = "";
}
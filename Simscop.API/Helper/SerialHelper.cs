using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Simscop.API.Helper;

public static class SerialHelper
{
    public static List<string> GetAllCom() => SerialPort.GetPortNames().ToList();
}
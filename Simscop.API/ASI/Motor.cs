using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Simscop.API.ASI
{
    
    public class Motor
    {
        public bool isBusy = false;

        public double x = 0;

        public double y = 0;

        public double z = 0;

        public string? _portName;
        public enum Axis
        {
            X = 1, Y = 2, Z = 3,
        }


        SerialPort serialPort = new SerialPort()
        {
            BaudRate = 9600,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
        };

        public bool OpenCom()
        {
            if (Valid())
                serialPort.PortName = _portName;
            else return false;
            try
            {
                if (_portName != null && !serialPort.IsOpen)
                {
                    serialPort.Open();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("串口已开启或已被占用", "Error");
                serialPort.Close();
                return false;
            }

            return true;

        }

        public bool CloseCom()
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }



        public void ReadPosition()
        {
            serialPort.Write("W X\r\n");
            serialPort.Write("W Y\r\n");
            serialPort.Write("W Z\r\n");

            Convert(serialPort.ReadExisting());

        }

        /// <summary>
        /// 相对位置移动
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="value"></param>
        public bool MoveRelative(Axis axis, double value)
        {
            switch (axis)
            {
                case Axis.X:
                    serialPort.Write($"R X={value * 10}\r\n");
                    break;
                case Axis.Y:
                    serialPort.Write($"R Y={value * 10}\r\n");
                    break;
                case Axis.Z:
                    serialPort.Write($"R Z={value * 10}\r\n");
                    break;
            }
            return true;
        }

        /// <summary>
        /// 绝对位置移动
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="value"></param>
        public bool MoveAbsolute(Axis axis, double value)
        {
            switch (axis)
            {
                case Axis.X:
                    serialPort.Write($"M X={value * 10}\r\n");
                    break;
                case Axis.Y:
                    serialPort.Write($"M Y={value * 10}\r\n");
                    break;
                case Axis.Z:
                    serialPort.Write($"M Z={value * 10}\r\n");
                    break;
            }
            return true;
        }

        /// <summary>
        /// 归零
        /// </summary>
        public void Home()
        {
            //if (isBusy = true)
            //    return;

            //isBusy = true;

            serialPort.Write("M X=0\r\n");
            serialPort.Write("M Y=0\r\n");
            serialPort.Write("M Z=0\r\n");
        }

        /// <summary>
        /// 设置当前位置为原点
        /// </summary>
        /// <returns></returns>
        public bool SetHome()
        {
            serialPort.Write("H X\r\n");
            serialPort.Write("H Y\r\n");
            serialPort.Write("H Z\r\n");
            return true;
        }

        private void Convert(string input)
        {
            try
            {
                MatchCollection matches = Regex.Matches(input, @":A (?<Value>[-\d]+)");

                if (matches.Count >= 3)
                {
                    double[] values = new double[3];

                    for (int i = 0; i < 3; i++)
                    {
                        if (matches[i].Success)
                        {
                            values[i] = double.Parse(matches[i].Groups["Value"].Value);
                        }
                    }

                    x = values[0] / 10;
                    y = values[1] / 10;
                    z = values[2] / 10;
                }
            }
            catch
            {

            }

        }

        public bool Valid()
        {
            while (true)
            {
                string[] portNames = SerialPort.GetPortNames();

                foreach (string portName in portNames)
                {


                    try
                    {
                        serialPort.PortName = portName;
                        serialPort.Open();

                        serialPort.Write("N\r\n");

                        Thread.Sleep(100);
                        var value = serialPort.ReadExisting();


                        if (value == ":A ASI-MS2000-XYBR-ZFR-USB \r\n")
                        {
                            Console.WriteLine($"Connected to {value} on port: {portName}");

                            serialPort.Close();
                            _portName = portName;
                            break;
                        }
                        serialPort.Close();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to connect to port {portName}: {ex.Message}");
                        serialPort.Close();
                        Thread.Sleep(100);
                    }
                }
                if (_portName != null) return true;
                return false;
            }
        }
    }

}

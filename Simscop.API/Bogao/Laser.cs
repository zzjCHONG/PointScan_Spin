using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Simscop.API.Bogao
{
    public class Laser
    {
        private SerialPort? _port = new()
        {
            BaudRate = 9600,
            StopBits = StopBits.One,
            DataBits = 8,
            Parity = Parity.None,
        };

        public string? _portName;

        internal string _connectState = string.Empty;

        public bool OpenCom()
        {
            try
            {
                if (Valid())
                {
                    _port.PortName = _portName;
                }
                else
                {
                    _connectState = "No available laser serial port found";
                    return false;
                }
                if (!_port.IsOpen)
                {
                    _port.Open();
                    _connectState = "Initialize laser completed!";
                    return true;
                }
                else
                {
                    _connectState = $"Failed to connect to port {_portName}";
                    Console.WriteLine(_connectState);
                    return false;
                }
            }
            catch (Exception)
            {
                _connectState = $"Failed to connect to port {_portName}";
                Console.WriteLine(_connectState);
                return false;
            } 
        }

        public bool CloseCom()
        {

            if (_port.IsOpen)
            {
                _port.Close();
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to disconnect to port {_portName}");
                return false;
            }
        }

        public bool OpenChannel(int count)
        {

            string Text = $"*OPEN{count}#";
            if (_port.IsOpen)
            {
                _port.Write(Text);
                Thread.Sleep(100);
                var value = _port.ReadExisting();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CloseChannel(int count)
        {
            string Text = $"*CLOSE{count}#";
            if (_port.IsOpen)
            {
                _port.Write(Text);
                Thread.Sleep(100);
                var value = _port.ReadExisting();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetStatus(int count,out bool status)
        {
            status = false;
            string Text = $"*State{count}?";
            if (_port.IsOpen)
            {
                _port.Write(Text);
                Thread.Sleep(100);
                var value = _port.ReadExisting();
                switch (ConvertStatus(value))
                {
                    case 1:
                        status =  true;
                        return true;
                    case 0:
                        status =  false;
                        return true;
                    case -1:
                        throw new Exception("获取失败");
                }
            }
            return true;
        }

        public int ConvertStatus(string input)
        {
            int index = input.IndexOf("#");

            if (index != -1 && index - 1 >= 0)
            {
                char value = input[index - 1];
                int result = int.Parse(value.ToString());
                return result;
            }
            else
            {
                Console.WriteLine("Conversion failed");
                return -1;
            }
        }

        public double ConvertPower(string input)
        {
            Match match = Regex.Match(input, @"\d+\.\d+");

            if (match.Success)
            {
                // 获取匹配的数字字符串
                string numberString = match.Value;

                // 将字符串表示转换为 double（或其他适当的类型）
                if (double.TryParse(numberString, out double result))
                {
                    return result;
                }
            }
            else
            {
                Console.WriteLine("Pattern not found.");
                return -1;
            }

            return 0;
        }

        public bool GetPower(int count,out int value)
        {
            double result = 0;
            value = 0;
            string Text = $"*Power_{count}?";
            if (_port.IsOpen)
            {
                _port.Write(Text);
                Thread.Sleep(100);
                var temp = _port.ReadExisting();
                result = ConvertPower(temp);
            }
            
            switch (count)
            {
                case 1:
                    value = (int)(result / 620 * 100);
                    break;
                case 2:
                    value = (int)(result / 530 * 100);
                    break;
                case 3:
                    value = (int)(result / 580 * 100);
                    break;
                case 4:
                    value = (int)(result / 610 * 100);
                    break;
            }
            return true;
        }

        public bool SetPower(int count, double power)
        {
            double value = 0;
            switch (count)
            {
                case 1:
                    value = (int)(power * 620 / 100);
                    break;
                case 2:
                    value = (int)(power * 530 / 100);
                    break;
                case 3:
                    value = (int)(power * 580 / 100);
                    break;
                case 4:
                    value = (int)(power * 610 / 100);
                    break;
            }
            string Text = $"*Set_Power_{count} {value}#";
            if (_port.IsOpen)
            {
                _port.Write(Text);
                Console.WriteLine(_port.ReadExisting());
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Valid()
        {
            while (true)
            {
                string[] portNames = SerialPort.GetPortNames();

                _portName = Serach("*IDN?", "*MCU-DA-4COM#\r\n", _port, 1000, true);

                return _portName != null;
            }
        }

        /// <summary>
        /// 使用验证信息查找串口
        /// </summary>
        /// <param name="valid">验证使用的命令</param>
        /// <param name="receive">命令接收后的对比参考字符</param>
        /// <param name="wait">接收等待时间</param>
        /// <param name="isContain">是否使用Contain的方式判断，如果为否，需要receive和接收的串口信息一致</param>
        /// <returns>
        /// 如果返回为空，则未找到需要的串口
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static string? Serach(string valid, string receive, SerialPort _port, int wait = 100, bool isContain = true)
        {
            var ports = SerialPort.GetPortNames();

            foreach (var name in ports)
            {

                try
                {
                    _port.PortName = name;
                    _port.Open();
                    if (!_port.IsOpen) throw new Exception("The ComName can`t open.");
                    _port.Write(valid);
                    Thread.Sleep(wait);

                    var str = _port.ReadExisting();

                    if (isContain ? str.Contains(receive) : str == receive) return name;

                }
                catch (UnauthorizedAccessException e) // the ComName is connected
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    // 这里超时错误SerialPort没有做单独的区分，因此这里错误不需要抛出，会出现没必要的错误
                    //throw new Exception(e.Message, e);
                }
                finally
                {
                    _port.Close();
                }
            }

            return null;
        }
    }
}

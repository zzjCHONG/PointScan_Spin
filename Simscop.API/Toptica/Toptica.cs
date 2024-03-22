using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Simscop.API.TopLaser
{
    public class Toptica
    {
        private string? _portName;
        private string? _connectState;
        private SerialPort _serialPort = new()
        {
            BaudRate = 115200,
            StopBits = StopBits.One,
            DataBits = 8,
            Parity = Parity.None,
        };

        public bool SetLaserState(int count, bool isEnable)
        {
            try
            {
                if (!_serialPort.IsOpen) return false;
                string laserChannel = string.Empty;
                string state = isEnable ? "t" : "f";
                switch (count + 1)
                {
                    case 2://RED
                        laserChannel = "laser3";
                        break;
                    case 3://GREEN
                        laserChannel = "laser2";
                        break;
                    case 4://BLUE
                        laserChannel = "laser1";
                        break;
                    case 1://PURPLE
                        laserChannel = "laser4";
                        break;
                }
                //enable & cw 需同时设置
                if (!Write($"(param-set! '{laserChannel}:enable #{state})\r\n")) return false;
                Thread.Sleep(100);
                if (!Write($"(param-set! '{laserChannel}:cw #{state})\r\n")) return false;
                Thread.Sleep(100);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool SetChannelPower(int count, double value)
        {
            try
            {
                string laserChannel = string.Empty;
                string channel = string.Empty;
                switch (count + 1)
                {
                    case 2:
                        laserChannel = "laser3";//_TRITCValue
                        break;
                    case 3:
                        laserChannel = "laser2";//_FITCValue
                        break;
                    case 4:
                        laserChannel = "laser1";//_DAPIValue
                        break;
                    case 1:
                        laserChannel = "laser4";//_CY5Value
                        break;
                }
                if (!Write($"(param-set! '{laserChannel}:level " + value.ToString() + ")\r\n")) return false;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool GetChannelPower(int count, out int value)
        {
            value = 0;
            try
            {
                string laserChannel = string.Empty;
                switch (count + 1)
                {
                    case 2://RED
                        laserChannel = "laser3";
                        break;
                    case 3://GREEN
                        laserChannel = "laser2";
                        break;
                    case 4://BLUE
                        laserChannel = "laser1";
                        break;
                    case 1://PURPLE
                        laserChannel = "laser4";
                        break;
                }
                string str = $"(param-ref '{laserChannel}:level)\r\n";
                if (!Write(str)) return false;
                Thread.Sleep(100);
                if (!GetContent(str, out string rtn)) return false;
                value = (int)Math.Round(double.Parse(rtn));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                Thread.Sleep(100);
            }
        }

        public bool GetStatus(int count, out bool status)
        {
            status = false;
            try
            {
                string laserChannel = string.Empty;
                switch (count + 1)
                {
                    case 2://RED
                        laserChannel = "laser3";
                        break;
                    case 3://GREEN
                        laserChannel = "laser2";
                        break;
                    case 4://BLUE
                        laserChannel = "laser1";
                        break;
                    case 1://PURPLE
                        laserChannel = "laser4";
                        break;
                }
                //判断CW&enable
                string strEnable = $"(param-ref '{laserChannel}:enable)\r\n";
                if (!Write(strEnable)) return false;
                Thread.Sleep(100);
                if (!GetContent(strEnable, out string rtn)) return false;
                if (rtn.Contains("#f"))
                {
                    status = false;
                }
                else if (rtn.Contains("#t"))
                {
                    status = true;
                }

                string strCW = $"(param-ref '{laserChannel}:cw)\r\n";
                if (!Write(strCW)) return false;
                Thread.Sleep(100);
                if (!GetContent(strCW, out string rtn2)) return false;
                if (rtn2.Contains("#f"))
                {
                    status &= false;
                }
                else if (rtn.Contains("#t"))
                {
                    status &= true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                Thread.Sleep(100);
            }
        }

        public string GetConnectState()
        {
            return _connectState ?? "Get connect state failed...";
        }

        public bool Connect()
        {
            if (Valid())
            {
                _serialPort.PortName = _portName;

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Open();
                    _connectState = "Initialize laser completed!";

                }
                else
                {
                    _connectState = $"Failed to connect to port {_portName}";
                }
                return _serialPort.IsOpen;
            }
            else
            {
                _connectState = "No available laser serial port found";
            }
            return false;
        }

        public bool Disconnect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
            return _serialPort.IsOpen == false;
        }

        private bool Write(string data)
        {
            if (!_serialPort.IsOpen)
                return false;

            _serialPort.Write(data);
            return true;
        }

        static void Check(string[] args)
        {
            string portName = "COM1"; // 替换为您要检查的串口名称

            SerialPort port = new SerialPort(portName);

            try
            {
                port.Open();
                Console.WriteLine($"串口 {portName} 未被占用");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"串口 {portName} 已被占用");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开串口 {portName} 发生错误: {ex.Message}");
            }
            finally
            {
                if (port.IsOpen)
                    port.Close();
            }
        }

        private bool CheckPort(string portName)
        {
            SerialPort port = new SerialPort(portName);
            try
            {
                port.Open();
                Console.WriteLine($"串口 {portName} 未被占用");
                if (port.IsOpen) port.Close();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"串口 {portName} 已被占用");

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开串口 {portName} 发生错误: {ex.Message}");
                return true;
            }
        }


        private bool Valid()
        {
            try
            {
                string[] portNames = SerialPort.GetPortNames();
                foreach (string portName in portNames)
                {
                    if (!CheckPort(portName)) continue;
                    if (!_serialPort.IsOpen)
                    {
                        _serialPort.PortName = portName;
                        _serialPort.Open();
                    }
                    _serialPort.Write("(param-ref 'serial-number)\r\n");
                    Thread.Sleep(100);
                    if (_serialPort.ReadExisting().Contains(
                        "(param-ref 'serial-number)\r\n\"iCHROME-CLE_50218\"\r\n>"))
                    {
                        _portName = portName;
                        _serialPort.Close();
                        break;
                    }
                    _serialPort.Close();
                }
                return !string.IsNullOrEmpty(_portName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool GetContent(string strInput, out string strOutput)
        {
            strOutput = string.Empty;
            try
            {
                var str = _serialPort.ReadExisting();
                if (string.IsNullOrEmpty(str)) return false;
                int index = str.LastIndexOf(strInput);
                strOutput = str.Substring(index + strInput.Length, str.Length - index - strInput.Length).Replace("\r\n", "").Replace(">", "");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

using System;
using System.IO.Ports;
using System.Threading;

namespace Simscop.API.Thorlabs
{
    public class ELL6
    {
        private readonly string? _serialNumber;
        private string? _portName;
        private string? _portChannel;

        public ELL6(bool isDeviceFront)
        {
            _serialNumber = isDeviceFront ? "10600345" : "10600320";
        }

        private SerialPort _serialPort = new()
        {
            BaudRate = 9600,
            StopBits = StopBits.One,
            DataBits = 8,
            Parity = Parity.None,
        };

        public bool Connect(out string msg)
        {
            msg = string.Empty;
            if (Valid())
            {
                _serialPort.PortName = _portName;
                if (!_serialPort.IsOpen) _serialPort.Open();

                if (_serialPort.IsOpen)
                {
                    msg = "Initialize slider completed!";
                }
                else
                {
                    msg = $"Failed to connect to port {_portName}";
                }
                return _serialPort.IsOpen;
            }
            msg = "No available slider serial port found";
            return false;
        }

        public bool Disconnect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
            return _serialPort.IsOpen==false;
        }

        public bool BackWard()
        {
            if (!_serialPort.IsOpen) return false;
            string data = $"{_portChannel}bw";
            if (!Write(data)) return false;
            if (!CheckMoveRtn(_serialPort.ReadExisting())) return false;
            return true;
        }

        public bool ForWard()
        {
            if (!_serialPort.IsOpen) return false;
            string data = $"{_portChannel}fw";
            if (!Write(data)) return false;
            if (!CheckMoveRtn(_serialPort.ReadExisting())) return false;
            return true;
        }

        public bool Home(bool isLeftSide)
        {
            if (!_serialPort.IsOpen) return false;
            int code = isLeftSide ? 1 : 0;
            string data = $"{_portChannel}ho{code}";
            if (!Write(data)) return false;
            if (!CheckMoveRtn(_serialPort.ReadExisting())) return false;
            return true;
        }

        private bool CheckMoveRtn(string rtn)
        {
            try
            {
                if (rtn.Substring(0, 3) == $"{_portChannel}PO")
                {
                    string str = rtn.Substring(3, rtn.Length - 5);//0000001F
                    int position = Convert.ToInt32(str, 16);
                    return true;
                }
                else
                {
                    if (rtn.Substring(0, 2) == $"{_portChannel}GS")
                    {
                        int code = Convert.ToInt32(rtn.Substring(2, rtn.Length - 1));
                        var A = (GSErrorEnum)code;
                        Console.WriteLine((GSErrorEnum)code);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        private bool Write(string data)
        {
            if (!_serialPort.IsOpen) 
                return false;

            _serialPort.Write(data);
            return true;
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
                    _serialPort.Write($"0in");
                    Thread.Sleep(100);
                    if (CheckRead(_serialPort.ReadExisting()))
                    {
                        _portChannel = "0";//有多个pin查找，此处省略
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

        private bool CheckRead(string str)
        {
            //0 IN 06 10600345 2023 12 01 001F 00000000  --
            //0 IN 06 10600320 2023 12 01 001F 00000000  --
            try
            {
                if (string.IsNullOrEmpty(str)) return false;
                string serialNumber = str.Remove(str.Length - 4, 4).Substring(5, 8);
                if (serialNumber != _serialNumber) return false;//序列号验证
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public enum GSErrorEnum
        {
            NoError,
            CommunicationTimeOut,
            MechanicalTimeOut,
            CommandErrorOrNotSupported,
            ValueOutofRange,
            ModuleIsolated,
            ModuleOutofIsolation,
            InitializingError,
            ThermalError,
            Busy,
            SensorError,
            MotorError,
            OutofRange,
            OverCurrentError,
            Reserved,
        }
    }

}



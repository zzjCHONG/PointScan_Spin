using NationalInstruments;
using NationalInstruments.DAQmx;
using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Simscop.Spindisk.Core.NICamera
{
    public class Card
    {
        static Wave _wave = new();

        public NationalInstruments.DAQmx.Task? AOTask { get; set; }

        public NationalInstruments.DAQmx.Task? AITask { get; set; }

        public string? AoSource { get; set; } = "Dev1/ao0:1";//输出，振镜，默认双通道

        public string? AiSource { get; set; } = "Dev1/ai0:1";//输入，相机，默认双通道

        public string? TriggerSource { get; set; } = "/Dev1/ao/StartTrigger";//时钟触发，同步

        public bool GetDevices(out List<string> devices)
        {
            devices = new List<string>();
            try
            {
                if(isDisposeTask) return true;
                string[] deviceNames = DaqSystem.Local.Devices;
                foreach (string device in deviceNames)
                {
                    devices.Add(device);
                }
                return true;
            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"GetDevices DAQmx Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public bool CreateAoTask()
        {
            if (isDisposeTask) return true;
            string taskName = $"AOStart{DateTime.Now.ToString("fff")}";
            try
            {
                if (!Valid()) return false;
                AOTask = null;
                AOTask = new NationalInstruments.DAQmx.Task(taskName);
                AOTask.AOChannels.CreateVoltageChannel(AoSource, "", _wave.MinAO, _wave.MaxAO, AOVoltageUnits.Volts);//创建电压输出通道
                AOTask.Timing.ConfigureSampleClock("", _wave._rate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, _wave._sampsPerChan);
                //配置数据采样时钟，rate始终速率-每秒采样数量，下降沿触发，samples采样模式-有限样本，samplesPerChannel各通道样本数量（有限样本模式下）

                var writerMul = new AnalogMultiChannelWriter(AOTask?.Stream);//NI设置：Analog-multichannel-multisamples-1DWaveform
                if (_wave._waveformArray == null) return false;
                double[]? firstHalf = _wave._waveformArray.Take(_wave._sampsPerChan).ToArray();
                double[]? secondHalf = _wave._waveformArray.Skip(_wave._sampsPerChan).ToArray();
                double[,]? newArray = CombineWaveArrays(firstHalf, secondHalf);

                AnalogWaveform<double>[] waveformArrayTram = new AnalogWaveform<double>[_wave._waveformArray.Length];
                waveformArrayTram = AnalogWaveform<double>.FromArray2D(newArray);
                writerMul.WriteWaveform(false, waveformArrayTram);

                return true;
            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"CreateAoTask DAQmx Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CreateAoTask" + ex.Message);
                return false;
            }
        }

        public bool CreateAiTask()
        {
            isDisposeTask=false;
            try
            {
                if (!Valid()) return false;
                AITask = null;
                AITask = new NationalInstruments.DAQmx.Task("AIStart");
                AITask.AIChannels.CreateVoltageChannel(AiSource, "", AITerminalConfiguration.Rse, _wave.MinAI, _wave.MaxAI, AIVoltageUnits.Volts);
                AITask.Timing.ConfigureSampleClock("", _wave._rate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, _wave._sampsPerChan);
                AITask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(TriggerSource, DigitalEdgeStartTriggerEdge.Rising);//信号同步
                AITask?.Control(TaskAction.Verify);
                return true;
            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"CreateAiTask DAQmx Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CreateAiTask" + ex.Message);
                return false;
            }
        }

        public bool StartTask(byte index)
        {
            if (isDisposeTask) return true;
            try
            {  
                var task = index == 0 ? AOTask : AITask;
                task?.Start();
                return true;
            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"StartTask DAQmx Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartTask" + ex.Message);
                return false;
            }
        }

        public bool StopTask(byte index)
        {
            if (isDisposeTask) return true;
            try
            {
                var task = index == 0 ? AOTask : AITask;
                task?.Stop();
                return true;
            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"StopTask DAQmx Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StopTask" + ex.Message);
                return false;
            }
        }

        public bool WaitUntilDone(byte index, int millisecond)
        {
            if (isDisposeTask) return true;
            try
            {
                var task = index == 0 ? AOTask : AITask;
                if (task == null) return false  ;
                if (millisecond == 0)
                {
                    task?.WaitUntilDone();
                }
                else
                {
                    task?.WaitUntilDone(millisecond);
                }
                return true;
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"WaitUntilDone-TimeoutException: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WaitUntilDone: {ex.Message}");
                return false;
            }
        }

        private static bool isDisposeTask=false;

        public bool DisposeTask(byte index)
        {
            var task = index == 0 ? AOTask : AITask;
            try
            {
                isDisposeTask= true;
                task?.Dispose();          
                task = index == 0 ? AOTask = null : AITask = null;
                return true;
            }
            catch (ObjectDisposedException)
            {
                // 如果任务已经被释放，则在这里处理 ObjectDisposedException 异常
                Debug.WriteLine($"Task-{task} has already been disposed.");
                return false;
            }
            catch (DaqException ex)
            {
                // 处理 DAQmx 异常
                Debug.WriteLine("DisposeTask DAQmxException occurred: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // 处理其他类型的异常
                Debug.WriteLine("DisposeTask： " + ex.Message);
                return false;
            }
        }

        public void RefrashWavePara()
        {
            Wave wave = new Wave();
            _wave = wave;
        }

        private bool Valid()
        {
            if (!GetDevices(out List<string> devices) || devices.Count == 0) return false;

            string device = devices[0];//默认一个通道
            AoSource = AoSource?.Replace("Dev1", device);  //输出，振镜，双通道
            AiSource = AiSource?.Replace("Dev1", device); //输入，相机，双通道
            TriggerSource = TriggerSource?.Replace("Dev1", device);//触发
            return true;
        }

        public bool ImageDataOutput(out int x, out int y, out List<short[]> resultArrayList)
        {
            x = 0;
            y = 0;
            resultArrayList = new List<short[]>();
            try
            {
                if (isDisposeTask) return true;
                if (!GetImageOriginData(out List<double[]> imageDataList)) return false;
                for (int i = 0; i < imageDataList.Count; i++)
                {
                    if (!ConverterImageData(imageDataList[i], out x, out y, out short[] resultArray)) return false;
                    resultArrayList.Add(resultArray);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageDataOutput Error:" + ex.Message);
                return false;
            }
        }

        private bool GetImageOriginData(out List<double[]> imageDataList)
        {
            imageDataList = new List<double[]>();
            try
            {
                if (isDisposeTask) return true;

                if(AITask==null||AOTask==null|| AITask?.Stream==null) return false;
                AnalogMultiChannelReader reader = new AnalogMultiChannelReader(AITask?.Stream);
                double[,]? dataMul = reader.ReadMultiSample(_wave._sampsPerChan);
                var row = dataMul.GetLength(0);
                var col = dataMul.GetLength(1);
                for (int i = 0; i < row; i++)
                {
                    double[] imageData = new double[_wave._sampsPerChan];
                    for (int j = 0; j < col; j++)
                    {
                        imageData[j] = dataMul[i, j];
                    }
                    imageDataList.Add(imageData);
                }
                return true;

            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"GetImageOriginData DAQmx Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetImageOriginData" + ex.Message);
                return false;
            }
        }

        private bool ConverterImageData(double[] imageData, out int x, out int y, out short[] resultArray)
        {
            double max = imageData.Max();
            double min = imageData.Min();
            double denominator = max - min;
            x = _wave.XPts - _wave.XMargin;
            y = _wave.YPts - _wave.YMargin;
            resultArray = new short[x * y];
            try
            {
                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        //1、计算待填入的新数组的序号
                        int arrayIndex = i * (_wave.XPts - _wave.XMargin) + j;

                        //2、计算待填入的图像信息数组的序号
                        int imageIndex = 0;
                        if (Convert.ToBoolean(_wave.WaveModel))//三角波
                        {
                            if (i % 2 == 0)
                            {
                                //偶数行
                                imageIndex = i * _wave.XPts + j + _wave.XMargin;
                            }
                            else
                            {
                                //奇数行，前边若干位舍弃，后边数逐一向前补齐，因增加了X的偏移，有效图像数量能够对应
                                //imageIndex = i * _wave.XPts + (_wave.XPts - j - 1 - _wave.XMargin);//不做任何处理对应的奇数行图像序号，重影
                                imageIndex = i * _wave.XPts + (_wave.XPts - j - 1 - _wave.XMargin) + _wave.XOffsetforTriangle;
                            }
                        }
                        else//锯齿波
                        {
                            imageIndex = i * _wave.XPts + j + _wave.XMargin;
                        }

                        //3、图像归一化
                        double data = (imageData[imageIndex] - min) / denominator * short.MaxValue;
                        if (double.IsNaN(data)) data = 0;

                        //4、填入对应数组
                        resultArray[arrayIndex] = (short)data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConverterImageData" + ex.Message);
                return false;
            }

            return true;
        }

        private double[,] CombineWaveArrays(double[] array1, double[] array2)
        {
            try
            {
                int length = array1.Length;
                double[,] resultArray = new double[2, length];
                for (int i = 0; i < array1.Length; i++)
                {
                    resultArray[0, i] = array1[i];
                    resultArray[1, i] = array2[i];
                }
                return resultArray;
            }
            catch (Exception )
            {
                throw new Exception();
            }
        }

        private void ArrayTransferinTriggleWave(int removeCount, short[] inputArray, out short[] outputArray)
        {
            outputArray = new short[inputArray.Length];

            // 舍弃前removeCount个数字       
            short[] newArray = new short[inputArray.Length - removeCount];
            Array.Copy(inputArray, removeCount, newArray, 0, newArray.Length);

            //将剩余数字前移
            Array.Copy(newArray, 0, inputArray, 0, newArray.Length);

            //末位的10个数字补零
            Array.Clear(inputArray, newArray.Length, 10);

            outputArray = inputArray;

        }
    }
}

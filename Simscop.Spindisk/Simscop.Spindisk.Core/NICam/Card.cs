using NationalInstruments;
using NationalInstruments.DAQmx;
using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.NICamera
{
    public class Card
    {
        static bool isDisposeTask = false;
        static Wave _wave = new();

        public NationalInstruments.DAQmx.Task? AOTask { get; set; } = null;

        public NationalInstruments.DAQmx.Task? AITask { get; set; } = null;

        public string? AoSource { get; set; } = "Dev1/ao0:1";//输出，振镜，双通道

        public string? AiSource { get; set; } = "Dev1/ai0:1";//输入，相机，双通道

        public string? TriggerSource { get; set; } = "/Dev1/ao/StartTrigger";//时钟触发，同步

        public bool CreateAoTask()
        {
            double[]? firstHalf;
            double[]? secondHalf;
            double[,]? newArray;
            try
            {
                isDisposeTask=false;
                //Init
                if (!Valid()) return false;
                AOTask = new NationalInstruments.DAQmx.Task("AOStart");
                AOTask.AOChannels.CreateVoltageChannel(AoSource, "", _wave.MinAO, _wave.MaxAO, AOVoltageUnits.Volts);//创建电压输出通道
                AOTask.Timing.ConfigureSampleClock("", _wave._rate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, _wave._sampsPerChan);
                //配置数据采样时钟，rate始终速率-每秒采样数量，下降沿触发，samples采样模式-有限样本，samplesPerChannel各通道样本数量（有限样本模式下）

                var writerMul = new AnalogMultiChannelWriter(AOTask?.Stream);//NI设置：Analog-multichannel-multisamples-1DWaveform
                if (_wave._waveformArray == null) return false;

                firstHalf = _wave._waveformArray.Take(_wave._sampsPerChan).ToArray();
                secondHalf = _wave._waveformArray.Skip(_wave._sampsPerChan).ToArray();
                newArray = CombineArrays(firstHalf, secondHalf);//将波形拆分成XY数组发送

                AnalogWaveform<double>[] waveformArrayTram = new AnalogWaveform<double>[_wave._waveformArray.Length];
                waveformArrayTram = AnalogWaveform<double>.FromArray2D(newArray);
                writerMul.WriteWaveform(false, waveformArrayTram);
                //writerMul.WriteMultiSample(false, newArray);

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
            finally
            {
                firstHalf = null;
                secondHalf = null;
                newArray = null;
            }
        }

        public bool CreateAiTask()
        {
            isDisposeTask = false;
            try
            {
                if (!Valid()) return false;
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

        public void StartTask(byte index)
        {
            try
            {
                if (isDisposeTask)
                {
                    Debug.WriteLine("StartTask return");
                    return;
                }
                    
                var task = index == 0 ? AOTask : AITask;
                task?.Start();
            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"StartTask DAQmx Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartTask" + ex.Message);
            }
        }

        public void StopTask(byte index)
        {
            try
            {
                if (isDisposeTask)
                {
                    Debug.WriteLine("StopTask return");
                    return;
                }
                var task = index == 0 ? AOTask : AITask;
                task?.Stop();
            }
            catch (DaqException ex)
            {
                Debug.WriteLine($"StopTask DAQmx Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StopTask" + ex.Message);
            }
        }

        public void WaitUntilDone(byte index, int millisecond)
        {
            //double timeoutMilliSecond = 5000;//单位：毫秒
            //TimeSpan timeoutSecond = TimeSpan.FromSeconds(5);//单位：秒
            try
            {
                if (isDisposeTask)
                {
                    Debug.WriteLine("WaitUntilDone return");
                    return;
                }
                var task = index == 0 ? AOTask : AITask;
                if (task == null) return;
                if (millisecond == 0)
                {
                    task?.WaitUntilDone();
                }
                else
                {
                    task?.WaitUntilDone(millisecond);
                }
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"WaitUntilDone-TimeoutException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WaitUntilDone: {ex.Message}");
            }
        }

        public void DisposeTask(byte index)
        {
            isDisposeTask=true;
            var task = index == 0 ? AOTask : AITask;
            try
            {
                //if (task != null || task?.IsDone == false) task.WaitUntilDone();
                task?.Dispose();
                task = index == 0 ? AOTask = null : AITask = null;
            }
            catch (ObjectDisposedException)
            {
                // 如果任务已经被释放，则在这里处理 ObjectDisposedException 异常
                Debug.WriteLine($"Task-{task} has already been disposed.");
            }
            catch (DaqException ex)
            {
                // 处理 DAQmx 异常
                Debug.WriteLine("DisposeTask DAQmxException occurred: " + ex.Message);
            }
            catch (Exception ex)
            {
                // 处理其他类型的异常
                Console.WriteLine("DisposeTask： " + ex.Message);
            }
        }

        public bool ReadImage(out List<Mat> mats)
        {
            mats = new List<Mat>();
            if (isDisposeTask)
            {
                Debug.WriteLine("ReadImage return");
                return false;
            }      
            GetImageData(out List<double[]> imageDataList);
            ConverterMulImage(imageDataList, out mats);
            return true;
        }

        public bool GetDevices(out List<string> devices)
        {
            devices = new List<string>();
            try
            {
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

        private bool GetImageData(out List<double[]> imageDataList)
        {
            imageDataList = new List<double[]>();
            double[,]? dataMul;
            try
            {
                AnalogMultiChannelReader reader = new AnalogMultiChannelReader(AITask?.Stream);
                dataMul = reader.ReadMultiSample(_wave._sampsPerChan);
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
                Debug.WriteLine($"ReadImageData DAQmx Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadImageData" + ex.Message);
                return false;
            }
            finally
            {
                dataMul = null;
            }
        }

        private bool ConverterMulImage(List<double[]> imageDataList, out List<Mat> mats)
        {
            mats = new List<Mat>();
            for (int i = 0; i < imageDataList.Count; i++)
            {
                if (ConverterDoubleArraytoImage(_wave.WaveModel, imageDataList[i], out Mat? mat))
                {
                    mats?.Add(mat);
                }
            }
            imageDataList.Clear();
            return true;
        }

        private bool ConverterDoubleArraytoImage(int waveModel, double[] imageData, out Mat matImage)
        {
            matImage = new();
            try
            {
                double max = imageData.Max();
                double min = imageData.Min();
                double denominator = max - min;
                int x = _wave.XPts - _wave.XMargin;
                int y = _wave.YPts - _wave.YMargin;
                int sampleSize = x * y;

                //matImage = new Mat(x, y, MatType.CV_8SC1);
                //matImage = new Mat(x, y, MatType.CV_8UC1);
                //matImage = new Mat(x, y, MatType.CV_16UC1);
                matImage = new Mat(x, y, MatType.CV_16SC1);
                //matImage = new Mat(x, y, MatType.CV_64FC1);//tif无64格式图像

                //sbyte[] sbytes = new sbyte[sampleSize];
                //byte[] bytes = new byte[sampleSize];
                //ushort[] ushorts = new ushort[sampleSize];
                short[] shorts = new short[sampleSize];
                //double[] doubles = new double[sampleSize];

                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        int imageIndex = i * (_wave.XPts - _wave.XMargin) + j;

                        int arrayIndex = 0;
                        if (Convert.ToBoolean(waveModel))
                        {
                            if (i % 2 == 0)
                            {
                                arrayIndex = i * _wave.XPts + j + _wave.XMargin;
                                //arrayIndex = 0;
                            }
                            else
                            {
                                arrayIndex = i * _wave.XPts + (_wave.XPts - j - 1 - _wave.XMargin);
                                //arrayIndex = 0;
                            }
                        }
                        else
                        {
                            arrayIndex = i * _wave.XPts + j + _wave.XMargin;
                        }

                        double data = (imageData[arrayIndex] - min) / denominator * short.MaxValue;//对应*不同格式的maxvalue
                        if (double.IsNaN(data)) data = 0;

                        //bytes[imageIndex] = (byte)(data);
                        //ushorts[imageIndex] = (ushort)data;
                        shorts[imageIndex] = (short)data;
                        //doubles[imageIndex] = data;
                    }
                }

                ArrayTransferinTriggleWave(23, shorts, out short[] outputArray);

                //byte[] bytes = new byte[x * y];
                //Buffer.BlockCopy(ushorts, 0, bytes, 0, bytes.Length);//若转成ushort类型，需先转成byte
                //Marshal.Copy(bytes, 0, matImage.Data, x * y);
                Marshal.Copy(outputArray, 0, matImage.Data, x * y);
                //Marshal.Copy(doubles, 0, matImage.Data, x * y);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConverterDoubleArraytoImage:" + ex.Message);
                return false;
            }
        }

        private void ArrayTransferinTriggleWave(int removeCount , short[] inputArray,out short[] outputArray)
        {
            outputArray=new short[inputArray.Length];

            int newLength = inputArray.Length - removeCount;       
            short[] newArray = new short[newLength];
            Array.Copy(inputArray, removeCount, newArray, 0, newLength);

            short[] tmpArray = new short[removeCount];

            outputArray = newArray.Concat(tmpArray).ToArray();
        }

        private double[,] CombineArrays(double[] array1, double[] array2)
        {
            // 创建一个二维数组
            int length = array1.Length;
            double[,] resultArray = new double[2, length];

            // 将两个数组放入二维数组的不同行

            for (int i = 0; i < array1.Length; i++)
            {
                resultArray[0, i] = array1[i];
                resultArray[1, i] = array2[i];
            }

            //Parallel.For(0, array1.Length, i =>
            //{
            //    resultArray[0, i] = array1[i];
            //    resultArray[1, i] = array2[i];
            //});

            return resultArray;
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

    }
}

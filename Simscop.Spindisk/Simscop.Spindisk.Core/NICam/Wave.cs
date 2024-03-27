using System;

namespace Simscop.Spindisk.Core.NICamera
{
    public class Wave
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; } = "Dev1";
        /// <summary>
        /// 波形模式
        /// 0-锯齿波，1-三角波
        /// </summary>
        public int WaveModel { get; set; } = 0;
        /// <summary>
        /// 原始X位置
        /// </summary>
        public int OriginX { get; set; } = 300;
        /// <summary>
        /// 原始Y位置
        /// </summary>
        public int OriginY { get; set; } = 300;
        /// <summary>
        /// X偏移
        /// </summary>
        public int XMargin { get; } = 0;
        /// <summary>
        /// Y偏移
        /// </summary>
        public int YMargin { get; } = 0;
        /// <summary>
        /// X方向像素数量
        /// </summary>
        public int XPts { get; set; }
        /// <summary>
        /// Y方向像素数量
        /// </summary>
        public int YPts { get; set; }
        /// <summary>
        /// X轴最大电压
        /// </summary>
        public double Max_x_v { get; set; } = 1;
        /// <summary>
        /// X轴最小电压
        /// </summary>
        public double Min_x_v { get; set; } = -1;
        /// <summary>
        /// Y轴最大电压
        /// </summary>
        public double Max_y_v { get; set; } = 1;
        /// <summary>
        /// Y轴最小电压
        /// </summary>
        public double Min_y_v { get; set; } = -1;
        /// <summary>
        /// 最小输出电压
        /// </summary>
        public double MinAO { get; set; } = -10;
        /// <summary>
        /// 最大输出电压
        /// </summary>
        public double MaxAO { get; set; } = 10;
        /// <summary>
        /// 最小输入电压
        /// </summary>
        public double MinAI { get; set; } = -10;
        /// <summary>
        /// 最大输入电压
        /// </summary>
        public double MaxAI { get; set; } = 10;
        /// <summary>
        /// 低电平时间
        /// </summary>
        public double LowTime { get; set; } = 0;
        /// <summary>
        /// 像素停留时间
        /// </summary>
        public double PixelDwelTime { get; set; } = 20;
        /// <summary>
        /// 三角波偏移
        /// </summary>
        public int XOffsetforTriangle { get; set; } = 1;

        public double _rate; //采样速率(Hz)
        public int _sampsPerChan;//每通道的采样点数
        public double[]? _waveformArray;//波形输出

        public Wave()
        {
            Config config = new Config();
            Max_x_v = config.MaxXV;
            Max_y_v = config.MaxYV;
            Min_x_v = config.MinXV;
            Min_y_v = config.MinYV;
            MaxAI = config.MaxAI;
            MaxAO = config.MaxAO;
            MinAI = config.MinAI;
            MinAO = config.MinAO;
            OriginX = config.XPixelFactor;
            OriginY = config.YPixelFactor;
            //OriginX = 5;
            //OriginY = 5;
            LowTime = config.LowTime;
            PixelDwelTime = config.PixelDwelTime;
            DeviceName = config.DeviceName;
            WaveModel = config.WaveMode;
            if (Convert.ToBoolean(WaveModel))
            {
                XMargin = XOffsetforTriangle;//X偏移，三角波会有行错位
            }
            else
            {
                XMargin = 100;//X偏移100，锯齿波会有转折过程的拖影错位           
            }

            GenerateWaveform();
        }

        private void GenerateWaveform()
        {
            XPts = OriginX + XMargin;
            YPts = OriginY + YMargin;
            _sampsPerChan = XPts * YPts;
            _rate = 1000000 / (PixelDwelTime + LowTime);//1000000（单位：微秒，即1秒）/（每个像素的持续时间+低电平时间）
            _waveformArray = null;
            _waveformArray = new double[_sampsPerChan * 2];//x、y双通道
            double waveformXFactor = (Max_x_v - Min_x_v) / (XPts - 1);
            double waveformYFactor = (Max_y_v - Min_y_v) / (YPts - 1);
            for (int i = 0; i < YPts; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < XPts; j++)
                    {
                        _waveformArray[_sampsPerChan + i * XPts + j] = waveformXFactor * j + Min_x_v;

                        if ((i + 1) < YPts)
                        {
                            if (Convert.ToBoolean(WaveModel))
                            {
                                _waveformArray[_sampsPerChan + (i + 2) * XPts - (j + 1)] = waveformXFactor * j + Min_x_v;//三角波，Z字形
                            }
                            else
                            {
                                _waveformArray[_sampsPerChan + (i + 1) * XPts + j] = waveformXFactor * j + Min_x_v;//锯齿波，循环至第一位置
                            }
                        }
                    }
                }
                for (int j = 0; j < XPts; j++)
                {
                    _waveformArray[i * XPts + j] = waveformYFactor * i + Min_y_v;//X轴
                }
            }
        }
    }
}

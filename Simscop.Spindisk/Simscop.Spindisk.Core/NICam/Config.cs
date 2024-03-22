using System;

namespace Simscop.Spindisk.Core.NICamera
{
    public class Config
    {
        public double MinAO;
        public double MaxAO;
        public double MinAI;
        public double MaxAI;

        public double MaxXV;
        public double MinXV;
        public double MaxYV;
        public double MinYV;

        public int XPixelFactor;
        public int YPixelFactor;

        public double PixelDwelTime;
        public double LowTime;

        public string SavePath;

        public string DeviceName;
        public int WaveMode;

        public Config()
        {
            IniFile iniFile = new IniFile(System.Environment.CurrentDirectory + "\\config.ini");//默认路径

            MinAO = Double.Parse(iniFile.GetString("Camera", "minAO", "-10"));
            MaxAO = Double.Parse(iniFile.GetString("Camera", "maxAO", "10"));
            MinAI = Double.Parse(iniFile.GetString("Camera", "minAI", "-10"));
            MaxAI = Double.Parse(iniFile.GetString("Camera", "maxAI", "10"));

            MaxXV = Double.Parse(iniFile.GetString("Camera", "maxXV", "1"));
            MinXV = Double.Parse(iniFile.GetString("Camera", "minXV", "-1"));
            MaxYV = Double.Parse(iniFile.GetString("Camera", "maxYV", "1"));
            MinYV = Double.Parse(iniFile.GetString("Camera", "minYV", "-1"));

            XPixelFactor = int.Parse(iniFile.GetString("Camera", "XPixelFactor", "300"));
            YPixelFactor = int.Parse(iniFile.GetString("Camera", "YPixelFactor", "300"));

            PixelDwelTime = int.Parse(iniFile.GetString("Camera", "PixelDwelTime", "20"));
            LowTime = int.Parse(iniFile.GetString("Camera", "LowTime", "0"));

            SavePath = iniFile.GetString("File", "SavePath", System.Environment.CurrentDirectory + "\\img");

            DeviceName = iniFile.GetString("Camera", "DeviceName", "Dev1");
            WaveMode = int.Parse(iniFile.GetString("Camera", "WaveMode", "0"));
        }

        public void Write(ConfigSettingEnum configSetting, double value)
        {
            string section = string.Empty;
            section = configSetting.ToString() == "SavePath" ? "File" : "Camera";
            IniFile iniFile = new IniFile(System.Environment.CurrentDirectory + "\\config.ini");
            iniFile.WriteValue(section, configSetting.ToString(), value);
        }

        public void Write(ConfigSettingEnum configSetting, string value)
        {
            string section = string.Empty;
            section = configSetting.ToString() == "SavePath" ? "File" : "Camera";

            IniFile iniFile = new IniFile(System.Environment.CurrentDirectory + "\\config.ini");
            iniFile.WriteValue(section, configSetting.ToString(), value);
        }
    }
    public enum ConfigSettingEnum
    {
        minAO,
        maxAO,
        minAI,
        maxAI,

        maxXV,
        minXV,
        maxYV,
        minYV,

        XPixelFactor,
        YPixelFactor,

        PixelDwelTime,
        LowTime,

        SavePath,

        DeviceName,
        WaveMode,
    }
}

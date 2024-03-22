using OpenCvSharp;
using Simscop.Spindisk.Core.NICamera;
using System;
using System.Collections.Generic;

namespace Simscop.Spindisk.Core.Models.NIDevice
{
    public class TestExample
    {
        public void Test()
        {
            Wave wave = new Wave(300, 300, true);
            Card card = new Card(wave);

            //1、设置AI，AI须较AO先开
            card.CreateAiTask();
            card.StartTask(1);

            //2、设置AO并开启
            card.CreateAoTask();
            card.StartTask(0);
            card.WaitUntilDone(0, 0);       
            card.WaitUntilDone(1, 0);

            //3、读取数据并转换图像
            //card.ReadImage(out Mat mat1, out Mat mat2);
            card.ReadImage(out List<Mat> mats);

            card.StopTask(1);
            card.StopTask(0);

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            for (int i = 0; i < mats.Count; i++)
            {
                string load= desktopPath + $@"\{i}.tif";
                mats[i].SaveImage(load);
            }

        }
    }
}

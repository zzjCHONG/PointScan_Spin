//http://shimat.github.io/opencvsharp/api/index.html

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenCvSharp.Extensions;

namespace Simscop.Lib.ImageExtension;


public static class Converter
{

    //public static bool FromBytes(byte[] frame,out BitmapImage? image)
    //{

    //    image = null;
    //    if (!FromBytes(frame, out Mat mat)) return false;

    //    try
    //    {
    //        var bitmap = mat.ToBitmap();
    //        var memoryStream = new MemoryStream();
    //        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
    //        memoryStream.Position = 0;
    //        image = new BitmapImage();
    //        image.BeginInit();
    //        image.CacheOption = BitmapCacheOption.OnLoad;
    //        image.StreamSource = memoryStream;
    //        image.EndInit();

    //        return true;
    //    }
    //    catch (Exception e)
    //    {
    //        throw new Exception("FromBytes", e);
    //    }
    //}
}
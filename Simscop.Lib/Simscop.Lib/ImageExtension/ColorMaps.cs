using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Simscop.Lib.ImageExtension;

public static class ColorMaps
{

    private static Mat? _green = null;
    public static Mat Green
    {
        get
        {
            if (_green == null)
            {
                _green = new Mat(256, 1, MatType.CV_8UC3);
                for (var i = 0; i < 256; i++)
                    _green.Set(i, 0, new Vec3b((byte)0, (byte)i, (byte)0));

            }
            return _green!;
        }
    }

    

    private static Mat? _red = null;
    public static Mat Red
    {
        get
        {
            if (_red == null)
            {
                _red = new Mat(256, 1, MatType.CV_8UC3);
                for (var i = 0; i < 256; i++)
                    _red.Set(i, 0, new Vec3b((byte)0, (byte)0, (byte)i));

            }
            return _red!;
        }
    }

    private static Mat? _blue = null;
    public static Mat Blue
    {
        get
        {
            if (_blue == null)
            {
                _blue = new Mat(256, 1, MatType.CV_8UC3);
                for (var i = 0; i < 256; i++)
                    _blue.Set(i, 0, new Vec3b((byte)i, (byte)0, (byte)0));

            }
            return _blue!;
        }
    }
}
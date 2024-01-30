using Lift.Core.ImageArray.Extensions;
using OpenCvSharp;

namespace Simscop.Spindisk.Core.Models
{
    internal class StitcherModel
    {

    }

    class Stitcher : IStitcher
    {
        public string Unit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int Width { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int Height { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public (double x, double y) LeftTop { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public (double x, double y) RightBottom { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double PerPixelDistance { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IStitcherProvider Provider { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Mat StitchMat { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool Step()
        {
            throw new System.NotImplementedException();
        }
    }

    class StitcherProvider : IStitcherProvider
    {
        public (Mat mat, double x, double y) Provide()
        {
            Mat mat = GlobalValue.CurrentFrame?.Clone() ?? new Mat();
            mat = mat.ToU8();
            return (mat, GlobalValue.GlobalMotor.X, GlobalValue.GlobalMotor.Y);
        }
    }
}

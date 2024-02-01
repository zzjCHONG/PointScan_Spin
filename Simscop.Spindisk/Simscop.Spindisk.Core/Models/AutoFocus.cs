using CommunityToolkit.Mvvm.Messaging;
using Lift.Core.Autofocus;
using Lift.Core.Autofocus.Drivers;
using Lift.Core.ImageArray.Extensions;
using OpenCvSharp;
using Simscop.API;
using Simscop.Spindisk.Core.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simscop.Spindisk.Core.Models
{
    public class AutoFocus : Jaf
    {
        private static AutoFocus focus;
        private static readonly object lockObject = new object();

        public static AutoFocus Create()
        {
            if (focus == null)
            {
                lock (lockObject)
                {
                    if (focus == null)
                    {
                        focus = new AutoFocus();

                        WeakReferenceMessenger.Default.Send<string>(SteerMessage.MotorReceive);


                    }
                }
            }
            return focus;
        }

        Andor _andor = new Andor();

        ASIMotor _motor;

        private AutoFocus()
        {
            WeakReferenceMessenger.Default.Register<ASIMotor, string>(this, SteerMessage.Motor, (s, e) =>
            {
                _motor = e;
            });
        }

        public override bool Capture(out Mat mat)
        {
            mat = new Mat();
            try
            {
                mat = GlobalValue.CurrentFrame?.Clone() ?? new Mat();
                mat = mat.ToU8();
                //mat = mat.MinMaxNorm().ToU8();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

            return true;
        }

        public override bool GetPosition(out double z)
        {
            _motor.ReadPosition();
            Thread.Sleep(100);
            z = _motor.Z;
            return true;
        }



        public override bool SetPosition(double z)
        {
            var value = Math.Round(z, 2);
            _motor.SetZPosition(value);
            _motor.ReadPosition();
            while (Math.Abs(_motor.Z - value) > 0.1)
                Thread.Sleep(10);
            return true;
        }

        public override void Focus()
        {
            double num = 5000.0;
            double num2 = 0.0;
            double num3 = 0.0;
            GetPosition(out var z);

            Debug.WriteLine(z);

            double num4 = z - FirstStep * (double)FirstCount;
            num4 = num4 > MaxZ ? MaxZ : num4;
            num4 = num4 < MinZ ? MinZ : num4;

            SetPosition(num4);
            Thread.Sleep(300);
            for (int i = 0; i < 2 * FirstCount + 1; i++)
            {
                double num5 = num4 + (double)i * FirstStep;
                if (!(num5 <= MinZ) && !(num5 >= MaxZ))
                {
                    SetPosition(num5);
                    GetPosition(out z);
                    Capture(out var mat);
                    num3 = Score(mat);
                    if (num3 > num2)
                    {
                        num2 = num3;
                        num = z;
                    }
                    else if (num2 - num3 > Threshold * num2)
                    {
                        break;
                    }
                }
            }

            num4 = num - SecondStep * (double)SeccondCount;
            num4 = num4 > MaxZ ? MaxZ : num4;
            num4 = num4 < MinZ ? MinZ : num4;


            SetPosition(num4);
            Thread.Sleep(100);
            num2 = 0.0;
            for (int j = 0; j < 2 * SeccondCount + 1; j++)
            {
                double num6 = num4 + (double)j * SecondStep;
                if (!(num6 <= MinZ) && !(num6 >= MaxZ))
                {
                    SetPosition(num4 + (double)j * SecondStep);
                    GetPosition(out z);
                    Capture(out var mat2);
                    num3 = Score(mat2);
                    if (num3 > num2)
                    {
                        num2 = num3;
                        num = z;
                    }
                    else if (num2 - num3 > Threshold * num2)
                    {
                        break;
                    }
                }
            }

            SetPosition(num);

            Debug.WriteLine("Done");
        }
    }
}

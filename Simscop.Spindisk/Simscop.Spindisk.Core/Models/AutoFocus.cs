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
            Thread.Sleep(10);
            z = _motor.Z;
            return true;
        }

        public override bool SetPosition(double z)
        {
            _motor.SetZPosition(z);
            _motor.ReadPosition();
            while (Math.Abs(_motor.Z - z) > 0.01)
                Thread.Sleep(10);
            return true;
        }

        public override void Focus()
        {
            base.Focus();
            Debug.WriteLine("Focus done");
        }
    }
}

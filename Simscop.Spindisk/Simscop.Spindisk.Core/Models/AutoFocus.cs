﻿using CommunityToolkit.Mvvm.Messaging;
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
using System.Windows;

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
                        focus.MaxZ = 75;
                        focus.MinZ = 30;
                        GlobalValue.GeneralFocus = focus;
                        GlobalValue.CustomFocus = focus;

                        WeakReferenceMessenger.Default.Send<string>(SteerMessage.MotorReceive);


                    }
                }
            }
            return focus;
        }

        MshotMotor _motor;
        //ASIMotor _motor;

        private AutoFocus()
        {

            WeakReferenceMessenger.Default.Register<MshotMotor, string>(this, SteerMessage.Motor, (s, e) =>
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

            int timeoutMilliseconds = 2000;

            DateTime startTime = DateTime.Now;

            try
            {
                while (Math.Abs(_motor.Z - value) > 0.1)
                {
                    if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMilliseconds)
                    {
                        WeakReferenceMessenger.Default.Send<SteerAnimationStateMessage>(new SteerAnimationStateMessage(-1));
                        throw new TimeoutException("SetPosition operation timed out");
                    }

                    Thread.Sleep(10);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("位移台出现错误，停止对焦");

                return false;
            }
        }

        public override void Focus()
        {
            double num = 5000.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double xPos = _motor.X;
            double yPos = _motor.Y;    
            GetPosition(out var z);

            Debug.WriteLine(z);

            double num4 = z - FirstStep * (double)FirstCount;
            num4 = num4 > MaxZ ? MaxZ : num4;
            num4 = num4 < MinZ ? MinZ : num4;

            if (!SetPosition(num4)) return;

            Thread.Sleep(300);
            for (int i = 0; i < 2 * FirstCount + 1; i++)
            {
                double num5 = num4 + (double)i * FirstStep;
                if (!(num5 <= MinZ) && !(num5 >= MaxZ))
                {
                    if (Math.Abs(xPos - _motor.X) > 0.1 || Math.Abs(yPos - _motor.Y) > 0.1) 
                    {
                        WeakReferenceMessenger.Default.Send<SteerAnimationStateMessage>(new SteerAnimationStateMessage(-1));
                        Debug.WriteLine("111"); return;
                    }
                    if (!SetPosition(num5)) break;
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


            if (!SetPosition(num4)) return;
            Thread.Sleep(100);
            num2 = 0.0;
            for (int j = 0; j < 2 * SeccondCount + 1; j++)
            {
                double num6 = num4 + (double)j * SecondStep;
                if (!(num6 <= MinZ) && !(num6 >= MaxZ))
                {
                    if (Math.Abs(xPos - _motor.X) > 0.1 || Math.Abs(yPos - _motor.Y) > 0.1)
                    {
                        WeakReferenceMessenger.Default.Send<SteerAnimationStateMessage>(new SteerAnimationStateMessage(-1));
                        Debug.WriteLine("222"); return;
                    }
                    if (!SetPosition(num4 + (double)j * SecondStep)) break;
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


            WeakReferenceMessenger.Default.Send<SteerAnimationStateMessage>(new SteerAnimationStateMessage(2));//对焦完成
            Debug.WriteLine("Done");
            
        }
    }
}

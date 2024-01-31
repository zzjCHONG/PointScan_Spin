using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp.WpfExtensions;
using Simscop.Spindisk.Core;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.ViewModels;

namespace Simscop.Spindisk.WPF.Views
{

    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private CameraViewModel cameraVM;
        private SpinViewModel spinVM;
        private ShellViewModel shellVM;
        private SteerViewModel steerVM;
        private LaserViewModel laserVM;
        private ScanViewModel scanVM;
        private ExampleViewModel exampleVM;

        private int frameCount = 0;
        private DateTime lastTime = DateTime.Now;

        private CameraView cameraView;
        private ScanView scanView;
        private ExampleView exampleView;

        public static ShellView Instance
        {
            get
            {
                return Nested.instance;
            }
        }
        class Nested
        {
            static Nested() { }
            internal static readonly ShellView instance = new ShellView();
        }

        private ShellView()
        {
            InitializeComponent();

            Pic1.MouseLeftButtonDown += PicDown;
            Pic2.MouseLeftButtonDown += PicDown;
            Pic3.MouseLeftButtonDown += PicDown;
            Pic4.MouseLeftButtonDown += PicDown;

            cameraVM = new CameraViewModel();
            shellVM = new ShellViewModel();
            spinVM = new SpinViewModel();
            scanVM = new ScanViewModel();
            steerVM = new SteerViewModel();
            laserVM = new LaserViewModel();
            exampleVM = new ExampleViewModel()
            {
                CameraVM = cameraVM,
                ScanVM = scanVM,
                SteerVM = steerVM
            };

            cameraView = new()
            {
                DataContext = cameraVM,
            };
            scanView = new()
            {
                DataContext = scanVM,
            };
            exampleView = new()
            {
                DataContext = exampleVM,
            };

            SetDataContext();

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            WeakReferenceMessenger.Default.Register<MainDisplayMessage>(this, (o, m) =>
            {
                RemotePicDown(m.Index);
            });  
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // 计算帧率
            frameCount++;
            var now = DateTime.Now;
            var elapsed = now - lastTime;
            if (elapsed >= TimeSpan.FromSeconds(1))
            {
                var fps = cameraVM.FrameRate;
                // 更新界面显示
                FpsLabel.Text = $"FPS: {fps:F1}";
                // 重置计数器
                frameCount = 0;
                lastTime = now;
            }
        }

        private void SetDataContext()
        {
            this.DataContext = shellVM;
            this.BaseCameraControl.DataContext = cameraVM;
            this.SpinControl.DataContext = spinVM;
            this.SteerControl.DataContext = steerVM;
            this.LaserControl.DataContext = laserVM;
        }

        private bool IsFull { get; set; } = false;

        public void PicDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var grid = sender as Border;

                if (!IsFull)
                {
                    foreach (Border child in Client.Children)
                        if (child != grid) child.Visibility = Visibility.Collapsed;

                    Client.Columns = 1;
                    Client.Rows = 1;
                    IsFull = true;
                }
                else
                {
                    foreach (Border child in Client.Children)
                        if (child != grid) child.Visibility = Visibility.Visible;

                    Client.Columns = 2;
                    Client.Rows = 2;
                    IsFull = false;
                }
            }
        }

        void RemotePicDown(int index)
        {
            if (!IsFull) return;

            var childs = Client.Children;

            var count = 0;

            foreach (Border child in childs)
            {
                child.Visibility = Visibility.Collapsed;
                if (count == index)
                    child.Visibility = Visibility.Visible;

                count++;
            }
        }

        // TODO 这里的卡顿问题已经定位了，原因就是在给datacontext的时候数据变化和赋值原因，解决办法挺简单的，单个窗口重复利用就行，但是这里目前就卡着吧，有空再改
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            cameraView.Show();
            cameraView.Topmost = true;
            cameraView.Topmost = false;
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void AboutBtClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe",
                @"https://www.simscop.com/WebShop/About.aspx");
        }

        private void HelpBtClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe",
                @"https://www.simscop.com/WebShop/Contact.aspx");
        }


        private void OpenBtClick(object sender, RoutedEventArgs e)
        {
            scanView.Show();
            scanView.Topmost = true;
            scanView.Topmost = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            exampleView.Show();
            exampleView.Topmost = true;
            exampleView.Topmost = false;
        }

    }

}

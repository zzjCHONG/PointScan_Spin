using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
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
        private int frameCount = 0;
        private DateTime lastTime = DateTime.Now;

        private CameraViewModel cameraVM;
        private SpinViewModel spinVM;
        private readonly ShellViewModel shellVM;
        private SteerViewModel steerVM;
        private LaserViewModel laserVM;
        private ExampleViewModel exampleVM;
        private ScanViewModel scanVM;
        private MultiChannelViewModel multiChannelSaveVM;
        private CameraSaveViewModel cameraSaveVM;
        private StitcherViewModel stitcherVM;

        private CameraView cameraView;
        private SettingView settingView;
        private ExampleView exampleView;
        private MultiChannelView multiChannelView;
        private CameraSaveView cameraSaveView;
        private StitcherView stitcherView;

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
            GlobalValue.GlobalShellViewModel = shellVM;
            spinVM = new SpinViewModel();
            steerVM = new SteerViewModel();
            scanVM = new ScanViewModel();
            laserVM = new LaserViewModel();
            cameraSaveVM = new CameraSaveViewModel();
            multiChannelSaveVM = new MultiChannelViewModel();
            stitcherVM = new StitcherViewModel();
            exampleVM = new ExampleViewModel()
            {
                CameraVM = cameraVM,
                SteerVM = steerVM
            };

            settingView = new()
            {
                DataContext = steerVM,
            };
            exampleView = new()
            {
                DataContext = exampleVM,
            };
            cameraSaveView = new()
            {
                DataContext = cameraSaveVM,
            };
            multiChannelView = new()
            {
                DataContext = multiChannelSaveVM,
            };
            stitcherView = new()
            {
                DataContext = stitcherVM,
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
            this.SettingControl.DataContext = steerVM;
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

        private void NavigateBtClick(object sender, RoutedEventArgs e)
        {
            stitcherView.Show();
            exampleView.Topmost = true;
        }
        
        private void HelpBtClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe",
                @"https://www.simscop.com/WebShop/Contact.aspx");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            exampleView.Show();
            exampleView.Topmost = true;
            exampleView.Topmost = false;
        }
    
        private void MultiChannel_OnClick(object sender, RoutedEventArgs e)
        {
            multiChannelView.Show();
            multiChannelView.Topmost = true;
            multiChannelView.Topmost = false;
        }

        private void CameraSave_OnClick(object sender, RoutedEventArgs e)
        {
            cameraSaveView.Show();
            cameraSaveView.Topmost = true;
            cameraSaveView.Topmost = false;
        }

    }

}

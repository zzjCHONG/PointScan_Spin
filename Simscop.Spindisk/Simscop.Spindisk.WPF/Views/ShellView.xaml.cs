using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
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
        private MultiChannelPreviewViewModel multiChannelPreviewVM;
        private MultiChannelSaveViewModel multiChannelSaveVM;
        private CameraView cameraView;
        private SettingView settingView;
        private ExampleView exampleView;
        private MultiChannelSave multiChannelSaveView;
        private MultiChannelPreview multiChannelPreview;

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
            steerVM = new SteerViewModel();
            scanVM = new ScanViewModel();
            laserVM = new LaserViewModel();
            multiChannelPreviewVM = new MultiChannelPreviewViewModel();
            multiChannelSaveVM= new MultiChannelSaveViewModel();
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
            multiChannelPreview = new ()
            {
                DataContext= multiChannelPreviewVM,
            };
            multiChannelSaveView = new()
            {
                DataContext = multiChannelSaveVM,
            };

            SetDataContext();

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            WeakReferenceMessenger.Default.Register<MainDisplayMessage>(this, (o, m) =>
            {
                RemotePicDown(m.Index);
            });

            //WeakReferenceMessenger.Default.Register<string>(SteerMessage.Setting, (s, e) =>
            //{
            //    settingView.Show();
            //});//temp
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
            settingView.Show();
            settingView.Topmost = true;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            exampleView.Show();
            exampleView.Topmost = true;
            exampleView.Topmost = false;
        }

        private void LevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<UI.Controls.RangeValue> e)
        {

        }
    
        private void MultiChannelSave_OnClick(object sender, RoutedEventArgs e)
        {
            multiChannelSaveView.Show();
            multiChannelSaveView.Topmost = true;
            multiChannelSaveView.Topmost = false;
        }


        private void MultiChannelPreview_OnClick(object sender, RoutedEventArgs e)
        {
            multiChannelPreview.Show();
            multiChannelPreview.Topmost = true;
            multiChannelPreview.Topmost = false;
        }

    }

}

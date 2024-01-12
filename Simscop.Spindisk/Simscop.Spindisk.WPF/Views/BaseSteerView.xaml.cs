using Simscop.Spindisk.Core.ViewModels;
using Simscop.UI.Controls;
using System;
using System.Collections.Generic;
using System.Printing.IndexedProperties;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Simscop.Spindisk.WPF.Views
{
    /// <summary>
    /// Interaction logic for BaseSteerView.xaml
    /// </summary>
    public partial class BaseSteerView : UserControl
    {
        public const int TimerInterval = 100;

        private readonly DispatcherTimer _rightTimer;
        private readonly DispatcherTimer _rightBottomTimer;
        private readonly DispatcherTimer _bottomTimer;
        private readonly DispatcherTimer _leftBottomTimer;
        private readonly DispatcherTimer _leftTimer;
        private readonly DispatcherTimer _leftTopTimer;
        private readonly DispatcherTimer _topTimer;
        private readonly DispatcherTimer _rightTopTimer;
        private readonly DispatcherTimer _upTimer;
        private readonly DispatcherTimer _downTimer;

        public SteerViewModel? Vm => DataContext as SteerViewModel;

        public BaseSteerView()
        {
            InitializeComponent();
            this.DataContext = null;

            _rightTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _rightTimer.Tick += (s, e) => Vm?.MoveX(-Vm.XyStep);
            RightMoveBt.PreviewMouseDown += (s, e) => _rightTimer.Start(); ;
            RightMoveBt.PreviewMouseUp += (s, e) => _rightTimer.Stop();
            RightMoveBt.Click += (s, e) => Vm?.MoveX(-Vm.XyStep);

            _rightBottomTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _rightBottomTimer.Tick += (s, e) =>
            {
                Vm?.MoveX(Vm.XyStep);
                Vm?.MoveY(-Vm.XyStep);
            };
            RightBottomMoveBt.PreviewMouseDown += (s, e) => _rightBottomTimer.Start();
            RightBottomMoveBt.PreviewMouseUp += (s, e) => _rightBottomTimer.Stop();
            RightBottomMoveBt.Click += (s, e) =>
            {
                Vm?.MoveX(Vm.XyStep);
                Vm?.MoveY(-Vm.XyStep);
            };

            _bottomTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _bottomTimer.Tick += (s, e) => Vm?.MoveY(-Vm.XyStep);
            BottomMoveBt.PreviewMouseDown += (s, e) => _bottomTimer.Start();
            BottomMoveBt.PreviewMouseUp += (s, e) => _bottomTimer.Stop();
            BottomMoveBt.Click += (s, e) => Vm?.MoveY(-Vm.XyStep);

            _leftBottomTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _leftBottomTimer.Tick += (s, e) =>
            {
                Vm?.MoveX(-Vm.XyStep);
                Vm?.MoveY(-Vm.XyStep);
            };
            LeftBottomMoveBt.PreviewMouseDown += (s, e) => _leftBottomTimer.Start();
            LeftBottomMoveBt.PreviewMouseUp += (s, e) => _leftBottomTimer.Stop();
            LeftBottomMoveBt.Click += (s, e) =>
            {
                Vm?.MoveX(-Vm.XyStep);
                Vm?.MoveY(-Vm.XyStep);
            };

            _leftTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _leftTimer.Tick += (s, e) => Vm?.MoveX(Vm.XyStep);
            LeftMoveBt.PreviewMouseDown += (s, e) => _leftTimer.Start();
            LeftMoveBt.PreviewMouseUp += (s, e) => _leftTimer.Stop();
            LeftMoveBt.Click += (s, e) => Vm?.MoveX(Vm.XyStep);

            _leftTopTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _leftTopTimer.Tick += (s, e) =>
            {
                Vm?.MoveX(Vm.XyStep);
                Vm?.MoveY(Vm.XyStep);
            };
            LeftTopMoveBt.PreviewMouseDown += (s, e) => _leftTopTimer.Start();
            LeftTopMoveBt.PreviewMouseUp += (s, e) => _leftTopTimer.Stop();
            LeftTopMoveBt.Click += (s, e) =>
            {
                Vm?.MoveX(Vm.XyStep);
                Vm?.MoveY(Vm.XyStep);
            };

            _topTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _topTimer.Tick += (s, e) => Vm?.MoveY(Vm.XyStep);
            TopMoveBt.PreviewMouseDown += (s, e) => _topTimer.Start();
            TopMoveBt.PreviewMouseUp += (s, e) => _topTimer.Stop();
            TopMoveBt.Click += (s, e) => Vm?.MoveY(Vm.XyStep);

            _rightTopTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _rightTopTimer.Tick += (s, e) =>
            {
                Vm?.MoveX(-Vm.XyStep);
                Vm?.MoveY(Vm.XyStep);
            };
            RightTopMoveBt.PreviewMouseDown += (s, e) => _rightTopTimer.Start();
            RightTopMoveBt.PreviewMouseUp += (s, e) => _rightTopTimer.Stop();
            RightTopMoveBt.Click += (s, e) =>
            {
                Vm?.MoveX(-Vm.XyStep);
                Vm?.MoveY(Vm.XyStep);
            };

            _upTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _upTimer.Tick += (s, e) => Vm?.MoveZ(Vm.ZStep);
            UpMoveBt.PreviewMouseDown += (s, e) => _upTimer.Start();
            UpMoveBt.PreviewMouseUp += (s, e) => _upTimer.Stop();
            UpMoveBt.Click += (s, e) => Vm?.MoveZ(Vm.ZStep);

            _downTimer = new DispatcherTimer(priority: DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _downTimer.Tick += (s, e) => Vm?.MoveZ(-Vm.ZStep);
            DownMoveBt.PreviewMouseDown += (s, e) => _downTimer.Start();
            DownMoveBt.PreviewMouseUp += (s, e) => _downTimer.Stop();
            DownMoveBt.Click += (s, e) => Vm?.MoveZ(-Vm.ZStep);

        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}

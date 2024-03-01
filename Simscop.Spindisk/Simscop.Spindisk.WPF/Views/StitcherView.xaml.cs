using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lift.Core.ImageArray.Algorithm;
using Lift.Core.ImageArray.Extensions;
using Lift.UI.Controls;
using OpenCvSharp;
using Simscop.Spindisk.Core;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.ViewModels;


namespace Simscop.Spindisk.WPF.Views;



/// <summary>
/// Interaction logic for StitcherView.xaml
/// </summary>
public partial class StitcherView: Lift.UI.Controls.Window
{
    public StitcherView()
    {
        InitializeComponent();

        this.DataContext = new StitcherViewModel();
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
    }

    bool isEventAllowed;

    private void Timer_Tick(object? sender, EventArgs e)
    {
        isEventAllowed = true;
        timer.Stop();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        this.Hide();
        e.Cancel = true;
    }

    DispatcherTimer timer;

    private void UIElement_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (isEventAllowed)
        {
            isEventAllowed = false;
            timer.Start();
            if (sender is not ImageViewer viewer) return;
            if (DataContext is not StitcherViewModel vm) return;
            if (vm.PosStart is null) return;
            Debug.WriteLine("鼠标右键点击");
            var pos = viewer.ImageCurrentPosition;

            var (x, y) = pos;


            if (x == -1 || y == -1) return;

            var xStage = (x - vm.ImageSize.Width / 2) * vm.PerPixel2Unit + vm.PosStart.Value.X;
            var yStage = (y - vm.ImageSize.Height / 2) * vm.PerPixel2Unit + vm.PosStart.Value.Y;

            Debug.WriteLine(pos + "---" + xStage + "----" + yStage);

            //WeakReferenceMessenger.Default.Send(nameof(CurrentPositionMessage), new CurrentPositionMessage(x, y));
            WeakReferenceMessenger.Default.Send(new MappingMoveMessage(xStage, yStage), nameof(MappingMoveMessage));
        }
        else
        {
            timer.Stop();
            timer.Start();
            if (sender is not ImageViewer viewer) return;
            if (DataContext is not StitcherViewModel vm) return;
            if (vm.PosStart is null) return;
            Debug.WriteLine("鼠标右键点击");
            var pos = viewer.ImageCurrentPosition;

            var (x, y) = pos;


            if (x == -1 || y == -1) return;

            var xStage = (x - vm.ImageSize.Width / 2) * vm.PerPixel2Unit + vm.PosStart.Value.X;
            var yStage = (y - vm.ImageSize.Height / 2) * vm.PerPixel2Unit + vm.PosStart.Value.Y;

            Debug.WriteLine(pos + "---" + xStage + "----" + yStage);

            //WeakReferenceMessenger.Default.Send(nameof(CurrentPositionMessage), new CurrentPositionMessage(x, y));
            WeakReferenceMessenger.Default.Send(new MappingMoveMessage(xStage, yStage), nameof(MappingMoveMessage));
        }
        
    }

}

public class CameraStitcherProvider : IStitcherProvider
{
    public int Count { get; set; } = 0;

    public List<string> Paths = new();

    public CameraStitcherProvider()
    {
        for (var i = 0; i < 6; i++)
            for (var j = 0; j < 20; j++)
                Paths.Add($@"E:\.test\stitch\{i + 1}-{j + 1}.TIF");
    }

    /// <inheritdoc/>
    //(Mat mat, double x, double y, int row, int col) IStitcherProvider.Provide()
    //{
    //    var path = Paths[Count++];
    //    var name = System.IO.Path.GetFileNameWithoutExtension(path);

    //    var pos = name.Split("-");
    //    var c = int.Parse(pos[0]);
    //    var r = int.Parse(pos[1]);
    //    var x = c * (2560 / 2);
    //    var y = r * (2160 / 2);

    //    Debug.WriteLine($"{x} - {y}");

    //    var mat = Cv2.ImRead(path, ImreadModes.Unchanged);
    //    mat = mat.ToU8();

    //    return new(mat, x, y, r - 1, c - 1);
    //}

    public (Mat mat, double x, double y) Provide()
    {
        throw new System.NotImplementedException();
    }
}




using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Simscop.Spindisk.Core.ViewModels
{
    public partial class ImageShowViewModel: ObservableObject
    {
        public ImageShowViewModel() 
        {
            //BitmapImage bitmap = new BitmapImage(new System.Uri(@"C:\Users\Administrator\Pictures\Camera Roll\1.jpg"));
            //MergeImageSource = bitmap;

            //BitmapSource bitmapSource = bitmap as BitmapSource;
            //BitmapFrame bitmapFrame = BitmapFrame.Create(bitmapSource);
            //ImageFrame = bitmapFrame;

            WeakReferenceMessenger.Default.Register<MultiChannelOpenImageWindowMwssage>(this, ((s, m) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MergeImageSource = new BitmapImage(new System.Uri(m.Filename));

                    ImageFrame = BitmapFrame.Create(MergeImageSource as BitmapSource);
                });

                //控制弹窗
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WeakReferenceMessenger.Default.Send<PopupWindowMessage>(new PopupWindowMessage());
                });

            }));
        }

        [ObservableProperty]
        private BitmapImage? _mergeImageSource;

        [ObservableProperty]
        private BitmapFrame? _imageFrame;

    }
}

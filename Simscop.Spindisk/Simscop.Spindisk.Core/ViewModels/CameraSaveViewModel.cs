using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using System;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;

namespace Simscop.Spindisk.Core.ViewModels
{
    public partial class CameraSaveViewModel : ObservableObject
    {
        public CameraSaveViewModel()
        {
            WeakReferenceMessenger.Default.Register<ChannelControlEnableMessage>(this, (s, m) =>
            {
                switch(m.Channel)
                {
                    case 1:
                        IsFirstChannelEnable = m.IsEnable;
                        break;
                    case 2:
                        IsSecondChannelEnable = m.IsEnable;
                        break;
                    case 3:
                        IsThirdChannelEnable = m.IsEnable;
                        break;
                    case 4:
                        IsFourthChannelEnable = m.IsEnable;
                        break;
                }

                IsChannelEnable = IsFirstChannelEnable || IsSecondChannelEnable || IsThirdChannelEnable || IsFourthChannelEnable;
            });

        }

        [ObservableProperty]
        private bool _isFirstChannelEnable=false;

        [ObservableProperty]
        private bool _isSecondChannelEnable = false;

        [ObservableProperty]
        private bool _isThirdChannelEnable = false;

        [ObservableProperty]
        private bool _isFourthChannelEnable = false;

        [ObservableProperty]
        private bool _isChannelEnable = false;

        [RelayCommand]
        void SaveChannelAOriginalImage()
        {
            SaveSingleImage(0, true);
        }

        [RelayCommand]
        void SaveChannelADisposeImage()
        {
            SaveSingleImage(0, false);
        }

        [RelayCommand]
        void SaveChannelBOriginalImage()
        {
            SaveSingleImage(1, true);
        }

        [RelayCommand]
        void SaveChannelBDisposeImage()
        {
            SaveSingleImage(1, false);
        }

        [RelayCommand]
        void SaveChannelCOriginalImage()
        {
            SaveSingleImage(2, true);
        }

        [RelayCommand]
        void SaveChannelCDisposeImage()
        {
            SaveSingleImage(2, false);
        }

        [RelayCommand]
        void SaveChannelDOriginalImage()
        {
            SaveSingleImage(3, true);
        }

        [RelayCommand]
        void SaveChannelDDisposeImage()
        {
            SaveSingleImage(3, false);
        }

        [ObservableProperty]
        private string _root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CameraCapture");//默认路径

        void SaveSingleImage(int channelID, bool isSaveOriginImage)
        {
            var dlg = new SaveFileDialog()
            {
                Title = "图像存储",
                FileName = GetFilename(channelID, isSaveOriginImage),
                Filter = "TIF|*.tif",
                DefaultExt = ".tif",
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(channelID, isSaveOriginImage, dlg.FileName));
                OpenFolderAndSelectFile(dlg.FileName);
            }                
        }

        string GetFilename(int channelID, bool isSaveOriginImage)
        {
            string channelName = string.Empty;
            string isOriginImage = isSaveOriginImage ? "Origin" : "Dispose";
            switch (channelID)
            {
                case 0:
                    channelName = "DAPI-405nm";
                    break;
                case 1:
                    channelName = "FITC-488nm";
                    break;
                case 2:
                    channelName = "TRITC-561nm";
                    break;
                case 3:
                    channelName = "CY5-640nm";
                    break;
                default:
                    break;
            }
            return $"{channelID + 1}_{channelName}_{isOriginImage}.tif";
        }

        [RelayCommand]
        void OpenFileDialog()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择存图文件夹";
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.SelectedPath = Root;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
                Root = folderBrowserDialog.SelectedPath;
        }

        [ObservableProperty]
        private bool _isSaveDisposeImage = false;

        [RelayCommand]
        void SaveSameKindofImage()
        {
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);

            //IsChannelEnable = IsFirstChannelEnable || IsSecondChannelEnable || IsThirdChannelEnable || IsFourthChannelEnable;

            if (IsFirstChannelEnable)
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(0, !IsSaveDisposeImage, Path.Combine(Root, GetFilename(0, !IsSaveDisposeImage))));
            if (IsSecondChannelEnable)
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(1, !IsSaveDisposeImage, Path.Combine(Root, GetFilename(1, !IsSaveDisposeImage))));
            if (IsThirdChannelEnable)
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(2, !IsSaveDisposeImage, Path.Combine(Root, GetFilename(2, !IsSaveDisposeImage))));
            if (IsFourthChannelEnable)
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(3, !IsSaveDisposeImage, Path.Combine(Root, GetFilename(3, !IsSaveDisposeImage))));

            OpenFolderAndSelectFile(Root);
        }

        private void OpenFolderAndSelectFile(String fileFullName)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select," + fileFullName;
            System.Diagnostics.Process.Start(psi);
        }

    }
}

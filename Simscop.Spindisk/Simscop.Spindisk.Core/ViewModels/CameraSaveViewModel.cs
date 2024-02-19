using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using System;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;

namespace Simscop.Spindisk.Core.ViewModels
{
    public partial class CameraSaveViewModel: ObservableObject
    {
        [ObservableProperty]
        private string _root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CameraCapture");//默认路径

        [RelayCommand]
        void SaveChannelAOriginalImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(0, true, Path.Combine(Root, GetFilename(0, true))));
        }

        [RelayCommand]
        void SaveChannelADisposeImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(0, false, Path.Combine(Root, GetFilename(0, false))));
        }

        [RelayCommand]
        void SaveChannelBOriginalImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(1, true, Path.Combine(Root, GetFilename(1, true))));
        }

        [RelayCommand]
        void SaveChannelBDisposeImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(1, false, Path.Combine(Root, GetFilename(1, false))));
        }

        [RelayCommand]
        void SaveChannelCOriginalImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(2, true, Path.Combine(Root, GetFilename(2, true))));
        }

        [RelayCommand]
        void SaveChannelCDisposeImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(2, false, Path.Combine(Root, GetFilename(2, false))));
        }

        [RelayCommand]
        void SaveChannelDOriginalImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(3, true, Path.Combine(Root, GetFilename(3, true))));
        }

        [RelayCommand]
        void SaveChannelDDisposeImage()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) Root = dialog.SelectedPath;
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(3, false, Path.Combine(Root, GetFilename(3, false))));
        }

        string GetFilename(int channelID,bool isSaveOriginImage)
        {
            string channelName = string.Empty;
            string isOriginImage= isSaveOriginImage? "Origin" : "Dispose";
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
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(0, !IsSaveDisposeImage, Path.Combine(Root, GetFilename(0, !IsSaveDisposeImage))));
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(1, !IsSaveDisposeImage, Path.Combine(Root, GetFilename(1, !IsSaveDisposeImage))));
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(2, !IsSaveDisposeImage, Path.Combine(Root, GetFilename(2, !IsSaveDisposeImage))));
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

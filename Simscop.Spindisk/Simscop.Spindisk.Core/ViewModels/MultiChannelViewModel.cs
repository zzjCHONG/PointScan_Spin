using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Simscop.Spindisk.Core.ViewModels
{
    public partial class MultiChannelViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool _isChannelASave=false;
        [ObservableProperty]
        private bool _isChannelBSave = false;
        [ObservableProperty]
        private bool _isChannelCSave = false;
        [ObservableProperty]
        private bool _isChannelDSave = false;

        [ObservableProperty]
        private string _root =Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"MultiChannelCapture") ;//默认路径

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

        [RelayCommand]
        void SaveMultiChannel()
        {
            string filepath = Path.Combine(Root, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
            if (IsChannelASave || IsChannelBSave || IsChannelCSave || IsChannelDSave && !Directory.Exists(filepath))
                Directory.CreateDirectory(filepath);

            if (IsChannelASave)
            {
                WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(0, true), nameof(LaserMessage));//显示
                WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(0));//全屏显示
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(0, true, Path.Combine(filepath, GetFilename(0))));//存原图
                Thread.Sleep(300);//频道切换需要缓冲时间
            }        
            if (IsChannelBSave)
            {
                WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(1, true), nameof(LaserMessage));
                WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(1));
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(1, true, Path.Combine(filepath, GetFilename(1))));
                Thread.Sleep(300);
            }         
            if (IsChannelCSave)
            {
                WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(2, true), nameof(LaserMessage));
                WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(2));
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(2, true, Path.Combine(filepath, GetFilename(2))));
                Thread.Sleep(300);
            }
            if (IsChannelDSave)
            {
                WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(3, true), nameof(LaserMessage));
                WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(3));
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(3, true, Path.Combine(filepath, GetFilename(3))));
                Thread.Sleep(300);
            }

            if (IsChannelASave || IsChannelBSave || IsChannelCSave || IsChannelDSave)
                WeakReferenceMessenger.Default.Send<MultiChannelMergeMessage>(new MultiChannelMergeMessage(Path.Combine(filepath, "MergeImage.tif")));

            OpenFolderAndSelectFile(filepath);
        }

        string GetFilename(int channelID)
        {
            string channelName = string.Empty;
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
            return $"{channelID + 1}_{channelName}.tif";
        }

        private void OpenFolderAndSelectFile(string fileFullName)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics. ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select," + fileFullName;
            System.Diagnostics.Process.Start(psi);
        }

    }
}

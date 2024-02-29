using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simscop.Spindisk.Core.ViewModels
{
    public partial class MultiChannelViewModel : ObservableObject
    {
        private static bool _isChannelASwitch = false;
        private static bool _isChannelBSwitch = false;
        private static bool _isChannelCSwitch = false;
        private static bool _isChannelDSwitch = false;

        public MultiChannelViewModel()
        {
            WeakReferenceMessenger.Default.Register<CurrentDispalyChannelEnableMessage>(this, (s, m) =>
            {
                _isChannelASwitch = m.IsFirstDisplayEnabled;
                _isChannelBSwitch = m.IsSecondDisplayEnabled;
                _isChannelCSwitch = m.IsThirdDisplayEnabled;
                _isChannelDSwitch = m.IsFourthDisplayEnabled;
            });

            WeakReferenceMessenger.Default.Register<ChannelControlEnableMessage>(this, (s, m) =>
            {
                switch (m.Channel)
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
            });
        }

        [ObservableProperty]
        private bool _isFirstChannelEnable = false;
        [ObservableProperty]
        private bool _isSecondChannelEnable = false;
        [ObservableProperty]
        private bool _isThirdChannelEnable = false;
        [ObservableProperty]
        private bool _isFourthChannelEnable = false;

        [ObservableProperty]
        private bool _isChannelASave = false;
        [ObservableProperty]
        private bool _isChannelBSave = false;
        [ObservableProperty]
        private bool _isChannelCSave = false;
        [ObservableProperty]
        private bool _isChannelDSave = false;

        [ObservableProperty]
        private string _root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MultiChannelCapture");//默认路径

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
            Task.Run(() =>
            {
                string filepath = Path.Combine(Root, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
                if (IsChannelASave || IsChannelBSave || IsChannelCSave || IsChannelDSave && !Directory.Exists(filepath))
                    Directory.CreateDirectory(filepath);

                if (IsChannelASave) ChannelSave(0, filepath);
                if (IsChannelBSave) ChannelSave(1, filepath);
                if (IsChannelCSave) ChannelSave(2, filepath);
                if (IsChannelDSave) ChannelSave(3, filepath);

                if (IsChannelASave || IsChannelBSave || IsChannelCSave || IsChannelDSave)
                    WeakReferenceMessenger.Default.Send<MultiChannelMergeMessage>(
                        new MultiChannelMergeMessage(Path.Combine(filepath, "MergeImage.tif"), IsChannelASave, IsChannelBSave, IsChannelCSave, IsChannelDSave));
                        
                OpenFolderAndSelectFile(filepath);

                int code = 0;
                if (_isChannelASwitch) code = 0;
                if (_isChannelBSwitch) code = 1;
                if (_isChannelCSwitch) code = 2;
                if (_isChannelDSwitch) code = 3;
                GlobalValue.GlobalLaser?.SetStatus(code, true);
                WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(code, true), nameof(LaserMessage));
                WeakReferenceMessenger.Default.Send<SpindiskMessage, string>(new SpindiskMessage(code), nameof(SpindiskMessage));
                WeakReferenceMessenger.Default.Send<MainDisplayMessage>(new MainDisplayMessage(code));

                GlobalValue.GlobalCamera?.StopCapture();
                GlobalValue.GlobalCamera?.StartCapture();//热重载
            });
        }

        private void ChannelSave(int ChannelId, string filepath)
        {
            GlobalValue.GlobalLaser?.SetStatus(ChannelId, true);//激光设置
            WeakReferenceMessenger.Default.Send<LaserMessage, string>(new LaserMessage(ChannelId, true), nameof(LaserMessage));//切换显示
            WeakReferenceMessenger.Default.Send<SpindiskMessage, string>(new SpindiskMessage(ChannelId), nameof(SpindiskMessage));//旋转台切换
            Thread.Sleep(1500);//频道切换缓冲
            WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(ChannelId, true, Path.Combine(filepath, GetFilename(ChannelId))));//存图
            Thread.Sleep(200);
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
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select," + fileFullName;
            System.Diagnostics.Process.Start(psi);
        }

    }
}

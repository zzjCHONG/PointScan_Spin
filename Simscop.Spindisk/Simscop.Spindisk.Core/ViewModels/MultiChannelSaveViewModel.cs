using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Simscop.Spindisk.Core.Messages;
using Simscop.Spindisk.Core.Models;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;

namespace Simscop.Spindisk.Core.ViewModels
{
    public partial class MultiChannelSaveViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool _isChannelASave=false;
        [ObservableProperty]
        private bool _isChannelAColored = false;
        [ObservableProperty]
        private int _colorModeA = 0;
        partial void OnIsChannelASaveChanged(bool value)
        {
            if (!value)
            {
                IsChannelAColored = false;
            }
            else
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(0, true));
            }
        }
        partial void OnIsChannelAColoredChanged(bool value)
        {
            if (!value) ColorModeA = 0;
        }
        partial void OnColorModeAChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(0, true));
            WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeA));      
        }

        [ObservableProperty]
        private bool _isChannelBSave = false;
        [ObservableProperty]
        private bool _isChannelBColored = false;
        [ObservableProperty]
        private int _colorModeB = 0;
        partial void OnIsChannelBSaveChanged(bool value)
        {
            if (!value)
            {
                IsChannelBColored = false;
            }
            else
            { 
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(1, true));
            }
        }
        partial void OnIsChannelBColoredChanged(bool value)
        {
            if (!value) ColorModeB = 0;
        }
        partial void OnColorModeBChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(1, true));
            WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeB));
        }

        [ObservableProperty]
        private bool _isChannelCSave = false;
        [ObservableProperty]
        private bool _isChannelCColored = false;
        [ObservableProperty]
        private int _colorModeC = 0;
        partial void OnIsChannelCSaveChanged(bool value)
        {
            if (!value)
            {
                IsChannelCColored = false;
            }
            else
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(2, true));
            }
        }
        partial void OnIsChannelCColoredChanged(bool value)
        {
            if (!value) ColorModeC = 0;
        }
        partial void OnColorModeCChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(2, true));
            WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeC));
        }

        [ObservableProperty]
        private bool _isChannelDSave = false;
        [ObservableProperty]
        private bool _isChannelDColored = false;
        [ObservableProperty]
        private int _colorModeD = 0;
        partial void OnIsChannelDSaveChanged(bool value)
        {
            if (!value)
            {
                IsChannelDColored = false;
            }
            else
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(3, true));
            }
        }
        partial void OnIsChannelDColoredChanged(bool value)
        {
            if (!value) ColorModeD = 0;
        }
        partial void OnColorModeDChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(3, true));
            WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeD));
        }

        [ObservableProperty]
        private string _root = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        [RelayCommand]
        void OpenFileDialog()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择存图文件夹";
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.SelectedPath = Root;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Root = folderBrowserDialog.SelectedPath;
            }
        }

        string GetFilename(int channelID)
        {
            string channelName = string.Empty;
            string colorMode=string.Empty;         
            switch (channelID)
            {
                case 0:
                    channelName = "DAPI-405nm";
                    colorMode = MatExtension.Colors[ColorModeA];
                    break;
                case 1:
                    channelName = "FITC-488nm";
                    colorMode = MatExtension.Colors[ColorModeB];
                    break;
                case 2:
                    channelName = "TRITC-561nm";
                    colorMode = MatExtension.Colors[ColorModeC];
                    break;
                case 3:
                    channelName = "CY5-640nm";
                    colorMode = MatExtension.Colors[ColorModeD];
                    break;
                default:
                    break;
            }
            return $"{channelID + 1}_{channelName}_{colorMode}.tif";
        }

        [RelayCommand]
        void SaveMultiChannel()
        {
            string filepath = Path.Combine(Root, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
            if(!Directory.Exists(filepath))Directory.CreateDirectory(filepath);

            if (IsChannelASave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(0, true));//通道切换
                WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeA));//伪彩设置
                WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(0)), MessageManage.DisplayFrame);//存图
            }
            if (IsChannelBSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(1, true));
                WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeB));
                WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(1)), MessageManage.DisplayFrame);
            }
            if (IsChannelCSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(2, true));
                WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeC));
                WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(2)), MessageManage.DisplayFrame);
            }
            if (IsChannelDSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveLaserMessage>(new MultiChannelSaveLaserMessage(3, true));
                WeakReferenceMessenger.Default.Send<MultiChannelSaveShellMessage>(new MultiChannelSaveShellMessage(ColorModeD));
                WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(3)), MessageManage.DisplayFrame);
            }

            OpenFolderAndSelectFile(filepath);
            IsChannelASave = false;
            IsChannelBSave = false;
            IsChannelCSave = false;
            IsChannelDSave = false;
        }

        private void OpenFolderAndSelectFile(String fileFullName)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics. ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select," + fileFullName;
            System.Diagnostics.Process.Start(psi);
        }
    }
}

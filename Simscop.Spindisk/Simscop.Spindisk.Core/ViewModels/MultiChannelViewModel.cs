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
    public partial class MultiChannelViewModel: ObservableObject
    {
        [ObservableProperty]
        private bool _isChannelASave=false;
        [ObservableProperty]
        private int _colorModeA = 0;
        partial void OnIsChannelASaveChanged(bool value)
        {
            if (!value)
            {
                ColorModeA = 0;
            }
            else
            {
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(0, true));
            }
        }
        partial void OnColorModeAChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(0, true));//激光通道开启
            WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeA));//选中的伪彩      
        }

        [ObservableProperty]
        private bool _isChannelBSave = false;
        [ObservableProperty]
        private int _colorModeB = 0;
        partial void OnIsChannelBSaveChanged(bool value)
        {
            if (!value)
            {
                ColorModeB = 0;
            }
            else
            { 
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(1, true));
            }
        }
        partial void OnColorModeBChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(1, true));
            WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeB));
        }

        [ObservableProperty]
        private bool _isChannelCSave = false;
        [ObservableProperty]
        private int _colorModeC = 0;
        partial void OnIsChannelCSaveChanged(bool value)
        {
            if (!value)
            {
                ColorModeC = 0;
            }
            else
            {
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(2, true));
            }
        }
        partial void OnColorModeCChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(2, true));
            WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeC));
        }

        [ObservableProperty]
        private bool _isChannelDSave = false;
        [ObservableProperty]
        private int _colorModeD = 0;
        partial void OnIsChannelDSaveChanged(bool value)
        {
            if (!value)
            {
                ColorModeD = 0;
            }
            else
            {
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(3, true));
            }
        }
        partial void OnColorModeDChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(3, true));
            WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeD));
        }

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
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(0, true));//通道切换
                WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeA));//伪彩设置
                //WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(0)), MessageManage.DisplayFrame);//存实时处理图
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(0, true, Path.Combine(filepath, GetFilename(0))));//存实时原图
            }
            if (IsChannelBSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(1, true));
                WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeB));
                //WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(1)), MessageManage.DisplayFrame);
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(1, true, Path.Combine(filepath, GetFilename(1))));
            }
            if (IsChannelCSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(2, true));
                WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeC));
                //WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(2)), MessageManage.DisplayFrame);
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(2, true, Path.Combine(filepath, GetFilename(2))));
            }
            if (IsChannelDSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelLaserMessage>(new MultiChannelLaserMessage(3, true));
                WeakReferenceMessenger.Default.Send<MultiChannelColorMessage>(new MultiChannelColorMessage(ColorModeD));
                //WeakReferenceMessenger.Default.Send<string, string>(Path.Combine(filepath, GetFilename(3)), MessageManage.DisplayFrame);
                WeakReferenceMessenger.Default.Send<CameraSaveMessage>(new CameraSaveMessage(3, true, Path.Combine(filepath, GetFilename(3))));
            }

            WeakReferenceMessenger.Default.Send<MultiChannelMergeMessage>();

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

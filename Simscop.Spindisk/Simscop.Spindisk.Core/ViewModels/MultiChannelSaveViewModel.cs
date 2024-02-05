using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using Simscop.Spindisk.Core.Messages;
using System;
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
            if (!value) IsChannelAColored = false;
        }
        partial void OnIsChannelAColoredChanged(bool value)
        {
            if (!value) ColorModeA = 0;
        }
        partial void OnColorModeAChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeA));
        }

        [ObservableProperty]
        private bool _isChannelBSave = false;
        [ObservableProperty]
        private bool _isChannelBColored = false;
        [ObservableProperty]
        private int _colorModeB = 0;
        partial void OnIsChannelBSaveChanged(bool value)
        {
            if (!value) IsChannelBColored = false;
        }
        partial void OnIsChannelBColoredChanged(bool value)
        {
            if (!value) ColorModeB = 0;
        }
        partial void OnColorModeBChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeB));
        }

        [ObservableProperty]
        private bool _isChannelCSave = false;
        [ObservableProperty]
        private bool _isChannelCColored = false;
        [ObservableProperty]
        private int _colorModeC = 0;
        partial void OnIsChannelCSaveChanged(bool value)
        {
            if (!value) IsChannelCColored = false;
        }
        partial void OnIsChannelCColoredChanged(bool value)
        {
            if (!value) ColorModeC = 0;
        }
        partial void OnColorModeCChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeC));
        }

        [ObservableProperty]
        private bool _isChannelDSave = false;
        [ObservableProperty]
        private bool _isChannelDColored = false;
        [ObservableProperty]
        private int _colorModeD = 0;
        partial void OnIsChannelDSaveChanged(bool value)
        {
            if (!value) IsChannelDColored = false;
            if (!value || !IsChannelDColored) ColorModeD = 0;
        }
        partial void OnIsChannelDColoredChanged(bool value)
        {
            if (!value) ColorModeD = 0;
        }
        partial void OnColorModeDChanged(int value)
        {
            WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeD));
        }

        [ObservableProperty]
        private string _root = @"C:\\Users\\Administrator\\Desktop\\新建文件夹";

        [RelayCommand]
        void OpenFileDialog()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择存图文件夹";
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Root = folderBrowserDialog.SelectedPath;
            }
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
            return Path.Combine(Root, $"{channelName}.tif");
        }

        [RelayCommand]
        void SaveMultiChannel()
        {
            //目前依附于显示存图

            if (IsChannelASave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeA));//伪彩设置
                WeakReferenceMessenger.Default.Send<string, string>(GetFilename(0), MessageManage.DisplayFrame);//存图
            }
            if (IsChannelBSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeB));
                WeakReferenceMessenger.Default.Send<string, string>(GetFilename(1), MessageManage.DisplayFrame);
            }
            if (IsChannelCSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeC));
                WeakReferenceMessenger.Default.Send<string, string>(GetFilename(2), MessageManage.DisplayFrame);
            }
            if (IsChannelDSave)
            {
                WeakReferenceMessenger.Default.Send<MultiChannelSaveMessage>(new MultiChannelSaveMessage(ColorModeD));
                WeakReferenceMessenger.Default.Send<string, string>(GetFilename(3), MessageManage.DisplayFrame);
            }
        }

    }
}

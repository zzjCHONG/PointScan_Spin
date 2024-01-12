using CommunityToolkit.Mvvm.ComponentModel;

namespace Simscop.Spindisk.Core.Models;

/**
 *
首先要开启直方图统计功能，然后直方图数据会在waitforframe中体现，获取到的一串数据分为三个部分
头文件+图像数据+直方图数据，其中直方图数据部分，每个灰度值个数占据4个字节；
获取直方图数据参考如下代码
int nSize = (int)(m_drawframe.uiImgSize + m_drawframe.usHeader + m_drawframe.uiHstSize);
byte[] pBuf = new byte[nSize];
Marshal.Copy(m_drawframe.pBuffer, pBuf, 0, nSize);
Buffer.BlockCopy(pBuf, (int)(m_drawframe.usHeader+m_drawframe.uiImgSize), pBuf, 0, (int)(m_drawframe.uiHstSize)); 

 */

public partial class HisttogramModel:ObservableObject
{
    [ObservableProperty]
    private double _minLevel=0;

    [ObservableProperty]
    private double _maxLevel=0;


}
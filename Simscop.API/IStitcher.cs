using OpenCvSharp;

/// <summary>
/// 提供拼接
/// </summary>
public interface IStitcherProvider
{
    /// <summary>
    /// 捕获某一帧图像
    /// </summary>
    /// <returns>
    /// mat 某一帧图像，要求为8U类型
    /// x   采集图像的实际坐标
    /// y   采集图像的实际坐标
    /// 
    /// (x,y) 实际为中心点坐标
    /// </returns>
    public (Mat mat, double x, double y) Provide();
}

/// <summary>
/// 拼接器
/// </summary>
public interface IStitcher
{
    /// <summary>
    /// 采集设备的物理单位（μm）
    /// </summary>
    public string Unit { get; set; }

    /// <summary>
    /// 理论最终图像的宽度
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 理论最终图像的高度
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// 最终图像左上角的实际坐标
    /// </summary>
    public (double x, double y) LeftTop { get; set; }

    /// <summary>
    /// 最终图像右上角的实际坐标
    /// </summary>
    public (double x, double y) RightBottom { get; set; }

    /// <summary>
    /// 每个像素对应的理论实际物理距离
    /// 各个倍镜对应的比例尺
    /// </summary>
    public double PerPixelDistance { get; set; }

    /// <summary>
    /// 图像数据提供者
    /// </summary>
    public IStitcherProvider Provider { get; set; }

    /// <summary>
    /// 某次有新数据后运行
    /// </summary>
    /// <returns>
    /// 返回值不影响最终拼接
    ///
    /// true  - 拼接算法拼接成功
    /// false - 拼接算法拼接失败，使用直接拼接的方法
    /// </returns>
    public bool Step();

    /// <summary>
    /// 拼接结果
    /// </summary>
    public Mat StitchMat { get; set; }
}
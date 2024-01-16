using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Simscop.API;

public class AndorAPI
{
    /// <summary>
    /// 打开特定相机，获得相机句柄
    ///  open up a handle to a particular camera
    /// </summary>
    /// <param name="CameraIndex"> the index of the camera that you wish to open</param>
    /// <param name="Hndl">返回相机句柄</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_Open", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int Open(int CameraIndex, ref int Hndl);

    /// <summary>
    /// 打开特定设备的句柄，可代替Open
    /// open up a handle to a particular device
    /// </summary>
    /// <param name="Device">设备描述符</param>
    /// <param name="Hndl">返回的句柄</param>
    /// <returns></returns>

    [DllImport("atcore.dll", EntryPoint = "AT_OpenDevice", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int OpenDevice(string Device, ref int Hndl);

    /// <summary>
    /// 关闭先前打开的相机句柄
    /// close a previously opened handle to a camera
    /// </summary>
    /// <param name="Hndl">open获得的handle</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_Close", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int Close(int Hndl);

    /// <summary>
    /// 判定Feature是否实现
    /// determine whether the camera has implemented the feature specified by the Feature parameter
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Implemented">可实现返回AT_TRUE或，否则为AT_FALSE</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_IsImplemented", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsImplemented(int Hndl, string Feature, ref int Implemented);

    /// <summary>
    /// 判定Feature是否可修改
    /// 永久不可改
    /// determine whether the feature specified by the Feature parameter can be modified
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="ReadOnly">可实现返回AT_TRUE或，否则为AT_FALSE</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_IsReadOnly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsReadOnly(int Hndl, string Feature, ref int ReadOnly);

    /// <summary>
    /// 判定Feature是否可修改
    /// determine whether the feature specified by the Feature parameter can currently be modified
    /// 暂时不可改
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Writable">可实现返回AT_TRUE或，否则为AT_FALSE</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_IsWritable", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsWritable(int Hndl, string Feature, ref int Writable);

    /// <summary>
    /// 判定Feature是否可读取
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Readable">可实现返回AT_TRUE或，否则为AT_FALSE</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_IsReadable", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsReadable(int Hndl, string Feature, ref int Readable);

    public delegate int FeatureCallback(int Hndl, string Feature, IntPtr Context);

    /// <summary>
    /// 回调，特性的值或其他属性更改时检索通知
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="EvCallback"></param>
    /// <param name="Context">提供上下文信息</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_RegisterFeatureCallback", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int RegisterFeatureCallback(int Hndl, string Feature, FeatureCallback EvCallback, IntPtr Context);

    /// <summary>
    /// 取消回调
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="EvCallback"></param>
    /// <param name="Context"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_UnregisterFeatureCallback", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int UnregisterFeatureCallback(int Hndl, string Feature, FeatureCallback EvCallback, IntPtr Context);

    /// <summary>
    /// 初始化SDK库，调用SDK前使用
    ///  prepare the SDK internal structures for use and must be called before any other SDK functions have been called
    /// </summary>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_InitialiseLibrary", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int InitialiseLibrary();

    /// <summary>
    /// 释放SDK资源
    ///  free up any resources used by the SDK
    /// </summary>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_FinaliseLibrary", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int FinaliseLibrary();

    /// <summary>
    /// 修改INT类型的特征，若设置只读、当前不可写、特征不是整数、未由相机实现则报错
    /// modify the value of the specified feature
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_SetInt", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int SetInt(int Hndl, string Feature, int Value);

    /// <summary>
    /// 返回INT类型特征的当前值，若设置只读、当前不可写、特征不是整数、未由相机实现则报错
    ///  return the current value for the specified feature
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetInt", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetInt(int Hndl, string Feature, ref int Value);//AT_64 *

    /// <summary>
    /// 获取INT特性的最大允许值
    ///  return the maximum allowable value for the specified integer type feature
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="MaxValue"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetIntMax", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetIntMax(int Hndl, string Feature, ref int MaxValue);//AT_64 *

    /// <summary>
    /// 获取INT特性的最小允许值
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="MinValue"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetIntMin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetIntMin(int Hndl, string Feature, ref int MinValue);

    /// <summary>
    /// 修改Float类型的特征，若设置只读、当前不可写、特征不是Float、未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_SetFloat", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int SetFloat(int Hndl, string Feature, double Value);

    /// <summary>
    /// 返回Float类型特征的当前值，若设置只读、当前不可写、特征不是Float、未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetFloat", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetFloat(int Hndl, string Feature, ref double Value);//double *

    /// <summary>
    /// 获取FLOAT特性的最大允许值
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="MaxValue"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetFloatMax", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetFloatMax(int Hndl, string Feature, ref double MaxValue);//double *

    /// <summary>
    /// 获取INT特性的最小允许值
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="MinValue"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetFloatMin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetFloatMin(int Hndl, string Feature, ref double MinValue);//double *

    /// <summary>
    /// 设定指定的bool类型的值，若设置只读、当前不可写、特征不是bool、未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_SetBool", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int SetBool(int Hndl, string Feature, int Value);

    /// <summary>
    /// 返回Bool类型特征的当前值，若设置只读、当前不可写、特征不是Bool、未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetBool", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetBool(int Hndl, string Feature, ref int Value);

    /// <summary>
    /// 触发指定命令执行，若设置当前不可写，非命令功能或未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_Command", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int Command(int Hndl, string Feature);

    /// <summary>
    /// 设定指定字符串特征的值，若设置当前不可写，特征不是String，未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="string"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_SetString", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int Setstring(int Hndl, string Feature, string String);

    /// <summary>
    /// 返回指定字符串特征的当前值，若设置当前不可读，特征不是String，未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="string"></param>
    /// <param name="stringLength"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetString", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    //public static extern int GetString(int Hndl, string Feature, out string String, int stringLength);
    public static extern int GetString(int Hndl, string Feature, StringBuilder String, int stringlength);

    /// <summary>
    /// 返回指定字符串特征的最大长度，用于Getstring
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="MaxstringLength"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetstringMaxLength", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetstringMaxLength(int Hndl, string Feature, ref int MaxstringLength);

    /// <summary>
    /// 设定指定枚举特性的当前选定索引，索引从0开始
    /// 特性只读、当前不可写、索引在允许范围之外，不是枚举特性、未被相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_SetEnumIndex", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int SetEnumIndex(int Hndl, string Feature, int Value);

    /// <summary>
    /// 设置指定枚举特征的当前值，String参数需当前有效且在可允许范围内
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="string"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_SetEnumstring", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int SetEnumstring(int Hndl, string Feature, string String);

    /// <summary>
    /// 检索指定枚举特性的当前选定索引
    /// 当前不可读、指定功能不是枚举类型、未由相机实现则报错
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetEnumIndex", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetEnumIndex(int Hndl, string Feature, ref int Value);

    /// <summary>
    /// 返回指定枚举特性的可设置数量
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Count"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetEnumCount", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetEnumCount(int Hndl, string Feature, ref int Count);

    /// <summary>
    /// 返回指定枚举特性索引的文本表示形式，索引从0开始，Index由GetEnumCount获得，需设置stringLength
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Index"></param>
    /// <param name="string"></param>
    /// <param name="stringLength"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_GetEnumStringByIndex", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetEnumstringByIndex(int Hndl, string Feature, int Index, StringBuilder String, int stringLength);

    /// <summary>
    /// 是否可选指定的枚举索引
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Index"></param>
    /// <param name="Available"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_IsEnumIndexAvailable", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsEnumIndexAvailable(int Hndl, string Feature, int Index, ref int Available);

    /// <summary>
    /// 相机是否支持指定的枚举特征索引。过滤实际可能不可用但仍列举出来的特性。
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Feature"></param>
    /// <param name="Index"></param>
    /// <param name="Implemented"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_IsEnumIndexImplemented", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsEnumIndexImplemented(int Hndl, string Feature, int Index, ref int Implemented);

    /// <summary>
    /// 配置存储获取的图像内存区域，调用WaitBuffer或Flush前，缓存区不可修改或释放
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="ImageBuffer"></param>
    /// <param name="ImageBufferSize">单个图像的大小，字节为单位</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_QueueBuffer", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int QueueBuffer(int Hndl, byte[] ImageBuffer, int ImageBufferSize);//AT_U8*， byte[]或string/ref byte

    /// <summary>
    /// QueueBuffer使用后，接收图像缓存区的数据
    /// </summary>
    /// <param name="Hndl"></param>
    /// <param name="Ptr">返回可用的缓存区地址</param>
    /// <param name="PtrSize">返回图像缓存区的大小</param>
    /// <param name="Timeout">等待时间设置</param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_WaitBuffer", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int WaitBuffer(int Hndl, ref IntPtr Ptr, ref int PtrSize, uint Timeout);//AT_U8**

    /// <summary>
    /// 清除QueueBuffer队列的所有缓存区。需在AT_Command后调用，否则将在下次采图时将重复调用，有无法预知的错误
    /// </summary>
    /// <param name="Hndl"></param>
    /// <returns></returns>
    [DllImport("atcore.dll", EntryPoint = "AT_Flush", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int Flush(int Hndl);



    //无对应说明
    [DllImport("atcore.dll", EntryPoint = "AT_SetEnumerated", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int SetEnumerated(int Hndl, string Feature, int Value);

    [DllImport("atcore.dll", EntryPoint = "AT_SetEnumeratedString", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int SetEnumeratedString(int Hndl, string Feature, string String);//const AT_WC*

    [DllImport("atcore.dll", EntryPoint = "AT_GetEnumerated", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetEnumerated(int Hndl, string Feature, ref int Value);

    [DllImport("atcore.dll", EntryPoint = "AT_GetEnumeratedCount", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetEnumeratedCount(int Hndl, string Feature, ref int Count);

    [DllImport("atcore.dll", EntryPoint = "AT_IsEnumeratedIndexAvailable", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsEnumeratedIndexAvailable(int Hndl, string Feature, int Index, ref int Available);

    [DllImport("atcore.dll", EntryPoint = "AT_IsEnumeratedIndexImplemented", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int IsEnumeratedIndexImplemented(int Hndl, string Feature, int Index, ref int Implemented);

    [DllImport("atcore.dll", EntryPoint = "AT_GetEnumeratedString", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetEnumeratedstring(int Hndl, string Feature, int Index, StringBuilder String, int stringLength);



    //atutility.dll
    [DllImport("atutility.dll", EntryPoint = "AT_ConvertBuffer", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int ConvertBuffer(byte[] inputBuffer, byte[] outputBuffer, int width, int height, int stride, string inputPixelEncoding, string outputPixelEncoding);

    [DllImport("atutility.dll", EntryPoint = "AT_ConvertBufferUsingMetadata", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int ConvertBufferUsingMetadata(byte[] inputBuffer, byte[] outputBuffer, int imagesizebytes, string outputPixelEncoding);

    [DllImport("atutility.dll", EntryPoint = "AT_GetWidthFromMetadata", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetWidthFromMetadata(byte[] inputBuffer, int imagesizebytes, ref int width);

    [DllImport("atutility.dll", EntryPoint = "AT_GetHeightFromMetadata", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetHeightFromMetadata(byte[] inputBuffer, int imagesizebytes, ref int height);

    [DllImport("atutility.dll", EntryPoint = "AT_GetStrideFromMetadata", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetStrideFromMetadata(byte[] inputBuffer, int imagesizebytes, ref int height);

    [DllImport("atutility.dll", EntryPoint = "AT_GetPixelEncodingFromMetadata", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetPixelEncodingFromMetadata(IntPtr inputBuffer, int imagesizebytes, string pixelEncoding, byte pixelEncodingSize);

    [DllImport("atutility.dll", EntryPoint = "AT_GetTimeStampFromMetadata", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetTimeStampFromMetadata(IntPtr inputBuffer, int imagesizebytes, ref int timeStamp);

    [DllImport("atutility.dll", EntryPoint = "AT_InitialiseUtilityLibrary", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int InitialiseUtilityLibrary();

    [DllImport("atutility.dll", EntryPoint = "AT_FinaliseUtilityLibrary", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int FinaliseUtilityLibrary();
}

public enum AndorErrorCodeEnum
{
    NO_DEFINE = -1,
    AT_SUCCESS = 0,
    AT_ERR_NOTINITIALISED = 1,
    AT_ERR_NOTIMPLEMENTED = 2,
    AT_ERR_READONLY = 3,
    AT_ERR_NOTREADABLE = 4,
    AT_ERR_NOTWRITABLE = 5,
    AT_ERR_OUTOFRANGE = 6,
    AT_ERR_INDEXNOTAVAILABLE = 7,
    AT_ERR_INDEXNOTIMPLEMENTED = 8,
    AT_ERR_EXCEEDEDMAXSTRINGLENGTH = 9,
    AT_ERR_CONNECTION = 10,
    AT_ERR_NODATA = 11,
    AT_ERR_INVALIDHANDLE = 12,
    AT_ERR_TIMEDOUT = 13,
    AT_ERR_BUFFERFULL = 14,
    AT_ERR_INVALIDSIZE = 15,
    AT_ERR_INVALIDALIGNMENT = 16,
    AT_ERR_COMM = 17,
    AT_ERR_STRINGNOTAVAILABLE = 18,
    AT_ERR_STRINGNOTIMPLEMENTED = 19,
    AT_ERR_NULL_FEATURE = 20,
    AT_ERR_NULL_HANDLE = 21,
    AT_ERR_NULL_IMPLEMENTED_VAR = 22,
    AT_ERR_NULL_READABLE_VAR = 23,
    AT_ERR_NULL_READONLY_VAR = 24,
    AT_ERR_NULL_WRITABLE_VAR = 25,
    AT_ERR_NULL_MINVALUE = 26,
    AT_ERR_NULL_MAXVALUE = 27,
    AT_ERR_NULL_VALUE = 28,
    AT_ERR_NULL_STRING = 29,
    AT_ERR_NULL_COUNT_VAR = 30,
    AT_ERR_NULL_ISAVAILABLE_VAR = 31,
    AT_ERR_NULL_MAXSTRINGLENGTH = 32,
    AT_ERR_NULL_EVCALLBACK = 33,
    AT_ERR_NULL_QUEUE_PTR = 34,
    AT_ERR_NULL_WAIT_PTR = 35,
    AT_ERR_NULL_PTRSIZE = 36,
    AT_ERR_NOMEMORY = 37,
    AT_ERR_DEVICEINUSE = 38,
    AT_ERR_DEVICENOTFOUND = 39,
    AT_ERR_HARDWARE_OVERFLOW = 100,

    AT_ERR_INVALIDOUTPUTPIXELENCODING = 1002,
    AT_ERR_INVALIDINPUTPIXELENCODING = 1003,
    AT_ERR_INVALIDMETADATAINFO = 1004,
    AT_ERR_CORRUPTEDMETADATA = 1005,
    AT_ERR_METADATANOTFOUND = 1006,

}

public enum CycleModeEnum
{
    Continuous = 0,
}
public enum PixelEncodingEnum
{
    Mono12PACKED,
    Mono12,
    Mono16,
    Mono32,
    Mono8,
}


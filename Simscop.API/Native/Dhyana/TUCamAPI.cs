using System;
using System.Runtime.InteropServices;

// NOTE 该文件有官方提供SDK,不做修改

namespace Simscop.API.Native
{
    //  The correct state [0x00000000, 0x7FFFFFFF] 
    //  The error   state [0x80000000, 0XFFFFFFFF]
    //  typedef enum TUCAM status
    public enum TUCAMRET : long
    {
        //  success
        TUCAMRET_SUCCESS = 0x00000001,       // no error, general success code, app should check the value is positive   
        TUCAMRET_FAILURE = 0x80000000,       // error    

        //  initialization error
        TUCAMRET_NO_MEMORY = 0x80000101,       // not enough memory
        TUCAMRET_NO_RESOURCE = 0x80000102,       // not enough resource except memory    
        TUCAMRET_NO_MODULE = 0x80000103,       // no sub module
        TUCAMRET_NO_DRIVER = 0x80000104,       // no driver
        TUCAMRET_NO_CAMERA = 0x80000105,       // no camera
        TUCAMRET_NO_GRABBER = 0x80000106,       // no grabber  
        TUCAMRET_NO_PROPERTY = 0x80000107,       // there is no alternative or influence id, or no more property id

        TUCAMRET_FAILOPEN_CAMERA = 0x80000110,       // fail open the camera
        TUCAMRET_FAILOPEN_BULKIN = 0x80000111,       // fail open the bulk in endpoint
        TUCAMRET_FAILOPEN_BULKOUT = 0x80000112,       // fail open the bulk out endpoint
        TUCAMRET_FAILOPEN_CONTROL = 0x80000113,       // fail open the control endpoint
        TUCAMRET_FAILCLOSE_CAMERA = 0x80000114,       // fail close the camera

        TUCAMRET_FAILOPEN_FILE = 0x80000115,       // fail open the file
        TUCAMRET_FAILOPEN_CODEC = 0x80000116,       // fail open the video codec
        TUCAMRET_FAILOPEN_CONTEXT = 0x80000117,       // fail open the video context

        //  status error
        TUCAMRET_INIT = 0x80000201,       // API requires has not initialized state.
        TUCAMRET_BUSY = 0x80000202,       // API cannot process in busy state.
        TUCAMRET_NOT_INIT = 0x80000203,       // API requires has initialized state.
        TUCAMRET_EXCLUDED = 0x80000204,       // some resource is exclusive and already used.
        TUCAMRET_NOT_BUSY = 0x80000205,       // API requires busy state.
        TUCAMRET_NOT_READY = 0x80000206,       // API requires ready state.

        //  wait error
        TUCAMRET_ABORT = 0x80000207,       // abort process
        TUCAMRET_TIMEOUT = 0x80000208,       // timeout
        TUCAMRET_LOSTFRAME = 0x80000209,       // frame data is lost
        TUCAMRET_MISSFRAME = 0x8000020A,       // frame is lost but reason is low lever driver's bug
        TUCAMRET_USB_STATUS_ERROR = 0x8000020B,       // the USB status error

        // calling error
        TUCAMRET_INVALID_CAMERA = 0x80000301,       // invalid camera
        TUCAMRET_INVALID_HANDLE = 0x80000302,       // invalid camera handle
        TUCAMRET_INVALID_OPTION = 0x80000303,       // invalid the option value of structure
        TUCAMRET_INVALID_IDPROP = 0x80000304,       // invalid property id
        TUCAMRET_INVALID_IDCAPA = 0x80000305,       // invalid capability id
        TUCAMRET_INVALID_IDPARAM = 0x80000306,       // invalid parameter id
        TUCAMRET_INVALID_PARAM = 0x80000307,       // invalid parameter
        TUCAMRET_INVALID_FRAMEIDX = 0x80000308,       // invalid frame index
        TUCAMRET_INVALID_VALUE = 0x80000309,       // invalid property value
        TUCAMRET_INVALID_EQUAL = 0x8000030A,       // invalid property value equal 
        TUCAMRET_INVALID_CHANNEL = 0x8000030B,       // the property id specifies channel but channel is invalid
        TUCAMRET_INVALID_SUBARRAY = 0x8000030C,       // the combination of subarray values are invalid. e.g. TUCAM_IDPROP_SUBARRAYHPOS + TUCAM_IDPROP_SUBARRAYHSIZE is greater than the number of horizontal pixel of sensor.
        TUCAMRET_INVALID_VIEW = 0x8000030D,       // invalid view window handle
        TUCAMRET_INVALID_PATH = 0x8000030E,       // invalid file path
        TUCAMRET_INVALID_IDVPROP = 0x8000030F,       // invalid vendor property id

        TUCAMRET_NO_VALUETEXT = 0x80000310,       // the property does not have value text
        TUCAMRET_OUT_OF_RANGE = 0x80000311,       // value is out of range

        TUCAMRET_NOT_SUPPORT = 0x80000312,       // camera does not support the function or property with current settings
        TUCAMRET_NOT_WRITABLE = 0x80000313,       // the property is not writable	
        TUCAMRET_NOT_READABLE = 0x80000314,       // the property is not readable


        TUCAMRET_WRONG_HANDSHAKE = 0x80000410,       // this error happens TUCAM get error code from camera unexpectedly
        TUCAMRET_NEWAPI_REQUIRED = 0x80000411,       // old API does not support the value because only new API supports the value

        TUCAMRET_ACCESSDENY = 0x80000412,       // the property cannot access during this TUCAM status

        TUCAMRET_NO_CORRECTIONDATA = 0x80000501,       // not take the dark and shading correction data yet.

        TUCAMRET_INVALID_PRFSETS = 0x80000601,       // the profiles set name is invalid

        TUCAMRET_DECODE_FAILURE = 0x80000701,       // the image decoding raw data to rgb data failure
        TUCAMRET_COPYDATA_FAILURE = 0x80000702,       // the image data copying failure
        TUCAMRET_ENCODE_FAILURE = 0x80000703,       // the image encoding data  to video failure
        TUCAMRET_WRITE_FAILURE = 0x80000704,		// the write the video frame failure

        //  camera or bus trouble
        TUCAMRET_FAIL_READ_CAMERA = 0x83001001,       // fail read from camera  
        TUCAMRET_FAIL_WRITE_CAMERA = 0x83001002,       // fail write to camera
        TUCAMRET_OPTICS_UNPLUGGED = 0x83001003,       // optics part is unplugged so please check it.

        TUCAMRET_RECEIVE_FINISH = 0x00000002,       // no error, vendor receive frame message  

    };

    //  typedef enum information id
    public enum TUCAM_IDINFO
    {
        TUIDI_BUS = 0x01,             // the bus type USB2.0/USB3.0
        TUIDI_VENDOR = 0x02,             // the vendor id
        TUIDI_PRODUCT = 0x03,             // the product id 
        TUIDI_VERSION_API = 0x04,             // the API version    
        TUIDI_VERSION_FRMW = 0x05,             // the firmware version
        TUIDI_VERSION_FPGA = 0x06,             // the FPGA version
        TUIDI_VERSION_DRIVER = 0x07,             // the driver version
        TUIDI_TRANSFER_RATE = 0x08,             // the transfer rate
        TUIDI_CAMERA_MODEL = 0x09,             // the camera model (string)
        TUIDI_CURRENT_WIDTH = 0x0A,             // the camera image data current width(must use TUCAM_Dev_GetInfoEx and after calling TUCAM_Buf_Alloc)
        TUIDI_CURRENT_HEIGHT = 0x0B,             // the camera image data current height(must use TUCAM_Dev_GetInfoEx and after calling TUCAM_Buf_Alloc)
        TUIDI_CAMERA_CHANNELS = 0x0C,             // the camera image data channels
        TUIDI_BCDDEVICE = 0x0D,             // the USB bcdDevice
        TUIDI_UTCTIME = 0x0F,             // the get utc time
        TUIDI_LONGITUDE_LATITUDE = 0x10,             // the get longitude latitude
        TUIDI_WORKING_TIME = 0x11,             // the get working time
        TUIDI_FAN_SPEED = 0x12,             // the get fan speed
        TUIDI_FPGA_TEMPERATURE = 0x13,             // the get fpga temperature
        TUIDI_PCBA_TEMPERATURE = 0x14,             // the get pcba temperature
        TUIDI_ENV_TEMPERATURE = 0x15,             // the get environment temperature
        TUIDI_DEVICE_ADDRESS = 0x16,             // the USB device address
        TUIDI_USB_PORT_ID = 0x17,             // the USB port id
        TUIDI_ENDINFO = 0x18,             // the string id end
    };

    // typedef enum capability id 
    public enum TUCAM_IDCAPA
    {
        TUIDC_RESOLUTION = 0x00,             // id capability resolution
        TUIDC_PIXELCLOCK = 0x01,             // id capability pixel clock
        TUIDC_BITOFDEPTH = 0x02,             // id capability bit of depth
        TUIDC_ATEXPOSURE = 0x03,             // id capability automatic exposure time  
        TUIDC_HORIZONTAL = 0x04,             // id capability horizontal
        TUIDC_VERTICAL = 0x05,             // id capability vertical
        TUIDC_ATWBALANCE = 0x06,             // id capability automatic white balance
        TUIDC_FAN_GEAR = 0x07,             // id capability fan gear
        TUIDC_ATLEVELS = 0x08,             // id capability automatic levels
        TUIDC_SHIFT = 0x09,             // (The reserved) id capability shift(15~8, 14~7, 13~6, 12~5, 11~4, 10~3, 9~2, 8~1, 7~0) [16bit]
        TUIDC_HISTC = 0x0A,             // id capability histogram statistic
        TUIDC_CHANNELS = 0x0B,             // id capability current channels(Only color camera support:0-RGB,1-Red,2-Green,3-Blue. Used in the property levels, see enum TUCHN_SELECT)
        TUIDC_ENHANCE = 0x0C,             // id capability enhance
        TUIDC_DFTCORRECTION = 0x0D,             // id capability defect correction (0-not correction, 1-calculate, 3-correction)
        TUIDC_ENABLEDENOISE = 0x0E,             // id capability enable denoise (TUIDP_NOISELEVEL effective)
        TUIDC_FLTCORRECTION = 0x0F,             // id capability flat field correction (0-not correction, 1-grab frame, 2-calculate, 3-correction)
        TUIDC_RESTARTLONGTM = 0x10,             // id capability restart long exposure time (only CCD camera support)
        TUIDC_DATAFORMAT = 0x11,             // id capability the data format(only YUV format data support 0-YUV 1-RAW)
        TUIDC_DRCORRECTION = 0x12,             // (The reserved)id capability dynamic range of correction
        TUIDC_VERCORRECTION = 0x13,             // id capability vertical correction(correction the image data show vertical, in windows os the default value is 1)
        TUIDC_MONOCHROME = 0x14,             // id capability monochromatic
        TUIDC_BLACKBALANCE = 0x15,             // id capability black balance
        TUIDC_IMGMODESELECT = 0x16,             // id capability image mode select(CMS mode)
        TUIDC_CAM_MULTIPLE = 0x17,             // id capability multiple cameras (how many cameras use at the same time, only SCMOS camera support)
        TUIDC_ENABLEPOWEEFREQUENCY = 0x18,             // id capability enable power frequency (50HZ or 60HZ)
        TUIDC_ROTATE_R90 = 0x19,                // id capability rotate 90 degree to right
        TUIDC_ROTATE_L90 = 0x1A,                // id capability rotate 90 degree to left
        TUIDC_NEGATIVE = 0x1B,              // id capability negative film enable
        TUIDC_HDR = 0x1C,				// id capability HDR enable
        TUIDC_ENABLEIMGPRO = 0x1D,             // id capability image process enable
        TUIDC_ENABLELED = 0x1E,             // id capability USB led enable
        TUIDC_ENABLETIMESTAMP = 0x1F,             // id capability time stamp enable
        TUIDC_ENABLEBLACKLEVEL = 0x20,				// id capability black level offset enable
        TUIDC_ATFOCUS = 0x21,             // id capability auto focus enable(0-manual 1-automatic focus 2-Once)
        TUIDC_ATFOCUS_STATUS = 0x22,             // id capability auto focus status(0-stop 1-focusing 2-completed 3-defocus)
        TUIDC_PGAGAIN = 0x23,             // id capability sensor pga gain
        TUIDC_ATEXPOSURE_MODE = 0x24,             // id capability automatic exposure time mode
        TUIDC_BINNING_SUM = 0x25,             // id capability the summation binning
        TUIDC_BINNING_AVG = 0x26,             // id capability the average binning
        TUIDC_FOCUS_C_MOUNT = 0x27,             // id capability the focus c-mount mode(0-normal 1-c-mount mode)
        TUIDC_ENABLEPI = 0x28,             // id capability PI enable
        TUIDC_ATEXPOSURE_STATUS = 0x29,             // id capability auto exposure status (0-doing 1-completed)
        TUIDC_ATWBALANCE_STATUS = 0x2A,             // id capability auto white balance status (0-doing 1-completed)
        TUIDC_TESTIMGMODE = 0x2B,             // id capability test image mode select
        TUIDC_SENSORRESET = 0x2C,             // id capability sensor reset
        TUIDC_PGAHIGH = 0x2D,             // id capability pga high gain
        TUIDC_PGALOW = 0x2E,             // id capability pga low gain
        TUIDC_PIXCLK1_EN = 0x2F,             // id capability pix1 clock enable
        TUIDC_PIXCLK2_EN = 0x30,             // id capability pix2 clock enable
        TUIDC_ATLEVELGEAR = 0x31,             // id capability auto level gear
        TUIDC_ENABLEDSNU = 0x32,             // id capability enable dsnu
        TUIDC_ENABLEOVERLAP = 0x33,             // id capability enable exposure time overlap mode
        TUIDC_CAMSTATE = 0x34,             // id capability camera state
        TUIDC_ENABLETRIOUT = 0x35,             // id capability enable trigger out enable
        TUIDC_ROLLINGSCANMODE = 0x36,             // id capability rolling scan mode
        TUIDC_ROLLINGSCANLTD = 0x37,             // id capability rolling scan line time delay
        TUIDC_ROLLINGSCANSLIT = 0x38,             // id capability rolling scan slit height
        TUIDC_ROLLINGSCANDIR = 0x39,             // id capability rolling scan direction
        TUIDC_ROLLINGSCANRESET = 0x3A,             // id capability rolling scan direction reset
        TUIDC_ENABLETEC = 0x3B,             // id capability TEC enable
        TUIDC_ENABLEBLC = 0x3C,             // id capability backlight compensation enable
        TUIDC_ENABLETHROUGHFOG = 0x3D,             // id capability electronic through fog enable
        TUIDC_ENABLEGAMMA = 0x3E,             // id capability gamma enable
        TUIDC_CAMPARASAVE = 0x3F,             // id capability camera parameter save
        TUIDC_CAMPARALOAD = 0x40,             // id capability camera parameter load
        TUIDC_ENDCAPABILITY = 0x41,             // id capability end 
    };

    // typedef enum property id
    public enum TUCAM_IDPROP
    {
        TUIDP_GLOBALGAIN = 0x00,             // id property global gain
        TUIDP_EXPOSURETM = 0x01,             // id property exposure time
        TUIDP_BRIGHTNESS = 0x02,             // id property brightness (Effective automatic exposure condition)
        TUIDP_BLACKLEVEL = 0x03,             // id property black level
        TUIDP_TEMPERATURE = 0x04,             // id property temperature control
        TUIDP_SHARPNESS = 0x05,             // id property sharpness
        TUIDP_NOISELEVEL = 0x06,             // id property the noise level
        TUIDP_HDR_KVALUE = 0x07,             // id property the HDR K value

        // image process property
        TUIDP_GAMMA = 0x08,             // id property gamma
        TUIDP_CONTRAST = 0x09,             // id property contrast
        TUIDP_LFTLEVELS = 0x0A,             // id property left levels
        TUIDP_RGTLEVELS = 0x0B,             // id property right levels
        TUIDP_CHNLGAIN = 0x0C,             // id property channel gain
        TUIDP_SATURATION = 0x0D,             // id property saturation
        TUIDP_CLRTEMPERATURE = 0x0E,             // id property color temperature
        TUIDP_CLRMATRIX = 0x0F,             // id property color matrix setting
        TUIDP_DPCLEVEL = 0x10,             // id property defect points correction level
        TUIDP_BLACKLEVELHG = 0x11,             // id property black level high gain
        TUIDP_BLACKLEVELLG = 0x12,             // id property black level low gain
        TUIDP_POWEEFREQUENCY = 0x13,             // id property power frequency (50HZ or 60HZ)
        TUIDP_HUE = 0x14,               // id property hue
        TUIDP_LIGHT = 0x15,				// id property light
        TUIDP_ENHANCE_STRENGTH = 0x16,				// id property enhance strength
        TUIDP_NOISELEVEL_3D = 0x17,				// id property the 3D noise level
        TUIDP_FOCUS_POSITION = 0x18,             // id property focus position
        TUIDP_FRAME_RATE = 0x19,             // id property frame rate
        TUIDP_START_TIME = 0x1A,             // id property start time
        TUIDP_FRAME_NUMBER = 0x1B,             // id property frame number
        TUIDP_INTERVAL_TIME = 0x1C,             // id property interval time
        TUIDP_GPS_APPLY = 0x1D,             // id property gps apply
        TUIDP_AMB_TEMPERATURE = 0x1E,             // id property ambient temperature
        TUIDP_AMB_HUMIDITY = 0x1F,             // id property ambient humidity
        TUIDP_AUTO_CTRLTEMP = 0x20,             // id property auto control temperature

        TUIDP_AVERAGEGRAY = 0x21,             // id property average gray setting
        TUIDP_AVERAGEGRAYTHD = 0x22,             // id property average gray threshold setting
        TUIDP_ENHANCETHD = 0x23,             // id property enhance threshold setting
        TUIDP_ENHANCEPARA = 0x24,             // id property enhance parameter setting
        TUIDP_EXPOSUREMAX = 0x25,             // id property max exposure time setting
        TUIDP_EXPOSUREMIN = 0x26,             // id property min exposure time setting
        TUIDP_GAINMAX = 0x27,             // id property max gain setting
        TUIDP_GAINMIN = 0x28,             // id property min gain setting
        TUIDP_THROUGHFOGPARA = 0x29,             // id property through fog parameter setting

        TUIDP_ENDPROPERTY = 0x2A,             // id property end 
    };

    // typedef enum the capture mode
    public enum TUCAM_CAPTURE_MODES
    {
        TUCCM_SEQUENCE = 0x00,             // capture start sequence mode
        TUCCM_TRIGGER_STANDARD = 0x01,             // capture start trigger standard mode
        TUCCM_TRIGGER_SYNCHRONOUS = 0x02,             // capture start trigger synchronous mode
        TUCCM_TRIGGER_GLOBAL = 0x03,             // capture start trigger global
        TUCCM_TRIGGER_SOFTWARE = 0x04,             // capture start trigger software
    };

    // typedef enum the image formats
    public enum TUIMG_FORMATS
    {
        TUFMT_RAW = 0x01,             // The format RAW
        TUFMT_TIF = 0x02,             // The format TIFF
        TUFMT_PNG = 0x04,             // The format PNG
        TUFMT_JPG = 0x08,             // The format JPEG
        TUFMT_BMP = 0x10,             // The format BMP
    };

    // typedef enum the register types
    public enum TUREG_TYPE
    {
        TUREG_SN = 0x01,             // The type register SN
        TUREG_DATA = 0x02,             // The type register data
    };

    // trigger mode
    // typedef enum the trigger exposure time mode
    public enum TUCAM_TRIGGER_EXP
    {
        TUCTE_EXPTM = 0x00,             // use exposure time 
        TUCTE_WIDTH = 0x01,             // use width level
    };

    // typedef enum the trigger edge mode
    public enum TUCAM_TRIGGER_EDGE
    {
        TUCTD_RISING = 0x01,             // rising edge
        TUCTD_FAILING = 0x00,             // failing edge
    };

    // typedef enum the trigger readout direction reset mode
    public enum TUCAM_TRIGGER_READOUTDIRRESET
    {
        TUCTD_YES = 0x00,            // yes reset
        TUCTD_NO = 0x01,            // no reset
    };

    // typedef enum the trigger readout direction mode
    public enum TUCAM_TRIGGER_READOUTDIR
    {
        TUCTD_DOWN = 0x00,            // down
        TUCTD_UP = 0x01,            // up
        TUCTD_DOWNUPCYC = 0x02,            // down up cycle
    };


    // outputtrigger mode
    // typedef enum the output trigger port mode
    public enum TUCAM_OUTPUTTRG_PORT
    {
        TUPORT_ONE = 0x00,            // use port1
        TUPORT_TWO = 0x01,            // use port2
        TUPORT_THREE = 0x02,            // use port3
    };

    // typedef enum the output trigger kind mode
    public enum TUCAM_OUTPUTTRG_KIND
    {
        TUOPT_GND = 0x00,              // use low 
        TUOPT_VCC = 0x01,              // use high
        TUOPT_IN = 0x02,              // use trigger input
        TUOPT_EXPSTART = 0x03,              // use exposure start
        TUOPT_EXPGLOBAL = 0x04,              // use global exposure 
        TUOPT_READEND = 0x05,              // use read end
    };

    // typedef enum the output trigger edge mode
    public enum TUCAM_OUTPUTTRG_EDGE
    {
        TUOPT_RISING = 0x00,             // rising edge
        TUOPT_FAILING = 0x01,             // failing edge
    };

    // typedef enum the frame formats
    public enum TUFRM_FORMATS
    {
        TUFRM_FMT_RAW = 0x10,             // The raw data
        TUFRM_FMT_USUAl = 0x11,             // The usually data
        TUFRM_FMT_RGB888 = 0x12,             // The RGB888 data for drawing
    };

    // typedef enum the SCMOS gain mode
    public enum TUGAIN_MODE
    {
        TUGAIN_HDR = 0x00,             // The HDR mode
        TUGAIN_HIGH = 0x01,             // The High gain mode
        TUGAIN_LOW = 0x02,             // The Low gain mode
    };


    /* **************************************************************** *

        struct defines

    * **************************************************************** */

    // the camera initialize struct
    public struct TUCAM_INIT
    {
        public UInt32 uiCamCount;                   // [out]
        public IntPtr pstrConfigPath;               // [in] save the path of the camera parameters 
    };

    //  the camera open struct
    public struct TUCAM_OPEN
    {
        public UInt32 uiIdxOpen;                   // [in]
        public IntPtr hIdxTUCam;                   // [out] the handle of the opened camera device   
    };

    //  the image open struct
    public struct TUIMG_OPEN
    {
        public IntPtr pszfileName;                 // [in]  the full path of the image file
        public IntPtr hIdxTUImg;                   // [out] the handle of the opened image file 
    };

    // the camera value text struct
    public struct TUCAM_VALUE_INFO
    {
        public Int32 nID;                         // [in] TUCAM_IDINFO
        public Int32 nValue;                      // [in] value of information
        public IntPtr pText;					    // [in/out] text of the value
        public Int32 nTextSize;          			// [in] text buffer size
    };

    // the camera value text struct
    public struct TUCAM_VALUE_TEXT
    {
        public Int32 nID;                         // [in] TUCAM_IDPROP / TUCAM_IDCAPA
        public Double dbValue;                     // [in] value of property
        public IntPtr pText;					    // [in/out] text of the value
        public Int32 nTextSize;          			// [in] text buffer size
    };

    // the camera capability attribute
    public struct TUCAM_CAPA_ATTR
    {
        public Int32 idCapa;                      // [in] TUCAM_IDCAPA

        public Int32 nValMin;                     // [out] minimum value
        public Int32 nValMax;                     // [out] maximum value
        public Int32 nValDft;                     // [out] default value
        public Int32 nValStep;                    // [out] minimum stepping between a value and the next
    };

    // the camera property attribute
    public struct TUCAM_PROP_ATTR
    {
        public Int32 idProp;                        // [in] TUCAM_IDPROP
        public Int32 nIdxChn;                       // [in/out] the index of channel

        public Double dbValMin;                     // [out] minimum value
        public Double dbValMax;                     // [out] maximum value
        public Double dbValDft;                     // [out] default value
        public Double dbValStep;                    // [out] minimum stepping between a value and the next
    };

    // the camera ROI attribute
    public struct TUCAM_ROI_ATTR
    {
        public bool bEnable;                     // [in/out] The ROI enable

        public Int32 nHOffset;                    // [in/out] The horizontal offset
        public Int32 nVOffset;                    // [in/out] The vertical offset
        public Int32 nWidth;                      // [in/out] The ROI width
        public Int32 nHeight;                     // [in/out] The ROI height
    };

    // the camera ROI calculate attribute
    public struct TUCAM_CALC_ROI_ATTR
    {
        public bool bEnable;                     // [in/out] The ROI enable

        public Int32 idCalc;                      // [in] TUCAM_IDCROI

        public Int32 nHOffset;                    // [in/out] The horizontal offset
        public Int32 nVOffset;                    // [in/out] The vertical offset
        public Int32 nWidth;                      // [in/out] The ROI width
        public Int32 nHeight;                     // [in/out] The ROI height
    };

    // the camera trigger attribute
    public struct TUCAM_TRIGGER_ATTR
    {
        public Int32 nTgrMode;                    // [in/out] The mode of trigger 
        public Int32 nExpMode;                    // [in/out] The mode of exposure [0, 1] 0:ExposureTime time   1:Width level 
        public Int32 nEdgeMode;                   // [in/out] The mode of edge     [0, 1] 0:Falling edge    1:Rising edge
        public Int32 nDelayTm;                    // [in/out] The time delay
        public Int32 nFrames;                     // [in/out] How many frames per trigger
        public Int32 nBufFrames;                  // [in/out] How many frames in buffer
    };

    // the camera trigger out attribute
    public struct TUCAM_TRGOUT_ATTR
    {
        public Int32 nTgrOutPort;                   // [in/out] The port of trigger out 
        public Int32 nTgrOutMode;                   // [in/out] The mode of trigger out
        public Int32 nEdgeMode;                     // [in/out] The mode of edge     [0, 1] 1:Falling edge    0:Rising edge
        public Int32 nDelayTm;                      // [in/out] The time delay
        public Int32 nWidth;                        // [in/out] The width of pulse
    };

    // the camera frame struct
    public struct TUCAM_FRAME
    {
        // TUCAM_Buf_WaitForFrame() use this structure. Some members have different direction.
        // [i:o] means, the member is input at TUCAM_Buf_WaitForFrame()
        // [i:i] and [o:o] means always input and output at both function.
        // "input" means application has to set the value before calling.
        // "output" means function fills a value at returning.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] szSignature;      // [out]Copyright+Version: TU+1.0 ['T', 'U', '1', '\0']

        //  The based information
        public ushort usHeader;         // [out] The frame header size
        public ushort usOffset;         // [out] The frame data offset
        public ushort usWidth;          // [out] The frame width
        public ushort usHeight;         // [out] The frame height
        public UInt32 uiWidthStep;      // [out] The frame width step

        public byte ucDepth;           // [out] The frame data depth 
        public byte ucFormat;          // [out] The frame data format                  
        public byte ucChannels;        // [out] The frame data channels
        public byte ucElemBytes;       // [out] The frame data bytes per element
        public byte ucFormatGet;       // [in]  Which frame data format do you want    see TUFRM_FORMATS

        public UInt32 uiIndex;          // [in/out] The frame index number
        public UInt32 uiImgSize;        // [out] The frame size
        public UInt32 uiRsdSize;        // [in]  The frame reserved size    (how many frames do you want)
        public UInt32 uiHstSize;        // [out] The frame histogram size

        public IntPtr pBuffer;          // [in/out] The frame buffer

    };

    // the file save struct
    public struct TUCAM_FILE_SAVE
    {
        public Int32 nSaveFmt;         // [in] the format of save file     see TUIMG_FORMATS
        public IntPtr pstrSavePath;     // [in] the path of save file 

        public IntPtr pFrame;           // [in] the struct of camera frame
    };

    // the record save struct
    public struct TUCAM_REC_SAVE
    {
        public Int32 nCodec;           // [in] the coder-decoder type
        public IntPtr pstrSavePath;     // [in] the path of save record file
        public float fFps;             // [in] the current FPS
    };

    // the register read/write struct
    public struct TUCAM_REG_RW
    {
        public Int32 nRegType;         // [in] the format of register     see TUREG_TYPE

        public IntPtr pBuf;			    // [in/out] pointer to the buffer value
        public Int32 nBufSize;         // [in] the buffer size
    };

    //  typedef struct draw initialize
    public struct TUCAM_DRAW_INIT
    {
        public IntPtr hWnd;            // [in] Handle the draw window

        public Int32 nMode;           // [in] (The reserved)Whether use hardware acceleration (If the GPU support) default:TUDRAW_DFT
        public byte ucChannels;      // [in] The data channels
        public Int32 nWidth;          // [in] The drawing data width
        public Int32 nHeight;         // [in] The drawing data height

    };

    //  typedef struct drawing
    public struct TUCAM_DRAW
    {
        public Int32 nSrcX;           // [in/out] The x-coordinate, in pixels, of the upper left corner of the source rectangle.
        public Int32 nSrcY;           // [in/out] The y-coordinate, in pixels, of the upper left corner of the source rectangle.
        public Int32 nSrcWidth;       // [in/out] Width,  in pixels, of the source rectangle.
        public Int32 nSrcHeight;      // [in/out] Height, in pixels, of the source rectangle.

        public Int32 nDstX;           // [in/out] The x-coordinate, in MM_TEXT client coordinates, of the upper left corner of the destination rectangle.
        public Int32 nDstY;           // [in/out] The y-coordinate, in MM_TEXT client coordinates, of the upper left corner of the destination rectangle.
        public Int32 nDstWidth;       // [in/out] Width,  in MM_TEXT client coordinates, of the destination rectangle.
        public Int32 nDstHeight;      // [in/out] Height, in MM_TEXT client coordinates, of the destination rectangle.

        public IntPtr pFrame;          // [in] the struct of camera frame
    };

    public class TUCamAPI
    {
        //
        // Initialize uninitialize and misc.
        //
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Api_Init(ref TUCAM_INIT pInitParam);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Api_Uninit();

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Dev_Open(ref TUCAM_OPEN pOpenParam);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Dev_Close(IntPtr hTUCam);

        // Get some device information (VID/PID/Version)
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Dev_GetInfo(IntPtr hTUCam, ref TUCAM_VALUE_INFO pInfo);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Dev_GetInfoEx(UInt32 uiICam, ref TUCAM_VALUE_INFO pInfo);


        //
        // Capability control
        //
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Capa_GetAttr(IntPtr hTUCam, ref TUCAM_CAPA_ATTR pAttr);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Capa_GetValue(IntPtr hTUCam, Int32 nCapa, ref Int32 pnVal);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Capa_SetValue(IntPtr hTUCam, Int32 nCapa, Int32 nVal);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Capa_GetValueText(IntPtr hTUCam, ref TUCAM_VALUE_TEXT pVal);


        //
        // Property control
        //
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Prop_GetAttr(IntPtr hTUCam, ref TUCAM_PROP_ATTR pAttr);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Prop_GetValue(IntPtr hTUCam, Int32 nProp, ref Double pdbVal, Int32 nChn);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Prop_SetValue(IntPtr hTUCam, Int32 nProp, Double dbval, Int32 nChn);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Prop_GetValueText(IntPtr hTUCam, ref TUCAM_VALUE_TEXT pVal, Int32 nChn);


        //
        // Buffer control
        //
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Buf_Alloc(IntPtr hTUCam, ref TUCAM_FRAME pFrame);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Buf_Release(IntPtr hTUCam);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Buf_AbortWait(IntPtr hTUCam);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Buf_WaitForFrame(IntPtr hTUCam, ref TUCAM_FRAME pFrame, Int32 nTimeOut);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Buf_CopyFrame(IntPtr hTUCam, ref TUCAM_FRAME pFrame);


        //
        // Capturing control
        //
        // ROI
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_SetROI(IntPtr hTUCam, TUCAM_ROI_ATTR roiAttr);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_GetROI(IntPtr hTUCam, ref TUCAM_ROI_ATTR roiAttr);

        // Trigger
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_SetTrigger(IntPtr hTUCam, TUCAM_TRIGGER_ATTR tgrAttr);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_GetTrigger(IntPtr hTUCam, ref TUCAM_TRIGGER_ATTR tgrAttr);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_DoSoftwareTrigger(IntPtr hTUCam);

        // TriggerOut
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_SetTriggerOut(IntPtr hTUCam, TUCAM_TRGOUT_ATTR tgroutAttr);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_GetTriggerOut(IntPtr hTUCam, ref TUCAM_TRGOUT_ATTR pTgrOutAttr);

        // Capturing
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_Start(IntPtr hTUCam, UInt32 uiMode);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Cap_Stop(IntPtr hTUCam);


        //
        // File control
        //
        // Image
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_File_SaveImage(IntPtr hTUCam, TUCAM_FILE_SAVE fileSave);

        // Profiles
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_File_LoadProfiles(IntPtr hTUCam, IntPtr pPrfName);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_File_SaveProfiles(IntPtr hTUCam, IntPtr pPrfName);

        // Video
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Rec_Start(IntPtr hTUCam, TUCAM_REC_SAVE recSave);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Rec_AppendFrame(IntPtr hTUCam, ref TUCAM_FRAME pFrame);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Rec_Stop(IntPtr hTUCam);


        //
        // Extended control
        //
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Reg_Read(IntPtr hTUCam, TUCAM_REG_RW regRW);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Reg_Write(IntPtr hTUCam, TUCAM_REG_RW regRW);


        // Drawing control
        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Draw_Init(IntPtr hTUCam, TUCAM_DRAW_INIT drawInit);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Draw_Frame(IntPtr hTUCam, ref TUCAM_DRAW pDrawing);

        [DllImport("TUCam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern TUCAMRET TUCAM_Draw_Uninit(IntPtr hTUCam);

    }
}

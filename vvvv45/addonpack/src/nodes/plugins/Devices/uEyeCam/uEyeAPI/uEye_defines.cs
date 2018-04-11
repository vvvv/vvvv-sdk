

/// <summary>
/// Zusammenfassung für uEye.
/// </summary>
public class uEye_defines
{
	
    // ----------------------------------------Driver name-----------------------------------------
    //
    public const string DRIVER_DLL_NAME = "ueye_api.dll";


    // ----------------------------------------Color modes-----------------------------------------
    //
    public const int IS_COLORMODE_INVALID = 0;
    public const int IS_COLORMODE_MONOCHROME = 1;
    public const int IS_COLORMODE_BAYER = 2;
    //
    // --------------------------------------------------------------------------------------------

	
    // ----------------------------------------Sensor Types----------------------------------------
    //
    public const int IS_SENSOR_INVALID = 0x0;
	
    // CMOS Sensors
    public const int IS_SENSOR_UI141X_M = 0x1;          // VGA rolling shutter - VGA monochrome
    public const int IS_SENSOR_UI141X_C = 0x2;          // VGA rolling shutter - VGA color
    public const int IS_SENSOR_UI144X_M = 0x3;          // SXGA rolling shutter - SXGA monochrome
    public const int IS_SENSOR_UI144X_C = 0x4;          // SXGA rolling shutter - SXGA color
	
    public const int IS_SENSOR_UI145X_C = 0x8;          // UXGA rolling shutter - UXGA color
    public const int IS_SENSOR_UI146X_C = 0xA;          // QXGA rolling shutter - QXGA color
    public const int IS_SENSOR_UI148X_C = 0xC;          //
	
    public const int IS_SENSOR_UI121X_M = 0x10;         // VGA global shutter - VGA monochrome
    public const int IS_SENSOR_UI121X_C = 0x11;         // VGA global shutter - VGA color
    public const int IS_SENSOR_UI122X_M = 0x12;         // VGA global shutter - VGA monochrome
    public const int IS_SENSOR_UI122X_C = 0x13;         // VGA global shutter - VGA color
	
    public const int IS_SENSOR_UI164X_C = 0x20;         // SXGA rolling shutter - SXGA color
	
    public const int IS_SENSOR_UI154X_M = 0x30;         // SXGA rolling shutter - SXGA monochrome
    public const int IS_SENSOR_UI154X_C = 0x31;         // SXGA rolling shutter - SXGA color
    public const int IS_SENSOR_UI1543_M = 0x32;         // SXGA rolling shutter - SXGA monochrome
    public const int IS_SENSOR_UI1543_C = 0x33;         // SXGA rolling shutter - SXGA color
    
    public const int IS_SENSOR_UI1453_C = 0x35;         // UXGA rolling shutter - UXGA color
    public const int IS_SENSOR_UI1463_C = 0x37;         // QXGA rolling shutter - QXGA monochrome
    public const int IS_SENSOR_UI1483_C = 0x39;         //


    // CCD Sensors
    public const int IS_SENSOR_UI223X_M = 0x80;         // Sony CCD sensor - XGA monochrome
    public const int IS_SENSOR_UI223X_C = 0x81;         // Sony CCD sensor - XGA color
	
    public const int IS_SENSOR_UI241X_M = 0x82;         // Sony CCD sensor - VGA monochrome 
    public const int IS_SENSOR_UI241X_C = 0x83;         // Sony CCD sensor - VGA color 
	
    public const int IS_SENSOR_UI234X_M = 0x84;         // Sony CCD sensor - VGA monochrome 
    public const int IS_SENSOR_UI234X_C = 0x85;         // 
    
    public const int IS_SENSOR_UI233X_M = 0x86;         // Sony CCD sensor - XGA / SXGA monochrome
    public const int IS_SENSOR_UI233X_C = 0x87;         // Sony CCD sensor - XGA / SXGA color

    public const int IS_SENSOR_UI221X_M = 0x88;         // Sony CCD sensor - VGA monochrome
    public const int IS_SENSOR_UI221X_C = 0x89;         // Sony CCD sensor - VGA color
	
    public const int IS_SENSOR_UI231X_M = 0x90;         // Sony CCD sensor - VGA monochrome
    public const int IS_SENSOR_UI231X_C = 0x91;         // Sony CCD sensor - VGA color
	
    public const int IS_SENSOR_UI222X_M = 0x92;         // Sony CCD sensor - CCIR / PAL monochrome
    public const int IS_SENSOR_UI222X_C = 0x93;         // Sony CCD sensor - CCIR / PAL color
	
    public const int IS_SENSOR_UI224X_M = 0x96;         // Sony CCD sensor - SXGA monochrome	
    public const int IS_SENSOR_UI224X_C = 0x97;         // Sony CCD sensor - SXGA color
	
    public const int IS_SENSOR_UI225X_M = 0x98;         // Sony CCD sensor - UXGA monochrome
    public const int IS_SENSOR_UI225X_C = 0x99;         // Sony CCD sensor - UXGA color
    //
    // --------------------------------------------------------------------------------------------



    // **********************************************
    // return values/error codes
    // **********************************************
    public const int IS_NO_SUCCESS                      =  -1;
    public const int IS_SUCCESS                         =   0; 
    public const int IS_INVALID_CAMERA_HANDLE           =   1;
    public const int IS_INVALID_HANDLE                  =   1;
	
    public const int IS_IO_REQUEST_FAILED               =   2;
    public const int IS_CANT_OPEN_DEVICE                =   3;
    public const int IS_CANT_CLOSE_DEVICE               =   4;
    public const int IS_CANT_SETUP_MEMORY               =   5;
    public const int IS_NO_HWND_FOR_ERROR_REPORT        =   6;
    public const int IS_ERROR_MESSAGE_NOT_CREATED       =   7;
    public const int IS_ERROR_STRING_NOT_FOUND          =   8;
    public const int IS_HOOK_NOT_CREATED                =   9;
    public const int IS_TIMER_NOT_CREATED               =  10;
    public const int IS_CANT_OPEN_REGISTRY              =  11;
    public const int IS_CANT_READ_REGISTRY              =  12;
    public const int IS_CANT_VALIDATE_BOARD             =  13;
    public const int IS_CANT_GIVE_BOARD_ACCESS          =  14;
    public const int IS_NO_IMAGE_MEM_ALLOCATED          =  15;
    public const int IS_CANT_CLEANUP_MEMORY             =  16;
    public const int IS_CANT_COMMUNICATE_WITH_DRIVER    =  17;
    public const int IS_FUNCTION_NOT_SUPPORTED_YET      =  18;
    public const int IS_OPERATING_SYSTEM_NOT_SUPPORTED  =  19;
	
    public const int IS_INVALID_VIDEO_IN                =  20;
    public const int IS_INVALID_IMG_SIZE                =  21;
    public const int IS_INVALID_ADDRESS                 =  22;
    public const int IS_INVALID_VIDEO_MODE              =  23;
    public const int IS_INVALID_AGC_MODE                =  24;
    public const int IS_INVALID_GAMMA_MODE              =  25;
    public const int IS_INVALID_SYNC_LEVEL              =  26;
    public const int IS_INVALID_CBARS_MODE              =  27;
    public const int IS_INVALID_COLOR_MODE              =  28;
    public const int IS_INVALID_SCALE_FACTOR            =  29;
    public const int IS_INVALID_IMAGE_SIZE              =  30;
    public const int IS_INVALID_IMAGE_POS               =  31;
    public const int IS_INVALID_CAPTURE_MODE            =  32;
    public const int IS_INVALID_RISC_PROGRAM            =  33;
    public const int IS_INVALID_BRIGHTNESS              =  34;
    public const int IS_INVALID_CONTRAST                =  35;
    public const int IS_INVALID_SATURATION_U            =  36;
    public const int IS_INVALID_SATURATION_V            =  37;
    public const int IS_INVALID_HUE                     =  38;
    public const int IS_INVALID_HOR_FILTER_STEP         =  39;
    public const int IS_INVALID_VERT_FILTER_STEP        =  40;
    public const int IS_INVALID_EEPROM_READ_ADDRESS     =  41;
    public const int IS_INVALID_EEPROM_WRITE_ADDRESS    =  42;
    public const int IS_INVALID_EEPROM_READ_LENGTH      =  43;
    public const int IS_INVALID_EEPROM_WRITE_LENGTH     =  44;
    public const int IS_INVALID_BOARD_INFO_POINTER      =  45;
    public const int IS_INVALID_DISPLAY_MODE            =  46;
    public const int IS_INVALID_ERR_REP_MODE            =  47;
    public const int IS_INVALID_BITS_PIXEL              =  48;
    public const int IS_INVALID_MEMORY_POINTER          =  49;
	
    public const int IS_FILE_WRITE_OPEN_ERROR           =  50;
    public const int IS_FILE_READ_OPEN_ERROR            =  51;
    public const int IS_FILE_READ_INVALID_BMP_ID        =  52;
    public const int IS_FILE_READ_INVALID_BMP_SIZE      =  53;
    public const int IS_FILE_READ_INVALID_BIT_COUNT     =  54;
    public const int IS_WRONG_KERNEL_VERSION            =  55;
	
    public const int IS_RISC_INVALID_XLENGTH            =  60;
    public const int IS_RISC_INVALID_YLENGTH            =  61;
    public const int IS_RISC_EXCEED_IMG_SIZE            =  62;
	
    public const int IS_DD_MAIN_FAILED                  =  70;
    public const int IS_DD_PRIMSURFACE_FAILED           =  71;
    public const int IS_DD_SCRN_SIZE_NOT_SUPPORTED      =  72;
    public const int IS_DD_CLIPPER_FAILED               =  73;
    public const int IS_DD_CLIPPER_HWND_FAILED          =  74;
    public const int IS_DD_CLIPPER_CONNECT_FAILED       =  75;
    public const int IS_DD_BACKSURFACE_FAILED           =  76;
    public const int IS_DD_BACKSURFACE_IN_SYSMEM        =  77;
    public const int IS_DD_MDL_MALLOC_ERR               =  78;
    public const int IS_DD_MDL_SIZE_ERR                 =  79;
    public const int IS_DD_CLIP_NO_CHANGE               =  80;
    public const int IS_DD_PRIMMEM_NULL                 =  81;
    public const int IS_DD_BACKMEM_NULL                 =  82;
    public const int IS_DD_BACKOVLMEM_NULL              =  83;
    public const int IS_DD_OVERLAYSURFACE_FAILED        =  84;
    public const int IS_DD_OVERLAYSURFACE_IN_SYSMEM     =  85;
    public const int IS_DD_OVERLAY_NOT_ALLOWED          =  86;
    public const int IS_DD_OVERLAY_COLKEY_ERR           =  87;
    public const int IS_DD_OVERLAY_NOT_ENABLED          =  88;
    public const int IS_DD_GET_DC_ERROR                 =  89;
    public const int IS_DD_DDRAW_DLL_NOT_LOADED         =  90;
    public const int IS_DD_THREAD_NOT_CREATED           =  91;
    public const int IS_DD_CANT_GET_CAPS                =  92;
    public const int IS_DD_NO_OVERLAYSURFACE            =  93;
    public const int IS_DD_NO_OVERLAYSTRETCH            =  94;
    public const int IS_DD_CANT_CREATE_OVERLAYSURFACE   =  95;
    public const int IS_DD_CANT_UPDATE_OVERLAYSURFACE   =  96;
    public const int IS_DD_INVALID_STRETCH              =  97;
	
    public const int IS_EV_INVALID_EVENT_NUMBER = 100;
    public const int IS_INVALID_MODE            = 101;
    public const int IS_CANT_FIND_FALCHOOK      = 102;
    public const int IS_CANT_FIND_HOOK          = 102;
    public const int IS_CANT_GET_HOOK_PROC_ADDR = 103;
    public const int IS_CANT_CHAIN_HOOK_PROC    = 104;
    public const int IS_CANT_SETUP_WND_PROC     = 105;
    public const int IS_HWND_NULL               = 106;
    public const int IS_INVALID_UPDATE_MODE     = 107;
    public const int IS_NO_ACTIVE_IMG_MEM       = 108;
    public const int IS_CANT_INIT_EVENT         = 109;
    public const int IS_FUNC_NOT_AVAIL_IN_OS    = 110;
    public const int IS_CAMERA_NOT_CONNECTED    = 111;
    public const int IS_SEQUENCE_LIST_EMPTY     = 112;
    public const int IS_CANT_ADD_TO_SEQUENCE    = 113;
    public const int IS_LOW_OF_SEQUENCE_RISC_MEM = 114;
    public const int IS_IMGMEM2FREE_USED_IN_SEQ = 115;
    public const int IS_IMGMEM_NOT_IN_SEQUENCE_LIST = 116;
    public const int IS_SEQUENCE_BUF_ALREADY_LOCKED = 117;
    public const int IS_INVALID_DEVICE_ID       = 118;
    public const int IS_INVALID_BOARD_ID        = 119;
    public const int IS_ALL_DEVICES_BUSY        = 120;
    public const int IS_HOOK_BUSY               = 121;
    public const int IS_TIMED_OUT               = 122;
    public const int IS_NULL_POINTER            = 123;
    public const int IS_WRONG_HOOK_VERSION      = 124;
    public const int IS_INVALID_PARAMETER       = 125;
    public const int IS_NOT_ALLOWED             = 126;
    public const int IS_OUT_OF_MEMORY           = 127;
    public const int IS_INVALID_WHILE_LIVE      = 128;
    public const int IS_ACCESS_VIOLATION        = 129;
    public const int IS_UNKNOWN_ROP_EFFECT      = 130;
    public const int IS_INVALID_RENDER_MODE     = 131;
    public const int IS_INVALID_THREAD_CONTEXT  = 132;
    public const int IS_NO_HARDWARE_INSTALLED   = 133;
    public const int IS_INVALID_WATCHDOG_TIME   = 134;
    public const int IS_INVALID_WATCHDOG_MODE   = 135;
    public const int IS_INVALID_PASSTHROUGH_IN  = 136;
    public const int IS_ERROR_SETTING_PASSTHROUGH_IN = 137;
    public const int IS_FAILURE_ON_SETTING_WATCHDOG = 138;
    public const int IS_NO_USB20                = 139;
    public const int IS_CAPTURE_RUNNING	        = 140;
	
    public const int IS_MEMORY_BOARD_ACTIVATED	= 141;
    public const int IS_MEMORY_BOARD_DEACTIVATED = 142;
    public const int IS_NO_MEMORY_BOARD_CONNECTED = 143;
    public const int IS_TOO_LESS_MEMORY			= 144;
    public const int IS_IMAGE_NOT_PRESENT		= 145;
    public const int IS_MEMORY_MODE_RUNNING		= 146;
    public const int IS_MEMORYBOARD_DISABLED	    = 147;
	
    public const int IS_TRIGGER_ACTIVATED		    = 148;
    public const int IS_WRONG_KEY				    = 150;
    public const int IS_CRC_ERROR                   = 151;
    public const int IS_NOT_YET_RELEASED            = 152;   // this feature is not available yet
    public const int IS_NOT_CALIBRATED              = 153;   // the camera is not calibrated
    public const int IS_WAITING_FOR_KERNEL          = 154;   // a request to the kernel exceeded
    public const int IS_NOT_SUPPORTED               = 155;   // operation mode is not supported
    public const int IS_TRIGGER_NOT_ACTIVATED       = 156;   // operation could not execute while trigger is disabled
    public const int IS_OPERATION_ABORTED           = 157;
    public const int IS_BAD_STRUCTURE_SIZE          = 158;
    public const int IS_INVALID_BUFFER_SIZE         = 159;
    public const int IS_INVALID_PIXEL_CLOCK         = 160;
    public const int IS_INVALID_EXPOSURE_TIME       = 161;
    public const int IS_AUTO_EXPOSURE_RUNNING       = 162;

    public const int IS_CANNOT_CREATE_BB_SURF       = 163; // error creating backbuffer surface  
    public const int IS_CANNOT_CREATE_BB_MIX        = 164; // backbuffer mixer surfaces can not be created
    public const int IS_BB_OVLMEM_NULL              = 165; // backbuffer overlay mem could not be locked  
    public const int IS_CANNOT_CREATE_BB_OVL        = 166; // backbuffer overlay mem could not be created  
    public const int IS_NOT_SUPP_IN_OVL_SURF_MODE   = 167; // function not supported in overlay surface mode  
    public const int IS_INVALID_SURFACE             = 168; // surface invalid
    public const int IS_SURFACE_LOST                = 169; // surface hase been lost  
    public const int IS_RELEASE_BB_OVL_DC           = 170; // error releasing backbuffer overlay DC  
    public const int IS_BB_TIMER_NOT_CREATED        = 171; // backbuffer timer could not be created  
    public const int IS_BB_OVL_NOT_EN               = 172; // backbuffer overlay has not been enabled  
    public const int IS_ONLY_IN_BB_MODE             = 173; // only possible in backbuffer mode 
    public const int IS_INVALID_COLOR_FORMAT	    = 174; // invalid color format
	
    // ***********************************************
    // common definitions
    // ***********************************************
    public const int IS_OFF = 0;
    public const int IS_ON = 1;
    public const int IS_IGNORE_PARAMETER = -1;
	

    // ***********************************************
    // device enumeration
    // ***********************************************
    public const int IS_USE_DEVICE_ID = 0x8000;


    // ***********************************************
    // autoExit enable/disable
    // ***********************************************
    public const int IS_DISABLE_AUTO_EXIT = 0;
    public const int IS_ENABLE_AUTO_EXIT = 1;
    public const int IS_GET_AUTO_EXIT_ENABLED = 0x8000;
    	

    // ***********************************************
    // live/freeze parameters
    // ***********************************************
    public const int IS_GET_LIVE = 0x8000;
    
    public const int IS_WAIT = 1;
    public const int IS_DONT_WAIT = 0;
    public const int IS_FORCE_VIDEO_STOP = 0x4000;
    public const int IS_FORCE_VIDEO_START = 0x4000;
		

    // ***********************************************
    // video finish constants
    // ***********************************************
    public const int IS_VIDEO_NOT_FINISH = 0;
    public const int IS_VIDEO_FINISH = 1;
	
	
    // ***********************************************
    // bitmap render modes
    // ***********************************************
    public const int IS_GET_RENDER_MODE = 0x8000;
    
    public const int IS_RENDER_DISABLED = 0;
    public const int IS_RENDER_NORMAL = 1;
    public const int IS_RENDER_FIT_TO_WINDOW = 2;
    public const int IS_RENDER_DOWNSCALE_1_2 = 4;
    public const int IS_RENDER_MIRROR_UPDOWN = 16;
    public const int IS_RENDER_DOUBLE_HEIGHT = 32;
    public const int IS_RENDER_HALF_HEIGHT = 64;
	
	
    // ***********************************************
    // external trigger mode constants
    // ***********************************************
    public const int IS_GET_EXTERNALTRIGGER = 0x8000;
    public const int IS_GET_TRIGGER_STATUS = 0x8001;
    public const int IS_GET_TRIGGER_MASK = 0x8002;
    public const int IS_GET_TRIGGER_INPUTS = 0x8003;
    public const int IS_GET_TRIGGER_COUNTER = 0x8000;
    
    public const int IS_SET_TRIG_OFF = 0x0;
    public const int IS_SET_TRIG_HI_LO = 0x1;
    public const int IS_SET_TRIG_LO_HI = 0x2;
    public const int IS_SET_TRIG_SOFTWARE = 0x8;
    public const int IS_SET_TRIG_MASK = 0x100;
        
    public const int IS_GET_TRIGGER_DELAY = 0x8000;
    public const int IS_GET_MIN_TRIGGER_DELAY = 0x8001;
    public const int IS_GET_MAX_TRIGGER_DELAY = 0x8002;
    public const int IS_GET_TRIGGER_DELAY_GRANULARITY = 0x8003;
    

    // ***********************************************
    //  timing
    // ***********************************************
    // pixelclock
    public const int IS_GET_PIXEL_CLOCK = 0x8000;
    public const int IS_GET_DEFAULT_PIXEL_CLK = 0x8001;
    // framerate
    public const int IS_GET_FRAMERATE = 0x8000;
    public const int IS_GET_DEFAULT_FRAMERATE = 0x8001;
    // exposure
    public const int IS_GET_EXPOSURE_TIME = 0x8000;
    public const int IS_GET_DEFAULT_EXPOSURE = 0x8001;


    // ***********************************************
    // gain definitions
    // ***********************************************
    public const int IS_GET_MASTER_GAIN = 0x8000;
    public const int IS_GET_RED_GAIN = 0x8001;
    public const int IS_GET_GREEN_GAIN = 0x8002;
    public const int IS_GET_BLUE_GAIN = 0x8003;        
    public const int IS_GET_DEFAULT_MASTER = 0x8004;
    public const int IS_GET_DEFAULT_RED = 0x8005;
    public const int IS_GET_DEFAULT_GREEN = 0x8006;
    public const int IS_GET_DEFAULT_BLUE = 0x8007;


    // ***********************************************
    // blacklevel compensation
    // ***********************************************
    public const int IS_GET_BL_COMPENSATION = 0x8000;
    public const int IS_GET_BL_OFFSET = 0x8001;
    public const int IS_GET_BL_DEFAULT_MODE = 0x8002;
    public const int IS_GET_BL_DEFAULT_OFFSET = 0x8003;
    public const int IS_GET_BL_SUPPORTED_MODE = 0x8004;
    
    public const int IS_BL_COMPENSATION_DISABLE = 0;
    public const int IS_BL_COMPENSATION_ENABLE = 1;
    public const int IS_BL_COMPENSATION_OFFSET = 32;


    // ***********************************************
    // hardware gamma definitions
    // ***********************************************
    public const int IS_GET_HW_GAMMA = 0x8000;
    public const int IS_GET_HW_SUPPORTED_GAMMA = 0x8001;

    public const int IS_SET_HW_GAMMA_OFF = 0x0;
    public const int IS_SET_HW_GAMMA_ON = 0x1;
    
    
    // ***********************************************
    // Image parameters
    // ***********************************************
    // brightness
    public const int IS_GET_BRIGHTNESS = 0x8000;
    public const int IS_MIN_BRIGHTNESS = 0;
    public const int IS_MAX_BRIGHTNESS = 255;
    public const int IS_DEFAULT_BRIGHTNESS = -1;
    //contrast    
    public const int IS_GET_CONTRAST = 0x8000;
    public const int IS_MIN_CONTRAST = 0;
    public const int IS_MAX_CONTRAST = 511;
    public const int IS_DEFAULT_CONTRAST = -1;
    // gamma    
    public const int IS_GET_GAMMA = 0x8000;
    public const int IS_MIN_GAMMA = 1;
    public const int IS_MAX_GAMMA = 1000;
    public const int IS_DEFAULT_GAMMA = -1;
    

    // ***********************************************
    // image pos + size
    // ***********************************************    
    public const int IS_GET_IMAGE_SIZE_X = 0x8000;
    public const int IS_GET_IMAGE_SIZE_Y = 0x8001;
    public const int IS_GET_IMAGE_SIZE_X_INC = 0x8002;
    public const int IS_GET_IMAGE_SIZE_Y_INC = 0x8003;
    public const int IS_GET_IMAGE_SIZE_X_MIN = 0x8004;
    public const int IS_GET_IMAGE_SIZE_Y_MIN = 0x8005;
    public const int IS_GET_IMAGE_SIZE_X_MAX = 0x8006;
    public const int IS_GET_IMAGE_SIZE_Y_MAX = 0x8007;
    
    public const int IS_GET_IMAGE_POS_X = 0x8001;
    public const int IS_GET_IMAGE_POS_Y = 0x8002;
    public const int IS_GET_IMAGE_POS_X_ABS = 0xC001;
    public const int IS_GET_IMAGE_POS_Y_ABS = 0xC002;
    public const int IS_GET_IMAGE_POS_X_INC = 0xC003;
    public const int IS_GET_IMAGE_POS_Y_INC = 0xC004;
    public const int IS_GET_IMAGE_POS_X_MIN = 0xC005;
    public const int IS_GET_IMAGE_POS_Y_MIN = 0xC006;
    public const int IS_GET_IMAGE_POS_X_MAX = 0xC007;
    public const int IS_GET_IMAGE_POS_Y_MAX = 0xC008;
    
    public const int IS_SET_IMAGE_POS_X_ABS = 0x00010000;
    public const int IS_SET_IMAGE_POS_Y_ABS = 0x00010000;

    // Compatibility
    public const int IS_SET_IMAGEPOS_X_ABS = 0x8000;
    public const int IS_SET_IMAGEPOS_Y_ABS = 0x8000;	


    // ***********************************************
    // rop effect constants
    // ***********************************************
    public const int IS_GET_ROP_EFFECT = 0x8000;
    
    public const int IS_SET_ROP_MIRROR_NONE = 0;
    public const int IS_SET_ROP_MIRROR_UPDOWN = 8;
    public const int IS_SET_ROP_MIRROR_UPDOWN_ODD = 16;
    public const int IS_SET_ROP_MIRROR_UPDOWN_EVEN = 32;
    public const int IS_SET_ROP_MIRROR_LEFTRIGHT = 64;


    // ***********************************************
    // subsampling
    // ***********************************************
    public const int IS_GET_SUBSAMPLING = 0x8000;
    public const int IS_GET_SUBSAMPLING_MODE = 0x8001;
    public const int IS_GET_SUBSAMPLING_TYPE = 0x8002;
    
    public const int IS_SUBSAMPLING_DISABLE = 0x0;
    
    public const int IS_SUBSAMPLING_2X_VERTICAL = 0x01;
    public const int IS_SUBSAMPLING_2X_HORIZONTAL = 0x02;
    public const int IS_SUBSAMPLING_4X_VERTICAL = 0x04;
    public const int IS_SUBSAMPLING_4X_HORIZONTAL = 0x08;
    
    // Compatibility
    public const int IS_SUBSAMPLING_VERT = IS_SUBSAMPLING_2X_VERTICAL;
    public const int IS_SUBSAMPLING_HOR = IS_SUBSAMPLING_2X_HORIZONTAL;
    
    
    // ***********************************************
    // binning
    // ***********************************************
    public const int IS_GET_BINNING = 0x8000;
    public const int IS_GET_BINNING_MODE = 0x8001;
    public const int IS_GET_BINNING_TYPE = 0x8002;
    
    public const int IS_BINNING_DISABLE = 0x0;    

    public const int IS_BINNING_2X_VERTICAL = 0x01;
    public const int IS_BINNING_2X_HORIZONTAL = 0x02;
    public const int IS_BINNING_4X_VERTICAL = 0x04;
    public const int IS_BINNING_4X_HORIZONTAL = 0x08;

    public const int IS_BINNING_COLOR = 0x01;
    public const int IS_BINNING_MONO = 0x02;

    // Compatibility
    public const int IS_BINNING_VERT = IS_BINNING_2X_VERTICAL;
    public const int IS_BINNING_HOR = IS_BINNING_2X_HORIZONTAL;
    
    
    // ***********************************************
    // Auto Control Parameter
    // ***********************************************
    public const int IS_SET_ENABLE_AUTO_GAIN = 0x8800;
    public const int IS_GET_ENABLE_AUTO_GAIN = 0x8801;
    public const int IS_SET_ENABLE_AUTO_SHUTTER = 0x8802;
    public const int IS_GET_ENABLE_AUTO_SHUTTER = 0x8803;
    public const int IS_SET_ENABLE_AUTO_WHITEBALANCE = 0x8804;
    public const int IS_GET_ENABLE_AUTO_WHITEBALANCE = 0x8805;
    public const int IS_SET_ENABLE_AUTO_FRAMERATE = 0x8806;
    public const int IS_GET_ENABLE_AUTO_FRAMERATE = 0x8807;    

    public const int IS_SET_AUTO_REFERENCE = 0x8000;
    public const int IS_GET_AUTO_REFERENCE = 0x8001;
    public const int IS_SET_AUTO_GAIN_MAX = 0x8002;
    public const int IS_GET_AUTO_GAIN_MAX = 0x8003;
    public const int IS_SET_AUTO_SHUTTER_MAX = 0x8004;
    public const int IS_GET_AUTO_SHUTTER_MAX = 0x8005;
    public const int IS_SET_AUTO_SPEED = 0x8006;
    public const int IS_GET_AUTO_SPEED = 0x8007;
    public const int IS_SET_AUTO_WB_OFFSET = 0x8008;
    public const int IS_GET_AUTO_WB_OFFSET = 0x8009;
    public const int IS_SET_AUTO_WB_GAIN_RANGE = 0x800A;
    public const int IS_GET_AUTO_WB_GAIN_RANGE = 0x800B;
    public const int IS_SET_AUTO_WB_SPEED = 0x800C;
    public const int IS_GET_AUTO_WB_SPEED = 0x800D;
    public const int IS_SET_AUTO_WB_ONCE = 0x800E;
    public const int IS_GET_AUTO_WB_ONCE = 0x800F;


    // ***********************************************
    // Auto Control definitions
    // ***********************************************
    public const int IS_MIN_AUTO_BRIGHT_REFERENCE     =     0;
    public const int IS_MAX_AUTO_BRIGHT_REFERENCE     =   255;
    public const int IS_DEFAULT_AUTO_BRIGHT_REFERENCE =   128;
    public const int IS_MIN_AUTO_SPEED                =     0;
    public const int IS_MAX_AUTO_SPEED                =   100;
    public const int IS_DEFAULT_AUTO_SPEED            =    50;
    public const int IS_DEFAULT_AUTO_WB_OFFSET        =     0;
    public const int IS_MIN_AUTO_WB_OFFSET            =   -50;
    public const int IS_MAX_AUTO_WB_OFFSET            =    50;
    public const int IS_DEFAULT_AUTO_WB_SPEED         =    50;
    public const int IS_MIN_AUTO_WB_SPEED             =     0;
    public const int IS_MAX_AUTO_WB_SPEED             =   100;
    public const int IS_MIN_AUTO_WB_REFERENCE         =     0;
    public const int IS_MAX_AUTO_WB_REFERENCE         =   255;


    // ***********************************************
    // AOI types to set/get
    // ***********************************************
    public const int IS_SET_AUTO_BRIGHT_AOI = 0x8000;
    public const int IS_GET_AUTO_BRIGHT_AOI = 0x8001;
    public const int IS_SET_IMAGE_AOI = 0x8002;
    public const int IS_GET_IMAGE_AOI = 0x8003;
    public const int IS_SET_AUTO_WB_AOI = 0x8004;
    public const int IS_GET_AUTO_WB_AOI = 0x8005;


    // ***********************************************
    // color modes
    // ***********************************************
    public const int IS_GET_COLOR_MODE = 0x8000;
    
    public const int IS_SET_CM_RGB32 = 0;
    public const int IS_SET_CM_RGB24 = 1;
    public const int IS_SET_CM_RGB16 = 2;
    public const int IS_SET_CM_RGB15 = 3;
    public const int IS_SET_CM_Y8 = 6;
    public const int IS_SET_CM_RGB8 = 7;
    public const int IS_SET_CM_BAYER = 11;
    public const int IS_SET_CM_UYVY = 12;
    public const int IS_SET_CM_UYVY_MONO =	13;
    public const int IS_SET_CM_UYVY_BAYER =	14;
    

    // ***********************************************
    // Hotpixel correction
    // ***********************************************
    public const int IS_GET_BPC_MODE = 0x8000;
    public const int IS_GET_BPC_THRESHOLD = 0x8001;

    public const int IS_BPC_DISABLE = 0;
    public const int IS_BPC_ENABLE_LEVEL_1 = 1;
    public const int IS_BPC_ENABLE_LEVEL_2 = 2;
    public const int IS_BPC_ENABLE_USER = 4;
    public const int IS_BPC_ENABLE_SOFTWARE = IS_BPC_ENABLE_LEVEL_2;
    public const int IS_BPC_ENABLE_HARDWARE = IS_BPC_ENABLE_LEVEL_1;

    public const int IS_SET_BADPIXEL_LIST = 0x01;
    public const int IS_GET_BADPIXEL_LIST = 0x02;
    public const int IS_GET_LIST_SIZE = 0x03;


    // ***********************************************
    // color correction definitions
    // ***********************************************
    public const int IS_GET_CCOR_MODE = 0x8000;
    public const int IS_CCOR_DISABLE = 0x0;
    public const int IS_CCOR_ENABLE = 0x1;


    // ***********************************************
    // bayer algorithm modes
    // ***********************************************
    public const int IS_GET_BAYER_CV_MODE = 0x8000;

    public const int IS_SET_BAYER_CV_NORMAL = 0x0000;
    public const int IS_SET_BAYER_CV_BETTER = 0x0001;
    public const int IS_SET_BAYER_CV_BEST = 0x0002;


    // ***********************************************
    // Edge enhancement
    // ***********************************************
    public const int IS_GET_EDGE_ENHANCEMENT = 0x8000;

    public const int IS_EDGE_EN_DISABLE = 0;
    public const int IS_EDGE_EN_STRONG = 1;
    public const int IS_EDGE_EN_WEAK = 2;
    
    
    // ***********************************************
    //  white balance modes
    // ***********************************************
    public const int IS_GET_WB_MODE = 0x8000;
    
    public const int IS_SET_WB_DISABLE = 0x0;
    public const int IS_SET_WB_USER = 0x1;
    public const int IS_SET_WB_AUTO_ENABLE = 0x2;
    public const int IS_SET_WB_AUTO_ENABLE_ONCE = 0x4;
    
    public const int IS_SET_WB_DAYLIGHT_65 = 0x101;
    public const int IS_SET_WB_COOL_WHITE = 0x102;
    public const int IS_SET_WB_U30 = 0x103;
    public const int IS_SET_WB_ILLUMINANT_A = 0x104;
    public const int IS_SET_WB_HORIZON = 0x105;
    

    // ***********************************************
    // flash strobe constants
    // ***********************************************
    public const int IS_GET_FLASHSTROBE_MODE = 0x8000;
    public const int IS_GET_FLASHSTROBE_LINE = 0x8001;
    
    public const int IS_SET_FLASH_OFF = 0;
    public const int IS_SET_FLASH_ON = 1;
    public const int IS_SET_FLASH_LO_ACTIVE = IS_SET_FLASH_ON;
    public const int IS_SET_FLASH_HI_ACTIVE = 2;
    public const int IS_SET_FLASH_HIGH = 3;
    public const int IS_SET_FLASH_LOW = 4;

    public const int IS_GET_FLASH_DELAY = -1;
    public const int IS_GET_FLASH_DURATION = -2;
    public const int IS_GET_MAX_FLASH_DELAY = -3;
    public const int IS_GET_MAX_FLASH_DURATION = -4;
    
    
    // ***********************************************
    // Digital IO constants
    // ***********************************************
    public const int IS_GET_IO = 0x8000;
    public const int IS_GET_IO_MASK = 0x8000;


    // ***********************************************
    // EEPROM defines
    // ***********************************************
    public const int IS_EEPROM_MIN_USER_ADDRESS = 0;
    public const int IS_EEPROM_MAX_USER_ADDRESS = 63;
    public const int IS_EEPROM_MAX_USER_SPACE = 64;


    // ***********************************************
    // error report modes
    // ***********************************************
    public const int IS_GET_ERR_REP_MODE = 0x8000;
    public const int IS_DISABLE_ERR_REP = 0;
    public const int IS_ENABLE_ERR_REP = 1;
    

    // ***********************************************
    // display mode slectors
    // ***********************************************
    public const int IS_GET_DISPLAY_MODE = 0x8000;
    public const int IS_GET_DISPLAY_SIZE_X = 0x8000;
    public const int IS_GET_DISPLAY_SIZE_Y = 0x8001;
    public const int IS_GET_DISPLAY_POS_X = 0x8000;
    public const int IS_GET_DISPLAY_POS_Y = 0x8001;
    
    public const int IS_SET_DM_DIB = 0x1;
    public const int IS_SET_DM_DIRECTDRAW = 0x2;
    public const int IS_SET_DM_ALLOW_SYSMEM = 0x40;
    public const int IS_SET_DM_ALLOW_PRIMARY = 0x80;
    // -- overlay display mode ---
    public const int IS_GET_DD_OVERLAY_SCALE = 0x8000;
    
    public const int IS_SET_DM_ALLOW_OVERLAY = 0x100;
    public const int IS_SET_DM_ALLOW_SCALING = 0x200;
    public const int IS_SET_DM_MONO			 = 0x800;
    public const int IS_SET_DM_BAYER         = 0x1000;
    
    // -- backbuffer display mode ---
    public const int IS_SET_DM_BACKBUFFER    = 0x2000;

    
 
    // ***********************************************
    // DirectDraw keying color constants
    // ***********************************************
    public const int IS_GET_KC_RED = 0x8000;
    public const int IS_GET_KC_GREEN = 0x8001;
    public const int IS_GET_KC_BLUE = 0x8002;
    public const int IS_GET_KC_RGB = 0x8003;
    public const int IS_GET_KC_INDEX = 0x8004;
    
    public const int IS_SET_KC_DEFAULT = 0xFF00FF;
    public const int IS_SET_KC_DEFAULT_8 = 253;


    // ***********************************************
    // memoryboard
    // ***********************************************
    public const int IS_MEMORY_GET_COUNT = 0x8000;
    public const int IS_MEMORY_GET_DELAY = 0x8001;
    public const int IS_MEMORY_MODE_DISABLE = 0x0;
    public const int IS_MEMORY_USE_TRIGGER = 0xFFFF;


    // ***********************************************
    // Testimage modes
    // ***********************************************
    public const int IS_GET_TEST_IMAGE = 0x8000;

    public const int IS_SET_TEST_IMAGE_DISABLED = 0x0000;
    public const int IS_SET_TEST_IMAGE_MEMORY_1 = 0x0001;
    public const int IS_SET_TEST_IMAGE_MEMORY_2 = 0x0002;
    public const int IS_SET_TEST_IMAGE_MEMORY_3 = 0x0003;


    // ***********************************************
    // save options
    // ***********************************************
    public const int IS_SAVE_USE_ACTUAL_IMAGE_SIZE = 0x00010000;


    // ***********************************************
    // event constants
    // ***********************************************
    public const int IS_SET_EVENT_FRAME = 2;
    public const int IS_SET_EVENT_EXTTRIG = 3;
    public const int IS_SET_EVENT_VSYNC = 4;
    public const int IS_SET_EVENT_SEQ = 5;
    public const int IS_SET_EVENT_STEAL = 6;
    public const int IS_SET_EVENT_TRANSFER_FAILED = 8;
    public const int IS_SET_EVENT_DEVICE_RECONNECTED = 9;
    public const int IS_SET_EVENT_MEMORY_MODE_FINISH = 10;
    public const int IS_SET_EVENT_FRAME_RECEIVED = 11;
    public const int IS_SET_EVENT_WB_FINISHED = 12;
    
    public const int IS_SET_EVENT_REMOVE = 128;
    public const int IS_SET_EVENT_REMOVAL = 129;
    public const int IS_SET_EVENT_NEW_DEVICE = 130;


    // ***********************************************
    // Window message defines
    // ***********************************************
    public const int IS_UEYE_MESSAGE = 1280;    //0x7FFF + 0x0100 --> WM_USER=1024+256=1280
    public const int IS_FRAME = 0x0;
    public const int IS_SEQUENCE = 0x1;
    public const int IS_TRIGGER = 0x2;
    public const int IS_TRANSFER_FAILED = 0x3;
    public const int IS_DEVICE_RECONNECTED = 0x4;
    public const int IS_MEMORY_MODE_FINISH = 0x5;
    public const int IS_FRAME_RECEIVED = 0x6;
    public const int IS_GENERIC_ERROR = 0x7;
    public const int IS_STEAL_VIDEO = 0x8;
    public const int  IS_WB_FINISHED = 0x9;
    
    public const int IS_DEVICE_REMOVED = 0x1000;
    public const int IS_DEVICE_REMOVAL = 0x1001;
    public const int IS_NEW_DEVICE = 0x1002;
  
  
    // ***********************************************
    // camera id constants
    // ***********************************************
    public const int IS_GET_CAMERA_ID = 0x8000;
    
    
    // ***********************************************
    // camera info constants
    // ***********************************************
    public const int IS_GET_STATUS = 0x8000;
    
    public const int IS_EXT_TRIGGER_EVENT_CNT = 0;
    public const int IS_FIFO_OVR_CNT = 1;
    public const int IS_SEQUENCE_CNT = 2;
    public const int IS_LAST_FRAME_FIFO_OVR = 3;
    public const int IS_SEQUENCE_SIZE = 4;
    public const int IS_STEAL_FINISHED = 6;
    public const int IS_BOARD_REVISION = 9;
    public const int IS_MIRROR_BITMAP_UPDOWN = 10;
    public const int IS_BUS_OVR_CNT = 11;
    public const int IS_STEAL_ERROR_CNT = 12;


    // ***********************************************
    // board type defines
    // ***********************************************
    public const int IS_BOARD_TYPE_FALCON = 1;
    public const int IS_BOARD_TYPE_EAGLE = 2;
    public const int IS_BOARD_TYPE_FALCON2 = 3;
    public const int IS_BOARD_TYPE_FALCON_PLUS = 7;
    public const int IS_BOARD_TYPE_FALCON_QUATTRO = 9;
    public const int IS_BOARD_TYPE_FALCON_DUO = 10;
    public const int IS_BOARD_TYPE_EAGLE_QUATTRO = 11;
    public const int IS_BOARD_TYPE_EAGLE_DUO = 12;
    public const int IS_BOARD_TYPE_UEYE_USB = 0x40;
    

    // ***********************************************
    // readable operation system defines
    // ***********************************************
    public const int IS_OS_UNDETERMINED = 0;
    public const int IS_OS_WIN95 = 1;
    public const int IS_OS_WINNT40 = 2;
    public const int IS_OS_WIN98 = 3;
    public const int IS_OS_WIN2000 = 4;
    public const int IS_OS_WINXP = 5;
    public const int IS_OS_WINME = 6;
    public const int IS_OS_WINNET = 7;
    public const int IS_OS_WINSERVER2003 = 8;
    

    // ***********************************************
    // usb bus speed
    // ***********************************************
    public const int IS_USB_10 = 1;
    public const int IS_USB_20 = 4;


    // ***********************************************
    // sequence flags
    // ***********************************************
    public const int IS_LOCK_LAST_BUFFER = 0x8002;
        

    // ***********************************************
    // BOARDINFO structure
    // ***********************************************	
    public struct CAMINFO
    {
        public string SerNo;		    //12
        public string id;			    //20
        public string Version;		    //10
        public string Date;			    //12
        public byte Select;
        public byte Type;
        public string Reserverd;        //8
    }


    // ***********************************************
    // SENSORINFO structure
    // ***********************************************
    public struct SENSORINFO
    {
        public int SensorID;
        public string strSensorName;    //32
        public byte nColorMode;	
        public int nMaxWidth;
        public int nMaxHeight;
        public bool bMasterGain;
        public bool bRGain;
        public bool bGGain;
        public bool bBGain;
        public bool bGlobShutter;
        public string reserved;         //16
    }
	
	
    // ***********************************************
    // REVISIONINFO structure
    // ***********************************************
    public struct REVISIONINFO
    {
        public int size;                // 2
        public int Sensor;              // 2
        public int Cypress;             // 2
        public long Blackfin;           // 4
        public int DspFirmware;         // 2
        // --12
        public int USB_Board;           // 2
        public int Sensor_Board;        // 2
        public int Processing_Board;    // 2
        public int Memory_Board;        // 2
        public int Housing;             // 2
        public int Filter;              // 2
        public int Timing_Board;        // 2
        public int Product;             // 2
        // --24
        public string reserved;         // --128
    }

	
    // ***********************************************
    // UEYE_CAMERA_INFO structure
    // ***********************************************
    public struct UEYE_CAMERA_INFO
    {
        public long dwCameraID;         // this is the user defineable camera ID
        public long dwDeviceID;         // this is the systems enumeration ID
        public long dwSensorID;         // this is the sensor ID e.g. IS_SENSOR_UI141X_M
        public long dwInUse;            // flag, whether the camera is in use or not
        public string SerNo;            // serial numer of the camera 16
        public string Model;            // model name of the camera 16
        public string dwReserved;
    }

    // usage of the list:
    // 1. call the DLL with .dwCount = 0
    // 2. DLL returns .dwCount = N  (N = number of available cameras)
    // 3. call DLL with .dwCount = N and a pointer to UEYE_CAMERA_LIST with
    //    and array of UEYE_CAMERA_INFO[N]
    // 4. DLL will fill in the array with the camera infos and
    //    will update the .dwCount member with the actual number of cameras
    //    because there may be a change in number of cameras between step 2 and 3
    // 5. check if there's a difference in actual .dwCount and formerly
    //    reported value of N and call DLL again with an updated array size
    // ***********************************************
    // UEYE_CAMERA_LIST structure
    // ***********************************************
    public struct UEYE_CAMERA_LIST
    {
        public long dwCount;
        public UEYE_CAMERA_INFO uci;
    }


    // ***********************************************
    // auto feature structs and definitions
    // ***********************************************
    public const int AC_SHUTTER = 0x00000001;
    public const int AC_GAIN = 0x00000002;
    public const int AC_WHITEBAL = 0x00000004;
    public const int AC_WB_RED_CHANNEL   = 0x00000008;
    public const int AC_WB_GREEN_CHANNEL = 0x00000010;
    public const int AC_WB_BLUE_CHANNEL  = 0x00000020;

    public const int ACS_ADJUSTING = 0x00000001;
    public const int ACS_FINISHED = 0x00000002;
    public const int ACS_DISABLED = 0x00000004;


    // ***********************************************
    // AUTO_BRIGHT_STATUS structure
    // ***********************************************
    public struct AUTO_BRIGHT_STATUS
    {
        public long curValue;             // current average greylevel
        public long curError;             // current auto brightness error
        public long curController;        // current active brightness controller -> AC_x
        public long curCtrlStatus;        // current control status -> ACS_x
    }


    // ***********************************************
    // AUTO_WB_STATUS structure
    // ***********************************************
    public struct AUTO_WB_CHANNEL_STATUS
    {
        public long curValue;             // current average greylevel
        public long curError;             // current auto wb error
        public long curCtrlStatus;        // current control status -> ACS_x
    }

    // ***********************************************
    // AUTO_WB_STATUS structure
    // ***********************************************
    public struct AUTO_WB_STATUS
    {
        public AUTO_WB_CHANNEL_STATUS RedChannel;
        public AUTO_WB_CHANNEL_STATUS GreenChannel;
        public AUTO_WB_CHANNEL_STATUS BlueChannel;
        public long curController;        // current active wb controller -> AC_x
    }


    // ***********************************************
    // UEYE_AUTO_INFO structure
    // ***********************************************
    public struct UEYE_AUTO_INFO
    {
        public long AutoAbility;                        // autocontrol ability
        public AUTO_BRIGHT_STATUS  sBrightCtrlStatus;   // brightness autocontrol status
        public AUTO_WB_STATUS  sWBCtrlStatus;           // white balance autocontrol status
        public string reserved;
    }
};
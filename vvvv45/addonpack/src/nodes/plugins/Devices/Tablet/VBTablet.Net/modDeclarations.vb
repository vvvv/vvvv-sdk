Option Strict On
Option Explicit On
Module modDeclarations
	'WinTab function constants and declarations
	'Derived (a lot :-) from a work provided by Jeremy Highley of Eclipse, Inc.
	
	'-------------------------------------------------
	'*********** Public API Declarations *************
	'-------------------------------------------------
	
    'Public Declare Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hWnd As IntPtr, ByVal nIndex As Integer) As WndProcDelegate
    'Public Declare Function SetWindowLong Lib "user32" Alias "SetWindowLongA" (ByVal hWnd As IntPtr, ByVal nIndex As Integer, ByVal dwNewLong As WndProcDelegate) As WndProcDelegate
    'Public Declare Function CallWindowProc Lib "user32" Alias "CallWindowProcA" (ByVal lpPrevWndFunc As WndProcDelegate, ByVal hWnd As Integer, ByVal msg As Integer, ByVal wparam As Integer, ByVal lparam As Integer) As Integer
	Public Declare Function SetForegroundWindow Lib "user32" (ByVal hWnd As Integer) As Integer
	Public Declare Function GetLastError Lib "kernel32" () As Integer

    Public Declare Sub CopyMemory Lib "kernel32" Alias "RtlMoveMemory" (ByRef lpDest As IntPtr, ByRef lpSource As IntPtr, ByVal bCopy As Integer)
	Public Declare Sub OutputDebugString Lib "kernel32"  Alias "OutputDebugStringA"(ByVal DebugString As String)
	'Public Declare Sub Sleep Lib "kernel32" (ByVal dwMiliseconds As Long)
	'-------------------------------------------------
	'*********** WinTab Type Declarations *************
	'-------------------------------------------------
	
	Public Const LCNAMELEN As Short = 40
	
	'LCS/Telegraphics documenation on the wintab32.dll driver claim
	'some of these veriable are UINT and int which should carry over as
	'integers.  But these are machine specific, so they actually end up
	'being 4 bytes wide on Intel machines (win95/98/NT).  Thus UINT and
	'int come over as longs in VB. (as does just about everything else)

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Explicit, Charset:=Runtime.InteropServices.CharSet.Ansi)> _
 Public Structure LOGCONTEXT
        '<Runtime.InteropServices.FieldOffset(0), Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=40)> Dim lcName() As Byte
        <Runtime.InteropServices.FieldOffset(0), Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst:=40)> Dim lcName As String
        'the next line is commented because VB 6 is UNICODE, so the
        'fixed length string of 40 characters is 80 bytes long.  This
        'screws up the whole structure, so I just forced the name
        'length of 40 total bytes using the byte array above.
        'lcName As String * LCNAMELEN
        <Runtime.InteropServices.FieldOffset(40)> Dim lcOptions As Integer
        <Runtime.InteropServices.FieldOffset(44)> Dim lcStatus As Integer
        <Runtime.InteropServices.FieldOffset(48)> Dim lcLocks As Integer
        <Runtime.InteropServices.FieldOffset(52)> Dim lcMsgBase As Integer
        <Runtime.InteropServices.FieldOffset(56)> Dim lcDevice As Integer
        <Runtime.InteropServices.FieldOffset(60)> Dim lcPktRate As Integer
        <Runtime.InteropServices.FieldOffset(64)> Dim lcPktData As Integer
        <Runtime.InteropServices.FieldOffset(68)> Dim lcPktMode As Integer
        <Runtime.InteropServices.FieldOffset(72)> Dim lcMoveMask As Integer
        <Runtime.InteropServices.FieldOffset(76)> Dim lcBtnDnMask As Integer
        <Runtime.InteropServices.FieldOffset(80)> Dim lcBtnUpMask As Integer
        <Runtime.InteropServices.FieldOffset(84)> Dim lcInOrgX As Integer
        <Runtime.InteropServices.FieldOffset(88)> Dim lcInOrgY As Integer
        <Runtime.InteropServices.FieldOffset(92)> Dim lcInOrgZ As Integer
        <Runtime.InteropServices.FieldOffset(96)> Dim lcInExtX As Integer
        <Runtime.InteropServices.FieldOffset(100)> Dim lcInExtY As Integer
        <Runtime.InteropServices.FieldOffset(104)> Dim lcInExtZ As Integer
        <Runtime.InteropServices.FieldOffset(108)> Dim lcOutOrgX As Integer
        <Runtime.InteropServices.FieldOffset(112)> Dim lcOutOrgY As Integer
        <Runtime.InteropServices.FieldOffset(116)> Dim lcOutOrgZ As Integer
        <Runtime.InteropServices.FieldOffset(120)> Dim lcOutExtX As Integer
        <Runtime.InteropServices.FieldOffset(124)> Dim lcOutExtY As Integer
        <Runtime.InteropServices.FieldOffset(128)> Dim lcOutExtZ As Integer
        <Runtime.InteropServices.FieldOffset(132)> Dim lcSensX As Integer
        <Runtime.InteropServices.FieldOffset(136)> Dim lcSensY As Integer
        <Runtime.InteropServices.FieldOffset(140)> Dim lcSensZ As Integer
        <Runtime.InteropServices.FieldOffset(144)> Dim lcSysMode As Integer
        <Runtime.InteropServices.FieldOffset(148)> Dim lcSysOrgX As Integer
        <Runtime.InteropServices.FieldOffset(152)> Dim lcSysOrgY As Integer
        <Runtime.InteropServices.FieldOffset(156)> Dim lcSysExtX As Integer
        <Runtime.InteropServices.FieldOffset(160)> Dim lcSysExtY As Integer
        <Runtime.InteropServices.FieldOffset(164)> Dim lcSysSensX As Integer
        <Runtime.InteropServices.FieldOffset(168)> Dim lcSysSensY As Integer

        'UPGRADE_TODO: "Initialize" must be called to initialize instances of this structure. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1026"'
        Public Sub Initialize()
            'ReDim lcName(LCNAMELEN - 1)
        End Sub
    End Structure

    'Public Structure hMgr 'Manager Handle
    '	Dim handle As Integer
    'End Structure

    'Public Structure hCtx 'Context Handle
    '	Dim handle As Integer
    'End Structure

    'Public Structure HWTHook 'Hook Handle
    '	Dim handle As Integer
    'End Structure

    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Explicit)> _
    Public Structure tagAxis
        <Runtime.InteropServices.FieldOffset(0)> Dim Min As Integer
        <Runtime.InteropServices.FieldOffset(4)> Dim Max As Integer
        <Runtime.InteropServices.FieldOffset(8)> Dim Units As Integer
        <Runtime.InteropServices.FieldOffset(12)> Dim Resolution As FIX32
    End Structure

    Public Structure tagOrientation
        Dim orAzimuth As Integer
        Dim orAltitude As Integer
        Dim orTwist As Integer
    End Structure

    Public Structure tagRotation
        Dim roPitch As Integer
        Dim roRoll As Integer
        Dim roYaw As Integer
    End Structure

    Public Structure tagPacket
        Dim pkStatus As Integer
        Dim pkTime As Integer
        Dim pkSerial As Integer
        Dim pkCursor As Integer
        Dim pkButtons As Integer
        Dim pkX As Integer
        Dim pkY As Integer
        Dim pkZ As Integer
        Dim pkNormalPressure As Integer
        Dim pkTangentPressure As Integer
        Dim pkAzimuth As Integer
        Dim pkAltitude As Integer
        Dim pkTwist As Integer
        Dim pkPitch As Integer
        Dim pkRoll As Integer
        Dim pkYaw As Integer
        <System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=4)> Public pkExpKeys As Short()
        <System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst:=1020)> Public pkExtension As Byte()
    End Structure

    ' two bytes in a short
    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Explicit)> _
    Public Structure PACKEDBYTES
        <Runtime.InteropServices.FieldOffset(0)> Dim All As Short
        <Runtime.InteropServices.FieldOffset(0)> Dim Low As Byte
        <Runtime.InteropServices.FieldOffset(1)> Dim High As Byte
    End Structure

    ' Two shorts in an int
    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Explicit)> _
    Public Structure FIX32
        <Runtime.InteropServices.FieldOffset(0)> Dim All As Integer
        <Runtime.InteropServices.FieldOffset(0)> Dim Low As Short
        <Runtime.InteropServices.FieldOffset(2)> Dim High As Short
    End Structure

    '-----------------------------------------------------------
    '*********** WinTab Constant Declarations *************
    '-----------------------------------------------------------

    'Message constants
    Public Const WT_DEFBASE As Short = &H7FF0S '#define WT_DEFBASE      0x7FF0
    Public Const WT_MAXOFFSET As Short = &HFS '#define WT_MAXOFFSET    0xF

    Public Const WT_PACKET As Integer = WT_DEFBASE + 0
    Public Const WT_CTXOPEN As Integer = WT_DEFBASE + 1
    Public Const WT_CTXCLOSE As Integer = WT_DEFBASE + 2
    Public Const WT_CTXUPDATE As Integer = WT_DEFBASE + 3
    Public Const WT_CTXOVERLAP As Integer = WT_DEFBASE + 4
    Public Const WT_PROXIMITY As Integer = WT_DEFBASE + 5
    Public Const WT_INFOCHANGE As Integer = WT_DEFBASE + 6
    Public Const WT_CSRCHANGE As Integer = WT_DEFBASE + 7
    Public Const WT_MAX As Integer = WT_DEFBASE + WT_MAXOFFSET

    'Packet constants
    Public Const PK_CONTEXT As Short = &H1S '/* reporting context */
    Public Const PK_STATUS As Short = &H2S '/* status bits */
    Public Const PK_TIME As Short = &H4S '/* time stamp */
    Public Const PK_CHANGED As Short = &H8S '/* change bit vector */
    Public Const PK_SERIAL_NUMBER As Short = &H10S '/* packet serial number */
    Public Const PK_CURSOR As Short = &H20S '/* reporting cursor */
    Public Const PK_BUTTONS As Short = &H40S '/* button information */
    Public Const PK_X As Short = &H80S '/* x axis */
    Public Const PK_Y As Short = &H100S '/* y axis */
    Public Const PK_Z As Short = &H200S '/* z axis */
    Public Const PK_NORMAL_PRESSURE As Short = &H400S '/* normal or tip pressure */
    Public Const PK_TANGENT_PRESSURE As Short = &H800S '/* tangential or barrel pressure */
    Public Const PK_ORIENTATION As Short = &H1000S '/* orientation info: tilts */
    Public Const PK_ROTATION As Short = &H2000S
    'Public Const PACKETDATA = PK_X Or PK_Y Or PK_Z Or PK_NORMAL_PRESSURE Or PK_BUTTONS 'Use this for pressure tablet digitizers
    'Public Const PACKETDATA = PK_X Or PK_Y Or PK_Z Or PK_BUTTONS 'Use this for 3D digitizers
    'Public Const PACKETDATA = PK_X Or PK_Y Or PK_BUTTONS          'Use this for 2D digitizers
    'Public Const PACKETMODE = PK_BUTTONS   'This means button values are reported "relative" to the last time they were reported
    'Public Const PACKETMODE = 0             'This means all values are reported "absoulte"

    'PACKET DEFINITION
    'The following definition controls what data items are requested from the Tablet during the
    '"WTOpen" command. Note that while some tablets will open with all data items requested
    '(i.e. X, Y, Z, and Pressure information), some tablets will not open if they do not support
    'a particular data item. For example, the GTCO Sketchmaster driver will fail on "WTOpen" if
    'you request Z data or Pressure data. However, the SummaSketch driver will succeed on open
    'even though Z and Pressure are not supported by this tablet. In this case, 0 is returned for
    'the Z and Pressure data, as you might expect.
    '
    '(VBTablet attempts to open as requested, but will fallback to X, Y, Pressure if WTOpen fails - LP)
    'unit specifiers
    Public Const TU_NONE As Short = 0
    Public Const TU_INCHES As Short = 1
    Public Const TU_CENTIMETRES As Short = 2
    Public Const TU_CIRCLE As Short = 3

    'System Button Assignment Values
    Public Const SBN_NONE As Short = &H0S
    Public Const SBN_LCLICK As Short = &H1S
    Public Const SBN_LDBLCLICK As Short = &H2S
    Public Const SBN_LDRAG As Short = &H3S
    Public Const SBN_RCLICK As Short = &H4S
    Public Const SBN_RDBLCLICK As Short = &H5S
    Public Const SBN_RDRAG As Short = &H6S
    Public Const SBN_MCLICK As Short = &H7S
    Public Const SBN_MDBLCLICK As Short = &H8S
    Public Const SBN_MDRAG As Short = &H9S

    'Pen Windows Assignments
    Public Const SBN_PTCLICK As Short = &H10S
    Public Const SBN_PTDBLCLICK As Short = &H20S
    Public Const SBN_PTDRAG As Short = &H30S
    Public Const SBN_PNCLICK As Short = &H40S
    Public Const SBN_PNDBLCLICK As Short = &H50S
    Public Const SBN_PNDRAG As Short = &H60S
    Public Const SBN_P1CLICK As Short = &H70S
    Public Const SBN_P1DBLCLICK As Short = &H80S
    Public Const SBN_P1DRAG As Short = &H90S
    Public Const SBN_P2CLICK As Short = &HA0S
    Public Const SBN_P2DBLCLICK As Short = &HB0S
    Public Const SBN_P2DRAG As Short = &HC0S
    Public Const SBN_P3CLICK As Short = &HD0S
    Public Const SBN_P3DBLCLICK As Short = &HE0S
    Public Const SBN_P3DRAG As Short = &HF0S

    'Hardware Capabilities
    Public Const HWC_INTEGRATED As Short = &H1S
    Public Const HWC_TOUCH As Short = &H2S
    Public Const HWC_HARDPROX As Short = &H4S
    Public Const HWC_PHYSID_CURSORS As Short = &H8S

    'Cursor Capabilities
    Public Const CRC_MULTIMODE As Short = &H1S
    Public Const CRC_AGGREGATE As Short = &H2S
    Public Const CRC_INVERT As Short = &H4S

    'Info Categories
    Public Const WTI_INTERFACE As Short = 1
    Public Const WTI_STATUS As Short = 2
    Public Const WTI_DEFCONTEXT As Short = 3
    Public Const WTI_DEFSYSCTX As Short = 4
    Public Const WTI_DEVICES As Short = 100
    Public Const WTI_CURSORS As Short = 200
    Public Const WTI_EXTENSIONS As Short = 300
    Public Const WTI_DDCTXS As Short = 400
    Public Const WTI_DSCTXS As Short = 500

    'Info Sub Catagories
    'WTI_INTERFACE
    Public Const IFC_WINTABID As Short = 1
    Public Const IFC_SPECVERSION As Short = 2
    Public Const IFC_IMPLVERSION As Short = 3
    Public Const IFC_NDEVICES As Short = 4
    Public Const IFC_NCURSORS As Short = 5
    Public Const IFC_NCONTEXTS As Short = 6
    Public Const IFC_CTXOPTIONS As Short = 7
    Public Const IFC_CTXSAVESIZE As Short = 8
    Public Const IFC_NEXTENSIONS As Short = 9
    Public Const IFC_NMANAGERS As Short = 10
    Public Const IFC_MAX As Short = 10

    'WTI_STATUS
    Public Const STA_CONTEXTS As Short = 1
    Public Const STA_SYSCTXS As Short = 2
    Public Const STA_PKTRATE As Short = 3
    Public Const STA_PKTDATA As Short = 4
    Public Const STA_MANAGERS As Short = 5
    Public Const STA_SYSTEM As Short = 6
    Public Const STA_BUTTONUSE As Short = 7
    Public Const STA_SYSBTNUSE As Short = 8
    Public Const STA_MAX As Short = 8

    'WTI_DEFCONTEXT
    Public Const CTX_NAME As Short = 1
    Public Const CTX_OPTIONS As Short = 2
    Public Const CTX_STATUS As Short = 3
    Public Const CTX_LOCKS As Short = 4
    Public Const CTX_MSGBASE As Short = 5
    Public Const CTX_DEVICE As Short = 6
    Public Const CTX_PKTRATE As Short = 7
    Public Const CTX_PKTDATA As Short = 8
    Public Const CTX_PKTMODE As Short = 9
    Public Const CTX_MOVEMASK As Short = 10
    Public Const CTX_BTNDNMASK As Short = 11
    Public Const CTX_BTNUPMASK As Short = 12
    Public Const CTX_INORGX As Short = 13
    Public Const CTX_INORGY As Short = 14
    Public Const CTX_INORGZ As Short = 15
    Public Const CTX_INEXTX As Short = 16
    Public Const CTX_INEXTY As Short = 17
    Public Const CTX_INEXTZ As Short = 18
    Public Const CTX_OUTORGX As Short = 19
    Public Const CTX_OUTORGY As Short = 20
    Public Const CTX_OUTORGZ As Short = 21
    Public Const CTX_OUTEXTX As Short = 22
    Public Const CTX_OUTEXTY As Short = 23
    Public Const CTX_OUTEXTZ As Short = 24
    Public Const CTX_SENSX As Short = 25
    Public Const CTX_SENSY As Short = 26
    Public Const CTX_SENSZ As Short = 27
    Public Const CTX_SYSMODE As Short = 28
    Public Const CTX_SYSORGX As Short = 29
    Public Const CTX_SYSORGY As Short = 30
    Public Const CTX_SYSEXTX As Short = 31
    Public Const CTX_SYSEXTY As Short = 32
    Public Const CTX_SYSSENSX As Short = 33
    Public Const CTX_SYSSENSY As Short = 34
    Public Const CTX_MAX As Short = 34

    'WTI_DEVICES
    Public Const DVC_NAME As Short = 1
    Public Const DVC_HARDWARE As Short = 2
    Public Const DVC_NCSRTYPES As Short = 3
    Public Const DVC_FIRSTCSR As Short = 4
    Public Const DVC_PKTRATE As Short = 5
    Public Const DVC_PKTDATA As Short = 6
    Public Const DVC_PKTMODE As Short = 7
    Public Const DVC_CSRDATA As Short = 8
    Public Const DVC_XMARGIN As Short = 9
    Public Const DVC_YMARGIN As Short = 10
    Public Const DVC_ZMARGIN As Short = 11
    Public Const DVC_X As Short = 12
    Public Const DVC_Y As Short = 13
    Public Const DVC_Z As Short = 14
    Public Const DVC_NPRESSURE As Short = 15
    Public Const DVC_TPRESSURE As Short = 16
    Public Const DVC_ORIENTATION As Short = 17
    Public Const DVC_ROTATION As Short = 18
    Public Const DVC_PNPID As Short = 19
    Public Const DVC_MAX As Short = 19

    'WTI_CURSORS
    Public Const CSR_NAME As Short = 1
    Public Const CSR_ACTIVE As Short = 2
    Public Const CSR_PKTDATA As Short = 3
    Public Const CSR_BUTTONS As Short = 4
    Public Const CSR_BUTTONBITS As Short = 5
    Public Const CSR_BTNNAMES As Short = 6
    Public Const CSR_BUTTONMAP As Short = 7
    Public Const CSR_SYSBTNMAP As Short = 8
    Public Const CSR_NPBUTTON As Short = 9
    Public Const CSR_NPBTNMARKS As Short = 10
    Public Const CSR_NPRESPONSE As Short = 11
    Public Const CSR_TPBUTTON As Short = 12
    Public Const CSR_TPBTNMARKS As Short = 13
    Public Const CSR_TPRESPONSE As Short = 14
    Public Const CSR_PHYSID As Short = 15
    Public Const CSR_MODE As Short = 16
    Public Const CSR_MINPKTDATA As Short = 17
    Public Const CSR_MINBUTTONS As Short = 18
    Public Const CSR_CAPABILITIES As Short = 19
    Public Const CSR_TYPE As Short = 20 'added by Wacom v1.2
    Public Const CSR_MAX As Short = 20

    Public Const CSR_TYPE_TRANSLATIONFACTOR As Short = &HF06S

    Public Const CSR_TYPE_NONE As Short = &H0S
    Public Const CSR_TYPE_STYLUS As Short = &H2S 'possibly graphire stylus
    Public Const CSR_TYPE_MOUSE As Short = &H206S
    Public Const CSR_TYPE_AIRBRUSH As Short = &H902S
    Public Const CSR_TYPE_INTUOSSTYLUS As Short = &H802S 'possibly intuous stylus
    Public Const CSR_TYPE_PUCK As Short = &H4S
    Public Const CSR_TYPE_5PUCK As Short = &H6S

    'WTI_EXTENSIONS
    Public Const EXT_NAME As Short = 1
    Public Const EXT_TAG As Short = 2
    Public Const EXT_MASK As Short = 3
    Public Const EXT_SIZE As Short = 4
    Public Const EXT_AXES As Short = 5
    Public Const EXT_DEFAULT As Short = 6
    Public Const EXT_DEFCONTEXT As Short = 7
    Public Const EXT_DEFSYSCTX As Short = 8
    Public Const EXT_CURSORS As Short = 9
    Public Const EXT_MAX As Short = 109 '/* Allow 100 cursors */

    'context option values
    Public Const CXO_SYSTEM As Short = &H1S
    Public Const CXO_PEN As Short = &H2S
    Public Const CXO_MESSAGES As Short = &H4S
    Public Const CXO_CSRMESSAGES As Short = &H8S
    Public Const CXO_MARGIN As Short = &H8000S
    Public Const CXO_MGNINSIDE As Short = &H4000S

    'context status values
    Public Const CXS_DISABLED As Short = &H1S
    Public Const CXS_OBSCURED As Short = &H2S
    Public Const CXS_ONTOP As Short = &H4S

    'context lock values
    Public Const CXL_INSIZE As Short = &H1S
    Public Const CXL_INASPECT As Short = &H2S
    Public Const CXL_SENSITIVITY As Short = &H4S
    Public Const CXL_MARGIN As Short = &H8S
    Public Const CXL_SYSOUT As Short = &H10S

    'Packet Status Values
    Public Const TPS_PROXIMITY As Short = &H1S
    Public Const TPS_QUEUE_ERR As Short = &H2S
    Public Const TPS_MARGIN As Short = &H4S
    Public Const TPS_GRAB As Short = &H8S
    Public Const TPS_INVERT As Short = &H10S

    'relative buttons
    Public Const TBN_NONE As Short = 0
    Public Const TBN_UP As Short = 1
    Public Const TBN_DOWN As Short = 2


    'device configuration constants
    Public Const WTDC_NONE As Short = 0
    Public Const WTDC_CANCEL As Short = 1
    Public Const WTDC_OK As Short = 2
    Public Const WTDC_RESTART As Short = 3

    'hook constants
    Public Const WTH_PLAYBACK As Short = 1
    Public Const WTH_RECORD As Short = 2
    Public Const WTHC_GETLPLPFN As Short = (-3)
    Public Const WTHC_LPLPFNNEXT As Short = (-2)
    Public Const WTHC_LPFNNEXT As Short = (-1)
    Public Const WTHC_ACTION As Short = 0
    Public Const WTHC_GETNEXT As Short = 1
    Public Const WTHC_SKIP As Short = 2

    'defaults constants
    Public Const WTP_LPDEFAULT As Short = (-1)
    Public Const WTP_LPDEFAULTBYTE As Byte = 255
    Public Const WTP_DWDEFAULT As Short = (-1)
    '-------------------------------------------------
    '*********** WinTab API Declarations *************
    '-------------------------------------------------

    'Aliased Functions (Can change these to unicode by changing the A to W in Alias name)

    'Used to read various pieces of information about the tablet.
    Public Declare Ansi Function WTInfo Lib "wintab32.dll" Alias "WTInfoA" (ByVal wCategory As Integer, ByVal nIndex As Integer, ByRef lpOutput As Integer) As Integer
    Public Declare Ansi Function WTInfo Lib "wintab32.dll" Alias "WTInfoA" (ByVal wCategory As Integer, ByVal nIndex As Integer, ByRef lpOutput As PACKEDBYTES) As Integer
    Public Declare Ansi Function WTInfo Lib "wintab32.dll" Alias "WTInfoA" (ByVal wCategory As Integer, ByVal nIndex As Integer, ByRef lpOutput As LOGCONTEXT) As Integer
    Public Declare Ansi Function WTInfo Lib "wintab32.dll" Alias "WTInfoA" (ByVal wCategory As Integer, ByVal nIndex As Integer, <System.Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPArray, Sizeconst:=3)> ByVal lpOutput As tagAxis()) As Integer
    Public Declare Ansi Function WTInfo Lib "wintab32.dll" Alias "WTInfoA" (ByVal wCategory As Integer, ByVal nIndex As Integer, ByRef lpOutput As tagAxis) As Integer
    Public Declare Ansi Function WTInfo Lib "wintab32.dll" Alias "WTInfoA" (ByVal wCategory As Integer, ByVal nIndex As Integer, ByVal lpOutput As String) As Integer
    Public Declare Ansi Function WTInfo Lib "wintab32.dll" Alias "WTInfoA" (ByVal wCategory As Integer, ByVal nIndex As Integer, ByRef lpOutput As Byte()) As Integer

    'Public Declare Function WTOpen Lib "wintab32.dll" Alias "WTOpenA" (ByVal hWnd As Long, LPLOGCONTEXT As LOGCONTEXT, ByVal fEnable As Integer) As Long 'Should be As HCTX
    'Used to begin accessing the Tablet.
    Public Declare Function WTOpen Lib "wintab32.dll" Alias "WTOpenA" (ByVal hWnd As IntPtr, ByRef LPLOGCONTEXT As LOGCONTEXT, ByVal fEnable As Boolean) As IntPtr


    'UPGRADE_WARNING: Structure LOGCONTEXT may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1050"'
    'Fills the supplied structure with the current context attributes for the passed handle.
    Public Declare Function WTGet Lib "wintab32.dll" Alias "WTGetA" (ByVal hCtx As IntPtr, ByRef LPLOGCONTEXT As LOGCONTEXT) As Integer 'Should be As BOOL


    'UPGRADE_WARNING: Structure LOGCONTEXT may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1050"'
    'Allows some of the context's attributes to be changed on the fly.
    Public Declare Function WTSet Lib "wintab32.dll" Alias "WTSetA" (ByVal hCtx As IntPtr, ByRef LPLOGCONTEXT As LOGCONTEXT) As Integer 'Should be As BOOL
    Public Declare Function WTSetLong Lib "wintab32.dll" Alias "WTSetA" (ByVal hCtx As IntPtr, ByVal LPLOGCONTEXT As Integer) As Integer 'Should be As BOOL


    'Basic Functions
    Public Declare Function WTClose Lib "wintab32.dll" (ByVal hCtx As IntPtr) As Integer 'Should be As BOOL
    'Used to end accessing the Tablet.

    Public Declare Function WTPacketsGet Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal cMaxPackets As Integer, ByRef lpPkts As tagPacket) As Integer
    'Used to poll the Tablet for input.

    Public Declare Function WTPacket Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal wSerial As Integer, ByRef lpPkts As tagPacket) As Integer 'Should be As BOOL
    'Similar to WTPacketsGet but is used in a window function.

    'Visibility Functions
    Public Declare Function WTEnable Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal fEnable As Boolean) As Integer
    'Enables and Disables a Tablet Context, temporarily turning on or off the processing of packets.

    Public Declare Function WTOverlap Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal fToTop As Boolean) As Boolean
    'Sends a tablet context to the top or bottom of the order of overlapping tablet contexts.

    'Context Editing Functions
    Public Declare Function WTConfig Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal hWnd As IntPtr) As Integer
    'Used to call a requestor which aids in configuring the Tablet

    ' Ext data can actually be anything
    Public Declare Function WTExtGet Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal wExt As Integer, ByRef lpData As Byte()) As Integer
    Public Declare Function WTExtSet Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal wExt As Integer, ByRef lpData As Byte()) As Integer

    Public Declare Function WTSave Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal lpSaveInfo As Byte()) As Integer
    'Fills the supplied buffer with binary save information that can be used to restore the equivalent context in a subsequent Windows session.

    Public Declare Function WTRestore Lib "wintab32.dll" (ByVal hWnd As IntPtr, ByVal lpSaveInfo As Byte(), ByVal fEnable As Boolean) As Integer
    'Creates a tablet context from the save information returned from the WTSave function.

    'Advanced Packet and Queue Functions
    Public Declare Function WTPacketsPeek Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal cMaxPackets As Integer, ByVal lpPkts As IntPtr) As Integer

    Public Declare Function WTDataGet Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal wBegin As Integer, ByVal wEnding As Integer, ByVal cMaxPackets As Integer, ByVal lpPkts As IntPtr, ByRef lpNPkts As Long) As Integer
    Public Declare Function WTDataPeek Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal wBegin As Integer, ByVal wEnding As Integer, ByVal cMaxPackets As Integer, ByVal lpPkts As IntPtr, ByRef lpNPkts As Long) As Integer 'can't use wEnd becau
    Public Declare Function WTQueuePacketsEx Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByRef lpOld As Integer, ByRef lpNew As Integer) As Integer 'Should be As BOOL
    'Returns the serial numbers of the oldest and newest packets currently in the queue.

    Public Declare Function WTQueueSizeGet Lib "wintab32.dll" (ByVal hCtx As IntPtr) As Integer
    Public Declare Function WTQueueSizeSet Lib "wintab32.dll" (ByVal hCtx As IntPtr, ByVal nPkts As Integer) As Integer

    'Manager Functions

    'Manager Handle Functions
    Public Declare Function WTMgrOpen Lib "wintab32.dll" (ByVal hWnd As IntPtr, ByVal wMsgBase As Integer) As IntPtr
    Public Declare Function WTMgrClose Lib "wintab32.dll" (ByVal hMgr As IntPtr) As Boolean

    'Manager Context Functions
    Public Declare Function WTMgrContextEnum Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal lpEnumFunc As EnumContextsDelegate, ByRef lparam As Integer) As Integer
    Public Declare Function WTMgrContextOwner Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal hCtx As IntPtr) As IntPtr
    Public Declare Function WTMgrDefContext Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal fSystem As Boolean) As IntPtr
    Public Declare Function WTMgrDefContextEx Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wDevice As Integer, ByVal fSystem As Boolean) As IntPtr

    'Manager Configuration Functions
    Public Declare Function WTMgrDeviceConfig Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByRef wDevice As Integer, ByVal hWnd As IntPtr) As Integer
    Public Declare Auto Function WTMgrConfigReplaceEx Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal fInstall As Integer, ByVal lpszModule As String, ByVal lpszCfgProc As String) As Integer

    'Manager Packet Hook Functions
    Public Declare Auto Function WTMgrPacketHookEx Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal nType As Integer, ByVal lpszModule As String, ByVal lpszHookProc As String) As IntPtr
    Public Declare Function WTMgrPacketUnHook Lib "wintab32.dll" (ByVal hHook As IntPtr) As Integer
    Public Declare Function WTMgrPacketHookNext Lib "wintab32.dll" (ByVal hHook As IntPtr, ByVal nCode As Integer, ByVal wparam As Integer, ByRef lparam As Integer) As Integer

    'Manager Preference Data Functions
    Public Declare Function WTMgrExt Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wExt As Integer, ByRef lpData As Integer) As Integer
    Public Declare Function WTMgrCsrEnable Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByVal fEnable As Boolean) As Integer
    ' byte arrays are 32 bytes in length
    Public Declare Function WTMgrCsrButtonMap Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpLogBtns As Byte(), ByRef lpSysBtns As Byte()) As Integer
    Public Declare Function WTMgrCsrButtonMap Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpLogBtns As Byte(), ByRef lpSysBtns As Byte) As Integer
    Public Declare Function WTMgrCsrButtonMap Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpLogBtns As Byte, ByRef lpSysBtns As Byte()) As Integer
    Public Declare Function WTMgrCsrButtonMap Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpLogBtns As Byte, ByRef lpSysBtns As Byte) As Integer
    ' Arrays are two elements each, release mark and press mark, WTP_LPDEFAULT used to reset default
    Public Declare Function WTMgrCsrPressureBtnMarksEx Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpNMarks As Integer(), ByRef lpTMarks As Integer()) As Integer
    Public Declare Function WTMgrCsrPressureBtnMarksEx Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpNMarks As Integer, ByRef lpTMarks As Integer) As Integer

    ' Pressure response curves are an array of UINT values.
    ' Array bounds are 0 to min(255, num physical values)
    ' Values are stretched over these values. Again, WTP_LPDEFAULT resets.
    Public Declare Function WTMgrCsrPressureResponse Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpNResp As Integer(), ByRef lpTResp As Integer()) As Integer
    Public Declare Function WTMgrCsrPressureResponse Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByRef lpNResp As Integer, ByRef lpTResp As Integer) As Integer
    Public Declare Function WTMgrCsrExt Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByVal wExt As Integer, ByRef lpData As IntPtr) As Integer
    Public Declare Function WTMgrCsrExt Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByVal wExt As Integer, ByRef lpData As Integer) As Integer
    Public Declare Function WTMgrCsrExt Lib "wintab32.dll" (ByVal hMgr As IntPtr, ByVal wCursor As Integer, ByVal wExt As Integer, ByRef lpData As Byte) As Integer
End Module
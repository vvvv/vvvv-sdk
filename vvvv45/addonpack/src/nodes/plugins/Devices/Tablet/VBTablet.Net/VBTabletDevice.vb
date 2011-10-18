Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletDevice_NET.TabletDevice")> Public Class TabletDevice
	'"Images of broken light which dance before me, like a million eyes - they call me, on and on, across the universe . . ." - The Beatles - Across the Universe
	'VBTablet TabletDevice class - ©2001 Laurence Parry
	'Information about the various tablet devices
	
	Public Margins As TabletDeviceMargins
	Public X As TabletDeviceAxis
	Public Y As TabletDeviceAxis
	Public Z As TabletDeviceAxis
	Public NormalPressure As TabletDeviceAxis 'normal (usually tip) pressure
	Public TangentPressure As TabletDeviceAxis 'tangential/barrel pressure
	Public Azimuth As TabletDeviceAxis 'clockwise rotation about Z-axis
	Public Altitude As TabletDeviceAxis 'angle with x-y plane (positive is towards +z)
	Public Twist As TabletDeviceAxis 'clockwise rotation about major axis
	Public Pitch As TabletDeviceAxis 'pitch of cursor
	Public Roll As TabletDeviceAxis 'roll of cursor
	Public Yaw As TabletDeviceAxis 'yaw of cursor
	Public AlwaysAvailable As TabletDeviceMask 'data items always available
	Public AlwaysRelative As TabletDeviceMask 'data items always reported relative
	Public CursorDependant As TabletDeviceMask 'data items dependent on selected cursor
	Private devindex As Integer '0-based internally, 1-based externally
	
    Public Sub New()
        MyBase.New()
        Margins = New TabletDeviceMargins
        X = New TabletDeviceAxis
        Y = New TabletDeviceAxis
        Z = New TabletDeviceAxis
        NormalPressure = New TabletDeviceAxis
        TangentPressure = New TabletDeviceAxis
        Azimuth = New TabletDeviceAxis
        Altitude = New TabletDeviceAxis
        Twist = New TabletDeviceAxis
        Pitch = New TabletDeviceAxis
        Roll = New TabletDeviceAxis
        Yaw = New TabletDeviceAxis
        AlwaysAvailable = New TabletDeviceMask
        AlwaysRelative = New TabletDeviceMask
        CursorDependant = New TabletDeviceMask
        X.item = DVC_X
        Y.item = DVC_Y
        Z.item = DVC_Z
        NormalPressure.item = DVC_NPRESSURE
        TangentPressure.item = DVC_TPRESSURE
        Azimuth.item = DVC_ORIENTATION
        Altitude.item = DVC_ORIENTATION
        Twist.item = DVC_ORIENTATION
        Altitude.Index = 1
        Twist.Index = 2
        Pitch.item = DVC_ROTATION
        Roll.item = DVC_ROTATION
        Yaw.item = DVC_ROTATION
        Roll.Index = 1
        Yaw.Index = 2
        Me.Index = 1
    End Sub

    'Set the index (1-based)

    'Get the index (not sure if this is necessary . . .)
    Public Property Index() As Integer
        Get
            Index = devindex - WTI_DEVICES + 1
        End Get
        Set(ByVal Value As Integer)
            tmpl = 0
            WTInfo(WTI_INTERFACE, IFC_NDEVICES, tmpl)
            If tmpl < Value Or Value < 1 Then
                If tmpl = 0 Then
                    tabError(19910, "Invalid device number (" & Value & ") - there are no tablet devices connected at this time")
                Else
                    tabError(19910, "Invalid device number (" & Value & ") - use Tablet.Interface.NumDevices to retrieve the number of devices available")
                End If
                Exit Property
            Else
                devindex = Value - 1 + WTI_DEVICES
            End If
            WTInfo(devindex, DVC_PKTDATA, tmpl)
            AlwaysAvailable.MaskValue = tmpl
            WTInfo(devindex, DVC_PKTMODE, tmpl)
            AlwaysRelative.MaskValue = tmpl
            WTInfo(devindex, DVC_CSRDATA, tmpl)
            CursorDependant.MaskValue = tmpl
            'set devindex for dependant classes (avoid global vars - evil! ;-)
            Margins.DIndex = devindex
            X.DIndex = devindex
            Y.DIndex = devindex
            Z.DIndex = devindex
            NormalPressure.DIndex = devindex
            TangentPressure.DIndex = devindex
            Azimuth.DIndex = devindex
            Altitude.DIndex = devindex
            Twist.DIndex = devindex
            Pitch.DIndex = devindex
            Roll.DIndex = devindex
            Yaw.DIndex = devindex
        End Set
    End Property

    'Returns the device name for the current device
    Public ReadOnly Property Name() As String
        Get
            If WTInfo(devindex, DVC_NAME, refstr) = 0 Then
                IsSupported(0)
                Return ""
            Else
                Return Left(refstr, tmp - 1)
            End If
        End Get
    End Property

    'Returns true if the device is integrated into the display
    Public ReadOnly Property IsIntegrated() As Boolean
        Get
            IsSupported(WTInfo(devindex, DVC_HARDWARE, tmpl))
            Return CBool(tmpl And HWC_INTEGRATED)
        End Get
    End Property


    'Returns true if the device requires the stylus to touch to report position
    Public ReadOnly Property IsContact() As Boolean
        Get
            IsSupported(WTInfo(devindex, DVC_HARDWARE, tmpl))
            Return CBool(tmpl And HWC_TOUCH)
        End Get
    End Property

    'Returns true if the device can give proximity entry/leaving reports
    Public ReadOnly Property IsProximal() As Boolean
        Get
            IsSupported(WTInfo(devindex, DVC_HARDWARE, tmpl))
            Return CBool(tmpl And HWC_HARDPROX)
        End Get
    End Property

    'Returns true if the device can identify cursors in hardware
    Public ReadOnly Property HasHardwareCursorID() As Boolean
        Get
            IsSupported(WTInfo(devindex, DVC_HARDWARE, tmpl))
            Return CBool(tmpl And HWC_PHYSID_CURSORS)
        End Get
    End Property

    'Returns the number of cursor types
    Public ReadOnly Property NumCursorTypes() As Integer
        Get
            IsSupported(WTInfo(devindex, DVC_NCSRTYPES, NumCursorTypes))
        End Get
    End Property

    'Returns the first cursor type number for this device
    Public ReadOnly Property FirstCursor() As Integer
        Get
            IsSupported(WTInfo(devindex, DVC_FIRSTCSR, FirstCursor))
            FirstCursor = FirstCursor + 1 'Convert to 1-based
        End Get
    End Property

    'Returns the maximal rate of packet updates per second for this device
    Public ReadOnly Property MaxPktRate() As Integer
        Get
            IsSupported(WTInfo(devindex, DVC_PKTRATE, MaxPktRate))
        End Get
    End Property

    'Returns the Plug-and-Play device ID
    Public ReadOnly Property PnPID() As String
        Get
            tmpl = WTInfo(devindex, DVC_PNPID, refstr)
            If tmpl = 0 Then
                PnPID = ""
                IsSupported(0)
            Else
                PnPID = Left(refstr, tmpl - 1)
            End If
        End Get
    End Property
End Class
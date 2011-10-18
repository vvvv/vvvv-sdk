Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletDeviceMask_NET.TabletDeviceMask")> Public Class TabletDeviceMask
	'"To every thing, turn, turn, turn, there is a season, turn, turn, turn - and a time to every purpose, under heaven . . ." - The Byrds - Turn, Turn, Turn
	'VBTablet TabletDeviceMask class - ©2001 Laurence Parry
	'This class represents the bitmask of values returned by a device
	'Pretty much the same as the TabletContextMask except without the Lets
	Private value As Integer 'The bitmask this instance modifies
	
	'Set the mask value. Since this is read-only, friend only. Because
	'this is a Friend/Public pair, you should not add a description of
	'this in Property Attributes (VB bug)
	
	'Gets the mask value. Clients should use the other properties
	'in preference to using this unless there is no alternative
	Public Property MaskValue() As Integer
		Get
			Return value
		End Get
		Set(ByVal Value As Integer)
            Me.value = Value
		End Set
	End Property
	
	'Returns true if the context bit is set
	Public ReadOnly Property Context() As Boolean
		Get
            Return CBool(value And PK_CONTEXT)
		End Get
	End Property
	
	'Returns true if the status bit is set
	Public ReadOnly Property Status() As Boolean
		Get
            Return CBool(value And PK_STATUS)
		End Get
	End Property
	
	'Returns true if the time bit is set
	Public ReadOnly Property Time() As Boolean
		Get
            Return CBool(value And PK_TIME)
		End Get
	End Property
	
	'Returns true if the changed bit is set
	Public ReadOnly Property Changed() As Boolean
		Get
            Return CBool(value And PK_CHANGED)
		End Get
	End Property
	
	'Returns true if the serial number bit is set
	Public ReadOnly Property PacketSerial() As Boolean
		Get
            Return CBool(value And PK_SERIAL_NUMBER)
		End Get
	End Property
	
	'Returns true if the cursor bit is set
	Public ReadOnly Property Cursor() As Boolean
		Get
            Return CBool(value And PK_CURSOR)
		End Get
	End Property
	
	'Returns true if the buttons bit is set
	Public ReadOnly Property Buttons() As Boolean
		Get
            Return CBool(value And PK_BUTTONS)
		End Get
	End Property
	
	'Returns true if the X bit is set
	Public ReadOnly Property X() As Boolean
		Get
            Return CBool(value And PK_X)
		End Get
	End Property
	
	'Returns true if the Y bit is set
	Public ReadOnly Property Y() As Boolean
		Get
            Return CBool(value And PK_Y)
		End Get
	End Property
	
	'Returns true if the Z bit is set
	Public ReadOnly Property Z() As Boolean
		Get
            Return CBool(value And PK_Z)
		End Get
	End Property
	
	'Returns true if the normal pressure bit is set
	Public ReadOnly Property NormalPressure() As Boolean
		Get
            Return CBool(value And PK_NORMAL_PRESSURE)
		End Get
	End Property
	
	'Returns true if the tangential/barrel pressure bit is set
	Public ReadOnly Property TangentPressure() As Boolean
		Get
            Return CBool(value And PK_TANGENT_PRESSURE)
		End Get
	End Property
	
	'Returns true if the orientation bit is set (azimuth, altitude, twist)
	Public ReadOnly Property Orientation() As Boolean
		Get
            Return CBool(value And PK_ORIENTATION)
		End Get
	End Property
	
	'Returns true if the rotation bit is set (pitch, roll, yaw)
	Public ReadOnly Property Rotation() As Boolean
		Get
            Return CBool(value And PK_ROTATION)
		End Get
	End Property
End Class
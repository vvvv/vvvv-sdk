Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletContextMask_NET.TabletContextMask")> Public Class TabletContextMask
	'"Using the wrong algorithm to solve a problem is like trying to cut a steak with a screwdriver - you may eventually get a digestable result, but you will expend considerably more effort than necessary, and the result is unlikely to be aesthetically pleasing." - THC, CEL and RLR, "Introduction to Algorithms"
	'VBTablet TabletContextMask class - ©2001 Laurence Parry
	'This class is the way I set event option bitmasks, what to
	'report in packets, etc. The owning class needs to call SetValue
	'before it is used (probably with a member of a private structure)
	Private value As Integer 'The bitmask this instance modifies
	
	'Sets the mask value. Clients should use the other properties
	'in preference to using this unless there is no alternative
	
	'Gets the mask value
	Public Property MaskValue() As Integer
		Get
			MaskValue = value
		End Get
		Set(ByVal Value As Integer)
            Me.value = Value
		End Set
	End Property
	
	'Returns true if the context bit is set
	
	'Sets the context bit
	Public Property Context() As Boolean
		Get
            Return CBool(value And PK_CONTEXT)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_CONTEXT
			Else
                Me.value = Me.value And Not PK_CONTEXT
			End If
		End Set
	End Property
	
	'Returns true if the status bit is set
	
	'Sets the status bit
	Public Property Status() As Boolean
		Get
            Return CBool(value And PK_STATUS)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_STATUS
			Else
                Me.value = Me.value And Not PK_STATUS
			End If
		End Set
	End Property
	
	'Returns true if the time bit is set
	
	'Sets the time bit
	Public Property Time() As Boolean
		Get
            Return CBool(value And PK_TIME)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_TIME
			Else
                Me.value = Me.value And Not PK_TIME
			End If
		End Set
	End Property
	
	'Returns true if the changed bit is set
	
	'Sets the time bit
	Public Property Changed() As Boolean
		Get
            Return CBool(value And PK_CHANGED)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_CHANGED
			Else
                Me.value = Me.value And Not PK_CHANGED
			End If
		End Set
	End Property
	
	'Returns true if the serial number bit is set
	
	'Sets the serial number bit
	Public Property PacketSerial() As Boolean
		Get
            Return CBool(value And PK_SERIAL_NUMBER)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_SERIAL_NUMBER
			Else
                Me.value = Me.value And Not PK_SERIAL_NUMBER
			End If
		End Set
	End Property
	
	'Returns true if the cursor bit is set
	
	'Sets the cursor bit
	Public Property Cursor() As Boolean
		Get
            Return CBool(value And PK_CURSOR)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_CURSOR
			Else
                Me.value = Me.value And Not PK_CURSOR
			End If
		End Set
	End Property
	
	'Returns true if the buttons bit is set
	
	'Sets the buttons bit
	Public Property Buttons() As Boolean
		Get
            Return CBool(value And PK_BUTTONS)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_BUTTONS
			Else
                Me.value = Me.value And Not PK_BUTTONS
			End If
		End Set
	End Property
	
	'Returns true if the X bit is set
	
	'Sets the x bit
	Public Property X() As Boolean
		Get
            Return CBool(value And PK_X)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_X
			Else
                Me.value = Me.value And Not PK_X
			End If
		End Set
	End Property
	
	'Returns true if the Y bit is set
	
	'Sets the y bit
	Public Property Y() As Boolean
		Get
            Return CBool(value And PK_Y)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_Y
			Else
                Me.value = Me.value And Not PK_Y
			End If
		End Set
	End Property
	
	'Returns true if the Z bit is set
	
	'Sets the z bit
	Public Property Z() As Boolean
		Get
            Return CBool(value And PK_Z)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_Z
			Else
                Me.value = Me.value And Not PK_Z
			End If
		End Set
	End Property
	
	'Returns true if the normal pressure bit is set
	
	'Sets the normal pressure bit
	Public Property NormalPressure() As Boolean
		Get
            Return CBool(value And PK_NORMAL_PRESSURE)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_NORMAL_PRESSURE
			Else
                Me.value = Me.value And Not PK_NORMAL_PRESSURE
			End If
		End Set
	End Property
	
	'Returns true if the tangential/barrel pressure bit is set
	
	'Sets the tangential/barrel pressure bit
	Public Property TangentPressure() As Boolean
		Get
            Return CBool(value And PK_TANGENT_PRESSURE)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_TANGENT_PRESSURE
			Else
                Me.value = Me.value And Not PK_TANGENT_PRESSURE
			End If
		End Set
	End Property
	
	'Returns true if the orientation bit is set (azimuth, altitude, twist)
	
	'Sets the orientation bit (azimuth, altitude, twist)
	Public Property Orientation() As Boolean
		Get
            Return CBool(value And PK_ORIENTATION)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_ORIENTATION
			Else
                Me.value = Me.value And Not PK_ORIENTATION
			End If
		End Set
	End Property
	
	'Returns true if the rotation bit is set (pitch, roll, yaw)
	
	'Sets the rotation bit (pitch, roll, yaw)
	Public Property Rotation() As Boolean
		Get
            Return CBool(value And PK_ROTATION)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or PK_ROTATION
			Else
                Me.value = Me.value And Not PK_ROTATION
			End If
		End Set
	End Property
End Class
Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletContextOptions_NET.TabletContextOptions")> Public Class TabletContextOptions
	'"When I am king, you will be first against the wall - with your opinion, which is of no consequence at all . . ." - Radiohead - Paranoid Android
	'VBTablet TabletContextOptions class - ©2001 Laurence Parry
	'This class sets context options. The owning class needs to call SetValue
	'before it is used (probably with a member of a private structure)
	Private value As Integer 'Reference to the bitmask this instance modifies
	
	'Set the value
	
	'Get the value
	Friend Property MaskValue() As Integer
		Get
			Return value
		End Get
		Set(ByVal Value As Integer)
            Me.value = Value
		End Set
	End Property
	
	'Returns true if this is a system context (false if a digitizing context)
	
	'Sets if this is a system context
	Public Property IsSystemCtx() As Boolean
		Get
            Return CBool(value And CXO_SYSTEM)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXO_SYSTEM
			Else
                Me.value = Me.value And (Not CXO_SYSTEM)
			End If
		End Set
	End Property
	
	'Returns true if this is a Pen Windows context
	
	'Sets if this is a Pen Windows context
	Public Property IsPenCtx() As Boolean
		Get
            Return CBool(value And CXO_PEN)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXO_PEN
			Else
                Me.value = Me.value And (Not CXO_PEN)
			End If
		End Set
	End Property
	
	
	'Returns true if this context should send event messages
	
	'Sets if this context should send event messages
	Public Property SendEventMessages() As Boolean
		Get
            Return CBool(value And CXO_MESSAGES)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXO_MESSAGES
			Else
                Me.value = Me.value And (Not CXO_MESSAGES)
			End If
		End Set
	End Property
	
	
	'Returns true if this context has a margin
	
	'Sets if this context has a margin
	Public Property HasMargin() As Boolean
		Get
            Return CBool(value And CXO_MARGIN)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXO_MARGIN
			Else
                Me.value = Me.value And (Not CXO_MARGIN)
			End If
		End Set
	End Property
	
	'Returns true if this context's margin is inside the context area
	
	'Sets if this context's margin is inside the context area
	Public Property MarginIsInside() As Boolean
		Get
            Return CBool(value And CXO_MGNINSIDE)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXO_MGNINSIDE
			Else
                Me.value = Me.value And (Not CXO_MGNINSIDE)
			End If
		End Set
	End Property
	
	'Returns true if this context should return cursor change messages
	
	'Sets if this context should return cursor change messages
	Public Property NotifyCursorChange() As Boolean
		Get
            Return CBool(value And CXO_CSRMESSAGES)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXO_CSRMESSAGES
			Else
                Me.value = Me.value And (Not CXO_CSRMESSAGES)
			End If
		End Set
	End Property
End Class
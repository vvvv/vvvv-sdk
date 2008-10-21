Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletContextLocks_NET.TabletContextLocks")> Public Class TabletContextLocks
	'"Here at GameSpy, we try to keep an open mind about necrophilia. Because, hey, you never know." - Fargo, "Gamespy Top 10 Diablo Memorial Day Barbeque Disasters"
	'VBTablet TabletContextLocks class - ©2001 Laurence Parry
	'This class sets which items of the context descriptor are locked
	'(can't be externally changed). The owning class needs to call SetValue
	'before it is used (probably with a member of a private structure)
	Private value As Integer 'Reference to the bitmask this instance modifies
	
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
	
	'Returns true if input extents are locked (origins cannot be locked)
	
	'Sets if the input extents are locked
	Public Property InputExtent() As Boolean
		Get
            Return CBool(value And CXL_INSIZE)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXL_INSIZE
			Else
                Me.value = Me.value And (Not CXL_INSIZE)
			End If
		End Set
	End Property
	
	'Returns true if the input xyz aspect is locked - if set, when one is
	'changed (externally), the others will be changed proportionately
	
	'Sets if the input xyz aspect is locked - if set, when one is
	'changed (externally), the others will be changed proportionately
	Public Property InputAspect() As Boolean
		Get
            Return CBool(value And CXL_INASPECT)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXL_INASPECT
			Else
                Me.value = Me.value And (Not CXL_INASPECT)
			End If
		End Set
	End Property
	
	'Returns true if the margin options are locked
	
	'Sets if the margin options are locked
	Public Property Margins() As Boolean
		Get
            Return CBool(value And CXL_MARGIN)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXL_MARGIN
			Else
                Me.value = Me.value And (Not CXL_MARGIN)
			End If
		End Set
	End Property
	
	'Returns true if the sensitivity settings are locked
	
	'Sets if the sensitivity settings are locked
	Public Property Sensitivity() As Boolean
		Get
            Return CBool(value And CXL_SENSITIVITY)
		End Get
		Set(ByVal Value As Boolean)
			If Value Then
                Me.value = Me.value Or CXL_SENSITIVITY
			Else
                Me.value = Me.value And (Not CXL_SENSITIVITY)
			End If
		End Set
	End Property
End Class
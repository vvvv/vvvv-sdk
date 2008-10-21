Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletDeviceMargins_NET.TabletDeviceMargins")> Public Class TabletDeviceMargins
	'"Ain't nowhere to run - ain't nowhere to hide. Ain't nowhere to go - reap the seed you sowed . . ." Ozzy Ozbourne, DMX & 'Ol Dirty Bastard - Nowhere to Run
	'VBTablet TabletDeviceMargins class - ©2001 Laurence Parry
	'This class is used to set the margin size
	Private devindex As Integer
	
	'used to initialise the device to retrieve information for
	Friend WriteOnly Property DIndex() As Integer
		Set(ByVal Value As Integer)
			devindex = Value
		End Set
	End Property
	
	'Returns the X-margin of the device's contexts, in tablet-native coordinates
	Public ReadOnly Property X() As Integer
		Get
			IsSupported(WTInfo(devindex, DVC_XMARGIN, X))
		End Get
	End Property
	
	'Returns the Y-margin of the device's contexts, in tablet-native coordinates
	Public ReadOnly Property Y() As Integer
		Get
			IsSupported(WTInfo(devindex, DVC_YMARGIN, Y))
		End Get
	End Property
	
	'Returns the Z-margin of the device's contexts, in tablet-native coordinates
	Public ReadOnly Property Z() As Integer
		Get
			IsSupported(WTInfo(devindex, DVC_ZMARGIN, Z))
		End Get
	End Property
End Class
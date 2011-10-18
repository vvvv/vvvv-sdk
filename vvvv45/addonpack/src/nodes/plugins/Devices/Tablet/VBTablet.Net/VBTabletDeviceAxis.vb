Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletDeviceAxis_NET.TabletDeviceAxis")> Public Class TabletDeviceAxis
	'"How many times must the cannon-balls fly, before they're forever banned?" - Bob Dylan - Blowing in the Wind
	'VBTablet TabletDeviceAxis class - ©2001 Laurence Parry
	'Returns X, Y, Resolution and Units for a variety of data values
	'The first three can optionally be returned in their correct units
	
	Private devindex As Integer
	Private localitem As Integer
	Private localindex As Integer 'index of information to get
	Private axis(2) As tagAxis '0 to 2 to allow for orientation
	
	'used to initialise the device to retrieve information for
	Friend WriteOnly Property DIndex() As Integer
		Set(ByVal Value As Integer)
			devindex = Value
		End Set
	End Property
	
	'used to initialise axis for orientation - sets which orientation to get
	Friend WriteOnly Property Index() As Integer
		Set(ByVal Value As Integer)
			localindex = Value
		End Set
	End Property
	
	'used to initialise axis - sets what axis info to get
	
	Friend Property item() As Integer
		Get
			Return localitem
		End Get
		Set(ByVal Value As Integer)
			localitem = Value
		End Set
	End Property
	
	'Returns the minimum value for this item
	Public ReadOnly Property Min(Optional ByVal InUnits As Boolean = False) As Single
		Get
            IsSupported(WTInfo(devindex, localitem, axis))

            If Not InUnits OrElse axis(localindex).Units = TU_NONE Then
                Return axis(localindex).Min
            Else
                Return axis(localindex).Min / Single.Parse(CStr(IIf(axis(localindex).Resolution.High >= 128, -(255 - axis(localindex).Resolution.High), axis(localindex).Resolution.High)) & "." & axis(localindex).Resolution.Low)
            End If

        End Get
    End Property
	
	'Returns the maximum value for this item
	Public ReadOnly Property Max(Optional ByVal InUnits As Boolean = False) As Single
        Get
            ' The ones which use AXIS[] need to pass the array
            ' The ones which don't need to pass the single item
            If localitem = DVC_ORIENTATION OrElse localitem = DVC_ORIENTATION Then
                IsSupported(WTInfo(devindex, localitem, axis))
            Else
                IsSupported(WTInfo(devindex, localitem, axis(localindex)))
            End If
            If Not InUnits OrElse axis(localindex).Units = TU_NONE Then
                Return axis(localindex).Max
            Else
                Return axis(localindex).Max / Single.Parse(CStr(IIf(axis(localindex).Resolution.High >= 128, -(255 - axis(localindex).Resolution.High), axis(localindex).Resolution.High)) & "." & axis(localindex).Resolution.Low)
            End If
        End Get
    End Property
	
	'Returns the units this item is measured in (if any)
	Public ReadOnly Property Units() As String
		Get
            IsSupported(WTInfo(devindex, localitem, axis))
			Select Case axis(localindex).Units
				Case TU_NONE
					Units = vbNullString
				Case TU_INCHES
					Units = "inches"
				Case TU_CENTIMETRES
					Units = "centimetres"
				Case TU_CIRCLE
					Units = "arc-units" 'You try and figure out a better name ;-)
			End Select
		End Get
	End Property
	
	'Returns the resolution this item is measured to
    Public ReadOnly Property Resolution() As Single
        Get
            IsSupported(WTInfo(devindex, localitem, axis))

            'If (axis(localindex).Units = TU_NONE) Or (InUnits = False) Then
            '    Resolution = axis(localindex).Resolution
            'Else
            '    Resolution = CSng(axis(localindex).Resolution / 4294967296.0#)
            'End If
            Return Single.Parse(Str(IIf(axis(localindex).Resolution.High >= 128, -(255 - axis(localindex).Resolution.High), axis(localindex).Resolution.High)) & "." & axis(localindex).Resolution.Low.ToString)
        End Get
    End Property
End Class
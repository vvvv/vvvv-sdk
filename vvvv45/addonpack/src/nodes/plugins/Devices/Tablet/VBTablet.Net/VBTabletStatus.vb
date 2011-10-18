Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletStatus_NET.TabletStatus")> Public Class TabletStatus
	'"Yaahh!!! Die! Die, you stupid carrot! <chop>" - Lemon, "Macropod Madness"
	'VBTablet TabletStatus class - ©2001 Laurence Parry
	'Information about the current status of the tablet/driver
	Public DataItemsUsed As TabletDeviceMask 'same values, might as well
	'use the class again. The data items in use by at least one context.
	
	'Returns the number of open contexts
	Public ReadOnly Property OpenContexts() As Integer
		Get
			IsSupported(WTInfo(WTI_STATUS, STA_CONTEXTS, OpenContexts))
		End Get
	End Property
	
	'Returns the number of open system contexts
	Public ReadOnly Property OpenSysContexts() As Integer
		Get
			IsSupported(WTInfo(WTI_STATUS, STA_SYSCTXS, OpenSysContexts))
		End Get
	End Property
	
	'Returns the maximum packet rate currently used by any context
	Public ReadOnly Property MaxCurrentPktRate() As Integer
		Get
			IsSupported(WTInfo(WTI_STATUS, STA_PKTRATE, MaxCurrentPktRate))
		End Get
	End Property
	
	'Returns the number of manager handles currently open
	Public ReadOnly Property OpenMgrHandles() As Integer
		Get
			IsSupported(WTInfo(WTI_STATUS, STA_MANAGERS, OpenMgrHandles))
		End Get
	End Property
	
	'Returns true if system pointing is available for the whole screen
	Public ReadOnly Property SystemIsFullscreen() As Boolean
		Get
            IsSupported(WTInfo(WTI_STATUS, STA_SYSTEM, CInt(SystemIsFullscreen)))
		End Get
	End Property
	
	'Returns true if the specified button is used (events requested by a context)
    Public ReadOnly Property ButtonInUse(ByVal Button As Byte) As Boolean
        Get
            If Button > 30 Then 'Button can't exist
                ButtonInUse = False
            Else
                IsSupported(WTInfo(WTI_STATUS, STA_BUTTONUSE, tmpl))
                ButtonInUse = CBool(tmpl And CInt(2 ^ Button))
            End If
        End Get
    End Property

    'Returns true if the specified button is used for a system button function
    Public ReadOnly Property ButtonInSysUse(ByVal Button As Byte) As Boolean
        Get
            If Button > 31 Then 'Button can't exist
                ButtonInSysUse = False
            Else
                IsSupported(WTInfo(WTI_STATUS, STA_SYSBTNUSE, tmpl))
                ButtonInSysUse = CBool(tmpl And CInt(2 ^ Button))
            End If
        End Get
    End Property

    Public Sub UpdateDataItemsUsed() 'Updates the DataItemsUsed structure
        WTInfo(WTI_STATUS, STA_PKTDATA, tmpl)
        DataItemsUsed.MaskValue = tmpl
    End Sub

    Public Sub New()
        MyBase.New()
        DataItemsUsed = New TabletDeviceMask
        UpdateDataItemsUsed()
    End Sub
End Class
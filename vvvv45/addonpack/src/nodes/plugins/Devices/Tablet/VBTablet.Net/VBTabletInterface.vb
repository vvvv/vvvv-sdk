Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletInterface_NET.TabletInterface")> Public Class TabletInterface
	'"Eternity lies ahead of us, and behind. Have *you* drunk your fill?" - Lady Deidrie Skye, "Conversations with Planet", Epilogue - Alpha Centauri
	'VBTablet TabletInterface class - ©2001 Laurence Parry
	'Information about the tablet interface
	'The context options supported by this interface
	Public ContextOptions As TabletContextOptions
	
	'Returns the name of the interface service provider
	Public ReadOnly Property Name() As String
        Get
            tmpl = WTInfo(WTI_INTERFACE, IFC_WINTABID, refstr)
            If tmpl = 0 Then
                IsSupported(0) 'signal an error if not supported
                Return ""
            Else
                Return refstr.Substring(0, tmpl - 1)
            End If
        End Get
    End Property
	
	'Returns the specification version of the interface
	Public ReadOnly Property SpecVersion() As Single
		Get
            Dim version As PACKEDBYTES
            IsSupported(WTInfo(WTI_INTERFACE, IFC_SPECVERSION, version))
            SpecVersion = Single.Parse(version.High & "." & version.Low) 'this gets silly
		End Get
	End Property
	
	'Returns the implementation version of the interface
	Public ReadOnly Property ImplVersion() As Single
		Get
            Dim version As PACKEDBYTES
            IsSupported(WTInfo(WTI_INTERFACE, IFC_IMPLVERSION, version))
            ImplVersion = Single.Parse(version.High & "." & version.Low) 'and here
        End Get
    End Property
	
	'Returns the current number of devices
	Public ReadOnly Property NumDevices() As Integer
		Get
			IsSupported(WTInfo(WTI_INTERFACE, IFC_NDEVICES, NumDevices))
		End Get
	End Property
	
	'Returns the number of cursors currently available
	Public ReadOnly Property NumCursors() As Integer
		Get
			IsSupported(WTInfo(WTI_INTERFACE, IFC_NCURSORS, NumCursors))
		End Get
	End Property
	
	'Returns the number of contexts supported
	Public ReadOnly Property NumContexts() As Integer
		Get
			IsSupported(WTInfo(WTI_INTERFACE, IFC_NCONTEXTS, NumContexts))
		End Get
	End Property
	
	'Returns the length of byte buffer required to save a context
	Public ReadOnly Property SaveLen() As Integer
		Get
			IsSupported(WTInfo(WTI_INTERFACE, IFC_CTXSAVESIZE, SaveLen))
		End Get
	End Property
	
	'Returns the number of (custom) extensions to the specification available
	Public ReadOnly Property NumExtensions() As Integer
		Get
			IsSupported(WTInfo(WTI_INTERFACE, IFC_NEXTENSIONS, NumExtensions))
		End Get
	End Property
	
	'Returns the number of manager handles supported
	Public ReadOnly Property NumManagers() As Integer
		Get
			IsSupported(WTInfo(WTI_INTERFACE, IFC_NMANAGERS, NumManagers))
		End Get
	End Property
	
    Public Sub New()
        MyBase.New()
        ContextOptions = New TabletContextOptions 'setup context options
        WTInfo(WTI_INTERFACE, IFC_CTXOPTIONS, tmpl)
        ContextOptions.MaskValue = tmpl
    End Sub
End Class
Option Strict On
Option Explicit On
Module modGlobals
	'"eip[0] = ex.a_entry;    /* eip, magic happens :-) */" - Linus Torvalds - exec.c, Linux v0.01
	'WinTab global functions and variables - ©2001 Laurence Parry
	'This is not great. However, I want to have the details
	'in separate modules (for ease of use), and need these
	'variables globally. Hopefully this list will stay short . . .
	
	Public Const GWL_WNDPROC As Integer = (-4)
	Public granularity As Short '1/granularity packets are passed on
	Public tmp As Short 'evil temp vars
	Public tmpl As Integer
	Public tmps As String
	Public tmpb() As Byte
	Public pkt As tagPacket 'The packet passed to Tablet.ProcessPacket
	Public refstr As String 'Buffer for return information
	Public ReportUnsupported As Boolean 'True if unsupported should raise an error
    Public pWndProcHandler As TabletWndProcHandler 'The address of the previous WndProc
    Public pTablet As Tablet 'A pointer to the VBTablet.Tablet object
    Public enumctx() As Long 'Holds the list of contexts during enumeration

	'Tests for supported functions - if not supported, and if an error
	'is required by the user, raise an error, else return 0
	Public Sub IsSupported(ByRef retcode As Integer)
		If ReportUnsupported Then
			If retcode = 0 Then
				tabError(19997, "Value not available with current device configuration")
			End If
		End If
	End Sub
	
	'Raise a tablet error
	Public Sub tabError(ByRef number As Integer, ByRef desc As String)
		OutputDebugString("VBTablet Error (" & number & "): " & desc)
		Err.Raise(vbObjectError Or number, "VBTablet.Tablet", desc)
	End Sub

    'Delegate Function WndProcDelegate(ByVal hwnd As Integer, ByVal msg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer



    Public Delegate Function EnumContextsDelegate(ByVal hCtx As Integer, ByRef lparam As Integer) As Integer

	'Callback for enumerating all contexts in Tablet.Manager.ListContexts
	Public Function EnumContexts(ByVal hCtx As Integer, ByRef lparam As Integer) As Integer
		Dim newsize As Integer
		newsize = UBound(enumctx) + 1
        ReDim Preserve enumctx(newsize)
        enumctx(newsize) = hCtx
		EnumContexts = 1
	End Function
End Module
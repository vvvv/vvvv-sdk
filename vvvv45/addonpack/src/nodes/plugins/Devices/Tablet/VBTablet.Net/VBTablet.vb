Option Strict On
Option Explicit On

Imports System.Windows.Forms

<System.Runtime.InteropServices.ProgId("Tablet_NET.Tablet")> Public Class Tablet
	'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
	'  VBTablet - a WinTab ActiveX component - ©2001 Laurence Parry   '
	'  Distributed under the GNU LGPL - see copying.txt for details   '
	'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
	'    "You are in a maze of twisty little classes, all alike"      '
	'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
	' This is the publicly creatable Tablet class for the VBTablet    '
	' WinTab ActiveX component. You can create instances of this      '
	' class in your projects and use them to communicate with WinTab. '
	'                                                                 '
	' This component was written . . . well, basically because I      '
	' wanted to see if it *could* be written. I had a look at the     '
	' existing VB WinTab programming example, and found it somewhat   '
	' lacking. So I decided to try and make my own - and here it is.  '
	'                                                                 '
	' You may find bugs, inefficiencies and inaccuracies. Hopefully   '
	' you will also find this of some use. Comments are welcome.      '
	' Enjoy!                       ---TheGreenReaper (Laurence Parry) '
	'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
	Public Device As TabletDevice
	Public Cursor As TabletCursor
	'UPGRADE_NOTE: Interface was upgraded to Interface_Renamed. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1061"'
    Public [Interface] As TabletInterface
    Public Context As TabletContext
    Public Extension As TabletExtension
    Public Manager As TabletManager
    Public Status As TabletStatus
    Public Event PacketArrival(ByRef ContextHandle As IntPtr, ByRef Cursor As Integer, ByRef X As Integer, ByRef Y As Integer, ByRef Z As Integer, ByRef Buttons As Integer, ByRef NormalPressure As Integer, ByRef TangentPressure As Integer, ByRef Azimuth As Integer, ByRef Altitude As Integer, ByRef Twist As Integer, ByRef Pitch As Integer, ByRef Roll As Integer, ByRef Yaw As Integer, ByRef PacketSerial As Integer, ByRef PacketTime As Integer)
    Public Event ProximityChange(ByRef InContext As Boolean, ByRef IsPhysical As Boolean, ByRef ContextHandle As IntPtr, ByRef ContextName As String)
    Public Event ContextRepositioned(ByRef OnTop As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)
    Public Event CursorChange(ByRef ContextHandle As IntPtr, ByRef ContextName As String)
    Public Event InfoChange(ByRef ManagerHandle As IntPtr, ByRef InfoCategory As String, ByRef InfoIndex As String)
    Public Event ContextUpdated(ByRef Status As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)
    Public Event ContextOpened(ByRef Status As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)
    Public Event ContextClosed(ByRef Status As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)
    Private Callbacks As Hashtable 'all the interface callbacks VBTablet will call
    Private Contexts As Hashtable 'all the contexts available
    Private hWndCtx As IntPtr  'hWnd used to recieve tablet packets
    Private tmptab As ITablet
    Private tmpcont As TabletContext

    'Adds an interface object to the list of interface callbacks. The
    'callbacks for each interface will be called as appropriate
    Public Function AddCallback(ByRef Callback As ITablet) As Integer
        AddCallback = CInt(Rnd() * Int32.MaxValue)
        Callbacks.Add(AddCallback.ToString(), Callback)
        System.Diagnostics.Debug.WriteLine("VBTablet: Callback " & AddCallback & " added")
    End Function

    'Removes an interface object from the list of interface callbacks.
    'The interface will no longer be called
    Public Sub RemoveCallback(ByRef Callback As Integer)
        On Error GoTo errorremovecallback
        Callbacks.Remove(Callback.ToString())
        System.Diagnostics.Debug.WriteLine("VBTablet: Callback " & Callback & " removed")
        Exit Sub
errorremovecallback:
        tabError(19911, "Tablet.RemoveCallback - Trying to remove invalid ITablet callback (" & Callback & ")")
    End Sub


    Public Property UnavailableIsError() As Boolean
        Get
            UnavailableIsError = ReportUnsupported
        End Get
        Set(ByVal Value As Boolean)
            ReportUnsupported = Value
        End Set
    End Property

    'Returns the current value of packet granularity set for the Tablet

    'Sets the value of packet granularity. This a way of increasing
    'performance - by increasing the granularity, you will reduce the
    'number of events your application recieves.
    Public Property PktGranularity() As Short
        Get
            PktGranularity = granularity
        End Get
        Set(ByVal Value As Short)
            granularity = System.Math.Abs(Me.PktGranularity)
        End Set
    End Property

    Property Connected() As Boolean
        Get
            Connected = Not pWndProcHandler Is Nothing
        End Get
        Set(ByVal Value As Boolean)
            If Value Then
                'Connects the Tablet to a window and attaches the current context.
                'You must set hWnd to a valid hWnd before connecting
                If pWndProcHandler Is Nothing Then
                    If Not hWndCtx.Equals(IntPtr.Zero) Then
                        If Not Context Is Nothing Then
                            Context.OpenContext(Context.Enabled)
                            If Context.hCtx.Equals(IntPtr.Zero) Then tabError(19917, "Tablet.Connected - Context failed to open")
                            Try
                                pWndProcHandler = New TabletWndProcHandler(hWndCtx)
                            Finally
                                If pWndProcHandler Is Nothing Then
                                    tabError(19920, "Tablet.Connect - Could not subclass window (pOldWndProc = 0). LastDLLError = " & Err.LastDllError)
                                Else
                                    System.Diagnostics.Debug.WriteLine("VBTablet: Connected")
                                    OutputDebugString("VBTablet: Connected" & vbNewLine)
                                End If
                            End Try
                        End If
                    Else
                        tabError(19905, "Tablet.Connected - Can't connect without valid hWnd")
                    End If
                End If
            Else
                'disconnects the Tablet from it's hWnd (which must be valid)
                If Not pWndProcHandler Is Nothing Then
                    pWndProcHandler.ReleaseHandle()
                    pWndProcHandler = Nothing
                Else
                    'Could raise an error . . . but it might make it harder
                    'tabError 19906, "Can't disconnect hWnd - none connected"
                    System.Diagnostics.Debug.WriteLine("VBTablet: Disconnecting (though not connected)")
                End If
                'Manager.Enabled = False 'disable manager if connected
                System.Diagnostics.Debug.WriteLine("VBTablet: Disconnected")
                OutputDebugString("VBTablet: Disconnected" & vbNewLine)
            End If
        End Set
    End Property

    'The hWnd used to capture packets. Must be a *valid* hWnd

    'Sets the window handle to be used. It's probably not a good idea to
    'reset this while connected - but hey, it's your program! ;-)
    Public Property hWnd() As IntPtr
        Get
            hWnd = hWndCtx
        End Get
        Set(ByVal Value As IntPtr)
            If pWndProcHandler Is Nothing Then
                hWndCtx = Value
            Else 'handle connected state
                Me.Connected = False
                hWndCtx = Value
                Me.Connected = True
            End If
            If Manager.Enabled Then 'reset to use new hWnd
                Manager.Enabled = False
                Manager.Enabled = True
            End If
        End Set
    End Property

    'Loads up all classes, sets the default context
    Public Sub New()
        MyBase.New()
        On Error GoTo errorinit
        Randomize()
        'UPGRADE_ISSUE: ObjPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'
        pTablet = Me 'Save pointer to Me
        OutputDebugString("VBTablet: Initialising Device" & vbNewLine)
        Device = New TabletDevice
        OutputDebugString("VBTablet: Initialising Cursor" & vbNewLine)
        Cursor = New TabletCursor
        OutputDebugString("VBTablet: Initialising Interface" & vbNewLine)
        [Interface] = New TabletInterface
        OutputDebugString("VBTablet: Initialising Context" & vbNewLine)
        Context = New TabletContext
        OutputDebugString("VBTablet: Initialising Extension" & vbNewLine)
        Extension = New TabletExtension
        OutputDebugString("VBTablet: Initialising Status" & vbNewLine)
        Status = New TabletStatus
        OutputDebugString("VBTablet: Initialising Manager" & vbNewLine)
        Manager = New TabletManager
        Callbacks = New Hashtable
        Contexts = New Hashtable
        System.Diagnostics.Debug.WriteLine("VBTablet: Loaded")
        OutputDebugString("VBTablet: Loaded" & vbNewLine)
        granularity = 1
        refstr = New String(" "c, 1024)
        If WTInfo(0, 0, vbNullString) = 0 Then
            tabError(19999, "Cannot initialize tablet - No tablet present")
        Else
            OutputDebugString("VBTablet: Adding default context" & vbNewLine)
            Contexts.Add("DefVBTabletContext", Context) 'this is the default context
            OutputDebugString("VBTablet: Getting default context" & vbNewLine)
            Context.GetContext(WTI_DEFSYSCTX, 0)
            Context.InitSettings() 'startup settings
        End If
        Exit Sub
errorinit:
        If Err.Number = 53 Then 'someone forgot to install WinTab, or is running this randomly
            tabError(19998, "Cannot find WINTAB32.DLL - check WinTab installation")
        Else
            Err.Raise(Err.Number, Err.Source, Err.Description, Err.HelpFile, Err.HelpContext)
        End If
    End Sub

    'Closes all contexts, disconnects from the window
    Protected Overrides Sub Finalize()
        If Not pWndProcHandler Is Nothing Then Me.Connected = False
        For Each tmpcont In Contexts.Values
            If Not tmpcont.hCtx.Equals(IntPtr.Zero) Then
                tmpcont.CloseContext()
                System.Windows.Forms.Application.DoEvents()
            End If
        Next tmpcont
        System.Diagnostics.Debug.WriteLine("VBTablet: Unloaded")
        OutputDebugString("VBTablet: Unloaded" & vbNewLine)
        MyBase.Finalize()
    End Sub

    'Adds a context, optionally with a specific ID (max length 39 chars)
    'and optionally as a digitising context
    Public Function AddContext(Optional ByVal ContextID As String = "", Optional ByRef IsDigitizingContext As Boolean = False) As String
        Dim contextobj As Object
        If ContextID = vbNullString Then
            AddContext = (Rnd() * Integer.MaxValue).ToString
            While Not Contexts.Contains(AddContext)
                AddContext = (Rnd() * Integer.MaxValue).ToString
            End While
            ' OK, so this number is valid
        Else
            If ContextID.Length > 39 Then ContextID = ContextID.Substring(0, 39)
            If Contexts.Contains(ContextID) Then
                tabError(19915, "VBTablet: Trying to add context " & ContextID & ", which is already present")
                'Added by Languard
                'Correcting compiler warning
                Return Nothing
            Else
                AddContext = ContextID
            End If
        End If
        On Error GoTo 0
        tmpcont = New TabletContext
        Contexts.Add(AddContext, tmpcont)
        If IsDigitizingContext Then
            tmpcont.GetContext(WTI_DEFCONTEXT, 0)
        Else
            tmpcont.GetContext(WTI_DEFSYSCTX, 0)
        End If
        tmpcont.Name = AddContext
        tmpcont.InitSettings()
        System.Diagnostics.Debug.WriteLine("VBTablet: Context " & AddContext & " added")
    End Function

    'Removes a tablet context
    Public Sub RemoveContext(ByRef ContextID As String)
        On Error GoTo errorremovecontext
        If Contexts.Item(ContextID) Is Context Then SelectContext("DefVBTabletContext")
        'UPGRADE_WARNING: Couldn't resolve default property of object Contexts().CloseContext. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
        DirectCast(Contexts.Item(ContextID), TabletContext).CloseContext()
        Contexts.Remove(ContextID)
        System.Diagnostics.Debug.WriteLine("VBTablet: Context " & ContextID & " removed")
        Exit Sub
errorremovecontext:
        tabError(19916, "Tablet.RemoveContext - Trying to remove nonexistant context (" & ContextID & ")")
    End Sub

    'Selects a tablet context
    Public Sub SelectContext(ByRef ContextID As String)
        If Contexts.Contains(ContextID) Then
            Context = DirectCast(Contexts.Item(ContextID), TabletContext)
            System.Diagnostics.Debug.WriteLine("VBTablet: Context " & ContextID & " selected")
        Else
            tabError(19916, "Tablet.SelectContext - Trying to select nonexistant context '" & ContextID & "'")
        End If
    End Sub

    'Retrieves the handle for the named context
    Public Function GetCtxHandleByName(ByRef ContextName As String) As IntPtr
        GetCtxHandleByName = IntPtr.Zero
        If Contexts.Contains(ContextName) Then
            GetCtxHandleByName = DirectCast(Contexts(ContextName), TabletContext).hCtx
        End If
    End Function

    Public Function GetCtxNameByHandle(ByRef ContextHandle As IntPtr) As String
        GetCtxNameByHandle = ""
        For Each tmpcont In Contexts.Values
            If tmpcont.hCtx.Equals(ContextHandle) Then
                GetCtxNameByHandle = tmpcont.Name
                Exit For
            End If
        Next tmpcont
    End Function

    'Process packets, dispatch events and call interfaces
    Friend Sub ProcessPacket(ByRef Message As Message)
        Select Case Message.Msg
            Case WT_PACKET
                With pkt
                    Context.pktStatus = .pkStatus
                    If Callbacks.Count() = 0 Then
                        'Beep
                        RaiseEvent PacketArrival(Message.LParam, .pkCursor, .pkX, .pkY, .pkZ, .pkButtons, .pkNormalPressure, .pkTangentPressure, .pkAzimuth, .pkAltitude, .pkTwist, .pkPitch, .pkRoll, .pkYaw, .pkSerial, .pkTime)
                    Else
                        For Each tmptab In Callbacks.Values
                            tmptab.PacketArrival(Message.LParam, .pkCursor, .pkX, .pkY, .pkZ, .pkButtons, .pkNormalPressure, .pkTangentPressure, .pkAzimuth, .pkAltitude, .pkTwist, .pkPitch, .pkRoll, .pkYaw, .pkSerial, .pkTime)
                        Next tmptab
                    End If
                End With

            Case WT_PROXIMITY
                'Debug.Print "test0:" & lparam
                tmpl = (Message.LParam.ToInt32 And &HFFFF0000)
                'Debug.Print "test2:" & tmpl
                'Debug.Print "test1:" & ((lparam - tmpl) And &HFFFF)
                tmps = GetCtxNameByHandle(Message.WParam)
                If Callbacks.Count() = 0 Then
                    RaiseEvent ProximityChange(CBool((Message.LParam.ToInt32 - tmpl) And &HFFFFS), CBool(tmpl), Message.WParam, tmps)
                Else
                    For Each tmptab In Callbacks.Values
                        tmptab.ProximityChange(CBool((Message.LParam.ToInt32 - tmpl) And &HFFFFS), CBool(tmpl), Message.WParam, tmps)
                    Next tmptab
                End If

            Case WT_CSRCHANGE
                tmps = GetCtxNameByHandle(Message.LParam)
                If Callbacks.Count() = 0 Then
                    RaiseEvent CursorChange(Message.LParam, tmps)
                Else
                    For Each tmptab In Callbacks.Values
                        tmptab.CursorChange(Message.LParam, tmps)
                    Next tmptab
                End If

            Case WT_CTXOPEN
                tmps = GetCtxNameByHandle(Message.WParam)
                If Callbacks.Count() = 0 Then
                    RaiseEvent ContextOpened(Message.LParam.ToInt32, Message.WParam, tmps)
                Else
                    For Each tmptab In Callbacks.Values
                        tmptab.ContextOpened(Message.LParam.ToInt32, Message.WParam, tmps)
                    Next tmptab
                End If
                System.Diagnostics.Debug.WriteLine("VBTablet: Context Opened")

            Case WT_CTXCLOSE
                tmps = GetCtxNameByHandle(Message.WParam)
                If Callbacks.Count() = 0 Then
                    RaiseEvent ContextClosed(Message.LParam.ToInt32, Message.WParam, tmps)
                Else
                    For Each tmptab In Callbacks.Values
                        tmptab.ContextClosed(Message.LParam.ToInt32, Message.WParam, tmps)
                    Next tmptab
                End If
                System.Diagnostics.Debug.WriteLine("VBTablet: Context Closed")

            Case WT_CTXUPDATE
                For Each tmpcont In Contexts.Values
                    If tmpcont.hCtx.Equals(Message.WParam) Then
                        Context.Reload()
                        Exit For
                    End If
                Next tmpcont
                tmps = GetCtxNameByHandle(Message.WParam)
                If Callbacks.Count() = 0 Then
                    RaiseEvent ContextUpdated(Message.LParam.ToInt32, Message.WParam, tmps)
                Else
                    For Each tmptab In Callbacks.Values
                        tmptab.ContextUpdated(Message.LParam.ToInt32, Message.WParam, tmps)
                    Next tmptab
                End If

            Case WT_CTXOVERLAP
                For Each tmpcont In Contexts.Values
                    If tmpcont.hCtx.Equals(Message.WParam) Then
                        tmpcont.OnTop = CBool(Message.LParam.ToInt32 And CXS_ONTOP)
                        tmps = tmpcont.Name
                        Exit For
                    End If
                Next tmpcont
                If Callbacks.Count() = 0 Then
                    RaiseEvent ContextRepositioned(Message.LParam.ToInt32, Message.WParam, tmps)
                Else
                    For Each tmptab In Callbacks.Values
                        tmptab.ContextRepositioned(Message.LParam.ToInt32, Message.WParam, tmps)
                    Next tmptab
                End If

            Case WT_INFOCHANGE
                'Debug.Print "test0:" & lparam
                tmpl = (Message.LParam.ToInt32 And &HFFFF0000)
                'Debug.Print "test2:" & tmpl
                'Debug.Print "test1:" & ((lparam - tmpl) And &HFFFF)
                Select Case (Message.LParam.ToInt32 - tmpl) And &HFFFFS
                    Case WTI_INTERFACE
                        tmps = "Interface"

                    Case WTI_STATUS
                        tmps = "Status"

                    Case WTI_DEFCONTEXT
                        tmps = "Default Context (digitising)"

                    Case WTI_DEFSYSCTX
                        tmps = "Default Context (system)"

                    Case WTI_DEVICES
                        tmps = "Device"

                    Case WTI_CURSORS
                        tmps = "Cursor"

                    Case WTI_EXTENSIONS
                        tmps = "Extension"

                    Case WTI_DDCTXS
                        tmps = "Default Device Context (digitising)"

                    Case WTI_DSCTXS
                        tmps = "Default Device Context (system)"

                    Case Else
                        tmps = "Unknown"

                End Select
                If Callbacks.Count() = 0 Then
                    RaiseEvent InfoChange(Message.WParam, tmps, CStr(tmpl + 1))
                Else
                    For Each tmptab In Callbacks.Values
                        tmptab.InfoChange(Message.WParam, tmps, CStr(tmpl + 1))
                    Next tmptab
                End If
        End Select
    End Sub

    Public Function GetExpKeys() As Short()
        Return pkt.pkExpKeys
    End Function

    'Gets the most recent packet's extension data as a variant containing
    'a byte array. Untested as I don't have any such extensions.
    Public Function GetExtensionData(ByRef numBytes As Short) As Object
        'added by Languard
        'changed to correct compiler warning
        If numBytes < 1 Then
            Return Nothing
        End If
        If numBytes > 1024 Then
            numBytes = 1024
        End If
        'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
        ReDim tmpb(numBytes - 1)
        System.Array.Copy(pkt.pkExtension, tmpb, numBytes)
        'UPGRADE_WARNING: Couldn't resolve default property of object GetExtensionData. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
        'Change by Languard
        'replaced VB6 copy with clone
        Return tmpb.Clone
    End Function

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    'This library is free software; you can redistribute it and/or            '
    'modify it under the terms of the GNU Lesser General Public               '
    'License as published by the Free Software Foundation; either             '
    'version 2.1 of the License, or (at your option) any later version.       '
    '                                                                         '
    'This library is distributed in the hope that it will be useful,          '
    'but WITHOUT ANY WARRANTY; without even the implied warranty of           '
    'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU        '
    'Lesser General Public License for more details.                          '
    '                                                                         '
    'You should have received a copy of the GNU Lesser General Public         '
    'License along with this library; if not, write to the Free Software      '
    'Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA '
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
End Class
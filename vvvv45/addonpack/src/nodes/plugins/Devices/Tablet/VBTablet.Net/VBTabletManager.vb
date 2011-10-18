Option Strict Off
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletManager_NET.TabletManager")> Public Class TabletManager
	'"So here it is, Merry Christmas, everybody's having fun. Look to the future, now - it's only just begun . . ." - Slade - Merry Christmas
	'VBTablet TabletManager class - ©2001 Laurence Parry
	'Changes global device and interface settings. These are not always
	'fully implemented by WinTab drivers, many of which rely on their own
	'settings dialogs, but at least *some* of them work for me . . . <g>
	'Be warned - you can mess up stuff if you're not careful, or even
	'if you are (eg. Wacom's setting dialog crashes if you have set the
	'eraser to function of a middle-click, which works fine otherwise)
    'UPGRADE_NOTE: hMgr was upgraded to hMgr. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1061"'
    Private hMgr As IntPtr 'Manager handle
    Private hHook As IntPtr 'Packet hook handle
    Private tmplng As Integer 'reserve tmpl for possible calling functions

    'Returns the manager handle in use
    ReadOnly Property hManager() As IntPtr
        Get
            hManager = hMgr
        End Get
    End Property

    'True if the manager functions have been enabled

    'Enable or disable manager functions. Tablet.hWnd must be valid to enable
    Property Enabled() As Boolean
        Get
            Return IIf(hMgr.Equals(IntPtr.Zero), False, True)
        End Get
        Set(ByVal Value As Boolean)
            If Value Then
                If hMgr.Equals(IntPtr.Zero) Then
                    If pTablet.hWnd.Equals(IntPtr.Zero) Then
                        tabError(19922, "Tablet.Manager - can't enable without valid Tablet.hWnd")
                    Else
                        hMgr = WTMgrOpen(pTablet.hWnd, WT_DEFBASE)
                        System.Diagnostics.Debug.WriteLine("VBTablet: Manager connected - hMgr = " & hMgr.ToString)
                    End If
                End If
            Else
                If Not hMgr.Equals(IntPtr.Zero) Then
                    WTMgrClose(hMgr)
                    System.Diagnostics.Debug.WriteLine("VBTablet: Manager disconnected")
                    hMgr = IntPtr.Zero
                End If
            End If
        End Set
    End Property

    'Returns true if a configuration window is available for the specified device
    Public ReadOnly Property DeviceCfgWindowAvailable(ByVal DeviceNum As Integer) As Boolean
        Get
            If hMgr.Equals(IntPtr.Zero) Then Exit Property
            WTInfo(WTI_INTERFACE, IFC_NDEVICES, tmplng)
            If tmplng < DeviceNum Or DeviceNum < 1 Then
                tabError(19910, "Invalid device number (" & DeviceNum & ") - use Tablet.Interface.NumDevices to retrieve the number of devices available")
            Else
                DeviceNum = DeviceNum - 1
            End If
            DeviceCfgWindowAvailable = IIf(WTMgrDeviceConfig(hMgr, DeviceNum, IntPtr.Zero) = 0, False, True)
        End Get
    End Property

    'Returns the window handle of the window owning a context handle
    Public Function GethWndByhCtx(ByRef ContextHandle As IntPtr) As IntPtr
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        GethWndByhCtx = WTMgrContextOwner(hMgr, ContextHandle)
    End Function

    'Returns a list of all context handles as a 1-based array of longs
    'in a variant. See EnumContexts in modGlobals
    Public Function ListContexts() As Object
        If hMgr.Equals(IntPtr.Zero) Then Exit Function
        'UPGRADE_ISSUE: As Long was removed from ReDim enumctx(0 To 0) statement. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1056"'
        ReDim enumctx(0) 'ubound=0 indicates start
        'UPGRADE_WARNING: Add a delegate for AddressOf EnumContexts Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1048"'
        WTMgrContextEnum(hMgr, AddressOf EnumContexts, 0)
        'UPGRADE_WARNING: Couldn't resolve default property of object enumctx. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
        'UPGRADE_WARNING: Couldn't resolve default property of object ListContexts. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
        ListContexts = enumctx
    End Function

    Protected Overrides Sub Finalize()
        If Not hMgr.Equals(IntPtr.Zero) Then Enabled = False 'disconnect if closing and connected
        MyBase.Finalize()
    End Sub

    'Shows the device configuration window for the specified device.
    'Returns 0 if unavailable, 1 if the user selected cancel,
    '2 if the user selected OK and 3 if changes require a reboot
    Public Function ShowDeviceCfgWindow(ByRef DeviceNum As Integer) As Integer
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        WTInfo(WTI_INTERFACE, IFC_NDEVICES, tmplng)
        If tmplng < DeviceNum Or DeviceNum < 1 Then
            tabError(19910, "Invalid device number (" & DeviceNum & ") - use Tablet.Interface.NumDevices to retrieve the number of devices available")
        Else
            DeviceNum = DeviceNum - 1
        End If
        ShowDeviceCfgWindow = WTMgrDeviceConfig(hMgr, DeviceNum, pTablet.hWnd)
    End Function

    'Sets global data for a specified extension (identified by ID).
    'Data must be in the correct form and of the correct size for the
    'extension, and should be passed as a 1-dimensional byte array.
    'If Data is Null, the extension will be reset to defaults.
    'Returns true if the data was set correctly.
    Public Function SetExtData(ByRef ExtensionID As Integer, ByRef Data As Object) As Boolean
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
        Dim tmpb() As Byte
        If IsDBNull(Data) Then
            SetExtData = IIf(WTMgrExt(hMgr, ExtensionID, WTP_LPDEFAULT) = 0, False, True)
        ElseIf Not IsArray(Data) Then
            tabError(19924, "Tablet.Manager.SetExtData - Data must be an array or Null")
        Else
            'UPGRADE_WARNING: Lower bound of array tmpb was changed from LBound(Data) to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            ReDim tmpb(UBound(Data))
            For tmp = LBound(Data) To UBound(Data)
                'UPGRADE_WARNING: Couldn't resolve default property of object Data(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                tmpb(tmp) = Data(tmp)
            Next tmp
            SetExtData = IIf(WTMgrExt(hMgr, ExtensionID, tmpb(LBound(tmpb))) = 0, False, True)
        End If
    End Function

    'Sets per-cursor data for a specified extension (identified by ID).
    'Data must be in the correct form and of the correct size for the
    'extension, and should be passed as a 1-dimensional byte array.
    'If Data is Null, the extension will be reset to defaults.
    'Returns true if the data was set correctly.
    Public Function SetExtCursorData(ByRef Cursor As Integer, ByRef ExtensionID As Integer, ByRef Data As Object) As Boolean
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        tmplng = 0
        WTInfo(WTI_INTERFACE, IFC_NCURSORS, tmplng)
        Dim tmpb() As Byte
        If tmplng < Cursor Or Cursor < 1 Then
            tabError(19910, "Invalid cursor number (" & Cursor & ") - use Tablet.Interface.NumCursors to retrieve the number of cursors available")
        Else
            Cursor = Cursor - 1
            'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
            If Data Is Nothing Then
                SetExtCursorData = IIf(WTMgrCsrExt(hMgr, Cursor, ExtensionID, WTP_LPDEFAULT) = 0, False, True)
            ElseIf Not IsArray(Data) Then
                tabError(19933, "Tablet.Manager.SetExtCursorData - Data must be an array or Null")
            Else
                'UPGRADE_WARNING: Lower bound of array tmpb was changed from LBound(Data) to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tmpb(UBound(Data))
                For tmp = LBound(Data) To UBound(Data)
                    'UPGRADE_WARNING: Couldn't resolve default property of object Data(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    tmpb(tmp) = Data(tmp)
                Next tmp
                SetExtCursorData = IIf(WTMgrCsrExt(hMgr, Cursor, ExtensionID, tmpb(LBound(tmpb))) = 0, False, True)
            End If
        End If
    End Function

    'Enables a cursor, or indicates the state of a cursor if the
    'cursor is hardware-enabled. Returns true if in the passed state.
    Public Function EnableCursor(ByRef Cursor As Integer, ByRef Enable As Boolean) As Boolean
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        tmplng = 0
        WTInfo(WTI_INTERFACE, IFC_NCURSORS, tmplng)
        If tmplng < Cursor Or Cursor < 1 Then
            tabError(19910, "Invalid cursor number (" & Cursor & ") - use Tablet.Interface.NumCursors to retrieve the number of cursors available")
        Else
            EnableCursor = IIf(WTMgrCsrEnable(hMgr, Cursor - 1, CInt(Enable)) = 0, False, True)
        End If
    End Function

    'Sets the physical->logical and logical->action button mappings
    'for the specified cursor (available from Tablet.Cursor.PhysicalMapping
    'and Tablet.Cursor.LogicalMapping). Each mapping should be in the
    'form of a 32-element byte array, or Null (to reset defaults)
    Public Function SetCursorButtonMap(ByRef Cursor As Integer, ByRef PhysicalMapping As Byte(), ByRef LogicalMapping As Byte()) As Byte()
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        tmplng = 0
        WTInfo(WTI_INTERFACE, IFC_NCURSORS, tmplng)
        Dim arraypos As Integer
        'UPGRADE_WARNING: Lower bound of array LogArray was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
        Dim LogArray(32) As Byte
        'UPGRADE_WARNING: Lower bound of array PhysArray was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
        Dim PhysArray(32) As Byte
        If tmplng < Cursor Or Cursor < 1 Then
            tabError(19910, "Invalid cursor number (" & Cursor & ") - use Tablet.Interface.NumCursors to retrieve the number of cursors available")
        Else 'A bit messy
            If Not IsArray(PhysicalMapping) Then
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
                If Not IsDBNull(PhysicalMapping) Then
                    GoTo errorarray
                End If
            ElseIf UBound(PhysicalMapping) - LBound(PhysicalMapping) + 1 <> 32 Then
                GoTo errorarray
            Else
                For arraypos = 1 To 32
                    'UPGRADE_WARNING: Couldn't resolve default property of object PhysicalMapping(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    PhysArray(arraypos) = PhysicalMapping(LBound(PhysicalMapping) + arraypos - 1) - 1
                Next arraypos
            End If
            If Not IsArray(LogicalMapping) Then
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
                If Not IsDBNull(LogicalMapping) Then
                    GoTo errorarray
                End If
            ElseIf UBound(LogicalMapping) - LBound(LogicalMapping) + 1 <> 32 Then
                GoTo errorarray
            Else
                For arraypos = 1 To 32
                    'UPGRADE_WARNING: Couldn't resolve default property of object LogicalMapping(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    LogArray(arraypos) = LogicalMapping(LBound(LogicalMapping) + arraypos - 1)
                Next arraypos
            End If
            Cursor = Cursor - 1
            'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
            If IsDBNull(LogicalMapping) And IsDBNull(PhysicalMapping) Then
                'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'
                'UPGRADE_WARNING: Couldn't resolve default property of object SetCursorButtonMap. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                SetCursorButtonMap = IIf(WTMgrCsrButtonMap(hMgr, Cursor, WTP_LPDEFAULTBYTE, WTP_LPDEFAULTBYTE) = 0, False, True)
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
            ElseIf IsDBNull(LogicalMapping) Then
                'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'
                'UPGRADE_WARNING: Couldn't resolve default property of object SetCursorButtonMap. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                SetCursorButtonMap = IIf(WTMgrCsrButtonMap(hMgr, Cursor, PhysArray(1), WTP_LPDEFAULTBYTE) = 0, False, True)
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
            ElseIf IsDBNull(PhysicalMapping) Then
                'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'
                'UPGRADE_WARNING: Couldn't resolve default property of object SetCursorButtonMap. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                SetCursorButtonMap = IIf(WTMgrCsrButtonMap(hMgr, Cursor, WTP_LPDEFAULTBYTE, LogArray(1)) = 0, False, True)
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object SetCursorButtonMap. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                SetCursorButtonMap = IIf(WTMgrCsrButtonMap(hMgr, Cursor, PhysArray, LogArray) = 0, False, True)
            End If
        End If
        Exit Function
errorarray:
        tabError(19925, "Tablet.Manager.SetCursorButtonMap - Mappings must be 32-byte arrays or Null")
    End Function

    'Sets button press and release marks for normal and tangential pressure
    'to the specified values, or resets them to their defaults if < 0
    '(Note: Press and release cannot be reset separately)
    'If pressure > NormalPress and the button is in up state, set to down.
    'If pressure < NormalRelease and the button is in down state, set to up.
    Public Function SetButtonMarks(ByRef Cursor As Integer, ByRef NormalPress As Integer, ByRef NormalRelease As Integer, ByRef TangentPress As Integer, ByRef TangentRelease As Integer) As Boolean
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        tmplng = 0
        WTInfo(WTI_INTERFACE, IFC_NCURSORS, tmplng)
        Dim normalarray(1) As Integer
        Dim tangentarray(1) As Integer
        If tmplng < Cursor Or Cursor < 1 Then
            tabError(19910, "Invalid cursor number (" & Cursor & ") - use Tablet.Interface.NumCursors to retrieve the number of cursors available")
        Else
            Cursor = Cursor - 1
            If (NormalPress < 0 Or NormalRelease < 0) And (TangentPress < 0 Or TangentRelease < 0) Then
                SetButtonMarks = IIf(WTMgrCsrPressureBtnMarksEx(hMgr, Cursor, WTP_LPDEFAULT, WTP_LPDEFAULT) = 0, False, True)
            ElseIf (NormalPress < 0 Or NormalRelease < 0) Then
                tangentarray(0) = TangentRelease
                tangentarray(1) = TangentPress
                SetButtonMarks = IIf(WTMgrCsrPressureBtnMarksEx(hMgr, Cursor, WTP_LPDEFAULT, tangentarray(0)) = 0, False, True)
            ElseIf (TangentPress < 0 Or TangentRelease < 0) Then
                normalarray(0) = NormalRelease
                normalarray(1) = NormalPress
                SetButtonMarks = IIf(WTMgrCsrPressureBtnMarksEx(hMgr, Cursor, normalarray(0), WTP_LPDEFAULT) = 0, False, True)
            Else
                normalarray(0) = NormalRelease
                normalarray(1) = NormalPress
                tangentarray(0) = TangentRelease
                tangentarray(1) = TangentPress
                SetButtonMarks = IIf(WTMgrCsrPressureBtnMarksEx(hMgr, Cursor, normalarray(0), tangentarray(0)) = 0, False, True)
            End If
        End If
    End Function

    'Sets the current Tablet.Context as a default context (digitizing or system),
    'optionally for a specific device. You should probably base it off the current
    'default otherwise settings changes might not take effect. If reset is true,
    'reset the context to the factory default

    Public Function SetDefaultContext(ByRef bSystem As Boolean, ByRef bReset As Boolean, Optional ByRef Device As Integer = 0) As Boolean
        'On Error Resume Next
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        Dim hMgrCtx As IntPtr
        If Device < 1 Then
            hMgrCtx = WTMgrDefContext(hMgr, bSystem)
        Else
            hMgrCtx = WTMgrDefContextEx(hMgr, Device - 1, bSystem)
        End If
        If Not hMgrCtx.Equals(IntPtr.Zero) Then
            If bReset Then
                tmpl = WTSetLong(hMgrCtx, WTP_LPDEFAULT)
            Else

                With pTablet.Context
                    Dim ctx As LOGCONTEXT = .hCtxDesc
                    ctx.lcPktData = .Report.MaskValue
                    ctx.lcMoveMask = .MoveEvents.MaskValue
                    ctx.lcPktMode = .Relative.MaskValue
                    ctx.lcOptions = .Options.MaskValue
                    .hCtxDesc = ctx
                    tmpl = WTSet(hMgrCtx, .hCtxDesc)
                End With

            End If
            If tmpl = 0 Then
                tabError(19935, "Tablet.Manager.SetDefaultContext - Failed to write current context to the default " & IIf(bSystem, "system", "digitizing") & " context" & IIf(Device < 1, "", " for device " & Device))
                SetDefaultContext = False
            Else
                SetDefaultContext = True
            End If
        Else
            tabError(19934, "Tablet.Manager.SetDefaultContext - Could not get a writable context handle for the default " & IIf(bSystem, "system", "digitizing") & " context" & IIf(Device < 1, "", " for device " & Device))
        End If
    End Function

    'Sets the normal and tangential pressure translation curves.
    'NormalPressure and TangentPressure should be arrays of Longs of the
    'same size as returned by Tablet.Cursor.Normal/TangentPressureCurve, or Null.
    Public Function SetPressureCurve(ByRef Cursor As Integer, ByRef NormalPressure As Object, ByRef TangentPressure As Object) As Boolean
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        tmplng = 0
        WTInfo(WTI_INTERFACE, IFC_NCURSORS, tmplng)
        Dim Normal() As Integer
        Dim tangent() As Integer
        Dim arraypos As Integer
        If tmplng < Cursor Or Cursor < 1 Then
            tabError(19910, "Invalid cursor number (" & Cursor & ") - use Tablet.Interface.NumCursors to retrieve the number of cursors available")
        Else
            Cursor = Cursor - 1
            'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
            If IsDBNull(NormalPressure) And IsDBNull(TangentPressure) Then
                SetPressureCurve = IIf(WTMgrCsrPressureResponse(hMgr, Cursor, WTP_LPDEFAULT, WTP_LPDEFAULT) = 0, False, True)
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
            ElseIf IsDBNull(NormalPressure) And IsArray(TangentPressure) Then
                'UPGRADE_WARNING: Lower bound of array tangent was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tangent(UBound(TangentPressure) - LBound(TangentPressure) + 1)
                For arraypos = 1 To UBound(TangentPressure) - LBound(TangentPressure) + 1
                    'UPGRADE_WARNING: Couldn't resolve default property of object TangentPressure(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    tangent(arraypos) = TangentPressure(LBound(TangentPressure) + arraypos - 1)
                Next arraypos
                SetPressureCurve = IIf(WTMgrCsrPressureResponse(hMgr, Cursor, WTP_LPDEFAULT, tangent(1)) = 0, False, True)
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
            ElseIf IsDBNull(TangentPressure) And IsArray(NormalPressure) Then
                'UPGRADE_WARNING: Lower bound of array Normal was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim Normal(UBound(NormalPressure) - LBound(NormalPressure) + 1)
                For arraypos = 1 To UBound(NormalPressure) - LBound(NormalPressure) + 1
                    'UPGRADE_WARNING: Couldn't resolve default property of object NormalPressure(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    Normal(arraypos) = NormalPressure(LBound(NormalPressure) + arraypos - 1)
                Next arraypos
                SetPressureCurve = IIf(WTMgrCsrPressureResponse(hMgr, Cursor, Normal(1), WTP_LPDEFAULT) = 0, False, True)
            ElseIf IsArray(NormalPressure) And IsArray(TangentPressure) Then
                'UPGRADE_WARNING: Lower bound of array Normal was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim Normal(UBound(NormalPressure) - LBound(NormalPressure) + 1)
                For arraypos = 1 To UBound(NormalPressure) - LBound(NormalPressure) + 1
                    'UPGRADE_WARNING: Couldn't resolve default property of object NormalPressure(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    Normal(arraypos) = NormalPressure(LBound(NormalPressure) + arraypos - 1)
                Next arraypos
                'UPGRADE_WARNING: Lower bound of array tangent was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tangent(UBound(TangentPressure) - LBound(TangentPressure) + 1)
                For arraypos = 1 To UBound(TangentPressure) - LBound(TangentPressure) + 1
                    'UPGRADE_WARNING: Couldn't resolve default property of object TangentPressure(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    tangent(arraypos) = TangentPressure(LBound(TangentPressure) + arraypos - 1)
                Next arraypos
                SetPressureCurve = IIf(WTMgrCsrPressureResponse(hMgr, Cursor, Normal(1), tangent(1)) = 0, False, True)
            Else
                tabError(19931, "Tablet.Manager.SetPressureCurve - Arguments must be arrays or Null")
            End If
        End If
    End Function

    'Sets a packet hook function in an external module. If you know what
    'that means, and you know how to use it, you probably aren't using VB
    'but hey, this library might as well be complete! - <g>
    'This code is totally untested, as it's not supported on my tablet.
    'Read the WinTab specification for more info on this if you need it.
    Public Function SetPacketHook(ByRef Recording As Boolean, ByRef DLLName As String, ByRef DLLFunc As String) As Boolean
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        If Not hHook.Equals(IntPtr.Zero) Then RemovePacketHook()
        hHook = WTMgrPacketHookEx(hMgr, IIf(Recording, WTH_RECORD, WTH_PLAYBACK), DLLName, DLLFunc)
        SetPacketHook = IIf(hHook.Equals(IntPtr.Zero), False, True)
    End Function

    'Removes a packet hook (if one has been created)
    Public Function RemovePacketHook() As Boolean
        If hMgr.Equals(IntPtr.Zero) Then
            tabError(19930, "Tablet.Manager - Must set Tablet.Manager.Enabled = True before accessing manager functions")
            Exit Function
        End If
        If hHook.Equals(IntPtr.Zero) Then
            tabError(19936, "Tablet.Manager.RemovePacketHook - No packet hook set!")
        Else
            RemovePacketHook = IIf(WTMgrPacketUnHook(hHook) = 0, False, True)
        End If
    End Function
End Class
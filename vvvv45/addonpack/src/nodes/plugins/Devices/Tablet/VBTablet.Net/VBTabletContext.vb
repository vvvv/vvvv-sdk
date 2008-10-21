Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletContext_NET.TabletContext")> Public Class TabletContext
	'"Imagine all the people, sharing all the world . . ." - John Lennon - Imagine
	'VBTablet TabletContext class - ©2001 Laurence Parry
	'This class describes a context. It's members can be set publicly
	'(although if you attempt to update it while in use, it will check
	'the return value of WTSet() and the update may fail).
	'Reference to the internal structure is restricted to the project
	
    Private pCtxDesc As LOGCONTEXT 'Context descriptor
	Private clsEnabled As Boolean
    Private hContext As IntPtr 'Context handle - known as hCtx outside
	Private IsOnTop As Boolean
	Private Status As Integer 'Status (as of last packet)
	Public Report As TabletContextMask 'Data items that should
	'be reported by the device. If ones that don't exist are set
	'opening the context may fail. My experience is that it works
	'and you just get 0 for those variables, but be careful . . .
	
	Public MoveEvents As TabletContextMask 'If set, these items
	'will cause a move event. Buttons, time, serial number and any
	'not set to be reported will be ignored.
	
	Public Relative As TabletContextMask 'Those items that should
	'be reported in relative terms rather than absolute. Ignored for
	'serial number and any not set to be reported.
	
	Public Options As TabletContextOptions 'General context options
	
	Public Locked As TabletContextLocks 'Locked context items.
	'Locked items may not be changed by the user, only by the app.
	
	'Sets the context status (from the last packet recieved)
	Friend WriteOnly Property pktStatus() As Integer
		Set(ByVal Value As Integer)
			Status = Value
		End Set
	End Property
	
	'Returns true if the cursor is in context
	Public ReadOnly Property CursorInContext() As Boolean
		Get
            Return Not CBool(Status And TPS_PROXIMITY) 'this *is* right ;-)
		End Get
	End Property
	
	'Returns true if the queue has overflowed (note: doesn't always work)
	Public ReadOnly Property QueueFull() As Boolean
		Get
            Return CBool(Status And TPS_QUEUE_ERR)
		End Get
	End Property
	
	'Returns true if the cursor is in the context margin
	Public ReadOnly Property CursorInMargin() As Boolean
		Get
            Return CBool(Status And TPS_MARGIN)
		End Get
	End Property
	
	'Returns true if the cursor has been grabbed (out of context, but
	'waiting for a button to be released)
	Public ReadOnly Property CursorIsGrabbed() As Boolean
		Get
            Return CBool(Status And TPS_GRAB)
		End Get
	End Property
	
	'Returns true if the cursor is inverted
	Public ReadOnly Property CursorIsInverted() As Boolean
		Get
            Return CBool(Status And TPS_INVERT)
		End Get
	End Property
	
	'Gets the packet queue size (how many packets will be queued before overflow)
	'Sets packet queue size. If you set it too high, you will waste space,
	'or the call will fail (in which case VBTablet will try smaller sizes
	'until the call suceeds).
	Public Property QueueSize() As Integer
		Get
            If hContext.Equals(IntPtr.Zero) Then
                tabError(19900, "Cannot retrieve queue size - no tablet context has been created.")
            Else
                QueueSize = WTQueueSizeGet(hContext)
            End If
        End Get
        Set(ByVal Value As Integer)
            If hContext.Equals(IntPtr.Zero) Then
                tabError(19901, "Cannot set queue size - no tablet context has been created.")
            Else
                Value = System.Math.Abs(Value) 'no negatives
                'The programmer may call with a very large value here.
                'If it's not honoured, the queue will be destroyed,
                'so the procedure must continue until a non-zero value is returned
                Do While WTQueueSizeSet(hContext, Value) = 0
                    Value = Value \ 2
                    If Value = 1 Then 'shouldn't happen . . . but . . .
                        '            pTablet.Disconnect
                        '            pTablet.Connect
                        tabError(19902, "Failed utterly to set tablet packet queue size.")
                    End If
                Loop
            End If
        End Set
    End Property

    'True if the context is at the top

    'Sets the Ontop property. Because this is a Friend/Public pair, you
    'should not add a description of this in Property Attributes (VB bug)
    Public Property OnTop() As Boolean
        Get
            Return IsOnTop
        End Get
        Set(ByVal Value As Boolean)
            IsOnTop = Value
        End Set
    End Property

    'Returns the context handle for this context, used by WinTab functions

    'Sets the context handle for this context. Don't do this unless you
    '*really* know what you're doing (you might want to call Reload after)
    Public Property hCtx() As IntPtr
        Get
            hCtx = hContext
        End Get
        Set(ByVal Value As IntPtr)
            hContext = Value
        End Set
    End Property

    'Get the data required to restore the context at a later date
    'Will not work on the default context

    'Restores a previously saved context (which will be disabled)
    Public Property RawData() As Byte()
        Get
            IsSupported(WTInfo(WTI_INTERFACE, IFC_CTXSAVESIZE, tmpl))
            If tmpl <> 0 Then
                ReDim RawData(tmpl)
                WTSave(hContext, RawData)
            End If
            'Added by Languard
            'Correcting compile warning
            Return Nothing
        End Get
        Set(ByVal Value As Byte())
            IsSupported(WTInfo(WTI_INTERFACE, IFC_CTXSAVESIZE, tmpl))
            If tmpl <> 0 Then
                If Not Len(Value) = tmpl Then
                    tabError(19912, "Incorrect length passed to Tablet.Context.RawData (correct is " & tmpl & "). Use Tablet.Interface.SaveLen to get the correct size.")
                Else
                    tmpl = WTRestore(hContext, Me.RawData, False) 'I'd like to make it optional, but that's not allowed with property pairs (no longer true?)
                    If tmpl = 0 Then
                        tabError(19913, "Tablet.Context.RawData - could not restore tablet context from supplied context data.")
                    End If
                End If
            End If
        End Set
    End Property

    'Returns the context descriptor. Only works within the project.
    Friend Property hCtxDesc() As LOGCONTEXT
        Get
            Return pCtxDesc
        End Get
        Set(ByVal Value As LOGCONTEXT)
            pCtxDesc = Value
        End Set
    End Property

    ''Returns a pointer to the context descriptor. Also private.
    '   Friend ReadOnly Property pContext() As Integer
    '       Get
    '           'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1040"'
    '           Return VarPtr(pCtxDesc)
    '       End Get
    '   End Property

    'Returns the context's name

    'Sets the context's name
    Public Property Name() As String
        Get
            'System.Text.ASCIIEncoding.ASCII.GetString(pCtxDesc.lcName)
            Return pCtxDesc.lcName
        End Get
        Set(ByVal Value As String)
            pCtxDesc.lcName = Value.Substring(0, Math.Min(40, Value.Length))
            'tmpl = Len(Value)
            'For tmp = 0 To LCNAMELEN - 1
            '    If tmp = tmpl Then
            '        pCtxDesc.lcName(tmp) = 0
            '        Exit Property
            '    Else
            '        pCtxDesc.lcName(tmp) = Asc(Mid(Me.Name, tmp + 1, 1))
            '    End If
            'Next tmp
            'pCtxDesc.lcName(LCNAMELEN - 1) = 0 'null terminate to ward off evil spirits
        End Set
    End Property

    'Returns the packet rate (packets sent/sec)

    'Sets the packet rate (packets sent/sec)
    Public Property PacketRate() As Integer
        Get
            PacketRate = pCtxDesc.lcPktRate
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcPktRate = System.Math.Abs(Value)
        End Set
    End Property

    'Sets the origin of the contex's input x-axis (absolute mode)


    'Returns the origin of the contex's input x-axis (absolute mode)
    Public Property InputOriginX() As Integer
        Get
            InputOriginX = pCtxDesc.lcInOrgX
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcInOrgX = Value
        End Set
    End Property

    'Sets the origin of the contex's input y-axis (absolute mode)

    'Returns the origin of the contex's input y-axis (absolute mode)
    Public Property InputOriginY() As Integer
        Get
            InputOriginY = pCtxDesc.lcInOrgY
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcInOrgY = Value
        End Set
    End Property

    'Sets the origin of the contex's input z-axis (absolute mode)

    'Returns the origin of the contex's input z-axis (absolute mode)
    Public Property InputOriginZ() As Integer
        Get
            InputOriginZ = pCtxDesc.lcInOrgZ
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcInOrgZ = Value
        End Set
    End Property

    'Sets the origin of the contex's output x-axis (absolute mode)


    'Returns the origin of the contex's output x-axis (absolute mode)
    Public Property OutputOriginX() As Integer
        Get
            OutputOriginX = pCtxDesc.lcOutOrgX
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcOutOrgX = Value
        End Set
    End Property

    'Sets the origin of the contex's output y-axis (absolute mode)

    'Returns the origin of the contex's output y-axis (absolute mode)
    Public Property OutputOriginY() As Integer
        Get
            OutputOriginY = pCtxDesc.lcOutOrgY
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcOutOrgY = Value
        End Set
    End Property

    'Sets the origin of the contex's output z-axis (absolute mode)

    'Returns the origin of the contex's output z-axis (absolute mode)
    Public Property OutputOriginZ() As Integer
        Get
            OutputOriginZ = pCtxDesc.lcOutOrgZ
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcOutOrgZ = Value
        End Set
    End Property

    'Sets the size of the contex's input x-axis (absolute mode)

    'Returns the size of the contex's input x-axis (absolute mode)
    Public Property InputExtentX() As Integer
        Get
            InputExtentX = pCtxDesc.lcInExtX
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcInExtX = Value
        End Set
    End Property

    'Sets the size of the contex's input y-axis (absolute mode)

    'Returns the size of the contex's input y-axis (absolute mode)
    Public Property InputExtentY() As Integer
        Get
            InputExtentY = pCtxDesc.lcInExtY
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcInExtY = Value
        End Set
    End Property

    'Sets the size of the contex's input z-axis (absolute mode)

    'Returns the size of the contex's input z-axis (absolute mode)
    Public Property InputExtentZ() As Integer
        Get
            InputExtentZ = pCtxDesc.lcInExtZ
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcInExtZ = Value
        End Set
    End Property

    'Sets the size of the contex's output x-axis (absolute mode)

    'Returns the size of the contex's output x-axis (absolute mode)
    Public Property OutputExtentX() As Integer
        Get
            OutputExtentX = pCtxDesc.lcOutExtX
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcOutExtX = Value
        End Set
    End Property

    'Sets the size of the contex's output y-axis (absolute mode)

    'Returns the size of the contex's output y-axis (absolute mode)
    Public Property OutputExtentY() As Integer
        Get
            OutputExtentY = pCtxDesc.lcOutExtY
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcOutExtY = Value
        End Set
    End Property

    'Sets the size of the contex's output z-axis (absolute mode)

    'Returns the size of the contex's output z-axis (absolute mode)
    Public Property OutputExtentZ() As Integer
        Get
            OutputExtentZ = pCtxDesc.lcOutExtZ
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcOutExtZ = Value
        End Set
    End Property

    'Sets relative-mode X-axis sensitivity

    'Returns relative-mode X-axis sensitivity
    Public Property SensitivityX() As Integer
        Get
            SensitivityX = pCtxDesc.lcSensX
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcSensX = Value
        End Set
    End Property

    'Sets relative-mode Y-axis sensitivity
    'Returns relative-mode Y-axis sensitivity
    Public Property SensitivityY() As Integer
        Get
            SensitivityY = pCtxDesc.lcSensY
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcSensY = Value
        End Set
    End Property

    'Sets relative-mode Z-axis sensitivity
    'Returns relative-mode Z-axis sensitivity
    Public Property SensitivityZ() As Integer
        Get
            SensitivityZ = pCtxDesc.lcSensZ
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcSensZ = Value
        End Set
    End Property

    'Sets the tracking mode. False is absolute, True is relative

    'Returns the system cursor tracking mode. False is absolute, True is relative
    Public Property TrackingMode() As Boolean
        Get
            If pCtxDesc.lcSysMode = 0 Then
                TrackingMode = False
            Else
                TrackingMode = True
            End If
        End Get
        Set(ByVal Value As Boolean)
            pCtxDesc.lcSysMode = CInt(Value)
        End Set
    End Property

    'Sets the X-origin of the system screen mapping area
    '(top-left, usually), in screen coordinates

    'Returns the X-origin of the system screen mapping area
    '(top-left, usually), in screen coordinates
    Public Property SysOriginX() As Integer
        Get
            SysOriginX = pCtxDesc.lcSysOrgX
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcSysOrgX = Value
        End Set
    End Property

    'Sets the Y-origin of the system screen mapping area
    '(top-left, usually), in screen coordinates

    'Returns the Y-origin of the system screen mapping area
    '(top-left, usually), in screen coordinates
    Public Property SysOriginY() As Integer
        Get
            SysOriginY = pCtxDesc.lcSysOrgY
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcSysOrgY = Value
        End Set
    End Property

    'Sets the X-extent of the system screen mapping area, in screen coordinates

    'Returns the X-extent of the system screen mapping area, in screen coordinates
    Public Property SysExtentX() As Integer
        Get
            SysExtentX = pCtxDesc.lcSysExtX
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcSysExtX = Value
        End Set
    End Property

    'Sets the Y-extent of the system screen mapping area, in screen coordinates

    'Returns the Y-extent of the system screen mapping area, in screen coordinates
    Public Property SysExtentY() As Integer
        Get
            SysExtentY = pCtxDesc.lcSysExtY
        End Get
        Set(ByVal Value As Integer)
            pCtxDesc.lcSysExtY = Value
        End Set
    End Property

    'Sets the X-sensitivity of the system cursor when in relative mode

    'Returns the X-sensitivity of the system cursor when in relative mode
    Public Property SysSensitivityX() As Single
        Get
            Dim sensitivity As FIX32
            sensitivity.All = pCtxDesc.lcSysSensX
            SysSensitivityX = Single.Parse(sensitivity.High & "." & sensitivity.Low)
        End Get
        Set(ByVal Value As Single)
            Dim sensitivity As FIX32
            sensitivity.High = CShort(Value)
            sensitivity.Low = CShort(Value - sensitivity.High)
            pCtxDesc.lcSysSensX = sensitivity.All
        End Set
    End Property

    'Sets the Y-sensitivity of the system cursor when in relative mode

    'Returns the Y-sensitivity of the system cursor when in relative mode
    Public Property SysSensitivityY() As Single
        Get
            Dim sensitivity As FIX32
            sensitivity.All = pCtxDesc.lcSysSensY
            SysSensitivityY = Single.Parse(sensitivity.High & "." & sensitivity.Low)
        End Get
        Set(ByVal Value As Single)
            Dim sensitivity As FIX32
            sensitivity.High = CShort(Value)
            sensitivity.Low = CShort(Value - sensitivity.High)
            pCtxDesc.lcSysSensY = sensitivity.All
        End Set
    End Property

    'Sets if button release events will be sent for the specified button

    'Returns true if button release events will be sent for the specified button
    Public Property ButtonUp(ByVal Button As Byte) As Boolean
        Get
            If Button > 31 Then 'Button can't exist
                ButtonUp = False
            Else
                ButtonUp = CBool(pCtxDesc.lcBtnUpMask And CInt(2 ^ Button))
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Button < 32 Then 'Otherwise button can't exist
                If Value Then
                    pCtxDesc.lcBtnUpMask = pCtxDesc.lcBtnUpMask Or CInt(2 ^ Button)
                Else
                    pCtxDesc.lcBtnUpMask = pCtxDesc.lcBtnUpMask And Not CInt(2 ^ Button)
                End If
            End If
        End Set
    End Property

    'Sets if button press events will be sent for the specified button

    'Returns true if button press events will be sent for the specified button
    Public Property ButtonDown(ByVal Button As Short) As Boolean
        Get
            If Button > 31 Then 'Button can't exist
                ButtonDown = False
            Else
                ButtonDown = CBool(pCtxDesc.lcBtnDnMask And CInt(2 ^ Button))
            End If
        End Get
        Set(ByVal Value As Boolean)
            If Button < 32 Then 'Otherwise button can't exist
                If Value Then
                    pCtxDesc.lcBtnDnMask = pCtxDesc.lcBtnDnMask Or CInt(2 ^ Button)
                Else
                    pCtxDesc.lcBtnDnMask = pCtxDesc.lcBtnDnMask And Not CInt(2 ^ Button)
                End If
            End If
        End Set
    End Property

    'Returns the extension data specific to this context
    'as a 0-based byte array

    'Sets the context-specific data for an extension. ExtData should
    'be a 0-based byte array of the same length as the default
    'data for a system or digitising context, as appropriate
    '(you could just get ExtensionData and then use it as a template)
    Public Property ExtensionData(ByVal Extension As Integer) As Byte()
        Get
            'Added by Languard
            'Unused local var
            'Dim previndex As Integer
            Dim ID As Integer
            Dim tmpb As Byte()
            If Extension < 0 Or Extension > 99 Then
                tabError(19918, "Tablet.Context.ExtensionData - Invalid extension number (" & Extension & ")")
                'correcting compiler warning - Languard
                Return Nothing
            Else

                WTInfo(WTI_EXTENSIONS + Extension - 1, EXT_TAG, ID)
                If ID = 5 Then
                    tmpl = 4 * 2
                ElseIf Me.Options.IsSystemCtx Then
                    tmpl = WTInfo(WTI_EXTENSIONS + (Extension - 1), EXT_DEFSYSCTX, vbNullString)
                Else
                    tmpl = WTInfo(WTI_EXTENSIONS + (Extension - 1), EXT_DEFCONTEXT, vbNullString)
                End If
                'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tmpb(tmpl - 1)

                IsSupported(WTExtGet(hCtx, ID, tmpb))
                'UPGRADE_WARNING: Couldn't resolve default property of object ExtensionData. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                Return tmpb
            End If
        End Get
        Set(ByVal Value As Byte())
            'correcting unused local var - Languard
            'Dim previndex As Integer
            Dim ID As Integer
            If Extension < 0 Or Extension > 99 Then
                tabError(19918, "Tablet.Context.ExtensionData - Invalid extension number (" & Extension & ")")
            Else
                WTInfo(WTI_EXTENSIONS + Extension, EXT_TAG, ID)
                'UPGRADE_WARNING: Couldn't resolve default property of object ExtensionData(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                IsSupported(WTExtSet(hCtx, ID, Value))
            End If
        End Set
    End Property

    'Returns true if this context is enabled (should send information)

    'Sets the context to being enabled or not
    Public Property Enabled() As Boolean
        Get
            Return clsEnabled
        End Get
        Set(ByVal Value As Boolean)
            If 0 <> WTEnable(hContext, Value) Then
                clsEnabled = Value
            Else
                System.Diagnostics.Debug.WriteLine("Context enable/disable failed")
            End If
        End Set
    End Property

    'Update the tablet driver with the local version of the context.
    'Make your changes to the values through the properties, then call this.
    Public Sub Update()
        pCtxDesc.lcPktData = Report.MaskValue
        pCtxDesc.lcMoveMask = MoveEvents.MaskValue
        pCtxDesc.lcPktMode = Relative.MaskValue
        pCtxDesc.lcOptions = Options.MaskValue
        tmpl = WTSet(hContext, pCtxDesc)
        If tmpl = 0 Then System.Diagnostics.Debug.WriteLine("Context.Update failed . . .")
    End Sub

    'Update the local context with the current context from the tablet driver.
    'Applications should call this before reading any context information.
    Public Sub Reload()
        WTGet(hContext, pCtxDesc)
        Report.MaskValue = pCtxDesc.lcPktData
        MoveEvents.MaskValue = pCtxDesc.lcMoveMask
        Relative.MaskValue = pCtxDesc.lcPktMode
        Options.MaskValue = pCtxDesc.lcOptions
    End Sub

    'Reposition the tablet context to the top or to the bottom of the stack.
    'Certain devices (eg Wacom tablets) will do this for your application
    'but don't count on it - you should reposition your contexts when
    'your application gains and loses focus
    Public Function Reposition(ByRef OnTop As Boolean) As Boolean
        If Not hContext.Equals(IntPtr.Zero) Then
            Return WTOverlap(hContext, OnTop)
        Else
            tabError(19906, "Can't reposition without a valid tablet context")
        End If
    End Function

    'Opens a context using the current description
    Public Function OpenContext(ByRef Enable As Boolean) As Integer
        If Not hContext.Equals(IntPtr.Zero) Then Me.CloseContext()
        'UPGRADE_WARNING: Couldn't resolve default property of object pCtxDesc. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
        hContext = WTOpen(pTablet.hWnd, pCtxDesc, Enable)
        If hContext.Equals(IntPtr.Zero) Then
            'fallback settings - some tablets don't open if unsupported features are specified
            pCtxDesc.lcPktData = pCtxDesc.lcPktData And Not (PK_Z Or PK_NORMAL_PRESSURE Or PK_TANGENT_PRESSURE Or PK_ORIENTATION Or PK_ROTATION)
            pCtxDesc.lcMoveMask = pCtxDesc.lcMoveMask And Not (PK_Z Or PK_NORMAL_PRESSURE Or PK_TANGENT_PRESSURE Or PK_ORIENTATION Or PK_ROTATION)
            'UPGRADE_WARNING: Couldn't resolve default property of object pCtxDesc. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
            hContext = WTOpen(pTablet.hWnd, pCtxDesc, Enable)
            If hContext.Equals(IntPtr.Zero) Then 'give up
                clsEnabled = False
                tabError(19921, "Tablet.Context.OpenContext failed")
                Exit Function
            End If
        End If
        Enabled = Enable
        Reload() 'update the settings - the WinTab driver may have changed them
    End Function

    'Close this context
    Public Function CloseContext() As Integer
        Enabled = False
        CloseContext = WTClose(hContext)
        If CloseContext <> 0 Then
            hContext = IntPtr.Zero
        End If
    End Function
    'Have to do it here, otherwise it doesn't work (can't access pCtxDesc
    'in any way). Annoying.
    Friend Function GetContext(ByRef ID As Integer, ByRef opt As Integer) As Integer
        If Not hContext.Equals(IntPtr.Zero) Then Me.CloseContext()
        'UPGRADE_WARNING: Couldn't resolve default property of object pCtxDesc. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
        tmpl = WTInfo(ID, opt, pCtxDesc)
        If tmpl <> 0 Then
            'hContext = GetContext
        Else
            System.Diagnostics.Debug.WriteLine("Failed to get default context")
        End If
        Report.MaskValue = pCtxDesc.lcPktData
        MoveEvents.MaskValue = pCtxDesc.lcMoveMask
        Relative.MaskValue = pCtxDesc.lcPktMode
        Options.MaskValue = pCtxDesc.lcOptions
    End Function

    'Shows a dialog for user editing of the context. Returns true if
    'the context was modified (Context.Reload will be called for you)
    Public Function ShowConfigDlg(ByRef hWnd As IntPtr) As Boolean
        If hContext.Equals(IntPtr.Zero) Then Exit Function
        ShowConfigDlg = Not CBool(WTConfig(hContext, hWnd))
        If ShowConfigDlg Then Reload()
    End Function

    Friend Sub InitSettings()
        pCtxDesc.lcOptions = pCtxDesc.lcOptions Or CXO_MESSAGES Or CXO_CSRMESSAGES 'receive packets as messages
        pCtxDesc.lcMsgBase = WT_DEFBASE 'establish message base number
        pCtxDesc.lcPktData = PK_STATUS Or PK_CURSOR Or PK_BUTTONS Or PK_X Or PK_Y Or PK_Z Or PK_NORMAL_PRESSURE Or PK_TANGENT_PRESSURE Or PK_ORIENTATION Or PK_ROTATION Or PK_TIME Or PK_SERIAL_NUMBER
        pCtxDesc.lcPktMode = 0 'PK_BUTTONS Or PK_X Or PK_Y Or PK_NORMAL_PRESSURE                     'report everything but buttons in absolute coordinates
        pCtxDesc.lcMoveMask = PK_CURSOR Or PK_STATUS Or PK_BUTTONS Or PK_X Or PK_Y Or PK_Z Or PK_NORMAL_PRESSURE Or PK_TANGENT_PRESSURE Or PK_ORIENTATION Or PK_ROTATION
        pCtxDesc.lcBtnDnMask = SBN_LCLICK Or PK_STATUS Or SBN_RCLICK Or SBN_LDBLCLICK Or SBN_RDBLCLICK
        pCtxDesc.lcBtnUpMask = pCtxDesc.lcBtnDnMask
        pCtxDesc.lcSysMode = 0 'absolute
        'pCtxDesc.lcSensX = 1
        'pCtxDesc.lcSensY = 1
        'pCtxDesc.lcSensZ = 1
        'pCtxDesc.lcInExtX = pTablet.Device.X.Max
        'pCtxDesc.lcInExtY = pTablet.Device.Y.Max
        'pCtxDesc.lcInExtZ = 0
        'pCtxDesc.lcInOrgX = 0
        'pCtxDesc.lcInOrgY = 0
        'pCtxDesc.lcInOrgZ = 0
        'pCtxDesc.lcOutExtX = pTablet.Device.X.Max
        'pCtxDesc.lcOutExtY = pTablet.Device.Y.Max
        'pCtxDesc.lcOutExtZ = 0
        'pCtxDesc.lcOutOrgX = 0
        'pCtxDesc.lcOutOrgY = 0
        'pCtxDesc.lcOutOrgZ = 0
        'WTInfo devindex, DVC_PKTRATE, tmpl
        'pCtxDesc.lcPktRate = tmpl
    End Sub

    Public Sub New()
        MyBase.New()
        pCtxDesc.Initialize()
        Report = New TabletContextMask
        MoveEvents = New TabletContextMask
        Relative = New TabletContextMask
        Options = New TabletContextOptions
        Locked = New TabletContextLocks
    End Sub
End Class
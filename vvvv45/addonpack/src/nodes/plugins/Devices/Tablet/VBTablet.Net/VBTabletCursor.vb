Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletCursor_NET.TabletCursor")> Public Class TabletCursor
	'"I met a girl who sang the blues, and asked her for some happy news - but she just smiled, and turned away . . ." - Don McClean - American Pie
	'VBTablet TabletCursor class - ©2001 Laurence Parry
	'Describes the cursors supported by the WinTab interface
	Public AvailableData As TabletDeviceMask 'Wow! Code reuse! ;-)
	'The data returned from this cursor
	Public MinAggregateData As TabletDeviceMask 'The minimum set of data
	'values available from this cursor (if it is an aggregate cursor)
	Private curindex As Integer
	Private tmpb() As Byte
	Private tmpla() As Integer
	
    'Set the index
	
	'Get the index
	Public Property Index() As Integer
		Get
            Index = curindex - WTI_CURSORS
		End Get
		Set(ByVal Value As Integer)
			tmpl = 0
			WTInfo(WTI_INTERFACE, IFC_NCURSORS, tmpl)
            If tmpl <= Value Or Value < 0 Then
                If tmpl = 0 Then
                    tabError(19910, "Invalid cursor number (" & Value & ") - there are no cursors available at this time")
                Else
                    tabError(19910, "Invalid cursor number (" & Value & ") - use Tablet.Interface.NumCursors to retrieve the number of cursors available")
                End If
                Exit Property
            Else
                curindex = Value + WTI_CURSORS
            End If
            WTInfo(curindex, CSR_PKTDATA, tmpl)
            AvailableData.MaskValue = tmpl
            WTInfo(curindex, CSR_MINPKTDATA, tmpl)
            MinAggregateData.MaskValue = tmpl
		End Set
	End Property
	
	'Returns the current cursor's name
	Public ReadOnly Property Name() As String
		Get
            tmpl = WTInfo(curindex, CSR_NAME, refstr)
            If tmpl = 0 Then
                Return ""
                IsSupported(0)
            Else
                Return refstr.Substring(0, tmpl - 1)
            End If
		End Get
	End Property
	
	'Returns true if the cursor is currently connected ("active")
	Public ReadOnly Property IsConnected() As Boolean
		Get
            IsSupported(WTInfo(curindex, CSR_ACTIVE, tmpl))
            IsConnected = CBool(tmpl)
		End Get
	End Property
	
	'Returns the number of buttons on this cursor
	Public ReadOnly Property NumButtons() As Integer
		Get
			IsSupported(WTInfo(curindex, CSR_BUTTONS, NumButtons))
		End Get
	End Property
	
	'Returns the number of button info bits returned by this cursor
	Public ReadOnly Property NumButtonBits() As Integer
		Get
			IsSupported(WTInfo(curindex, CSR_BUTTONBITS, NumButtonBits))
		End Get
	End Property
	
	'Returns the button names for this cursor. Specifying <=0 will return
	'*all* names in the original format (a null after each, then another
	'null at the end). Passing button > 0 gives the button at that position
	'(without null character), or an empty string if there are no more buttons left
	Public ReadOnly Property ButtonName(ByVal Button As Integer) As String
		Get
			Dim finalpos As Integer
			finalpos = WTInfo(curindex, CSR_BTNNAMES, refstr)
			Dim startpos As Integer
			Dim endpos As Integer
			If finalpos = 0 Then
				ButtonName = ""
				IsSupported(0)
			Else
				If Button > 0 Then
					tmpl = 0
					Do 
						tmpl = tmpl + 1
						startpos = endpos + 1
						endpos = InStr(startpos, refstr, vbNullChar, CompareMethod.Binary)
                        If finalpos - endpos = 0 Then
                            'correcting compiler warning - Languard
                            Return "" 'button not there
                        End If
					Loop While tmpl < Button
                    ButtonName = refstr.Substring(startpos, endpos - startpos)
				Else
                    ButtonName = refstr.Substring(0, finalpos)
				End If
			End If
		End Get
	End Property
	
	'Maps physical buttons to logical buttons. If you pass a number
	'from 1-32, it returns the mapping for that button, otherwise
	'it returns all mappings in a byte array
	
    Property PhysicalMapping() As Byte()
        Get
            ReDim PhysicalMapping(31)
            Debug.WriteLine("FIXME: Getting TabletCursor.PhysicalMapping breaks the engine")
            'IsSupported(WTInfo(curindex, CSR_BUTTONMAP, PhysicalMapping))
            Return PhysicalMapping
        End Get
        Set(ByVal Value As Byte())
            'UPGRADE_WARNING: Lower bound of array physical was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            Dim physical(32) As Byte
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tmpb(31)
                IsSupported(WTInfo(curindex, CSR_SYSBTNMAP, tmpb))

                If (Not IsArray(Value)) Or UBound(Value) - LBound(Value) <> 31 Then Exit Property
                For tmp = 0 To 31
                    'UPGRADE_WARNING: Couldn't resolve default property of object Mapping(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    physical(tmp) = Value(LBound(Value) + tmp - 1)
                Next tmp

                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetCursorButtonMap(curindex - WTI_CURSORS + 1, physical, tmpb)
                            .Enabled = False
                        End If
                    Else
                        .SetCursorButtonMap(curindex - WTI_CURSORS + 1, physical, tmpb)
                    End If
                End With
            Else
                tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Maps logical buttons to action. If you pass a number from 1-32,
    'it returns the mapping for that button, otherwise it returns all
    'mappings in a byte array

    'Maps logical buttons to action. If you pass a number from 1-32,
    'it sets the mapping for that button, otherwise it sets all
    'mappings (which should be provided as a 32-byte array
    Property LogicalMapping() As Byte()
        Get
            'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            ReDim LogicalMapping(32)
            IsSupported(WTInfo(curindex, CSR_SYSBTNMAP, LogicalMapping))
            Return LogicalMapping
        End Get
        Set(ByVal Value As Byte())
            'UPGRADE_WARNING: Lower bound of array logical was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            Dim logical(31) As Byte
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tmpb(31)
                IsSupported(WTInfo(curindex, CSR_BUTTONMAP, tmpb))
                For tmp = 1 To 32
                    'UPGRADE_WARNING: Couldn't resolve default property of object Mapping(). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    logical(tmp) = Value(LBound(Value) + tmp - 1)
                Next tmp
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetCursorButtonMap(curindex - WTI_CURSORS, tmpb, logical)
                            .Enabled = False
                        End If
                    Else
                        .SetCursorButtonMap(curindex - WTI_CURSORS, tmpb, logical)
                    End If
                End With
            Else
            tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the (physical) number of the normal pressure button
    ReadOnly Property NormalPressureButton() As Integer
        Get
            IsSupported(WTInfo(curindex, CSR_NPBUTTON, NormalPressureButton))
        End Get
    End Property

    'Returns the press mark for the normal pressure button
    'If the pressure rises above this value the button is set to "down"

    'Sets the press mark for the normal pressure button
    Property NormalPressurePressMark() As Integer
        Get
            'UPGRADE_WARNING: Lower bound of array tmpla was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            ReDim tmpla(2)
            IsSupported(WTInfo(curindex, CSR_NPBTNMARKS, tmpla(1)))
            NormalPressurePressMark = tmpla(2)
        End Get
        Set(ByVal Value As Integer)
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetButtonMarks((Me.Index), Value, (Me.NormalPressureReleaseMark), (Me.TangentPressurePressMark), (Me.TangentPressureReleaseMark))
                            .Enabled = False
                        End If
                    Else
                        .SetButtonMarks((Me.Index), Value, (Me.NormalPressureReleaseMark), (Me.TangentPressurePressMark), (Me.TangentPressureReleaseMark))
                    End If
                End With
            Else
                tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the release mark for the normal pressure button
    'If the pressure falls below this value the button is set to "up"

    'Sets the release mark for the normal pressure button
    Property NormalPressureReleaseMark() As Integer
        Get
            'UPGRADE_WARNING: Lower bound of array tmpla was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            ReDim tmpla(2)
            IsSupported(WTInfo(curindex, CSR_NPBTNMARKS, tmpla(1)))
            NormalPressureReleaseMark = tmpla(1)
        End Get
        Set(ByVal Value As Integer)
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetButtonMarks((Me.Index), (Me.NormalPressurePressMark), Value, (Me.TangentPressurePressMark), (Me.TangentPressureReleaseMark))
                            .Enabled = False
                        End If
                    Else
                        .SetButtonMarks((Me.Index), (Me.NormalPressurePressMark), Value, (Me.TangentPressurePressMark), (Me.TangentPressureReleaseMark))
                    End If
                End With
            Else
                tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the normal pressure curve as a variant array of longs
    '(will be Empty if not supported on this cursor/device)
    'change by Languard: will return Nothing (null) if not supported

    'Sets the normal pressure curve, which should be passed as a
    'variant array of longs (or Null to reset)
    Property NormalPressureCurve() As Object
        'Changes by Languard
        'correcting compiler warning on code paths
        'reformating to match .NET conventions
        Get
            Dim result As Object
            tmpl = WTInfo(curindex, CSR_NPRESPONSE, 0)
            If tmpl = 0 Then
                IsSupported(0)
                result = Nothing
            Else
                'UPGRADE_WARNING: Lower bound of array tmpla was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tmpla(tmpl \ 4)
                WTInfo(curindex, CSR_NPRESPONSE, tmpla(1))
                'UPGRADE_WARNING: Couldn't resolve default property of object NormalPressureCurve. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                'result = VB6.CopyArray(tmpla)
                result = tmpla
                'For tmp = 1 To tmpl \ 4
                '    Debug.Print tmpla(tmp)
                'Next tmp
            End If
            Return result
        End Get
        Set(ByVal Value As Object)
            Dim TangentPressure As Object
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
                If Not IsArray(Value) And Not IsDBNull(Value) Then
                    tabError(19932, "Tablet.Cursor - Pressure curve must be passed as a long array or Null")
                Else
                    'UPGRADE_WARNING: Couldn't resolve default property of object Me.TangentPressureCurve. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object TangentPressure. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    TangentPressure = Me.TangentPressureCurve
                    'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1041"'
                    'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object TangentPressure. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    If IsNothing(TangentPressure) Then TangentPressure = System.DBNull.Value
                End If
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetPressureCurve((Me.Index), Value, TangentPressure)
                            .Enabled = False
                        End If
                    Else
                        .SetPressureCurve((Me.Index), Value, TangentPressure)
                    End If
                End With
            Else
                tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the (physical) number of the tangential/barrel pressure button
    ReadOnly Property TangentPressureButton() As Integer
        Get
            IsSupported(WTInfo(curindex, CSR_TPBUTTON, TangentPressureButton))
        End Get
    End Property

    'Returns the press mark for the tangential pressure button.
    'If the pressure rises above this value the button is set to "down".

    'Sets the press mark for the tangential pressure button.
    Property TangentPressurePressMark() As Integer
        Get
            'UPGRADE_WARNING: Lower bound of array tmpla was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            ReDim tmpla(2)
            IsSupported(WTInfo(curindex, CSR_TPBTNMARKS, tmpla(1)))
            TangentPressurePressMark = tmpla(2)
        End Get
        Set(ByVal Value As Integer)
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetButtonMarks((Me.Index), (Me.NormalPressurePressMark), (Me.NormalPressureReleaseMark), Value, (Me.TangentPressureReleaseMark))
                            .Enabled = False
                        End If
                    Else
                        .SetButtonMarks((Me.Index), (Me.NormalPressurePressMark), (Me.NormalPressureReleaseMark), Value, (Me.TangentPressureReleaseMark))
                    End If
                End With
            Else
                tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the release mark for the tangential pressure button
    'If the pressure falls below this value the button is set to "up".

    'Sets the release mark for the tangential pressure button
    Property TangentPressureReleaseMark() As Integer
        Get
            'UPGRADE_WARNING: Lower bound of array tmpla was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            ReDim tmpla(2)
            IsSupported(WTInfo(curindex, CSR_TPBTNMARKS, tmpla(1)))
            TangentPressureReleaseMark = tmpla(1)
        End Get
        Set(ByVal Value As Integer)
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetButtonMarks((Me.Index), (Me.NormalPressurePressMark), (Me.NormalPressureReleaseMark), (Me.TangentPressurePressMark), Value)
                            .Enabled = False
                        End If
                    Else
                        .SetButtonMarks((Me.Index), (Me.NormalPressurePressMark), (Me.NormalPressureReleaseMark), (Me.TangentPressurePressMark), Value)
                    End If
                End With
            Else
                tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the tangential pressure curve as a variant array of longs
    '(will be Empty if not supported on this cursor/device)

    'Sets the normal pressure curve, which should be passed as a
    'variant array of longs (or Null to reset)
    Property TangentPressureCurve() As Object
        Get
            tmpl = WTInfo(curindex, CSR_TPRESPONSE, 0)
            If tmpl = 0 Then
                IsSupported(0)
                TangentPressureCurve = Nothing
            Else
                'UPGRADE_WARNING: Lower bound of array tmpla was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tmpla(tmpl \ 4)
                WTInfo(curindex, CSR_TPRESPONSE, tmpla(1))
                'UPGRADE_WARNING: Couldn't resolve default property of object TangentPressureCurve. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                'Change by Languard
                'remove vb6 copy and replaced with clone
                TangentPressureCurve = tmpla.Clone
            End If
        End Get
        Set(ByVal Value As Object)
            Dim NormalPressure As Object
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
                If Not IsArray(Value) And Not IsDBNull(Value) Then
                    tabError(19932, "Tablet.Cursor - Pressure curve must be passed as a long array or Null")
                Else
                    'UPGRADE_WARNING: Couldn't resolve default property of object Me.NormalPressureCurve. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object NormalPressure. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    NormalPressure = Me.NormalPressureCurve
                    'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1041"'
                    'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1049"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object NormalPressure. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    If IsNothing(NormalPressure) Then NormalPressure = System.DBNull.Value
                End If
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19926, "Tablet.Cursor - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetPressureCurve((Me.Index), NormalPressure, Value)
                            .Enabled = False
                        End If
                    Else
                        .SetPressureCurve((Me.Index), NormalPressure, Value)
                    End If
                End With
            Else
                tabError(19925, "Tablet.Cursor - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the physical ID of the selected cursor (for separate tools)
    ReadOnly Property PhysicalID() As Integer
        Get
            IsSupported(WTInfo(curindex, CSR_PHYSID, PhysicalID))
        End Get
    End Property

    'Returns the physical cursor type
    ReadOnly Property PhysicalType() As Integer
        Get
            IsSupported(WTInfo(curindex, CSR_TYPE, PhysicalType))
        End Get
    End Property

    'Returns true if this cursor is an inverted form of another
    '(the cursor immediately previous)
    ReadOnly Property IsInverted() As Boolean
        Get
            IsSupported(WTInfo(curindex, CSR_CAPABILITIES, tmpl))
            Return CBool(tmpl And CRC_INVERT)
        End Get
    End Property

    'Returns true if this cursor is aggregate (formed from more than one
    'physical cursor)
    ReadOnly Property IsAggregate() As Boolean
        Get
            IsSupported(WTInfo(curindex, CSR_CAPABILITIES, tmpl))
            Return CBool(tmpl And CRC_AGGREGATE)
        End Get
    End Property

    'Returns true if this cursor is multimode (one of several cursors
    'used by a single physical cursor - the opposite of aggregate)
    ReadOnly Property IsMultiMode() As Boolean
        Get
            IsSupported(WTInfo(curindex, CSR_CAPABILITIES, tmpl))
            Return CBool(tmpl And CRC_MULTIMODE)
        End Get
    End Property

    'Returns the cursor mode (if this cursor is multimode)
    ReadOnly Property Mode() As Integer
        Get
            IsSupported(WTInfo(curindex, CSR_MODE, Mode))
        End Get
    End Property

    'Returns the mininum number of buttons that will be available
    '(if this is an aggregate cursor)
    ReadOnly Property MinAggregateButtons() As Integer
        Get
            IsSupported(WTInfo(curindex, CSR_MINBUTTONS, MinAggregateButtons))
        End Get
    End Property

    'Translates a physical cursor type to a string (if known)
    Public Function TranslatePhysicalType(ByRef PhysicalType As Integer) As String
        Select Case (PhysicalType And CSR_TYPE_TRANSLATIONFACTOR)
            Case CSR_TYPE_AIRBRUSH
                TranslatePhysicalType = "airbrush"
            Case CSR_TYPE_STYLUS
                TranslatePhysicalType = "stylus"
            Case CSR_TYPE_PUCK
                TranslatePhysicalType = "puck"
            Case CSR_TYPE_5PUCK
                TranslatePhysicalType = "5-button puck"
            Case CSR_TYPE_MOUSE
                TranslatePhysicalType = "mouse"
            Case CSR_TYPE_INTUOSSTYLUS
                TranslatePhysicalType = "stylus (Intuos)"
            Case CSR_TYPE_NONE
                TranslatePhysicalType = "non-connected/default device type"
            Case Else
                TranslatePhysicalType = "unknown device type"
        End Select
    End Function

    'Translates an emulated mouse action into a string
    Public Function TranslateButtonAction(ByRef ButtonAction As Integer) As String
        Select Case ButtonAction Mod &H10S
            Case SBN_NONE
                TranslateButtonAction = "(no action)"
            Case SBN_LCLICK
                TranslateButtonAction = "left button click"
            Case SBN_LDBLCLICK
                TranslateButtonAction = "left button double-click"
            Case SBN_LDRAG
                TranslateButtonAction = "left button drag"
            Case SBN_RCLICK
                TranslateButtonAction = "right button click"
            Case SBN_RDBLCLICK
                TranslateButtonAction = "right button double-click"
            Case SBN_RDRAG
                TranslateButtonAction = "right button drag"
            Case SBN_MCLICK
                TranslateButtonAction = "middle button click"
            Case SBN_MDBLCLICK
                TranslateButtonAction = "middle button double-click"
            Case SBN_MDRAG
                TranslateButtonAction = "middle button drag"
        End Select
    End Function

    'Translates a Pen Windows action into a string
    Public Function TranslatePenAction(ByRef PenAction As Integer) As String
        Select Case PenAction - (PenAction Mod &H10S)
            Case SBN_PTCLICK
                TranslatePenAction = "pen tip click"
            Case SBN_PTDBLCLICK
                TranslatePenAction = "pen tip double-click"
            Case SBN_PTDRAG
                TranslatePenAction = "pen tip drag"
            Case SBN_PNCLICK
                TranslatePenAction = "inverted pen tip click"
            Case SBN_PNDBLCLICK
                TranslatePenAction = "inverted pen tip double-click"
            Case SBN_PNDRAG
                TranslatePenAction = "inverted pen tip drag"
            Case SBN_P1CLICK
                TranslatePenAction = "barrel button 1 click"
            Case SBN_P1DBLCLICK
                TranslatePenAction = "barrel button 1 double-click"
            Case SBN_P1DRAG
                TranslatePenAction = "barrel button 1 drag"
            Case SBN_P2CLICK
                TranslatePenAction = "barrel button 2 click"
            Case SBN_P2DBLCLICK
                TranslatePenAction = "barrel button 2 double-click"
            Case SBN_P2DRAG
                TranslatePenAction = "barrel button 2 drag"
            Case SBN_P3CLICK
                TranslatePenAction = "barrel button 3 click"
            Case SBN_P3DBLCLICK
                TranslatePenAction = "barrel button 3 double-click"
            Case SBN_P3DRAG
                TranslatePenAction = "barrel button 3 drag"
        End Select
    End Function

    Public Sub New()
        MyBase.New()
        AvailableData = New TabletDeviceMask
        MinAggregateData = New TabletDeviceMask
        On Error GoTo A
        Me.Index = 1
        Return
A:
        Me.Index = 0
    End Sub
End Class
Option Strict On
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1043"'
<System.Runtime.InteropServices.ProgId("TabletExtension_NET.TabletExtension")> Public Class TabletExtension
	'"I'm breaking through, I'm bending spoons, I'm keeping flowers in full bloom - I'm looking for answers from the Great Beyond" - REM - The Great Beyond
	'VBTablet TabletExtension class - ©2001 Laurence Parry
	'This class returns information on the custom extensions to the
	'WinTab interface supported by this implementation. These are
	'usually hardware-specific additions (like the Wacom ToolID).
	'The number of extensions is available from Interface.NumExtensions()
	'Applications should check this number is not 0 and then set
	'the 1-based Index property.
	Private extindex As Integer
	Private ax() As tagAxis
	Private axmin() As Integer
	Private axmax() As Integer
	Private axunits() As Integer
    Private axresolution() As FIX32
	Public OrMask As TabletDeviceMask
	
	'Set the index - the extension to retrieve information on
	
	'Get the index (not sure if this is necessary . . .)
	Public Property Index() As Integer
		Get
			Index = extindex - WTI_EXTENSIONS + 1
		End Get
		Set(ByVal Value As Integer)
			tmpl = 0
			WTInfo(WTI_INTERFACE, IFC_NEXTENSIONS, tmpl)
			If tmpl < Value Or Value > 100 Or Value < 1 Then
				tabError(19910, "Invalid extension number - use Tablet.Interface.NumExtensions to retrieve the number of extensions available")
			Else
				extindex = Value - 1 + WTI_EXTENSIONS
				tmpl = 0
				WTInfo(extindex, EXT_MASK, tmpl)
				OrMask.MaskValue = tmpl
			End If
		End Set
	End Property
	
	'Returns the name of this extension
	Public ReadOnly Property Name() As String
		Get
			tmpl = WTInfo(extindex, EXT_NAME, refstr)
			If tmpl = 0 Then
				'Name = extindex
				Return ""
				IsSupported(0) 'signal an error if not supported
			Else
				'This may be *wrong*. Should be tmp - 1. I think Wacom messed this up.
				'Name = tmpl
				Return refstr
				'Name = Left$(refstr, tmpl - 1)
			End If
		End Get
	End Property
	
	'Returns this extension's tag ID
	Public ReadOnly Property ID() As Integer
		Get
			IsSupported(WTInfo(extindex, EXT_TAG, ID))
		End Get
	End Property
	
	'Returns the size of the extension for a context in absolute mode
	Public ReadOnly Property AbsoluteSize() As Integer
		Get
			Dim size(1) As Integer
			IsSupported(WTInfo(extindex, EXT_SIZE, size(0)))
			AbsoluteSize = size(0)
		End Get
	End Property
	
	'Returns the size of the extension for a context in relative mode
	Public ReadOnly Property RelativeSize() As Integer
		Get
			Dim size(1) As Integer
			IsSupported(WTInfo(extindex, EXT_SIZE, size(0)))
			RelativeSize = size(1)
		End Get
	End Property

    Public ReadOnly Property ExpKeyMask() As Short()
        Get
            Dim size(1) As Integer
            Dim sho(3) As Short
            IsSupported(WTInfo(extindex, EXT_SIZE, size(0)))
            Buffer.BlockCopy(size, 0, sho, 0, 8)
            ExpKeyMask = sho
        End Get
    End Property

	'Axes required by the extension. Returns as an array of four variants,
	'each containing an array of longs - Min, Max, Units and Resolution
	Public ReadOnly Property Axes() As Object
		Get
			tmpl = WTInfo(extindex, EXT_AXES, 0)
			'UPGRADE_WARNING: Lower bound of array AxesHolder was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
            Dim AxesHolder(3) As Object
			If tmpl = 0 Then
				IsSupported(0)
			Else
				'UPGRADE_WARNING: Lower bound of array ax was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
				ReDim ax(tmpl \ 16)
                tmpl = WTInfo(extindex, EXT_AXES, ax)
				'UPGRADE_WARNING: Lower bound of array axmin was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
				ReDim axmin(tmpl \ 4)
				'UPGRADE_WARNING: Lower bound of array axmax was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
				ReDim axmax(tmpl \ 4)
				'UPGRADE_WARNING: Lower bound of array axunits was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
				ReDim axunits(tmpl \ 4)
				'UPGRADE_WARNING: Lower bound of array axresolution was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
				ReDim axresolution(tmpl \ 4)
				For tmpl = 1 To tmpl \ 16
					axmin(tmpl) = ax(tmpl).Min
					axmax(tmpl) = ax(tmpl).Max
					axunits(tmpl) = ax(tmpl).Units
					axresolution(tmpl) = ax(tmpl).Resolution
                Next tmpl
                'Change by Languard
                'remove vb6 copy and replaced with clone
				'UPGRADE_WARNING: Couldn't resolve default property of object AxesHolder(1). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                AxesHolder(0) = axmin.Clone
				'UPGRADE_WARNING: Couldn't resolve default property of object AxesHolder(2). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                AxesHolder(1) = axmax.Clone
				'UPGRADE_WARNING: Couldn't resolve default property of object AxesHolder(3). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                AxesHolder(2) = axunits.Clone
				'UPGRADE_WARNING: Couldn't resolve default property of object AxesHolder(4). Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                AxesHolder(3) = axresolution.Clone
				'UPGRADE_WARNING: Couldn't resolve default property of object Axes. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                Axes = AxesHolder.Clone
			End If
		End Get
	End Property
	
	'Returns the default data for the extension as as byte array
	
	Public Property DefaultData() As Object
		Get
			tmpl = WTInfo(extindex, EXT_DEFAULT, 0)
			If tmpl = 0 Then
				IsSupported(0)
			Else
				'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
				ReDim tmpb(tmpl)
                tmpl = WTInfo(extindex, EXT_DEFAULT, tmpb)
				'UPGRADE_WARNING: Couldn't resolve default property of object DefaultData. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                DefaultData = tmpb.Clone
            End If
        End Get
        Set(ByVal Value As Object)
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19928, "Tablet.Extension - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetExtData((Me.ID), Value)
                            .Enabled = False
                        End If
                    Else
                        .SetExtData((Me.ID), Value)
                    End If
                End With
            Else
                tabError(19927, "Tablet.Extension - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property

    'Returns the default system context data as a byte array
    Public ReadOnly Property DefaultSystemData() As Object
        Get
            tmpl = WTInfo(extindex, EXT_DEFSYSCTX, 0)
            If tmpl = 0 Then
                IsSupported(0)
            Else
                'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                ReDim tmpb(tmpl)
                tmpl = WTInfo(extindex, EXT_DEFSYSCTX, tmpb)
                'UPGRADE_WARNING: Couldn't resolve default property of object DefaultSystemData. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                DefaultSystemData = tmpb.Clone
            End If
        End Get
    End Property
	
	'Returns the default digitizing context data as a byte array
	Public ReadOnly Property DefaultDigitizingData() As Object
		Get
			tmpl = WTInfo(extindex, EXT_DEFCONTEXT, 0)
			If tmpl = 0 Then
				IsSupported(0)
			Else
				'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
				ReDim tmpb(tmpl)
                tmpl = WTInfo(extindex, EXT_DEFCONTEXT, tmpb)
				'UPGRADE_WARNING: Couldn't resolve default property of object DefaultDigitizingData. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                DefaultDigitizingData = tmpb.Clone
            End If
        End Get
    End Property

    'Returns the cursor-specific data for this extension
    '(for the specified [1-based] cursor) as a byte array


    'Sets the cursor-specific data for this extension for the
    'specified [1-based] cursor - Data should be a byte array
    Public Property CursorData(ByVal Cursor As Integer) As Object
        Get
            tmpl = 0
            WTInfo(WTI_INTERFACE, IFC_NCURSORS, tmpl)
            If tmpl < Cursor Or Cursor > 100 Then
                tabError(19910, "Invalid cursor number (" & Index & ") - use Tablet.Interface.NumCursors to retrieve the number of cursors available")
            Else
                Cursor = Cursor - 1
                tmpl = WTInfo(extindex, EXT_CURSORS + Cursor, 0)
                If tmpl = 0 Then
                    IsSupported(0)
                Else
                    'UPGRADE_WARNING: Lower bound of array tmpb was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1033"'
                    ReDim tmpb(tmpl)
                    tmpl = WTInfo(extindex, EXT_CURSORS + Cursor, tmpb)
                    'UPGRADE_WARNING: Couldn't resolve default property of object CursorData. Click for more: 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="vbup1037"'
                    CursorData = tmpb.Clone
                End If
            End If
        End Get
        Set(ByVal Value As Object)
            If Not pTablet.hWnd.Equals(IntPtr.Zero) Then
                With pTablet.Manager
                    If Not .Enabled Then
                        .Enabled = True
                        If .Enabled = False Then
                            tabError(19928, "Tablet.Extension - Cannot alter settings (unable to acquire tablet manager status)")
                        Else
                            .SetExtCursorData(Cursor, (Me.ID), Value)
                            .Enabled = False
                        End If
                    Else
                        .SetExtCursorData(Cursor, (Me.ID), Value)
                    End If
                End With
            Else
                tabError(19927, "Tablet.Extension - Cannot alter settings without valid Tablet.hWnd (for tablet manager status)")
            End If
        End Set
    End Property
	
    Public Sub New()
        MyBase.New()
        OrMask = New TabletDeviceMask
        Dim numext As Integer
        WTInfo(WTI_INTERFACE, IFC_NEXTENSIONS, numext)
        If numext > 0 Then
            Me.Index = 1
        End If
    End Sub
End Class
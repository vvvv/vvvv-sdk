Option Strict On
Option Explicit On 
Imports System.Windows.Forms

Friend Class TabletWndProcHandler
    Inherits System.Windows.Forms.NativeWindow

    Friend Sub New(ByVal hWnd As IntPtr)
        MyBase.AssignHandle(hWnd)
    End Sub


    Protected Overrides Sub WndProc(ByRef m As Message)
        'Subclassed WndProc for the hWnd
        'DO NOT DEBUG THIS FUNCTION! DO NOT DEBUG IF THIS FUNCTION IS IN USE!
        'set the handle to the message generating tablet
        Select Case m.Msg
            Case WT_PACKET
                'Debug.Print "Packet: " & wparam & "," & lparam
                'WTPacketsGet pTablet.Context.hCtx, 1, pkt
                'WTPacket lparam, wparam, pkt
                'note - NEEDS CLng()
                WTPacketsGet(m.LParam, 1, pkt)
                If m.WParam.ToInt32 Mod granularity = 0 Then
                    'WTPacketsGet pTablet.Context.hCtx, 1, pkt
                    'WTPacket lparam, wparam, pkt
                    pTablet.ProcessPacket(m)
                End If

            Case WT_CTXOPEN, WT_CTXCLOSE, WT_CTXUPDATE, WT_CTXOVERLAP, WT_PROXIMITY, WT_INFOCHANGE, WT_CSRCHANGE
                'WTPacketsGet pTablet.Context.hCtx, 1, pkt
                'why the heck is this still here?
                'WTPacket lparam, wparam, pkt
                pTablet.ProcessPacket(m)

            Case Else
                ' Will be handed by the previous wndproc
                MyBase.WndProc(m)
        End Select

    End Sub

End Class

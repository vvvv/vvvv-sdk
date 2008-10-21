Option Strict On
Option Explicit On
Public Interface ITablet
    '"Baggins! We hates it, we hates it, we hates it for ever!" - Gollum, "The Hobbit"
    'VBTablet ITablet class - ©2001 Laurence Parry
    'This is the tablet interface. It should be implemented by applications
    'that wish to use the interface callback method of packet and event
    'notification, rather than VB events. This is faster (and may be
    'easier for non-VB clients to implement)

    'This is the abstract class. It is here to define the calls that can
    'be made by the component to the client program implementing this interface.
    '**Don't change it and then remake this DLL**
    'If you need to make changes, make an entirely new interface
    '(you should support the old one, too).

    'Signals the arrival of a tablet information packet. Not all values of information will be supported on all devices
    Sub PacketArrival(ByRef ContextHandle As IntPtr, ByRef Cursor As Integer, ByRef X As Integer, ByRef Y As Integer, ByRef Z As Integer, ByRef Buttons As Integer, ByRef NormalPressure As Integer, ByRef TangentPressure As Integer, ByRef Azimuth As Integer, ByRef Altitude As Integer, ByRef Twist As Integer, ByRef Pitch As Integer, ByRef Roll As Integer, ByRef Yaw As Integer, ByRef PacketSerial As Integer, ByRef PacketTime As Integer)

    'Signals a change in cursor proximity, either due to context repositioning or hardware (the user moved the stylus in/out of range)
    Sub ProximityChange(ByRef InContext As Boolean, ByRef IsPhysical As Boolean, ByRef ContextHandle As IntPtr, ByRef ContextName As String)

    'Signals that a context has been repositioned
    Sub ContextRepositioned(ByRef OnTop As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)

    'Signals that the cursor for the specified context has changed
    Sub CursorChange(ByRef ContextHandle As IntPtr, ByRef ContextName As String)

    'Signals a change in global information, caused by the manager with the supplied handle (0 for changes due to hardware) and identified by category and index
    Sub InfoChange(ByRef ManagerHandle As IntPtr, ByRef InfoCategory As String, ByRef InfoIndex As String)

    'Signals that a context has been updated (VBTablet will already have called Context.Reload to retrieve the new values)
    Sub ContextUpdated(ByRef Status As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)

    'Signals that a context has been opened
    Sub ContextOpened(ByRef Status As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)

    'Signals that a context has been closed
    Sub ContextClosed(ByRef Status As Integer, ByRef ContextHandle As IntPtr, ByRef ContextName As String)

End Interface
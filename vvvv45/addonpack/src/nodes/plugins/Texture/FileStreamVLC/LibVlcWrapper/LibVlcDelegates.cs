//    
//    
//    Author:  Frederik Tilkin
//
// ========================================================================

using System;
using System.Runtime.InteropServices;

namespace LibVlcWrapper
{

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	unsafe delegate IntPtr VlcVideoLockHandlerDelegate(ref IntPtr data, ref IntPtr pixels);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	unsafe delegate void VlcVideoUnlockHandlerDelegate(ref IntPtr data, ref IntPtr id, ref IntPtr pixels);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	unsafe delegate void VlcVideoDisplayHandlerDelegate(ref IntPtr data, ref IntPtr id);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	unsafe delegate void VlcEventHandlerDelegate(ref libvlc_event_t libvlc_event, IntPtr userData);

	//typedef void(* libvlc_audio_play_cb)(void *data, const void *samples, unsigned count, int64_t pts)
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	unsafe delegate void VlcAudioPlayDelegate( ref IntPtr data, IntPtr samples, UInt32 count, Int64 pts);
}
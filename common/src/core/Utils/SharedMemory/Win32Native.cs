// Copyright (c) 2003 Richard Blewett
//
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included 
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.

using System;
using System.Security;
using System.Runtime.InteropServices;

namespace VVVV.Utils.SharedMemory
{
	/// <summary>
	/// Summary description for Win32Native.
	/// </summary>
	/// 
	[SuppressUnmanagedCodeSecurity]
	internal static class Win32Native
	{
		internal enum FileMap
		{
			FILE_MAP_READ = 0x4,
			FILE_MAP_WRITE = 0x2,
			FILE_MAP_COPY = 0x1,
			FILE_MAP_ALL_ACCESS = 0x1 + 0x2 + 0x4 + 0x8 + 0x10 + 0xF0000
		} ;

		internal enum StdHandle
		{
			STD_INPUT_HANDLE=-10,
			STD_OUTPUT_HANDLE=-11,
			STD_ERROR_HANDLE=-12
		}
		internal enum ProtectionLevel
		{
			PAGE_NOACCESS = 0x1,
			PAGE_READONLY=0x2,
			PAGE_READWRITE=0x4,
			PAGE_WRITECOPY=0x8,
			PAGE_EXECUTE =0x10
		} ;

		internal const int INVALID_HANDLE_VALUE = -1;
		internal const int ERROR_INVALID_HANDLE = 6;

		[DllImport("Kernel32.dll", SetLastError=true)]
		internal static extern IntPtr CreateFileMapping(	IntPtr hFile,
														IntPtr secAttributes,
														ProtectionLevel dwProtect,
														int dwMaximumSizeHigh,
														int dwMaximumSizeLow,
														string lpName);

		[DllImport("Kernel32.dll", SetLastError=true)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport("Kernel32.dll", SetLastError=true)]
		internal static extern IntPtr MapViewOfFile (	IntPtr hFileMappingObject,
														FileMap dwDesiredAccess,
														int dwFileOffsetHigh, 
														int dwFileOffsetLow,
														int dwNumberOfBytesToMap );

		[DllImport("Kernel32.dll", SetLastError=true)]
		internal static extern bool UnmapViewOfFile(IntPtr map);

		[DllImport("Kernel32.dll", SetLastError=true)]
		internal static extern IntPtr OpenFileMapping(	FileMap dwDesiredAccess,  // access mode
														bool bInheritHandle,    // inherit flag
														string lpName          // object name
														);

		[DllImport("Kernel32.dll")]
		internal static extern uint GetLastError();

		[DllImport("Kernel32.dll")]
		internal static extern void CopyMemory(int dest, int source, int size);

	}
}

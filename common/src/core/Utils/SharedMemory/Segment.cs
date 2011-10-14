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
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// VVVV SharedMemory Utilities 
/// </summary>
namespace VVVV.Utils.SharedMemory
{
	/// <summary>
	/// Enum for specifying whether a new shared memory 
	/// segment should be created or just attach to an existing one
	/// </summary>
	public enum SharedMemoryCreationFlag
	{
		Create,
		Attach
	}

	public class SharedMemoryException : ApplicationException
	{
		public SharedMemoryException(string msg)
			: base(msg)
		{
		}
	}


	/// <summary>
	/// This class wraps Win32 shared memory.
	/// </summary>
	/// 
	public sealed class Segment : IDisposable
	{
		private IntPtr	nativeHandle = IntPtr.Zero;
		private IntPtr	nativePointer = IntPtr.Zero;
		private Mutex	guard = null;
		private int		currentSize = 0;

		public Segment(string name, SharedMemoryCreationFlag creationFlag, int size )
		{
		    if( string.IsNullOrEmpty(name) )
			{
				throw new SharedMemoryException("You must supply a segment name");
			}

			if( size <= 0 && creationFlag == SharedMemoryCreationFlag.Create)
			{
				throw new SharedMemoryException("Size must be postive and non-zero");
			}

			// Create unique named mutex
			guard = new Mutex(false, "m"+name);
			
			// Create or attach to shared memory segment
			if( creationFlag == SharedMemoryCreationFlag.Create )
			{
				nativeHandle = Win32Native.CreateFileMapping((IntPtr)Win32Native.INVALID_HANDLE_VALUE,
					IntPtr.Zero,
					Win32Native.ProtectionLevel.PAGE_READWRITE,
					0,
					size,
					name);
			}
			else
			{
				nativeHandle = Win32Native.OpenFileMapping(Win32Native.FileMap.FILE_MAP_ALL_ACCESS, true, name);
			}

			if( nativeHandle == IntPtr.Zero )
			{
				uint i = Win32Native.GetLastError();
				if( i == Win32Native.ERROR_INVALID_HANDLE )
					throw new SharedMemoryException("Shared memory segment already in use");
				else
					throw new SharedMemoryException("Unable to access shared memory segment. GetLastError = " + i);
			}

			// Get pointer to shared memory segment
			nativePointer = Win32Native.MapViewOfFile(nativeHandle, Win32Native.FileMap.FILE_MAP_ALL_ACCESS, 0, 0, 0);

			if( nativePointer == IntPtr.Zero )
			{
				uint i = Win32Native.GetLastError();
				Win32Native.CloseHandle(nativeHandle);
				nativeHandle = IntPtr.Zero;
				throw new SharedMemoryException("Unable to map shared memory segment. GetLastError = " + i);
			}

			this.currentSize = size;
		}

		/// <summary>
		/// Provides a cross processs lock on the named mutex
		/// </summary>
		public void Lock()
		{
			guard.WaitOne();
		}

		/// <summary>
		/// Releases to cross process lock on the named mutex
		/// </summary>
		public void Unlock()
		{
			guard.ReleaseMutex();
		}

		/// <summary>
		/// Provides access to the cross process waithandle
		/// </summary>
		public WaitHandle WaitHandle
		{
			get{ return guard; }
		}

		/// <summary>
		/// Returns the object graph stored in the shared memory segment
		/// </summary>
		/// <returns>System.Object - root of object graph</returns>
		public object GetData()
		{
			MemoryStream ms = new MemoryStream();

			CopySharedMemoryToStream(ms);

			BinaryFormatter bf = new BinaryFormatter();

			return bf.Deserialize(ms);
		}

		/// <summary>
		/// Stores serializable object graph in shared memory
		/// </summary>
		/// <param name="obj">System.Object root of object graph to be stored in shared memory</param>
		public void SetData(object obj)
		{
			// Ensure root object is serializable
			if( !obj.GetType().IsSerializable )
			{
				throw new SharedMemoryException("stored objects must be serializable");
			}

			// Calculate size of serialized object graph
			MemoryStream tempms = new MemoryStream();

			BinaryFormatter formatter = new BinaryFormatter();

			formatter.Serialize(tempms, obj);

			long marshalledSize = tempms.Length;

			if( currentSize < (int)marshalledSize + Marshal.SizeOf(typeof(long)) )
			{
				throw new SharedMemoryException("The data to be stored is too large for the segment");
			}
            
			// Construct holding memory stream
			MemoryStream ms = new MemoryStream();

			BinaryWriter bs = new BinaryWriter(ms);

			// write size of serialized object graph at start of memory stream
			bs.Write(marshalledSize);

			// write object graph to memory stream
			formatter.Serialize(ms, obj);

			// reset stream to start 
			ms.Seek( 0, SeekOrigin.Begin );

			CopyStreamToSharedMemory(ms);	
		}

		/// <summary>
		/// Finalizer to free up shared memory segment native handle
		/// </summary>
		~Segment()
		{
			Dispose(false);
		}

		/// <summary>
		/// IDisposable.Dispose allow timely clean up and removed the need for finalization
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		/// <summary>
		/// Common clean up method
		/// </summary>
		/// <param name="disposing"></param>
		private void Dispose( bool disposing )
		{
			// Only clean up managed resources if being called from IDisposable.Dispose
			if( disposing )
			{
				guard.Close();
			}

			// always clean up unmanaged resources
			if( nativePointer != IntPtr.Zero )
			{
				Win32Native.UnmapViewOfFile(nativePointer);
			}

			if( nativeHandle != IntPtr.Zero )
			{
				Win32Native.CloseHandle(nativeHandle);
			}
		}

		/// <summary>
		/// Copies stream to shared memory segment using unsafe pointers
		/// </summary>
		/// <param name="stream"> System.IO.Stream - data to be copied to shared memory</param>
		private unsafe void CopyStreamToSharedMemory( Stream stream )
		{
			// Read stream data into byte array
			BinaryReader reader = new BinaryReader(stream);

			Byte[] data = reader.ReadBytes((int)stream.Length);

			// Copy the byte array to shared memory
			fixed( byte* source = data )
			{
				void* temp = nativePointer.ToPointer();
				
				byte* dest = (byte*)temp;	

				Win32Native.CopyMemory((int) dest, (int) source, (int)stream.Length);
			}
		}
		
		public unsafe void CopyByteArrayToSharedMemory(IntPtr Bytes, int Size)
		{
			// Copy the byte array to shared memory
			byte* source = (byte*)Bytes.ToPointer();
			//fixed( byte* source = (byte*) Bytes.ToPointer())
			{
				void* temp = nativePointer.ToPointer();
				
				byte* dest = (byte*)temp;	

				Win32Native.CopyMemory((int) dest, (int) source, Size);
			}
		}

		/// <summary>
		/// Copies shared memory data to passed stream using unsafe pointers
		/// </summary>
		/// <param name="stream">System.IO.Stream - stream to receive data</param>
		private unsafe void CopySharedMemoryToStream( Stream stream )
		{
			// Create a tempory byte array to store the length
			void* temp = nativePointer.ToPointer();

			byte* source = (byte*)temp;

			long len = *(long*)temp;
				
			// Set the source data pointer to start of serialized object graph
			source = (byte*)temp;
			source += 8;

			// Create a byte array to hold the serialized data
            Byte[] data = new Byte[len];

			// Copy the shared memory data to byte array
			fixed(byte* dest = data )
			{
				Win32Native.CopyMemory((int)dest, (int)source, (int)len);
			}

			// Write the byte array to the stream
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(data);

			// Reset stream to start
			stream.Seek(0, SeekOrigin.Begin);
		}
	}
}

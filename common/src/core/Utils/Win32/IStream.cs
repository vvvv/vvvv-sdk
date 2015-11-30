using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using com = System.Runtime.InteropServices.ComTypes;

namespace VVVV.Utils.Win32
{
    // Summary:
    //     Provides the managed definition of the IStream interface, with ISequentialStream
    //     functionality.
    // Read/Write methods use pointer instead of marshaling unmanaged data to byte arrays.
    [Guid("0000000c-0000-0000-C000-000000000046"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStream
    {
        //
        // Summary:
        //     Reads a specified number of bytes from the stream object into memory starting
        //     at the current seek pointer.
        //
        // Parameters:
        //   pv:
        //     When this method returns, contains the data read from the stream. This parameter
        //     is passed uninitialized.
        //
        //   cb:
        //     The number of bytes to read from the stream object.
        //
        //   pcbRead:
        //     A pointer to a ULONG variable that receives the actual number of bytes read
        //     from the stream object.
        void Read(IntPtr pv, int cb, IntPtr pcbRead);
        //
        // Summary:
        //     Writes a specified number of bytes into the stream object starting at the
        //     current seek pointer.
        //
        // Parameters:
        //   pv:
        //     The buffer to write this stream to.
        //
        //   cb:
        //     The number of bytes to write to the stream.
        //
        //   pcbWritten:
        //     On successful return, contains the actual number of bytes written to the
        //     stream object. If the caller sets this pointer to System.IntPtr.Zero, this
        //     method does not provide the actual number of bytes written.
        void Write(IntPtr pv, int cb, IntPtr pcbWritten);
        //
        // Summary:
        //     Changes the seek pointer to a new location relative to the beginning of the
        //     stream, to the end of the stream, or to the current seek pointer.
        //
        // Parameters:
        //   dlibMove:
        //     The displacement to add to dwOrigin.
        //
        //   dwOrigin:
        //     The origin of the seek. The origin can be the beginning of the file, the
        //     current seek pointer, or the end of the file.
        //
        //   plibNewPosition:
        //     On successful return, contains the offset of the seek pointer from the beginning
        //     of the stream.
        void Seek(long dlibMove, [MarshalAs(UnmanagedType.I4)] System.IO.SeekOrigin dwOrigin, IntPtr plibNewPosition);
        //
        // Summary:
        //     Changes the size of the stream object.
        //
        // Parameters:
        //   libNewSize:
        //     The new size of the stream as a number of bytes.
        void SetSize(long libNewSize);
        //
        // Summary:
        //     Copies a specified number of bytes from the current seek pointer in the stream
        //     to the current seek pointer in another stream.
        //
        // Parameters:
        //   pstm:
        //     A reference to the destination stream.
        //
        //   cb:
        //     The number of bytes to copy from the source stream.
        //
        //   pcbRead:
        //     On successful return, contains the actual number of bytes read from the source.
        //
        //   pcbWritten:
        //     On successful return, contains the actual number of bytes written to the
        //     destination.
        void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);
        //
        // Summary:
        //     Ensures that any changes made to a stream object that is open in transacted
        //     mode are reflected in the parent storage.
        //
        // Parameters:
        //   grfCommitFlags:
        //     A value that controls how the changes for the stream object are committed.
        void Commit(int grfCommitFlags);
        //
        // Summary:
        //     Discards all changes that have been made to a transacted stream since the
        //     last System.Runtime.InteropServices.ComTypes.IStream.Commit(System.Int32)
        //     call.
        void Revert();
        //
        // Summary:
        //     Restricts access to a specified range of bytes in the stream.
        //
        // Parameters:
        //   libOffset:
        //     The byte offset for the beginning of the range.
        //
        //   cb:
        //     The length of the range, in bytes, to restrict.
        //
        //   dwLockType:
        //     The requested restrictions on accessing the range.
        void LockRegion(long libOffset, long cb, int dwLockType);
        //
        // Summary:
        //     Removes the access restriction on a range of bytes previously restricted
        //     with the System.Runtime.InteropServices.ComTypes.IStream.LockRegion(System.Int64,System.Int64,System.Int32)
        //     method.
        //
        // Parameters:
        //   libOffset:
        //     The byte offset for the beginning of the range.
        //
        //   cb:
        //     The length, in bytes, of the range to restrict.
        //
        //   dwLockType:
        //     The access restrictions previously placed on the range.
        void UnlockRegion(long libOffset, long cb, int dwLockType);
        //
        // Summary:
        //     Retrieves the System.Runtime.InteropServices.STATSTG structure for this stream.
        //
        // Parameters:
        //   pstatstg:
        //     When this method returns, contains a STATSTG structure that describes this
        //     stream object. This parameter is passed uninitialized.
        //
        //   grfStatFlag:
        //     Members in the STATSTG structure that this method does not return, thus saving
        //     some memory allocation operations.
        void Stat(out com.STATSTG pstatstg, int grfStatFlag);
        // Summary:
        //     Creates a new stream object with its own seek pointer that references the
        //     same bytes as the original stream.
        //
        // Parameters:
        //   ppstm:
        //     When this method returns, contains the new stream object. This parameter
        //     is passed uninitialized.
        void Clone(out IStream ppstm);
    }
}

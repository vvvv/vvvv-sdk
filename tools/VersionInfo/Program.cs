using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace VVVV.Tools
{
    public class VersionInfo
    {
        enum BinaryType : uint
        {
            SCS_32BIT_BINARY = 0, // A 32-bit Windows-based application
            SCS_64BIT_BINARY = 6, // A 64-bit Windows-based application.
            SCS_DOS_BINARY = 1, // An MS-DOS – based application
            SCS_OS216_BINARY = 5, // A 16-bit OS/2-based application
            SCS_PIF_BINARY = 3, // A PIF file that executes an MS-DOS – based application
            SCS_POSIX_BINARY = 4, // A POSIX – based application
            SCS_WOW_BINARY = 2 // A 16-bit Windows-based application 
        }

        static BinaryType? GetBinaryType(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                stream.Seek(0x3C, SeekOrigin.Begin);
                using (var reader = new BinaryReader(stream))
                {
                    if (stream.Position + sizeof(int) > stream.Length)
                        return null;
                    var peOffset = reader.ReadInt32();
                    stream.Seek(peOffset, SeekOrigin.Begin);
                    if (stream.Position + sizeof(uint) > stream.Length)
                        return null;
                    var peHead = reader.ReadUInt32();
                    if (peHead != 0x00004550) // "PE\0\0"
                        return null;
                    if (stream.Position + sizeof(ushort) > stream.Length)
                        return null;
                    switch (reader.ReadUInt16())
                    {
                        case 0x14c:
                            return BinaryType.SCS_32BIT_BINARY;
                        case 0x8664:
                            return BinaryType.SCS_64BIT_BINARY;
                        default:
                            return null;
                    }
                }
            }
        }

        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return 1;
            }

            var filename = args[0];
            Console.WriteLine($"{GetVersionInfo(filename)}_{GetPlatform(filename)}");
            
            return 0;
        }
        
        private static void PrintUsage()
        {
            Console.WriteLine("Usage: VersionInfo.exe file");
        }
        
        public static string GetVersionInfo(string filename)
        {
            if (!Path.IsPathRooted(filename))
            {
                filename = Path.GetFullPath(filename);
            }
            
            if (!File.Exists(filename))
            {
                throw new ArgumentException(string.Format("Can't find file '{0}'.", filename));
            }
            
            var vi = FileVersionInfo.GetVersionInfo(filename);
            var version = string.Format("50{0}{1}", vi.ProductVersion, vi.FileMajorPart);
            if (vi.FileMinorPart > 0)
                version += "." + vi.FileMinorPart;
            if (vi.FilePrivatePart > 0)
                version += "." + vi.FilePrivatePart;
            return version;
        }

        public static string GetPlatform(string filename)
        {
            var result = GetBinaryType(filename);
            if (result.HasValue)
            {
                switch (result.Value)
                {
                    case BinaryType.SCS_32BIT_BINARY:
                        return "x86";
                    case BinaryType.SCS_64BIT_BINARY:
                        return "x64";
                }
            }
            return "unknown";
        }
    }
}
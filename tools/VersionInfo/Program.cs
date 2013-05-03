using System;
using System.Diagnostics;
using System.IO;

namespace VVVV.Tools
{
    public class VersionInfo
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return 1;
            }
            
            Console.WriteLine(GetVersionInfo(args[0]));
            
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
            var version = string.Format("45{0}{1}", vi.ProductVersion, vi.FileMajorPart);
            if (vi.FileMinorPart > 0)
                version += "." + vi.FileMinorPart;
            if (vi.FilePrivatePart > 0)
                version += "." + vi.FilePrivatePart;
            return version;
        }
    }
}
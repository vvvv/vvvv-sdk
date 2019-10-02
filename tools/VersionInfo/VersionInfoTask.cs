using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace VVVV.Tools.MSBuild
{
    public class GetVersionInfo : Task
    {
        [Required]
        public string File
        {
            get;
            set;
        }
        
        [Output]
        public string NewVersionString
        {
            get;
            private set;
        }

        [Output]
        public string OldVersionString
        {
            get;
            private set;
        }

        [Output]
        public bool IsPreview
        {
            get;
            private set;
        }

        public override bool Execute()
        {
            try 
            {
                var vi = FileVersionInfo.GetVersionInfo(File);
                NewVersionString = VersionInfo.GetNewVersionString(vi);
                OldVersionString = VersionInfo.GetOldVersionString(vi);
                IsPreview = vi.IsDebug || vi.IsPreRelease || vi.IsSpecialBuild;
            } 
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
            
            return true;
        }
    }

    public class GetPlatformFromBinary : Task
    {
        [Required]
        public string File
        {
            get;
            set;
        }

        [Output]
        public string ReturnValue
        {
            get;
            private set;
        }

        public override bool Execute()
        {
            try
            {
                ReturnValue = VVVV.Tools.VersionInfo.GetPlatform(File);
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }

            return true;
        }
    }
}

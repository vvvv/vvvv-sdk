using System;
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
        public string ReturnValue
        {
            get;
            private set;
        }
        
        public override bool Execute()
        {
            try 
            {
                ReturnValue = VVVV.Tools.VersionInfo.GetVersionInfo(File);
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

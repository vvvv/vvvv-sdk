using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;

namespace VVVV.Webinterface.HttpServer
{
    /// <summary>
    /// Handles the Ressources which are add to the Libary.
    /// </summary>
    class Ressources
    {

        
        Assembly ExecutingProjekt = Assembly.GetExecutingAssembly();
        private string[] FFiles;


        public List<string> Files 
        {
            get 
            {
                List<string> FileNames = new List<string>();

                //foreach (string File in FFiles)
                //{
                //    ManifestResourceInfo Info = ExecutingProjekt.GetManifestResourceInfo(File);
                //    FileNames.Add(Info.FileName);
                //}

                foreach (string File in FFiles)
                {
                    FileNames.Add(File);
                }

                return FileNames; 
            }
        }
        

        public Ressources()
        {
            FFiles = ExecutingProjekt.GetManifestResourceNames();
            
        }

        public Byte[] SearchFile(string Filename)
        {
            
            Byte[] RequestedFile = null;

            foreach (string File in FFiles)
            {
                if (File.Contains(Filename))
                {
                     Stream FoundedFile = ExecutingProjekt.GetManifestResourceStream(File);

                     byte[] bytes = new byte[FoundedFile.Length];
                     int read;
                     string LoadedFile = String.Empty;
                     while ((read = FoundedFile.Read(bytes, 0, bytes.Length)) != 0)
                     {
                         LoadedFile = LoadedFile + Encoding.UTF8.GetString(bytes, 0, read);
                     }

                     RequestedFile = bytes;

                     FoundedFile.Flush();
                     FoundedFile.Close();
                     FoundedFile = null;
                }
            }

            return RequestedFile;
        }
    }
}

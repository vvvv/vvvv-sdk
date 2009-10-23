using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using System.Reflection;

namespace VVVV.Webinterface.HttpServer
{
    class Ressources
    {

        
        Assembly ExecutingProjekt = Assembly.GetExecutingAssembly();
        private string[] Files;
        

        public Ressources()
        {

        }

        public Byte[] SearchFile(string Filename)
        {
            Files = ExecutingProjekt.GetManifestResourceNames();
            Byte[] RequestedFile = null;

            foreach (string File in Files)
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

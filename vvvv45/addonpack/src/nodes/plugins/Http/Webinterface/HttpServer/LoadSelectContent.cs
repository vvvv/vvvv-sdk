using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using VVVV.Webinterface.HttpServer;
using VVVV.Webinterface.Utilities;


namespace VVVV.Webinterface.HttpServer
{
    class LoadSelectContent
    {

        private byte[] mContentAsByte;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        

        public byte[] ContentAsBytes
        {
            get
            {
                return mContentAsByte;
            }
        }

        /// <summary>
        /// Load and Build the Requested File from VVVV or from Disc
        /// </summary>
        /// <param name="pFilename">Filename which is requested by the client</param>
        /// <param name="pFileLocation">whole file URI which is reqquested by the client</param>
        /// <param name="pPaths">Root Pathes set by VVVV Renderer (HTTP)</param>
        /// <param name="pHtmlPages">Pages to Build by an GET Request</param>
        public LoadSelectContent( string pFilename,string pFileLocation, List<string> pPaths)
        {
            if (pFilename == "dummy.html")
            {
                //Build an special HTML dummyFile for the comet communication
                string tPageToSend = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
                mContentAsByte = Encoding.UTF8.GetBytes(tPageToSend);
            }
            else if(mWebinterfaceSingelton.ServerFilesUrl.Contains(pFilename))
            {
                // if the file name is set by VVVV Build all Files from an Requested Page and Encode it as Bytes
                SortedList<string, string> tServerFiles;
                if (mWebinterfaceSingelton.GuiLists.ContainsKey(pFilename))
                {

                    tServerFiles = mWebinterfaceSingelton.BuildPages(pFilename);
                }
                else
                {
                    tServerFiles = mWebinterfaceSingelton.BuildPages(pFilename);
                }

                string RequestedFile;
                tServerFiles.TryGetValue(pFilename, out RequestedFile);
                mContentAsByte = Encoding.UTF8.GetBytes(RequestedFile);
            }else
            {
                //loads file from disc
                LoadFromDisc(pFilename,pFileLocation,pPaths);
            }
        }

        /// <summary>
        /// Checks is vvvv defines the file or if it should be searched in the defined root directories oon the disc
        /// </summary>
        /// <param name="pFilename">requested file name</param>
        /// <param name="pFileLocation">request file URI</param>
        /// <param name="pPaths">defined root directories</param>
        private void LoadFromDisc(string pFilename,string pFileLocation, List<string> pPaths)
        {

            bool FoundFileFlag = false;

            //checks for evers path defined by vvvv if there is an file on disc
            if (pPaths != null)
            {
                foreach (string pPath in pPaths)
                {
                    //Changes the HTML References to windows file references
                    pFileLocation = Regex.Replace(pFileLocation, @"/", @"\");

                    //delets the first backslashes because it throws an wrong path in Path.Combine
                    pFileLocation = Regex.Replace(pFileLocation, @"^\\", "");
                    string newPath = Path.Combine(pPath, pFileLocation);

                    //check the combined absolute path
                    if (File.Exists(newPath))
                    {
                        FoundFileFlag = true;
                        LoadFile(newPath);
                    }
                    else
                    {
                        //searches in every root folder if there is the requested file 
                        DirectoryInfo tDInfo = new DirectoryInfo(pPath);
                        FileInfo[] tFileInfoList = tDInfo.GetFiles();

                        foreach (FileInfo pFileInfo in tFileInfoList)
                        {
                            if (pFileInfo.Name == pFilename)
                            {
                                FoundFileFlag = true;
                                LoadFile(pFileInfo.FullName);
                            }
                        }
                    }
                }
            }
            if (FoundFileFlag == false)
            {
                Ressources r = new Ressources();
                Byte[] RessourceFile = r.SearchFile(pFilename);
                if (RessourceFile != null)
                {
                    mContentAsByte = RessourceFile;
                }else
                {
                    mContentAsByte = Encoding.UTF8.GetBytes(HTMLToolkit.ErrorMessage(pFilename));
                }

                r = null;
            }
        }

        /// <summary>
        /// Loads the Requested File from disc and encodes it as bytes
        /// </summary>
        /// <param name="pFilePath">Requested File Path</param>
        private void LoadFile(string pFilePath)
        {
            try
            {
                FileStream tFs = new FileStream(pFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader reader = new BinaryReader(tFs);

                byte[] bytes = new byte[tFs.Length];
                int read;
                string LoadedFile = String.Empty;
                while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Read from the file and write the data to the network
                    LoadedFile = LoadedFile + Encoding.UTF8.GetString(bytes, 0, read);
                }


                mContentAsByte = bytes;

                reader.Close();
                tFs.Close();
            }
            catch (FileLoadException ex)
            {
                mContentAsByte = Encoding.UTF8.GetBytes(ex.Message);
            }
            catch (Exception ex)
            {
                mContentAsByte = Encoding.UTF8.GetBytes(ex.Message);
            }
        }
    }
}

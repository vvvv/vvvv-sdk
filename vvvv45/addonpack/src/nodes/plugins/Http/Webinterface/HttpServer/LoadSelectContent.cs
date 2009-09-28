using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using VVVV.Webinterface.HttpServer;


namespace VVVV.Webinterface.HttpServer
{
    class LoadSelectContent
    {

        private string mContent;
        private byte[] mContentAsByte;
        private string mFileExtension;
        private string mFileLocation;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        

        public string Content
        {
            get
            {
                return mContent;
            }
        }


        public string FileExtension
        {
            get
            {
                return mFileExtension;
            }

        }

        public byte[] ContentAsBytes
        {
            get
            {
                return mContentAsByte;
            }
        }

        public LoadSelectContent( string pFilename,string pFileLocation, List<string> pPaths, SortedList<string,byte[]> pHtmlPages)
        {


            if (pFilename == "dummy.html")
            {
                string tPageToSend = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
                mContentAsByte = Encoding.UTF8.GetBytes(tPageToSend);
            }else if(mWebinterfaceSingelton.ServerFilesUrl.Contains(pFilename))
            {

                SortedList<string, string> tServerFiles;
                if (mWebinterfaceSingelton.GuiLists.ContainsKey(pFilename))
                {
                    mWebinterfaceSingelton.BuildPages(pFilename);
                    tServerFiles = mWebinterfaceSingelton.ServerFiles;
                }
                else
                {
                    tServerFiles = mWebinterfaceSingelton.ServerFiles;
                }


                string RequestedFile;
                tServerFiles.TryGetValue(pFilename, out RequestedFile);
                mContentAsByte = Encoding.UTF8.GetBytes(RequestedFile);
            }else
            {
                LoadFromDisc(pFilename,pFileLocation,pPaths);
            }
        }


        private void LoadFromDisc(string pFilename,string pFileLocation, List<string> pPaths)
        {
            bool FoundFileFlag = false;

            foreach (string pPath in pPaths)
            {

                

                try
                {
                    pFileLocation = Regex.Replace(pFileLocation, @"/", @"\");
                    pFileLocation = Regex.Replace(pFileLocation, @"^\\", ""); 
                    string newPath = Path.Combine(pPath,pFileLocation);
                    
                    if (File.Exists(newPath))
                    {
                        Debug.WriteLine("Filepath exist");
                        Debug.WriteLine("pPath: " + pPath);
                        Debug.WriteLine("pFileLocation: " + pFileLocation);
                        Debug.WriteLine("newFilePath:  " + newPath);    

                        FoundFileFlag = true;
                        LoadFile(newPath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }


                DirectoryInfo tDInfo = new DirectoryInfo(pPath);
                FileInfo[] tFileInfoList = tDInfo.GetFiles();

                if (FoundFileFlag == false)
                {
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

            if (FoundFileFlag == false)
            {
                mContent = "File " + "'" + pFilename + "'" + " Not Found";
                mContentAsByte = Encoding.UTF8.GetBytes(mContent);
                if (pFilename.Contains("."))
                {
                    mFileExtension = (pFilename.Split('.'))[1];
                }
            }
        }


        private void LoadFile(string pFilePath)
        {
            try
            {
                FileStream tFs = new FileStream(pFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader reader = new BinaryReader(tFs);
                mFileExtension = pFilePath;

                byte[] bytes = new byte[tFs.Length];
                int read;

                while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Read from the file and write the data to the network
                    mContent = mContent + Encoding.UTF8.GetString(bytes, 0, read);
                }

                mContentAsByte = bytes;

                reader.Close();
                tFs.Close();
            }

            catch (FileLoadException ex)
            {
                //////Debug.WriteLine("LoadSelectContent: " + ex.Message.ToString());
                mContent = ex.Message.ToString();
            }
            catch (Exception ex)
            {
                //////Debug.WriteLine("LoadSelectContent: " + ex.Message.ToString());
                mContent = ex.Message.ToString();
            }
        }
    }
}

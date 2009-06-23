using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32 ;


namespace VVVV.Webinterface.HttpServer
{
    class ItemsToServ
    {


        private string mPath;
        private DirectoryInfo mSourceFolder;
        private DirectoryInfo[] mSubFolders;

        private List<string> mFileListVVVV = new List<string>();
        private List<string> mFileListNamesVVVV = new List<string>(); 

        private SortedList<string,byte[]> mPicListServer = new SortedList<string,byte[]>();
        private SortedList<string,string> mFileListServer = new SortedList<string,string>();
        
        private string[] mFilterTypen = new string[] { ".js", ".jpg", ".png", ".css" , ".ico"};

        #region Properties


        /// <summary>
        /// The List of filtert files to serve
        /// </summary>
        public List<string> FileListVVVV
        {
            get
            {
                if (mFileListVVVV.Count == 0)
                {
                   mFileListVVVV.Add("no files found");
                   return mFileListVVVV;
                }
                else
                {
                   return mFileListVVVV;
                }
                
            }
        
        }

        public List<string> FileListNameVVVV
        {
            get
            {
                if (mFileListNamesVVVV.Count == 0)
                {
                    mFileListNamesVVVV.Add("no files found");
                    return mFileListNamesVVVV;
                }
                else
                {
                    return mFileListNamesVVVV;
                }
            }
        }


        /// <summary>
        /// fileextension which should served
        /// </summary>
        public string[] FilterTypen
        {
            set
            {
                mFilterTypen = value;
            }
        }


        #endregion Properties


         /// <summary>
        /// Reads the defined Server Folder and select the files to serve
         /// </summary>  
        public ItemsToServ(string pPath)
        {
            this.mPath = pPath;
            SetSourceFolder(pPath);
            mSubFolders = mSourceFolder.GetDirectories();
        }

        private void SetSourceFolder(string pPath)
        {
            mSourceFolder = new DirectoryInfo(pPath);
        }




        public void ReadServerFolder(string pPath)
        {
            mFileListVVVV.Clear();
            mFileListNamesVVVV.Clear();
            mFileListServer.Clear();
            mPicListServer.Clear();
            

            //mSubFolders = mSourceFolder.GetDirectories();
            SetSourceFolder(pPath);

            List<FileInfo> tFileInfoList = new List<FileInfo>();
            FileInfo[] tFileInfoMainFolder = mSourceFolder.GetFiles();

            for (int i = 0; i < tFileInfoMainFolder.Length; i++)
            {
                tFileInfoList.Add(tFileInfoMainFolder[i]);
            }

            for (int i = 0; i < mSubFolders.Length; i++)
            {
                FileInfo[] tFileInfoSubFolder = mSubFolders[i].GetFiles();
                for (int f = 0; f < tFileInfoSubFolder.Length; f++)
                {
                    tFileInfoList.Add(tFileInfoSubFolder[f]);
                }
            }

            FilterFileList(tFileInfoList);
        }


        private void FilterFileList(List<FileInfo> pFileInfoList)
        {
            foreach(FileInfo pFileInfo in pFileInfoList)
            {

                for (int i = 0; i < mFilterTypen.Length; i++)
                {
                    string tExtensionFile = pFileInfo.Extension;

                    if (mFilterTypen[i] == tExtensionFile)
                    {
                        LoadFiles(tExtensionFile, pFileInfo);
                        mFileListVVVV.Add(pFileInfo.FullName);
                        mFileListNamesVVVV.Add(pFileInfo.Name);
                    }
                }               
            }

        }

        private void LoadFiles(string pExtension,FileInfo pFileInfo)
        {
            if (pExtension == ".jpg" || pExtension == ".png" || pExtension == ".ico")
            {

                int iTotBytes = 0;

                string sResponse = "";

                FileStream fs = new FileStream(pFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                // Create a reader that can read bytes from the FileStream.


                BinaryReader reader = new BinaryReader(fs);
                byte[] bytes = new byte[fs.Length];
                int read;
                while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Read from the file and write the data to the network
                    sResponse = sResponse + Encoding.UTF8.GetString(bytes, 0, read);

                    iTotBytes = iTotBytes + read;

                }
                reader.Close();
                fs.Close();

                if (mPicListServer.ContainsKey(pFileInfo.Name) == false)
                {
                    mPicListServer.Add(pFileInfo.Name, bytes);
                }
                if (mFileListServer.ContainsKey(pFileInfo.Name) == false)
                {
                    mFileListServer.Add(pFileInfo.Name, sResponse);
                }
            }
            else if (pExtension == ".js" || pExtension == ".css")
            {
                string tFileContent;
                StreamReader tSr = new StreamReader(pFileInfo.FullName, Encoding.GetEncoding("UTF-8"));
                tFileContent = tSr.ReadToEnd();

                if(mFileListServer.ContainsKey(pFileInfo.Name) == false)
                {
                    mFileListServer.Add(pFileInfo.Name, tFileContent);
                }
            }
            else 
            {
                mFileListServer.Add(pFileInfo.Name, "No Conversion Type found");
            }
        }

        public string GetFileContent(string pFilename)
        {

            if (mFileListServer.ContainsKey(pFilename))
            {
                string tFileContent;
                mFileListServer.TryGetValue(pFilename, out tFileContent);
                return tFileContent;
            }
            else
            {
                Debug.WriteLine("File not found in FileList Server in ItemstoServe");
                return "File Not found";
            }
        }


        public byte[] GetPicContent(string pFilename)
        {
            if (mPicListServer.ContainsKey(pFilename))
            {
                byte[] tPicContent;
                mPicListServer.TryGetValue(pFilename, out tPicContent);
                return tPicContent;
            }
            else
            {
                Debug.WriteLine("File not found in FilList Server in call Items to Serve");
                byte[] tPicContent = Encoding.UTF8.GetBytes("File Not found"); 
                return tPicContent;

            }
        }

    }
}

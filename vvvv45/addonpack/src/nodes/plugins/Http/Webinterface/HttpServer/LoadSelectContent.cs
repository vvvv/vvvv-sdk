using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using VVVV.Webinterface.HttpServer;

namespace VVVV.Webinterface.HttpServer
{
    class LoadSelectContent
    {

        private string mContent;
        private byte[] mContentAsByte;
        private string mStatus = new HTTPStatusCode("").Code200;
        private string mFileExtension;

        public string Content
        {
            get
            {
                return mContent;
            }
        }

        public string StatusCode
        {
            get
            {
                return mStatus;
            }
        }

        public string FileExtension
        {
            get
            {
                return mFileExtension;
            }

        }

        public byte[] ContentAsBaytes
        {
            get
            {
                return mContentAsByte;
            }
        }

        public LoadSelectContent( string pFilename, List<string> pPaths)
        {
            bool ListFlag;
            mStatus = new HTTPStatusCode("").Code200;

            ListFlag = CheckFileNameInVVVVLists(pFilename);
            if(ListFlag == false)
            {
                LoadFromDisc(pFilename, pPaths);
            }
        }


        private bool CheckFileNameInVVVVLists(string pFilename)
        {
            //check Lists from vvvv for filename
            return false;
        }



        private void LoadFromDisc(string pFilename, List<string> pPaths)
        {
            bool FoundFileFlag = false;

            foreach (string pPath in pPaths)
            {
                DirectoryInfo tDInfo = new DirectoryInfo(pPath);
                FileInfo[] tFileInfoList = tDInfo.GetFiles();

                if (FoundFileFlag == false)
                {
                    foreach (FileInfo pFileInfo in tFileInfoList)
                    {
                        if (pFileInfo.Name == pFilename)
                        {
                            FoundFileFlag = true;
                            LoadFile(pFileInfo);
                        }
                    }
                }
            }

            if (FoundFileFlag == false)
            {
                mContent = "File " + "'" + pFilename + "'" + " Not Found";
                mStatus = new HTTPStatusCode("").Code404;
            }
            else
            {
                mStatus = new HTTPStatusCode("").Code200;
            }
        }


        private void LoadFile(FileInfo pFileInfo)
        {
            try
            {
                FileStream tFs = new FileStream(pFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader reader = new BinaryReader(tFs);
                mFileExtension = pFileInfo.FullName;

                byte[] bytes = new byte[tFs.Length];
                int read;

                while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Read from the file and write the data to the network
                    mContent = mContent + Encoding.ASCII.GetString(bytes, 0, read);
                }

                mContentAsByte = bytes;

                reader.Close();
                tFs.Close();
            }

            catch (FileLoadException ex)
            {
                Debug.WriteLine("LoadSelectContent: " + ex.Message.ToString());
                mContent = "ex.Message.ToString()";
                mStatus = new HTTPStatusCode().Code500;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadSelectContent: " + ex.Message.ToString());
                mContent = "ex.Message.ToString()";
                mStatus = new HTTPStatusCode().Code500;
            }
        }
    }
}

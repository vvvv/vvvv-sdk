using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{
    class Response
    {

        #region field declaration

        private string mBody = "";

        private Request mRequest;
        private ResponseHeader mHeader;
        private byte[] mHeaderasBytes;
        private byte[] mResponseAsBytes;
        

        #endregion field declaration



        public string Text
        {
            get
            {
                return mHeader.Text + Environment.NewLine + mBody; 
            }
        }

        public byte[] TextInBytes
        {
            get
            {
                
                return mResponseAsBytes;
            }
        }



        public Response(Request pRequest, List<string> pFolderToServ)
        {
            this.mRequest = pRequest;

            LoadSelectContent tLoadSelectContent = new LoadSelectContent(pRequest.FileName, pFolderToServ);
            mBody = tLoadSelectContent.Content;

            try
            {
                mHeader = new ResponseHeader(tLoadSelectContent.StatusCode);

                mHeader.SetAttribute("content-type", GetContentType(tLoadSelectContent.FileExtension));
                mHeader.SetAttribute("accept-ranges", "bytes");
                mHeader.SetAttribute("content-length", tLoadSelectContent.ContentAsBaytes.Length.ToString());
                //mHeader.SetAttribute("connection", "keep-alive");
                

                mHeaderasBytes = Encoding.ASCII.GetBytes(mHeader.Text);
                mResponseAsBytes = Combine(mHeaderasBytes, tLoadSelectContent.ContentAsBaytes);
                Debug.WriteLine(mHeader.Text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Header exeption:" + ex.Message.ToString());
            }
       
        }


        private string GetContentType(string pFilePath)
        {
            string mimeType = "application/unknown";

            string ext = System.IO.Path.GetExtension(pFilePath).ToLower();

            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

            if (regKey != null && regKey.GetValue("Content Type") != null)
            {
                mimeType = regKey.GetValue("Content Type").ToString();
            }
            else if (ext == ".js")
            {
                mimeType = "application/x-javascript";
            }

            return mimeType;
        }


        private byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
        





     }
}

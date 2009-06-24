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
        private ResponseHeader mHeader;
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



        public Response(string pFilename,byte[] pContent, string pStausCode)
        {

            mHeader = new ResponseHeader(pStausCode);

            mHeader.SetAttribute("content-type", GetContentType(pFilename.Split('.')[1]));
            mHeader.SetAttribute("accept-ranges", "bytes");
            mHeader.SetAttribute("content-length", pContent.Length.ToString());
            mHeader.SetAttribute("connection", "close");
           
            mResponseAsBytes = Combine(Encoding.UTF8.GetBytes(mHeader.Text), pContent);
        }


        public Response(string pFilename, string pContentType, byte[] pContent, string pStausCode)
        {

            mHeader = new ResponseHeader(pStausCode);

            mHeader.SetAttribute("content-type", pContentType);
            mHeader.SetAttribute("accept-ranges", "bytes");
            mHeader.SetAttribute("content-length", pContent.Length.ToString());
            mHeader.SetAttribute("connection", "close");

            mResponseAsBytes = Combine(Encoding.UTF8.GetBytes(mHeader.Text), pContent);
        }


        private string GetContentType(string pFilePath)
        {
            string mimeType = "application/unknown";

            string ext = pFilePath.ToLower();

            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("." + ext);

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

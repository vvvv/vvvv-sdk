using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

using VVVV.Utils.VMath;


namespace VVVV.Webinterface.Utilities
{ 

    class HTMLToolkit
    {

        public static void SavePage(string pURL, string pPage)
        {
            System.IO.FileStream tFile = new System.IO.FileStream(pURL, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            System.IO.StreamWriter tWriter = new System.IO.StreamWriter(tFile, System.Text.Encoding.UTF8);

            tWriter.Write(pPage);
            tWriter.Flush();
            tWriter.Close();
        }

        #region conversion

        public static double MapScale(double pValue, double pSourceMin, double pSourceMax, double pMin, double pMax)
        {
            double tFactor = (pMax - pMin) / (pSourceMax - pSourceMin);

            return tFactor * (pValue - pSourceMin) + pMin;
        }

        public static double MapTransform(double pValue, double pSourceMin, double pSourceMax, double pMin, double pMax, double pScale)
        {
            double tFactor = (pMax - pMin) / (pSourceMax - pSourceMin);

            return ((tFactor * (pValue - pSourceMin) + pMin)) - (pScale/2);
        }

        public static string ValueToStringRelativ(double value)
        {
            string output = null;


            if (value == 0)
            {
                output = "0";
            }
            else
            {
                value = Math.Floor(value * 100);
                string temp = value.ToString(".####");
                System.Text.StringBuilder sb = new System.Text.StringBuilder(temp);
                output = sb.Replace(",", ".").ToString();
            }

            return output;
        }

        public static string CreatePageID(string pNodePath)
        {
            string[] tPatchIDs = pNodePath.Split('/');
            string tNewNodePath = String.Empty;
            bool ContainsDescriptivName = false;

            for (int i = 0; i < tPatchIDs.Length; i++)
            {
                string tID = tPatchIDs[i];

                if (tID != "")
                {
                    if (Regex.IsMatch(tID, "[a-zA-z]"))
                    {
                        tNewNodePath = tID;
                        ContainsDescriptivName = true;
                    }
                    else
                    {
                        if (i == tPatchIDs.Length - 1)
                        {
                            int temp = Convert.ToInt16(tID);
                            tNewNodePath += "Node" + String.Format("{0:00000}", temp);
                        }

                        else
                        {
                            int temp = Convert.ToInt16(tID);
                            tNewNodePath += "Patch" + String.Format("{0:00000}", temp);
                        }

                    }
                }
            }

            if (ContainsDescriptivName)
            {
                return  tNewNodePath;
            }
            else
            {
                return tNewNodePath;
            }
        }

        public static string CreateSliceID(string pNodePath, int pSliceId)
        {
            string[] tPatchIDs = pNodePath.Split('/');
            string tNewNodePath = String.Empty;
            bool ContainsDescriptivName = false;

            for (int i = 0; i < tPatchIDs.Length; i++)
            {
                string tID = tPatchIDs[i];

                if (tID != "")
                {
                    if (Regex.IsMatch(tID, "[a-zA-z]"))
                    {
                        tNewNodePath = tID;
                        ContainsDescriptivName = true;
                    }
                    else
                    {
                        if (i == tPatchIDs.Length - 1)
                        {
                            int temp = Convert.ToInt16(tID);
                            tNewNodePath += "Node" + String.Format("{0:00000}", temp);
                        }

                        else
                        {
                            int temp = Convert.ToInt16(tID);
                            tNewNodePath += "Patch" + String.Format("{0:00000}", temp);
                        }

                    }
                }
            }

            if (ContainsDescriptivName)
            {
                return tNewNodePath + "Slice" +String.Format("{0:00000}", pSliceId); ;
            }
            else
            {
                return tNewNodePath + "Slice" + String.Format("{0:00000}", pSliceId); ;
            }
            
            
        }

        #endregion conversion


        #region HtmlSnippets

        public static string ErrorMessage(string pFilename, string ErrorMessage)
        {

            return
            String.Format(@"
            <!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
            <html><head>
            <title>{1}</title>
            </head><body>
            <h1>Not Found</h1>
            <p>The requested File /{0} was not found on this server.</p>
            <hr>
            </body></html>
            ", pFilename, ErrorMessage);
        }


        #endregion
    }
}

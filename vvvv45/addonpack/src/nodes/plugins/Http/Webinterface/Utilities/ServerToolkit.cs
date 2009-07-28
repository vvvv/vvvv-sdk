using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Management;
using System.Net;

namespace VVVV.Webinterface.Utilities
{
    class ServerToolkit
    {

        public static List<string> GetLocalIPAdresses()
        {
            List<string> mIpList = new List<string>();

            try
            {

                ManagementClass tMc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection tMoc = tMc.GetInstances();
                foreach (ManagementObject tMo in tMoc)
                {
                    if ((bool)tMo["IpEnable"])
                    {
                        string[] ipAdresses = (string[])tMo["IPAdresses"];
                        foreach (string ipAdress in ipAdresses)
                        {
                            mIpList.Add(ipAdress);
                        }
                    }

                }
            }
            catch
            {
                mIpList.Add("No IP Found");
            }

            return mIpList;
        }
    }
}

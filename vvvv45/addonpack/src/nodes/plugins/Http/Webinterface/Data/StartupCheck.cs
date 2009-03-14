/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Phlegma
 * Datum: 19.12.2008
 * Zeit: 03:38
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using VVVV.Webinterface.Utilities;

namespace VVVV.Webinterface.Data
{
	/// <summary>
	/// Description of StartupCheck.
	/// </summary>
	public class StartupCheck
	{

		private string mStartupPath = Application.StartupPath;
        private string mStartupFolder = Application.StartupPath;
		private string mSystemXmlFile;
		private List<string> mStartupPathSubFolders = new List<string>();
        private List<string> mStartupPathSubFoldersNames = new List<string>();

		public string StartupPath
		{
			get
			{
				return mStartupPath;
					
			}
		}

        public string SartupFolder
        {
            get
            {
                return mStartupFolder;
            }

            set
            {

                mStartupFolder = Path.Combine(mStartupPath, value);
            }
        }
		
		public string StartupSubFolder 
		{
			set
			{
               
                    if (mStartupPathSubFoldersNames.Contains(value) == false)
                    {
                        mStartupPathSubFoldersNames.Add(value);
                        mStartupPathSubFolders.Add(Path.Combine(mStartupFolder,value));
                    }
                    else
                    {
                        return;
                    }
			}
		}
		
		public string XmlFileName
		{
			get
			{
				return mSystemXmlFile;
			}
			set
			{
				mSystemXmlFile = mStartupPath + "/" + value +".xml";
			}
		}
		
		public StartupCheck()
		{
		
		}
		
		public void CheckifStartupPathExist()
		{
			if(System.IO.Directory.Exists(mStartupFolder) == false)
			{
				System.IO.Directory.CreateDirectory(mStartupFolder);
                Debug.WriteLine("create Path: " + mStartupFolder);
                foreach (string pValue in mStartupPathSubFoldersNames)
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(mStartupFolder,pValue));
                    Debug.WriteLine("create Path: " + pValue);
                }
			}
			else
			{
                Debug.WriteLine("StartupPath:" + mStartupPath + "does exist");
                foreach (string pSubfolder in mStartupPathSubFolders)
                {
                    if (System.IO.Directory.Exists(pSubfolder) == false)
                    {
                        System.IO.Directory.CreateDirectory(pSubfolder);
                    }
                    else
                    {
                        Debug.WriteLine("Path: " + pSubfolder + " existes");
                    }
                }
			}
		}
		

        public string getSubFolderPath(string pName)
        {
            if (mStartupPathSubFoldersNames.Contains(pName) == true)
            {
                return Path.Combine(mStartupFolder, pName);
            }
            else
            {
                return mStartupFolder;
            }
            
        }


		public bool CheckIfXMLFileExist(string pFilename)
		{
			if(System.IO.File.Exists(pFilename))
			{
			   	Debug.WriteLine("Xml File in " + pFilename + " exist");
			   	return true;
			}
			else
			{
				Debug.WriteLine("Xml does not exist");
				return false;
			}
		}
	}
	
}

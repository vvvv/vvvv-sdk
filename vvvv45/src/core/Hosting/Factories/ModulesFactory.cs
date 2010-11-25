using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
    /// <summary>
    /// Effects factory, parses and watches the effect directory
    /// </summary>
    [Export(typeof(IAddonFactory))]
    public class ModulesFactory : AbstractFileFactory<IAddonHost>
    {
        private string FDTD = "";
        
        [Import]
    	protected ILogger Logger { get; set; }
        
        public ModulesFactory()
        	: base(".v4p")
        {
        }
        
        public override string JobStdSubPath {
			get {
				return "modules";
			}
		}

        //create a node info from a filename
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            if (FDTD == "") LoadDTD();

            //check extension
            if (Path.GetExtension(filename) != FileExtension) yield break;
            
            //check filename structure
            string fn = Path.GetFileNameWithoutExtension(filename);
            if (!Regex.IsMatch(fn, @"^.+\s\(.+\)$")) yield break;
            
            //match the filename
            var match = Regex.Match(fn, @"(\S+) \((\S+)(?: ([^)]*))?\)");
            
            //create node info and read matches
            var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
            	match.Groups[1].Value,
            	match.Groups[2].Value,
            	match.Groups[3].Value,
            	filename);
            
            nodeInfo.BeginUpdate();
            nodeInfo.Type = NodeType.Module;
            nodeInfo.Factory = this;
            nodeInfo.InitialBoxSize = new System.Drawing.Size(320, 240);
            nodeInfo.InitialWindowSize = new System.Drawing.Size(600, 400);
            
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    //skip first line
                    var s = sr.ReadLine();
                    
                    var settings = new XmlReaderSettings();
                    settings.ProhibitDtd = false;
                    
                    var xmlReader = XmlReader.Create((new StringReader(FDTD + sr.ReadToEnd())), settings);
                    //xmlReader.Settings
                    if(xmlReader.ReadToFollowing("INFO"))
                    {
                        nodeInfo.Author = xmlReader.GetAttribute("author");
                        nodeInfo.Help = xmlReader.GetAttribute("description");
                        nodeInfo.Tags = xmlReader.GetAttribute("tags");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Could not extract module info of " + nodeInfo.Systemname);
                Logger.Log(e);
                yield break;
            }
            finally
            {
            	nodeInfo.CommitUpdate();
            }
            
            yield return nodeInfo;
        }
        
        protected override bool CreateNode(INodeInfo nodeInfo, IAddonHost nodeHost)
        {
        	// Will never get called.
        	return true;
        }
        
        protected override bool DeleteNode(INodeInfo nodeInfo, IAddonHost nodeHost)
        {
        	// Will never get called.
        	return true;
        }
        
        //get the dtd string
        private void LoadDTD()
        {
        	var path = Shell.CallerPath.ConcatPath(@"..");
        	path = Path.GetFullPath(path);
            var files = Directory.GetFiles(path, "*.dtd");
            
            if (files.Length>0)
	            using (StreamReader sr = new StreamReader(files[0]))
	            {
	                //add the DOCTYPE definition to place the DTD inline
	                FDTD = sr.ReadLine();
	                FDTD += @"<!DOCTYPE PATCH [";
	                FDTD += sr.ReadToEnd();
	                FDTD += @"]>";
	            }
        }
    }
}

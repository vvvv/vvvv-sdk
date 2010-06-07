using System;
using System.Collections.Generic;

using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of CategoryEntryContentProvider.
	/// </summary>
	public class CategoryEntryProvider: ITreeContentProvider, ILabelProvider
	{
	    Dictionary<string, string> FCategoryDict = new Dictionary<string, string>();
		public CategoryEntryProvider()
		{
		    FCategoryDict.Add("2d", "Geometry in 2d, like connecting lines, calculating coordinates etc.");
		    FCategoryDict.Add("3d", "Geometry in 3d.");
		    FCategoryDict.Add("4d", "");
		    FCategoryDict.Add("Animation", "Things which will animate over time and therefore have an internal state; Generate motion, smooth and filter motion, record and store values. FlipFlops and other Logic nodes.");
		    FCategoryDict.Add("Astronomy", "Everything having to do with the Earth and the Universe; Current Time, calculation of earth, moon and sun’s parameters.");
		    FCategoryDict.Add("Boolean", "Logic Operators.");
		    FCategoryDict.Add("Color", "Working with color, color addition, subtraction, blending, color models etc.");
		    FCategoryDict.Add("Debug", "Displaying system status information in various undocumented formats.");
		    FCategoryDict.Add("Devices", "Control external devices, and get data from them.");
		    FCategoryDict.Add("Differential", "Create ultra smooth motions by working with position and velocity at the same time.");
		    FCategoryDict.Add("DShow9", "Audio and Video playback and effects based on Microsofts DirectShow Framework.");
		    FCategoryDict.Add("DX9", "DirectX9 based rendering system");
		    FCategoryDict.Add("Enumerations", "Work with enumerated data types");
		    FCategoryDict.Add("EX9", "The DirectX9 based rendering system made more Explicit. So geometry generation is separated from geometry display in the shader.");
		    FCategoryDict.Add("File", "Operations on the file system. Read, write, copy, delete, parse files etc.");
		    FCategoryDict.Add("Flash", "Everything related to rendering Flash content.");
		    FCategoryDict.Add("GDI", "Old school simple rendering system. Simple nodes for didactical use and lowtek graphics.");
		    FCategoryDict.Add("HTML", "Nodes making use of HTML strings local or on the internet");
		    FCategoryDict.Add("Network", "Internet functionality like HTTP, IRC, UDP, TCP, ...");
		    FCategoryDict.Add("Node", "Operations on the generic so called node pins.");
		    FCategoryDict.Add("ODE", "The Open Dynamics Engine for physical behaviour.");
		    FCategoryDict.Add("Quaternion", "Work with Quaternion vectors for rotations.");
		    FCategoryDict.Add("Spectral", "Operations for reducing value spreads to some few values. Summing, Averaging etc.");
		    FCategoryDict.Add("Spreads", "Operations creating value spreads out of few key values. Also spread operations.");
		    FCategoryDict.Add("String", "String functions, appending, searching, sorting, string spread and spectral operations.");
		    FCategoryDict.Add("System", "Control of built in hardware, like mouse, keyboard, sound card mixer, power management etc.");
		    FCategoryDict.Add("Transforms", "Nodes for creating and manipulating 3d-transformations.");
		    FCategoryDict.Add("TTY", "Old school tty console rendering system for printing out status and debug messages.");
		    FCategoryDict.Add("Value", "Everything dealing with numercial values: Mathematical operations, ...");
		    FCategoryDict.Add("VVVV", "Everything directly related to the running vvvv instance: Command line parameters, Event outputs, Quit command, ...");
		    FCategoryDict.Add("Windows", "Control Windows´ Windows, Desktop Icons etc.");
		}
		
		public void Dispose()
		{
		}
		
		public string GetText(object element)
		{
		    return (element as CategoryEntry).Name;
		}
	    
        public event EventHandler LabelChanged;
	    
        public event EventHandler ContentChanged;
	    
        System.Collections.IEnumerable ITreeContentProvider.GetChildren(object element)
        {
            return (element as CategoryEntry).NodeInfoEntries;
        }
	    
        public string GetToolTip(object element)
        {
            if (FCategoryDict.ContainsKey((element as CategoryEntry).Name))
                return FCategoryDict[(element as CategoryEntry).Name];
            else 
                return "";
        }
	}
}

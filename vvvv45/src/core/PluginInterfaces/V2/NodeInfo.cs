using System;
using System.Runtime.InteropServices;
using System.Drawing;

using VVVV.Core.Model;
using VVVV.Core.Runtime;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    #region INodeInfo
	/// <summary>
	/// Interface for the <see cref="VVVV.PluginInterfaces.V2.INodeInfo">INodeInfo</see>. Also see <a href="http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming" target="_blank">VVVV Naming Conventions</a>.
	/// </summary>
	[Guid("581998D6-ED08-4E73-821A-46AFF59C78BD"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeInfo : IPluginInfo, IDisposable
	{
//		/// <summary>
//		/// The nodes main visible name. Use CamelCaps and no spaces.
//		/// </summary>
//		string Name {get;}
//		/// <summary>
//		/// The category in which the plugin can be found. Try to use an existing one.
//		/// </summary>
//		string Category {get;}
//		/// <summary>
//		/// Optional. Leave blank if not needed to distinguish two nodes of the same name and category.
//		/// </summary>
//		string Version {get;}
		/// <summary>
		/// Arguments used by the IAddonFactory to create this node.
		/// </summary>
		string Arguments {get; set;}
		/// <summary>
		/// Name of the file used by the IAddonFactory to create this node.
		/// </summary>
		string Filename {get;}
		/// <summary>
		/// The nodes unique username in the form of: Name (Category Version) where the Name can be a symbol
		/// </summary>
		string Username {get;}
		/// <summary>
		/// The nodes unique systemname in the form of: Name (Category Version)
		/// </summary>
		string Systemname {get;}
		/// <summary>
		/// The node type. Set by the PluginFactory.
		/// </summary>
		NodeType Type {get; set;}
		/// <summary>
		/// Arbritary user data. Can be used by factories to store additional data.
		/// </summary>
		object UserData { get; set; }
		/// <summary>
		/// Define if this node should be evaluated every frame, even if no outpur is read.
		/// </summary>
		bool AutoEvaluate {get; set;}
		/// <summary>
		/// Define if this node should be ignored in a NodeBrowser or not.
		/// </summary>
		bool Ignore {get; set;}
		/// <summary>
		/// Disables events. Call this function if bunch of properties will get changed.
		/// </summary>
		void BeginUpdate();
		/// <summary>
		/// Enables events. Call this function after a BeginUpdate(). Will trigger a NodeInfoUpdated event.
		/// </summary>
		void CommitUpdate();
	}
	
	public static class NodeInfoExtensionMethods
	{
		public static void UpdateFromNodeInfo(this INodeInfo nodeInfo, INodeInfo otherNodeInfo)
		{
		    nodeInfo.Shortcut = otherNodeInfo.Shortcut;
			nodeInfo.Author = otherNodeInfo.Author;
			nodeInfo.Help = otherNodeInfo.Help;
			nodeInfo.Tags = otherNodeInfo.Tags;
			nodeInfo.Bugs = otherNodeInfo.Bugs;
			nodeInfo.Credits = otherNodeInfo.Credits;
			nodeInfo.Warnings = otherNodeInfo.Warnings;

			nodeInfo.Namespace = otherNodeInfo.Namespace;
			nodeInfo.Class = otherNodeInfo.Class;
			nodeInfo.InitialBoxSize = otherNodeInfo.InitialBoxSize;
			nodeInfo.InitialComponentMode = otherNodeInfo.InitialComponentMode;
			nodeInfo.InitialWindowSize = otherNodeInfo.InitialWindowSize;
		
			nodeInfo.Arguments = otherNodeInfo.Arguments;
			nodeInfo.Type = otherNodeInfo.Type;
			nodeInfo.UserData = otherNodeInfo.UserData;
			nodeInfo.AutoEvaluate = otherNodeInfo.AutoEvaluate;
			nodeInfo.Ignore = otherNodeInfo.Ignore;
		}
		
		public static void UpdateFromPluginInfo(this INodeInfo nodeInfo, IPluginInfo pluginInfo)
		{
		    nodeInfo.Shortcut = pluginInfo.Shortcut;
			nodeInfo.Author = pluginInfo.Author;
			nodeInfo.Help = pluginInfo.Help;
			nodeInfo.Tags = pluginInfo.Tags;
			nodeInfo.Bugs = pluginInfo.Bugs;
			nodeInfo.Credits = pluginInfo.Credits;
			nodeInfo.Warnings = pluginInfo.Warnings;

			nodeInfo.Namespace = pluginInfo.Namespace;
			nodeInfo.Class = pluginInfo.Class;
			nodeInfo.InitialBoxSize = pluginInfo.InitialBoxSize;
			nodeInfo.InitialComponentMode = pluginInfo.InitialComponentMode;
			nodeInfo.InitialWindowSize = pluginInfo.InitialWindowSize;
		}
	}
	#endregion INodeInfo
}


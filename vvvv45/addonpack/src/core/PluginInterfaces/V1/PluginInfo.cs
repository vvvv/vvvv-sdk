using System;
using System.Runtime.InteropServices;
using System.Drawing;

using VVVV.Core.Model;

namespace VVVV.PluginInterfaces.V1
{
    #region IPluginInfo
	/// <summary>
	/// Interface for the <see cref="VVVV.PluginInterfaces.V1.PluginInfo">PluginInfo</see>. Also see <a href="http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming" target="_blank">VVVV Naming Conventions</a>.
	/// </summary>
	[Guid("16EE5CF9-0D75-4ECF-9440-7D2909E8F7DC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginInfo
	{
		/// <summary>
		/// The nodes main visible name. Use CamelCaps and no spaces.
		/// </summary>
		string Name {get; set;}
		/// <summary>
		/// The category in which the plugin can be found. Try to use an existing one.
		/// </summary>
		string Category {get; set;}
		/// <summary>
		/// Optional. Leave blank if not needed to distinguish two nodes of the same name and category.
		/// </summary>
		string Version {get; set;}
		/// <summary>
		/// Optional. Shortcut to create an instance of this node.
		/// </summary>
		string Shortcut {get; set;}
		/// <summary>
		/// Describe the nodes function in a few words.
		/// </summary>
		string Help {get; set;}
		/// <summary>
		/// Specify a comma separated list of tags that describe the node. Name, category and Version don't need to be duplicated here.
		/// </summary>
		string Tags {get; set;}
		/// <summary>
		/// Specify the plugins author.
		/// </summary>
		string Author {get; set;}
		/// <summary>
		/// Give credits to thirdparty code used.
		/// </summary>
		string Credits {get; set;}
		/// <summary>
		/// Specify known problems.
		/// </summary>
		string Bugs {get; set;}
		/// <summary>
		/// Specify any usage of the node that may cause troubles.
		/// </summary>
		string Warnings {get; set;}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in window-mode.
		/// </summary>
		Size InitialWindowSize {get; set;}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in box-mode.
		/// </summary>
		Size InitialBoxSize {get; set;}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial component mode.
		/// </summary>
		TComponentMode InitialComponentMode {get; set;}
		
		/// <summary>
		/// The nodes namespace. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		string Namespace {get; set;}
		/// <summary>
		/// The nodes classname. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		string Class {get; set;}
	}
	#endregion IPluginInfo
	
	#region PluginInfo
	/// <summary>
	/// Helper Class that implements the <see cref="VVVV.PluginInterfaces.V1.IPluginInfo">IPluginInfo</see> interface.
	/// </summary>
	[Guid("FE1216D6-5439-416D-8FB7-16E9A29EF67B")]
	[Serializable]
	[ComVisible(false)]
	public class PluginInfo: MarshalByRefObject, IPluginInfo
	{
		private string FName = "";
		private string FCategory = "";
		private string FVersion = "";
		private string FShortcut = "";
		private string FAuthor = "";
		private string FHelp = "";
		private string FTags = "";
		private string FBugs = "";
		private string FCredits = "";
		private string FWarnings = "";
		private string FNamespace = "";
		private string FClass = "";
		private Size FInitialWindowSize = new Size(0, 0);
		private Size FInitialBoxSize = new Size(0, 0);
		private TComponentMode FInitialComponentMode = TComponentMode.Hidden;
		
		/// <summary>
		/// The nodes main visible name. Use CamelCaps and no spaces.
		/// </summary>
		public string Name
		{
			get {return FName;}
			set {FName = value;}
		}
		/// <summary>
		/// The category in which the plugin can be found. Try to use an existing one.
		/// </summary>
		public string Category
		{
			get {return FCategory;}
			set {FCategory = value;}
		}
		/// <summary>
		/// Optional. Leave blank if not needed to distinguish two nodes of the same name and category.
		/// </summary>
		public string Version
		{
			get {return FVersion;}
			set {FVersion = value;}
		}
		/// <summary>
		/// Optional. Shortcut to create an instance of this node.
		/// </summary>
		public string Shortcut 
		{
			get {return FShortcut;}
			set {FShortcut = value;}
		}
		/// <summary>
		/// Specify the plugins author.
		/// </summary>
		public string Author
		{
			get {return FAuthor;}
			set {FAuthor = value;}
		}
		/// <summary>
		/// Describe the nodes function in a few words.
		/// </summary>
		public string Help
		{
			get {return FHelp;}
			set {FHelp = value;}
		}
		/// <summary>
		/// Specify a comma separated list of tags that describe the node. Name, category and Version don't need to be duplicated here.
		/// </summary>
		public string Tags
		{
			get {return FTags;}
			set {FTags = value;}
		}
		/// <summary>
		/// Specify known problems.
		/// </summary>
		public string Bugs
		{
			get {return FBugs;}
			set {FBugs = value;}
		}
		/// <summary>
		/// Give credits to thirdparty code used.
		/// </summary>
		public string Credits
		{
			get {return FCredits;}
			set {FCredits = value;}
		}
		/// <summary>
		/// Specify any usage of the node that may cause troubles.
		/// </summary>
		public string Warnings
		{
			get {return FWarnings;}
			set {FWarnings = value;}
		}
		/// <summary>
		/// The nodes namespace. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		public string Namespace
		{
			get {return FNamespace;}
			set {FNamespace = value;}
		}
		/// <summary>
		/// The nodes classname. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		public string Class
		{
			get {return FClass;}
			set {FClass = value;}
		}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in window-mode.
		/// </summary>
		public Size InitialWindowSize
		{
			get {return FInitialWindowSize;}
			set {FInitialWindowSize = value;}
		}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in box-mode.
		/// </summary>
		public Size InitialBoxSize
		{
			get {return FInitialBoxSize;}
			set {FInitialBoxSize = value;}
		}
		
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial component mode.
		/// </summary>
		public TComponentMode InitialComponentMode
		{
			get {return FInitialComponentMode;}
			set {FInitialComponentMode = value;}
		}
	}
	#endregion PluginInfo
}


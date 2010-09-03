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
	public interface INodeInfo : IPluginInfo
	{
		/// <summary>
		/// Arguments used by the PluginFactory to create this node.
		/// </summary>
		string Arguments {get; set;}
		/// <summary>
		/// Name of the file used by the PluginFactory to create this node.
		/// </summary>
		string Filename {get; set;}
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
		/// Reference to the <see cref="IExecutable">IExecutable</see> which was used to create this node. Set by the AddonFactory.
		/// </summary>
		IExecutable Executable {get; set;}
		/// <summary>
		/// Define if this node should be evaluated every frame, even if no outpur is read.
		/// </summary>
		bool AutoEvaluate {get; set;}
		/// <summary>
		/// Define if this node should be ignored in a NodeBrowser or not.
		/// </summary>
		bool Ignore {get; set;}
	}
	#endregion INodeInfo
	
	#region NodeInfo	
	/// <summary>
	/// Helper Class that implements the <see cref="INodeInfo">INodeInfo</see> interface.
	/// </summary>
	[Guid("36F845F4-A486-49EC-9A0C-CB254FF2B297")]
	[Serializable]
	public class NodeInfo: PluginInfo, INodeInfo
	{
		private string FArguments = "";
		private string FFilename = "";
		private NodeType FType = NodeType.Plugin;
		[NonSerialized]
		private IExecutable FExcecutable = null;
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		public NodeInfo ()
		{  
		}
		
		/// <summary>
		/// Creates a new NodeInfo from an existing <see cref="VVVV.PluginInterfaces.V1.IPluginInfo">IPluginInfo</see>.
		/// </summary>
		/// <param name="Info">The existing plugin <see cref="VVVV.PluginInterfaces.V1.IPluginInfo">IPluginInfo</see>.</param>
		public NodeInfo (IPluginInfo Info)
		{
		    this.Name = Info.Name;
		    this.Category = Info.Category;
		    this.Version = Info.Version;
		    this.Shortcut = Info.Shortcut;
			this.Author = Info.Author;
			this.Help = Info.Help;
			this.Tags = Info.Tags;
			this.Bugs = Info.Bugs;
			this.Credits = Info.Credits;
			this.Warnings = Info.Warnings;

			this.Namespace = Info.Namespace;
			this.Class = Info.Class;
			this.InitialBoxSize = Info.InitialBoxSize;
			this.InitialComponentMode = Info.InitialComponentMode;
			this.InitialWindowSize = Info.InitialWindowSize;
			this.Ignore = false;
		}
		
		/// <summary>
		/// Arguments used by the PluginFactory to create this node.
		/// </summary>
		public string Arguments
		{
			get {return FArguments;}
			set {FArguments = value;}
		}
		
		/// <summary>
		/// Name of the file used by the PluginFactory to create this node.
		/// </summary>
		public string Filename 
		{
			get {return FFilename;}
			set {FFilename = value;}
		}
		
		/// <summary>
		/// The nodes unique username in the form of: Name (Category Version) where the Name can be a symbol
		/// </summary>
		public string Username 
		{
			get 
			{
			    if (string.IsNullOrEmpty(this.Version))
					return this.Name + " (" + this.Category + ")";
				else
					return this.Name + " (" + this.Category + " " + this.Version + ")";
			}
		}
		
		/// <summary>
		/// The nodes unique username in the form of: Name (Category Version)
		/// </summary>
		public string Systemname 
		{
			get 
			{
			    if (string.IsNullOrEmpty(this.Version))
					return this.Name + " (" + this.Category + ")";
				else
					return this.Name + " (" + this.Category + " " + this.Version + ")";
			}
		}
		
		/// <summary>
		/// The node type. Set by the PluginFactory.
		/// </summary>
		public NodeType Type 
		{
			get {return FType;}
			set {FType = value;}
		}
		
		/// <summary>
		/// Reference to the <see cref="IExecutable">IExecutable</see> which was used to create this node. Set by the PluginFactory.
		/// </summary>
		public IExecutable Executable 
		{
			get {return FExcecutable;}
			set {FExcecutable = value;}
		}
		
		/// <summary>
		/// Define if this node should be evaluated every frame, even if no outpur is read.
		/// </summary>
		public bool AutoEvaluate
		{
			get;
			set;
		}
		
		/// <summary>
		/// Define if this node should be ignored in a NodeBrowser or not.
		/// </summary>
		public bool Ignore
		{
			get;
			set;
		}
		
        public override bool Equals(object obj)
        {
            INodeInfo ni = null;
            if (obj is INodeInfo)
                ni = obj as INodeInfo;    
            else
                return false;
            
            return (this.Systemname == ni.Systemname); 
            /*    && (this.Author == ni.Author) 
                && (this.Warnings == ni.Warnings)
                && (this.Class == ni.Class)
                && (this.Credits == ni.Credits)
                && (this.Filename == ni.Filename)
                && (this.Help == ni.Help)
                && (this.InitialBoxSize == ni.InitialBoxSize)
                && (this.InitialComponentMode == ni.InitialComponentMode)
                && (this.InitialWindowSize == ni.InitialWindowSize)
                && (this.Namespace == ni.Namespace)
                && (this.Shortcut == ni.Shortcut)
                && (this.Tags == ni.Tags);*/
        }
        
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
	#endregion NodeInfo
}


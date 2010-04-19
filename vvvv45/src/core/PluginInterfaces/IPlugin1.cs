using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.HDE.Model;

namespace VVVV.PluginInterfaces.V1
{
	#region plugin
	/// <summary>
	/// The one single interface a plugin has to implement
	/// </summary>
	[Guid("084BB2C9-E8B4-4575-8611-C262399B2A95"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginBase
	{
		/// <summary>
		/// Called by the PluginHost to hand itself over to the plugin. This is where the plugin creates its initial pins.
		/// </summary>
		/// <param name="Host">Interface to the PluginHost.</param>
		void SetPluginHost(IPluginHost Host);
	}
	
	/// <summary>
	/// The one single interface a plugin has to implement
	/// </summary>
	[Guid("7F813C89-4EDE-4087-A626-4320BE41C87F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPlugin: IPluginBase
	{
		/// <summary>
		/// Called by the PluginHost before the Evaluate function every frame for every ConfigurationPin that has changed. 
		/// The ConfigurationPin is handed over as the functions input parameter. This is where a plugin would typically 
		/// create/delete pins as reaction to the changed value of a ConfigurationPin that specifies the number of pins of a specific type.
		/// </summary>
		/// <param name="input">Interface to the ConfigurationPin for which the function is called.</param>
		void Configurate(IPluginConfig input);
		/// <summary>
		/// Called by the PluginHost once per frame. This is where the plugin calculates and sets the SliceCounts and Values
		/// of its outputs depending on the values of its current inputs.
		/// </summary>
		/// <param name="SpreadMax">The maximum SliceCount of all of the plugins inputs, which would typically be used
		/// to adjust the SliceCounts of all outputs accordingly.</param>
		void Evaluate(int SpreadMax);
		/// <summary>
		/// Called by the PluginHost only once during initialization to find out if this plugin needs to be evaluated
		/// every frame even if there is not output connected. Typically this can return FALSE as long as the plugin doesn't have
		/// a special reason for doing otherwise.
		/// </summary>
		bool AutoEvaluate {get;}
	}
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that needs to know when one of its pins is connected or disconnected
	/// </summary>
	[Guid("B77C459E-E561-424B-AB3A-572C9BB6CD93"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConnections
	{
		/// <summary>
		/// Called by the PluginHost for every input or output that is being connected. This is typically useful for 
		/// NodeIO Inputs that can cache a reference to the upstream interface at this place instead of getting the reference
		/// every frame in Evaluate.
		/// </summary>
		/// <param name="pin">Interface to the pin for which the function is called.</param>
		void ConnectPin(IPluginIO pin);
		/// <summary>
		/// Called by the PluginHost for every input or output that is being disconnected. This is typically useful for 
		/// NodeIO Inputs that can set a cached reference to the upstream interface to null at this place.
		/// </summary>
		/// <param name="pin">Interface to the pin for which the function is called.</param>
		void DisconnectPin(IPluginIO pin);
	}

	#endregion plugin
	
	#region PluginDXInterfaces
	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with DirectX resources like Meshes, Textures, Layers...
	/// </summary>
	[Guid("1BDD5442-8113-4EF4-9951-906633170D8C"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost every frame for every device. Therefore a plugin should only do 
		/// device specific operations here and still keep node specific calculations in the Evaluate call. 
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="OnDevice">Pointer to the device on which the resources is to be updated.</param>
		void UpdateResource(IPluginOut ForPin, int OnDevice);
		/// <summary>
		/// Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
		/// This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="OnDevice">Pointer to the device on which the resources is to be destroyed.</param>
		/// <param name="OnlyUnManaged">If True only unmanaged DirectX resources need to be destroyed.</param>
		void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged);
	}
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with DirectX Meshes
	/// </summary>
	[Guid("E0DF9FCE-327E-4492-9C03-BA513CF93FC4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXMesh: IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost everytime a mesh is accessed via a pin on the plugin.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than handing back the right mesh.
		/// </summary>
		/// <param name="ForPin">Interface to the pin via which the mesh is accessed.</param>
		/// <param name="OnDevice">Pointer to the device for which the mesh is accessed.</param>
		/// <param name="Mesh">The retrieved mesh</param>
		void GetMesh(IDXMeshOut ForPin, int OnDevice, out int Mesh);
	}
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with DirectX Textures
	/// </summary>
	[Guid("A679DDC2-3740-4FDE-9CCF-5EB290A3433B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXTexture: IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost everytime a texture is accessed via a pin on the plugin.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than handing back the right texture.
		/// </summary>
		/// <param name="ForPin">Interface to the pin via which the texture is accessed.</param>
		/// <param name="OnDevice">Pointer to the device for which the texture is accessed.</param>
		/// <param name="Texture">The retrieved mesh</param>
		void GetTexture(IDXTextureOut ForPin, int OnDevice, out int Texture);
	}

	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with DirectX Layers
	/// </summary>
	[Guid("14F2AA87-EF8B-4A93-8F67-7CCA3F5E3522"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXLayer: IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost everytime it needs to update its StateBlock. Here the plugin
		/// must specify all States it will set during <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.Render()">IPluginDXLayer.Render</see>
		/// via calls to <see cref="VVVV.PluginInterfaces.V1.IDXRenderStateIn">IDXRenderStateIn</see>'s and <see cref="VVVV.PluginInterfaces.V1.IDXSamplerStateIn">IDXSamplerStateIn</see>'s functions.
		/// </summary>
		void SetStates();
		/// <summary>
		/// Called by the PluginHost everytime the plugin is supposed to render itself.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than some drawing calls.
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="OnDevice">Pointer to the device on which the plugin is supposed to render.</param>
		void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice);
	}  
	
	/// <summary>
	/// Interface to access the hosts current DirectX device. Available as input parameter to any of the IPluginDXResource functions 
	/// </summary>
	[Guid("765B10CB-4CA9-4927-B1DF-A8FB67692267"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXDevice
	{
		int DevicePointer();
	}
	#endregion PluginDXInterfaces
	
	#region plugin info
	
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
	
	/// <summary>
	/// Interface for the <see cref="VVVV.PluginInterfaces.V1.INodeInfo">INodeInfo</see>. Also see <a href="http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming" target="_blank">VVVV Naming Conventions</a>.
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
		/// The nodes unique username in the form of: Name (Category Version)
		/// </summary>
		string Username {get;}
		/// <summary>
		/// The node type. Set by the PluginFactory.
		/// </summary>
		TNodeType Type {get; set;}
		/// <summary>
		/// Reference to the <see cref="VVVV.HDE.Interfaces.IExecutable">IExecutable</see> which was used to create this node. Set by the PluginFactory.
		/// </summary>
		IExecutable Executable {get; set;}
	}
	
	/// <summary>
	/// Helper Class that implements the <see cref="VVVV.PluginInterfaces.V1.IPluginInfo">IPluginInfo</see> interface.
	/// </summary>
	[Guid("FE1216D6-5439-416D-8FB7-16E9A29EF67B")]
	public class PluginInfo: MarshalByRefObject, IPluginInfo
	{
		private string FName = "";
		private string FCategory = "";
		private string FVersion = "";
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
	
	/// <summary>
	/// Helper Class that implements the <see cref="VVVV.PluginInterfaces.V1.INodeInfo">INodeInfo</see> interface.
	/// </summary>
	[Guid("36F845F4-A486-49EC-9A0C-CB254FF2B297")]
	public class NodeInfo: PluginInfo, INodeInfo
	{
		
		private string FArguments = "";
		private string FFilename = "";
		private TNodeType FType = TNodeType.Plugin;
		private IExecutable FExcecutable = null;
		
		/// <summary>
		/// Creates a new NodeInfo from an existing <see cref="VVVV.PluginInterfaces.V1.IPluginInfo">IPluginInfo</see>.
		/// </summary>
		/// <param name="PluginInfo"></param>
		public NodeInfo (IPluginInfo Info)
		{
			this.Author = Info.Author;
			this.Bugs = Info.Bugs;
			this.Category = Info.Category;
			this.Class = Info.Class;
			this.Credits = Info.Credits;
			this.Help = Info.Help;
			this.InitialBoxSize = Info.InitialBoxSize;
			this.InitialComponentMode = Info.InitialComponentMode;
			this.InitialWindowSize = Info.InitialWindowSize;
			this.Name = Info.Name;
			this.Namespace = Info.Namespace;
			this.Tags = Info.Tags;
			this.Version = Info.Version;
			this.Warnings = Info.Warnings;
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
		/// The nodes unique username in the form of: Name (Category Version)
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
		/// The node type. Set by the PluginFactory.
		/// </summary>
		public TNodeType Type 
		{
			get {return FType;}
			set {FType = value;}
		}
		
		/// <summary>
		/// Reference to the <see cref="VVVV.HDE.Interfaces.IExecutable">IExecutable</see> which was used to create this node. Set by the PluginFactory.
		/// </summary>
		public IExecutable Executable 
		{
			get {return FExcecutable;}
			set {FExcecutable = value;}
		}
	}
	
	#endregion plugin info
}

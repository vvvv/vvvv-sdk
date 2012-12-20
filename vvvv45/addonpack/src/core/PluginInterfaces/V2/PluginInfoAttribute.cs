using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.PluginInterfaces.V2
{
	[MetadataAttribute]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	[ComVisible(false)]
	public sealed class PluginInfoAttribute: ExportAttribute
	{
		public PluginInfoAttribute() : base(typeof(IPluginBase)) { }
		public string Name { get; set; }
		private string FCategory;
		public string Category 
		{
			get {return FCategory;}
			set 
			{
				if ((value == "2D") || (value == "3D") || (value == "4D"))
					value = value.ToLower();
				FCategory = value;
			}
		}
		public string Version { get; set; }
		public string Shortcut { get; set; }
		public string Author { get; set; }
		public string Help { get; set; }
		public string Tags { get; set; }
		public string Bugs { get; set; }
		public string Credits { get; set; }
		public string Warnings { get; set; }
		public int InitialWindowWidth { get; set; }
		public int InitialWindowHeight { get; set; }
		public int InitialBoxWidth { get; set; }
		public int InitialBoxHeight { get; set; }
		public TComponentMode InitialComponentMode { get; set; }
		public bool AutoEvaluate { get; set; }
		public bool Ignore { get; set; }
		
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
	}
}
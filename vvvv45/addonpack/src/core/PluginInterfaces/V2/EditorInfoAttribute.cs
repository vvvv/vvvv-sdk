using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.PluginInterfaces.V2
{
	[MetadataAttribute]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	[ComVisible(false)]
	public sealed class EditorInfoAttribute: ExportAttribute
	{
		public EditorInfoAttribute(params string[] fileExtensions)
			: base(typeof(IEditor)) 
		{ 
			FileExtensions = fileExtensions;
		}
		
		public string[] FileExtensions
		{
			get;
			private set;
		}
	}
}
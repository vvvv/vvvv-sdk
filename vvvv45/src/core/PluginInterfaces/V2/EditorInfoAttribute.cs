using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.PluginInterfaces.V2
{
	[MetadataAttribute]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public class EditorInfoAttribute: ExportAttribute
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
using System;
using System.IO;
using System.Web;

using VVVV.Core.Model;
using VVVV.Core.View;
using VVVV.Utils;

namespace VVVV.Core
{
	// Acts on models of type IReference
	public class ReferenceViewProvider: INamed, IDescripted
	{
		public ReferenceViewProvider (IReference reference)
		{
			string suffix = string.Empty;
			
			if (File.Exists(reference.AssemblyLocation))
				Description = PathUtils.MakeRelativePath(Path.GetDirectoryName(reference.Project.LocalPath), reference.AssemblyLocation);
			else
				suffix = " (missing)";
			
			Name = reference.Name + suffix;	
		}

		#region INamed implementation
		public string Name 
		{
			get;
			private set;
		}

		public event RenamedHandler Renamed;
		
		protected virtual void OnRenamed(string newName)
		{
			if (Renamed != null)
				Renamed(this, newName);
		}
		#endregion
		
		public string Description 
		{
			get;
			private set;
		}
	}
}


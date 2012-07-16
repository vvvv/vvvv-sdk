using System;
using System.Collections;
using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;
using VVVV.Core.View;

namespace VVVV.HDE.ProjectExplorer
{
	public class FXProjectViewProvider : ProjectViewProvider, IEnumerable
	{
		public FXProjectViewProvider(FXProject project, ModelMapper mapper)
			: base(project, mapper)
		{
			mapper.RegisterMapping<IEditableIDList<IReference>, INamed, FXReferenceListNameProvider>();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
        {
			// Hide empty includes
			if (FProject.References.Count > 0)
            	yield return FProject.References;
			
            foreach (var folder in FRootFolder) {
                yield return folder;
            }
        }
	}
	
	class FXReferenceListNameProvider : INamed
	{
		public event RenamedHandler Renamed;
		
		protected virtual void OnRenamed(string newName)
		{
			if (Renamed != null) {
				Renamed(this, newName);
			}
		}
		public string Name {
			get {
				return "Includes";
			}
		}
	}
}

using System;
using VVVV.Core;
using VVVV.Core.Menu;
using VVVV.Core.Model;
using VVVV.Core.View;

namespace VVVV.HDE.ProjectExplorer
{
	public class MsBuildProjectViewProvider : ProjectViewProvider
	{
		public MsBuildProjectViewProvider(MsBuildProject project, ModelMapper mapper)
			:base(project, mapper)
		{
			mapper.RegisterMapping<IEditableIDList<IReference>, IAddMenuProvider, ReferencesViewProvider>();
		}
	}
}

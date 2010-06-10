using System;
using VVVV.Utils.Unity;
using Microsoft.Practices.Unity;

namespace VVVV.Nodes.GraphViewer
{
	/// <summary>
	/// Description of GraphViewerModelContainerExtension.
	/// </summary>
	public class GraphViewerModelContainerExtension: UnityContainerExtension
	{
		protected override void Initialize()
		{
			Func<LifetimeManager> lifetimeManagerCreator = () => new ContainerControlledLifetimeManager();
			Container.RegisterInterfaces<PatchNodeProvider, PatchNode>(lifetimeManagerCreator);
		}
	}
}

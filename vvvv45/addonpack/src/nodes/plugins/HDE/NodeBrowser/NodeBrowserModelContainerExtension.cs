using System;
using VVVV.Utils.Unity;
using Microsoft.Practices.Unity;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of HdeModelProviderExtension.
	/// </summary>
	public class NodeBrowserModelContainerExtension: UnityContainerExtension
	{
		protected override void Initialize()
		{
			Func<LifetimeManager> lifetimeManagerCreator = () => new ContainerControlledLifetimeManager();
			Container.RegisterInterfaces<NodeListModelProvider, NodeListModel>(lifetimeManagerCreator);
			Container.RegisterInterfaces<CategoryEntryProvider, CategoryEntry>(lifetimeManagerCreator);
			Container.RegisterInterfaces<NodeInfoEntryProvider, NodeInfoEntry>(lifetimeManagerCreator);
		}
	}
}

using System;
using VVVV.Utils.Unity;
using Microsoft.Practices.Unity;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of HdeModelProviderExtension.
	/// </summary>
	public class NodeBrowserModelProviderExtension: UnityContainerExtension
	{
		protected override void Initialize()
		{
			Func<LifetimeManager> lifetimeManagerCreator = () => new ContainerControlledLifetimeManager();
			Container.RegisterInterfaces<NodeListModelContentProvider, AlphabetModel>(lifetimeManagerCreator);
			Container.RegisterInterfaces<NodeListModelContentProvider, CategoryModel>(lifetimeManagerCreator);
			Container.RegisterInterfaces<CategoryEntryContentProvider, CategoryEntry>(lifetimeManagerCreator);
			Container.RegisterInterfaces<NodeInfoContentProvider, NodeInfoDummy>(lifetimeManagerCreator);
		}
	}
}

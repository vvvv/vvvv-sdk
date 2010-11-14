using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.Core.Runtime;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	#region ProxyNodeInfo
	
	[Serializable]
	internal class ProxyNodeInfo : INodeInfo
	{
		[NonSerialized]
		private INodeInfo FNodeInfo;
		private bool FInUpdate;
		
		public ProxyNodeInfo(INodeInfo nodeInfo)
		{
			FNodeInfo = nodeInfo;
			
			Reload();
		}
		
		private bool FInInternalUpdate;
		void UpdateInternal()
		{
			if (FInInternalUpdate) return;
			
			FInInternalUpdate = true;
			
			try
			{
				FNodeInfo.UpdateFromNodeInfo(this);
			}
			finally
			{
				FInInternalUpdate = false;
			}
		}
		
		private bool FInReload;
		public void Reload()
		{
			if (FInReload) return;
			
			FInReload = true;
			
			bool inUpdate = FInUpdate;
			FInUpdate = true;
			
			try
			{
				Arguments = FNodeInfo.Arguments;
				Author = FNodeInfo.Author;
				AutoEvaluate = FNodeInfo.AutoEvaluate;
				Bugs = FNodeInfo.Bugs;
				Category = FNodeInfo.Category;
				Credits = FNodeInfo.Credits;
				Filename = FNodeInfo.Filename;
				Help = FNodeInfo.Help;
				Ignore = FNodeInfo.Ignore;
				InitialBoxSize = FNodeInfo.InitialBoxSize;
				InitialComponentMode = FNodeInfo.InitialComponentMode;
				InitialWindowSize = FNodeInfo.InitialWindowSize;
				Name = FNodeInfo.Name;
				Shortcut = FNodeInfo.Shortcut;
				Systemname = FNodeInfo.Systemname;
				Tags = FNodeInfo.Tags;
				Type = FNodeInfo.Type;
				UserData = FNodeInfo.UserData;
				Factory = FNodeInfo.Factory;
				Username = FNodeInfo.Username;
				Version = FNodeInfo.Version;
				Warnings = FNodeInfo.Warnings;
			}
			finally
			{
				FInUpdate = inUpdate;
				FInReload = false;
			}
		}
		
		private string FArguments;
		public string Arguments {
			get {
				return FArguments;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FArguments = string.Empty;
				else
					FArguments = value;
				
				if (!FInUpdate)
					FNodeInfo.Arguments = Arguments;
			}
		}
		
		private string FFilename;
		public string Filename {
			get {
				return FFilename;
			}
			private set {
				if (string.IsNullOrEmpty(value))
					FFilename = string.Empty;
				else
					FFilename = value;
			}
		}
		
		private string FUsername;
		public string Username {
			get {
				return FUsername;
			}
			private set {
				if (string.IsNullOrEmpty(value))
					FUsername = string.Empty;
				else
					FUsername = value;
			}
		}
		
		private string FSystemname;
		public string Systemname {
			get {
				return FSystemname;
			}
			private set {
				if (string.IsNullOrEmpty(value))
					FSystemname = string.Empty;
				else
					FSystemname = value;
			}
		}
		
		private NodeType FType;
		public NodeType Type {
			get {
				return FType;
			}
			set {
				FType = value;
				
				if (!FInUpdate)
					FNodeInfo.Type = Type;
			}
		}
		
		[NonSerialized]
		private object FUserData;
		public object UserData {
			get {
				return FUserData;
			}
			set {
				FUserData = value;
				
				if (!FInUpdate)
					FNodeInfo.UserData = UserData;
			}
		}
		
		[NonSerialized]
		private IAddonFactory FFactory;
		public IAddonFactory Factory {
			get {
				return FFactory;
			}
			set {
				FFactory = value;
				
				if (!FInUpdate)
					FNodeInfo.Factory = Factory;
			}
		}
		
		private bool FAutoEvaluate;
		public bool AutoEvaluate {
			get {
				return FAutoEvaluate;
			}
			set {
				FAutoEvaluate = value;
				
				if (!FInUpdate)
					FNodeInfo.AutoEvaluate = AutoEvaluate;
			}
		}
		
		private bool FIgnore;
		public bool Ignore {
			get {
				return FIgnore;
			}
			set {
				FIgnore = value;
				
				if (!FInUpdate)
					FNodeInfo.Ignore = Ignore;
			}
		}
		
		private string FName;
		public string Name {
			get {
				return FName;
			}
			private set {
				if (string.IsNullOrEmpty(value))
					FName = string.Empty;
				else
					FName = value;
			}
		}
		
		private string FCategory;
		public string Category {
			get {
				return FCategory;
			}
			private set {
				if (string.IsNullOrEmpty(value))
					FCategory = string.Empty;
				else
					FCategory = value;
			}
		}
		
		private string FVersion;
		public string Version {
			get {
				return FVersion;
			}
			private set {
				if (string.IsNullOrEmpty(value))
					FVersion = string.Empty;
				else
					FVersion = value;
			}
		}
		
		private string FShortcut;
		public string Shortcut {
			get {
				return FShortcut;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FShortcut = string.Empty;
				else
					FShortcut = value;
				
				if (!FInUpdate)
					FNodeInfo.Shortcut = Shortcut;
			}
		}
		
		private string FHelp;
		public string Help {
			get {
				return FHelp;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FHelp = string.Empty;
				else
					FHelp = value;
				
				if (!FInUpdate)
					FNodeInfo.Help = Help;
			}
		}
		
		private string FTags;
		public string Tags {
			get {
				return FTags;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FTags = string.Empty;
				else
					FTags = value;
				
				if (!FInUpdate)
					FNodeInfo.Tags = Tags;
			}
		}
		
		private string FAuthor;
		public string Author {
			get {
				return FAuthor;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FAuthor = string.Empty;
				else
					FAuthor = value;
				
				if (!FInUpdate)
					FNodeInfo.Author = Author;
			}
		}
		
		private string FCredits;
		public string Credits {
			get {
				return FCredits;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FCredits = string.Empty;
				else
					FCredits = value;
				
				if (!FInUpdate)
					FNodeInfo.Credits = Credits;
			}
		}
		
		private string FBugs;
		public string Bugs {
			get {
				return FBugs;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FBugs = string.Empty;
				else
					FBugs = value;
				
				if (!FInUpdate)
					FNodeInfo.Bugs = Bugs;
			}
		}
		
		private string FWarnings;
		public string Warnings {
			get {
				return FWarnings;
			}
			set {
				if (string.IsNullOrEmpty(value))
					FWarnings = string.Empty;
				else
					FWarnings = value;
				
				if (!FInUpdate)
					FNodeInfo.Warnings = Warnings;
			}
		}
		
		private System.Drawing.Size FInitialWindowSize;
		public System.Drawing.Size InitialWindowSize {
			get {
				return FInitialWindowSize;
			}
			set {
				FInitialWindowSize = value;
				
				if (!FInUpdate)
					FNodeInfo.InitialWindowSize = InitialWindowSize;
			}
		}
		
		private System.Drawing.Size FInitialBoxSize;
		public System.Drawing.Size InitialBoxSize {
			get {
				return FInitialBoxSize;
			}
			set {
				FInitialBoxSize = value;
				
				if (!FInUpdate)
					FNodeInfo.InitialBoxSize = InitialBoxSize;
			}
		}
		
		private TComponentMode FInitialComponentMode;
		public TComponentMode InitialComponentMode {
			get {
				return FInitialComponentMode;
			}
			set {
				FInitialComponentMode = value;
				
				if (!FInUpdate)
					FNodeInfo.InitialComponentMode = InitialComponentMode;
			}
		}
		
		public void BeginUpdate()
		{
			FInUpdate = true;
		}
		
		public void CommitUpdate()
		{
			try
			{
				if (FInUpdate)
				{
					FNodeInfo.BeginUpdate();
					UpdateInternal();
					FNodeInfo.CommitUpdate();
				}
			}
			finally
			{
				FInUpdate = false;
			}
		}
		
		public void Dispose()
		{
			FNodeInfo = null;
		}
	}
	
	#endregion
		
	
	class ProxyNodeInfoFactory : INodeInfoFactory, INodeInfoListener, IDisposable
	{
		private IInternalNodeInfoFactory FFactory;
		private Dictionary<INodeInfo, ProxyNodeInfo> FInternalToProxyMap;
		private Dictionary<INodeInfo, INodeInfo> FProxyToInternalMap;
		
		public ProxyNodeInfoFactory(IInternalNodeInfoFactory nodeInfoFactory)
		{
			FFactory = nodeInfoFactory;
			FInternalToProxyMap = new Dictionary<INodeInfo, ProxyNodeInfo>();
			FProxyToInternalMap = new Dictionary<INodeInfo, INodeInfo>();
			
			foreach (var nodeInfo in nodeInfoFactory.NodeInfos)
				NodeInfoAddedCB(nodeInfo);
			
			nodeInfoFactory.AddListener(this);
		}
		
		public void Dispose()
		{
			FFactory.RemoveListener(this);
		}
		
		public INodeInfo[] NodeInfos
		{
			get
			{
				return FInternalToProxyMap.Values.ToArray();
			}
		}
		
		public INodeInfo ToProxy(INodeInfo nodeInfo)
		{
			return FInternalToProxyMap[nodeInfo];
		}
		
		public INodeInfo ToInternal(INodeInfo nodeInfo)
		{
			return FProxyToInternalMap[nodeInfo];
		}
				
		public INodeInfo CreateNodeInfo(string name, string category, string version, string filename)
		{
			var nodeInfo = FFactory.CreateNodeInfo(name, category, version, filename);
			return FInternalToProxyMap[nodeInfo];
		}
		
		public void DestroyNodeInfo(INodeInfo nodeInfo)
		{
			nodeInfo = ToInternal(nodeInfo);
			FFactory.DestroyNodeInfo(nodeInfo);
		}
		
		public void NodeInfoAddedCB(INodeInfo nodeInfo)
		{
			var proxyNodeInfo = new ProxyNodeInfo(nodeInfo);
			FInternalToProxyMap[nodeInfo] = proxyNodeInfo;
			FProxyToInternalMap[proxyNodeInfo] = nodeInfo;
			OnNodeInfoAdded(proxyNodeInfo);
		}
		
		public void NodeInfoUpdatedCB(INodeInfo nodeInfo)
		{
			var proxyNodeInfo = FInternalToProxyMap[nodeInfo];
			proxyNodeInfo.Reload();
			OnNodeInfoUpdated(proxyNodeInfo);
		}
		
		public void NodeInfoRemovedCB(INodeInfo nodeInfo)
		{
			var proxyNodeInfo = FInternalToProxyMap[nodeInfo];
			FInternalToProxyMap.Remove(nodeInfo);
			FProxyToInternalMap.Remove(proxyNodeInfo);
			OnNodeInfoRemoved(proxyNodeInfo);
		}
		
		public event NodeInfoEventHandler NodeInfoAdded;
		
		protected virtual void OnNodeInfoAdded(INodeInfo nodeInfo)
		{
			if (NodeInfoAdded != null) {
				NodeInfoAdded(this, nodeInfo);
			}
		}
		
		public event NodeInfoEventHandler NodeInfoUpdated;
		
		protected virtual void OnNodeInfoUpdated(INodeInfo nodeInfo)
		{
			if (NodeInfoUpdated != null) {
				NodeInfoUpdated(this, nodeInfo);
			}
		}
		
		public event NodeInfoEventHandler NodeInfoRemoved;
		
		protected virtual void OnNodeInfoRemoved(INodeInfo nodeInfo)
		{
			if (NodeInfoRemoved != null) {
				NodeInfoRemoved(this, nodeInfo);
			}
		}
	}
}

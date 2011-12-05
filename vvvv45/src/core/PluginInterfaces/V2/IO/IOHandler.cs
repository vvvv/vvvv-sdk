using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIOHandler
	{
		object RawIOObject
		{
			get;
		}
		
		object Metadata
		{
			get;
		}
	}
	
	[ComVisible(false)]
	public abstract class IOHandler : IIOHandler
	{
		public static IOHandler<TIOObject> Create<TIOObject>(
			TIOObject iOObject,
			object metadata,
			Action<TIOObject> preEvalAction = null,
			Action<TIOObject> postEvalAction = null,
			Action<TIOObject> configAction = null
		)
		{
			return new IOHandler<TIOObject>(iOObject, metadata, preEvalAction, postEvalAction, configAction);
		}
		
		public object RawIOObject
		{
			get;
			private set;
		}
		public object Metadata
		{
			get;
			private set;
		}
		public readonly bool NeedsPreEvaluation;
		public readonly bool NeedsPostEvaluation;
		public readonly bool NeedsConfiguration;
		
		protected IOHandler(object rawIOObject, object metadata, bool pre, bool post, bool config = false)
		{
			RawIOObject = rawIOObject;
			Metadata = metadata;
			NeedsPreEvaluation = pre;
			NeedsPostEvaluation = post;
			NeedsConfiguration = config;
		}
		
		public abstract void PreEvaluate();
		public abstract void PostEvaluate();
		public abstract void Configurate();
	}
	
	public interface IIOHandler<out T> : IIOHandler
	{
		T IOObject
		{
			get;
		}
	}
	
	[ComVisible(false)]
	public class IOHandler<TIOObject> : IOHandler, IIOHandler<TIOObject>
	{
		private readonly Action<TIOObject> PreEvalAction;
		private readonly Action<TIOObject> PostEvalAction;
		private readonly Action<TIOObject> ConfigurateAction;
		
		public IOHandler(
			TIOObject iOObject,
			object metadata,
			Action<TIOObject> preEvalAction,
			Action<TIOObject> postEvalAction,
			Action<TIOObject> configAction)
			: base(iOObject, metadata, preEvalAction != null, postEvalAction != null, configAction != null)
		{
			IOObject = iOObject;
			PreEvalAction = preEvalAction;
			PostEvalAction = postEvalAction;
			ConfigurateAction = configAction;
		}
		
		public TIOObject IOObject
		{
			get;
			private set;
		}
		
		public override void PreEvaluate()
		{
			PreEvalAction(IOObject);
		}
		
		public override void PostEvaluate()
		{
			PostEvalAction(IOObject);
		}
		
		public override void Configurate()
		{
			ConfigurateAction(IOObject);
		}
	}
}

using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using VL.Core;
using VL.Lang;
using VL.Lang.Platforms.CIL;
using VL.Lang.Symbols;
using VL.Model;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.VL.Hosting
{
    using global::VL.Lang.Platforms.CIL.Runtime;
    using global::VL.Lib.Animation;
    using System.Collections.Immutable;
    using EvalAction = Action<int, MemoryIOStream<object>, IInStream[], IOutStream[]>;

    public class NodePlugin : IPlugin, IDisposable, IIOFactory, IRuntimeInstance
    {
        public class BuildResult
        {
            string FSymbolError;
            DocSymbols FScope;

            public BuildResult(NodePlugin plugin, CilCompilation compilation, IOperationDefinitionSymbol operation, IMethodDefinition methodToCall, 
                MethodInfo clrMethodToCall, EvalAction evaluateAction, string error)
            {
                Plugin = plugin;
                Compilation = compilation;
                Operation = operation;
                MethodToCall = methodToCall;
                ClrMethodToCall = clrMethodToCall;
                EvaluateAction = evaluateAction;
                Error = error;
            }

            public NodePlugin Plugin { get; }
            public CilCompilation Compilation { get; }
            public DocSymbols Scope => InterlockedHelper.CacheNoLock(ref FScope, () => Compilation.GetCurrent(Operation.DocSymbols));
            public IVLFactory Factory => Compilation.Platform.UserFactory;
            public IOperationDefinitionSymbol Operation { get; }
            public IMethodDefinition MethodToCall { get; }
            public MethodInfo ClrMethodToCall { get; }
            public EvalAction EvaluateAction { get; }
            public string Error { get; }
            public string SymbolError => InterlockedHelper.CacheNoLock(ref FSymbolError, () =>
            {
                var message = Operation.Messages.FirstOrDefault(m => m.Severity == MessageSeverity.Error);
                if (message != null)
                    return message.What;
                return string.Empty;
            });

            public Type InstanceType
            {
                get
                {
                    var methodToCall = ClrMethodToCall;
                    if (methodToCall != null && !methodToCall.IsStatic)
                        return methodToCall.DeclaringType;
                    return null;
                }
            }

            public BuildResult WithError(string error) => error != Error ? new BuildResult(Plugin, Compilation, Operation, MethodToCall, ClrMethodToCall, EvaluateAction, error) : this;
            public BuildResult WithCompilation(CilCompilation compilation) => compilation != Compilation ? new BuildResult(Plugin, compilation, Operation, MethodToCall, ClrMethodToCall, EvaluateAction, Error) : this;
        }

        private readonly Host FVlHost;
        private readonly RuntimeHost FRuntimeHost;
        private readonly IPluginHost2 FPluginHost;
        private readonly IIORegistry FIORegistry;
        private readonly ElementId FInstanceId;

        private readonly MemoryIOStream<object> FInstances = new MemoryIOStream<object>();
        private readonly List<IIOContainer<IInStream>> FInputs = new List<IIOContainer<IInStream>>();
        private readonly List<IIOContainer<IOutStream>> FOutputs = new List<IIOContainer<IOutStream>>();
        private readonly Messages FMessages = new Messages();

        //private Action<int, MemoryIOStream<object>, IInStream[], IOutStream[]> FEvaluateAction;
        private IInStream[] FInStreams;
        private IOutStream[] FOutStreams;
        private BuildResult FBuildResult;

        public NodePlugin(Host vlHost, RuntimeHost runtimeHost, NodeId nodeId, IPluginHost2 pluginHost, IIORegistry ioRegistry)
        {
            FInstanceId = Element.ProduceNewIdentity();
            FVlHost = vlHost;
            FRuntimeHost = runtimeHost;
            NodeId = nodeId;
            FPluginHost = pluginHost;
            FIORegistry = ioRegistry;
            FBuildResult = new BuildResult(this, null, null, null, null, null, null);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Disposing?.Invoke(this, EventArgs.Empty);
                foreach (var instance in FInstances)
                    Dispose(instance);
                FInstances.Length = 0;
            }
        }

        public NodeId NodeId { get; }
        public BuildResult LastBuildResult => FBuildResult;
        public CilCompilation Compilation => FBuildResult.Compilation ?? FRuntimeHost.Compilation;
        public bool IsDisposed { get; private set; }

        // Gets called on background thread. Does not write to any fields. All results get returned on stack.
        public BuildResult Update(CilCompilation compilation)
        {
            if (compilation == FBuildResult.Compilation)
                return FBuildResult; // Same compilation, nothing to do
            if (compilation.Age < FBuildResult.Compilation?.Age)
                return FBuildResult; // Dismiss outdated compilations
            var definitionSymbol = compilation.EntryPoints.FirstOrDefault(e => e.Node.Identity == NodeId) as INodeDefinitionSymbol;
            if (definitionSymbol == null)
                return FBuildResult.WithError($"Couldn't find entry point with id {NodeId} in compilation.");
            var closedDefinitionSymbol = definitionSymbol.GetClosedOrDefault(definitionSymbol.DocSymbols);
            if (closedDefinitionSymbol == null)
                return FBuildResult.WithError($"Couldn't close the generic node {definitionSymbol}.");
            // Look for the desired operation and build our evaluate method around it
            IMethodDefinition methodToCall;
            var processDefinitionSymbol = closedDefinitionSymbol as IProcessDefinitionSymbol;
            if (processDefinitionSymbol != null)
            {
                // TODO: Rewrite me to support process nodes fully (calling all its operations!)
                var updateOperation = processDefinitionSymbol.Operations.FirstOrDefault(p => p.Name == NameAndVersion.Update);
                if (updateOperation == null)
                    return FBuildResult.WithError($"Couldn't find 'Update' operation in {processDefinitionSymbol}.");
                methodToCall = compilation.GetCciMethod(updateOperation);
            }
            else
                methodToCall = compilation.GetCciMethod((IOperationDefinitionSymbol)closedDefinitionSymbol);

            if (methodToCall == null)
                return FBuildResult.WithError($"Couldn't find suitable method to call in evaluate for node {closedDefinitionSymbol}.");
            if (methodToCall.IsGeneric)
                return FBuildResult.WithError($"The method {methodToCall} must not be generic. Use the closed flag or annotate in- and output pins to avoid generic nodes.");
            var containingType = methodToCall.ContainingTypeDefinition;
            if (containingType.IsGeneric)
                return FBuildResult.WithError($"The containing type {containingType} must not be generic. Use the closed flag or annotate in- and output pins to avoid generic nodes.");

            // Only if the method changed our pins need to be synced and our state restored
            if (methodToCall != FBuildResult.MethodToCall)
            {
                var scope = closedDefinitionSymbol.DocSymbols;
                var operation = compilation.GetVlOperation(methodToCall, scope);
                var stateOutput = operation.GetStateOutput();
                var returnValueIsState = !methodToCall.IsStatic;
                var builder = new EvaluateMethodBuilder(FVlHost, compilation, scope, returnValueIsState);
                var evaluateAction = builder.Build(methodToCall);
                return new BuildResult(
                    plugin: this,
                    compilation: compilation,
                    operation: operation,
                    methodToCall: methodToCall,
                    clrMethodToCall: (MethodInfo)compilation.GetClrMethod(methodToCall),
                    evaluateAction: evaluateAction,
                    error: null);
            }

            // Remember the new compilation so we can exit early in subsequent calls
            return FBuildResult.WithCompilation(compilation);
        }

        // Called sequentially for all VL plugins on main thread
        public void SyncPinsAndRestoreState(BuildResult buildResult, HotSwapper swapper)
        {
            if (buildResult == FBuildResult)
                return; // Same build result, nothing to do
            if (buildResult == null)
                return; // Something went seriously wrong during compile phase
            if (buildResult.ClrMethodToCall == null)
            {
                FBuildResult = buildResult;
                return; // Something went wrong during compile phase
            }
            if (buildResult.Compilation.Age < FBuildResult.Compilation?.Age)
                return; // Dismiss outdated builds
            if (buildResult.ClrMethodToCall == FBuildResult.ClrMethodToCall)
            {
                // Same CLR method, nothing to do except for remember the new result
                // Holding on to the old would introduce a memory leak as old solutions and compilation could not get garbage collected
                FBuildResult = buildResult; 
                return; 
            }
            try
            {
                // Synchronize pins
                SyncPins(buildResult);

                // Restore the state
                if (FInstances.Length > 0)
                {
                    var instanceType = buildResult.InstanceType;
                    using (var reader = FInstances.GetReader())
                    using (var writer = FInstances.GetWriter())
                    {
                        while (!reader.Eos)
                        {
                            var oldInstance = reader.Read();
                            if (oldInstance != null)
                            {
                                var newInstance = swapper.Swap(oldInstance, buildResult.Scope, buildResult.Factory);
                                writer.Write(newInstance);
                            }
                            else
                            {
                                var newInstance = instanceType != null ? buildResult.Factory.CreateInstance(instanceType, FInstanceId) : null;
                                writer.Write(newInstance);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
                throw;
            }
            finally
            {
                FBuildResult = buildResult;
            }
        }

        void SyncPins(BuildResult buildResult)
        {
            var updateOperation = buildResult.Operation;
            var compilation = buildResult.Compilation;
            var scope = buildResult.Scope;
            FPluginHost.Plugin = null;
            foreach (var input in FInputs)
                input.Dispose();
            foreach (var output in FOutputs)
                output.Dispose();
            FInputs.Clear();
            FOutputs.Clear();
            if (updateOperation != null)
            {
                foreach (var input in updateOperation.GetRegularInputs())
                {
                    var clrType = compilation.GetClrType(input.Type, typeof(object));
                    var spreadType = typeof(IInStream<>).MakeGenericType(clrType);
                    var inputAttribute = new InputAttribute(input.Name);
                    if (input.DefaultValue.IsNotDummy())
                        SetDefaultValue(inputAttribute, input.DefaultValue.GetClrValue(buildResult.Compilation), scope);
                    else
                    {
                        var genericTypeDefinition = clrType.IsGenericType ? clrType.GetGenericTypeDefinition() : null;
                        var dataType = (genericTypeDefinition == typeof(IEnumerable<>) || genericTypeDefinition == typeof(global::VL.Lib.Collections.Spread<>))
                            ? clrType.GenericTypeArguments[0]
                            : clrType;
                        var defaultValue = compilation.DefaultClrValue(dataType, scope);
                        if (defaultValue != null)
                            SetDefaultValue(inputAttribute, defaultValue, scope);
                    }
                    var ioContainer = this.CreateIOContainer(spreadType, inputAttribute) as IIOContainer<IInStream>;
                    FInputs.Add(ioContainer);
                }
            }
            FInStreams = FInputs.Select(c => c.IOObject).ToArray();
            if (updateOperation != null)
            {
                foreach (var output in updateOperation.GetRegularOutputs())
                {
                    var clrType = compilation.GetClrType(output.Type, typeof(object));
                    var spreadType = typeof(IOutStream<>).MakeGenericType(clrType);
                    var inputAttribute = new OutputAttribute(output.Name);
                    var ioContainer = this.CreateIOContainer(spreadType, inputAttribute) as IIOContainer<IOutStream>;
                    FOutputs.Add(ioContainer);
                }
            }
            FOutStreams = FOutputs.Select(c => c.IOObject).ToArray();
            FPluginHost.Plugin = this;
        }

        static void SetDefaultValue(InputAttribute attribute, object value, DocSymbols scope)
        {
            var type = value.GetType();
            if (value is string)
                attribute.DefaultString = (string)value;
            else if (value is char)
                attribute.DefaultString = value.Convert<string>(scope);
            else if (type.IsPrimitive)
            {
                if (value is bool)
                    attribute.DefaultBoolean = (bool)value;
                else
                    attribute.DefaultValue = value.Convert<double>(scope);
            }
            else if (type.IsEnum)
                attribute.DefaultEnumEntry = value.ToString();
            else if (type.IsVector(scope))
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                attribute.DefaultValues = fields.Select(f => (double)Convert.ChangeType(f.GetValue(value), typeof(double))).ToArray();
            }
            else
                attribute.DefaultNodeValue = value;
        }

        void Dispose(object value)
        {
            var disposable = value as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        void LogException(Exception exception)
        {
            var errorMessage = exception.ToString();
            FPluginHost.Log(TLogType.Error, errorMessage);
            FPluginHost.LastRuntimeError = errorMessage;
            FMessages.Add(NodeId, MessageSeverity.Error, errorMessage);
        }

        public void Stop()
        {
            var exceptions = default(List<Exception>);
            foreach (var instance in FInstances)
            {
                try
                {
                    Dispose(instance);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>(1);
                    exceptions.Add(e);
                }
            }
            FInstances.Length = 0;
            if (exceptions != null)
            {
                var aggregateException = new AggregateException(exceptions);
                FPluginHost.Log(TLogType.Error, string.Format("During cleanup of the VL instances the following exceptions occured: {0}", aggregateException.ToString()));
            }
        }

        public void SetPluginHost(PluginInterfaces.V1.IPluginHost host)
        {
            Created?.Invoke(this, EventArgs.Empty);
        }

        public void Configurate(IPluginConfig input)
        {
            Configuring?.Invoke(this, new ConfigEventArgs(input));
        }

        internal void ClearRuntimeMessages()
        {
            FMessages.Clear();
        }

        public void Evaluate(int spreadMax)
        {
            Synchronizing?.Invoke(this, EventArgs.Empty);

            var buildResult = FBuildResult;
            if (buildResult.Error != null)
            {
                // This way the vvvv plugin node stays red
                PluginHost.LastRuntimeError = buildResult.Error;
                return;
            }
            else if (!string.IsNullOrEmpty(buildResult.SymbolError))
            {
                PluginHost.LastRuntimeError = buildResult.SymbolError;
            }

            var runtimeHost = FRuntimeHost;
            if (runtimeHost.Mode == RunMode.Paused || runtimeHost.Mode == RunMode.Stopped)
            {
                if (FMessages.Errors.Any())
                    PluginHost.LastRuntimeError = FMessages.Errors.FirstOrDefault().What;
                return;
            }

            var x = Clocks.FrameClock.Time; // hack. make sure that somebody is interested in time. otherwise HDEFrameClock observable will not trigger.
            try
            {
                RuntimeGraph.HandleException(NodeId);

                spreadMax = StreamUtils.GetSpreadMax(FInStreams);

                // Prepare internal state
                if (FInstances.Length != spreadMax)
                {
                    var instanceType = buildResult.InstanceType;
                    if (instanceType != null)
                        FInstances.Resize(spreadMax, () => buildResult.Factory.CreateInstance(instanceType, FInstanceId), value => Dispose(value));
                    else
                        FInstances.Resize(spreadMax, () => null, value => Dispose(value));
                }

                buildResult.EvaluateAction(spreadMax, FInstances, FInStreams, FOutStreams);
            }
            catch (RuntimeException exception)
            {
                // Collect the exception messages
                foreach (var p in exception.ExtractElementMessages(runtimeHost.Compilation.Compilation))
                    FMessages.Add(p.Key.TracingId, MessageSeverity.Error, p.Value);
                // Let others know about it
                runtimeHost.RaiseOnException(exception);
                // And finally tell vvvv about it
                throw exception.Original;
            }
            finally
            {
                Flushing?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool AutoEvaluate
        {
            get { return FBuildResult.InstanceType != null; }
        }

        #region IIOFactory

        public IPluginHost2 PluginHost
        {
            get { return FPluginHost; }
        }

        public IIOContainer CreateIOContainer(IOBuildContext context)
        {
            var io = FIORegistry.CreateIOContainer(this, context);
            if (io == null)
            {
                throw new NotSupportedException(string.Format("Can't create container for build context '{1}'.", context));
            }
            return io;
        }

        public bool CanCreateIOContainer(IOBuildContext context)
        {
            if (!FIORegistry.CanCreate(context))
            {
                var type = context.IOType;
                if (type.IsGenericType)
                {
                    var openGenericType = type.GetGenericTypeDefinition();
                    return FIORegistry.CanCreate(context.ReplaceIOType(openGenericType));
                }

                return false;
            }
            return true;
        }

        public event EventHandler Synchronizing;

        public event EventHandler Flushing;

        public event EventHandler<ConfigEventArgs> Configuring;

        // Not used
        public event EventHandler<ConnectionEventArgs> Connected;

        // Not used
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public event EventHandler Created;

        public event EventHandler Disposing;

        #endregion

        #region IRuntimeInstance

        public object Value
        {
            get
            {
                if (FBuildResult.InstanceType != null)
                    return FInstances.FirstOrDefault();
                return null;
            }
        }

        public IEnumerable<object> Results
        {
            // TODO
            get { return Enumerable.Empty<object>(); }
        }

        public ImmutableArray<Message> RuntimeMessages => FMessages.All.ToImmutableArray();

        public IVLObject Object => Value as IVLObject;

        #endregion

        public override string ToString() => $"{FBuildResult.MethodToCall}";
    }
}

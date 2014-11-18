using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VL.Lang.Model;
using VL.Lang.Symbols;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.VL.Factories
{
    public class Node : IPlugin, IDisposable, IIOFactory
    {
        private readonly VLType FVlType;
        private readonly IRuntimeHost FRuntimeHost;
        private readonly IPluginHost2 FPluginHost;
        private readonly IIORegistry FIORegistry;

        private readonly Spread<IValue> FInstances = new Spread<IValue>();
        private readonly List<IIOContainer<ISpread>> FInputs = new List<IIOContainer<ISpread>>();
        private readonly List<IIOContainer<ISpread>> FOutputs = new List<IIOContainer<ISpread>>();

        private ConcreteType FType;

        public Node(VLType vlType, IRuntimeHost runtimeHost, IPluginHost2 pluginHost, IIORegistry ioRegistry)
        {
            FVlType = vlType;
            FRuntimeHost = runtimeHost;
            FPluginHost = pluginHost;
            FIORegistry = ioRegistry;
            FRuntimeHost.Updated += HandleRuntimeHostUpdated;
        }

        public void Dispose()
        {
            FRuntimeHost.Updated -= HandleRuntimeHostUpdated;
            if (Disposing != null)
                Disposing(this, EventArgs.Empty);
            foreach (var instance in FInstances)
                Dispose(instance);
            FInstances.SliceCount = 0;
        }

        void HandleRuntimeHostUpdated(object sender, RuntimeUpdatedEventArgs e)
        {
            FType = FRuntimeHost.GetType(FVlType, e.NewCompilation);
            SyncPins(FType);
            for (int i = 0; i < FInstances.SliceCount; i++)
            {
                var oldInstance = FInstances[i];
                var newInstance = FRuntimeHost.Restore(oldInstance, FType, e.OldCompilation, e.NewCompilation);
                FInstances[i] = newInstance;
            }
        }

        void SyncPins(ConcreteType type)
        {
            var updateOperation = type.OperationDefinitions.FirstOrDefault(o => o.Name == VLLayer.UPDATE);
            if (updateOperation != null)
            {
                foreach (var input in FInputs)
                    input.Dispose();
                foreach (var output in FOutputs)
                    output.Dispose();
                FInputs.Clear();
                FOutputs.Clear();
                foreach (var input in updateOperation.RegularInputs)
                {
                    var clrType = FRuntimeHost.GetMarshalingType(input.Type);
                    var spreadType = typeof(ISpread<>).MakeGenericType(clrType);
                    var inputAttribute = new InputAttribute(input.Name);
                    var ioContainer = this.CreateIOContainer(spreadType, inputAttribute) as IIOContainer<ISpread>;
                    FInputs.Add(ioContainer);
                }
                foreach (var output in updateOperation.RegularOutputs)
                {
                    var clrType = FRuntimeHost.GetMarshalingType(output.Type);
                    var spreadType = typeof(ISpread<>).MakeGenericType(clrType);
                    var inputAttribute = new OutputAttribute(output.Name);
                    var ioContainer = this.CreateIOContainer(spreadType, inputAttribute) as IIOContainer<ISpread>;
                    FOutputs.Add(ioContainer);
                }
            }
        }

        void Dispose(IValue value)
        {
            var destructor = value.Type.Destructor;
            if (destructor != null)
                FRuntimeHost.Eval(value, destructor, Enumerable.Empty<IValue>());
        }

        public void SetPluginHost(IPluginHost host)
        {
            if (Created != null)
                Created(this, EventArgs.Empty);
        }

        public void Configurate(IPluginConfig input)
        {
            if (Configuring != null)
                Configuring(this, new ConfigEventArgs(input));
        }

        public void Evaluate(int spreadMax)
        {
            if (FType == null)
                return;

            if (Synchronizing != null)
                Synchronizing(this, EventArgs.Empty);

            // Prepare inernal state
            FInstances.Resize(
                spreadMax,
                () => FRuntimeHost.CreateValue(FType),
                value => Dispose(value));

            // Prepare output pins
            foreach (var output in FOutputs)
            {
                output.IOObject.SliceCount = spreadMax;
            }

            var updateOperation = FType.OperationDefinitions.FirstOrDefault(o => o.Name == VLLayer.UPDATE);
            for (int i = 0; i < spreadMax; i++)
            {
                var instance = FRuntimeHost.Marshal(FInstances[i]);
                var arguments = FInputs.Select(container => container.IOObject[i]);
                var outputValues = FRuntimeHost.Eval(instance, updateOperation, arguments);
                var j = 0;
                foreach (var outputValue in outputValues)
                {
                    if (j == 0)
                        FInstances[i] = FRuntimeHost.Marshal(outputValue);
                    else
                    {
                        var outputSpread = FOutputs[j - 1].IOObject;
                        outputSpread[i] = outputValue;
                    }
                    j++;
                }
            }

            if (Flushing != null)
                Flushing(this, EventArgs.Empty);
        }

        public bool AutoEvaluate
        {
            get { return true; }
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
    }
}

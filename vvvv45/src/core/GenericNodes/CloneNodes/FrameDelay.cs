using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

namespace VVVV.Nodes.Generic
{
    public abstract class FrameDelayNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable, IPluginFeedbackLoop, IPluginAwareOfEvaluation
    {
        [Config("Count", DefaultValue = 1, MinValue = 1, IsSingle = true)]
        public IDiffSpread<int> CountIn;

        public Spread<IIOContainer<ISpread<T>>> InputContainers = new Spread<IIOContainer<ISpread<T>>>();
        public Spread<IIOContainer<ISpread<T>>> DefaultContainers = new Spread<IIOContainer<ISpread<T>>>();
        public Spread<IIOContainer<ISpread<T>>> OutputContainers = new Spread<IIOContainer<ISpread<T>>>();

        [Input("Initialize", IsSingle = true, IsBang = true, Order = int.MaxValue)]
        public ISpread<bool> InitializeIn;

        [Import]
        protected IIOFactory FIOFactory;

        [Import]
        protected IMainLoop FMainLoop;

        readonly Copier<T> FCopier;

        public FrameDelayNode(Copier<T> copier)
        {
            FCopier = copier;
        }
        private readonly Spread<ISpread<T>> FBuffers = new Spread<ISpread<T>>();

        public void OnImportsSatisfied()
        {
            var inputAttribute = new InputAttribute("Input") { };
            CountIn.Changed += HandlePinCountChanged;
            FMainLoop.OnPrepareGraph += HandleOnPrepareGraph;
        }

        public void Dispose()
        {
            CountIn.Changed -= HandlePinCountChanged;
            FMainLoop.OnPrepareGraph -= HandleOnPrepareGraph;
        }

        public bool OutputRequiresInputEvaluation(IPluginIO inputPin, IPluginIO outputPin)
        {
            // Feedback loops are only allowed for our regular input pins (not the default pins).
            return !(InputContainers.Any(c => c.GetPluginIO() == inputPin));
        }

        private void HandlePinCountChanged(IDiffSpread<int> sender)
        {
            var count = Math.Max(1, CountIn[0]);
            ResizePinGroups(count, InputContainers, (i) => new InputAttribute(string.Format("Input {0}", i)) { AutoValidate = false });
            ResizePinGroups(count, DefaultContainers, (i) => new InputAttribute(string.Format("Default {0}", i)) { AutoValidate = false });
            ResizePinGroups(count, OutputContainers, (i) => new OutputAttribute(string.Format("Output {0}", i)));
            FBuffers.Resize(
                count,
                i => new Spread<T>(1),
                DisposeSpread
            );
        }

        private static void DisposeSpread(ISpread<T> spread)
        {
            foreach (var slice in spread)
            {
                var disposableSlice = slice as IDisposable;
                if (disposableSlice != null)
                    disposableSlice.Dispose();
            }
        }

        private void ResizePinGroups<TSpread>(int count, Spread<IIOContainer<TSpread>> pinSpread, Func<int, IOAttribute> ioAttributeFactory)
            where TSpread : class
        {
            pinSpread.ResizeAndDispose(
                count,
                (i) =>
                {
                    var ioAttribute = ioAttributeFactory(i + 1);
                    return FIOFactory.CreateIOContainer<TSpread>(ioAttribute);
                }
            );
        }

        bool FFirstFrame = true;
        public void Evaluate(int spreadMax)
        {
            var init = InitializeIn.SliceCount > 0 && InitializeIn[0];
            if (FFirstFrame || init)
            {
                FFirstFrame = false;
                for (int i = 0; i < FBuffers.SliceCount; i++)
                {
                    var defaultSpread = DefaultContainers[i].IOObject;
                    // Validate the default input
                    defaultSpread.Sync();
                    // And write it to the output
                    var outputSpread = OutputContainers[i].IOObject;
                    outputSpread.AssignFrom(FCopier.CopySpread(defaultSpread));
                }
            }
            else
            {
                for (int i = 0; i < FBuffers.SliceCount; i++)
                {
                    var buffer = FBuffers[i];
                    // Write the cached result from the last frame
                    var outputSpread = OutputContainers[i].IOObject;
                    outputSpread.AssignFrom(buffer);
                }
            }
        }

        // We registered at mainloop. So this is called even when node is disabled 
        void HandleOnPrepareGraph(object sender, EventArgs e)
        {
            // Might trigger our Evaluate method if no one asked for the data of our outputs yet
            FIOFactory.PluginHost.Evaluate();

            // Let's do not evaluate anything in case of being turned off.
            if (Stopped)
                return;
            
            for (int i = 0; i < FBuffers.SliceCount; i++)
            {
                var inputSpread = InputContainers[i].IOObject;
                // Validate the regular input
                inputSpread.Sync();
                // And cache the result for the next frame
                FBuffers[i] = FCopier.CopySpread(inputSpread);
            }
        }

        bool Stopped;

        /// <summary>
        /// Node will get evaluated this frame and the coming frames.
        /// </summary>
        public void TurnOn()
        {
            Stopped = false;
        }

        /// <summary>
        /// Node will not get evaluated this frame and the coming frames.
        /// </summary>
        public void TurnOff()
        {
            Stopped = true;
        }
    }
}

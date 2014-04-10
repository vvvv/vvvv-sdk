using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
    public abstract class FrameDelayNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Config("Count", DefaultValue = 1, MinValue = 0, IsSingle = true)]
        public IDiffSpread<int> CountIn;

        public Spread<IIOContainer<ISpread<T>>> InputContainers = new Spread<IIOContainer<ISpread<T>>>();
        public Spread<IIOContainer<ISpread<T>>> DefaultContainers = new Spread<IIOContainer<ISpread<T>>>();
        public Spread<IIOContainer<ISpread<T>>> OutputContainers = new Spread<IIOContainer<ISpread<T>>>();

        [Input("Initialize", IsSingle = true, Order = int.MaxValue)]
        public ISpread<bool> InitializeIn;

        [Import]
        protected IIOFactory FIOFactory;

        [Import]
        protected IMainLoop FMainLoop;

        private readonly Spread<ISpread<T>> FBuffers = new Spread<ISpread<T>>();

        public void OnImportsSatisfied()
        {
            var inputAttribute = new InputAttribute("Input") { };
            CountIn.Changed += HandlePinCountChanged;
            FMainLoop.OnResetCache += HandleOnResetCache;
        }

        public void Dispose()
        {
            CountIn.Changed -= HandlePinCountChanged;
            FMainLoop.OnResetCache -= HandleOnResetCache;
        }
		
		private void HandlePinCountChanged(IDiffSpread<int> sender)
		{
            var count = CountIn[0];
            ResizePinGroups(count, InputContainers, (i) => new InputAttribute(string.Format("Input {0}", i)));
            ResizePinGroups(count, DefaultContainers, (i) => new InputAttribute(string.Format("Default {0}", i)));
            ResizePinGroups(count, OutputContainers, (i) => new OutputAttribute(string.Format("Output {0}", i)) { AllowFeedback = true });
            FBuffers.Resize(
                count,
                i => new Spread<T>(1),
                DisposeSpread
            );
            WriteBufferedDataToOutputs();
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

        public void Evaluate(int SpreadMax)
        {
            var inputSpreads = InitializeIn.SliceCount > 0 && InitializeIn[0]
                ? DefaultContainers.Select(c => c.IOObject)
                : InputContainers.Select(c => c.IOObject);
            var i = 0;
            foreach (var inputSpread in inputSpreads)
            {
                FBuffers[i++] = CloneSpread(inputSpread);
            }
        }

        void HandleOnResetCache(object sender, EventArgs e)
        {
            WriteBufferedDataToOutputs();
        }

        private void WriteBufferedDataToOutputs()
        {
            var outputSpreads = OutputContainers.Select(c => c.IOObject);
            var i = 0;
            foreach (var outputSpread in outputSpreads)
            {
                outputSpread.AssignFrom(FBuffers[i++]);
            }
        }

        protected virtual ISpread<T> CloneSpread(ISpread<T> spread)
        {
            var clonedSpread = new Spread<T>(spread.SliceCount);
            var clonedBuffer = clonedSpread.Stream.Buffer;
            var buffer = spread.Stream.Buffer;
            for (int i = 0; i < buffer.Length; i++)
            {
                clonedBuffer[i] = CloneSlice(buffer[i]);
            }
            return clonedSpread;
        }

        protected abstract T CloneSlice(T slice);
    }

    [PluginInfo(Name = "FrameDelay", Category = "Raw", AutoEvaluate = true)]
    public class RawFrameDelayNode : FrameDelayNode<System.IO.Stream>
    {
        protected override System.IO.Stream CloneSlice(System.IO.Stream slice)
        {
            var clone = new System.IO.MemoryStream((int)slice.Length);
            slice.Position = 0;
            slice.CopyTo(clone);
            return clone;
        }
    }
}

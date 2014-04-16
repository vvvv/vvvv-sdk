using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

namespace VVVV.Nodes
{
    public abstract class FrameDelayNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable, IPluginFeedbackLoop
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
                for (int i = 0; i < OutputContainers.SliceCount; i++)
                {
                    var defaultSpread = DefaultContainers[i].IOObject;
                    // Validate the default input
                    defaultSpread.Sync();
                    // And write it to the output
                    var outputSpread = OutputContainers[i].IOObject;
                    outputSpread.AssignFrom(CloneSpread(defaultSpread));
                }
            }
            else
            {
                // Do nothing here - we output the data from the last frame
            }
        }

        void HandleOnPrepareGraph(object sender, EventArgs e)
        {
            // Might trigger our Evaluate method if no one asked for the data of our outputs yet
            FIOFactory.PluginHost.Evaluate();

            for (int i = 0; i < OutputContainers.SliceCount; i++)
            {
                var inputSpread = InputContainers[i].IOObject;
                // Validate the regular input
                inputSpread.Sync();
                // And cache the result for the next frame
                var outputSpread = OutputContainers[i].IOObject;
                outputSpread.AssignFrom(CloneSpread(inputSpread));
            }
        }

        protected virtual ISpread<T> CloneSpread(ISpread<T> spread)
        {
            var clonedSpread = new Spread<T>(spread.SliceCount);
            var clonedBuffer = clonedSpread.Stream.Buffer;
            var buffer = spread.Stream.Buffer;
            for (int i = 0; i < spread.SliceCount; i++)
            {
                clonedBuffer[i] = CloneSlice(buffer[i]);
            }
            return clonedSpread;
        }

        protected abstract T CloneSlice(T slice);
    }


    [PluginInfo(Name = "FrameDelay", Category = "Color")]
    public class ColorFrameDelayNode : FrameDelayNode<RGBAColor>
    {
        protected override RGBAColor CloneSlice(RGBAColor slice)
        {
            return slice;
        }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Enumerations")]
    public class EnumerationsFrameDelayNode : FrameDelayNode<EnumEntry>
    {
        protected override EnumEntry CloneSlice(EnumEntry slice)
        {
            return slice;
        }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Raw")]
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

    [PluginInfo(Name = "FrameDelay", Category = "String")]
    public class StringFrameDelayNode : FrameDelayNode<string>
    {
        protected override string CloneSlice(string slice)
        {
            return slice;
        }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Value")]
    public class ValueFrameDelayNode : FrameDelayNode<double>
    {
        protected override double CloneSlice(double slice)
        {
            return slice;
        }
    }

}

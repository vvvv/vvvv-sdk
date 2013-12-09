#region usings

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils.VMath;
using VVVV.Utils.Streams;

#endregion usings

namespace VVVV.Nodes
{
    public class ClockWork
    {
        private readonly ISpread Spread;
        private int counter;
        private int length;
        private object[] buffer;

        public ClockWork(ISpread spread)
        {
            this.Spread = spread;
            Reset();
        }

        public void Reset()
        {
            counter = 0;
            length = Spread.SliceCount;
            buffer = ((ISpread<object>)Spread).Stream.Buffer;
        }
        
        public int Counter
        {
            get { return counter; }
        }

        public int Length
        {
            get { return length; }
        }

        public MemoryIOStream<T> Stream<T>()
        {
            return ((ISpread<T>)Spread).Stream; 
        }

        public object Current
        {
            get { return buffer[counter];  }
            set { buffer[counter] = value; }
        }

        public object[] Buffer()
        {
            return buffer;
        }

        public bool Tick()
        {
            if (++counter >= length)
            {
                Reset();
                return true;
            }
            return false;
        }
    }

	
	#region PluginInfo
    [PluginInfo(Name = "Map", Category = "Value", Version = "Advanced",
        Help = "Maps the value in the given range to a proportional value in the given output range", 
        Tags = "velcrome")
    ]
    #endregion PluginInfo
    public class AdvancedValueMapNode : IPluginEvaluate
    {
        #region fields & pins

        private const int INPUT = 0;
        private const int BIN = 1;
        private const int A = 2;
        private const int B = 3;
        private const int C = 4;
        private const int D = 5;
        private const int MAP = 6;

        [Input("Input", DefaultValue = 0.5, Order = INPUT)] public ISpread<double> FInput;
        [Input("Input Binsize", DefaultValue = 1, Order = BIN, Visibility = PinVisibility.Hidden, MinValue = 1)] 
        public ISpread<int> FBinsize;

        [Input("Source Minimum", DefaultValue = 0.0, Order = A)] public ISpread<double> FInStart;
        [Input("Source Maximum", DefaultValue = 1.0, Order = B)] public ISpread<double> FInEnd;
        [Input("Destination Minimum", DefaultValue = 0.0, Order = C)] public ISpread<double> FOutStart;
        [Input("Destination Maximum", DefaultValue = 1.0, Order = D)] public ISpread<double> FOutEnd;

        [Input("Mapping", DefaultEnumEntry = "Float", Order = MAP)] public IDiffSpread<TMapMode> FMapping;

        [Output("Output")] protected ISpread<double> FOutput;
        private readonly ClockWork[] clock;

        #endregion fields & pins

        public AdvancedValueMapNode()
        {
            clock = new[]
                        {
                            new ClockWork(FInput),
                            new ClockWork(FBinsize),

                            new ClockWork(FInStart),
                            new ClockWork(FInEnd),
                            new ClockWork(FOutStart),
                            new ClockWork(FOutEnd),
                            new ClockWork(FMapping)
                        };
        }


        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = SpreadMax;
            
            int binCounter = 0;

            for (int index = 0; index < SpreadMax; index++)
            {
                double ratio = VMath.Ratio((double) clock[INPUT].Current, (double) clock[A].Current,
                                           (double) clock[B].Current, (TMapMode) clock[MAP].Current);
                FOutput[index] = VMath.Lerp((double) clock[C].Current, (double) clock[D].Current, ratio);

                if (++binCounter >= (int) clock[BIN].Current)
                {
                    binCounter++;
                    clock[BIN].Tick();

                    clock[A].Tick();
                    clock[B].Tick();
                    clock[C].Tick();
                    clock[D].Tick();
                    clock[MAP].Tick();
                }
            }
        }
    }

    #region PluginInfo
	[PluginInfo(Name = "MapRange", Category = "Value", Version = "Advanced", 
        Help = "Maps the value in the given range to a proportional value in the given output range", 
        Tags = "velcrome")]
	#endregion PluginInfo
    public class AdvancedValueMapRangeNode : IPluginEvaluate
	{
        #region fields & pins

        private const int INPUT = 0;
        private const int BIN = 1;
        private const int A = 2;
        private const int B = 3;
        private const int C = 4;
        private const int D = 5;
        private const int MAP = 6;    

        [Input("Input", DefaultValue = 0.5, Order = INPUT)]
        public ISpread<double> FInput;

        [Input("Input Binsize", DefaultValue = 1, Order = BIN, Visibility = PinVisibility.Hidden, MinValue = 1)]
        public ISpread<int> FBinsize;
		
		[Input("Source Center", DefaultValue = 0.0, Order = A)]
		public ISpread<double> FInCenter;
		
		[Input("Source Width", DefaultValue = 1.0, Order = B)]
		public ISpread<double> FInWidth;
		
		[Input("Destination Center", DefaultValue = 0.0, Order = C)]
		public ISpread<double> FOutCenter;
		
		[Input("Destination Width", DefaultValue = 1.0, Order = D)]
		public ISpread<double> FOutWidth;

        [Input("Mapping", DefaultEnumEntry = "Float", Order = MAP)]
        public IDiffSpread<TMapMode> FMapping;

        [Output("Output")] 
        protected ISpread<double> FOutput;
		
  	    private readonly ClockWork[] clock;

		#endregion fields & pins

        public AdvancedValueMapRangeNode()
        {
            clock = new[]
            {
                new ClockWork(FInput), 
                new ClockWork(FBinsize), 

                new ClockWork(FInCenter), 
                new ClockWork(FInWidth), 
                new ClockWork(FOutCenter), 
                new ClockWork(FOutWidth), 
                new ClockWork(FMapping)
            };
        }

		
		public void Evaluate(int SpreadMax)
		{
			SpreadMax = FInput.SliceCount;
			FOutput.SliceCount = SpreadMax;
			
            int binCounter = 0;

            for ( int index = 0 ; index < SpreadMax; index++)
            {
                double halfWidth = (double) clock[B].Current/2;
                double ratio = VMath.Ratio((double)clock[INPUT].Current, (double)clock[A].Current - halfWidth, (double)clock[A].Current + halfWidth, (TMapMode)clock[MAP].Current);
                halfWidth = (double) clock[D].Current/2;
                FOutput[index] = VMath.Lerp((double)clock[C].Current - halfWidth, (double)clock[C].Current + halfWidth, ratio);
				
				if (++binCounter >= (int)clock[BIN].Current)
				{
				    binCounter++;
                    clock[BIN].Tick();

                    clock[A].Tick();
                    clock[B].Tick();
                    clock[C].Tick();
                    clock[D].Tick();
                    clock[MAP].Tick();
				}
			}
		}
	}
}
using System;
using System.Drawing;

namespace VVVV.Nodes.Timeliner
{
	
	public sealed class TLTime
	{
		public const double MinTimeStep = 0.0001;
		private double[] FCurrentTimes = new double[0];
		private bool[] FTimeChanged = new bool[0];
		private double FStartTime;
		private double FHostTime;
		private double FSpeed;
		private double FOldTime;
		
		private bool FIsRunning;
		private bool FForceCurrentTime = false;
		
		public TLAutomataPin Automata {get; set;}
		public TLTransformer Transformer {get; set;}
		
		public TLTime()
		{
			TimeCount = 1;
		}
		
		private int FTimeCount;
		public int TimeCount
		{
			get {return FTimeCount;}
			set
			{
				if (FCurrentTimes.Length != value)
				{
					FTimeCount = value;
					FCurrentTimes = new double[FTimeCount];
					FTimeChanged = new bool[FTimeCount];
					for (int i=0; i<FTimeCount; i++)
						FTimeChanged[i] = false;
				}
			}
		}
		
		public double GetTime(int Index)
		{
			return FCurrentTimes[Index % FCurrentTimes.Length];
		}
		
		public double GetTimeAsX(int Index)
		{
			return Transformer.TransformPoint(new PointF((float) GetTime(Index), 0)).X;
		}
		
		public void SetTime(int Index, double Time)
		{
			if (FCurrentTimes[Index] != Time)
			{
				FCurrentTimes[Index] = Time;
				FTimeChanged[Index] = true;
			}
			FForceCurrentTime = true;
		}
		
		public bool Changed(int Index)
		{
			return FTimeChanged[Index % FTimeChanged.Length];
		}
		
		public void InvalidateTimes()
		{
			for (int i=0; i<FTimeChanged.Length; i++)
				FTimeChanged[i] = false;
		}
		
		public double HostTime
		{
			get { return FHostTime;}
			set { FHostTime = value;}
		}
		
		public double Speed
		{
			get { return FSpeed;}
			set { FSpeed = value;}
		}
		
		public bool IsRunning
		{
			get { return FIsRunning;}
			set
			{
				FForceCurrentTime = true;
				FIsRunning = value;
			}
		}
		
		public bool IsSeeking
		{
			get{return FForceCurrentTime;}
		}
		
		public void Evaluate()
		{
			//execute user/gui-forced time
			if (FForceCurrentTime)
				FStartTime = FHostTime;
			
			if (FIsRunning)
			{
				double deltaTime = (FHostTime - FOldTime) * FSpeed;
				FCurrentTimes[0] += deltaTime;
			}
			
			FOldTime = FHostTime;

			if (Automata != null)
			{
				if (FForceCurrentTime)
					Automata.ForceStateFromCurrentTime(FCurrentTimes[0]);
				
				EvaluateAutomata();
				
			}

			FForceCurrentTime = false;
		}
		
		
		private void EvaluateAutomata()
		{
			Automata.Evaluate(FCurrentTimes[0]);

			//now check the automatas command to see what to do:
			switch (Automata.Command)
			{
				case TLAutomataCommand.NoChange:
					{
						//nothing to do
						if (!FIsRunning)
							FStartTime = FHostTime;
						break;
					}
				case TLAutomataCommand.Play:
					{
						IsRunning = true;
						break;
					}
				case TLAutomataCommand.Pause:
					{
						FCurrentTimes[0] = Automata.PauseTime;
						FForceCurrentTime = true;
						IsRunning = false;
						break;
					}
				case TLAutomataCommand.Jump:
					{
						FCurrentTimes[0] = Automata.TimeToJumpTo;
						FForceCurrentTime = true;
						break;
					}
			}
			
			//execute automata-forced time
			if (FForceCurrentTime)
			{
				FStartTime = FHostTime;
				FForceCurrentTime = false;
			}
		}
	}
}


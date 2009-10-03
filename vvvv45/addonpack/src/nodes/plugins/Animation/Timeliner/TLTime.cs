using System;

namespace VVVV.Nodes.Timeliner
{
	
	public sealed class TLTime
	{
		public const double MinTimeStep = 0.0001;
		private double[] FCurrentTimes = new double[0];
		private bool[] FTimeChanged = new bool[0];
		private double FStartTime;
		private double FHostTime;
		private double FOffset;
		
		private bool FIsRunning;
		private bool FForceCurrentTime = false;
		
		private TLAutomataPin FAutomata;
		public TLAutomataPin Automata
		{
			set{FAutomata = value;}
		}
		
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
			{
				FOffset = FCurrentTimes[0];
				FStartTime = FHostTime;
			}
			
			if (FIsRunning)
				FCurrentTimes[0] = FHostTime - FStartTime + FOffset;
			
			if (FAutomata != null)
			{
				if (FForceCurrentTime)
					FAutomata.ForceStateFromCurrentTime(FCurrentTimes[0]);
				
				EvaluateAutomata();
			}

			FForceCurrentTime = false;
		}
		
		
		private void EvaluateAutomata()
		{
			FAutomata.Evaluate(FCurrentTimes[0]);

			//now check the automatas command to see what to do:
			switch (FAutomata.Command)
			{
				case TLAutomataCommand.NoChange:
					{
						//nothing to do
						if (!FIsRunning)
						{
							FOffset = FCurrentTimes[0];
							FStartTime = FHostTime;
						}
						break;
					}
				case TLAutomataCommand.Play:
					{
						IsRunning = true;
						break;
					}
				case TLAutomataCommand.Pause:
					{
						FCurrentTimes[0] = FAutomata.PauseTime;
						FForceCurrentTime = true;
						IsRunning = false;
						break;
					}
				case TLAutomataCommand.Jump:
					{
						FCurrentTimes[0] = FAutomata.TimeToJumpTo;
						FForceCurrentTime = true;
						break;
					}
			}
			
			//execute automata-forced time
			if (FForceCurrentTime)
			{
				FOffset = FCurrentTimes[0];
				FStartTime = FHostTime;
				FForceCurrentTime = false;
			}
		}
	}
}


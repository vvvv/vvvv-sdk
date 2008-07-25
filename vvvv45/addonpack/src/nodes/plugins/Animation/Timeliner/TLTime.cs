using System;

namespace VVVV.Nodes.Timeliner
{
	public sealed class TLTime
	{
		private double FCurrentTime;
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
		{}
		
		
		public double CurrentTime
		{
   			get { return FCurrentTime;}
   			set 
   			{ 
   				FCurrentTime = value;
   				FForceCurrentTime = true;
   			}
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
		
		public void Evaluate()
		{
			//execute user/gui-forced time
			if (FForceCurrentTime)
			{
				FOffset = FCurrentTime;
				FStartTime = FHostTime;
			}
			
			if (FIsRunning)
				FCurrentTime = FHostTime - FStartTime + FOffset;
				
			if (FAutomata != null)
			{
				if (FForceCurrentTime)
					FAutomata.ForceStateFromCurrentTime(FCurrentTime);
				
				EvaluateAutomata();
			}			

			FForceCurrentTime = false;
		}
			
		
		private void EvaluateAutomata()
		{
			FAutomata.Evaluate(FCurrentTime);

			//now check the automatas command to see what to do:
			switch (FAutomata.Command)
			{
				case TLAutomataCommand.NoChange:
					{
						//nothing to do
						if (!FIsRunning)
						{
							FOffset = FCurrentTime;
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
						FCurrentTime = FAutomata.PauseTime;
						FForceCurrentTime = true;
						IsRunning = false;
						break;
					}
				case TLAutomataCommand.Jump:
					{
						FCurrentTime = FAutomata.TimeToJumpTo;
						FForceCurrentTime = true;
						break;
					}
			}
			
			//execute automata-forced time
			if (FForceCurrentTime)
			{
				FOffset = FCurrentTime;
				FStartTime = FHostTime;
				FForceCurrentTime = false;
			}
		}
	}	
}


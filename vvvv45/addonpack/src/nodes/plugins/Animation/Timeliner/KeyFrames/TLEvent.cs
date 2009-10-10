using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public enum TLActionCommand {next, previous, play, pause, jump, loop};
	
	public struct TLAction
	{
		public TLActionCommand Command;
		private string FGotoState;
		public string GotoState
		{
			get{return FGotoState;}
			set{FGotoState = value;}
		}
		
		public override string ToString()
		{
			if (Command == TLActionCommand.jump)
				return FGotoState;
			else
				return Command.ToString();
		}
		
		public string Icon
		{
			get
			{
				switch (Command)
				{
					case TLActionCommand.next: return ">>";
					case TLActionCommand.previous: return "<<";
					case TLActionCommand.loop: return "><";
					case TLActionCommand.pause: return " ||";
					case TLActionCommand.play: return " >";
					case TLActionCommand.jump: return "->";
					default: return "->";
				}
			}
		}
	}
	
	public class TLEvent
	{
		private IValueFastIn FEventPin;
		public IValueFastIn EventPin
		{
			get{return FEventPin;}
			set{FEventPin = value;}
		}
		
		private TLAction FAction;
		public TLAction Action
		{
			get{return FAction;}
		}
		
		public string GotoState
		{
			get {return FAction.GotoState;}
		}
		
		private string FName;
		public string Name
		{
			get {return FName;}
		}
	
		public TLEvent(string Event)
		{
			char[] s = {' '};
			string[] e = Event.Split(s);
			
			FName = e[0];
			
			try
			{
				FAction.Command = (TLActionCommand) Enum.Parse(typeof(TLActionCommand), e[1]);
			}
			catch (System.ArgumentException)
			{
				FAction.Command = TLActionCommand.jump;
				FAction.GotoState = e[1];
			}
		}
		
		public override string ToString()
		{
			return FName + " " + FAction.ToString() + ";";
		}
	}
}

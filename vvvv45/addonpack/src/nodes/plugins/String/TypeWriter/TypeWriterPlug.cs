#region usings
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using VVVV.Utils;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Core.Logging;
using VVVV.Utils.IO;
#endregion using

namespace TypeWriter
{
	[PluginInfo(Name = "Typewriter",
	            Category = "String",
	            Credits = "Based on an original version by bo27, Yuri Dolgov",
	            Author = "vvvv group",
	            Help = "Takes all keyboardinput including keyboard commands and returns a resulting string",
	            Tags = "keyboard")]
	public class TypeWriterPlugin: IPluginEvaluate
	{
		#region field declaration
		//input pin declaration
		[Input("Keyboard State", IsSingle = true)]
		IDiffSpread<KeyboardState> FKeyboardState;
		
		[Input("Text", DefaultString = "", IsSingle = true)]
		ISpread<string> FInputText;
		
		[Input("Insert Text", IsBang = true, IsSingle = true)]
		ISpread<bool> FInsertText;
		
		[Input("Initial Text", IsSingle = true)]
		ISpread<string> FInitialText;
		
		[Input("Initialize", IsSingle = true, IsBang = true)]
		ISpread<bool> FInitialize;
		
		[Input("Cursor Position", IsSingle = true, MinValue = 0, MaxValue = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
		ISpread<int> FNewCursorPosition;
		
		[Input("Set Cursor Position", IsSingle = true, IsBang = true, Visibility = PinVisibility.OnlyInspector)]
		ISpread<bool> FSetCursorPosition;
		
		[Input("Ignore Navigation Keys", IsSingle = true, DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
		ISpread<bool> FIgnoreNavigationKeys;
		
		//output pin declaration
		[Output("Output")]
		ISpread<string> FOutput;
		
		[Output("Cursor Position")]
		ISpread<int> FCursorPosition;

		[Import()]
		ILogger Flogger;
		
		[Import()]
		IHDEHost FHDEHost;
		
		private bool FControlKeyPressed;
		private string FLastCapitalKey;
		private string FText = "";
		private int FCursorCharPos = 0;
		//defined as string but must be only one character:
		private string FNewlineSymbol = Environment.NewLine;
		private Dictionary<uint, double> FBufferedCommands = new Dictionary<uint, double>();
		private Dictionary<string, double> FBufferedKeys = new Dictionary<string, double>();
		private KeyboardState FLastKeyboardState = KeyboardState.Empty;
		
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		#endregion field declaration

		#region Text navigation helper
		//todo:
		//text splitting is done here every call,
		//should be done globally
		int GetLineCount()
		{
			var lines = FText.Split(new string[] {FNewlineSymbol}, StringSplitOptions.None);
			return lines.Length;
		}
		
		int GetLineOfCursor()
		{
			var lineStart = 0;
			var lineEnd = 0;
			var lineIndex = 0;
			var lines = FText.Split(new string[] {FNewlineSymbol}, StringSplitOptions.None);
			foreach (var line in lines)
			{
				lineEnd += line.Length + FNewlineSymbol.Length;
				
				if (FCursorCharPos >= lineStart && FCursorCharPos < lineEnd)
					break;
				
				lineStart += line.Length + FNewlineSymbol.Length;
				lineIndex++;
			}
			
			return lineIndex;
		}
		
		int GetStartOfLine(int lineIndex)
		{
			var lineStart = 0;
			var lines = FText.Split(new string[] {FNewlineSymbol}, StringSplitOptions.None);
			
			for (int i = 0; i < lineIndex; i++)
				lineStart += lines[i].Length + FNewlineSymbol.Length;
			
			return lineStart;
		}
		
		int GetLengthOfLine(int lineIndex)
		{
			var lines = FText.Split(new string[] {FNewlineSymbol}, StringSplitOptions.None);
			return lines[lineIndex].Length;
		}
		#endregion Text navigation helper

		#region Cursor
		void CursorToTextStart()
		{
			FCursorCharPos = 0;
		}

		void CursorToTextEnd()
		{
			FCursorCharPos = FText.Length;
		}
		
		void CursorOneLineUp()
		{
			int line = GetLineOfCursor();
			if (line > 0)
			{
				var startOfLine = GetStartOfLine(line);
				var posInLine = FCursorCharPos - startOfLine;
				var newLine = line - 1;
				
				var newPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));
				
				FCursorCharPos = EnsureCorrectCursorPlacement(newPos);
			}
		}

		void CursorOneLineDown()
		{
			int line = GetLineOfCursor();
			if (line < GetLineCount() - 1)
			{
				var startOfLine = GetStartOfLine(line);
				var posInLine = FCursorCharPos - startOfLine;
				var newLine = line + 1;
				
				var newPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));
				
				FCursorCharPos = EnsureCorrectCursorPlacement(newPos);
			}
		}
		
		int EnsureCorrectCursorPlacement(int newPos)
		{
			try
			{
				if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
					newPos -= 1;
			}
			catch
			{
				//FText[newPos] may access out of range char..nevermind
			}
			
			return Math.Max(0, Math.Min(FText.Length, newPos));
		}

		void CursorToLineStart()
		{
			int line = GetLineOfCursor();
			FCursorCharPos = GetStartOfLine(line);
		}

		void CursorToLineEnd()
		{
			int line = GetLineOfCursor();
			FCursorCharPos = GetStartOfLine(line) + GetLengthOfLine(line);
		}

		void CursorStepsRight(int steps = 1)
		{
			var newPos = Math.Min(FText.Length, FCursorCharPos + steps);
			//if cursor lands on an \r and the next symbol is an \n make sure to step one more
			try
			{
				if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
					newPos += 1;
			}
			catch
			{
				//FText[newPos] may access out of range char..nevermind
			}
			
			FCursorCharPos = Math.Min(FText.Length, newPos);
		}

		void CursorStepsLeft()
		{
			var newPos = Math.Max(0, FCursorCharPos - 1);
			//if cursor lands on an \n and the previouse symbol is an \r make sure to step one more
			try
			{
				if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
					newPos -= 1;
			}
			catch
			{
				//FText[newPos] may access out of range char..nevermind
			}
			
			FCursorCharPos = Math.Max(0, newPos);
		}
		
		void CursorStepsWordRight()
		{
			var nextSpace = FText.IndexOfAny(new char[] {' ', FNewlineSymbol[0]}, FCursorCharPos) + 1;
			FCursorCharPos = Math.Min(FText.Length, nextSpace);
		}
		
		void CursorStepsWordLeft()
		{
			if (FCursorCharPos > 2)
			{
				var lastSpace = FText.LastIndexOfAny(new char[] {' ', FNewlineSymbol[0]}, FCursorCharPos - 2) + 1;
				FCursorCharPos = Math.Max(0, lastSpace);
			}
		}
		#endregion

		#region Commands
		private void AddNewChar(string str)
		{
			FText = FText.Insert(FCursorCharPos, str);
			
			if (str.ToUpper() == str)
				FLastCapitalKey = str;
			
			CursorStepsRight(str.Length);
		}
		
		private void DeleteLeftChar()
		{
			if (FCursorCharPos > 0)
			{
				CursorStepsLeft();
				DeleteRightChar();
			}
		}

		private void DeleteRightChar()
		{
			var charCount = 1;
			if (FText[FCursorCharPos] == '\r' && FNewlineSymbol.Length > 1)
				charCount = FNewlineSymbol.Length;
			
			FText = FText.Remove(FCursorCharPos, charCount);
		}

		private bool RunCommand(uint ScanCode)
		{
			try
			{
				switch ((Keys)ScanCode)
				{
					case 0:
						return true;
					case Keys.Space:
						AddNewChar(" ");
						return true;
					case Keys.Return:
						AddNewChar(FNewlineSymbol);
						return true;
					case Keys.Back:
						DeleteLeftChar();
						return true;
					case Keys.Delete:
						DeleteRightChar();
						return true;
					case Keys.Tab:
						AddNewChar(" ");
						AddNewChar(" ");
						AddNewChar(" ");
						AddNewChar(" ");
						AddNewChar(" ");
						return true;
					case Keys.Left:
						if (!FIgnoreNavigationKeys[0])
							if (FControlKeyPressed)
								CursorStepsWordLeft();
							else
								CursorStepsLeft();
						return true;
					case Keys.Right:
						if (!FIgnoreNavigationKeys[0])
							if (FControlKeyPressed)
								CursorStepsWordRight();
							else
								CursorStepsRight();
						return true;
					case Keys.Up:
						if (!FIgnoreNavigationKeys[0])
							CursorOneLineUp();
						return true;
					case Keys.Down:
						if (!FIgnoreNavigationKeys[0])
							CursorOneLineDown();
						return true;
					case Keys.Home:
						if (!FIgnoreNavigationKeys[0])
							CursorToLineStart();
						return true;
					case Keys.End:
						if (!FIgnoreNavigationKeys[0])
							CursorToLineEnd();
						return true;
					case Keys.Prior:
						if (!FIgnoreNavigationKeys[0])
							CursorToTextStart();
						return true;
					case Keys.Next:
						if (!FIgnoreNavigationKeys[0])
							CursorToTextEnd();
						return true;
				}
			}
			catch { };
			return false;
		}
		#endregion

		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			if (FSetCursorPosition[0])
				FCursorCharPos = Math.Min(FText.Length, Math.Max(0, FNewCursorPosition[0]));

			//initializing with text
			if (FInitialize[0])
			{
				FText = FInitialText[0];
				CursorToTextEnd();
			}

			if (FInsertText[0])
				AddNewChar(FInputText[0]);
			
			if (FKeyboardState.SliceCount > 0)
			{
                var keyboard = FKeyboardState[0] ?? KeyboardState.Empty;
                if (keyboard != FLastKeyboardState)
				{
                    if (keyboard.KeyCodes.Count > 0)
					{
                        FControlKeyPressed = (keyboard.Modifiers & Keys.Control) > 0;
                        var altKeyPressed = (keyboard.Modifiers & Keys.Alt) > 0;
                        var lastKeyCode = (uint)keyboard.KeyCodes.Last();

                        var keyUp = keyboard.KeyCodes.Count < FLastKeyboardState.KeyCodes.Count;
                        var keyDown = keyboard.KeyCodes.Count > FLastKeyboardState.KeyCodes.Count;
							
						if (keyDown || !keyUp)
						{
							if (lastKeyCode < 48)
							{
								if (!altKeyPressed)
									RunCommand(lastKeyCode);
							}
                            else if (keyboard.KeyChars.Count > 0)
                                AddNewChar(keyboard.KeyChars.Last().ToString());
						}
					}
                    FLastKeyboardState = keyboard;
				}
			}

			FOutput[0] = FText;
			FCursorPosition[0] = FCursorCharPos;
		}
	}
}
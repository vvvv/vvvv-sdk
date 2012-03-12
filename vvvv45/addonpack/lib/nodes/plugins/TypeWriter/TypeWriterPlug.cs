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

using VVVV.Utils;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Core.Logging;
#endregion using

namespace TypeWriter
{
	[PluginInfo(Name = "Typewriter",
	            Category = "String",
				Credits = "bo27, Yuri Dolgov",
				Author = "vvvv group",
	            Help = "Takes all keyboardinput including keyboard commands and returns a resulting string",
	            Tags = "keyboard")]
	public class TypeWriterPlugin: IPluginEvaluate
	{
		[DllImport("user32.dll")]
		static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);
		
		[DllImport("user32.dll")]
		static extern bool GetKeyboardState(byte[] lpKeyState);

		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("user32.dll")]
		static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll")]
		static extern short GetKeyState(Keys nVirtKey);

		public static bool IsKeyPressed(Keys testKey)
		{
			bool keyPressed = false;

			short result = GetKeyState(testKey);

			switch (result)
			{
				case 0:
					// Not pressed and not toggled on.
					keyPressed = false;
					break;

				case 1:
					// Not pressed, but toggled on
					keyPressed = false;
					break;

				default:
					// Pressed (and may be toggled on)
					keyPressed = true;
					break;
			}

			return keyPressed;
		}
		public static string VKCodeToUnicode(uint VKCode)
		{
			System.Text.StringBuilder sbString = new System.Text.StringBuilder();

			byte[] bKeyState = new byte[255];
			bool bKeyStateStatus = GetKeyboardState(bKeyState);
			if (!bKeyStateStatus)
				return "";
			uint lScanCode = MapVirtualKey(VKCode, 0);
			IntPtr HKL = GetKeyboardLayout(0);

			ToUnicodeEx(VKCode, lScanCode, bKeyState, sbString, (int)5, (uint)0, HKL);
			return sbString.ToString();
		}

		#region field declaration
		//input pin declaration
		[Input("KeyCode", DefaultValue = 0)]
		IDiffSpread<uint> FInputKey;
		
		[Input("Initial Text", IsSingle = true)]
		ISpread<string> FInitialText;
		
		[Input("Initialize", IsSingle = true, IsBang = true)]
		IDiffSpread<bool> FInitialize;
		
		[Input("Set Cursor Position", IsSingle = true, MinValue = 0, MaxValue = int.MaxValue)]
		IDiffSpread<int> FSetCursorPosition;
		
		//output pin declaration
		[Output("Output")]
		ISpread<string> FOutput;
		
		[Output("Cursor Position")]
		ISpread<int> FCursorPosition;

		[Import()]
		ILogger Flogger;
		
		[Import()]
		IHDEHost FHDEHost;
		
		public string FText = "";
		private int FCursorCharPos = 0;
		//defined as string but must be only one character:
		private string FNewlineSymbol = Environment.NewLine;
		private Dictionary<uint, double> FBufferedCommands = new Dictionary<uint, double>();
		private Dictionary<string, double> FBufferedKeys = new Dictionary<string, double>();
		
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
				
				FCursorCharPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));
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
				
				FCursorCharPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));
			}
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

		void CursorStepsRight()
		{
			var newPos = Math.Min(FText.Length, FCursorCharPos + 1);
			//cursor may currently be on \r when coming from cursorDownOneLine
			//or it may now jump to \r
			//in both cases we need to jump 2 steps to take \n into account
			try
			{
				if ((FText[FCursorCharPos] == '\r' || FText[newPos] == '\r')
					&& FNewlineSymbol.Length > 1)
					newPos += FNewlineSymbol.Length-1;
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
			//cursor may currently be on \n when coming from cursorUpOneLine
			//or it may now jump to \n
			//in both cases we need to jump 2 steps to take \r into account
			try
			{
				if ((FText[FCursorCharPos] == '\n' || FText[newPos] == '\n')
					&& FNewlineSymbol.Length > 1)
					newPos -= FNewlineSymbol.Length-1;
			}
			catch
			{
				//FText[FCursorCharPos] may access out of range char..nevermind
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
				
			CursorStepsRight();
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
						if (IsKeyPressed(Keys.ControlKey))
							CursorStepsWordLeft();
						else
							CursorStepsLeft();
						return true;
					case Keys.Right:
						if (IsKeyPressed(Keys.ControlKey))
							CursorStepsWordRight();
						else
							CursorStepsRight();
						return true;
					case Keys.Up:
						CursorOneLineUp();
						return true;
					case Keys.Down:
						CursorOneLineDown();
						return true;
					case Keys.Home:
						CursorToLineStart();
						return true;
					case Keys.End:
						CursorToLineEnd();
						return true;
					case Keys.Prior:
						CursorToTextStart();
						return true;
					case Keys.Next:
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
            if (FSetCursorPosition.IsChanged)
				FCursorCharPos = Math.Min(FText.Length, Math.Max(0, FSetCursorPosition[0]));

			//initializing with text
			if (FInitialize[0])
			{
				FText = FInitialText[0];
				CursorToTextEnd();
			}
			
			if (FInputKey[0] == 0 
			|| (FInputKey.SliceCount == 1 && FInputKey[0] == (uint)Keys.ControlKey)
			|| (FInputKey.SliceCount == 1 && FInputKey[0] == (uint)Keys.ShiftKey))
			{
				FBufferedKeys.Clear();
				FBufferedCommands.Clear();
			}
			else
			{
				var now = FHDEHost.GetCurrentTime();
				//make current keyboardstate i1!1nto a string
				//access FInputKey[-1] since earlier slices would be ctrl or shift
				var newChar = VKCodeToUnicode(FInputKey[-1]);
				
				//if this is a command
				if (FInputKey[-1] < 48 || string.IsNullOrEmpty(newChar.Trim()))
				{
					//add this to buffered commands
					if (FInputKey[-1] != 17)
						if (!FBufferedCommands.ContainsKey(FInputKey[-1]))
							FBufferedCommands.Add(FInputKey[-1], now);

					FBufferedKeys.Clear();
				}
				else //this is a string
				{
					if (!FBufferedKeys.ContainsKey(newChar))
						FBufferedKeys.Add(newChar, now);
					
					FBufferedCommands.Clear();
				}
					
				//check if any of the buffered keys/commands need to executed
				foreach (var key in FBufferedKeys)
					if (now - key.Value == 0 || now - key.Value > 0.4)
						AddNewChar(key.Key);
				
				foreach (var key in FBufferedCommands)
					if (now - key.Value == 0 || now - key.Value > 0.4)
						RunCommand(key.Key);
			}	
			
			FOutput[0] = FText;
			FCursorPosition[0] = FCursorCharPos;
		}
	}
}
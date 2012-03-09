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
	            Help = "",
	            Tags = "")]
	public class TypeWriterPlugin : IPluginEvaluate
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
		
		[Input("Doubles Speed", MinValue = 0, MaxValue = 1)]
		IDiffSpread<double> FDoublesSpeed;
		
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
		
		public string FText;
		/// <summary>
		/// Входная строка
		/// </summary>
		public string KbdInput = string.Empty;
		/// <summary>
		/// Скорость повтора
		/// </summary>
		public double DSpeed = 0;
		/// <summary>
		/// Контрольный ключ
		/// </summary>
		public string PKey = string.Empty;
		/// <summary>
		/// запоминаем введенный символ
		/// </summary>
		private string LastChar;
		/// <summary>
		/// последняя команда
		/// </summary>
		private uint LastScanCode;
		private uint DrebezgScanCode;
		private int FCursorCharPos = 0;
		
		//defined as string but must be only one character:
		private string FNewlineSymbol = Environment.NewLine;

		// Делегат, вызывающий конец паузы по таймеру
		TimerCallback timerDelegate;
		/// <summary>
		/// Переинициализация модема
		/// </summary>
		private System.Threading.Timer FRepeatTimer;
		private int timerdelay = 0;
		
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		#endregion field declaration

		#region constructor/destructor
		public TypeWriterPlugin()
		{
//			timerDelegate = new TimerCallback(RepeatChar);
//			FRepeatTimer = new System.Threading.Timer(timerDelegate, 1, Timeout.Infinite, Timeout.Infinite);
			
			FText = "";
		}

		// Implementing IDisposable's Dispose method.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!FDisposed)
			{
				if (disposing)
				{
					// Dispose managed resources.
					FRepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
					FRepeatTimer.Dispose();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.

				//Flogger.Log(LogType.Message, "TypeWriter is being deleted");

				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~TypeWriterPlugin()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}
		#endregion constructor/destructor

		#region Text navigation helper
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

		#region таймеры дребезга и повтора
		/// <summary>
		/// Повтор символа
		/// </summary>
		/// <param name="stateInfo"></param>
		/*public void RepeatChar(Object stateInfo)
		{
			try
			{
				int sInfo = 0;
				sInfo = (int)stateInfo;

				if (sInfo == 1)
				{
					if (LastChar != null)
					{
						NewCommand(LastChar);
//						Invalidate = true;
					}
					if (LastScanCode != 0)
					{
						RunCommand(LastScanCode);
//						Invalidate = true;
					}
				}
			}
			catch { };
		}*/

		#endregion

		#region Commands
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
					case Keys.Return:
						AddNewChar(FNewlineSymbol);
						return true;
					case Keys.Back:
						LastScanCode = ScanCode;
						DeleteLeftChar();
						return true;
					case Keys.Delete:
						LastScanCode = ScanCode;
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

		private void AddNewChar(string str)
		{
			FText = FText.Insert(FCursorCharPos, str);
			LastChar = str;
				
			CursorStepsRight();
		}
		#endregion

		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			#region входные параметры
			
            if (FSetCursorPosition.IsChanged)
				FCursorCharPos = Math.Min(FText.Length, Math.Max(0, FSetCursorPosition[0]));

			/*
            if (FDoublesSpeed.IsChanged)
            {
                DSpeed = FDoublesSpeed[0];
                // Сбрасываем сторожевой таймер
                if (RepeatTimer != null)
                {
                    if (DSpeed == 0)
                    {
                        RepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        timerdelay = 0;
                    }
                    else
                    {
                        timerdelay = Convert.ToInt32(DSpeed * 1000);
                        if (timerdelay > 0)
                        {
                            if (timerdelay < 200) timerdelay = 200;
                            RepeatTimer.Change(timerdelay, timerdelay);
                        }
                    }
                }
            }
			 */
			#endregion

			//initializing with text
			if (FInitialize[0])
			{
				FText = FInitialText[0];
				CursorToTextEnd();

				LastChar = null;
				LastScanCode = 0;
				DrebezgScanCode = 0;
//				FRepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}

			//key input
			if (FInputKey.IsChanged)
			{
//				Flogger.Log(LogType.Debug, FInputKey[0].ToString());
				if (FInputKey.SliceCount > 0)
				{
					
//					double a;
//					/// проверка сканкода
//					if (LastScanCode != 0 || LastChar != null)
//					{
//						bool flagScanCode = false;
//						bool flagLastChar = false;
//						
//						for (int i = 0; i < FInputKey.SliceCount; i++)
//						{
//							a = FInputKey[i];
//							uint ScanCode = Convert.ToUInt16(a);
//							if (ScanCode == LastScanCode)
//								flagScanCode = true;
//							string newchar = VKCodeToUnicode(ScanCode);
//							if (newchar == LastrstChar)
//								flagLastChar = true;
//						}
//						
//						if (flagScanCode == false)
//							LastScanCode = 0;
//						if (flagLastChar == false)
//							LastChar = null;
//					}
					
					for (int i = 0; i < FInputKey.SliceCount; i++)
					{
//						a = FInputKey[i];

//						uint ScanCode = Convert.ToUInt16(a);
//						/// все кнопки отпущены
//						if (FInputKey.SliceCount == 1 && ScanCode == 0)
//						{
//							LastChar = null;
//							LastScanCode = 0;
//							DrebezgScanCode = 0;
////							FRepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
//						}
						
						string newchar = VKCodeToUnicode(FInputKey[i]);
						Flogger.Log(LogType.Debug, newchar);
						
//						if (DrebezgScanCode == 0)
//						{
//							if (newchar != "" && IsKeyPressed((Keys)ScanCode))
//								DrebezgScanCode = ScanCode;
//						}
//						else
//						{
//							if (newchar != "" && DrebezgScanCode == ScanCode)
//								return;
//						}
//						
//						if (timerdelay != 0)
//						{
////							if (IsKeyPressed((Keys)ScanCode))
////								FRepeatTimer.Change(timerdelay, timerdelay);
//						}
						
						if (RunCommand(FInputKey[i]))
							continue;
						
						if (!string.IsNullOrEmpty(newchar) && newchar[0] > 31)
							AddNewChar(newchar);
					}
				}
			}
			
			FOutput[0] = FText;
			FCursorPosition[0] = FCursorCharPos;
		}
	}
}
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
using System.Reactive.Linq;

using VVVV.Utils;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Core.Logging;
using VVVV.Utils.IO;
#endregion using

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Typewriter",
	            Category = "String",
	            Credits = "Based on an original version by bo27, Yuri Dolgov",
	            Author = "vvvv group",
	            Help = "Takes all keyboardinput including keyboard commands and returns a resulting string",
	            Tags = "keyboard")]
	public class TypeWriterPlugin: IPluginEvaluate, IDisposable
	{
		//input pin declaration
		[Input("Keyboard")]
		IDiffSpread<Keyboard> FKeyboardIn;
		
		[Input("Text", DefaultString = "")]
		ISpread<string> FInputText;
		
		[Input("Insert Text", IsBang = true)]
		ISpread<bool> FInsertText;
		
		[Input("Initial Text")]
		ISpread<string> FInitialText;
		
		[Input("Initialize", IsBang = true)]
		ISpread<bool> FInitialize;
		
		[Input("Cursor Position", MinValue = 0, MaxValue = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
		ISpread<int> FNewCursorPosition;
		
		[Input("Set Cursor Position", IsBang = true, Visibility = PinVisibility.OnlyInspector)]
		ISpread<bool> FSetCursorPosition;
		
		[Input("Ignore Navigation Keys", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector)]
		ISpread<bool> FIgnoreNavigationKeys;
		
		//output pin declaration
		[Output("Output")]
		ISpread<string> FOutput;
		
		[Output("Cursor Position")]
		ISpread<int> FCursorPosition;

        Spread<TypeWriter> FTypeWriters = new Spread<TypeWriter>();
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int spreadMax)
		{
            FTypeWriters.ResizeAndDispose(spreadMax);
            FOutput.SliceCount = spreadMax;
            FCursorPosition.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var typeWriter = FTypeWriters[i];
                typeWriter.IgnoreNavigationKeys = FIgnoreNavigationKeys[i];
                typeWriter.Keyboard = FKeyboardIn[i];
                if (FSetCursorPosition[i])
                    typeWriter.CursorPosition = FNewCursorPosition[i];
                if (FInitialize[i])
                    typeWriter.Initialize(FInitialText[i]);
                if (FInsertText[i])
                    typeWriter.InsertText(FInputText[i]);
                FOutput[i] = typeWriter.Output;
                FCursorPosition[i] = typeWriter.CursorPosition;
            }
		}

        public void Dispose()
        {
            FTypeWriters.ResizeAndDispose(0);
        }
    }
}
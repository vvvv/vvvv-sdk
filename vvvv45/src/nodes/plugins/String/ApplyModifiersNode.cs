#region usings
using System;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ApplyModifiers", 
	            Category = "String",
                Version = "Legacy",
	            Help = "Returns the currently pressed character of a keyboard.",
	            AutoEvaluate = true,
				Tags = "keyboard, convert")]
	#endregion PluginInfo
    public class ApplyModifiersNode : IPluginEvaluate, IDisposable
	{
        [Input("Input")]
        public ISpread<Keyboard> Input;

        [Output("Output")]
        public ISpread<string> Output;

        Spread<Subscription<Keyboard, KeyNotification>> FSubscriptions = new Spread<Subscription<Keyboard, KeyNotification>>();

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
        }

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
            FSubscriptions.ResizeAndDispose(
                spreadMax,
                slice =>
                {
                    return new Subscription<Keyboard, KeyNotification>(
                        keyboard => keyboard.KeyNotifications,
                        (keyboard, n) =>
                        {
                            switch (n.Kind)
                            {
                                case KeyNotificationKind.KeyPress:
                                    var keyPress = n as KeyPressNotification;
                                    Output[slice] = new string(keyPress.KeyChar, 1);
                                    break;
                                case KeyNotificationKind.KeyUp:
                                    Output[slice] = string.Empty;
                                    break;
                            }
                        }
                    );
                }
            );
            for (int i = 0; i < spreadMax; i++)
                FSubscriptions[i].Update(Input[i]);
		}
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Windows.Forms;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;
using VVVV.Core.Logging;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Concurrency;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "KeyMatch",
                Category = "Keyboard",
                Help = "Detects pressed keys when connected with a Keyboard Node. Use the inspector to specify the keys to check.",
                AutoEvaluate = true)]
    public class KeyMatchNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        public enum KeyMode { Press, Toggle, UpOnly, DownOnly, DownUp, RepeatedEvent };

        class KeyMatch : IDisposable
        {
            public readonly Keys KeyCode;
            private readonly IScheduler FScheduler;
            private readonly IIOContainer<ISpread<bool>> FContainer;
            private IDisposable FSubscription;

            public KeyMatch(IIOFactory factory, Keys keyCode, IScheduler scheduler)
            {
                KeyCode = keyCode;
                FScheduler = scheduler;
                var attribute = new OutputAttribute(keyCode.ToString())
                {
                    IsSingle = true,
                    IsBang = true
                };
                FContainer = factory.CreateIOContainer<ISpread<bool>>(attribute);
            }

            public void Dispose()
            {
                Unsubscribe();
                FContainer.Dispose();
            }

            private bool Output
            {
                get { return FContainer.IOObject[0]; }
                set { FContainer.IOObject[0] = value; }
            }

            public void Subscribe(Keyboard keyboard, KeyMode mode)
            {
                var notifications = keyboard.KeyNotifications
                    .OfType<KeyCodeNotification>()
                    .Where(n => n.KeyCode == KeyCode);

                var distinctNotifications = notifications.DistinctUntilChanged(n => n.Kind);

                IObservable<bool> result;
                switch (mode)
                {
                    case KeyMode.Press:
                        result = distinctNotifications.Select(n => n.Kind == KeyNotificationKind.KeyDown);
                        break;
                    case KeyMode.Toggle:
                        result = distinctNotifications.OfType<KeyDownNotification>()
                            .Scan(false, (toggled, _) => !toggled);
                        break;
                    case KeyMode.UpOnly:
                        result = distinctNotifications.OfType<KeyUpNotification>().Edge();
                        break;
                    case KeyMode.DownOnly:
                        result = distinctNotifications.OfType<KeyDownNotification>().Edge();
                        break;
                    case KeyMode.DownUp:
                        result = distinctNotifications.Edge();
                        break;
                    case KeyMode.RepeatedEvent:
                        result = notifications.OfType<KeyDownNotification>().Edge();
                        break;
                    default:
                        throw new NotImplementedException();
                }

                FSubscription = result.ObserveOn(FScheduler)
                    .Subscribe(v => Output = v);
            }

            public void Unsubscribe()
            {
                if (FSubscription != null)
                {
                    FSubscription.Dispose();
                    FSubscription = null;
                }
                Output = false;
            }
        }

        [Config("Key Match", IsSingle = true)]
        public IDiffSpread<string> KeyMatchConfig;

        [Input("Keyboard", IsSingle = true)]
        public ISpread<Keyboard> KeyboardIn;

        [Input("Reset Toggle", IsSingle = true, IsBang = true)]
        public ISpread<bool> ResetIn;

        [Input("Key Mode", IsSingle = true)]
        public ISpread<KeyMode> KeyModeIn;

        [Import()]
        protected IIOFactory FIOFactory;

        private readonly List<KeyMatch> FKeyMatches = new List<KeyMatch>();
        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Keyboard FKeyboard = Keyboard.Empty;
        private KeyMode FKeyMode;

        public void OnImportsSatisfied()
        {
            KeyMatchConfig.Changed += KeyMatchChangedCB;
        }

        void KeyMatchChangedCB(IDiffSpread<string> sender)
        {
            var keyCodes = KeyMatchConfig[0].ToKeyCodes();

            //add new pins
            foreach (var keyCode in keyCodes)
            {
                if (!FKeyMatches.Any(keyMatch => keyMatch.KeyCode == keyCode))
                {
                    var keyMatch = new KeyMatch(FIOFactory, keyCode, FScheduler);
                    keyMatch.Subscribe(FKeyboard, FKeyMode);
                    FKeyMatches.Add(keyMatch);
                }
            }

            //remove obsolete pins
            foreach (var keyMatch in FKeyMatches.ToArray())
            {
                if (!keyCodes.Contains(keyMatch.KeyCode))
                {
                    FKeyMatches.Remove(keyMatch);
                    keyMatch.Dispose();
                }
            }
        }

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            //resubsribe if necessary
            var keyboard = KeyboardIn[0] ?? Keyboard.Empty;
            var keyMode = KeyModeIn[0];
            if (ResetIn[0] || (keyboard != FKeyboard) || (keyMode != FKeyMode))
            {
                FKeyboard = keyboard;
                FKeyMode = keyMode;
                foreach (var keyMatch in FKeyMatches)
                {
                    keyMatch.Unsubscribe();
                    keyMatch.Subscribe(keyboard, keyMode);
                }
            }

            //process events
            FScheduler.Run();
        }
    }

    [PluginInfo(Name = "RadioKeyMatch",
                Category = "Keyboard",
                Help = "Similiar to KeyMatch, but does not create a output pin for each key to check, but returns the index of the pressed key on its output pin.",
                AutoEvaluate = true)]
    public class RadioKeyMatchNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Keyboard", IsSingle = true)]
        public ISpread<Keyboard> KeyboardIn;

        [Input("Key Match")]
        public ISpread<string> KeyMatchIn;

        [Output("Output")]
        public ISpread<int> Output;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Subscription<Keyboard, KeyDownNotification> FSubscription;

        public void OnImportsSatisfied()
        {
            FSubscription = new Subscription<Keyboard, KeyDownNotification>(
                keyboard => keyboard.KeyNotifications.OfType<KeyDownNotification>(),
                n =>
                {
                    var pressedKeyCode = n.KeyCode;
                    for (int i = 0; i < Output.SliceCount; i++)
                    {
                        var keyCodes = KeyMatchIn[i].ToKeyCodes();
                        var index = keyCodes.IndexOf(pressedKeyCode);
                        if (index >= 0)
                            Output[i] = index;
                    }
                },
                FScheduler
            );
        }

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            //resubscribe if necessary
            FSubscription.Update(KeyboardIn[0]);
            //prepare output
            Output.SliceCount = spreadMax;
            //process events
            FScheduler.Run();
        }

        public void Dispose()
        {
            FSubscription.Dispose();
        }
    }
}

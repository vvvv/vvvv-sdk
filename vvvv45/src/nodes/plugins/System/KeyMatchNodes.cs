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
    public class KeyMatchNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        public enum KeyMode { Press, Toggle, UpOnly, DownOnly, DownUp, RepeatedEvent };

        class Column : IDisposable
        {
            public readonly Keys KeyCode;
            public readonly IIOContainer<ISpread<bool>> IOContainer;
            public readonly Spread<KeyMatch> KeyMatches = new Spread<KeyMatch>();
            private readonly IScheduler FScheduler;

            public Column(IIOFactory factory, Keys keyCode, IScheduler scheduler)
            {
                KeyCode = keyCode;
                FScheduler = scheduler;
                var attribute = new OutputAttribute(keyCode.ToString()) { IsBang = true };
                IOContainer = factory.CreateIOContainer<ISpread<bool>>(attribute);
            }

            public void Resize(int sliceCount)
            {
                KeyMatches.ResizeAndDispose(sliceCount, slice => new KeyMatch(IOContainer.IOObject, KeyCode, slice, FScheduler));
                IOContainer.IOObject.SliceCount = sliceCount;
            }

            public void Dispose()
            {
                Resize(0);
                IOContainer.Dispose();
            }
        }

        class KeyMatch : IDisposable
        {
            public readonly Keys KeyCode;
            private readonly IScheduler FScheduler;
            private readonly ISpread<bool> FOutputSpread;
            private readonly int FSlice;
            private IDisposable FSubscription;
            private Keyboard FKeyboard;
            private KeyMode FKeyMode;

            public KeyMatch(ISpread<bool> outputSpread, Keys keyCode, int slice, IScheduler scheduler)
            {
                KeyCode = keyCode;
                FSlice = slice;
                FScheduler = scheduler;
                FOutputSpread = outputSpread;
            }

            public void Dispose()
            {
                Unsubscribe();
            }

            private bool Output
            {
                get { return FOutputSpread[FSlice]; }
                set { FOutputSpread[FSlice] = value; }
            }

            public void Update(Keyboard keyboard, KeyMode mode)
            {
                if (FKeyboard != keyboard || FKeyMode != mode)
                {
                    Unsubscribe();
                    FKeyboard = keyboard;
                    FKeyMode = mode;
                    Subscribe();
                }
            }

            public void Reset()
            {
                Unsubscribe();
                Subscribe();
            }

            public void Subscribe()
            {
                var notifications = FKeyboard.KeyNotifications
                    .OfType<KeyCodeNotification>()
                    .Where(n => n.KeyCode == KeyCode);

                var distinctNotifications = notifications.DistinctUntilChanged(n => n.Kind);

                IObservable<bool> result;
                switch (FKeyMode)
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

                //on keyboard lost: keep toggle state when in toggle mode; in any other case just release the key 
                if (FKeyMode != KeyMode.Toggle)
                    result = result.Merge(FKeyboard.KeyNotifications.OfType<KeyboardLostNotification>().Select(_ => false));

                FSubscription = result
                    .ObserveOn(FScheduler)
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

        [Input("Keyboard")]
        public ISpread<Keyboard> KeyboardIn;

        [Input("Reset Toggle", IsBang = true)]
        public ISpread<bool> ResetIn;

        [Input("Key Mode")]
        public ISpread<KeyMode> KeyModeIn;

        [Import()]
        protected IIOFactory FIOFactory;

        private readonly List<Column> FColumns = new List<Column>();
        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();

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
                if (!FColumns.Any(col => col.KeyCode == keyCode))
                {
                    var column = new Column(FIOFactory, keyCode, FScheduler);
                    FColumns.Add(column);
                }
            }

            //remove obsolete pins
            foreach (var column in FColumns.ToArray())
            {
                if (!keyCodes.Contains(column.KeyCode))
                {
                    FColumns.Remove(column);
                    column.Dispose();
                }
            }

            var spreadMax = SpreadUtils.SpreadMax(KeyMatchConfig, KeyboardIn, ResetIn, KeyModeIn);
            UpdateColumns(spreadMax);
        }

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            //update the key match columns
            UpdateColumns(spreadMax);
            //process events
            FScheduler.Run();
        }

        private void UpdateColumns(int spreadMax)
        {
            foreach (var column in FColumns)
                column.Resize(spreadMax);
            for (int i = 0; i < spreadMax; i++)
            {
                var keyboard = KeyboardIn[i] ?? Keyboard.Empty;
                var keyMode = KeyModeIn[i];
                var doReset = ResetIn[i];
                foreach (var column in FColumns)
                {
                    var keyMatch = column.KeyMatches[i];
                    keyMatch.Update(keyboard, keyMode);
                    if (doReset)
                        keyMatch.Reset();
                }
            }
        }

        public void Dispose()
        {
            foreach (var column in FColumns)
                column.Dispose();
        }
    }

    [PluginInfo(Name = "RadioKeyMatch",
                Category = "Keyboard",
                Help = "Similiar to KeyMatch, but does not create a output pin for each key to check, but returns the index of the pressed key on its output pin.",
                AutoEvaluate = true)]
    public class RadioKeyMatchNode : IPluginEvaluate, IDisposable
    {
        [Input("Keyboard")]
        public ISpread<Keyboard> KeyboardIn;

        [Input("Key Match")]
        public ISpread<ISpread<string>> KeyMatchesIn;

        [Output("Output")]
        public ISpread<ISpread<int>> Outputs;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private readonly Spread<Subscription<Keyboard, KeyDownNotification>> FSubscriptions = new Spread<Subscription<Keyboard, KeyDownNotification>>();

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            spreadMax = SpreadUtils.SpreadMax(KeyboardIn, KeyMatchesIn);
            FSubscriptions.ResizeAndDispose(
                spreadMax,
                slice =>
                {
                    return new Subscription<Keyboard, KeyDownNotification>(
                        keyboard => keyboard.KeyNotifications.OfType<KeyDownNotification>(),
                        (keyboard, n) =>
                        {
                            var pressedKeyCode = n.KeyCode;
                            var keyMatches = KeyMatchesIn[slice];
                            var output = Outputs[slice];
                            for (int i = 0; i < output.SliceCount; i++)
                            {
                                var keyCodes = keyMatches[i].ToKeyCodes();
                                var index = keyCodes.IndexOf(pressedKeyCode);
                                if (index >= 0)
                                    output[i] = index;
                            }
                        },
                        FScheduler
                    );
                }
            );

            Outputs.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            {
                Outputs[i].SliceCount = KeyMatchesIn[i].SliceCount;
                FSubscriptions[i].Update(KeyboardIn[i]);
            }

            //process events
            FScheduler.Run();
        }

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "KeyboardState", Category = "System", Version = "Join Legacy")]
    public class LegacyKeyStateJoinNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Key Code")]
        public IDiffSpread<ISpread<int>> FKeyIn;

        [Input("Caps Lock", IsSingle = true)]
        public IDiffSpread<bool> FCapsIn;

        [Input("Time")]
        public IDiffSpread<int> FTimeIn;
        
        [Output("Keyboard")]
        public ISpread<Keyboard> FOutput;

        Spread<Subject<KeyNotification>> FSubjects = new Spread<Subject<KeyNotification>>();
        Spread<KeyboardState> FKeyboardStates = new Spread<KeyboardState>();

        public void OnImportsSatisfied()
        {
            FOutput.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            if (!FKeyIn.IsChanged && !FCapsIn.IsChanged && !FTimeIn.IsChanged) return;

            spreadMax = SpreadUtils.SpreadMax(FKeyIn, FCapsIn, FTimeIn);
            FSubjects.ResizeAndDispose(spreadMax, slice => new Subject<KeyNotification>());
            FOutput.ResizeAndDismiss(spreadMax, slice => new Keyboard(FSubjects[slice], true));
            FKeyboardStates.ResizeAndDismiss(spreadMax, () => KeyboardState.Empty);

            for (int i = 0; i < spreadMax; i++)
            {
                var keyboard = FOutput[i];
                var keyboardState = new KeyboardState(FKeyIn[i].Cast<Keys>(), FCapsIn[0], FTimeIn[i]);
                var previousKeyboardState = FKeyboardStates[i];
                if (keyboardState != previousKeyboardState)
                {
                    var subject = FSubjects[i];
                    var keyDowns = keyboardState.KeyCodes.Except(previousKeyboardState.KeyCodes);
                    foreach (var keyDown in keyDowns)
                        subject.OnNext(new KeyDownNotification(keyDown, this));
                    var keyUps = previousKeyboardState.KeyCodes.Except(keyboardState.KeyCodes);
                    foreach (var keyUp in keyUps)
                        subject.OnNext(new KeyUpNotification(keyUp, this));
                }
                FKeyboardStates[i] = keyboardState;
            }
        }
    }
}

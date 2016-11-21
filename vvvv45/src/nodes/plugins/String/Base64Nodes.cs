using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.IO;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Text
{
    [PluginInfo(Name = "Encode", Category = "String", Version = "Base64")]
    public class Base64EncodeNode : IPluginEvaluate
    {
        [Input("Input Data")]
        public IDiffSpread<Stream> FInput;

        [Output("Output String")]
        public ISpread<string> FOutput;

        public void Evaluate(int spreadMax)
        {
            if (!FInput.IsChanged) return;

            FOutput.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            {
                var inputStream = FInput[i];
                inputStream.Position = 0;
                var inputArray = new byte[inputStream.Length];
                inputStream.Read(inputArray, 0, inputArray.Length);
                FOutput[i] = Convert.ToBase64String(inputArray);
            }
        }
    }

    [PluginInfo(Name = "Decode", Category = "Raw", Version = "Base64")]
    public class Base64DecodeNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input String")]
        public IDiffSpread<string> FInput;

        [Output("Output Data", AutoFlush = false)]
        public ISpread<Stream> FOutput;

        public void OnImportsSatisfied()
        {
            FOutput.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            if (!FInput.IsChanged) return;

            FOutput.ResizeAndDispose(spreadMax, () => new MemoryComStream());
            for (int i = 0; i < spreadMax; i++)
            {
                var outputArray = Convert.FromBase64String(FInput[i]);
                var outputStream = FOutput[i];
                outputStream.Position = 0;
                outputStream.SetLength(outputArray.Length);
                outputStream.Write(outputArray, 0, outputArray.Length);
            }
            // Will mark all slices changed
            FOutput.Flush(true);
        }
    }
}

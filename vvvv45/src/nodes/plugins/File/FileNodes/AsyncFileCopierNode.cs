using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.File
{
    [PluginInfo(
        Name = "Copier", 
        Category = "File", 
        Version = "Async",
        Tags = "copy",
        Help = "Copies a spread of files to a location without causing vvvv to halt during the copy process. Outputs the progress of the copy process.")]
    public class AsyncFileCopierNode : IPluginEvaluate, IDisposable
    {
        class CopyOperation : IDisposable
        {
            public readonly string From;
            public readonly string To;
            private CancellationTokenSource CancellationTokenSource;
            private Task CopyTask;

            public CopyOperation(string from, string to)
            {
                From = from;
                To = to;
            }

            public void Run(IProgress<float> progress)
            {
                CancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = CancellationTokenSource.Token;
                CopyTask = Task.Run(
                    async () =>
                    {
                        using (var source = new FileStream(From, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var destination = new FileStream(To, FileMode.Create, FileAccess.Write))
                        {
                            var streamLength = (float)source.Length;
                            var buffer = new byte[0x1000];
                            int numCopied = 0;
                            int numRead;
                            while ((numRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
                            {
                                await destination.WriteAsync(buffer, 0, numRead, cancellationToken);
                                numCopied += numRead;
                                progress.Report(numCopied / streamLength);
                            }
                        }
                    },
                    CancellationTokenSource.Token
                );
            }

            public bool IsRunning { get { return CopyTask != null; } }

            public bool IsCompleted { get { return CopyTask != null && CopyTask.IsCompleted; } }

            public void Dispose()
            {
                if (CancellationTokenSource != null)
                {
                    CancellationTokenSource.Cancel();
                    CancellationTokenSource.Dispose();
                    CancellationTokenSource = null;
                }
                if (CopyTask != null)
                {
                    try
                    {
                        CopyTask.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        ae.Handle(e => e is OperationCanceledException);
                    }
                    finally
                    {
                        CopyTask.Dispose();
                        CopyTask = null;
                    }
                }
            }
        }

        [Input("From Filename")]
        public ISpread<string> FromFilenameIn;
        [Input("To Filename")]
        public ISpread<string> ToFilenameIn;
        [Input("Copy", IsBang = true)]
        public ISpread<bool> DoCopyIn;
        [Output("Progress")]
        public ISpread<float> ProgressOut;
        [Import]
        public ILogger Logger;

        private readonly Spread<CopyOperation> FCopyOperations = new Spread<CopyOperation>();

        public void Evaluate(int spreadMax)
        {
            FCopyOperations.Resize(spreadMax, i => new CopyOperation(FromFilenameIn[i], ToFilenameIn[i]), DisposeAndLogExceptions);
            ProgressOut.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
			{
                var copyOperation = FCopyOperations[i];
                var from = FromFilenameIn[i];
                var to = ToFilenameIn[i];
                if (from != copyOperation.From || to != copyOperation.To || copyOperation.IsCompleted)
                {
                    DisposeAndLogExceptions(copyOperation);
                    copyOperation = new CopyOperation(from, to);
                    ProgressOut[i] = 0;
                }
                var doCopy = DoCopyIn[i];
                if (doCopy && !copyOperation.IsRunning)
                {
                    copyOperation.Run(new Progress<float>(p => ProgressOut[i] = p));
                }
                FCopyOperations[i] = copyOperation;
			}
        }

        public void Dispose()
        {
            foreach (var copyOperation in FCopyOperations)
                DisposeAndLogExceptions(copyOperation);
        }

        private void DisposeAndLogExceptions(CopyOperation copyOperation)
        {
            try
            {
                copyOperation.Dispose();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }
    }
}

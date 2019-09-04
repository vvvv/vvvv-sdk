using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using VL.Lang.Helper;
using VL.Lang.Platforms.CIL;
using VL.Lang.Platforms.CIL.Runtime;
using VL.Lang.Symbols;
using VL.Lib.Animation;
using VL.Model;
using VL.UI.Core;
using VVVV.Core.Logging;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;

namespace VVVV.VL.Hosting
{
    public class RuntimeHost : IRuntimeHost
    {
        public event EventHandler<TargetCompilationUpdateEventArgs> Updated;
        public event EventHandler<FrameCompletedEventArgs> FrameCompleted;
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler Paused;
        public event EventHandler ModeChanged;
        public event EventHandler<Exception> OnException;

        private ImmutableArray<NodePlugin> FInstances = ImmutableArray<NodePlugin>.Empty;
        private Platform FPlatform;
        private Host FVlHost;
        private IHDEHost FHDEHost;
        private ILogger FLogger;
        private CilCompilation FCompilation;
        private IObservable<EventPattern<FrameCompletedEventArgs>> FFrames;
        private IObservable<EventPattern<TargetCompilationUpdateEventArgs>> FUpdates;
        private IObservable<EventPattern<Exception>> FExceptions;
        private Exception FException;
        private bool FBusy;

        private ImplicitEntryPointInstanceManager ImplicitEntryPointInstanceManager;

        class HDERealTimeClock : IClock
        {
            IHDEHost FHDEHost;

            public HDERealTimeClock(IHDEHost host)
            {
                FHDEHost = host;
            }

            public Time Time => FHDEHost.RealTime;
        }

        public RuntimeHost(Platform platform)
        {
            ImplicitEntryPointInstanceManager = new ImplicitEntryPointInstanceManager();
            ImplicitEntryPointInstanceManager.OnException += ImplicitEntryPointInstanceManager_OnException;
            Platform = platform;
            Mode = RunMode.Running;
        }

        private void ImplicitEntryPointInstanceManager_OnException(object sender, Exception e)
        {
            RaiseOnException(e);
        }

        // Called on main thread
        public void Initialize(Host vlHost, IHDEHost hdeHost, ILogger logger)
        {
            FVlHost = vlHost;
            FPlatform = (Platform)FVlHost.Session.TargetPlatform;
            
            FHDEHost = hdeHost;
            FLogger = logger;
            FHDEHost.MainLoop.OnResetCache += HandleMainLoopOnResetCache;
            FCompilation = FPlatform.LatestCompilation;
            Clocks.FRealTimeClock = new HDERealTimeClock(FHDEHost);
            FHDEHost.MainLoop.OnInitFrame += MainLoop_OnInitFrame;
        }

        private void MainLoop_OnInitFrame(object sender, EventArgs e)
        {
            Clocks.FFrameClock.SetFrameTime(FHDEHost.FrameTime);

            if (Mode == RunMode.Running || Mode == RunMode.Stepping)
                ClearRuntimeMessages();
        }

        private void ClearRuntimeMessages()
        {
            foreach (var i in FInstances)
                i.ClearRuntimeMessages();
        }

        public async Task UpdateAsync(CilCompilation compilation, CancellationToken token)
        {
            try
            {
                var results = new List<NodePlugin.BuildResult>(FInstances.Length);
                foreach (var instance in FInstances)
                {
                    var buildResult = await Task.Run(() => instance.Update(compilation), token);
                    results.Add(buildResult);
                }

                token.ThrowIfCancellationRequested();

                using (var swapper = new HotSwapper(Compilation, compilation, token))
                {
                    foreach (var result in results)
                    {
                        var instance = result.Plugin;
                        if (!instance.IsDisposed) // It could be that the node has already been deleted
                            instance.SyncPinsAndRestoreState(result, swapper);
                    }
                    ImplicitEntryPointInstanceManager.Update(swapper);
                    Compilation = compilation;
                }
            }
            catch (OperationCanceledException)
            {
                // Fine
            }
            catch (Exception e)
            {
                LogAndStop(e);
            }
        }

        private void LogAndStop(Exception e)
        {
            // Log the error
            FLogger.Log(e);
            // Put runtime into stop mode so state gets cleared and user has a chance to act on error
            Mode = RunMode.Stopped;
        }

        public Platform Platform { get; }
        public CilCompilation Compilation
        {
            get { return FCompilation; }
            private set
            {
                if (value != Compilation)
                {
                    FCompilation = value;
                    Updated?.Invoke(this, new TargetCompilationUpdateEventArgs(CancellationToken.None, null, value));
                }
            }
        }

        public bool IsRunning => Mode == RunMode.Running || Mode == RunMode.Stepping;
        public IEnumerable<IRuntimeInstance> HostingAppInstances => FInstances;
        public IEnumerable<IRuntimeInstance> ImplicitEntryPointInstances => ImplicitEntryPointInstanceManager.Instances;
        public ulong Frame { get; private set; }
        public VLSession Session => FVlHost.Session;


        bool FCompilationNeededBeforeRestarting;
        public bool CompilationNeededBeforeRestarting
        {
            get => FCompilationNeededBeforeRestarting;
            set { FCompilationNeededBeforeRestarting |= value; }
        }

        async Task CompileThenSetMode(RunMode mode)
        {
            await Session.UpdateCompilationAsync(Session.CurrentSolution, progress: null, incremental: true);
            FCompilationNeededBeforeRestarting = false;
            Mode = mode;
        }

        RunMode FMode;
        public RunMode Mode
        {
            get { return FMode; }
            set
            {
                if (FMode != value)
                {
                    //if ((FMode == RunMode.Stopped || FMode == RunMode.Paused) && (value == RunMode.Stepping || value == RunMode.Running) && (FSession.IsWaitingForDelayedHotSwap))
                    //    FSession.HotSwapNow();
                    if (FCompilationNeededBeforeRestarting)
                    {
                        CompileThenSetMode(value);
                        return;
                    }

                    FMode = value;
                    ModeChanged?.Invoke(this, EventArgs.Empty);
                    switch (value)
                    {
                        case RunMode.Stopped:
                            Stop();
                            OnStopped();
                            break;
                        case RunMode.Paused:
                            OnPaused();
                            break;
                        case RunMode.Running:
                        case RunMode.Stepping: // todo: cleanup this logic. step is no state
                            ClearRuntimeMessages();
                            OnStarted();
                            break;
                    }
                }
            }
        }

        public void Dispose()
        {
            FHDEHost.MainLoop.OnResetCache -= HandleMainLoopOnResetCache;
            FInstances = FInstances.Clear();
            FrameCompleted = null;
        }

        private void HandleMainLoopOnResetCache(object sender, EventArgs e)
        {
            Step();
        }

        bool pauseNextFrame;
        public void Step()
        {
            if (pauseNextFrame)
                Mode = RunMode.Paused;
            pauseNextFrame = Mode == RunMode.Stepping;
            if (Mode == RunMode.Running || Mode == RunMode.Stepping)
            {
                SharedStep();
            }
        }

        /// <summary>
        /// keep in sync with standalone runtimehost
        /// </summary>
        void SharedStep()
        {
            if (FBusy)
                return;

            FBusy = true;

            try
            {
                FException = null;

                ImplicitEntryPointInstanceManager.StepInstances();
            }
            catch (Exception e)
            {
                RaiseOnException(e);
            }
            finally
            {
                OnFrameCompleted(Clocks.FrameClock.TimeDifference);

                Frame++;
                FBusy = false;
            }
        }

        public void Stop()
        {
            foreach (var instance in FInstances)
                instance.Stop();
            ImplicitEntryPointInstanceManager.StopInstances();
        }

        public NodePlugin CreateInstance(Node node, IInternalPluginHost nodeHost, IIORegistry ioRegistry)
        {
            var instance = new NodePlugin(FVlHost, this, node.Identity, nodeHost, ioRegistry);
            FInstances = FInstances.Add(instance);

            var buildResult = instance.Update(Compilation);
            instance.SyncPinsAndRestoreState(buildResult, swapper: null);

            return instance;
        }

        public void DeleteInstance(NodePlugin instance)
        {
            FInstances = FInstances.Remove(instance);
            instance.Dispose();
            //FVlHost.Session.RemoveEntryPoint(instance.NodeId);
            // TODO: Move me back in once dynamic assemblies get indeed unloaded again. For now this reduces the memory usage when creating the nodelist.xml file by quite a bit.
            //var compilation = Platform.LatestCompilation.RemoveEntryPoint(instance.NodeId);
            //Update(compilation);
            //Platform.LatestCompilation = compilation; // Will trigger Platform_LatestCompilationUpdated without any effect
        }

        public Exception LatestException => FException;

        public IObservable<EventPattern<FrameCompletedEventArgs>> Frames => InterlockedHelper.CacheNoLock(ref FFrames,
            () => Observable.FromEventPattern<FrameCompletedEventArgs>(this, nameof(IRuntimeHost.FrameCompleted)));

        public IObservable<EventPattern<TargetCompilationUpdateEventArgs>> Updates => InterlockedHelper.CacheNoLock(ref FUpdates,
            () => Observable.FromEventPattern<TargetCompilationUpdateEventArgs>(this, nameof(IRuntimeHost.Updated)));

        public IObservable<EventPattern<Exception>> Exceptions => InterlockedHelper.CacheNoLock(ref FExceptions,
            () => Observable.FromEventPattern<Exception>(this, nameof(OnException)));

        protected virtual void OnFrameCompleted(double elapsed)
        {
            FrameCompleted?.Invoke(this, new FrameCompletedEventArgs(Frame, TimeSpan.FromSeconds(elapsed)));
        }

        protected virtual void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPaused()
        {
            Paused?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnModeChanged()
        {
            ModeChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void RaiseOnException(Exception e)
        {
            var settings = Settings.Default;
            if (settings.RuntimePauseOnError)
                Mode = RunMode.Paused;
            FException = e;
            OnException?.Invoke(this, e);
        }
    }
}

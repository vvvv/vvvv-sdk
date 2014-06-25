using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VVVV.PluginInterfaces.V2;
using Xilium.CefGlue;

namespace VVVV.Nodes.Texture.HTML
{
    public class HTMLTextureApp : CefApp
    {
        const string CmdAssemblySearchPathSwitch = "assembly-search-path";

        // Main entry point when called by CEF
        public static int Main(string[] args)
        {
            CefRuntime.Load();

            var app = new HTMLTextureApp();
            var mainArgs = new CefMainArgs(args);
            var exitCode = CefRuntime.ExecuteProcess(mainArgs, app, IntPtr.Zero);
            if (exitCode != -1)
                return exitCode;

            CefRuntime.Shutdown();
            return 0;
        }

        private static readonly Dictionary<CefThreadId, CefTaskScheduler> TaskSchedulers = new Dictionary<CefThreadId, CefTaskScheduler>();

        public static TaskScheduler GetTaskScheduler(CefThreadId threadId)
        {
            CefTaskScheduler result;
            if (!TaskSchedulers.TryGetValue(threadId, out result))
            {
                result = new CefTaskScheduler(threadId);
                TaskSchedulers.Add(threadId, result);
            }
            return result;
        }

        class CefTaskScheduler : TaskScheduler
        {
            class TaskWrapper : CefTask
            {
                public readonly CefTaskScheduler Scheduler;
                public readonly Task Task;
                private bool Removed;

                public TaskWrapper(CefTaskScheduler scheduler, Task task)
                {
                    Scheduler = scheduler;
                    Task = task;
                }

                protected override void Execute()
                {
                    Scheduler.TryExecuteTask(Task);
                    lock (Scheduler.Tasks)
                        Removed = Scheduler.Tasks.Remove(this);
                }

                protected override void Dispose(bool disposing)
                {
                    if (!Removed)
                    {
                        lock (Scheduler.Tasks)
                            Scheduler.Tasks.Remove(this);
                    }
                    base.Dispose(disposing);
                }
            }

            private readonly CefThreadId ThreadId;
            private readonly LinkedList<TaskWrapper> Tasks = new LinkedList<TaskWrapper>();

            public CefTaskScheduler(CefThreadId threadId)
            {
                ThreadId = threadId;
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                lock (Tasks)
                {
                    foreach (var taskWrapper in Tasks)
                        yield return taskWrapper.Task;
                }
            }

            protected override void QueueTask(Task task)
            {
                lock (Tasks)
                {
                    var t = new TaskWrapper(this, task);
                    if (CefRuntime.PostTask(ThreadId, t))
                        Tasks.AddLast(t);
                }
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return false;
            }
        }

        class DomVisitor : CefDomVisitor
        {
            public XDocument Result;
            public Exception Exception;

            protected override void Visit(CefDomDocument document)
            {
                using (var xmlReader = new CefXmlReader(document))
                {
                    try
                    {
                        Result = XDocument.Load(xmlReader);
                    }
                    catch (Exception e)
                    {
                        Exception = e;
                    }
                }
            }
        }

        class BrowserProcessHandler : CefBrowserProcessHandler
        {
            protected override void OnRenderProcessThreadCreated(CefListValue extraInfo)
            {
                base.OnRenderProcessThreadCreated(extraInfo);
            }

            protected override void OnBeforeChildProcessLaunch(CefCommandLine commandLine)
            {
                if (!commandLine.HasSwitch(CmdAssemblySearchPathSwitch))
                {
                    var searchPath = Path.GetDirectoryName(typeof(IPluginEvaluate).Assembly.Location);
                    commandLine.AppendSwitch(CmdAssemblySearchPathSwitch, searchPath);
                }
                base.OnBeforeChildProcessLaunch(commandLine);
            }
        }

        class RenderProcessHandler : CefRenderProcessHandler
        {
            protected override void OnRenderThreadCreated(CefListValue extraInfo)
            {
                base.OnRenderThreadCreated(extraInfo);
            }

            protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
            {
                switch (message.Name)
                {
                    case "dom-request":
                        var identifier = message.GetFrameIdentifier();
                        HandleDomRequest(browser, sourceProcess, identifier);
                        return true;
                    default:
                        return base.OnProcessMessageReceived(browser, sourceProcess, message);
                }
            }

            private void HandleDomRequest(CefBrowser browser, CefProcessId sourceProcess, long frameIdentifier)
            {
                var scheduler = GetTaskScheduler(CefThreadId.Renderer);
                Task.Factory.StartNew(
                    () =>
                    {
                        var frame = browser.GetFrame(frameIdentifier);
                        if (frame != null)
                        {
                            var visitor = new DomVisitor();
                            frame.VisitDom(visitor);
                            var response = CefProcessMessage.Create("dom-response");
                            response.SetFrameIdentifier(frame.Identifier);
                            if (visitor.Result != null)
                            {
                                response.Arguments.SetBool(2, true);
                                response.Arguments.SetString(3, visitor.Result.ToString());
                            }
                            else
                            {
                                response.Arguments.SetBool(2, false);
                                response.Arguments.SetString(3, visitor.Exception.ToString());
                            }
                            browser.SendProcessMessage(sourceProcess, response);
                        }
                    },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    scheduler);
            }
        }

        protected override void OnRegisterCustomSchemes(CefSchemeRegistrar registrar)
        {
            registrar.AddCustomScheme(SchemeHandlerFactory.SCHEME_NAME, false, true, false);
            base.OnRegisterCustomSchemes(registrar);
        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            if (commandLine.HasSwitch(CmdAssemblySearchPathSwitch))
            {
                var searchPath = commandLine.GetSwitchValue(CmdAssemblySearchPathSwitch);
                AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveEventArgs) =>
                {
                    var assemblyName = new AssemblyName(resolveEventArgs.Name);
                    var assemblyFileName = assemblyName.Name + ".dll";
                    var assemblyLocation = Path.Combine(searchPath, assemblyFileName);
                    if (File.Exists(assemblyLocation))
                        return Assembly.LoadFrom(assemblyLocation);
                    return null;
                };

            }
            base.OnBeforeCommandLineProcessing(processType, commandLine);
        }

        protected override CefBrowserProcessHandler GetBrowserProcessHandler()
        {
            return new BrowserProcessHandler();
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return new RenderProcessHandler();
        }
    }
}

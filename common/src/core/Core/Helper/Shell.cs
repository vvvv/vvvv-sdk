using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using Microsoft.Practices.Unity;

using VVVV.Core.Commands;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.Core.Model;
using VVVV.Core.Logging;
using VVVV.Utils.Network;
using System.Collections.Generic;
using System.Threading;

namespace VVVV.Core
{
    /// <summary>
    /// A shell should be created once on startup of an application.
    /// It sets up and provides access to a CompositionContainer (MEF)
    /// and a UnityContainer.
    /// TODO: Use http://mefcontrib.codeplex.com to sync Unity and MEF.
    /// </summary>
    public class Shell : MarshalByRefObject
    {
        private static string FSCallerPath = null;
        
        /// <summary>
        /// The absolute path to the directory of this assembly.
        /// </summary>
        public static string CallerPath
        {
            get
            {
                if (FSCallerPath == null)
                {
                    var currentAssembly = Assembly.GetCallingAssembly();
                    FSCallerPath = Path.GetDirectoryName(currentAssembly.Location);
                }
                
                return FSCallerPath;
            }
        }
        
        private static string FTempPath = Path.GetTempPath().ConcatPath("vvvv");
        
        /// <summary>
        /// The temporary path used by this application.
        /// </summary>
        public static string TempPath
        {
        	get
        	{
        		return FTempPath;
        	}
        }
        
        private static Shell FInstance;
        public static Shell Instance
        {
            get
            {
            	if (FInstance == null)
            		return Initialize();
            	return FInstance;
            }
        }

        public CompositionContainer Container { get; private set; }
        
        public CommandLineArguments CommandLineArguments { get; private set; }
        
        [Export(typeof(ILogger))]
        public DefaultLogger Logger { get; private set; }
        
        //the solution
        [Export]
        private Solution FSolution;
        public Solution Solution 
        { 
            get
            {
                return FSolution;
            }
            set
            {
                FSolution = value;
                Root = value;
            }
        }

        private IIDContainer FRoot;
        public IIDContainer Root
        {
            get
            {
                return FRoot;
            }
            set
            {
                FRoot = value;
            }
        }

        //port and remoting manager
        private static int FPort = 3344;
        private static RemotingManagerTCP FRemoter = new RemotingManagerTCP();

        private Shell()
        {
        	if (!Directory.Exists(FTempPath))
        		Directory.CreateDirectory(TempPath);
        	
            CommandLineArguments = new CommandLineArguments();
            Logger = new DefaultLogger();
            
            Compose();

            CommandLineArguments.Parse();
        }
        
        public static Shell Initialize()
        {
        	if (FInstance == null)
            	FInstance = new Shell();
            return FInstance;
        }
        
        private void Compose()
        {
//            var catalog = new AggregateCatalog();
//            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Shell).Assembly));
//            catalog.Catalogs.Add(new DirectoryCatalog(CallerPath));
            
            Container = new CompositionContainer();
            
            try
            {
            	Container.ComposeParts(this);
            }
            catch (Exception e)
            {
            	Logger.Log(e);
            }
        }

        // find IIDItem of a specific ID
        public IIDItem GetIDItem(string id)
        {
            IEnumerable<string> pathParts = id.Split('/');
            if (pathParts.First() == Root.Name)
                return Root.GetIDItem(pathParts.Skip(1), new List<string>());
            else
                throw new Exception("can only resolve paths that start at solution");
        }

        // map to T at id item
        public T GetAtID<T>(string id)
        {
            var item = GetIDItem(id);
            return (T)item.ServiceProvider.GetService(typeof(T));
        }

        //publish an object
        public static void PublishObject(string objectName, MarshalByRefObject obj)
        {
            var channelName = "ShellChannel";

            if (!RemotingUtils.ChannelExists(channelName))
            {
                FRemoter.InitializeServerChannel(channelName, FPort, false);
                Console.WriteLine("Channel crated: " + channelName);
            }

            FRemoter.PublishObject(obj, objectName);
            Console.WriteLine("Published object: " + objectName);
        }

        public bool IsRuntime { get; set; }

        public SynchronizationContext MainThread { get; set; }
    }
}

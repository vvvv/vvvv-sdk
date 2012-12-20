﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using VVVV.PluginInterfaces.V2;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using VVVV.Core.Logging;
using VVVV.Core;
using System.ComponentModel.Composition.Hosting;

namespace VVVV.Hosting.Factories
{
    [ComVisible(false)]
    public class StartableInfo
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }

    [ComVisible(false)]
    public class StartableStatus : IStartableStatus
    {
        public IStartable Startable { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public StartableInfo Info { get; set; }


        public string Name
        {
            get { return Info.Name;  }
        }

        public string TypeName
        {
            get { return Info.Type.FullName; }
        }
    }

    [Export(typeof(IStartableRegistry))]
    [Export(typeof(StartableRegistry))]
    [ComVisible(false)]
    public class StartableRegistry : IStartableRegistry
    {
        class StartableImporter
        {
            [Import(typeof(IStartable), AllowRecomposition = true)]
            public ExportFactory<IStartable> StartableExportFactory { get; set; }
        }

        class StartableInterfaces : List<StartableInfo> { }

        class StartableCache : Dictionary<string, StartableInterfaces> { }

        private Type FReflectionOnlyStartableType;

        private StartableCache FStartable = new StartableCache();

        private List<string> FProcessedAssemblies = new List<string>();

        private List<IStartableStatus> FStarted = new List<IStartableStatus>();

        private readonly CompositionContainer FParentContainer;

        private StartableImporter FStartableImporter = new StartableImporter();

        List<IStartableStatus> IStartableRegistry.Status
        {
            get { return FStarted; }
        }

#pragma warning disable 0649
        [Import]
        ILogger FLogger; 
#pragma warning restore

        [ImportingConstructor]
        public StartableRegistry(CompositionContainer parentContainer)
        {
            FParentContainer = parentContainer;

            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += HandleReflectionOnlyAssemblyResolve;

            var pluginInterfacesAssemblyName = typeof(IStartable).Assembly.FullName;
            var pluginInterfacesAssembly = Assembly.ReflectionOnlyLoad(pluginInterfacesAssemblyName);
            FReflectionOnlyStartableType = pluginInterfacesAssembly.GetExportedTypes().Where(t => t.Name == typeof(IStartable).Name).First();
        }

        public bool ProcessType(Type type, Assembly assembly)
        {
            //Already scanned
            if (FStartable.ContainsKey(assembly.FullName)) { return false; }

            bool nonLazyStartable = false;

            if (!type.IsAbstract && !type.IsGenericTypeDefinition && FReflectionOnlyStartableType.IsAssignableFrom(type))
            {
                var attribute = GetStartableAttributeData(type);

                if (attribute != null)
                {
                    var namedArguments = new Dictionary<string, object>();
                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        namedArguments[namedArgument.MemberInfo.Name] = namedArgument.TypedValue.Value;
                    }

                    string name = type.FullName;
                    if (namedArguments.ContainsKey("Name"))
                    {
                        name = namedArguments["Name"].ToString();
                    }

                    bool lazy = true;
                    if (namedArguments.ContainsKey("Lazy"))
                    {
                        try
                        {
                            lazy = bool.Parse(namedArguments["Lazy"].ToString());
                        }
                        catch
                        {
                            FLogger.Log(LogType.Warning, "Startable :" + name + " Lazy attribute not found. Defaulting to true");
                            //Lazy by default
                            lazy = true;
                        }
                    }



                    AddToCache(assembly, type, name);

                    if (!lazy) { nonLazyStartable = true; }
                }
            }
            return nonLazyStartable;
        }

        public void ProcessAssembly(Assembly assembly)
        {
            if (FStartable.ContainsKey(assembly.FullName))
            {
                if (!FProcessedAssemblies.Contains(assembly.FullName))
                {
                    foreach (StartableInfo info in FStartable[assembly.FullName])
                    {
                        StartableStatus s = new StartableStatus();
                        s.Info = info;

                        try
                        {
                            Type realtype = assembly.GetType(info.Type.FullName);
                            var catalog = new TypeCatalog(realtype);
                            var exportProviders = new ExportProvider[] { FParentContainer };
                            var container = new CompositionContainer(catalog, exportProviders);
                            container.ComposeParts(FStartableImporter);

                            var lifetimeContext = FStartableImporter.StartableExportFactory.CreateExport();

                            s.Startable = lifetimeContext.Value;
                            s.Startable.Start();
                            s.Success = true;
                            s.Message = "OK";

                        }
                        catch (Exception ex)
                        {
                            FLogger.Log(ex);
                            s.Startable = null;
                            s.Success = false;
                            s.Message = ex.Message;
                        }

                        this.FStarted.Add(s);
                    }
                    FProcessedAssemblies.Add(assembly.FullName);
                }
            }
        }

        public bool ContainsStartable(Assembly assembly)
        {
            return FStartable.ContainsKey(assembly.FullName);
        }

        public void ShutDown()
        {
            foreach (StartableStatus s in this.FStarted)
            {
                try
                {
                    if (s.Startable != null)
                    {
                        s.Startable.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    FLogger.Log(ex);
                }
            }
            //Shouldnt be needed, but clears references;
            this.FStarted.Clear();
        }

        private static CustomAttributeData GetStartableAttributeData(Type type)
        {
            var attributes = CustomAttributeData.GetCustomAttributes(type).Where(ca => ca.Constructor.DeclaringType.FullName == typeof(StartableAttribute).FullName).ToArray();
            return attributes.Length > 0 ? attributes[0] : null;
        }

        public void AddFromXml(string startablelist)
        {
            if (!File.Exists(startablelist)) { return; } //Early exit

            //Precache files, so we also make sure assembly will load once
            StartableCache cache = new StartableCache();

            Dictionary<string, bool> lazylist = new Dictionary<string, bool>();

            //Store base directory
            var baseDir = Path.GetDirectoryName(startablelist);

            #region Load File
            using (var streamReader = new StreamReader(startablelist))
            {
                var settings = new XmlReaderSettings();
                settings.ProhibitDtd = false;

                using (var xmlReader = XmlReader.Create(streamReader, settings))
                {
                    //Scans all startable elements
                    while (xmlReader.ReadToFollowing("STARTABLE"))
                    {
                        var name = xmlReader.GetAttribute("name");
                        var filename = xmlReader.GetAttribute("filename");
                        var lazy = xmlReader.GetAttribute("filename") == "true";
                        var typename = xmlReader.GetAttribute("type");

                        try 
                        {
                            //Load just to check if valid assembly, we also need signature
                            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(Path.Combine(baseDir, filename));
                            Type type = assembly.GetType(typename);

                            AddToCache(assembly, type, name);

                            if (!lazylist.ContainsKey(assembly.FullName))
                            {
                                //Lazy by default
                                lazylist.Add(assembly.FullName, true);
                            }

                            /* Mark assembly as non lazy, if one interface in there
                             * is non lazy, assembly needs to be loaded even if there is lazy interfaces */
                            if (!lazy) { lazylist[assembly.FullName] = false; } 
                        } 
                        catch 
                        {
                            //Invalid Assembly name
                        } 
                    }
                }
            }
            #endregion

            //Ok now we finished caching, so lookup each assembly that we need to start automatically
            foreach (string key in lazylist.Keys)
            {
                if (!lazylist[key])
                {
                    var assemblyload = Assembly.LoadFrom(key);
                    ProcessAssembly(assemblyload);
                }
            }
        }

        #region Add Startable type to cache
        private void AddToCache(Assembly assembly, Type type, string name)
        {
            StartableInterfaces infos;
            if (!FStartable.ContainsKey(assembly.FullName))
            {
                infos = new StartableInterfaces();
                FStartable[assembly.FullName] = infos;
            }
            else
            {
                infos = FStartable[assembly.FullName];
            }

            StartableInfo info = new StartableInfo();
            info.Name = name;
            info.Type = type;

            infos.Add(info);
        }
        #endregion

        private Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string fullName = args.Name.Trim();
            string partialName = GetPartialAssemblyName(fullName);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AssemblyName name = assembly.GetName();
                if (name.Name == partialName)
                    return assembly;
            }

            return null;
        }

        private Assembly HandleReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string fullName = args.Name.Trim();
            string partialName = GetPartialAssemblyName(fullName);

            foreach (Assembly assembly in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
            {
                AssemblyName name = assembly.GetName();
                if (name.Name == partialName)
                    return assembly;
            }

            string fileName = partialName + ".dll";
            //string path = Path.Combine(FCurrentAssemblyDir, fileName);
           // if (File.Exists(path))
            //{
           //     return Assembly.ReflectionOnlyLoadFrom(path);
           // }
            string path = Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), fileName);
            if (File.Exists(path))
            {
                return Assembly.ReflectionOnlyLoadFrom(path);
            }

            path = Path.Combine(Shell.CallerPath, fileName);
            if (File.Exists(path))
            {
                return Assembly.ReflectionOnlyLoadFrom(path);
            }

            return Assembly.ReflectionOnlyLoad(fullName);
        }

        private static string GetPartialAssemblyName(string fullName)
        {
            fullName = fullName.Trim();
            if (fullName.IndexOf(',') >= 0)
                return ReplaceLegacyAssemblyNames(fullName.Substring(0, fullName.IndexOf(',')));
            else
                return ReplaceLegacyAssemblyNames(fullName);
        }

        private static string ReplaceLegacyAssemblyNames(string partialName)
        {
            switch (partialName)
            {
                case "_PluginInterfaces":
                case "PluginInterfaces":
                    return "VVVV.PluginInterfaces";
                case "_Utils":
                    return "VVVV.Utils";
                default:
                    return partialName;
            }
        }

    }
}
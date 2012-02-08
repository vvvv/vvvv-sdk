﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using VVVV.PluginInterfaces.V2;
using System.Diagnostics;

namespace VVVV.Hosting.Factories
{
    public class StartableInfo
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }

    public class StartableStatus
    {
        public IStartable Startable { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public StartableInfo Info { get; set; }
    }

    public class StartableRegistry
    {
        class StartableInterfaces : List<StartableInfo> { }

        class StartableCache : Dictionary<string, StartableInterfaces> { }

        private Type FReflectionOnlyStartableType;

        private StartableCache FStartable = new StartableCache();

        private List<string> FProcessedAssemblies = new List<string>();

        private List<StartableStatus> FStarted = new List<StartableStatus>();

        public List<StartableStatus> Status
        {
            get { return FStarted; }
        }

        public StartableRegistry(Assembly pluginInterfacesAssembly)
        {
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
                    info.Type = type;

                    bool lazy = true;
                    if (namedArguments.ContainsKey("Lazy"))
                    {
                        try
                        {
                            lazy = bool.Parse(namedArguments["Lazy"].ToString());
                        }
                        catch
                        {
                            //Lazy by default
                            lazy = true;
                        }
                    }

                    if (namedArguments.ContainsKey("Name"))
                    {
                        info.Name = namedArguments["Name"].ToString();
                    }
                    else
                    {
                        info.Name = type.FullName;
                    }

                    infos.Add(info);

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
                        object o = assembly.CreateInstance(info.Type.FullName);

                        StartableStatus s = new StartableStatus();
                        s.Startable = (IStartable)o;
                        s.Info = info;

                        try
                        {
                            s.Startable.Start();
                            s.Success = true;
                            s.Message = "OK";
                        }
                        catch (Exception ex)
                        {
                            s.Success = false;
                            s.Message = ex.Message;
                        }

                        this.FStarted.Add(s);
                    }
                    FProcessedAssemblies.Add(assembly.FullName);
                }
            }
        }

        public void ShutDown()
        {
            foreach (StartableStatus s in this.FStarted)
            {
                try
                {
                    s.Startable.Shutdown();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to stop: " + ex.Message);
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



    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
    public class StartableRegistry
    {

        class StartableInterfaces : List<Type> { }

        class StartableCache : Dictionary<string, StartableInterfaces> { }

        private Type FReflectionOnlyStartableType;

        private StartableCache FStartable = new StartableCache();

        private List<string> FProcessedAssemblies = new List<string>();

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

                    StartableInterfaces ifaces;
                    if (!FStartable.ContainsKey(assembly.FullName))
                    {
                        ifaces = new StartableInterfaces();
                        FStartable[assembly.FullName] = ifaces;
                    }
                    else
                    {
                        ifaces = FStartable[assembly.FullName];
                    }

                    ifaces.Add(type);

                    bool lazy = true;
                    if (namedArguments.ContainsKey("Lazy"))
                    {
                        lazy = bool.Parse(namedArguments["Lazy"].ToString());
                    }
                    //bool lazy = (bool)namedArguments("Lazy");
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
                    foreach (Type t in FStartable[assembly.FullName])
                    {
                        object startable = assembly.CreateInstance(t.FullName);
                        ((IStartable)startable).Start();
                    }
                    FProcessedAssemblies.Add(assembly.FullName);
                }
            }
        }

        private static CustomAttributeData GetStartableAttributeData(Type type)
        {
            var attributes = CustomAttributeData.GetCustomAttributes(type).Where(ca => ca.Constructor.DeclaringType.FullName == typeof(StartableAttribute).FullName).ToArray();
            return attributes.Length > 0 ? attributes[0] : null;
        }



    }
}
using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Nodes.Finder
{
    public class NodeFilter
    {
        public static bool IsGlobalSearchScope(string query)
        {
            var tags = ParseQuery(query);
            return tags.Contains("<");
        }
        
        static List<string> ParseQuery(string query)
        {
            query += (char) 160;
            var tags = query.Split(new char[1]{' '}).ToList();
            for (int i = tags.Count-1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(tags[i].Trim()))
                    tags.RemoveAt(i);
            }
            
            return tags;
        }
        
        public NodeView UpdateFilter(string query, INode2 startNode, bool openModules)
        {
            // Parse query
            Tags = ParseQuery(query);

            // Set filter scope
            if (Tags.Contains("<"))
            {
                MinLevel = int.MinValue;
                MaxLevel = int.MaxValue;
                Tags.Remove("<");
            }
            else if (Tags.Contains(">"))
            {
                MinLevel = 0;
                MaxLevel = int.MaxValue;
                Tags.Remove(">");
            }
            else
            {
                MinLevel = 0;
                MaxLevel = 1;
            }
            
            // Set filter flags which control what to search
            Flags = FilterFlags.None;
            if (Tags.Contains("s"))
            {
                Flags |= FilterFlags.Send;
                Flags |= FilterFlags.Receive;
                Tags.Remove("s");
            }
            if (Tags.Contains("/"))
            {
                Flags |= FilterFlags.Comment;
                Tags.Remove("/");
            }
            if (Tags.Contains("x"))
            {
                Flags |= FilterFlags.Effect;
                Tags.Remove("x");
            }
            if (Tags.Contains("f"))
            {
                Flags |= FilterFlags.Freeframe;
                Tags.Remove("f");
            }
            if (Tags.Contains("m"))
            {
                Flags |= FilterFlags.Module;
                Tags.Remove("m");
            }
            if (Tags.Contains("p"))
            {
                Flags |= FilterFlags.Plugin;
                Tags.Remove("p");
            }
            if (Tags.Contains("d"))
            {
                Flags |= FilterFlags.Dynamic;
                Tags.Remove("d");
            }
            if (Tags.Contains("l"))
            {
                Flags |= FilterFlags.VL;
                Tags.Remove("l");
            }
            if (Tags.Contains("i"))
            {
                Flags |= FilterFlags.IONode;
                Tags.Remove("i");
            }
            if (Tags.Contains("e"))
            {
                Flags |= FilterFlags.Exposed;
                Tags.Remove("e");
            }
            if (Tags.Contains("n"))
            {
                Flags |= FilterFlags.Native;
                Tags.Remove("n");
            }
            if (Tags.Contains("a"))
            {
                Flags |= FilterFlags.VST;
                Tags.Remove("a");
            }
            if (Tags.Contains("t"))
            {
                Flags |= FilterFlags.Patch;
                Tags.Remove("t");
            }
            if (Tags.Contains("r"))
            {
                Flags |= FilterFlags.Unknown;
                Tags.Remove("r");
            }
            if (Tags.Contains("v"))
            {
                Flags |= FilterFlags.VL;
                Tags.Remove("v");
            }
            if (Tags.Contains("b"))
            {
                Flags |= FilterFlags.Boygrouped;
                Tags.Remove("b");
            }
            if (Tags.Contains("w"))
            {
                Flags |= FilterFlags.Window;
                Tags.Remove("w");
            }
            
            // If nothing set look for all kind of nodes
            if (Flags == FilterFlags.None)
                Flags = FilterFlags.AllNodeTypes;
            
            // Set filter tags which control where to search
            var wFlags = FilterFlags.None;
            if (Tags.Contains("l"))
            {
                wFlags |= FilterFlags.Label;
                Tags.Remove("l");
            }
            if (Tags.Contains("#"))
            {
                wFlags |= FilterFlags.ID;
                Tags.Remove("#");
            }
            
            // If nothing set search in node name
            if (wFlags == FilterFlags.None)
                wFlags = FilterFlags.Name;
            
            Flags |= wFlags;
            
            // Set filter tags
            for (int i = 0; i < Tags.Count; i++)
                Tags[i] = Tags[i].Trim((char) 160);
            
            return new NodeView(null, startNode, this, 0, openModules);
        }
        
        public int MinLevel
        {
            get;
            private set;
        }
        
        public int MaxLevel
        {
            get;
            private set;
        }
        
        public bool ScopeIsGlobal
        {
            get
            {
                return MinLevel == int.MinValue && MaxLevel == int.MaxValue;
            }
        }
        
        public bool ScopeIsLocal
        {
            get
            {
                return MinLevel == 0 && MaxLevel == 1;
            }
        }
        
        public FilterFlags Flags
        {
            get;
            private set;
        }
        
        public List<string> Tags
        {
            get;
            private set;
        }
    }
}

using System;

namespace VVVV.Nodes.Finder
{
    [Flags]
    public enum FilterFlags
    {
        None = 0x0,
        Send = 0x1,
        Comment = 0x2,
        Label = 0x4,
        Effect = 0x8,
        Freeframe = 0x10,
        Module = 0x20,
        Plugin = 0x40,
        IONode = 0x80,
        Native = 0x100,
        VST = 0x200,
        Patch = 0x400,
        Unknown = 0x800,
        Boygrouped = 0x1000,
        Name = 0x2000,
        Window = 0x4000,
        ID = 0x8000,
        Dynamic = 0x10000,
        Text = 0x20000,
        Receive = 0x40000,
        Exposed = 0x80000,
        VL = 0x100000,
        Addon = Effect | Freeframe | Module | Plugin | VST | Dynamic | VL,
        AllNodeTypes = Addon | Send | Comment | IONode | Native | Patch | Unknown | Boygrouped | Window | Dynamic | Text | Receive | Exposed | VL
    }
}

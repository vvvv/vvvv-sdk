using System;
using System.IO;
using VVVV.Core.Model;

namespace VVVV.Core.Model.FX
{
    public class FXDocument : TextDocument
    {
        public FXDocument(string name, Uri location)
            : base(name, location)
        {
        }
        
        public override bool CanRenameTo(string value)
        {
        	return base.CanRenameTo(value) && (Path.GetExtension(value) == ".fx" || Path.GetExtension(value) == ".fxh");
        }
    }
}

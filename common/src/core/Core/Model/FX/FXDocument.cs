using System;
using System.IO;
using VVVV.Core.Model;

namespace VVVV.Core.Model.FX
{
    public class FXDocument : TextDocument
    {
        public FXDocument(string name, string path)
            : base(name, path)
        {
        }
        
        public override bool CanRenameTo(string value)
        {
        	return base.CanRenameTo(value) && (Path.GetExtension(value) == ".fx" || Path.GetExtension(value) == ".fxh");
        }

        public override void SaveTo(string path)
        {
            base.SaveTo(path);
            if (Saved != null)
                Saved(this, EventArgs.Empty);
        }

        public event EventHandler Saved;
    }
}

using System;
using System.Collections;

using VVVV.Core;

namespace VVVV.Core.View
{
    public class DefaultNameProvider : INamed, IDisposable
    {
    	private INamed FNamedModel;
    	
        public DefaultNameProvider(ModelMapper mapper)
        {
            var model = mapper.Model;
            
            // best option to provide a name is to be able to look it up on the model through interface INamed
            FNamedModel = model as INamed;
            if (FNamedModel != null)
            {
                Name = FNamedModel.Name;
                FNamedModel.Renamed += DefaultNameProvider_Renamed;
            }
            else
            {
                bool resolved = false;

                if (model is IIDItem)
                {
                    var parent = ((IIDItem) model).Owner;
                    
                    // Find my name through property of the parent model object if accesible
                    if (parent != null)
                        Name = PropertyName(parent, model, ref resolved);
                }

                if (!resolved)
                    // default .NET string representation of the model object
                    Name = model.ToString();
            }                   
        }

        public void Dispose()
        {
        	if (FNamedModel != null)
        		FNamedModel.Renamed -= DefaultNameProvider_Renamed;
        }

        public void DefaultNameProvider_Renamed(INamed sender, string newName)
        {
            Name = newName;
        }
 
        public event RenamedHandler Renamed;

        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null)
                Renamed(this, newName);
        }

        protected string FName;
        public string Name
        {
            get
            {
                return FName;
            }
            set
            {
                OnRenamed(value);
                FName = value;
            }
        }

        protected string PropertyName(object obj, object model, ref bool resolved)
        {
            foreach (var property in (obj.GetType().GetProperties()))
            {
                if ((property.GetIndexParameters().Length == 0) && (property.CanRead))
                {
                    var propertyValue = property.GetValue(obj, null);
                    if ((propertyValue == model) || (propertyValue == this))
                    {
                        resolved = true;
                        return property.Name;   
                    }
                }
            }
            
            resolved = false;
            return "not found";
        }
    }

}

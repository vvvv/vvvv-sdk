using System;
using System.Collections;
using VVVV.Core.Collections.Sync;


namespace VVVV.Core
{
    /// <summary>
    /// a ModelMap is a synced map of your model 
    /// register it as a default enumerable provider to get a deep map of your model
    /// </summary>
    public class ModelMap : IEnumerable
    {
        public IList Childs
        {
            get;
            protected set;
        }

        public ModelMapper ModelMapper
        {
            get;
            protected set;
        }

        public object Model
        {
            get;
            protected set;
        }

        public ModelMap(ModelMapper mapper) :
            this(mapper, Empty.List)
        {
            if (mapper.Model is IEnumerable)
            {
                Childs = new ArrayList();

                // sync the new childs list with the model enumerable
                // and create child model maps for each child. 
                // by default another ModelMap instance will be created (when registered in registry)
                Childs.SyncWith(mapper.Model as IEnumerable, child => mapper.CreateChildMapper(child).Map<IEnumerable>());
            }
        }

        protected ModelMap(ModelMapper mapper, IList childs)
        {
            ModelMapper = mapper;
            Model = ModelMapper.Model;
            Childs = childs;
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return Childs.GetEnumerator();
        }

        #endregion
    }



    public class ModelMapLeave : ModelMap
    {
        public ModelMapLeave(ModelMapper mapper) 
            : base(mapper, Empty.List)
        {            
        }
    }
}

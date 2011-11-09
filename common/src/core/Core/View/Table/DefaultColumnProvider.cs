using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.Core.Model;

namespace VVVV.Core.View.Table
{
    /// <summary>
    /// Retrieves column information from first returned element.
    /// </summary>
    public class DefaultColumnProvider : IEnumerable<Column>
    {
        protected IEnumerable FEnumerable;
        
        public DefaultColumnProvider(ModelMapper mapper)
        {
            if (mapper.CanMap<IEnumerable>())
                FEnumerable = mapper.Map<IEnumerable>();
            else
                FEnumerable = Empty.Enumerable;
        }
        
        public IEnumerator<Column> GetEnumerator()
        {
            var enumerator = FEnumerable.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                
                if (item is IDContainer)
                {
                    var idContainer = item as IDContainer;

                    foreach (var idItem in idContainer) {
                        if (idItem is IViewableProperty<string>)
                        {
                            var property = idItem as IViewableProperty<string>;
                            yield return new Column(property.Name);
                        }
                    }                    
                }
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

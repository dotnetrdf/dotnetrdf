using System;
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public abstract class WrapperEnumerable<T> 
        : IEnumerable<T>
    {
        protected WrapperEnumerable(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            this.InnerEnumerable = enumerable;
        }
        
        protected IEnumerable<T> InnerEnumerable { get; private set; }

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
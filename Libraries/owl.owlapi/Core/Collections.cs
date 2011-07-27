using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class BaseCollection<T> : IEnumerable<T>
    {
        protected internal abstract void Add(T item);

        protected internal abstract void Delete(T item);

        public abstract int Count
        {
            get;
        }

        public abstract IEnumerator<T> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }

    public abstract class BaseAxiomCollection : BaseCollection<IAxiom>
    {
        public event AxiomEventHandler AxiomAdded;

        public event AxiomEventHandler AxiomRemoved;
    }

    public abstract class BaseAnnotationCollection : BaseCollection<IAnnotation>
    {
        public event AnnotationEventHandler AnnotationAdded;

        public event AnnotationEventHandler AnnotationRemoved;
    }
}

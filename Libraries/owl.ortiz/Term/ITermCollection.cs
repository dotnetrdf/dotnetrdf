using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term
{
    public interface ITermCollection<T>
        : IEnumerable<T>
        where T : ITerm
    {
        bool Contains(T t);

        bool IsEmpty
        {
            get;
        }

        int Count
        {
            get;
        }
    }

    public interface ITermList<T>
        : ITermCollection<T>
        where T : ITerm
    {

    }

    public interface ITermSet<T>
        : ITermCollection<T>
        where T : ITerm
    {

    }
}

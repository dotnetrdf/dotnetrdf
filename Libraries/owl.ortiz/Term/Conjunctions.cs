using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term
{
    public interface IAnd<T>
        : ITermSet<T>
        where T : ITerm
    {
    }

    public interface IOr<T>
        : ITermSet<T>
        where T : ITerm
    {
    }

    public interface ISubsumption<TSub,TSuper>
        where TSub : ITerm
        where TSuper : ITerm
    {
        TSub Sub
        {
            get;
        }

        TSuper Super
        {
            get;
        }
    }

    public interface INot<T>
        where T : ITerm
    {
        T Argument
        {
            get;
        }
    }
}

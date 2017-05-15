using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;
using VDS.OWL.Term.Query;

namespace VDS.OWL.Term.Axiom
{
    public interface IAnnotation<T> 
        : IAxiom, IAtomicQueryAtom
        where T : ITerm
    {
        IAnnotationValue Object
        {
            get;
        }

        IAnnotationProperty Property
        {
            get;
        }

        T Subject
        {
            get;
        }
    }
}

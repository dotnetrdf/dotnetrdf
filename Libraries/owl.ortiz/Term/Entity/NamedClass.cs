using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface INamedClass
        : INamedTerm, IClassExpression, INamedEntity
    {
    }

    public static class NamedClasses
    {
        public static INamedClass Bottom
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedClass Top
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface INamedDataProperty
        : INamedTerm, INamedProperty, IDataProperty
    {
    }

    public static class NamedDataProperties
    {
        public static INamedDataProperty Bottom
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedDataProperty Top
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

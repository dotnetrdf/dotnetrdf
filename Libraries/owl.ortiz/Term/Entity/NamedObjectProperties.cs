using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface INamedObjectProperty
        : INamedProperty, IObjectProperty
    {

    }

    public static class NamedObjectProperties
    {
        public static INamedObjectProperty Bottom
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static INamedObjectProperty Top
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

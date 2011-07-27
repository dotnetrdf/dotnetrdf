 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public interface IEntity
    {
        Iri EntityIri
        {
            get;
        }
    }

    public abstract class BaseEntity : IEntity
    {
        public Iri EntityIri { get; protected set; }
    }
}

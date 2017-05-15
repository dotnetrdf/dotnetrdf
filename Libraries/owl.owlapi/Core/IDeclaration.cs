using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public interface IDeclaration : IAxiom
    {
        IEntity Entity
        {
            get;
        }
    }

    public class Declaration : Axiom, IDeclaration
    {
        public IEntity Entity { get; protected set; }
    }
}

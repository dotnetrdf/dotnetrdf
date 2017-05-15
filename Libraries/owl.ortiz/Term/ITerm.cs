using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Visitor;

namespace VDS.OWL.Term
{
    public interface ITerm
    {
        T Accept<T>(ITermVisitor<T> visitor) where T : ITerm;

        void Accept(ITermVisitorVoid visitor);

        bool IsNamed
        {
            get;
        }
    }
}

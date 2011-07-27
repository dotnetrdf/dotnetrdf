using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term
{
    public interface ITermVisitor<T>
        where T : ITerm
    {
    }

    public interface ITermVisitorVoid
    {

    }
}

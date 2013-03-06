using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing.Formatting
{
    public interface IQuadFormatter
    {
        String Format(Quad q);
    }
}

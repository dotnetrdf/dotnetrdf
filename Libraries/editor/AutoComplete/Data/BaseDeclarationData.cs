using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public abstract class BaseDeclarationData : BaseCompletionData
    {
        public BaseDeclarationData(String prefix, String postfix)
            : base("<New Base URI Declaration>", prefix + "<Enter Base URI here>" + postfix, "Inserts a new Base URI declaration") { }
    }

    public class TurtleStyleBaseDeclarationData : BaseDeclarationData
    {
        public TurtleStyleBaseDeclarationData()
            : base("@base ", ".") { }
    }

    public class SparqlStyleBaseDeclarationData : BaseDeclarationData
    {
        public SparqlStyleBaseDeclarationData()
            : base("BASE", String.Empty) { }
    }
}

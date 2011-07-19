using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public abstract class BaseDefaultPrefixDeclarationData : BaseCompletionData
    {
        public BaseDefaultPrefixDeclarationData(String prefix, String postfix)
            : base("<New Default Prefix Declaration>", prefix + ": <Enter Default Namespace URI here>" + postfix, "Inserts a new Default Prefix declaration", 100.0d) { }
    }

    public class TurtleStyleDefaultPrefixDeclarationData : BaseDefaultPrefixDeclarationData
    {
        public TurtleStyleDefaultPrefixDeclarationData()
            : base("@prefix ", ".") { }
    }

    public class SparqlStyleDefaultPrefixDeclarationData : BaseDefaultPrefixDeclarationData
    {
        public SparqlStyleDefaultPrefixDeclarationData()
            : base("PREFIX ", String.Empty) { }
    }
}

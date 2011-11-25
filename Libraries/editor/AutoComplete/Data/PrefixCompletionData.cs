using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public abstract class BasePrefixDeclarationData : BaseCompletionData
    {
        public BasePrefixDeclarationData(String nsPrefix, String nsUri, String prefix, String postfix)
            : base(prefix + nsPrefix + ": <" + nsUri + ">" + postfix, prefix + nsPrefix + ": <" + nsUri + ">" + postfix, "Inserts a prefix declaration for the " + nsPrefix + " prefix which has a namespace URI of " + nsUri) { }
    }

    public class TurtleStylePrefixDeclarationData : BasePrefixDeclarationData
    {
        public TurtleStylePrefixDeclarationData(String nsPrefix, String nsUri)
            : base(nsPrefix, nsUri, "@prefix ", ".") { }
    }

    public class SparqlStylePrefixDeclarationData : BasePrefixDeclarationData
    {
        public SparqlStylePrefixDeclarationData(String nsPrefix, String nsUri)
            : base(nsPrefix, nsUri, "PREFIX ", String.Empty) { }
    }
}

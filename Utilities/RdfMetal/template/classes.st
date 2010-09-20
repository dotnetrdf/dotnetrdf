using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using VDS.RDF.Linq;
$refs:namespaceref()$

namespace $opts.dotnetNamespace$
{
[assembly: Ontology(
    BaseUri = "$uri$",
    Name = "$handle$",
    Prefix = "$handle$",
    UrlOfOntology = "$uri$")]


    public partial class $handle$DataContext : RdfDataContext
    {
        public $handle$DataContext(LinqTripleStore store) : base(store)
        {
        }
        public $handle$DataContext(string store) : base(new LinqTripleStore(store))
        {
        }

		$classes:query()$
    }

$classes:class()$

}
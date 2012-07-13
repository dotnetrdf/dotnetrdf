using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning.Sesame.Owlim
{
    /// <summary>
    /// 
    /// </summary>
    /// <para>
    /// </para>
    /// <pre>
    /// @prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
    /// @prefix rep: <http://www.openrdf.org/config/repository#>.
    /// @prefix sr: <http://www.openrdf.org/config/repository/sail#>.
    /// @prefix sail: <http://www.openrdf.org/config/sail#>.
    /// @prefix owlim: <http://www.ontotext.com/trree/owlim#>.
    /// 
    /// [] a rep:Repository ;
       /// rep:repositoryID "owlim" ;
    ///    rdfs:label "OWLIM Getting Started" ;
    ///    rep:repositoryImpl [
    ///      rep:repositoryType "openrdf:SailRepository" ;
    ///      sr:sailImpl [
    ///        sail:sailType "swiftowlim:Sail" ;
    ///        owlim:ruleset "owl-max" ;
    ///        owlim:partialRDFS  "true" ;
    ///        owlim:noPersist "true" ;
    ///        owlim:storage-folder "junit-storage" ;
    ///        owlim:base-URL "http://example.org#" ;
    ///        owlim:new-triples-file "new-triples-file.nt" ;
    ///        owlim:entity-index-size "200000" ;
    ///        owlim:jobsize "200" ;
    ///        owlim:repository-type "in-memory-repository" ;
    ///        owlim:imports "./ontology/my_ontology.rdf" ;
    ///        owlim:defaultNS "http://www.my-organisation.org/ontology#"
    ///      ]
    ///    ].
    /// </pre>
    /// <para>
    /// See <a href="http://owlim.ontotext.com/display/OWLIMv51/OWLIM-Lite+Configuration">here</a> for the OWLIM Lite configuration documentation
    /// </para>
    class OwlimLiteTemplate
    {
    }
}

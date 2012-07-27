/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if UNFINISHED

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


#endif
/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
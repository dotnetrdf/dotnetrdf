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

namespace VDS.RDF.Shacl
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Query;

    public class ShaclShapesGraph : WrapperGraph
    {
        public ShaclShapesGraph(IGraph g)
            : base(g)
        {
        }

        public IEnumerable<ShaclShape> TargetedShapes
        {
            get
            {
                var results = (SparqlResultSet)this.ExecuteQuery(@"
PREFIX : <http://www.w3.org/ns/shacl#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT DISTINCT ?shape {
    {
        SELECT * {
            VALUES ?target { :targetNode :targetClass :targetSubjectsOf :targetObjectsOf }

            ?shape ?target ?any .
        }
    }

    UNION {
        SELECT * {
            VALUES ?class { :NodeShape :PropertyShape }

            ?shape a rdfs:Class, ?class.
        }
    }
}
");

                return results.Select(result => result["shape"]).Select(ShaclShape.Parse);
            }
        }

        public bool Validate(IGraph dataGragh, out ShaclValidationReport report)
        {
            var g = new Graph();
            g.NamespaceMap.AddNamespace("sh", UriFactory.Create(Shacl.BaseUri));
            report = ShaclValidationReport.Create(g);

            foreach (var targetedShape in this.TargetedShapes)
            {
                if (!targetedShape.Validate(dataGragh, report))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

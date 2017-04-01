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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Core
{

    /**
     * Manages the registered SPARQL aggregations (such as SUM).
     * These are loaded from the sp system ontology.
     * 
     * @author Holger Knublauch
     */
    public class Aggregations
    {

        private static Dictionary<String, INode> name2Type = new Dictionary<String, INode>();

        private static Dictionary<INode, String> type2Name = new Dictionary<INode, String>();

        // Load types from sp system ontology
        static Aggregations()
        {
            IGraph model = SP.GetModel();
            IEnumerator<INode> it = model.GetTriplesWithPredicateObject(RDFS.PropertySubClassOf, SP.ClassAggregation).Select(t => t.Subject).GetEnumerator();
            while (it.MoveNext())
            {
                INode aggType = it.Current;
                Triple labelTriple = model.GetTriplesWithSubjectPredicate(aggType, RDFS.PropertyLabel).FirstOrDefault();
                if (labelTriple != null) {
                    String name = ((ILiteralNode)labelTriple.Object).Value;
                    register(aggType, name);
                }
            }
        }


        /**
         * If registered, returns the display name of a given aggregation type.
         * @param aggType  the aggregation type, e.g. sp:Sum
         * @return the name (e.g., "SUM") or null if not registered
         */
        public static String getName(INode aggType)
        {
            if (type2Name.ContainsKey(aggType))
            {
                return type2Name[aggType];
            }
            return null;
        }


        /**
         * If registered, returns the aggregation INode for a given display name. 
         * @param name  the name (e.g., "SUM")
         * @return the type or null if not registered
         */
        public static INode getType(String name)
        {
            if (name2Type.ContainsKey(name))
            {
                return name2Type[name];
            }
            return null;
        }


        /**
         * Programatically adds a new aggregation type.  This is usually only
         * populated from the sp system ontology, but API users may want to
         * bypass (and extend) this mechanism.
         * @param aggType  the type to register
         * @param name  the display name
         */
        public static void register(INode aggType, String name)
        {
            type2Name[aggType] = name;
            name2Type[name] = aggType;
        }
    }
}
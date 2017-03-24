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
using System.Linq;
using System.Reflection;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Represents common extensions that are useful across all Plugin libraries
    /// </summary>
    static class DataExtensions
    {
        /// <summary>
        /// Gets either the String form of the Object of the Empty String
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Result of calling <strong>ToString()</strong> on non-null objects and the empty string for null objects</returns>
        internal static String ToSafeString(this Object obj)
        {
            return (obj != null ? obj.ToString() : String.Empty);
        }

        /// <summary>
        /// Ensures that a specific Object Factory type is registered in a Configuration Graph
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        /// <param name="factoryType">Factory Type</param>
        internal static void EnsureObjectFactory(this ConfigurationSerializationContext context, Type factoryType)
        {
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            String assm = Assembly.GetAssembly(factoryType).FullName;
            if (assm.Contains(',')) assm = assm.Substring(0, assm.IndexOf(','));

            //Firstly need to ensure our object factory has been referenced
            SparqlParameterizedString factoryCheck = new SparqlParameterizedString();
            factoryCheck.Namespaces.AddNamespace("dnr", UriFactory.Create(ConfigurationLoader.ConfigurationNamespace));
            factoryCheck.CommandText = "ASK WHERE { ?factory a dnr:ObjectFactory ; dnr:type '" + factoryType.FullName + ", " + assm + "' . }";
            SparqlResultSet rset = context.Graph.ExecuteQuery(factoryCheck) as SparqlResultSet;
            if (!rset.Result)
            {
                INode factory = context.Graph.CreateBlankNode();
                context.Graph.Assert(new Triple(factory, rdfType, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassObjectFactory))));
                context.Graph.Assert(new Triple(factory, dnrType, context.Graph.CreateLiteralNode(factoryType.FullName + ", " + assm)));
            }
        }
    }
}

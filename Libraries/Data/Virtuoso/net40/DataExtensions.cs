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
    static class PluginExtensions
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
        /// Gets either the String form of the URI of the Empty String
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>Result of calling <strong>AbsoluteUri</strong> on non-null URIs and the empty string for null URIs</returns>
        internal static String ToSafeString(this Uri u)
        {
            return (u != null ? u.AbsoluteUri : String.Empty);
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

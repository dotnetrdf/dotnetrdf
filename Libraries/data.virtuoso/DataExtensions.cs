using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        /// Ensures that a specific Object Factory type is registered in a Configuration Graph
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        /// <param name="factoryType">Factory Type</param>
        internal static void EnsureObjectFactory(this ConfigurationSerializationContext context, Type factoryType)
        {
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            String assm = Assembly.GetAssembly(factoryType).FullName;//Assembly.GetCallingAssembly().FullName;
            if (assm.Contains(',')) assm = assm.Substring(0, assm.IndexOf(','));

            //Firstly need to ensure our object factory has been referenced
            SparqlParameterizedString factoryCheck = new SparqlParameterizedString();
            factoryCheck.Namespaces.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
            factoryCheck.CommandText = "ASK WHERE { ?factory a dnr:ObjectFactory ; dnr:type '" + factoryType.FullName + ", " + assm + "' . }";
            SparqlResultSet rset = context.Graph.ExecuteQuery(factoryCheck) as SparqlResultSet;
            if (!rset.Result)
            {
                INode factory = context.Graph.CreateBlankNode();
                context.Graph.Assert(new Triple(factory, rdfType, ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassObjectFactory)));
                context.Graph.Assert(new Triple(factory, dnrType, context.Graph.CreateLiteralNode(factoryType.FullName + ", " + assm)));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Storage
{
    static class DataExtensions
    {
        internal static String ToSafeString(this Object obj)
        {
            return (obj != null ? obj.ToString() : String.Empty);
        }

        internal static void EnsureObjectFactory(this ConfigurationSerializationContext context, Type factoryType)
        {
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            String assm = Assembly.GetCallingAssembly().FullName;
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

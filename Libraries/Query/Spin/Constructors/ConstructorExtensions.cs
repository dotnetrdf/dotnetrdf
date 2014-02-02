using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Constructors
{
    internal static class ConstructorExtensions
    {
        internal static void RunConstructors(this SpinWrappedDataset dataset)
        {
            SpinWrappedDataset.QueryMode currentExecutionMode = dataset.QueryExecutionMode;

            String commandText = "SELECT DISTINCT ?class @FROM WHERE { ?s a ?class };";
            if (dataset.CurrentExecutionContext == null)
            {
                commandText = commandText.Replace("@FROM", "");
            }
            else
            {
                commandText = commandText.Replace("@FROM", "FROM <" + dataset.CurrentExecutionContext.ToString() + ">");
            }

            List<Resource> usedClasses = new List<Resource>();
            usedClasses.AddRange(((SparqlResultSet)dataset._storage.Query(commandText)).Results.Select(r => Resource.Get(r.Value("class"), dataset.spinProcessor)).Distinct());
            dataset.spinProcessor.SortClasses(usedClasses);

            dataset.QueryExecutionMode = SpinWrappedDataset.QueryMode.SpinInferencing;
            // TODO evaluate the constructors 
            dataset.QueryExecutionMode = currentExecutionMode;
        }
    }
}

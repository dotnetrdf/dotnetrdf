using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Model;

namespace VDS.RDF.Query.Spin.Constructors
{
    internal static class ConstructorExtensions
    {
        // TODO pass the 
        internal static IGraph RunConstructors(this SpinWrappedDataset dataset, List<Resource> usedClasses)
        {
            IGraph outputGraph = new ThreadSafeGraph();
            if (usedClasses.Count > 0)
            {
                dataset.spinProcessor.SortClasses(usedClasses);

                SpinWrappedDataset.QueryMode currentExecutionMode = dataset.QueryExecutionMode;
                dataset.QueryExecutionMode = SpinWrappedDataset.QueryMode.SpinInferencing;

                foreach (Resource classResource in usedClasses)
                {
                    IEnumerable<IUpdate> constructors = dataset.spinProcessor.GetConstructorsForClass(classResource);
                    if (constructors.Count() > 0)
                    {
                        outputGraph.Assert(dataset.ExecuteUpdate(constructors).Triples);
                    }
                }
                dataset.QueryExecutionMode = currentExecutionMode;
            }
            return outputGraph;
        }


    }
}

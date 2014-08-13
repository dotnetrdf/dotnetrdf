using System;
using System.Collections.Generic;
using VDS.RDF.Query.Grouping;

namespace VDS.RDF.Query.Engine.Grouping
{
    public interface ISolutionGroup
        : ISolution
    {
        IDictionary<string, IAccumulator> Accumulators { get; }

        void FinalizeGroup();
    }
}

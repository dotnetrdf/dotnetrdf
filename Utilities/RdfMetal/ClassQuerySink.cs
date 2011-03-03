using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using VDS.RDF;
using VDS.RDF.Linq;
using VDS.RDF.Query;

namespace VDS.RDF.Utilities.Linq.Metal
{
    public class ClassQuerySink : BaseLinqResultsSink
    {
        private readonly bool ignoreBnodes;
        private readonly string @namespace;
        private readonly string[] varNames;

        public List<NameValueCollection> bindings = new List<NameValueCollection>();

        public ClassQuerySink(bool ignoreBnodes, string @namespace, string[] varNames)
        {
            this.ignoreBnodes = ignoreBnodes;
            this.@namespace = @namespace;
            this.varNames = varNames;
        }

        protected override void ProcessResult(SparqlResult result)
        {
            var nvc = new NameValueCollection();

            foreach (string varName in varNames)
            {
                if (!result.HasValue(varName)) continue;

                INode resource = result[varName];

                if (resource.NodeType == NodeType.Uri)
                {
                    if (string.IsNullOrEmpty(@namespace) || ((UriNode)resource).ToString().StartsWith(@namespace))
                    {
                        nvc[varName] = resource.ToString();
                    }
                }
                else if (resource.NodeType == NodeType.Blank && !ignoreBnodes)
                {
                    var bn = resource as BlankNode;
                    if (string.IsNullOrEmpty(@namespace) || bn.InternalID.StartsWith(@namespace))
                    {
                        nvc[varName] = bn.InternalID;
                    }
                }
            }
            bindings.Add(nvc);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.LinkedData.Profiles
{
    /// <summary>
    /// Class for representing Expansion Datasets
    /// </summary>
    public class ExpansionDataset : VoIDDataset
    {
        private bool _ignore = false;
        private List<Uri> _discoveryEndpoints = new List<Uri>();

        public ExpansionDataset(IGraph expansionDescription, INode datasetSubj)
            : base(expansionDescription, datasetSubj)
        {
            //Check for aat:ignoreDataset
            IEnumerable<Triple> ts = expansionDescription.GetTriplesWithSubjectPredicate(datasetSubj, expansionDescription.CreateUriNode("aat:ignoreDataset"));
            Triple t = ts.FirstOrDefault();
            if (t != null)
            {
                if (t.Object.NodeType == NodeType.Literal)
                {
                    ILiteralNode l = (ILiteralNode)t.Object;
                    if (l.DataType != null && l.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                    {
                        this._ignore = Boolean.Parse(l.Value);
                    }
                }
            }

            //Find URI Discovery Endpoints
            ts = expansionDescription.GetTriplesWithSubjectPredicate(datasetSubj, expansionDescription.CreateUriNode("aat:uriDiscoveryEndpoint"));
            foreach (Triple endpoint in ts)
            {
                if (endpoint.Object.NodeType == NodeType.Uri)
                {
                    this._discoveryEndpoints.Add(((IUriNode)endpoint.Object).Uri);
                }
            }
        }

        public ExpansionDataset(INode datasetSubj)
        {
            this._subj = datasetSubj;
        }

        public bool Ignore
        {
            get
            {
                return this._ignore;
            }
        }

        public IEnumerable<Uri> UriDiscoveryEndpoints
        {
            get
            {
                return this._discoveryEndpoints;
            }
        }
    }
}

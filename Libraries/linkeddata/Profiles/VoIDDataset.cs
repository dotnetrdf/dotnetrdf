using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.LinkedData.Profiles
{
    /// <summary>
    /// Class for representing VoID Datasets
    /// </summary>
    public class VoIDDataset
    {
        private String _title = String.Empty, _description = String.Empty;
        private Uri _homepage;
        private List<Uri> _sparqlEndpoints = new List<Uri>();
        private List<Uri> _lookupEndpoints = new List<Uri>();
        protected INode _subj;

        protected VoIDDataset()
        {

        }

        public VoIDDataset(IGraph voidDescription, INode datasetSubj)
        {
            this._subj = datasetSubj;

            IEnumerable<Triple> ts;
            Triple t;

            //Get Title, Description and Homepage (if present)
            ts = voidDescription.GetTriplesWithSubjectPredicate(datasetSubj, voidDescription.CreateUriNode("dcterms:title"));
            t = ts.FirstOrDefault();
            if (t != null)
            {
                if (t.Object.NodeType == NodeType.Literal)
                {
                    this._title = ((ILiteralNode)t.Object).Value;
                }
            }
            ts = voidDescription.GetTriplesWithSubjectPredicate(datasetSubj, voidDescription.CreateUriNode("dcterms:description"));
            t = ts.FirstOrDefault();
            if (t != null)
            {
                if (t.Object.NodeType == NodeType.Literal)
                {
                    this._description = ((ILiteralNode)t.Object).Value;
                }
            }
            ts = voidDescription.GetTriplesWithSubjectPredicate(datasetSubj, voidDescription.CreateUriNode("foaf:homepage"));
            t = ts.FirstOrDefault();
            if (t != null)
            {
                if (t.Object.NodeType == NodeType.Uri)
                {
                    this._homepage = ((IUriNode)t.Object).Uri;
                }
            }

            //Find SPARQL Endpoints
            ts = voidDescription.GetTriplesWithSubjectPredicate(datasetSubj, voidDescription.CreateUriNode("void:sparqlEndpoint"));
            foreach (Triple endpoint in ts) 
            {
                if (endpoint.Object.NodeType == NodeType.Uri)
                {
                    this._sparqlEndpoints.Add(((IUriNode)endpoint.Object).Uri);
                }
            }

            //Find URI Lookup Endpoints
            ts = voidDescription.GetTriplesWithSubjectPredicate(datasetSubj, voidDescription.CreateUriNode("void:uriLookupEndpoint"));
            foreach (Triple endpoint in ts)
            {
                if (endpoint.Object.NodeType == NodeType.Uri)
                {
                    this._lookupEndpoints.Add(((IUriNode)endpoint.Object).Uri);
                }
            }
        }

        public INode Subject
        {
            get
            {
                return this._subj;
            }
        }

        public String Title
        {
            get
            {
                return this._title;
            }
        }

        public String Description
        {
            get
            {
                return this._description;
            }
        }

        public Uri Homepage
        {
            get
            {
                return this._homepage;
            }
        }

        public IEnumerable<Uri> SparqlEndpoints
        {
            get
            {
                return this._sparqlEndpoints;
            }
        }

        public IEnumerable<Uri> UriLookupEndpoints
        {
            get
            {
                return this._lookupEndpoints;
            }
        }
    }
}

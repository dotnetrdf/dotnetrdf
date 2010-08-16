using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.LinkedData.Profiles
{
    /// <summary>
    /// Class for representing VoID Descriptions
    /// </summary>
    public class VoIDDescription
    {
        /// <summary>
        /// VoID Namespace URI
        /// </summary>
        public const String VoIDNamespace = "http://rdfs.org/ns/void#";
        /// <summary>
        /// FOAF Namespace URI
        /// </summary>
        public const String FoafNamespace = "http://xmlns.com/foaf/0.1/";
        /// <summary>
        /// Dublin Core Terms Namespace URI
        /// </summary>
        public const String DublinCoreTermsNamespace = "http://purl.org/dc/terms/";

        protected Dictionary<INode, VoIDDataset> _datasets = new Dictionary<INode, VoIDDataset>();
        protected Dictionary<INode, VoIDLinkset> _linksets = new Dictionary<INode, VoIDLinkset>();

        /// <summary>
        /// Creates an empty VoID Description
        /// </summary>
        /// <remarks>
        /// Included to allow derived classes to call an empty constructor
        /// </remarks>
        protected internal VoIDDescription()
        {

        }

        /// <summary>
        /// Creates a new VoID Description which is loaded from the given File
        /// </summary>
        /// <param name="file">Filename</param>
        public VoIDDescription(String file)
        {
            Graph g = new Graph();
            FileLoader.Load(g, file);

            this.Initialise(g);
        }

        /// <summary>
        /// Creates a new VoID Description which is loaded from the given URI
        /// </summary>
        /// <param name="u">URI</param>
        public VoIDDescription(Uri u)
        {
            Graph g = new Graph();
            UriLoader.Load(g, u);

            this.Initialise(g);
        }

        /// <summary>
        /// Creates a new VoID Description from the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public VoIDDescription(IGraph g)
        {
            this.Initialise(g);
        }

        protected virtual void Initialise(IGraph g)
        {
            //First ensure all the correct namespace prefixes are set
            g.NamespaceMap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            g.NamespaceMap.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            g.NamespaceMap.AddNamespace("owl", new Uri(NamespaceMapper.OWL));
            g.NamespaceMap.AddNamespace("void", new Uri(VoIDNamespace));
            g.NamespaceMap.AddNamespace("foaf", new Uri(FoafNamespace));
            g.NamespaceMap.AddNamespace("dcterms", new Uri(DublinCoreTermsNamespace));

            //Find Datasets
            foreach (Triple t in g.GetTriplesWithPredicateObject(g.CreateUriNode("rdf:type"), g.CreateUriNode("void:Dataset")))
            {
                VoIDDataset dataset = new VoIDDataset(g, t.Subject);
                if (this._datasets.ContainsKey(t.Subject))
                {
                    throw new NotImplementedException("Merging VoID Datasets is not yet implemented");
                }
                else
                {
                    this._datasets.Add(t.Subject, dataset);
                }
            }

            //Find Linksets
            foreach (Triple t in g.GetTriplesWithPredicateObject(g.CreateUriNode("rdf:type"), g.CreateUriNode("void:Linkset")))
            {
                VoIDLinkset linkset = new VoIDLinkset(g, t.Subject);
                if (this._linksets.ContainsKey(t.Subject))
                {
                    throw new NotImplementedException("Merging VoID Linksets is not yet implemented");
                }
                else
                {
                    this._linksets.Add(t.Subject, linkset);
                }
            }
        }

        /// <summary>
        /// Gets the Datasets specified in the VoID Description
        /// </summary>
        public IEnumerable<VoIDDataset> Datasets
        {
            get
            {
                return this._datasets.Values;
            }
        }

        /// <summary>
        /// Gets the Linksets specified in the VoID Description
        /// </summary>
        public IEnumerable<VoIDLinkset> Linksets
        {
            get
            {
                return this._linksets.Values;
            }
        }

        /// <summary>
        /// Gets a Dataset with the given Subject
        /// </summary>
        /// <param name="datasetSubj">Subject</param>
        /// <returns></returns>
        public VoIDDataset GetDataset(INode datasetSubj)
        {
            if (this._datasets.ContainsKey(datasetSubj))
            {
                return this._datasets[datasetSubj];
            }
            else
            {
                throw new KeyNotFoundException("A Dataset with the given Subject does not exist in this VoID Profile");
            }
        }

        /// <summary>
        /// Gets a Linkset with the given Subject
        /// </summary>
        /// <param name="linksetSubj">Subject</param>
        /// <returns></returns>
        public VoIDLinkset GetLinkset(INode linksetSubj)
        {
            if (this._linksets.ContainsKey(linksetSubj))
            {
                return this._linksets[linksetSubj];
            }
            else
            {
                throw new KeyNotFoundException("A Linkset with the given Subject does not exist in this VoID Profile");
            }
        }
    }
}

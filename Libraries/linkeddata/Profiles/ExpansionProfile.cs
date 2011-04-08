using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.LinkedData.Profiles
{
    /// <summary>
    /// Class for representing Expansion Profiles
    /// </summary>
    public class ExpansionProfile : VoIDDescription
    {
        /// <summary>
        /// All About That Namespace URI
        /// </summary>
        public const String AATNamespace = "http://www.dotnetrdf.org/AllAboutThat/";

        private int _maxDepth = 1;
        private IUriNode _node;

        /// <summary>
        /// Creates a new Expansion Profile which uses the Default Profile
        /// </summary>
        /// <remarks>
        /// The Default Profile is an embedded resource in this Assembly with a type of <strong>VDS.RDF.LinkedData.Profiles.DefaultExpansionProfile.ttl</strong>
        /// </remarks>
        public ExpansionProfile()
        {
            this.LoadDefaultProfile();
        }

        public ExpansionProfile(bool empty)
        {
            if (!empty)
            {
                this.LoadDefaultProfile();
            }
        }

        /// <summary>
        /// Creates a new Expansion Profile which is loaded from the given File
        /// </summary>
        /// <param name="file">Filename</param>
        public ExpansionProfile(String file)
            : base(file) { }

        /// <summary>
        /// Creates a new Expansion Profile which is loaded from the given URI
        /// </summary>
        /// <param name="u">URI</param>
        public ExpansionProfile(Uri u)
            : base(u) { }

        /// <summary>
        /// Creates a new Expansion Profile which is loaded from the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public ExpansionProfile(IGraph g)
            : base(g) { }

        /// <summary>
        /// Loads the Default Profile which is embedded in this assembly
        /// </summary>
        private void LoadDefaultProfile()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("VDS.RDF.LinkedData.Profiles.DefaultExpansionProfile.ttl"));
            TurtleParser ttlparser = new TurtleParser();
            Graph g = new Graph();
            ttlparser.Load(g, reader);
            this.Initialise(g);
        }

        protected override void Initialise(IGraph g)
        {
            if (g.BaseUri != null) this._node = g.CreateUriNode();

            //First ensure all the correct namespace prefixes are set to the correct URIs
            g.NamespaceMap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            g.NamespaceMap.AddNamespace("void", new Uri(VoIDNamespace));
            g.NamespaceMap.AddNamespace("foaf", new Uri(FoafNamespace));
            g.NamespaceMap.AddNamespace("dcterms", new Uri(DublinCoreTermsNamespace));
            g.NamespaceMap.AddNamespace("aat", new Uri(AATNamespace));

            //First look for an Expansion Profile description
            Triple profileDescriptor = new Triple(g.CreateUriNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode("aat:ExpansionProfile"));
            if (g.ContainsTriple(profileDescriptor))
            {
                //Does it specify a Max Expansion Depth?
                Triple maxDepthSpecifier = g.GetTriplesWithSubjectPredicate(g.CreateUriNode(), g.CreateUriNode("aat:maxExpansionDepth")).FirstOrDefault();
                if (maxDepthSpecifier != null)
                {
                    if (maxDepthSpecifier.Object.NodeType == NodeType.Literal)
                    {
                        ILiteralNode l = (ILiteralNode)maxDepthSpecifier.Object;
                        if (l.DataType != null && l.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeInteger))
                        {
                            this._maxDepth = Int32.Parse(l.Value);
                        }
                    }
                }
            }

            //Find Datasets
            foreach (Triple t in g.GetTriplesWithPredicateObject(g.CreateUriNode("rdf:type"), g.CreateUriNode("void:Dataset")))
            {
                ExpansionDataset dataset = new ExpansionDataset(g, t.Subject);
                if (this._datasets.ContainsKey(t.Subject))
                {
                    throw new NotImplementedException("Merging Expansion Datasets is not yet implemented");
                }
                else
                {
                    this._datasets.Add(t.Subject, dataset);
                }
            }

            //Find Linksets
            foreach (Triple t in g.GetTriplesWithPredicateObject(g.CreateUriNode("rdf:type"), g.CreateUriNode("void:Linkset")))
            {
                ExpansionLinkset linkset = new ExpansionLinkset(g, t.Subject);
                if (this._linksets.ContainsKey(t.Subject))
                {
                    throw new NotImplementedException("Merging Expansion Linksets is not yet implemented");
                }
                else
                {
                    this._linksets.Add(t.Subject, linkset);
                }
            }

        }

        /// <summary>
        /// Gets/Sets the Max Expansion Depth
        /// </summary>
        public int MaxExpansionDepth
        {
            get
            {
                return Math.Max(this._maxDepth, 0);
            }
            set
            {
                this._maxDepth = Math.Max(value, 0);
            }
        }

        /// <summary>
        /// Gets the Expansion Datasets in the Profile
        /// </summary>
        public IEnumerable<ExpansionDataset> ExpansionDatasets
        {
            get
            {
                return (from d in this._datasets.Values
                        where d is ExpansionDataset
                        select (ExpansionDataset)d);
            }
        }

        /// <summary>
        /// Gets the Expansion Linksets in the Profile
        /// </summary>
        public IEnumerable<ExpansionLinkset> ExpansionLinksets
        {
            get
            {
                return (from l in this._linksets.Values
                        where l is ExpansionLinkset
                        select (ExpansionLinkset)l);
            }
        }

        /// <summary>
        /// Gets the Base URI of the Profile as a URI Node (if it exists) or a null
        /// </summary>
        public INode BaseUri
        {
            get
            {
                return this._node;
            }
        }

        /// <summary>
        /// Gets a Dataset with the given Subject
        /// </summary>
        /// <param name="datasetSubj">Subject</param>
        /// <returns></returns>
        public ExpansionDataset GetExpansionDataset(INode datasetSubj)
        {
            if (this._datasets.ContainsKey(datasetSubj))
            {
                return (ExpansionDataset)this._datasets[datasetSubj];
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
        public ExpansionLinkset GetExpansionLinkset(INode linksetSubj)
        {
            if (this._linksets.ContainsKey(linksetSubj))
            {
                return (ExpansionLinkset)this._linksets[linksetSubj];
            }
            else
            {
                throw new KeyNotFoundException("A Linkset with the given Subject does not exist in this VoID Profile");
            }
        }

        public void AddExpansionDataset(ExpansionDataset d)
        {
            if (!this._datasets.ContainsKey(d.Subject))
            {
                this._datasets.Add(d.Subject, d);
            }
        }

        public void AddExpansionLinkset(ExpansionLinkset l)
        {
            if (!this._linksets.ContainsKey(l.Subject))
            {
                this._linksets.Add(l.Subject, l);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Graph Stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// The IGraphStore interface is a hybrid of the old ITripleStore and ISparqlDataset interfaces from the 1.0 APIs, it allows for data to be manipulated at either the Graph or Quad level as desired.
    /// </para>
    /// </remarks>
    public interface IGraphStore
    {
        /// <summary>
        /// Gets the URIs of the Graphs in the Store
        /// </summary>
        IEnumerable<Uri> GraphUris
        { 
            get;
        }

        /// <summary>
        /// Gets the Graphs in the Store
        /// </summary>
        IEnumerable<IGraph> Graphs
        { 
            get;
        }

        /// <summary>
        /// Gets a specific Graph from the Store
        /// </summary>
        /// <param name="u">Graph URI</param>
        /// <returns></returns>
        IGraph this[Uri u]
        { 
            get; 
        }

        /// <summary>
        /// Determines whether the store has a specific graph
        /// </summary>
        /// <param name="u">Graph URI</param>
        /// <returns>True if the Graph exists in the store, false otherwise</returns>
        bool HasGraph(Uri u);

        /// <summary>
        /// Adds the contents of a Graph to the store using the Graphs stated <see cref="IGraph.BaseUri"/> as the Graph URI for the added Quads
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns>True if any quads are added, false otherwise</returns>
        bool Add(IGraph g);

        /// <summary>
        /// Adds the contents of a Graph to the store using the given URI for the added Quads
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="graphUri">URI of the Graph to add to</param>
        /// <returns>True if any quads are added, false otherwise</returns>
        bool Add(IGraph g, Uri graphUri);

        /// <summary>
        /// Adds a Triple to the Store as a Quad using the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        /// <returns>True if a quad is added, false otherwise</returns>
        bool AddTriple(Uri graphUri, Triple t);

        /// <summary>
        /// Adds a Quad to the Store
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns>True if a quad is added, false otherwise</returns>
        bool AddQuad(Quad q);

        /// <summary>
        /// Copies the contents of one graph to another
        /// </summary>
        /// <param name="srcUri">Source Graph URI</param>
        /// <param name="destUri">Target Graph URI</param>
        /// <param name="overwrite">If true the contents of the target graph are overwritten, if false the copied data is added to the existing data in the target graph</param>
        /// <returns>True if any quads are copied, false otherwise</returns>
        bool Copy(Uri srcUri, Uri destUri, bool overwrite);

        /// <summary>
        /// Moves the contents of one graph to another
        /// </summary>
        /// <param name="srcUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        /// <param name="overwrite">If true the contents of the target graph are overwritten, if false the moved data is added to the existing data in the target graph</param>
        /// <returns>True if any quads are moved, false otherwise</returns>
        bool Move(Uri srcUri, Uri destUri, bool overwrite);

        /// <summary>
        /// Removes the contents of the given graph from the store
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns>True if any quads are removed, false otherwise</returns>
        bool Remove(IGraph g);

        /// <summary>
        /// Removes the contents of the graph with the given URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>True if any quads are removed, false otherwise</returns>
        bool Remove(Uri u);

        /// <summary>
        /// Get all Triples in the Store
        /// </summary>
        IEnumerable<Triple> Triples 
        { 
            get;
        }

  //Get all Triples with given Subject in given Graph(s)
  IEnumerable<Triple> GetTriplesWithSubject(IEnumerable<Uri> graphUris, INode subj);

  //Get all Quads in the store
  IEnumerable<Quad> Quads { get; }

  //Get all quads that match the given subject across all graphs
  IEnumerable<Quad> GetQuadsWithSubject(INode subj)

  //Get all quads that match the given subject in the given graphs
  IEnumerable<Quad> GetQuadsWithSubject(IEnumerable<Uri> graphUris, INode subj);

  //Is a Triple found anywhere in the store
  bool ContainsTriple(Triple t);

  //Is the Triple contained in the given Graphs
  bool ContainsTriple(IEnumerable<Uri> graphUris, Triple t);

  //Does a Quad exist in the store
  bool ContainsQuad(Quad q);
    }
}

/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// An Inference Engine which uses RDFS reasoning
    /// </summary>
    /// <remarks>
    /// <para>
    /// Does basic RDFS inferencing using the schema taken from the Graph(s) which are provided in calls to the reasoners <see cref="StaticRdfsReasoner.Initialise">Initialise()</see> method.
    /// </para>
    /// <para>
    /// Types of inference performed are as follows:
    /// </para>
    /// <ul>
    ///     <li>Class hierarchy reasoning - asserts additional types triples for anything that is typed as the subclass of a class.</li>
    ///     <li>Property hierarchy reasoning - asserts additional property triples for anything where the predicate is a subproperty of a defined property</li>
    ///     <li>Domain &amp; Range reasoning - asserts additional type triples based on the domains and ranges of properties</li>
    /// </ul>
    /// </remarks>
    public class StaticRdfsReasoner : IInferenceEngine
    {
        private Dictionary<INode, INode> _classMappings = new Dictionary<INode, INode>();
        private Dictionary<INode, INode> _propertyMappings = new Dictionary<INode, INode>();
        private HashTable<INode, INode> _domainMappings = new HashTable<INode, INode>();
        private HashTable<INode, INode> _rangeMappings = new HashTable<INode, INode>();
        private IUriNode _rdfType, _rdfsClass, _rdfsSubClass, _rdfProperty, _rdfsSubProperty, _rdfsRange, _rdfsDomain;

        /// <summary>
        /// Creates a new instance of the Static RdfsReasoner
        /// </summary>
        public StaticRdfsReasoner()
        {
            Graph g = new Graph();
            this._rdfType = g.CreateUriNode("rdf:type");
            this._rdfsClass = g.CreateUriNode("rdfs:Class");
            this._rdfsSubClass = g.CreateUriNode("rdfs:subClassOf");
            this._rdfProperty = g.CreateUriNode("rdf:Property");
            this._rdfsSubProperty = g.CreateUriNode("rdfs:subPropertyOf");
            this._rdfsDomain = g.CreateUriNode("rdfs:domain");
            this._rdfsRange = g.CreateUriNode("rdfs:range");
        }

        /// <summary>
        /// Applies inference to the given Graph and outputs the inferred information to that Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public virtual void Apply(IGraph g)
        {
            this.Apply(g, g);
        }

        /// <summary>
        /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph
        /// </summary>
        /// <param name="input">Graph to apply inference to</param>
        /// <param name="output">Graph inferred information is output to</param>
        public virtual void Apply(IGraph input, IGraph output)
        {
            //Infer information
            List<Triple> inferences = new List<Triple>();
            foreach (Triple t in input.Triples)
            {
                //Apply class/property hierarchy inferencing
                if (t.Predicate.Equals(this._rdfType))
                {
                    if (!t.Object.Equals(this._rdfsClass) && !t.Object.Equals(this._rdfProperty))
                    {
                        this.InferClasses(t, input, output, inferences);
                    }
                }
                else if (t.Predicate.Equals(this._rdfsSubClass))
                {
                    //Assert that this thing is a Class
                    inferences.Add(new Triple(t.Subject.CopyNode(output), this._rdfType.CopyNode(output), this._rdfsClass.CopyNode(output)));
                }
                else if (t.Predicate.Equals(this._rdfsSubProperty))
                {
                    //Assert that this thing is a Property
                    inferences.Add(new Triple(t.Subject.CopyNode(output), this._rdfType.CopyNode(output), this._rdfProperty.CopyNode(output)));
                }
                else if (this._propertyMappings.ContainsKey(t.Predicate))
                {
                    INode property = t.Predicate;

                    //Navigate up the property hierarchy asserting additional properties if able
                    while (this._propertyMappings.ContainsKey(property))
                    {
                        if (this._propertyMappings[property] != null)
                        {
                            //Assert additional properties
                            inferences.Add(new Triple(t.Subject.CopyNode(output), this._propertyMappings[property].CopyNode(output), t.Object.CopyNode(output)));
                            property = this._propertyMappings[property];
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //Apply Domain and Range inferencing on Predicates
                if (this._rangeMappings.ContainsKey(t.Predicate))
                {
                    //Assert additional type information
                    foreach (INode n in this._rangeMappings.GetValues(t.Predicate))
                    {
                        inferences.Add(new Triple(t.Object.CopyNode(output), this._rdfType.CopyNode(output), n.CopyNode(output)));
                    }

                    //Call InferClasses to get extra type information
                    this.InferClasses(inferences[inferences.Count - 1], input, output, inferences);
                }
                if (this._domainMappings.ContainsKey(t.Predicate))
                {
                    //Assert additional type information
                    foreach (INode n in this._domainMappings.GetValues(t.Predicate))
                    {
                        inferences.Add(new Triple(t.Subject.CopyNode(output), this._rdfType.CopyNode(output), n.CopyNode(output)));
                    }
                    
                    //Call InferClasses to get extra type information
                    this.InferClasses(inferences[inferences.Count - 1], input, output, inferences);
                }
            }

            //Assert the inferred information
            inferences.RemoveAll(t => t.Subject.NodeType == NodeType.Literal);
            if (inferences.Count > 0)
            {
                output.Assert(inferences);
            }
        }

        /// <summary>
        /// Imports any Class heirarchy information from the given Graph into the Reasoners Knowledge Base in order to initialise the Reasoner
        /// </summary>
        /// <param name="g">Graph to import from</param>
        /// <remarks>
        /// Looks for Triples defining things to be classes and those defining that something is a subClass of something
        /// </remarks>
        public void Initialise(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                if (t.Predicate.Equals(this._rdfType))
                {
                    if (t.Object.Equals(this._rdfsClass))
                    {
                        //The Triple defines a Class
                        if (!this._classMappings.ContainsKey(t.Subject))
                        {
                            this._classMappings.Add(t.Subject, null);
                        }
                    } 
                    else if (t.Object.Equals(this._rdfProperty)) 
                    {
                        //The Triple defines a Property
                        if (!this._propertyMappings.ContainsKey(t.Subject)) 
                        {
                            this._propertyMappings.Add(t.Subject, null);
                        }
                    }
                }
                else if (t.Predicate.Equals(this._rdfsSubClass))
                {
                    //The Triple defines a Sub Class
                    if (!this._classMappings.ContainsKey(t.Subject))
                    {
                        this._classMappings.Add(t.Subject, t.Object);
                    }
                    else if (this._classMappings[t.Subject] == null)
                    {
                        this._classMappings[t.Subject] = t.Object;
                    }
                }
                else if (t.Predicate.Equals(this._rdfsSubProperty))
                {
                    //The Triple defines a Sub property
                    if (!this._propertyMappings.ContainsKey(t.Subject))
                    {
                        this._propertyMappings.Add(t.Subject, t.Object);
                    }
                    else if (this._propertyMappings[t.Subject] == null)
                    {
                        this._propertyMappings[t.Subject] = t.Object;
                    }
                }
                else if (t.Predicate.Equals(this._rdfsRange))
                {
                    //This Triple defines a Range
                    if (!this._propertyMappings.ContainsKey(t.Subject))
                    {
                        this._propertyMappings.Add(t.Subject, null);
                    }
                    if (!this._rangeMappings.Contains(t.Subject, t.Object))
                    {
                        this._rangeMappings.Add(t.Subject, t.Object);
                    }
                    if (!this._classMappings.ContainsKey(t.Object))
                    {
                        this._classMappings.Add(t.Object, null);
                    }
                }
                else if (t.Predicate.Equals(this._rdfsDomain))
                {
                    //This Triple defines a Domain
                    if (!this._propertyMappings.ContainsKey(t.Subject))
                    {
                        this._propertyMappings.Add(t.Subject, null);
                    }
                    if (!this._domainMappings.Contains(t.Subject, t.Object))
                    {
                        this._domainMappings.Add(t.Subject, t.Object);
                    }
                    if (!this._classMappings.ContainsKey(t.Object))
                    {
                        this._classMappings.Add(t.Object, null);
                    }
                }
                else
                {
                    //Just add the property as a predicate
                    if (!this._propertyMappings.ContainsKey(t.Predicate))
                    {
                        this._propertyMappings.Add(t.Predicate, null);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method which applies Class hierarchy inferencing
        /// </summary>
        /// <param name="t">Triple defining the type for something</param>
        /// <param name="input">Input Graph</param>
        /// <param name="output">Output Graph</param>
        /// <param name="inferences">List of Inferences</param>
        private void InferClasses(Triple t, IGraph input, IGraph output, List<Triple> inferences)
        {
            INode type = t.Object;

            //Navigate up the class hierarchy asserting additional types if able
            while (this._classMappings.ContainsKey(type))
            {
                if (this._classMappings[type] != null)
                {
                    //Assert additional type information
                    inferences.Add(new Triple(t.Subject.CopyNode(output), t.Predicate.CopyNode(output), this._classMappings[type].CopyNode(output)));
                    type = this._classMappings[type];
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// An Inference Engine which uses RDFS reasoning
    /// </summary>
    /// <remarks>
    /// Does basic RDFS inferencing as detailed in the remarks for the <see cref="StaticRdfsReasoner">StaticRdfsReasoner</see> except every Graph that inference is applied to has the potential to alter the schema which is in use.
    /// </remarks>
    public class RdfsReasoner : StaticRdfsReasoner
    {
        /// <summary>
        /// Applies inference to the Input Graph and outputs the inferred information to the Output Graph
        /// </summary>
        /// <param name="input">Graph to apply inference to</param>
        /// <param name="output">Graph inferred information is output to</param>
        public override void Apply(IGraph input, IGraph output)
        {
            //Use this Graph to further initialise the Reasoner
            this.Initialise(input);

            //Use the Base Reasoner to do the inference
            base.Apply(input, output);
        }
    }
}

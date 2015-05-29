/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Context Class for writing serializing Configuration information
    /// </summary>
    public class ConfigurationSerializationContext
    {
        private INode _nextSubj = null;

        /// <summary>
        /// Creates a new Serialization Context
        /// </summary>
        public ConfigurationSerializationContext()
        {
            this.Graph = new Graph();
            this.Graph.Namespaces.AddNamespace("dnr", UriFactory.Create(ConfigurationVocabulary.ConfigurationNamespace));
        }

        /// <summary>
        /// Creates a new Serialization Context
        /// </summary>
        /// <param name="g">Base Configuration Graph</param>
        public ConfigurationSerializationContext(IGraph g)
        {
            this.Graph = g;
        }

        /// <summary>
        /// Gets the Graph to which Configuration information should be written
        /// </summary>
        public IGraph Graph { get; protected set; }

        /// <summary>
        /// Gets/Sets the next subject to be used
        /// </summary>
        /// <remarks>
        /// <para>
        /// Always returns a Blank Node if none is currently explicitly specified
        /// </para>
        /// <para>
        /// Used to link objects together when you want some subsidiary object to serialize it's configuration and link that to the configuration you are currently serializing
        /// </para>
        /// </remarks>
        public INode NextSubject
        {
            get
            {
                INode temp = this._nextSubj;
                if (temp == null)
                {
                    //When not set generate a new blank node
                    temp = this.Graph.CreateBlankNode();
                }
                else
                {
                    //When retrieving a set subject make sure to null it so it isn't reused
                    this._nextSubj = null;
                }
                return temp;
            }
            set
            {
                this._nextSubj = value;
            }
        }
    }
}

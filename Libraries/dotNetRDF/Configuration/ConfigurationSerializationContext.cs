/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Context Class for writing serializing Configuration information
    /// </summary>
    public class ConfigurationSerializationContext
    {
        /// <summary>
        /// Configuration Graph being written to
        /// </summary>
        protected IGraph _g;

        private INode _nextSubj = null;

        /// <summary>
        /// Creates a new Serialization Context
        /// </summary>
        public ConfigurationSerializationContext()
        {
            _g = new Graph();
            _g.NamespaceMap.AddNamespace("dnr", UriFactory.Create(ConfigurationLoader.ConfigurationNamespace));
        }

        /// <summary>
        /// Creates a new Serialization Context
        /// </summary>
        /// <param name="g">Base Configuration Graph</param>
        public ConfigurationSerializationContext(IGraph g)
        {
            _g = g;
        }

        /// <summary>
        /// Gets the Graph to which Configuration information should be written
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _g;
            }
        }

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
                INode temp = _nextSubj;
                if (temp == null)
                {
                    // When not set generate a new blank node
                    temp = _g.CreateBlankNode();
                }
                else
                {
                    // When retrieving a set subject null it so it isn't reused
                    _nextSubj = null;
                }
                return temp;
            }
            set
            {
                _nextSubj = value;
            }
        }
    }
}

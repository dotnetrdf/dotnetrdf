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
            this._g = new Graph();
            this._g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
        }

        /// <summary>
        /// Creates a new Serialization Context
        /// </summary>
        /// <param name="g">Base Configuration Graph</param>
        public ConfigurationSerializationContext(IGraph g)
        {
            this._g = g;
        }

        /// <summary>
        /// Gets the Graph to which Configuration information should be written
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
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
                INode temp = this._nextSubj;
                if (temp == null)
                {
                    //When not set generate a new blank node
                    temp = this._g.CreateBlankNode();
                }
                else
                {
                    //When retrieving a set subject null it so it isn't reused
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

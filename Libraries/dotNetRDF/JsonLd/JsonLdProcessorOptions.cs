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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// A collection of options for setting up the JSON-LD processor
    /// </summary>
    public class JsonLdProcessorOptions
    {
        /// <summary>
        /// Overrides the base IRI of the document being processed
        /// </summary>
        public Uri Base { get; set; }

        /// <summary>
        /// Get or set the function to use to resolve an IRI reference to a document
        /// into parsed JSON.
        /// </summary>
        /// <remarks>If the function returns null or throws an exception, it will be assumed that dereferencing the IRI has failed</remarks>
        public Func<Uri, RemoteDocument> Loader { get; set; }

        /// <summary>
        /// Get or set the syntax version that the processor will use.
        /// </summary>
        /// <remarks>Defaults to <see cref="JsonLdProcessingMode.JsonLd11"/></remarks>
        public JsonLdProcessingMode? ProcessingMode = JsonLdProcessingMode.JsonLd11;

        /// <summary>
        /// A context that is used to initialize the active context when expanding a document.
        /// </summary>
        public JToken ExpandContext;

        /// <summary>
        /// Flag indicating if arrays of one element should be replaced by the single value during compaction.
        /// </summary>
        /// <remarks>Defaults to <code>true</code></remarks>
        public bool CompactArrays = true;

        /// <summary>
        /// If set to true, the JSON-LD processor may emit blank nodes for triple predicates, otherwise they will be omitted.
        /// </summary>
        /// <remarks>Defaults to <code>true</code></remarks>
        public bool ProduceGeneralizedRdf = true;

        /// <summary>
        /// Sets the value object embed flag used in the Framing Algorithm
        /// </summary>
        public JsonLdEmbed Embed = JsonLdEmbed.Last;

        /// <summary>
        /// Sets the value explicit inclusion flag used in the Framing Algorithm.
        /// </summary>
        public bool Explicit = false;
        
        /// <summary>
        /// Sets the value omit default flag used in the Framing Algorithm
        /// </summary>
        public bool OmitDefault = false;
        
        /// <summary>
        /// Sets the value require all flag used in the Framing Algorithm
        /// </summary>
        public bool RequireAll = false;
        
        /// <summary>
        /// Instead of framing a merged graph, frame only the default graph.
        /// </summary>
        public bool FrameDefault = false;

        /// <summary>
        /// Removes @id from node objects where the value is a blank node identifier used only once within the document.
        /// </summary>
        public bool PruneBlankNodeIdentifiers = true;
    }
}

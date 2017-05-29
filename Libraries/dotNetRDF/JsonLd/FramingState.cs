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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.JsonLd
{
    internal class FramingState
    {
        public JsonLdEmbed Embed { get; set; }
        public bool ExplicitInclusion { get; set; }
        public bool RequireAll { get; set; }
        public bool OmitDefault { get; set; }
        public JObject GraphMap { get; set; }
        public string GraphName { get; set; }
        public JObject Subjects => GraphMap[GraphName] as JObject;
        public Stack<string> GraphStack { get; set; }
        public JObject Link { get; set; }

        public FramingState(JsonLdProcessorOptions options, JObject graphMap, string graphName)
        {
            Embed = options.Embed;
            ExplicitInclusion = options.Explicit;
            RequireAll = options.RequireAll;
            OmitDefault = options.OmitDefault;
            GraphMap = graphMap;
            GraphName = graphName;
            GraphStack = new Stack<string>();
            Link = new JObject();
        }
    }
}

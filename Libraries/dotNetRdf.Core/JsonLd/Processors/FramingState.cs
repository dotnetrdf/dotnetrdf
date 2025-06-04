/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd.Processors;

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
    public bool Embedded { get; set; }

    private readonly Dictionary<string, Dictionary<string, Tuple<JToken, string>>> _embeds;

    public FramingState(JsonLdProcessorOptions options, JObject graphMap, string graphName)
    {
        Embed = options.Embed;
        Embedded = false;
        ExplicitInclusion = options.Explicit;
        RequireAll = options.RequireAll;
        OmitDefault = options.OmitDefault;
        GraphMap = graphMap;
        GraphName = graphName;
        GraphStack = new Stack<string>();
        Link = new JObject();
        _embeds = new Dictionary<string, Dictionary<string, Tuple<JToken, string>>>();
    }

    public void TrackEmbeddedNodes(bool forceNew)
    {
        if (forceNew || !_embeds.ContainsKey(GraphName))
        {
            _embeds[GraphName] = new Dictionary<string, Tuple<JToken, string>>();
        }
    }

    public bool HasEmbeddedNode(string id)
    {
        return _embeds[GraphName].ContainsKey(id);
    }

    public void AddEmbeddedNode(string id, JToken node, string property)
    {
        if (!_embeds.ContainsKey(GraphName))
        {
            _embeds[GraphName] = new Dictionary<string, Tuple<JToken, string>>();
        }

        _embeds[GraphName][id] = new Tuple<JToken, string>(node, property);
    }

    public Tuple<JToken, string> GetEmbeddedNode(string id)
    {
        if (_embeds.ContainsKey(GraphName) && _embeds[GraphName].ContainsKey(id))
        {
            return _embeds[GraphName][id];
        }

        return null;
    }
}

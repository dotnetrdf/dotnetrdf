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
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model;



internal class TemplateCallImpl : ModuleCallImpl, ITemplateCall
{

    public TemplateCallImpl(INode node, IGraph graph, SpinProcessor spinModel)
        : base(node, graph, spinModel)
    {

    }

    public Dictionary<IArgument, IResource> getArgumentsMap()
    {
        var map = new Dictionary<IArgument, IResource>();
        var template = getTemplate();
        if (template != null)
        {
            foreach (var ad in template.getArguments(false))
            {
                var argProperty = ad.getPredicate();
                if (argProperty != null)
                {
                    var value = getObject(argProperty);
                    if (value != null)
                    {
                        map[ad] = value;
                    }
                }
            }
        }

        return map;
    }


    public Dictionary<IResource, IResource> getArgumentsMapByProperties()
    {
        var map = new Dictionary<IResource, IResource>();
        var template = getTemplate();
        if (template != null)
        {
            foreach (var ad in template.getArguments(false))
            {
                var argProperty = ad.getPredicate();
                if (argProperty != null)
                {
                    var valueS = getObject(argProperty);
                    if (valueS != null)
                    {
                        map[argProperty] = valueS;
                    }
                }
            }
        }

        return map;
    }


    public Dictionary<String, IResource> getArgumentsMapByVarNames()
    {
        var map = new Dictionary<String, IResource>();
        var template = getTemplate();
        if (template != null)
        {
            foreach (var ad in template.getArguments(false))
            {
                var argProperty = ad.getPredicate();
                if (argProperty != null)
                {
                    var varName = ad.getVarName();
                    var valueS = getObject(argProperty);
                    if (valueS != null)
                    {
                        map[varName] = valueS;
                    }
                }
            }
        }
        return map;
    }


    /*override*/ public Dictionary<String, IResource> getInitialBinding()
    {
        var map = new Dictionary<String, IResource>();
        var input = getArgumentsMapByVarNames();
        foreach (var varName in input.Keys)
        {
            var value = input[varName];
            map.Add(varName, value);
        }
        return map;
    }


    override public IModule getModule()
    {
        return getTemplate();
    }


    public String getQueryString()
    {
        // TODO 
        //Dictionary<String, IResource> map = getArgumentsMapByVarNames();
        //StringSparqlPrinter p = new StringSparqlPrinter(new StringBuilder(), map);
        //ITemplate template = getTemplate();
        //p.setUsePrefixes(false);
        //template.getBody().print(p);
        //return p.getString();
        return String.Empty;
    }


    public ITemplate getTemplate()
    {
        var s = getResource(RDF.PropertyType);
        if (s != null && s.isUri())
        {
            return SPINModuleRegistry.getTemplate(s.Uri, s.getModel());
        }
        else
        {
            return null;
        }
    }


    override public void Print(ISparqlPrinter p)
    {
        // TODO Auto-generated method stub

    }

    public SparqlQuery createQueryExecution(IEnumerable<Uri> dataset)
    {
        throw new NotImplementedException();
    }
}
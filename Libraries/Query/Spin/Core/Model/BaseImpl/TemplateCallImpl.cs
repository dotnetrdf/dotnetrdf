/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
/*
 
A C# port of the SPIN API (http://topbraid.org/spin/api/)
an open source Java API distributed by TopQuadrant to encourage the adoption of SPIN in the community. The SPIN API is built on the Apache Jena API and provides the following features: 
 
-----------------------------------------------------------------------------

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class TemplateCallImpl : ModuleCallImpl, ITemplateCallResource
    {
        public TemplateCallImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public Dictionary<IArgumentResource, IResource> getArgumentsMap()
        {
            Dictionary<IArgumentResource, IResource> map = new Dictionary<IArgumentResource, IResource>();
            ITemplateResource template = getTemplate();
            if (template != null)
            {
                foreach (IArgumentResource ad in template.getArguments(false))
                {
                    IResource argProperty = ad.getPredicate();
                    if (argProperty != null)
                    {
                        IResource value = GetObject(argProperty);
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
            Dictionary<IResource, IResource> map = new Dictionary<IResource, IResource>();
            ITemplateResource template = getTemplate();
            if (template != null)
            {
                foreach (IArgumentResource ad in template.getArguments(false))
                {
                    IResource argProperty = ad.getPredicate();
                    if (argProperty != null)
                    {
                        IResource valueS = GetObject(argProperty);
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
            Dictionary<String, IResource> map = new Dictionary<String, IResource>();
            ITemplateResource template = getTemplate();
            if (template != null)
            {
                foreach (IArgumentResource ad in template.getArguments(false))
                {
                    IResource argProperty = ad.getPredicate();
                    if (argProperty != null)
                    {
                        String varName = ad.getVarName();
                        IResource valueS = GetObject(argProperty);
                        if (valueS != null)
                        {
                            map[varName] = valueS;
                        }
                    }
                }
            }
            return map;
        }

        /*override*/

        public Dictionary<String, IResource> getInitialBinding()
        {
            Dictionary<String, IResource> map = new Dictionary<String, IResource>();
            Dictionary<String, IResource> input = getArgumentsMapByVarNames();
            foreach (String varName in input.Keys)
            {
                IResource value = input[varName];
                map.Add(varName, value);
            }
            return map;
        }

        override public IModuleResource getModule()
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

        public ITemplateResource getTemplate()
        {
            IResource s = GetResource(RDF.PropertyType);
            if (s != null && s.IsUri())
            {
                return GetModel().GetTemplate(s.Uri);
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
}
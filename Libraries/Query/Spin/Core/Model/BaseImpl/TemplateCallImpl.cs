/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

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
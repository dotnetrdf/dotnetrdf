/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.system;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query;
using org.topbraid.spin.arq;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public class TemplateCallImpl : ModuleCallImpl, ITemplateCall
    {

        public TemplateCallImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }

        // TODO
        /*override*/ public SparqlQuery createQueryExecution(IEnumerable<Uri> dataset)
        {
            
            IModule template = getModule();
            IQuery query = SPINFactory.asQuery(template.getBody());
            Dictionary<String, INode> initialBindings = new Dictionary<String, INode>();
            Dictionary<IArgument, IResource> args = getArgumentsMap();
            foreach (IArgument arg in args.Keys)
            {
                IResource value = args[arg];
                initialBindings.Add(arg.getVarName(), value);
            }
            return ARQFactory.get().createQueryExecution(query, dataset, initialBindings);
        }


        public Dictionary<IArgument, IResource> getArgumentsMap()
        {
            Dictionary<IArgument, IResource> map = new Dictionary<IArgument, IResource>();
            ITemplate template = getTemplate();
            if (template != null)
            {
                foreach (IArgument ad in template.getArguments(false))
                {
                    IResource argProperty = ad.getPredicate();
                    if (argProperty != null)
                    {
                        IResource value = getObject(argProperty);
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
            ITemplate template = getTemplate();
            if (template != null)
            {
                foreach (IArgument ad in template.getArguments(false))
                {
                    IResource argProperty = ad.getPredicate();
                    if (argProperty != null)
                    {
                        IResource valueS = getObject(argProperty);
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
            ITemplate template = getTemplate();
            if (template != null)
            {
                foreach (IArgument ad in template.getArguments(false))
                {
                    IResource argProperty = ad.getPredicate();
                    if (argProperty != null)
                    {
                        String varName = ad.getVarName();
                        IResource valueS = getObject(argProperty);
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
            Dictionary<String, IResource> map = new Dictionary<String, IResource>();
            Dictionary<String, IResource> input = getArgumentsMapByVarNames();
            foreach (String varName in input.Keys)
            {
                IResource value = input[varName];
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
            IResource s = getResource(RDF.PropertyType);
            if (s != null && s.isUri())
            {
                return SPINModuleRegistry.getTemplate(s.Uri(), s.getModel());
            }
            else
            {
                return null;
            }
        }


        override public void print(IContextualSparqlPrinter p)
        {
            // TODO Auto-generated method stub

        }
    }
}
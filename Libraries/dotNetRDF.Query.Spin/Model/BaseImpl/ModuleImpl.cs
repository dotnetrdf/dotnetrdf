/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model
{
    public class ModuleImpl : AbstractSPINResource, IModule
    {


        public ModuleImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public List<IArgument> getArguments(bool ordered)
        {
            List<IArgument> results = new List<IArgument>();
            IEnumerator<Triple> it = null;
            //JenaUtil.setGraphReadOptimization(true);
            try
            {
                IEnumerable<IResource> classes = getModel().GetAllSuperClasses(this, true);
                foreach (IResource cls in classes)
                {
                    it =cls.listProperties(SPIN.PropertyConstraint).GetEnumerator();
                    while (it.MoveNext())
                    {
                        Triple s = it.Current;
                        addArgumentFromConstraint(s, results);
                    }
                }
            }
            finally
            {
                if (it != null)
                {
                    it.Dispose();
                }
                //JenaUtil.setGraphReadOptimization(false);
            }

            if (ordered)
            {
                results.Sort(delegate(IArgument o1, IArgument o2)
                {
                    IResource p1 = o1.getPredicate();
                    IResource p2 = o2.getPredicate();
                    if (p1 != null && p2 != null)
                    {
                        return RDFUtil.uriComparer.Compare(p1.Uri, p2.Uri);
                    }
                    else
                    {
                        return 0;
                    }
                }
                );
            }

            return results;
        }

        /**
         * 
         * @param constaint is a statement whose subject is a class, and whose predicate is SPIN.constraint
         * @param results
         */
        private void addArgumentFromConstraint(Triple constraint, List<IArgument> results)
        {
            if (constraint.Object is IBlankNode)
            {
                // Optimized case to avoid walking up class hierarchy
                IEnumerator<Triple> types = Resource.Get(constraint.Object, getModel()).listProperties(RDF.PropertyType).GetEnumerator();
                while (types.MoveNext())
                {
                    Triple typeS = types.Current;
                    if (typeS.Object is IUriNode)
                    {
                        if (RDFUtil.sameTerm(SPL.ClassArgument, typeS.Object))
                        {
                            results.Add(SPINFactory.asArgument(Resource.Get(constraint.Object, getModel())));
                        }
                        else if (!RDFUtil.sameTerm(SPL.ClassAttribute,typeS.Object))
                        {
                            if (Resource.Get(typeS.Object, getModel()).hasProperty(RDFS.PropertySubClassOf, SPL.ClassArgument))
                            {
                                results.Add(SPINFactory.asArgument(Resource.Get(constraint.Object, getModel())));
                            }
                        }
                    }
                }
            }
            else if (constraint.Object is IUriNode && Resource.Get(constraint.Object, getModel()).hasProperty(RDFS.PropertySubClassOf, SPL.ClassArgument))
            {
                results.Add(SPINFactory.asArgument(Resource.Get(constraint.Object, getModel())));
            }
        }


        public Dictionary<String, IArgument> getArgumentsMap()
        {
            Dictionary<String, IArgument> results = new Dictionary<String, IArgument>();
            foreach (IArgument argument in getArguments(false))
            {
                IResource property = argument.getPredicate();
                if (property != null)
                {
                    results[property.Uri.ToString().Replace(SP.BASE_URI, "")] = argument;
                }
            }
            return results;
        }


        public ICommand getBody()
        {
            IResource node = null; //ModulesUtil.getBody(this);
            if (node is IResource)
            {
                return SPINFactory.asCommand(node);
            }
            else
            {
                return null;
            }
        }


        public String getComment()
        {
            return getString(RDFS.PropertyComment);
        }


        public bool isAbstract()
        {
            return SPINFactory.isAbstract(this);
        }


        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }
    }
}
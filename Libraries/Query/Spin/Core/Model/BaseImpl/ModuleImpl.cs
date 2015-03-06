using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class ModuleImpl : AbstractSPINResource, IModuleResource
    {


        public ModuleImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }


        public List<IArgumentResource> getArguments(bool ordered)
        {
            List<IArgumentResource> results = new List<IArgumentResource>();
            IEnumerator<Triple> it = null;
            //JenaUtil.setGraphReadOptimization(true);
            try
            {
                IEnumerable<IResource> classes = GetModel().GetAllSuperClasses(this, true);
                foreach (IResource cls in classes)
                {
                    it =cls.ListProperties(SPIN.PropertyConstraint).GetEnumerator();
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
                results.Sort(delegate(IArgumentResource o1, IArgumentResource o2)
                {
                    IResource p1 = o1.getPredicate();
                    IResource p2 = o2.getPredicate();
                    if (p1 != null && p2 != null)
                    {
                        return RDFHelper.uriComparer.Compare(p1.Uri, p2.Uri);
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
        private void addArgumentFromConstraint(Triple constraint, List<IArgumentResource> results)
        {
            if (constraint.Object is IBlankNode)
            {
                // Optimized case to avoid walking up class hierarchy
                IEnumerator<Triple> types = SpinResource.Get(constraint.Object, GetModel()).ListProperties(RDF.PropertyType).GetEnumerator();
                while (types.MoveNext())
                {
                    Triple typeS = types.Current;
                    if (typeS.Object is IUriNode)
                    {
                        if (RDFHelper.SameTerm(SPL.ClassArgument, typeS.Object))
                        {
                            results.Add(ResourceFactory.asArgument(SpinResource.Get(constraint.Object, GetModel())));
                        }
                        else if (!RDFHelper.SameTerm(SPL.ClassAttribute,typeS.Object))
                        {
                            if (SpinResource.Get(typeS.Object, GetModel()).HasProperty(RDFS.PropertySubClassOf, SPL.ClassArgument))
                            {
                                results.Add(ResourceFactory.asArgument(SpinResource.Get(constraint.Object, GetModel())));
                            }
                        }
                    }
                }
            }
            else if (constraint.Object is IUriNode && SpinResource.Get(constraint.Object, GetModel()).HasProperty(RDFS.PropertySubClassOf, SPL.ClassArgument))
            {
                results.Add(ResourceFactory.asArgument(SpinResource.Get(constraint.Object, GetModel())));
            }
        }


        public Dictionary<String, IArgumentResource> getArgumentsMap()
        {
            Dictionary<String, IArgumentResource> results = new Dictionary<String, IArgumentResource>();
            foreach (IArgumentResource argument in getArguments(false))
            {
                IResource property = argument.getPredicate();
                if (property != null)
                {
                    results[property.Uri.ToString().Replace(SP.BASE_URI, "")] = argument;
                }
            }
            return results;
        }


        public ICommandResource getBody()
        {
            IResource node = GetResource(SPIN.PropertyBody);
            if (node is IResource)
            {
                return ResourceFactory.asCommand(node);
            }
            else
            {
                return null;
            }
        }


        public String getComment()
        {
            return GetString(RDFS.PropertyComment);
        }


        public bool isAbstract()
        {
            return ResourceFactory.isAbstract(this);
        }


        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }
    }
}
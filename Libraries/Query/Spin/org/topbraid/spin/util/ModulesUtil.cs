/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using org.topbraid.spin.model;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.util
{

    /**
     * Utilities on SPIN modules.
     * 
     * @author Holger Knublauch
     */
    public class ModulesUtil
    {


        /**
         * Gets the spin:body of a module, including inherited ones if the
         * direct body is null. 
         * @param module  the module to get the body of
         * @return the body or null
         */
        public static IResource getBody(IResource module)
        {
            Triple s = module.getProperty(SPIN.PropertyBody);
            if (s == null)
            {
                return getSuperClassesBody(module, new HashSet<IResource>());
            }
            else
            {
                return Resource.Get(s.Object, module.getModel());
            }
        }


        /**
         * Attempts to find "good" default bindings for a collection of INode values
         * at a given module.  For each argument, this algorithm checks whether each
         * value would match the argument's type.
         * @param module  the module INode to check
         * @param values  the potential values
         * @return a Map of argProperty properties to a subset of the values
         */
        public static Dictionary<IResource, IResource> getPotentialBindings(IModule module, IResource[] values)
        {
            Dictionary<IResource, IResource> results = new Dictionary<IResource, IResource>();
            foreach (IArgument argument in module.getArguments(false))
            {
                IResource argProperty = argument.getPredicate();
                if (argProperty != null)
                {
                    IResource argType = argument.getValueType();
                    if (argType != null)
                    {
                        foreach (IResource value in values)
                        {
                            if (!value.isLiteral())
                            {
                                IResource resource = value;
                                //JenaUtil.hasIndirectType has been wrongly intrepreted
                                if (resource.hasProperty(RDF.PropertyType, argType))
                                {
                                    results[argProperty] = resource;
                                }
                            }
                            else
                            {
                                if (RDFUtil.sameTerm(argType, ((ILiteralNode)value.getSource()).DataType))
                                {
                                    results[argProperty] = value;
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }


        private static IResource getSuperClassesBody(IResource module, HashSet<IResource> reached)
        {
            IEnumerator<Triple> it = module.listProperties(RDFS.PropertySubClassOf).GetEnumerator();
            while (it.MoveNext())
            {
                Triple next = it.Current;
                if (!(next.Object is ILiteralNode))
                {
                    IResource superClass = Resource.Get(next.Object, module.getModel());
                    if (!reached.Contains(superClass))
                    {
                        reached.Add(superClass);
                        Triple s = superClass.getProperty(SPIN.PropertyBody);
                        if (s != null)
                        {
                            it.Dispose();
                            return Resource.Get(s.Object, module.getModel());
                        }
                        else
                        {
                            IResource body = getSuperClassesBody(module, reached);
                            if (body != null)
                            {
                                it.Dispose();
                                return body;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
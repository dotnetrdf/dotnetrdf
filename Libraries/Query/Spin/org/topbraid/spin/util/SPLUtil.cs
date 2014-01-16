/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using org.topbraid.spin.model;
using org.topbraid.spin.model.impl;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.util
{


    /**
     * Utilities related to the spl namespace.
     * 
     * @author Holger Knublauch
     */
    public class SPLUtil
    {

        private static void addDefaultValuesForType(IResource cls, Dictionary<IResource, IResource> results, HashSet<IResource> reached)
        {
            reached.Add(cls);
            SpinProcessor model = cls.getModel();
            IEnumerator<Triple> it = cls.listProperties(SPIN.rule).GetEnumerator();
            while (it.MoveNext())
            {
                Triple s = it.Current;
                if (!(s.Object is ILiteralNode))
                {
                    IResource templateCall = Resource.Get(s.Object, model);
                    if (templateCall.hasProperty(RDF.type, SPL.InferDefaultValue))
                    {
                        IResource predicate = templateCall.getResource(SPL.predicate);
                        if (predicate != null && predicate.isUri())
                        {
                            if (!results.ContainsKey(predicate))
                            {
                                IResource v = templateCall.getResource(SPL.defaultValue);
                                if (v != null)
                                {
                                    results[predicate] = v;
                                }
                            }
                        }
                    }
                }
            }

            foreach (IResource superClass in model.GetAllSuperClasses(cls))
            {
                if (!reached.Contains(superClass))
                {
                    addDefaultValuesForType(superClass, results, reached);
                }
            }
        }


        /**
         * Gets any declared spl:Argument that is attached to the types of a given
         * subject (or its superclasses) via spin:constraint, that has a given predicate
         * as its spl:predicate.
         * @param subject  the instance to get an Argument of
         * @param predicate  the predicate to match
         * @return the Argument or null if none found for that type
         */
        public static IArgument getArgument(IResource subject, IResource predicate)
        {
            HashSet<IResource> reached = new HashSet<IResource>();
            foreach (IResource type in subject.getObjects(RDF.type))
            {
                IArgument arg = getArgumentHelper(type, predicate, reached);
                if (arg != null)
                {
                    return arg;
                }
            }
            return null;
        }


        private static IArgument getArgumentHelper(IResource type, IResource predicate, HashSet<IResource> reached)
        {

            if (reached.Contains(type))
            {
                return null;
            }

            reached.Add(type);

            // Check if current type has a matching spin:constraint declaration
            IEnumerator<Triple> it = type.listProperties(SPIN.constraint).GetEnumerator();
            while (it.MoveNext())
            {
                IResource s = Resource.Get(it.Current.Object, type.getModel());
                if (s.isBlank() &&
                        s.hasProperty(SPL.predicate, predicate) &&
                        s.hasProperty(RDF.type, SPL.Argument))
                {
                    it.Dispose();
                    return (IArgument)s.As(typeof(ArgumentImpl));
                }
            }

            // Walk up superclasses
            foreach (IResource ss in type.getObjects(RDFS.subClassOf).ToList())
            {
                if (!ss.isLiteral())
                {
                    IArgument arg = getArgumentHelper(ss, predicate, reached);
                    if (arg != null)
                    {
                        return arg;
                    }
                }
            }
            return null;
        }


        private static IResource getDefaultValueForType(IResource cls, IResource predicate, HashSet<IResource> reached)
        {
            reached.Add(cls);
            SpinProcessor model = cls.getModel();
            IEnumerator<IResource> it = cls.getObjects(SPIN.rule).GetEnumerator();
            while (it.MoveNext())
            {
                IResource templateCall = it.Current;
                if (!templateCall.isLiteral())
                {
                    if (templateCall.hasProperty(RDF.type, SPL.InferDefaultValue))
                    {
                        if (templateCall.hasProperty(SPL.predicate, predicate))
                        {
                            IResource v = templateCall.getResource(SPL.defaultValue);
                            if (v != null)
                            {
                                it.Dispose();
                                return v;
                            }
                        }
                    }
                }
            }

            foreach (IResource superClass in model.GetAllSuperClasses(cls))
            {
                if (!reached.Contains(superClass))
                {
                    IResource value = getDefaultValueForType(superClass, predicate, reached);
                    if (value != null)
                    {
                        return value;
                    }
                }
            }

            return null;
        }


        /**
         * Creates a Map from Properties to RDFNodes based on declared
         * spl:InferDefaultValues.
         * @param subject
         * @return a Map from Properties to their default values (no null values)
         */
        public static Dictionary<IResource, IResource> getDefaultValues(IResource subject)
        {
            Dictionary<IResource, IResource> results = new Dictionary<IResource, IResource>();
            HashSet<IResource> reached = new HashSet<IResource>();
            foreach (IResource type in subject.getObjects(RDF.type))
            {
                addDefaultValuesForType(type, results, reached);
            }
            return results;
        }


        /**
         * Same as <code>getObject(subject, predicate, false)</code>.
         * @see #getObject(INode, INode, boolean)
         */
        public static IResource getObject(IResource subject, IResource predicate)
        {
            return getObject(subject, predicate, false);
        }


        /**
         * Gets the (first) value of a subject/predicate combination.
         * If no value exists, then it checks whether any spl:InferDefaultValue
         * has been defined for the type(s) of the subject.
         * No need to run inferences first.
         * @param subject  the subject to get the object of
         * @param predicate  the predicate
         * @param includeSubProperties  true to also check for sub-properties of predicate
         * @return the object or null
         */
        public static IResource getObject(IResource subject, IResource predicate, bool includeSubProperties)
        {
            IResource s = subject.getObject(predicate);
            if (s != null)
            {
                return s;
            }
            else
            {
                HashSet<IResource> reached = new HashSet<IResource>();
                foreach (IResource type in subject.getObjects(RDF.type))
                {
                    IResource obj = getDefaultValueForType(type, predicate, reached);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
                if (includeSubProperties)
                {
                    foreach (IResource subProperty in predicate.getModel().GetAllSubProperties(predicate))
                    {
                        IResource value = getObject(subject, subProperty, false);
                        if (value != null)
                        {
                            return value;
                        }
                    }
                }

                return null;
            }
        }


        /**
         * Checks whether a given INode is an instance of spl:Argument (or a subclass
         * thereof.
         * @param resource  the INode to test
         * @return true if resource is an argument
         */
        public static bool isArgument(IResource resource)
        {
            return resource.hasProperty(RDF.type, SPL.Argument);
        }


        /**
         * Checks if a given INode is a defined spl:Argument of a given subject INode.
         * @param subject  the subject
         * @param predicate  the INode to test
         * @return true  if an spl:Argument exists in the type hierarchy of subject
         */
        public static bool isArgumentPredicate(IResource subject, IResource predicate)
        {
            IEnumerator<Triple> args = null;
            IEnumerator<Triple> classes = null;
            //JenaUtil.setGraphReadOptimization(true);
            try
            {
                if (SP.existsModel(subject.getSource().Graph))
                {
                    SpinProcessor model = predicate.getModel();
                    args = model.GetTriplesWithPredicateObject(SPL.predicate, predicate).GetEnumerator();
                    while (args.MoveNext())
                    {
                        IResource arg = Resource.Get(args.Current.Subject, model);
                        if (arg.hasProperty(RDF.type, SPL.Argument))
                        {
                            classes = model.GetTriplesWithPredicateObject(SPIN.constraint, arg).GetEnumerator();
                            while (classes.MoveNext())
                            {
                                IResource cls = Resource.Get(classes.Current.Subject, model);
                                if (subject.hasProperty(RDF.type, cls))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            finally
            {
                if (classes != null)
                {
                    classes.Dispose();
                }
                if (args != null)
                {
                    args.Dispose();
                }
                //JenaUtil.setGraphReadOptimization(false);
            }
        }
    }
}
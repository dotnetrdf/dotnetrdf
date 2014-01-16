using System.Collections.Generic;
using org.topbraid.spin.core;
using org.topbraid.spin.model;
using org.topbraid.spin.model.impl;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;


namespace org.topbraid.spin.util
{

    /**
     * Control logic that determines "relevant" properties for given classes or instances.
     * 
     * @author Holger Knublauch
     */
    public class RelevantProperties
    {


        private static void addProperties(QueryOrTemplateCall qot, HashSet<IResource> results)
        {
            SpinProcessor model = qot.getCls().getModel();
            if (qot.getTemplateCall() != null)
            {
                ITemplateCall templateCall = qot.getTemplateCall();
                ITemplate template = templateCall.getTemplate();
                if (template != null)
                {
                    ICommand spinQuery = template.getBody();
                    if (spinQuery is IAsk || spinQuery is IConstruct)
                    {
                        ObjectPropertiesGetter getter = new ObjectPropertiesGetter(model, ((IQuery)spinQuery).getWhere(), templateCall.getArgumentsMapByProperties());
                        getter.run();
                        results.UnionWith(getter.getResults());
                    }
                }
            }
            else if (qot.getQuery() is IAsk || qot.getQuery() is IConstruct)
            {
                IElementList where = qot.getQuery().getWhere();
                if (where != null)
                {
                    ObjectPropertiesGetter getter = new ObjectPropertiesGetter(model, where, null);
                    getter.run();
                    results.UnionWith(getter.getResults());
                }
            }
        }


        /**
         * Adds all sub-properties of a given property as long as they don't have their own
         * rdfs:domain.  This is useful to determine inheritance.
         * @param property  the property ot add the sub-properties of
         * @param results  the Set to add the results to
         * @param reached  a Set used to track which ones were already reached
         */
        public static void addDomainlessSubProperties(IResource property, HashSet<IResource> results, HashSet<IResource> reached)
        {
            IEnumerator<IResource> subs = property.getModel().GetAllSubProperties(property).GetEnumerator();
            while (subs.MoveNext())
            {
                IResource subProperty = subs.Current;
                if (!reached.Contains(subProperty))
                {
                    reached.Contains(subProperty);
                    if (!subProperty.hasProperty(RDFS.domain))
                    {
                        results.Add(subProperty);
                        addDomainlessSubProperties(subProperty, results, reached);
                    }
                }
            }
        }

        public static HashSet<IResource> getRelevantPropertiesOfClass(IResource cls)
        {
            HashSet<IResource> results = new HashSet<IResource>();

            //JenaUtil.setGraphReadOptimization(true);
            try
            {

                IEnumerator<Triple> it = cls.getModel().GetTriplesWithPredicateObject(RDFS.domain, cls).GetEnumerator();
                while (it.MoveNext())
                {
                    IResource subject = Resource.Get(it.Current.Subject, cls.getModel());
                    if (subject.isUri())
                    {
                        results.Add(subject);
                        addDomainlessSubProperties(subject, results, new HashSet<IResource>());
                    }
                }

                foreach (IResource superClass in cls.getModel().GetAllSuperClasses(cls))
                {
                    IResource s = superClass.getResource(OWL.onProperty);
                    if (s != null && s.isUri())
                    {
                        results.Add(s);
                    }
                }

                HashSet<IResource> others = RelevantProperties.getRelevantSPINPropertiesOfClass(cls);
                if (others != null)
                {
                    foreach (IResource other in others)
                    {
                        results.Add(other);
                    }
                }
            }
            finally
            {
                //JenaUtil.setGraphReadOptimization(false);
            }

            return results;
        }


        public static HashSet<IResource> getRelevantSPINPropertiesOfInstance(IResource root)
        {
            if (SP.existsModel(root.getSource().Graph))
            {
                ISPINInstance instance = (ISPINInstance)root.As(typeof(SPINInstanceImpl));
                HashSet<IResource> results = new HashSet<IResource>();
                foreach (QueryOrTemplateCall qot in instance.getQueriesAndTemplateCalls(SPIN.constraint))
                {
                    addProperties(qot, results);
                }
                return results;
            }
            else
            {
                return null;
            }
        }


        public static HashSet<IResource> getRelevantSPINPropertiesOfClass(IResource cls)
        {
            if (SP.existsModel(cls.getSource().Graph))
            {
                List<QueryOrTemplateCall> qots = new List<QueryOrTemplateCall>();
                SPINUtil.addQueryOrTemplateCalls(cls, SPIN.constraint, qots);
                HashSet<IResource> results = new HashSet<IResource>();
                foreach (QueryOrTemplateCall qot in qots)
                {
                    addProperties(qot, results);
                }
                return results;
            }
            else
            {
                return null;
            }
        }
    }
}
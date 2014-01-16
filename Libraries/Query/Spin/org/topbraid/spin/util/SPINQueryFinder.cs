using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.topbraid.spin.util;
using org.topbraid.spin.model;
using org.topbraid.spin.vocabulary;
using org.topbraid.spin.system;
using org.topbraid.spin.model.update;
using VDS.RDF.Query.Spin.Util;
using org.topbraid.spin.arq;
using VDS.RDF.Query.Spin.Statistics;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.org.topbraid.spin.util
{
    class SPINQueryFinder
    {
        // TODO change the model type to a SPINReasoner
        public static void Add(Dictionary<IResource, List<CommandWrapper>> class2Query, Triple s, SpinProcessor spinModel, bool withClass, bool allowAsk)
        {
            if (!(s.Object is ILiteralNode))
            {
                IResource queryNode = Resource.Get(s.Object, spinModel);
                String spinQueryText = null;
                String label = null;
                ICommand spinCommand = null;
                ITemplate template = null;
                ITemplateCall templateCall = SPINFactory.asTemplateCall(queryNode);
                if (templateCall != null)
                {
                    template = templateCall.getTemplate();
                    if (template != null)
                    {
                        ICommand body = template.getBody();
                        if (body is IConstruct || (allowAsk && body is IAsk))
                        {
                            spinCommand = body;
                        }
                        else if (body is IUpdate)
                        {
                            spinCommand = body;
                        }
                    }
                    spinQueryText = SPINLabels.getLabel(templateCall);
                    label = spinQueryText;
                }
                else
                {
                    spinCommand = SPINFactory.asCommand(queryNode);
                    if (spinCommand != null)
                    {
                        label = spinCommand.getComment();
                    }
                }

                if (spinCommand != null)
                {
                    //TODO do not precompute Sparql commandText
                    String queryString = ARQFactory.get().createCommandString(spinCommand);
                    bool thisUnbound = spinCommand.hasProperty(SPIN.thisUnbound, RDFUtil.TRUE);
                    if (spinQueryText == null)
                    {
                        if (String.IsNullOrWhiteSpace(queryString))
                        {
                            return;
                        }
                        spinQueryText = queryString;
                    }
                    int? thisDepth = null;
                    if (!thisUnbound && withClass &&
                            (spinCommand is IConstruct || spinCommand is IUpdate || spinCommand is IAsk))
                    {
                        thisDepth = SPINUtil.containsThis((ICommandWithWhere)spinCommand);
                        if (thisDepth != null && thisDepth < 2)
                        {
                            queryString = SPINUtil.addThisTypeClause(queryString);
                        }
                    }
                    CommandWrapper wrapper = null;
                    IResource source = templateCall != null ? (IResource)templateCall : (IResource)spinCommand;
                    if (spinCommand is IQuery)
                    {
                        // this may still be useful in case native transactions are supported ?
                        // TODO precompiled query ?is not needed anymore? since we will rewrite queries depending on the Model current context and monitors if NativeTransactions are not supported
                        SparqlParameterizedString commandText = ARQFactory.get().createQuery(queryString);
                        /*TODO Replace a Construct query by an insert into an temporary execution graph. may be done in the QueryWrapper class */
                        if (commandText.IsConstruct || (allowAsk && commandText.IsAsk))
                        {
                            wrapper = new QueryWrapper(commandText, source, spinQueryText, (IQuery)spinCommand, label, s, thisUnbound, (int)thisDepth);
                        }
                    }
                    else if (spinCommand is IUpdate)
                    {
                        SparqlParameterizedString updateRequest = ARQFactory.get().createUpdateRequest(queryString);
                        /*TODO com.hp.hpl.jena.update.Update operation = updateRequest.getOperations().get(0);
                        wrapper = new UpdateWrapper(operation, source, spinQueryText, (IUpdate)spinCommand, label, s, thisUnbound, thisDepth);
                         */
                    }
                    if (wrapper != null)
                    {
                        IResource type = Resource.Get(s.Subject, spinModel);
                        List<CommandWrapper> list = null;
                        if (class2Query.ContainsKey(type))
                        {
                            list = class2Query[type];
                        }
                        if (list == null)
                        {
                            list = new List<CommandWrapper>();
                            class2Query.Add(type, list);
                        }
                        list.Add(wrapper);
                    }

                    if (template != null && wrapper != null)
                    {
                        Dictionary<String, IResource> bindings = templateCall.getArgumentsMapByVarNames();
                        if (bindings.Count > 0)
                        {
                            wrapper.setTemplateBinding(bindings);
                        }
                    }
                }
            }
        }


        //private static Dictionary<Model, Dictionary<IResource, List<CommandWrapper>>> cache = new Dictionary<Model, Dictionary<IResource, List<CommandWrapper>>>();
        /**
         * Gets a Map of QueryWrappers with their associated classes. 
         * @param model  the Model to operate on
         * @param queryModel  the Model to query on (might be different)
         * @param predicate  the predicate such as <code>spin:rule</code>
         * @param withClass  true to also include a SPARQL clause to bind ?this
         *                   (something along the lines of ?this a ?THIS_CLASS) 
         * @param allowAsk  also return ASK queries
         * @return the result Map, possibly empty but not null
         */
        public static Dictionary<IResource, List<CommandWrapper>> GetClass2QueryMap(SpinProcessor spinModel, SpinWrapperDataset queryModel, INode predicate, bool withClass, bool allowAsk)
        {
            Dictionary<IResource, List<CommandWrapper>> class2Query = new Dictionary<IResource, List<CommandWrapper>>();
            DateTime startTime = DateTime.Now;
            IEnumerator<Triple> it = spinModel.GetTriplesWithPredicate(predicate).GetEnumerator();
            for (; it.MoveNext(); )
            {
                Add(class2Query, it.Current, spinModel, withClass, allowAsk);
            }
            return class2Query;
        }
    }
}

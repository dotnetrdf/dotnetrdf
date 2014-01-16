using VDS.RDF.Storage;
using org.topbraid.spin.model;

using System;
using System.Linq;
using VDS.RDF;
using System.Collections.Generic;

using VDS.RDF.Query.Spin;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin.Util;
using org.topbraid.spin.model.update;
using org.topbraid.spin.arq;
namespace org.topbraid.spin.util
{
    /**
     * A utility to convert RDF graphs between the sp:text syntax and SPIN RDF triples.
     * Can be used as a pre-processor of files so that they only use one syntax.
     * 
     * @author Holger Knublauch
     */
    public class SPTextUtil
    {

        /**
         * Adds an sp:text reflecting the SPIN RDF of a given Command.
         * @param command  the SPIN Command to convert
         */
        public static void addSPText(ICommand command)
        {
            /*
            MultiUnion unionGraph = new MultiUnion(new Graph[] {
				command.getModel().getGraph(),
				SPIN.getModel().getGraph()
		});
            unionGraph.setBaseGraph(command.getModel().getGraph());
            Model unionModel = ModelFactory.createModelForGraph(unionGraph);
            */
            // We use the unprefixed strings by default, to avoid potential parsing problems
            String str = ARQFactory.get().createCommandString(command);
            command.AddProperty(SP.text, RDFUtil.CreateLiteralNode(str));
        }


        /**
         * Removes any SPIN RDF syntax triples from a given Model.
         * For example this will remove the sp:where triple tree from an sp:Select,
         * but leave the surrounding sp:Select in place.
         * You may want to call {@link #ensureSPTextExists(Model)} beforehand to make
         * sure that the resulting SPIN resources remain valid.
         * @param model  the Model to manipulate
         */
        public static void deleteSPINRDF(SpinProcessor model)
        {
            foreach (INode type in model.GetAllSubClasses(SP.Query, true))
            {
                foreach (IResource instance in model.GetTriplesWithPredicateObject(RDF.type, type).Select(t => Resource.Get(t.Subject, model)).ToList())
                {
                    ICommand command = SPINFactory.asCommand(instance);
                    foreach (Triple s in command.listProperties().ToList())
                    {
                        if (!RDFUtil.sameTerm(RDF.type, s.Predicate) && !RDFUtil.sameTerm(SP.text, s.Predicate))
                        {
                            deleteWithDependingBNodes(s);
                        }
                    }
                }
            }
        }


        private static void deleteWithDependingBNodes(Triple s)
        {

            // Stop if subject is a bnode with other incoming triples, e.g. a shared variable
            if (s.Subject is IBlankNode)
            {
                IEnumerator<Triple> it = s.Graph.GetTriplesWithObject(s.Subject).GetEnumerator();
                if (it.MoveNext())
                {  // One is expected...
                    if (it.MoveNext())
                    {  // ... but second is one too many
                        it.Dispose();
                        return;
                    }
                }
                it.Dispose();
            }

            if (s.Object is IBlankNode)
            {
                foreach (Triple d in s.Graph.GetTriplesWithSubject(s.Object).ToList())
                {
                    deleteWithDependingBNodes(d);
                }
            }
            s.Graph.Retract(s);
        }


        /**
         * Ensures that each SPIN Command with an sp:text also has the SPIN RDF syntax triples.
         * For example this will create the sp:where triple for all sp:Selects, assuming they
         * do have sp:text triples.
         * @param model  the Model to walk through
         */
        public static void ensureSPINRDFExists(SpinProcessor model)
        {
            return;
            //TODO
            //foreach (IResource instance in model.GetTriplesWithPredicate(SP.text).Select(t => Resource.Get(t.Subject, model)).ToList())
            //{
            //    if (!hasSPINRDF(instance))
            //    {
            //        String text = instance.getString(SP.text);

            //        // Create SPIN RDF triples into a new temp Model
            //        Model baseModel = JenaUtil.createMemoryModel();
            //        MultiUnion unionGraph = new MultiUnion(new Graph[] {
            //            baseModel.getGraph(),
            //            model.getGraph()
            //    });
            //        unionGraph.setBaseGraph(baseModel.getGraph());
            //        baseModel.getGraph().getPrefixMapping().setNsPrefixes(model);
            //        Model tempModel = ModelFactory.createModelForGraph(unionGraph);
            //        ICommand tempCommand;
            //        if (SPINFactory.asCommand(instance) is IUpdate)
            //        {
            //            tempCommand = ARQ2SPIN.parseUpdate(text, tempModel);
            //        }
            //        else
            //        {
            //            tempCommand = ARQ2SPIN.parseQuery(text, tempModel);
            //        }
            //        tempCommand.removeAll(RDF.type);

            //        // Copy all remaining temp triples into old resource, redirecting some triples
            //        foreach (Triple s in baseModel.listStatements().toList())
            //        {
            //            if (s.Subject.Equals(tempCommand))
            //            {
            //                instance.addProperty(s.Predicate, s.Object);
            //            }
            //            else
            //            {
            //                instance.getModel().Add(s);
            //            }
            //        }
            //    }
            //}
        }


        /**
         * Ensures that each SPIN Command (query/update) in a given Model has an sp:text triple.
         * @param model  the Model to manipulate
         */
        public static void ensureSPTextExists(SpinProcessor model)
        {
            foreach (INode type in model.GetAllSubClasses(SP.Query, true))
            {
                foreach (IResource instance in model.GetTriplesWithPredicateObject(RDF.type, type).Select(t => Resource.Get(t.Subject, model)).ToList())
                {
                    ICommand command = SPINFactory.asCommand(instance);
                    if (!instance.hasProperty(SP.text))
                    {
                        addSPText(command);
                    }
                }
            }
        }


        /**
         * Checks if a given SPIN Command has at least one other triple beside the rdf:type, sp:text
         * and spin:thisUnbound triple.  This indicates whether SPIN RDF triples exist. 
         * @param command  the Command to check
         * @return true if the command has SPIN RDF triples
         */
        public static bool hasSPINRDF(IResource command)
        {
            IEnumerator<Triple> it = command.listProperties().GetEnumerator();
            try
            {
                while (it.MoveNext())
                {
                    Triple o = it.Current;
                    if (!RDFUtil.sameTerm(RDF.type, o.Predicate) && !RDFUtil.sameTerm(SP.text, o.Predicate) && !RDFUtil.sameTerm(SPIN.thisUnbound, o.Predicate))
                    {
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                it.Dispose();
            }
        }
    }
}
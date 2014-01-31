using VDS.RDF.Query.Spin.LibraryOntology;
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
{

    public class NamedGraphImpl : ElementImpl, INamedGraph
    {

        public NamedGraphImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getNameNode()
        {
            IResource r = getObject(SP.PropertyGraphNameNode);
            if (r != null)
            {
                IVariable variable = SPINFactory.asVariable(r);
                if (variable != null)
                {
                    return variable;
                }
                else
                {
                    return r;
                }
            }
            else
            {
                return null;
            }
        }


        override public void print(ISparqlFactory p)
        {
            return; 
            //p.printKeyword("GRAPH");
            //p.print(" ");
            //IResource graphNode = getNameNode();
            //IResource graphSubsitutionVar = graphNode.getResource(RDFRuntime.PropertyExecutionBinding);
            //if (p.CurrentSparqlContext == SparqlContext.QueryContext)
            //{
            //    // TODO if the graphNode is a Uri replace it with a variable bound to the graph (if not replaced), its updates (if any) and the subsequent entailment graphs
            //    // otherwise if the target graph variable is replaced by a FILTER pattern
            //    // otherwise print the uri as is
            //    if (graphNode.isUri())
            //    {
            //        //graphNode = p.Dataset().CreateUpdateControlledGraph(graphNode);
            //    }
            //    else { 
            //    }
            //    printVarOrResource(p, graphNode);
            //}
            //else
            //{
            //    if (graphNode.isUri())
            //    {
            //        graphNode = p.Dataset.CreateUpdateControlledGraph(graphNode);
            //    }
            //    else {
            //        if (graphSubsitutionVar ==null) {
            //            // TODO make an Util method since this code should be reused at many places
            //            graphSubsitutionVar = SPINFactory.createVariable(p.Dataset.spinProcessor, "cug_"+ Guid.NewGuid().ToString().Replace("-", ""));
            //            graphNode.AddProperty(RDFRuntime.PropertyExecutionBinding, graphSubsitutionVar);
            //            // TODO voir comment et à qui rattacher les contraintes additionnelles.
            //            // TODO remappedVar.AddProperty(SPINEval.mappingConstraint, graphSubsitutionVar (<dotnetrdf-model:updatesGraph>|<dotnetrdf-model:replacesGraph>) graphNode . 
            //            graphSubsitutionVar.AddProperty(RDFRuntime.PropertyExecutionBindingPattern, SPINFactory.createTriplePattern(getModel(), graphSubsitutionVar, Resource.Get(RDFRuntime.PropertyUpdatesGraph, getModel()), graphNode));
            //        }
            //        graphNode = graphSubsitutionVar;
            //    }
            //    printVarOrResource(p, graphNode);
            //}
            //printNestedElementList(p);
            //// TODO Relocate this at the Where pattern level or nearest Where's nested pattern that uses the variable.
            //if (p.CurrentSparqlContext == SparqlContext.QueryContext && graphSubsitutionVar != null)
            //{
            //    // That is not really sexy
            //    IElement mappingConstraint = SPINFactory.asElement(graphSubsitutionVar.getResource(RDFRuntime.PropertyExecutionBindingPattern));
            //    if (mappingConstraint != null)
            //    {
            //        p.println();
            //        mappingConstraint.print(p);
            //        //remappedVar.clearProperty(SPINEval.mappingConstraint);
            //    }
            //}
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}

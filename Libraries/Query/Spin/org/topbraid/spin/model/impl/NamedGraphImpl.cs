/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.model.visitor;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Spin.Util;
using System;

namespace org.topbraid.spin.model.impl
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


        override public void print(IContextualSparqlPrinter p)
        {
            p.printKeyword("GRAPH");
            p.print(" ");
            IResource graphNode = getNameNode();
            IResource graphSubsitutionVar = graphNode.getResource(SPINRuntime.PropertyExecutionBinding);
            if (p.CurrentSparqlContext == SparqlContext.QueryContext)
            {
                if (graphNode.isUri())
                {
                    // TODO replace this with a variable if the target graph is updatable
                    // otherwise if the target graph is replaced rplaces the uri with the CUG for the graph
                    // otherwise print the uri as is
                    graphNode = p.Dataset().CreateUpdateControlledGraph(graphNode);
                }
                printVarOrResource(p, graphNode);
            }
            else
            {
                // TODO modifier le dataset pour ajouter les CUG nécessaires
                if (graphNode.isUri())
                {
                    graphNode = p.Dataset().CreateUpdateControlledGraph(graphNode);
                }
                else {
                    if (graphSubsitutionVar ==null) {
                        // TODO make an Util method since this code should be reused at many places
                        graphSubsitutionVar = SPINFactory.createVariable(p.Dataset().spinProcessor, "cug_"+ Guid.NewGuid().ToString().Replace("-", ""));
                        graphNode.AddProperty(SPINRuntime.PropertyExecutionBinding, graphSubsitutionVar);
                        // TODO voir comment et à qui rattacher les contraintes additionnelles.
                        // TODO remappedVar.AddProperty(SPINEval.mappingConstraint, graphSubsitutionVar (<dotnetrdf-model:updatesGraph>|<dotnetrdf-model:replacesGraph>) graphNode . 
                        graphSubsitutionVar.AddProperty(SPINRuntime.PropertyExecutionBindingPattern, SPINFactory.createTriplePattern(getModel(), graphSubsitutionVar, Resource.Get(SPINRuntime.PropertyUpdatesGraph, getModel()), graphNode));
                    }
                    graphNode = graphSubsitutionVar;
                }
                printVarOrResource(p, graphNode);
            }
            printNestedElementList(p);
            // TODO Relocate this at the Where pattern level or nearest Where's nested pattern that uses the variable.
            if (p.CurrentSparqlContext == SparqlContext.QueryContext && graphSubsitutionVar != null)
            {
                // That is not really sexy
                IElement mappingConstraint = SPINFactory.asElement(graphSubsitutionVar.getResource(SPINRuntime.PropertyExecutionBindingPattern));
                if (mappingConstraint != null)
                {
                    p.println();
                    mappingConstraint.print(p);
                    //remappedVar.clearProperty(SPINEval.mappingConstraint);
                }
            }
        }


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }
    }
}

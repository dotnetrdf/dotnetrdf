/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace VDS.RDF.Query.Spin.Model.visitor
{

    /**
     * Basic, "empty" implementation of ElementVisitor.
     * 
     * @author Holger Knublauch
     */
    public abstract class AbstractElementVisitor : IElementVisitor
    {


        public virtual void visit(IBind let)
        {
        }


        public virtual void visit(IElementList elementList)
        {
        }


        public virtual void visit(IExists exists)
        {
        }


        public virtual void visit(IFilter filter)
        {
        }


        public virtual void visit(IMinus minus)
        {
        }


        public virtual void visit(INamedGraph namedGraph)
        {
        }


        public virtual void visit(INotExists notExists)
        {
        }


        public virtual void visit(IOptional optional)
        {
        }


        public virtual void visit(IService service)
        {
        }


        public virtual void visit(ISubQuery subQuery)
        {
        }


        public virtual void visit(ITriplePath triplePath)
        {
        }


        public virtual void visit(ITriplePattern triplePattern)
        {
        }


        public virtual void visit(IUnion union)
        {
        }


        public virtual void visit(IValues values)
        {
        }
    }
}
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace VDS.RDF.Query.Spin.Model.visitor
{
    /**
     * An interface to visit the various kinds of Elements.
     * 
     * @author Holger Knublauch
     */
    public interface IElementVisitor
    {

        void visit(IBind bind);


        void visit(IElementList elementList);


        void visit(IExists exists);


        void visit(IFilter filter);


        void visit(IMinus minus);


        void visit(INamedGraph namedGraph);


        void visit(INotExists notExists);


        void visit(IOptional optional);


        void visit(IService service);


        void visit(ISubQuery subQuery);


        void visit(ITriplePath triplePath);


        void visit(ITriplePattern triplePattern);


        void visit(IUnion union);


        void visit(IValues values);
    }
}
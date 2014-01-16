/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF;
using org.topbraid.spin.util;
using org.topbraid.spin.system;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.vocabulary;
using System;

namespace org.topbraid.spin.model.visitor
{


    /**
     * A utility that can be used to traverse all TriplePatterns under a given
     * root Element.  This also traverses function calls and simulates the
     * bindings of those function calls if a Function has a registered body. 
     * 
     * @author Holger Knublauch
     */
    public abstract class AbstractTriplesVisitor
    {

        // Needed to remember the bindings of a FunctionCall so that they can be substituted
        private Dictionary<IResource, IResource> _bindings;

        private IElement _element;


        public AbstractTriplesVisitor(IElement element, Dictionary<IResource, IResource> initialBindings)
        {
            this._bindings = initialBindings;
            this._element = element;
        }


        public void run()
        {
            ElementWalker walker = new ElementWalker(new MyElementVisitor(this, _bindings), new MyExpressionVisitor(this, _bindings));
            _element.visit(walker);
        }


        /**
         * Will be called on each TriplePattern.
         * @param triplePattern  the TriplePattern
         */
        protected abstract void handleTriplePattern(ITriplePattern triplePattern, Dictionary<IResource, IResource> bindings);


        // This visitor collects the relevant predicates
        private class MyElementVisitor : AbstractElementVisitor
        {
            private Dictionary<IResource, IResource> _enclosedBindings;
            private AbstractTriplesVisitor _instance;

            public MyElementVisitor(AbstractTriplesVisitor instance, Dictionary<IResource, IResource> bindings)
            {
                _instance = instance;
                _enclosedBindings = bindings;
            }

            public void visit(ITriplePattern triplePattern)
            {
                _instance.handleTriplePattern(triplePattern, _enclosedBindings);
            }
        }


        // This visitor walks into SPIN Function calls 
        private class MyExpressionVisitor : AbstractExpressionVisitor
        {
            private HashSet<IFunctionCall> reachedFunctionCalls = new HashSet<IFunctionCall>();

            private AbstractTriplesVisitor _instance;
            private Dictionary<IResource, IResource> _enclosedBindings;

            public MyExpressionVisitor(AbstractTriplesVisitor instance, Dictionary<IResource, IResource> bindings)
            {
                _instance = instance;
                _enclosedBindings = bindings;
            }

            public void visit(IFunctionCall functionCall)
            {
                IResource function = functionCall.getFunction();
                if (function != null && function.isUri() && !reachedFunctionCalls.Contains(functionCall))
                {
                    reachedFunctionCalls.Add(functionCall);
                    IFunction f = SPINModuleRegistry.getFunction(function.Uri(), null);
                    if (f != null)
                    {
                        IResource bodyS = f.getResource(SPIN.PropertyBody);
                        if (bodyS != null)
                        {
                            Dictionary<IResource, IResource> oldBindings = _enclosedBindings;
                            _enclosedBindings = functionCall.getArgumentsMap();
                            if (oldBindings != null)
                            {
                                Dictionary<String, IResource> varNamesBindings = SPINUtil.mapProperty2VarNames(oldBindings);
                                SPINUtil.applyBindings(_enclosedBindings, varNamesBindings);
                            }
                            IQuery spinQuery = SPINFactory.asQuery(bodyS);
                            IElementList where = spinQuery.getWhere();
                            if (where != null)
                            {
                                ElementWalker walker = new ElementWalker(new MyElementVisitor(_instance, _enclosedBindings), this);
                                where.visit(walker);
                            }
                            _enclosedBindings = oldBindings;
                        }
                    }
                }
            }
        }
    }
}
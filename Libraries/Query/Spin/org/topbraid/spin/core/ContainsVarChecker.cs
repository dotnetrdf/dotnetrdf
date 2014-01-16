/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using org.topbraid.spin.model;
using org.topbraid.spin.model.impl;
using org.topbraid.spin.model.update;
using org.topbraid.spin.model.visitor;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.core
{
    /**
     * Can be used to check whether a given SPIN Query contains a certain
     * variable, either in the WHERE clause or the CONSTRUCT template(s).
     * 
     * @author Holger Knublauch
     */
    public class ContainsVarChecker
    {

        private int result;

        private INode var;

        private class LocalElementVisitor : AbstractElementVisitor
        {

            private INode _var;
            private ContainsVarChecker _checker;

            public LocalElementVisitor(ContainsVarChecker checker, INode var)
            {
                _var = var;
                _checker = checker;

            }

            override public void visit(ITriplePath triplePath)
            {
                if (RDFUtil.sameTerm(_var, triplePath.getObject()) ||
                    RDFUtil.sameTerm(_var, triplePath.getSubject()))
                {
                    _checker.setResult();
                }
            }

            override public void visit(ITriplePattern triplePattern)
            {
                if (_checker.containsVar(triplePattern))
                {
                    _checker.setResult();
                }
            }
        }
        private IElementVisitor el;

        private class LocalExpressionVisitor : AbstractExpressionVisitor
        {

            private INode _var;
            private ContainsVarChecker _checker;

            public LocalExpressionVisitor(ContainsVarChecker checker, INode var)
            {
                _var = var;
                _checker = checker;

            }
            override public void visit(IVariable variable)
            {
                if (RDFUtil.sameTerm(_var, variable))
                {
                    _checker.setResult();
                }
            }
        }
        private IExpressionVisitor ex = null;

        private ElementWalkerWithDepth walker;


        public ContainsVarChecker(INode var)
        {
            this.var = var;
            el = new LocalElementVisitor(this, var);
            ex = new LocalExpressionVisitor(this, var);
        }


        /**
         * Tries to find a usage of the variable and returns the maximum depth of the relevant
         * element inside of the structure.
         * @param command  the Command to start traversal with
         * @return an int >= 0 or null if not found at all
         */
        public int checkDepth(ICommandWithWhere command)
        {

            if (command is IConstruct)
            {
                // Check head of Construct
                foreach (ITripleTemplate template in ((IConstruct)command).getTemplates())
                {
                    if (containsVar(template))
                    {
                        result = 0;
                    }
                }
            }
            else if (command is IModify)
            {
                IResource modify = command;
                if (templateContainsVar(modify.getObject(SP.PropertyInsertPattern)))
                {
                    result = 0;
                }
                if (templateContainsVar(modify.getObject(SP.PropertyDeletePattern)))
                {
                    result = 0;
                }
            }

            IElementList where = command.getWhere();
            if (where != null)
            {
                walker = new ElementWalkerWithDepth(el, ex);
                walker.visit(where);
            }

            return result;
        }


        private bool containsVar(ITriple triple)
        {
            return RDFUtil.sameTerm(var, triple.getObject()) ||
                RDFUtil.sameTerm(var, triple.getPredicate()) ||
                RDFUtil.sameTerm(var, triple.getSubject());
        }


        private void setResult()
        {
            if (result == null)
            {
                result = walker.getDepth();
            }
            else
            {
                result = Math.Max(result, walker.getDepth());
            }
        }


        private bool templateContainsVar(IResource list)
        {
            if (list != null && !list.isLiteral())
            {
                IEnumerator<IResource> nodes = list.AsList().GetEnumerator();
                while (nodes.MoveNext())
                {
                    IResource node = nodes.Current;
                    if (node.hasProperty(RDF.type, SP.ClassNamedGraph))
                    {
                        INamedGraph namedGraph = (INamedGraph)node.As(typeof(NamedGraphImpl));
                        foreach (IElement element in namedGraph.getElements())
                        {
                            if (element is ITriple)
                            {
                                if (containsVar((ITriple)element))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (containsVar((ITripleTemplate)node.As(typeof(TripleTemplateImpl))))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
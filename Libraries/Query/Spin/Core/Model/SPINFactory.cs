/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
/*
 
A C# port of the SPIN API (http://topbraid.org/spin/api/)
an open source Java API distributed by TopQuadrant to encourage the adoption of SPIN in the community. The SPIN API is built on the Apache Jena API and provides the following features: 
 
-----------------------------------------------------------------------------

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;


// TODO is this really needed ?
namespace VDS.RDF.Query.Spin.Model
{

    // TODO rename this as SPINUtil and relocate into Core

    /**
     * The singleton that is used to convert plain Jena objects into
     * SPIN API resources, and to do corresponding tests.
     * 
     * @author Holger Knublauch
     */
    //@SuppressWarnings("deprecation")

    public class SPINFactory
    {

        /**
         * Attempts to cast a given INode into an Aggregation.
         * Resources that have an aggregation type as their rdf:type
         * are recognized as well-formed aggregations.
         * @param resource  the INode to cast
         * @return the Aggregation or null if INode is not a well-formed aggregation
         */
        public static IAggregationResource asAggregation(IResource resource)
        {
            if (resource == null) return null;
            IEnumerator<IResource> it = resource.GetObjects(RDF.PropertyType).GetEnumerator();
            //JenaUtil.setGraphReadOptimization(true);
            try
            {
                while (it.MoveNext())
                {
                    IResource type = it.Current;
                    if (type.IsUri())
                    {
                        if (Aggregations.getName((IResource)type) != null)
                        {
                            it.Dispose();
                            return new AggregationImpl(resource, resource.GetModel());
                        }
                    }
                }
            }
            finally
            {
                //JenaUtil.setGraphReadOptimization(false);
            }
            return null;
        }


        public static IArgumentResource asArgument(IResource resource)
        {
            if (resource == null) return null;
            return new ArgumentImpl(resource, resource.GetModel());
        }

        /**
         * Attempts to cast a given INode into the most specific
         * subclass of Command, esp Update or Query.
         * @param resource  the INode to cast
         * @return resource cast into the best possible type or null
         */
        public static ICommandResource asCommand(IResource resource)
        {
            if (resource == null) return null;
            IQueryResource query = asQuery(resource);
            if (query != null)
            {
                return query;
            }
            else
            {
                return asUpdate(resource);
            }
        }


        /**
         * Checks whether a given INode represents a SPARQL element, and returns
         * an instance of a subclass of Element if so.
         * @param resource  the INode to check
         * @return INode as an Element or null if resource is not an element
         */
        public static IElementResource asElement(IResource resource)
        {
            if (resource == null) return null;
            /*sealed*/
            ITriplePatternResource triplePattern = asTriplePattern(resource);
            if (triplePattern != null)
            {
                return triplePattern;
            }
            else if (resource.CanAs(SP.ClassTriplePath))
            {
                return (ITriplePathResource)resource.As(typeof(TriplePathImpl));
            }
            else if (resource.CanAs(SP.ClassFilter))
            {
                return (IFilterResource)resource.As(typeof(FilterImpl));
            }
            else if (resource.CanAs(SP.ClassBind))
            {
                return (IBindResource)resource.As(typeof(BindImpl));
            }
            else if (resource.CanAs(SP.ClassOptional))
            {
                return (IOptionalResource)resource.As(typeof(OptionalImpl));
            }
            else if (resource.CanAs(SP.ClassNamedGraph))
            {
                return (INamedGraphResource)resource.As(typeof(NamedGraphImpl));
            }
            else if (resource.CanAs(SP.ClassMinus))
            {
                return (IMinusResource)resource.As(typeof(MinusImpl));
            }
            else if (resource.CanAs(SP.ClassExists))
            {
                return (IExistsResource)resource.As(typeof(ExistsImpl));
            }
            else if (resource.CanAs(SP.ClassNotExists))
            {
                return (INotExistsResource)resource.As(typeof(NotExistsImpl));
            }
            else if (resource.CanAs(SP.ClassService))
            {
                return (IServiceResource)resource.As(typeof(ServiceImpl));
            }
            else if (resource.CanAs(SP.ClassSubQuery))
            {
                return (ISubQueryResource)resource.As(typeof(SubQueryImpl));
            }
            else if (resource.CanAs(SP.ClassUnion))
            {
                return (IUnionResource)resource.As(typeof(UnionImpl));
            }
            else if (resource.CanAs(SP.ClassValues))
            {
                return (IValuesResource)resource.As(typeof(ValuesImpl));
            }
            else if (isElementList(resource))
            {
                return (IElementListResource)resource.As(typeof(ElementListImpl));
            }
            else
            {
                return null;
            }
        }


        /**
         * Returns the most specific Java instance for a given INode.
         * If the node is an aggregation, it will be returned as instance of
         * Aggregation.
         * If the node is a function call, it will be returned as instance of
         * FunctionCall.
         * If it's a Variable, the Variable will be returned.
         * Otherwise the node itself will be returned.
         * @param node  the node to cast
         * @return node or node as a Function or Variable
         */
        public static IResource asExpression(IResource resource)
        {
            if (resource == null) return null;
            if (resource is INode)
            {
                IVariableResource var = SPINFactory.asVariable(resource);
                if (var != null)
                {
                    return var;
                }
                IAggregationResource aggr = SPINFactory.asAggregation((IResource)resource);
                if (aggr != null)
                {
                    return aggr;
                }
                IFunctionCallResource functionCall = SPINFactory.asFunctionCall((IResource)resource);
                if (functionCall != null)
                {
                    return functionCall;
                }
            }
            return resource;
        }


        /**
         * Converts a given INode into a Function instance.
         * No other tests are done.
         * @param resource  the INode to convert
         * @return the Function
         */
        public static IFunctionResource asFunction(IResource resource)
        {
            if (resource == null) return null;
            if (resource.CanAs(SPIN.ClassFunction)) return (IFunctionResource)resource.As(typeof(FunctionImpl));
            return null;
        }


        /**
         * Checks if a given INode might represent a Function call, and if
         * yes returns the resource as Function.  The condition here is fairly
         * general: a function call must be a blank node with an rdf:type triple
         * where the type triple's object is a URI resource.  It is generally
         * assumed that this function is called after other options have been
         * exhausted.  For example, in order to test whether a resource is a
         * variable or a function call, the variable test must be done first
         * as it is more specific than these test conditions 
         * @param resource  the INode to test
         * @return resource as a Function or null if resource cannot be cast
         */
        public static IFunctionCallResource asFunctionCall(IResource resource)
        {
            if (resource == null) return null;
            if (resource is IBlankNode)
            {
                IResource t = resource.GetResource(RDF.PropertyType);
                if (t != null && !RDFHelper.SameTerm(SP.ClassVariable,t))
                {
                    return (IFunctionCallResource)resource.As(typeof(FunctionCallImpl));
                }
            }
            return null;
        }


        /**
         * Checks if a given INode is a SPIN query, and returns an
         * instance of a subclass of Query if so. 
         * @param resource  the INode to test
         * @return resource as a Query or null
         */
        public static IQueryResource asQuery(IResource resource)
        {
            if (resource == null) return null;
            if (resource.CanAs(SP.ClassSelect))
            {
                return (IQueryResource)resource.As(typeof(SelectImpl));
            }
            else if (resource.CanAs(SP.ClassConstruct))
            {
                return (IQueryResource)resource.As(typeof(ConstructImpl));
            }
            else if (resource.CanAs(SP.ClassAsk))
            {
                return (IQueryResource)resource.As(typeof(AskImpl));
            }
            else if (resource.CanAs(SP.ClassDescribe))
            {
                return (IQueryResource)resource.As(typeof(DescribeImpl));
            }
            else
            {
                return null;
            }
        }


        /**
         * Converts a given INode into a Template instance.
         * No other tests are done.
         * @param resource  the INode to convert
         * @return the Template
         */
        public static ITemplateResource asTemplate(IResource resource)
        {
            if (resource == null) return null;
            return (ITemplateResource)resource.As(typeof(TemplateImpl));
        }


        /**
         * Checks whether a given INode can be cast into TemplateCall, and returns
         * it as a TemplateCall instance if so.
         * @param node  the node to convert
         * @return an instance of TemplateCall or null
         */
        public static ITemplateCallResource asTemplateCall(IResource resource)
        {
            if (resource == null) return null;
            if (!resource.IsLiteral())
            {
                IResource t= resource.GetResource(RDF.PropertyType);
                if (t!=null && t.IsUri())
                {
                    ITemplateResource template =  t.GetModel().GetTemplate(t.Uri);
                    if (template != null)
                    {
                        return (ITemplateCallResource)resource.As(typeof(TemplateCallImpl));
                    }
                }
            }
            return null;
        }


        /**
         * Checks whether a given INode can be converted into a TriplePattern, and if yes,
         * returns an instance of TriplePattern.
         * @param node  the node to test
         * @return node as TriplePattern or null
         */
        public static ITriplePatternResource asTriplePattern(IResource resource)
        {
            if (resource == null) return null;
            if (resource.HasProperty(SP.PropertyPredicate))
            {
                return new TriplePatternImpl(resource, resource.GetModel());
            }
            else
            {
                return null;
            }
        }


        /**
         * Checks if a given INode is a subclass of sp:Update and
         * casts it into the most specific Java class possible.
         * @param resource  the INode to cast
         * @return the Update or null if resource cannot be cast
         */
        public static IUpdateResource asUpdate(IResource resource)
        {
            if (resource == null) return null;
            if (resource.CanAs(SP.ClassModify))
            {
                return (IModifyResource)resource.As(typeof(ModifyImpl));
            }
            else if (resource.CanAs(SP.ClassClear))
            {
                return (IClearResource)resource.As(typeof(ClearImpl));
            }
            else if (resource.CanAs(SP.ClassCreate))
            {
                return (ICreateResource)resource.As(typeof(CreateImpl));
            }
            else if (resource.CanAs(SP.ClassDeleteData))
            {
                return (IDeleteDataResource)resource.As(typeof(DeleteDataImpl));
            }
            else if (resource.CanAs(SP.ClassDeleteWhere))
            {
                return (IDeleteWhereResource)resource.As(typeof(DeleteWhereImpl));
            }
            else if (resource.CanAs(SP.ClassDrop))
            {
                return (IDropResource)resource.As(typeof(DropImpl));
            }
            else if (resource.CanAs(SP.ClassInsertData))
            {
                return (IInsertDataResource)resource.As(typeof(InsertDataImpl));
            }
            else if (resource.CanAs(SP.ClassLoad))
            {
                return (ILoadResource)resource.As(typeof(LoadImpl));
            }
            else if (resource.CanAs(SP.ClassDelete))
            {
                return (IDeleteResource)resource.As(typeof(DeleteImpl));
            }
            else if (resource.CanAs(SP.ClassInsert))
            {
                return (IInsertResource)resource.As(typeof(InsertImpl));
            }
            else
            {
                return null;
            }
        }


        /**
         * Checks whether a given INode can be cast into a Variable and - if yes -
         * converts it into an instance of Variable.  The INode must have a value
         * for spin:varName.
         * @param node  the node to check
         * @return resource as a Variable or null
         */
        public static IVariableResource asVariable(IResource resource)
        {
            if (resource == null) return null;
            if (resource.HasProperty(SP.PropertyVarName))
            {
                return (IVariableResource)resource.As(typeof(VariableImpl));
            }
            else
            {
                return null;
            }
        }


        /**
         * Creates an spl:Argument with a given property and value type.
         * The new Argument resource will be a blank node in a given Model.
         * @param model  the Model
         * @param argProperty  the property or null
         * @param argType  the value type or null
         * @param optional  true if the Argument shall be optional
         * @return the new Argument
         */
        public static IArgumentResource createArgument(SpinProcessor model, INode argProperty, INode argType, bool optional)
        {
            IArgumentResource a = (IArgumentResource)model.CreateResource(SPL.ClassArgument).As(typeof(ArgumentImpl));
            if (argProperty != null)
            {
                a.AddProperty(SPL.PropertyPredicate, argProperty);
            }
            if (argType != null)
            {
                a.AddProperty(SPL.PropertyValueType, argType);
            }
            if (optional)
            {
                a.AddProperty(SPL.PropertyOptional, RDFHelper.TRUE);
            }
            return a;
        }


        /**
         * Creates a new spl:Attribute as a blank node in a given Model.
         * @param model  the Model to create the attribute in
         * @param argProperty  the predicate or null
         * @param argType  the value type or null
         * @param minCount  the minimum cardinality or null
         * @param maxCount  the maximum cardinality or null
         * @return a new Attribute
         */
        public static IAttributeResource createAttribute(SpinProcessor model, INode argProperty, INode argType, int minCount, int maxCount)
        {
            IAttributeResource a =(IAttributeResource) model.CreateResource(SPL.ClassAttribute).As(typeof(AttributeImpl));
            if (argProperty != null)
            {
                a.AddProperty(SPL.PropertyPredicate, argProperty);
            }
            if (argType != null)
            {
                a.AddProperty(SPL.PropertyValueType, argType);
            }
            if (minCount != null)
            {
                a.AddProperty(SPL.PropertyMinCount, RDFHelper.CreateLiteralNode(minCount.ToString(), XSD.DatatypeInt.Uri));
            }
            if (maxCount != null)
            {
                a.AddProperty(SPL.PropertyMaxCount, RDFHelper.CreateLiteralNode(maxCount.ToString(), XSD.DatatypeInt.Uri));
            }
            return a;
        }


        /**
         * Creates an Ask query for a given WHERE clause.
         * @param model  the Model to create the Ask (blank node) in
         * @param where  the elements of the WHERE clause
         * @return the new Ask query
         */
        public static IAskResource createAsk(SpinProcessor model, IElementListResource where)
        {
            IAskResource ask = (IAskResource)model.CreateResource(SP.ClassAsk).As(typeof(AskImpl));
            ask.AddProperty(SP.PropertyWhere, where);
            return ask;
        }


        /**
         * Creates a Bind in a given Model as a blank node.
         * @param model  the Model to create the Bind in
         * @param variable  the Variable to assign
         * @param expression  the expression
         * @return a new Bind instance
         */
        public static IBindResource createBind(SpinProcessor model, IVariableResource variable, INode expression)
        {
            IBindResource bind = (IBindResource)model.CreateResource(SP.ClassBind).As(typeof(BindImpl));
            if (variable != null)
            {
                bind.AddProperty(SP.PropertyVariable, variable);
            }
            if (expression != null)
            {
                bind.AddProperty(SP.PropertyExpression, expression);
            }
            return bind;
        }


        /**
         * Creates a new ElementList in a given Model.
         * @param model  the Model to create the ElementList in
         * @param elements  the elements (may be empty)
         * @return a new ElementList (may be rdf:nil)
         */
        public static IElementListResource createElementList(SpinProcessor model, IElementResource[] elements)
        {
            if (elements.Length > 0)
            {
                return (IElementListResource)model.CreateList(elements).As(typeof(ElementListImpl));
            }
            else
            {
                return (IElementListResource)SpinResource.Get(RDF.Nil, model).As(typeof(ElementListImpl));
            }
        }


        /**
         * Creates a new ElementList in a given Model.
         * @param model  the Model to create the ElementList in
         * @param elements  the elements (may be empty)
         * @return a new ElementList (may be rdf:nil)
         */
        public static IElementListResource createElementList(SpinProcessor model, IEnumerator<IElementResource> elements)
        {
            if (elements.MoveNext())
            {
                elements.Reset();
                return (IElementListResource)model.CreateList(elements).As(typeof(ElementListImpl));
            }
            else
            {
                return (IElementListResource)SpinResource.Get(RDF.Nil, model).As(typeof(ElementListImpl));
            }
        }


        /**
         * Creates a new Exists as a blank node in a given Model.
         * @param model  the Model to create the EXISTS in
         * @param elements  the elements of the EXISTS
         * @return a new Exists
         */
        public static IExistsResource createExists(SpinProcessor model, IElementListResource elements)
        {
            IExistsResource notExists = (IExistsResource)model.CreateResource(SP.ClassExists).As(typeof(ExistsImpl));
            notExists.AddProperty(SP.PropertyElements, elements);
            return notExists;
        }


        /**
         * Creates a Filter from a given expression.
         * @param model  the Model to create the (blank node) Filter in
         * @param expression  the expression node (not null)
         * @return a new Filter
         */
        public static IFilterResource createFilter(SpinProcessor model, INode expression)
        {
            IFilterResource filter = (IFilterResource)model.CreateResource(SP.ClassFilter).As(typeof(FilterImpl));
            filter.AddProperty(SP.PropertyExpression, expression);
            return filter;
        }


        /**
         * Creates a new Function call, which is basically an instance of the
         * function's class.
         * @param model  the Model to create the function call in
         * @param function  the function class (must be a URI resource)
         * @return a new instance of function
         */
        public static IFunctionCallResource createFunctionCall(SpinProcessor model, INode function)
        {
            return (IFunctionCallResource)model.CreateResource(function).As(typeof(FunctionCallImpl));
        }


        /**
         * Creates a new Minus as a blank node in a given Model.
         * @param model  the Model to create the MINUS in
         * @param elements  the elements of the MINUS
         * @return a new Minus
         */
        public static IMinusResource createMinus(SpinProcessor model, IElementListResource elements)
        {
            IMinusResource minus = (IMinusResource)model.CreateResource(SP.ClassMinus).As(typeof(MinusImpl));
            minus.AddProperty(SP.PropertyElements, elements);
            return minus;
        }


        /**
         * Creates a new NamedGraph element as a blank node in a given Model.
         * @param model  the Model to generate the NamedGraph in
         * @param graphNameNode  the URI resource of the graph name
         * @param elements  the elements in the NamedGraph
         * @return a new NamedGraph
         */
        public static INamedGraphResource createNamedGraph(SpinProcessor model, INode graphNameNode, IResource elements)
        {
            INamedGraphResource result =(INamedGraphResource) model.CreateResource(SP.ClassNamedGraph).As(typeof(NamedGraphImpl));
            result.AddProperty(SP.PropertyGraphNameNode, graphNameNode);
            result.AddProperty(SP.PropertyElements, elements);
            return result;
        }


        /**
         * Creates a new NotExists as a blank node in a given Model.
         * @param model  the Model to create the NOT EXISTS in
         * @param elements  the elements of the NOT EXISTS
         * @return a new NotExists
         */
        public static INotExistsResource createNotExists(SpinProcessor model, IElementListResource elements)
        {
            INotExistsResource notExists = (INotExistsResource)model.CreateResource(SP.ClassNotExists).As(typeof(NotExistsImpl));
            notExists.AddProperty(SP.PropertyElements, elements);
            return notExists;
        }


        /**
         * Creates a new Optional as a blank node in a given Model. 
         * @param model  the Model to create the OPTIONAL in
         * @param elements  the elements of the OPTIONAL
         * @return a new Optional
         */
        public static IOptionalResource createOptional(SpinProcessor model, IElementListResource elements)
        {
            IOptionalResource optional = (IOptionalResource)model.CreateResource(SP.ClassOptional).As(typeof(OptionalImpl));
            optional.AddProperty(SP.PropertyElements, elements);
            return optional;
        }


        public static IServiceResource createService(SpinProcessor model, INode serviceURI, IElementListResource elements)
        {
            IServiceResource service = (IServiceResource)model.CreateResource(SP.ClassService).As(typeof(ServiceImpl));
            service.AddProperty(SP.PropertyServiceURI, serviceURI);
            service.AddProperty(SP.PropertyElements, elements);
            return service;
        }


        /**
         * Creates a new SubQuery as a blank node in a given Model.
         * @param model  the Model to create the SubQuery in
         * @param subQuery  the nested query
         * @return a new SubQuery
         */
        public static ISubQueryResource createSubQuery(SpinProcessor model, IQueryResource subQuery)
        {
            ISubQueryResource result = (ISubQueryResource)model.CreateResource(SP.ClassSubQuery).As(typeof(SubQueryImpl));
            result.AddProperty(SP.PropertyQuery, subQuery);
            return result;
        }


        /**
         * Creates a new TemplateCall as a blank node instance of a given template.
         * @param model  the Model to create a template call in
         * @param template  the template class
         * @return the new TemplateCall or null
         */
        public static ITemplateCallResource createTemplateCall(SpinProcessor model, INode template)
        {
            ITemplateCallResource templateCall = (ITemplateCallResource)model.CreateResource(template).As(typeof(TemplateCallImpl));
            return templateCall;
        }


        /**
         * Creates a new TriplePath as a blank node in a given Model.
         * @param model  the Model to create the path in
         * @param subject  the subject (not null)
         * @param path  the path (not null)
         * @param object  the object (not null)
         * @return a new TriplePath
         */
        public static ITriplePathResource createTriplePath(SpinProcessor model, INode subject, INode path, INode obj)
        {
            ITriplePathResource triplePath = (ITriplePathResource)model.CreateResource(SP.ClassTriplePath).As(typeof(TriplePathImpl));
            triplePath.AddProperty(SP.PropertySubject, subject);
            triplePath.AddProperty(SP.PropertyPath, path);
            triplePath.AddProperty(SP.PropertyObject, obj);
            return triplePath;
        }


        /**
         * Creates a new TriplePattern as a blank node in a given Model.
         * @param model  the Model to create the pattern in
         * @param subject  the subject (not null)
         * @param predicate  the predicate (not null)
         * @param object  the object (not null)
         * @return a new TriplePattern
         */
        public static ITriplePatternResource createTriplePattern(SpinProcessor model, INode subject, INode predicate, INode obj)
        {
            // No rdf:type sp:TriplePattern needed - engine looks for sp:predicate
            ITriplePatternResource triplePattern = (ITriplePatternResource)model.CreateResource().As(typeof(TriplePatternImpl));
            triplePattern.AddProperty(SP.PropertySubject, subject);
            triplePattern.AddProperty(SP.PropertyPredicate, predicate);
            triplePattern.AddProperty(SP.PropertyObject, obj);
            return triplePattern;
        }


        /**
         * Creates a new UNION element as a blank node in a given Model.
         * @param model  the Model to create the Union in
         * @param elements  the elements
         * @return a new Union
         */
        public static IUnionResource createUnion(SpinProcessor model, IElementListResource elements)
        {
            IUnionResource union = (IUnionResource)model.CreateResource(SP.ClassUnion).As(typeof(UnionImpl));
            union.AddProperty(SP.PropertyElements, elements);
            return union;
        }


        // TODO Should not be needed since we would rely on the underlying storage capabilities
        /** 
         * Creates a new Values element.
         * @param model  the Model to create the Values in
         * @param data  the Table providing the actual data
         * @return a new Values
         */
        public static IValuesResource createValues(SpinProcessor model, SparqlResultSet data, bool untyped)
        {
            IResource blank = untyped ? model.CreateResource() : model.CreateResource(SP.ClassValues);
            IValuesResource values = (IValuesResource)blank.As(typeof(ValuesImpl));

            List<IResource> vars = new List<IResource>();
            foreach (String varName in data.Variables)
            {
                vars.Add(SpinResource.Get(RDFHelper.CreateLiteralNode(varName), model));
            }
            IResource varList = model.CreateList(vars.GetEnumerator());
            values.AddProperty(SP.PropertyVarNames, varList);

            IEnumerator<SparqlResult> bindings = data.Results.GetEnumerator();
            if (bindings.MoveNext())
            {
                List<IResource> lists = new List<IResource>();
                while (bindings.MoveNext())
                {
                    List<IResource> nodes = new List<IResource>();
                    SparqlResult binding = bindings.Current;
                    foreach (String varName in data.Variables)
                    {
                        INode value = binding.Value(varName);
                        if (value == null)
                        {
                            nodes.Add(SpinResource.Get(SP.ClassPropertyUndef, model));
                        }
                        else
                        {
                            nodes.Add(SpinResource.Get(value, model));
                        }
                    }
                    lists.Add(model.CreateList(nodes.GetEnumerator()));
                }
                values.AddProperty(SP.PropertyBindings, model.CreateList(lists.GetEnumerator()));
            }

            return values;
        }


        /**
         * Creates a new Variable as a blank node in a given Model.
         * @param model  the Model
         * @param varName  the name of the variable
         * @return the Variable
         */
        public static IVariableResource createVariable(SpinProcessor model, String varName)
        {
            IVariableResource variable = (IVariableResource)model.CreateResource(SP.ClassVariable).As(typeof(VariableImpl));
            variable.AddProperty(SP.PropertyVarName, RDFHelper.CreateLiteralNode(varName));
            return variable;
        }


        /**
         * Gets an spl:Attribute defined for a given property on a given class.
         * The spl:Attribute must be a direct spin:constraint on the class.
         * @param cls  the class
         * @param property  the property
         * @return the Attribute or null if none is found
         */
        public static IAttributeResource getAttribute(IResource cls, INode property)
        {
            IEnumerator<Triple> it = cls.ListProperties(SPIN.PropertyConstraint).GetEnumerator();
            while (it.MoveNext())
            {
                IResource obj = SpinResource.Get(it.Current.Object, cls.GetModel());
                if (obj is INode && ((IResource)obj).HasProperty(RDF.PropertyType, SPL.ClassAttribute))
                {
                    IAttributeResource a = (IAttributeResource)obj.As(typeof(AttributeImpl));
                    if (RDFHelper.SameTerm(property, a.getPredicate()))
                    {
                        it.Dispose();
                        return a;
                    }
                }
            }
            return null;
        }


        /**
         * Gets the most appopriate metaclass to wrap a given Command into a
         * Template.  For example, for an Ask query, this will return spin:AskTemplate.
         * @param command  the Command, cast into the best possible subclass
         * @return the Template metaclass
         */
        public static INode getTemplateMetaClass(ICommandResource command)
        {
            if (command is IAskResource)
            {
                return SPIN.ClassAskTemplate;
            }
            else if (command is IConstructResource)
            {
                return SPIN.ClassConstructTemplate;
            }
            else if (command is ISelectResource)
            {
                return SPIN.ClassSelectTemplate;
            }
            else if (command is IUpdateResource)
            {
                return SPIN.ClassUpdateTemplate;
            }
            else
            {
                throw new ArgumentException("Unsupported Command type: " + SPINLabels.getLabel(command.GetResource(RDF.PropertyType)));
            }
        }


        /**
         * Checks whether a given module has been declared abstract using
         * <code>spin:abstract</code.
         * @param module  the module to test
         * @return true if abstract
         */
        public static bool isAbstract(IResource module)
        {
            return module.HasProperty(SPIN.PropertyAbstract, RDFHelper.TRUE);
        }


        /**
         * Checks if a given INode can be cast into an ElementList.
         * It must be either rdf:nil or an rdf:List where the first
         * list item is an element using <code>asElement()</code>.
         * @param resource  the resource to test
         * @return true if resource is an element list
         */
        public static bool isElementList(IResource resource)
        {
            if (resource.IsUri() && RDFHelper.SameTerm(RDF.Nil, resource))
            {
                return true;
            }
            else
            {
                return resource.HasProperty(RDF.PropertyFirst);
                /*
                ITriple firstS = Model.getProperty(Model, resource, RDF.first);
                if (firstS != null && !(firstS.Object is ILiteralNode))
                {
                    INode first = firstS.Object;
                    return asElement(first) != null;
                }
                else
                {
                    return false;
                }
                */
            }
        }


        /**
         * Checks if a given INode is an instance of a class that has
         * type spin:Module (or its subclasses such as spin:Function).
         * @param resource  the INode to check
         * @return true  if resource is a Module
         */
        public static bool isModuleInstance(IResource resource)
        {
            foreach (IResource type in resource.GetObjects(RDF.PropertyType))
            {
                if (type.HasProperty(RDFS.PropertySubClassOf, SPIN.ClassModule))
                {
                    return true;
                }
            }
            return false;
        }


        /**
         * Checks if a given INode is spin:query or a sub-property of it.
         * @param predicate  the INode to test
         * @return true if predicate is a query property
         */
        public static bool isQueryProperty(IResource predicate)
        {
            return RDFHelper.SameTerm(SPIN.PropertyQuery,predicate) || predicate.HasProperty(RDFS.PropertySubPropertyOf, SPIN.PropertyQuery);
        }


        /**
         * Checks whether a given INode is a TemplateCall.  The condition for this
         * is stricter than for <code>asTemplateCall</code> as the node also must have
         * a valid template assigned to it, i.e. the type of the node must be an
         * instance of spin:Template.
         * @param node  the INode to check
         * @return true if node is a TemplateCall
         */
        public static bool isTemplateCall(IResource node)
        {
            ITemplateCallResource templateCall = asTemplateCall(node);
            return templateCall != null && templateCall.getTemplate() != null;
        }


        /**
         * Checks whether a given INode is a variable.
         * @param node  the node to check
         * @return true if node is a variable
         */
        public static bool isVariable(IResource node)
        {
            return asVariable(node) != null;
        }
    }
}
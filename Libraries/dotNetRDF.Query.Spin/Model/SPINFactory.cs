/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Util;


// TODO is this really needed ?
namespace VDS.RDF.Query.Spin.Model
{

    // TODO rename this as SPINUtil and relocate into Core

    /* *
     * The singleton that is used to convert plain Jena objects into
     * SPIN API resources, and to do corresponding tests.
     * 
     * @author Holger Knublauch
     */
    //@SuppressWarnings("deprecation")

    internal class SPINFactory
    {

        /* *
         * Attempts to cast a given INode into an Aggregation.
         * Resources that have an aggregation type as their rdf:type
         * are recognized as well-formed aggregations.
         * @param resource  the INode to cast
         * @return the Aggregation or null if INode is not a well-formed aggregation
         */
        public static IAggregation asAggregation(IResource resource)
        {
            if (resource == null) return null;
            IEnumerator<IResource> it = resource.getObjects(RDF.PropertyType).GetEnumerator();
            //JenaUtil.setGraphReadOptimization(true);
            try
            {
                while (it.MoveNext())
                {
                    IResource type = it.Current;
                    if (type.isUri())
                    {
                        if (Aggregations.getName((IResource)type) != null)
                        {
                            it.Dispose();
                            return new AggregationImpl(resource, resource.getModel());
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


        public static IArgument asArgument(IResource resource)
        {
            if (resource == null) return null;
            return new ArgumentImpl(resource, resource.getModel());
        }

        /* *
         * Attempts to cast a given INode into the most specific
         * subclass of Command, esp Update or Query.
         * @param resource  the INode to cast
         * @return resource cast into the best possible type or null
         */
        public static ICommand asCommand(IResource resource)
        {
            if (resource == null) return null;
            IQuery query = asQuery(resource);
            if (query != null)
            {
                return query;
            }
            else
            {
                return asUpdate(resource);
            }
        }


        /* *
         * Checks whether a given INode represents a SPARQL element, and returns
         * an instance of a subclass of Element if so.
         * @param resource  the INode to check
         * @return INode as an Element or null if resource is not an element
         */
        public static IElement asElement(IResource resource)
        {
            if (resource == null) return null;
            /*sealed*/
            ITriplePattern triplePattern = asTriplePattern(resource);
            if (triplePattern != null)
            {
                return triplePattern;
            }
            else if (resource.canAs(SP.ClassTriplePath))
            {
                return (ITriplePath)resource.As(typeof(TriplePathImpl));
            }
            else if (resource.canAs(SP.ClassFilter))
            {
                return (IFilter)resource.As(typeof(FilterImpl));
            }
            else if (resource.canAs(SP.ClassBind))
            {
                return (IBind)resource.As(typeof(BindImpl));
            }
            else if (resource.canAs(SP.ClassOptional))
            {
                return (IOptional)resource.As(typeof(OptionalImpl));
            }
            else if (resource.canAs(SP.ClassNamedGraph))
            {
                return (INamedGraph)resource.As(typeof(NamedGraphImpl));
            }
            else if (resource.canAs(SP.ClassMinus))
            {
                return (IMinus)resource.As(typeof(MinusImpl));
            }
            else if (resource.canAs(SP.ClassExists))
            {
                return (IExists)resource.As(typeof(ExistsImpl));
            }
            else if (resource.canAs(SP.ClassNotExists))
            {
                return (INotExists)resource.As(typeof(NotExistsImpl));
            }
            else if (resource.canAs(SP.ClassService))
            {
                return (IService)resource.As(typeof(ServiceImpl));
            }
            else if (resource.canAs(SP.ClassSubQuery))
            {
                return (ISubQuery)resource.As(typeof(SubQueryImpl));
            }
            else if (resource.canAs(SP.ClassUnion))
            {
                return (IUnion)resource.As(typeof(UnionImpl));
            }
            else if (resource.canAs(SP.ClassValues))
            {
                return (IValues)resource.As(typeof(ValuesImpl));
            }
            else if (isElementList(resource))
            {
                return (IElementList)resource.As(typeof(ElementListImpl));
            }
            else
            {
                return null;
            }
        }


        /* *
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
                IVariable var = SPINFactory.asVariable(resource);
                if (var != null)
                {
                    return var;
                }
                IAggregation aggr = SPINFactory.asAggregation((IResource)resource);
                if (aggr != null)
                {
                    return aggr;
                }
                IFunctionCall functionCall = SPINFactory.asFunctionCall((IResource)resource);
                if (functionCall != null)
                {
                    return functionCall;
                }
            }
            return resource;
        }


        /* *
         * Converts a given INode into a Function instance.
         * No other tests are done.
         * @param resource  the INode to convert
         * @return the Function
         */
        public static IFunction asFunction(IResource resource)
        {
            if (resource == null) return null;
            return (IFunction)resource.As(typeof(FunctionImpl));
        }


        /* *
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
        public static IFunctionCall asFunctionCall(IResource resource)
        {
            if (resource == null) return null;
            if (resource is IBlankNode)
            {
                IResource t = resource.getResource(RDF.PropertyType);
                if (t != null && !RDFUtil.sameTerm(SP.ClassVariable,t))
                {
                    return (IFunctionCall)resource.As(typeof(FunctionCallImpl));
                }
            }
            return null;
        }


        /* *
         * Checks if a given INode is a SPIN query, and returns an
         * instance of a subclass of Query if so. 
         * @param resource  the INode to test
         * @return resource as a Query or null
         */
        public static IQuery asQuery(IResource resource)
        {
            if (resource == null) return null;
            if (resource.canAs(SP.ClassSelect))
            {
                return (IQuery)resource.As(typeof(SelectImpl));
            }
            else if (resource.canAs(SP.ClassConstruct))
            {
                return (IQuery)resource.As(typeof(ConstructImpl));
            }
            else if (resource.canAs(SP.ClassAsk))
            {
                return (IQuery)resource.As(typeof(AskImpl));
            }
            else if (resource.canAs(SP.ClassDescribe))
            {
                return (IQuery)resource.As(typeof(DescribeImpl));
            }
            else
            {
                return null;
            }
        }


        /* *
         * Converts a given INode into a Template instance.
         * No other tests are done.
         * @param resource  the INode to convert
         * @return the Template
         */
        public static ITemplate asTemplate(IResource resource)
        {
            if (resource == null) return null;
            return (ITemplate)resource.As(typeof(TemplateImpl));
        }


        /* *
         * Checks whether a given INode can be cast into TemplateCall, and returns
         * it as a TemplateCall instance if so.
         * @param node  the node to convert
         * @return an instance of TemplateCall or null
         */
        public static ITemplateCall asTemplateCall(IResource resource)
        {
            if (resource == null) return null;
            if (!resource.isLiteral())
            {
                IResource t= resource.getResource(RDF.PropertyType);
                if (t!=null && t.isUri())
                {
                    ITemplate template = SPINModuleRegistry.getTemplate(t.Uri, t.getModel());
                    if (template != null)
                    {
                        return (ITemplateCall)resource.As(typeof(TemplateCallImpl));
                    }
                }
            }
            return null;
        }


        /* *
         * Checks whether a given INode can be converted into a TriplePattern, and if yes,
         * returns an instance of TriplePattern.
         * @param node  the node to test
         * @return node as TriplePattern or null
         */
        public static ITriplePattern asTriplePattern(IResource resource)
        {
            if (resource == null) return null;
            if (resource.hasProperty(SP.PropertyPredicate))
            {
                return new TriplePatternImpl(resource, resource.getModel());
            }
            else
            {
                return null;
            }
        }


        /* *
         * Checks if a given INode is a subclass of sp:Update and
         * casts it into the most specific Java class possible.
         * @param resource  the INode to cast
         * @return the Update or null if resource cannot be cast
         */
        public static IUpdate asUpdate(IResource resource)
        {
            if (resource == null) return null;
            if (resource.canAs(SP.ClassModify))
            {
                return (IModify)resource.As(typeof(ModifyImpl));
            }
            else if (resource.canAs(SP.ClassClear))
            {
                return (IClear)resource.As(typeof(ClearImpl));
            }
            else if (resource.canAs(SP.ClassCreate))
            {
                return (ICreate)resource.As(typeof(CreateImpl));
            }
            else if (resource.canAs(SP.ClassDeleteData))
            {
                return (IDeleteData)resource.As(typeof(DeleteDataImpl));
            }
            else if (resource.canAs(SP.ClassDeleteWhere))
            {
                return (IDeleteWhere)resource.As(typeof(DeleteWhereImpl));
            }
            else if (resource.canAs(SP.ClassDrop))
            {
                return (IDrop)resource.As(typeof(DropImpl));
            }
            else if (resource.canAs(SP.ClassInsertData))
            {
                return (IInsertData)resource.As(typeof(InsertDataImpl));
            }
            else if (resource.canAs(SP.ClassLoad))
            {
                return (ILoad)resource.As(typeof(LoadImpl));
            }
#pragma warning disable 612
            else if (resource.canAs(SP.ClassDelete))
            {
                return (IDelete)resource.As(typeof(DeleteImpl));
            }
            else if (resource.canAs(SP.ClassInsert))
            {
                return (IInsert)resource.As(typeof(InsertImpl));
            }
#pragma warning restore 612
            else
            {
                return null;
            }
        }


        /* *
         * Checks whether a given INode can be cast into a Variable and - if yes -
         * converts it into an instance of Variable.  The INode must have a value
         * for spin:varName.
         * @param node  the node to check
         * @return resource as a Variable or null
         */
        public static IVariable asVariable(IResource resource)
        {
            if (resource == null) return null;
            if (resource.hasProperty(SP.PropertyVarName))
            {
                return (IVariable)resource.As(typeof(VariableImpl));
            }
            else
            {
                return null;
            }
        }


        /* *
         * Creates an spl:Argument with a given property and value type.
         * The new Argument resource will be a blank node in a given Model.
         * @param model  the Model
         * @param argProperty  the property or null
         * @param argType  the value type or null
         * @param optional  true if the Argument shall be optional
         * @return the new Argument
         */
        public static IArgument createArgument(SpinProcessor model, INode argProperty, INode argType, bool optional)
        {
            IArgument a = (IArgument)model.CreateResource(SPL.ClassArgument).As(typeof(ArgumentImpl));
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
                a.AddProperty(SPL.PropertyOptional, RDFUtil.TRUE);
            }
            return a;
        }


        /* *
         * Creates a new spl:Attribute as a blank node in a given Model.
         * @param model  the Model to create the attribute in
         * @param argProperty  the predicate or null
         * @param argType  the value type or null
         * @param minCount  the minimum cardinality or null
         * @param maxCount  the maximum cardinality or null
         * @return a new Attribute
         */
        public static IAttribute createAttribute(SpinProcessor model, INode argProperty, INode argType, int minCount, int maxCount)
        {
            IAttribute a =(IAttribute) model.CreateResource(SPL.ClassAttribute).As(typeof(AttributeImpl));
            if (argProperty != null)
            {
                a.AddProperty(SPL.PropertyPredicate, argProperty);
            }
            if (argType != null)
            {
                a.AddProperty(SPL.PropertyValueType, argType);
            }
            a.AddProperty(SPL.PropertyMinCount, RDFUtil.CreateLiteralNode(minCount.ToString(), XSD.DatatypeInt.Uri));
            a.AddProperty(SPL.PropertyMaxCount, RDFUtil.CreateLiteralNode(maxCount.ToString(), XSD.DatatypeInt.Uri));
            return a;
        }


        /* *
         * Creates an Ask query for a given WHERE clause.
         * @param model  the Model to create the Ask (blank node) in
         * @param where  the elements of the WHERE clause
         * @return the new Ask query
         */
        public static IAsk createAsk(SpinProcessor model, IElementList where)
        {
            IAsk ask = (IAsk)model.CreateResource(SP.ClassAsk).As(typeof(AskImpl));
            ask.AddProperty(SP.PropertyWhere, where);
            return ask;
        }


        /* *
         * Creates a Bind in a given Model as a blank node.
         * @param model  the Model to create the Bind in
         * @param variable  the Variable to assign
         * @param expression  the expression
         * @return a new Bind instance
         */
        public static IBind createBind(SpinProcessor model, IVariable variable, INode expression)
        {
            IBind bind = (IBind)model.CreateResource(SP.ClassBind).As(typeof(BindImpl));
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


        /* *
         * Creates a new ElementList in a given Model.
         * @param model  the Model to create the ElementList in
         * @param elements  the elements (may be empty)
         * @return a new ElementList (may be rdf:nil)
         */
        public static IElementList createElementList(SpinProcessor model, IElement[] elements)
        {
            if (elements.Length > 0)
            {
                return (IElementList)model.CreateList(elements).As(typeof(ElementListImpl));
            }
            else
            {
                return (IElementList)Resource.Get(RDF.Nil, model).As(typeof(ElementListImpl));
            }
        }


        /* *
         * Creates a new ElementList in a given Model.
         * @param model  the Model to create the ElementList in
         * @param elements  the elements (may be empty)
         * @return a new ElementList (may be rdf:nil)
         */
        public static IElementList createElementList(SpinProcessor model, IEnumerator<IElement> elements)
        {
            if (elements.MoveNext())
            {
                elements.Reset();
                return (IElementList)model.CreateList(elements).As(typeof(ElementListImpl));
            }
            else
            {
                return (IElementList)Resource.Get(RDF.Nil, model).As(typeof(ElementListImpl));
            }
        }


        /* *
         * Creates a new Exists as a blank node in a given Model.
         * @param model  the Model to create the EXISTS in
         * @param elements  the elements of the EXISTS
         * @return a new Exists
         */
        public static IExists createExists(SpinProcessor model, IElementList elements)
        {
            IExists notExists = (IExists)model.CreateResource(SP.ClassExists).As(typeof(ExistsImpl));
            notExists.AddProperty(SP.PropertyElements, elements);
            return notExists;
        }


        /* *
         * Creates a Filter from a given expression.
         * @param model  the Model to create the (blank node) Filter in
         * @param expression  the expression node (not null)
         * @return a new Filter
         */
        public static IFilter createFilter(SpinProcessor model, INode expression)
        {
            IFilter filter = (IFilter)model.CreateResource(SP.ClassFilter).As(typeof(FilterImpl));
            filter.AddProperty(SP.PropertyExpression, expression);
            return filter;
        }


        /* *
         * Creates a new Function call, which is basically an instance of the
         * function's class.
         * @param model  the Model to create the function call in
         * @param function  the function class (must be a URI resource)
         * @return a new instance of function
         */
        public static IFunctionCall createFunctionCall(SpinProcessor model, INode function)
        {
            return (IFunctionCall)model.CreateResource(function).As(typeof(FunctionCallImpl));
        }


        /* *
         * Creates a new Minus as a blank node in a given Model.
         * @param model  the Model to create the MINUS in
         * @param elements  the elements of the MINUS
         * @return a new Minus
         */
        public static IMinus createMinus(SpinProcessor model, IElementList elements)
        {
            IMinus minus = (IMinus)model.CreateResource(SP.ClassMinus).As(typeof(MinusImpl));
            minus.AddProperty(SP.PropertyElements, elements);
            return minus;
        }


        /* *
         * Creates a new NamedGraph element as a blank node in a given Model.
         * @param model  the Model to generate the NamedGraph in
         * @param graphNameNode  the URI resource of the graph name
         * @param elements  the elements in the NamedGraph
         * @return a new NamedGraph
         */
        public static INamedGraph createNamedGraph(SpinProcessor model, INode graphNameNode, IResource elements)
        {
            INamedGraph result =(INamedGraph) model.CreateResource(SP.ClassNamedGraph).As(typeof(NamedGraphImpl));
            result.AddProperty(SP.PropertyGraphNameNode, graphNameNode);
            result.AddProperty(SP.PropertyElements, elements);
            return result;
        }


        /* *
         * Creates a new NotExists as a blank node in a given Model.
         * @param model  the Model to create the NOT EXISTS in
         * @param elements  the elements of the NOT EXISTS
         * @return a new NotExists
         */
        public static INotExists createNotExists(SpinProcessor model, IElementList elements)
        {
            INotExists notExists = (INotExists)model.CreateResource(SP.ClassNotExists).As(typeof(NotExistsImpl));
            notExists.AddProperty(SP.PropertyElements, elements);
            return notExists;
        }


        /* *
         * Creates a new Optional as a blank node in a given Model. 
         * @param model  the Model to create the OPTIONAL in
         * @param elements  the elements of the OPTIONAL
         * @return a new Optional
         */
        public static IOptional createOptional(SpinProcessor model, IElementList elements)
        {
            IOptional optional = (IOptional)model.CreateResource(SP.ClassOptional).As(typeof(OptionalImpl));
            optional.AddProperty(SP.PropertyElements, elements);
            return optional;
        }


        public static IService createService(SpinProcessor model, INode serviceURI, IElementList elements)
        {
            IService service = (IService)model.CreateResource(SP.ClassService).As(typeof(ServiceImpl));
            service.AddProperty(SP.PropertyServiceURI, serviceURI);
            service.AddProperty(SP.PropertyElements, elements);
            return service;
        }


        /* *
         * Creates a new SubQuery as a blank node in a given Model.
         * @param model  the Model to create the SubQuery in
         * @param subQuery  the nested query
         * @return a new SubQuery
         */
        public static ISubQuery createSubQuery(SpinProcessor model, IQuery subQuery)
        {
            ISubQuery result = (ISubQuery)model.CreateResource(SP.ClassSubQuery).As(typeof(SubQueryImpl));
            result.AddProperty(SP.PropertyQuery, subQuery);
            return result;
        }


        /* *
         * Creates a new TemplateCall as a blank node instance of a given template.
         * @param model  the Model to create a template call in
         * @param template  the template class
         * @return the new TemplateCall or null
         */
        public static ITemplateCall createTemplateCall(SpinProcessor model, INode template)
        {
            ITemplateCall templateCall = (ITemplateCall)model.CreateResource(template).As(typeof(TemplateCallImpl));
            return templateCall;
        }


        /* *
         * Creates a new TriplePath as a blank node in a given Model.
         * @param model  the Model to create the path in
         * @param subject  the subject (not null)
         * @param path  the path (not null)
         * @param object  the object (not null)
         * @return a new TriplePath
         */
        public static ITriplePath createTriplePath(SpinProcessor model, INode subject, INode path, INode obj)
        {
            ITriplePath triplePath = (ITriplePath)model.CreateResource(SP.ClassTriplePath).As(typeof(TriplePathImpl));
            triplePath.AddProperty(SP.PropertySubject, subject);
            triplePath.AddProperty(SP.PropertyPath, path);
            triplePath.AddProperty(SP.PropertyObject, obj);
            return triplePath;
        }


        /* *
         * Creates a new TriplePattern as a blank node in a given Model.
         * @param model  the Model to create the pattern in
         * @param subject  the subject (not null)
         * @param predicate  the predicate (not null)
         * @param object  the object (not null)
         * @return a new TriplePattern
         */
        public static ITriplePattern createTriplePattern(SpinProcessor model, INode subject, INode predicate, INode obj)
        {
            // No rdf:type sp:TriplePattern needed - engine looks for sp:predicate
            ITriplePattern triplePattern = (ITriplePattern)model.CreateResource().As(typeof(TriplePatternImpl));
            triplePattern.AddProperty(SP.PropertySubject, subject);
            triplePattern.AddProperty(SP.PropertyPredicate, predicate);
            triplePattern.AddProperty(SP.PropertyObject, obj);
            return triplePattern;
        }


        /* *
         * Creates a new UNION element as a blank node in a given Model.
         * @param model  the Model to create the Union in
         * @param elements  the elements
         * @return a new Union
         */
        public static IUnion createUnion(SpinProcessor model, IElementList elements)
        {
            IUnion union = (IUnion)model.CreateResource(SP.ClassUnion).As(typeof(UnionImpl));
            union.AddProperty(SP.PropertyElements, elements);
            return union;
        }


        // TODO Should not be needed since we would rely on the underlying storage capabilities
        /* * 
         * Creates a new Values element.
         * @param model  the Model to create the Values in
         * @param data  the Table providing the actual data
         * @return a new Values
         */
        public static IValues createValues(SpinProcessor model, SparqlResultSet data, bool untyped)
        {
            IResource blank = untyped ? model.CreateResource() : model.CreateResource(SP.ClassValues);
            IValues values = (IValues)blank.As(typeof(ValuesImpl));

            List<IResource> vars = new List<IResource>();
            foreach (String varName in data.Variables)
            {
                vars.Add(Resource.Get(RDFUtil.CreateLiteralNode(varName), model));
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
                            nodes.Add(Resource.Get(SP.ClassPropertyUndef, model));
                        }
                        else
                        {
                            nodes.Add(Resource.Get(value, model));
                        }
                    }
                    lists.Add(model.CreateList(nodes.GetEnumerator()));
                }
                values.AddProperty(SP.PropertyBindings, model.CreateList(lists.GetEnumerator()));
            }

            return values;
        }


        /* *
         * Creates a new Variable as a blank node in a given Model.
         * @param model  the Model
         * @param varName  the name of the variable
         * @return the Variable
         */
        public static IVariable createVariable(SpinProcessor model, String varName)
        {
            IVariable variable = (IVariable)model.CreateResource(SP.ClassVariable).As(typeof(VariableImpl));
            variable.AddProperty(SP.PropertyVarName, RDFUtil.CreateLiteralNode(varName));
            return variable;
        }


        /* *
         * Gets an spl:Attribute defined for a given property on a given class.
         * The spl:Attribute must be a direct spin:constraint on the class.
         * @param cls  the class
         * @param property  the property
         * @return the Attribute or null if none is found
         */
        public static IAttribute getAttribute(IResource cls, INode property)
        {
            IEnumerator<Triple> it = cls.listProperties(SPIN.PropertyConstraint).GetEnumerator();
            while (it.MoveNext())
            {
                IResource obj = Resource.Get(it.Current.Object, cls.getModel());
                if (obj is INode && ((IResource)obj).hasProperty(RDF.PropertyType, SPL.ClassAttribute))
                {
                    IAttribute a = (IAttribute)obj.As(typeof(AttributeImpl));
                    if (RDFUtil.sameTerm(property, a.getPredicate()))
                    {
                        it.Dispose();
                        return a;
                    }
                }
            }
            return null;
        }


        /* *
         * Gets the most appopriate metaclass to wrap a given Command into a
         * Template.  For example, for an Ask query, this will return spin:AskTemplate.
         * @param command  the Command, cast into the best possible subclass
         * @return the Template metaclass
         */
        public static INode getTemplateMetaClass(ICommand command)
        {
            if (command is IAsk)
            {
                return SPIN.ClassAskTemplate;
            }
            else if (command is IConstruct)
            {
                return SPIN.ClassConstructTemplate;
            }
            else if (command is ISelect)
            {
                return SPIN.ClassSelectTemplate;
            }
            else if (command is IUpdate)
            {
                return SPIN.ClassUpdateTemplate;
            }
            else
            {
                throw new ArgumentException("Unsupported Command type: " + SPINLabels.getLabel(command.getResource(RDF.PropertyType)));
            }
        }


        /* *
         * Checks whether a given module has been declared abstract using
         * <code>spin:abstract</code.
         * @param module  the module to test
         * @return true if abstract
         */
        public static bool isAbstract(IResource module)
        {
            return module.hasProperty(SPIN.PropertyAbstract, RDFUtil.TRUE);
        }


        /* *
         * Checks if a given INode can be cast into an ElementList.
         * It must be either rdf:nil or an rdf:List where the first
         * list item is an element using <code>asElement()</code>.
         * @param resource  the resource to test
         * @return true if resource is an element list
         */
        public static bool isElementList(IResource resource)
        {
            if (resource.isUri() && RDFUtil.sameTerm(RDF.Nil, resource))
            {
                return true;
            }
            else
            {
                return resource.hasProperty(RDF.PropertyFirst);
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


        /* *
         * Checks if a given INode is an instance of a class that has
         * type spin:Module (or its subclasses such as spin:Function).
         * @param resource  the INode to check
         * @return true  if resource is a Module
         */
        public static bool isModuleInstance(IResource resource)
        {
            foreach (IResource type in resource.getObjects(RDF.PropertyType))
            {
                if (type.hasProperty(RDFS.PropertySubClassOf, SPIN.ClassModule))
                {
                    return true;
                }
            }
            return false;
        }


        /* *
         * Checks if a given INode is spin:query or a sub-property of it.
         * @param predicate  the INode to test
         * @return true if predicate is a query property
         */
        public static bool isQueryProperty(IResource predicate)
        {
            return RDFUtil.sameTerm(SPIN.PropertyQuery,predicate) || predicate.hasProperty(RDFS.PropertySubPropertyOf, SPIN.PropertyQuery);
        }


        /* *
         * Checks whether a given INode is a TemplateCall.  The condition for this
         * is stricter than for <code>asTemplateCall</code> as the node also must have
         * a valid template assigned to it, i.e. the type of the node must be an
         * instance of spin:Template.
         * @param node  the INode to check
         * @return true if node is a TemplateCall
         */
        public static bool isTemplateCall(IResource node)
        {
            ITemplateCall templateCall = asTemplateCall(node);
            return templateCall != null && templateCall.getTemplate() != null;
        }


        /* *
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
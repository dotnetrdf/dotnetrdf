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

namespace VDS.RDF.Query.Spin.Model
{
    internal interface ISPINFactory
    {
        VDS.RDF.Query.Spin.Model.IAggregationResource asAggregation(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IArgumentResource asArgument(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.ICommandResource asCommand(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IElementResource asElement(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IResource asExpression(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IFunctionResource asFunction(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IFunctionCallResource asFunctionCall(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IQueryResource asQuery(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.ITemplateResource asTemplate(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.ITemplateCallResource asTemplateCall(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.ITriplePatternResource asTriplePattern(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IUpdateResource asUpdate(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IVariableResource asVariable(VDS.RDF.Query.Spin.Model.IResource resource);

        VDS.RDF.Query.Spin.Model.IArgumentResource createArgument(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode argProperty, VDS.RDF.INode argType, bool optional);

        VDS.RDF.Query.Spin.Model.IAskResource createAsk(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IElementListResource where);

        VDS.RDF.Query.Spin.Model.IAttributeResource createAttribute(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode argProperty, VDS.RDF.INode argType, int minCount, int maxCount);

        VDS.RDF.Query.Spin.Model.IBindResource createBind(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IVariableResource variable, VDS.RDF.INode expression);

        VDS.RDF.Query.Spin.Model.IElementListResource createElementList(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IElementResource[] elements);

        VDS.RDF.Query.Spin.Model.IElementListResource createElementList(VDS.RDF.Query.Spin.SpinModel model, System.Collections.Generic.IEnumerator<VDS.RDF.Query.Spin.Model.IElementResource> elements);

        VDS.RDF.Query.Spin.Model.IExistsResource createExists(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IElementListResource elements);

        VDS.RDF.Query.Spin.Model.IFilterResource createFilter(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode expression);

        VDS.RDF.Query.Spin.Model.IFunctionCallResource createFunctionCall(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode function);

        VDS.RDF.Query.Spin.Model.IMinusResource createMinus(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IElementListResource elements);

        VDS.RDF.Query.Spin.Model.INamedGraphResource createNamedGraph(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode graphNameNode, VDS.RDF.Query.Spin.Model.IResource elements);

        VDS.RDF.Query.Spin.Model.INotExistsResource createNotExists(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IElementListResource elements);

        VDS.RDF.Query.Spin.Model.IOptionalResource createOptional(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IElementListResource elements);

        VDS.RDF.Query.Spin.Model.IServiceResource createService(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode serviceURI, VDS.RDF.Query.Spin.Model.IElementListResource elements);

        VDS.RDF.Query.Spin.Model.ISubQueryResource createSubQuery(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IQueryResource subQuery);

        VDS.RDF.Query.Spin.Model.ITemplateCallResource createTemplateCall(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode template);

        VDS.RDF.Query.Spin.Model.ITriplePathResource createTriplePath(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode subject, VDS.RDF.INode path, VDS.RDF.INode obj);

        VDS.RDF.Query.Spin.Model.ITriplePatternResource createTriplePattern(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.INode subject, VDS.RDF.INode predicate, VDS.RDF.INode obj);

        VDS.RDF.Query.Spin.Model.IUnionResource createUnion(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.Spin.Model.IElementListResource elements);

        VDS.RDF.Query.Spin.Model.IValuesResource createValues(VDS.RDF.Query.Spin.SpinModel model, VDS.RDF.Query.SparqlResultSet data, bool untyped);

        VDS.RDF.Query.Spin.Model.IVariableResource createVariable(VDS.RDF.Query.Spin.SpinModel model, string varName);

        VDS.RDF.Query.Spin.Model.IAttributeResource getAttribute(VDS.RDF.Query.Spin.Model.IResource cls, VDS.RDF.INode property);

        VDS.RDF.INode getTemplateMetaClass(VDS.RDF.Query.Spin.Model.ICommandResource command);

        bool isAbstract(VDS.RDF.Query.Spin.Model.IResource module);

        bool isElementList(VDS.RDF.Query.Spin.Model.IResource resource);

        bool isModuleInstance(VDS.RDF.Query.Spin.Model.IResource resource);

        bool isQueryProperty(VDS.RDF.Query.Spin.Model.IResource predicate);

        bool isTemplateCall(VDS.RDF.Query.Spin.Model.IResource node);

        bool isVariable(VDS.RDF.Query.Spin.Model.IResource node);
    }
}
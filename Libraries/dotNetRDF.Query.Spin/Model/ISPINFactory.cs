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
namespace VDS.RDF.Query.Spin.Model
{
    interface ISPINFactory
    {

        VDS.RDF.Query.Spin.Model.IAggregation asAggregation(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IArgument asArgument(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.ICommand asCommand(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IElement asElement(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IResource asExpression(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IFunction asFunction(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IFunctionCall asFunctionCall(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IQuery asQuery(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.ITemplate asTemplate(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.ITemplateCall asTemplateCall(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.ITriplePattern asTriplePattern(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IUpdate asUpdate(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IVariable asVariable(VDS.RDF.Query.Spin.Model.IResource resource);
        VDS.RDF.Query.Spin.Model.IArgument createArgument(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode argProperty, VDS.RDF.INode argType, bool optional);
        VDS.RDF.Query.Spin.Model.IAsk createAsk(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IElementList where);
        VDS.RDF.Query.Spin.Model.IAttribute createAttribute(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode argProperty, VDS.RDF.INode argType, int minCount, int maxCount);
        VDS.RDF.Query.Spin.Model.IBind createBind(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IVariable variable, VDS.RDF.INode expression);
        VDS.RDF.Query.Spin.Model.IElementList createElementList(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IElement[] elements);
        VDS.RDF.Query.Spin.Model.IElementList createElementList(VDS.RDF.Query.Spin.SpinProcessor model, System.Collections.Generic.IEnumerator<VDS.RDF.Query.Spin.Model.IElement> elements);
        VDS.RDF.Query.Spin.Model.IExists createExists(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IElementList elements);
        VDS.RDF.Query.Spin.Model.IFilter createFilter(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode expression);
        VDS.RDF.Query.Spin.Model.IFunctionCall createFunctionCall(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode function);
        VDS.RDF.Query.Spin.Model.IMinus createMinus(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IElementList elements);
        VDS.RDF.Query.Spin.Model.INamedGraph createNamedGraph(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode graphNameNode, VDS.RDF.Query.Spin.Model.IResource elements);
        VDS.RDF.Query.Spin.Model.INotExists createNotExists(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IElementList elements);
        VDS.RDF.Query.Spin.Model.IOptional createOptional(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IElementList elements);
        VDS.RDF.Query.Spin.Model.IService createService(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode serviceURI, VDS.RDF.Query.Spin.Model.IElementList elements);
        VDS.RDF.Query.Spin.Model.ISubQuery createSubQuery(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IQuery subQuery);
        VDS.RDF.Query.Spin.Model.ITemplateCall createTemplateCall(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode template);
        VDS.RDF.Query.Spin.Model.ITriplePath createTriplePath(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode subject, VDS.RDF.INode path, VDS.RDF.INode obj);
        VDS.RDF.Query.Spin.Model.ITriplePattern createTriplePattern(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.INode subject, VDS.RDF.INode predicate, VDS.RDF.INode obj);
        VDS.RDF.Query.Spin.Model.IUnion createUnion(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.Spin.Model.IElementList elements);
        VDS.RDF.Query.Spin.Model.IValues createValues(VDS.RDF.Query.Spin.SpinProcessor model, VDS.RDF.Query.SparqlResultSet data, bool untyped);
        VDS.RDF.Query.Spin.Model.IVariable createVariable(VDS.RDF.Query.Spin.SpinProcessor model, string varName);
        VDS.RDF.Query.Spin.Model.IAttribute getAttribute(VDS.RDF.Query.Spin.Model.IResource cls, VDS.RDF.INode property);
        VDS.RDF.INode getTemplateMetaClass(VDS.RDF.Query.Spin.Model.ICommand command);

        bool isAbstract(VDS.RDF.Query.Spin.Model.IResource module);
        bool isElementList(VDS.RDF.Query.Spin.Model.IResource resource);
        bool isModuleInstance(VDS.RDF.Query.Spin.Model.IResource resource);
        bool isQueryProperty(VDS.RDF.Query.Spin.Model.IResource predicate);
        bool isTemplateCall(VDS.RDF.Query.Spin.Model.IResource node);
        bool isVariable(VDS.RDF.Query.Spin.Model.IResource node);
    }
}

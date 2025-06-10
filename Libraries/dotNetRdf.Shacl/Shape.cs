/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Shacl.Shapes;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl;

internal abstract class Shape : GraphWrapperNode
{
    [DebuggerStepThrough]
    protected Shape(INode node, IGraph shapesGraph)
        : base(node, shapesGraph)
    {
    }

    internal INode Severity
    {
        get
        {
            return Vocabulary.Severity.ObjectsOf(this).SingleOrDefault() ?? Vocabulary.Violation;
        }
    }

    // TODO: Spec says this is a collection
    internal ILiteralNode Message
    {
        get
        {
            return (ILiteralNode)Vocabulary.Message.ObjectsOf(this).SingleOrDefault();
        }
    }

    private bool Deactivated
    {
        get
        {
            return Vocabulary.Deactivated.ObjectsOf(this).SingleOrDefault()?.AsValuedNode().AsBoolean() ?? false;
        }
    }

    private new ShapesGraph Graph
    {
        get
        {
            return new ShapesGraph(base.Graph);
        }
    }

    private IEnumerable<Constraint> Constraints
    {
        get
        {
            return Vocabulary.Constraints.SelectMany(constraint =>
                from o in constraint.ObjectsOf(this)
                select Constraint.Parse(this, constraint, o));
        }
    }

    private IEnumerable<Target> Targets
    {
        get
        {
            bool isShape(GraphWrapperNode node)
            {
                return Vocabulary.Shapes.Any(node.IsInstanceOf);
            }

            bool isClass(GraphWrapperNode node)
            {
                return node.IsInstanceOf(Vocabulary.RdfsClass);
            }

            IEnumerable<Target> selectTargets(INode type) =>
                from target in type.ObjectsOf(this)
                select Target.Parse(type, target);

            IEnumerable<Target> targets = Vocabulary.Targets.SelectMany(selectTargets);

            IEnumerable<Target> implicitClassTargets =
                from shape in this.AsEnumerable()
                where isClass(shape) && isShape(shape)
                select Target.Parse(Vocabulary.TargetClass, shape);

            return targets.Union(implicitClassTargets);
        }
    }

    internal static Shape Parse(GraphWrapperNode node)
    {
        return Parse(node, node.Graph);
    }

    internal static Shape Parse(INode node, IGraph graph)
    {
        if (Vocabulary.Path.ObjectsOf(node, graph).Any())
        {
            return new Property(node, graph);
        }
        else
        {
            return new Node(node, graph);
        }
    }

    internal bool Validate(IGraph dataGraph, Report report)
    {
        return SelectFocusNodes(dataGraph)
            .Select(focusNode => Validate(dataGraph, focusNode, focusNode.AsEnumerable(), report))
            .Aggregate(true, (a, b) => a && b);
    }

    internal bool Validate(IGraph dataGraph, INode focusNode)
    {
        return Validate(dataGraph, focusNode, focusNode.AsEnumerable());
    }

    internal bool Validate(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report = null)
    {
        if (Deactivated)
        {
            return true;
        }

        return ValidateInternal(dataGraph, focusNode, valueNodes, report);
    }

    protected virtual bool ValidateInternal(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report)
    {
        IEnumerable<Constraint> components = (
            from component in Graph.ConstraintComponents
            where component.Matches(this)
            select component.Constraints(this))
            .Aggregate(Enumerable.Empty<Constraint>(), (first, second) => first.Concat(second));

        return (
            from constraint in Constraints.Concat(components)
            select constraint.Validate(dataGraph, focusNode, valueNodes, report))
            .Aggregate(true, (first, second) => first && second);
    }

    private IEnumerable<INode> SelectFocusNodes(IGraph dataGraph)
    {
        return Targets.SelectMany(target => target.SelectFocusNodes(dataGraph)).Distinct();
    }
}
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

namespace VDS.RDF.Shacl.Validation
{
    using System.Diagnostics;
    using System.Linq;

    internal class Result : WrapperNode
    {
        [DebuggerStepThrough]
        private Result(INode node)
            : base(node)
        {
        }

        internal INode Severity
        {
            get
            {
                return Vocabulary.ResultSeverity.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var severity in Vocabulary.ResultSeverity.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.ResultSeverity, severity);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.ResultSeverity, value);
            }
        }

        internal INode FocusNode
        {
            get
            {
                return Vocabulary.FocusNode.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var focusNode in Vocabulary.FocusNode.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.FocusNode, focusNode);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.FocusNode, value);
            }
        }

        internal INode ResultValue
        {
            get
            {
                return Vocabulary.Value.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var valueNode in Vocabulary.Value.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.Value, valueNode);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.Value, value);
            }
        }

        internal INode SourceShape
        {
            get
            {
                return Vocabulary.SourceShape.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceShape in Vocabulary.SourceShape.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.SourceShape, sourceShape);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.SourceShape, value);
            }
        }

        internal INode Message
        {
            get
            {
                return Vocabulary.ResultMessage.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceShape in Vocabulary.ResultMessage.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.ResultMessage, sourceShape);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.ResultMessage, value);
            }
        }

        internal INode SourceConstraintComponent
        {
            get
            {
                return Vocabulary.SourceConstraintComponent.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraintComponent in Vocabulary.SourceConstraintComponent.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.SourceConstraintComponent, sourceConstraintComponent);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.SourceConstraintComponent, value);
            }
        }

        internal Path ResultPath
        {
            get
            {
                return Vocabulary.ResultPath.ObjectsOf(this).Select(Path.Parse).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraintComponent in Vocabulary.ResultPath.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.ResultPath, sourceConstraintComponent);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.ResultPath, value);
            }
        }

        internal INode SourceConstraint
        {
            get
            {
                return Vocabulary.SourceConstraint.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraint in Vocabulary.SourceConstraint.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.SourceConstraint, sourceConstraint);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.SourceConstraint, value);
            }
        }

        private INode Type
        {
            get
            {
                return Vocabulary.RdfType.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var type in Vocabulary.RdfType.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Vocabulary.RdfType, type);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Vocabulary.RdfType, value);
            }
        }

        internal static Result Create(IGraph g)
        {
            var report = new Result(g.CreateBlankNode())
            {
                Type = Vocabulary.ValidationResult,
            };

            return report;
        }

        internal static Result Parse(INode node)
        {
            return new Result(node);
        }
    }
}
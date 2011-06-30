using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents an arbitrary property path in the algebra (only used when strict algebra is generated)
    /// </summary>
    public class PropertyPath : BasePathOperator, ITerminalOperator
    {
        public PropertyPath(PatternItem start, ISparqlPath path, PatternItem end)
            : base(start, path, end) { }

        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Try and generate an Algebra expression
            //Make sure we don't generate clashing temporary variable IDs over the life of the
            //Evaluation
            PathTransformContext transformContext = new PathTransformContext(this.PathStart, this.PathEnd);
            if (context["PathTransformID"] != null)
            {
                transformContext.NextID = (int)context["PathTransformID"];
            }
            ISparqlAlgebra algebra = this.Path.ToAlgebra(transformContext);
            context["PathTransformID"] = transformContext.NextID;

            //Now we can evaluate the resulting algebra
            //Note: We may need to preserve Blank Node variables across evaluations
            //which we usually don't do BUT because of the way we translate only part of the path
            //into an algebra at a time and may need to do further nested translate calls we do
            //need to do this here
            BaseMultiset initialInput = context.InputMultiset;
            bool trimMode = context.TrimTemporaryVariables;
            try
            {
                context.TrimTemporaryVariables = false;
                BaseMultiset result = context.Evaluate(algebra);//algebra.Evaluate(context);
                //Also note that we don't trim temporary variables here even if we've set the setting back
                //to enabled since a Trim will be done at the end of whatever BGP we are being evaluated in

                //Once we have our results can join then into our input
                context.OutputMultiset = initialInput.Join(result);
            }
            finally
            {
                context.TrimTemporaryVariables = trimMode;
            }

            return context.OutputMultiset;
        }

        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            gp.AddTriplePattern(new PropertyPathPattern(this.PathStart, this.Path, this.PathEnd));
            return gp;
        }

        public override string ToString()
        {
            return "PropertyPath()";
        }
    }
}

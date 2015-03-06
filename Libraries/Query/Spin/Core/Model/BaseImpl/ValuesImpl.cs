using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class ValuesImpl : ElementImpl, IValuesResource
    {

        public ValuesImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {

        }


        public List<Dictionary<String, IResource>> getBindings()
        {
            List<String> varNames = getVarNames();
            List<Dictionary<String, IResource>> bindings = new List<Dictionary<String, IResource>>();
            List<IResource> outerList = GetResource(SP.PropertyBindings).AsList();
            if (outerList != null)
            {
                foreach (IResource innerList in outerList)
                {
                    Dictionary<String, IResource> binding = new Dictionary<String, IResource>();
                    bindings.Add(binding);
                    IEnumerator<String> vars = varNames.GetEnumerator();
                    IEnumerator<IResource> values = innerList.AsList().GetEnumerator();
                    while (vars.MoveNext())
                    {
                        String varName = vars.Current;
                        IResource value = values.Current;
                        if (!RDFHelper.SameTerm(SP.ClassPropertyUndef, value))
                        {
                            binding.Add(varName, value);
                        }
                    }
                }
            }
            return bindings;
        }


        public List<String> getVarNames()
        {
            List<String> results = new List<String>();
            List<IResource> list = GetResource(SP.PropertyVarNames).AsList();
            foreach (IResource member in list)
            {
                results.Add(((IValuedNode)member.AsNode()).AsString());
            }
            return results;
        }


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("VALUES");
            p.print(" ");
            List<String> varNames = getVarNames();
            if (varNames.Count == 1)
            {
                p.printVariable(varNames[0]);
            }
            else
            {
                p.print("(");
                IEnumerator<String> vit = varNames.GetEnumerator();
                while (vit.MoveNext())
                {
                    p.printVariable(vit.Current);
                    if (vit.MoveNext())
                    {
                        p.print(" ");
                    }
                }
                p.print(")");
            }
            p.print(" {");
            p.println();
            foreach (Dictionary<String, IResource> binding in getBindings())
            {
                p.printIndentation(p.getIndentation() + 1);
                if (varNames.Count != 1)
                {
                    p.print("(");
                }
                IEnumerator<String> vit = varNames.GetEnumerator();
                while (vit.MoveNext())
                {
                    String varName = vit.Current;
                    IResource value = binding[varName];
                    if (value == null)
                    {
                        p.printKeyword("UNDEF");
                    }
                    else if (value.IsUri())
                    {
                        p.printURIResource(value);
                    }
                    else
                    {
                        TupleImpl.print(GetModel(), SpinResource.Get(value, GetModel()), p);
                    }
                    if (vit.MoveNext())
                    {
                        p.print(" ");
                    }
                }
                if (varNames.Count != 1)
                {
                    p.print(")");
                }
                p.println();
            }
            p.printIndentation(p.getIndentation());
            p.print("}");
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}
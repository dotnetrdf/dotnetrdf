using System;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class TupleImpl : AbstractSPINResource
    {
        public TupleImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IResource getObject()
        {
            return getRDFNodeOrVariable(SP.PropertyObject);
        }

        public IResource getObjectResource()
        {
            IResource node = getRDFNodeOrVariable(SP.PropertyObject);
            if (!(node is IVariableResource))
            {
                return node;
            }
            else
            {
                return null;
            }
        }

        public IResource getSubject()
        {
            return getRDFNodeOrVariable(SP.PropertySubject);
        }

        protected IResource getRDFNodeOrVariable(INode predicate)
        {
            IResource node = GetResource(predicate);
            if (node != null)
            {
                IVariableResource var = ResourceFactory.asVariable(node);
                if (var != null)
                {
                    return var;
                }
                else
                {
                    return node;
                }
            }
            else
            {
                return null;
            }
        }

        internal void print(IResource node, ISparqlPrinter p)
        {
            TupleImpl.print(GetModel(), node, p);
        }

        internal void print(IResource node, ISparqlPrinter p, bool abbrevRDFType)
        {
            TupleImpl.print(GetModel(), node, p, abbrevRDFType);
        }

        public static void print(SpinModel model, IResource node, ISparqlPrinter p)
        {
            print(model, node, p, false);
        }

        public static void print(SpinModel model, IResource node, ISparqlPrinter p, bool abbrevRDFType)
        {
            // TODO find the good tests ?????
            if (!node.IsLiteral())
            {
                if (abbrevRDFType && RDFHelper.SameTerm(RDF.PropertyType, node))
                {
                    p.print("a");
                }
                else
                {
                    printVarOrResource(p, node);
                }
            }
            else
            {
                //TODO INamespaceMapper pm = p.getUsePrefixes() ? model.getGraph().getPrefixMapping() : SPINExpressions.emptyPrefixMapping;
                String str = node.AsNode().ToString();// TODO is this correct ? // FmtUtils.stringForNode(node, null/*TODO pm*/);
                p.print(str);
            }
        }
    }
}
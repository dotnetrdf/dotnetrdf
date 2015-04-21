using System;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    public class TriplePathImpl : TupleImpl, ITriplePathResource
    {
        public TriplePathImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        //public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}

        override public void Print(ISparqlPrinter p)
        {
            print(getSubject(), p);
            p.print(" ");

            IResource pathS = GetObject(SP.PropertyPath);
            if (pathS == null || pathS.IsLiteral())
            {
                p.print("<Missing path>");
            }
            else
            {
                printPath(pathS, p);
            }
            p.print(" ");
            print(getObject(), p);
        }

        private void printPath(IResource path, ISparqlPrinter p)
        {
            ISparqlPath arqPath = createPath(path);
            if (p.getUsePrefixes())
            {
                //TODO INamespaceMapper prefixMapping = path.getModel().getPrefixMapping();
                String str = arqPath.ToString(); //PathWriter.asString(arqPath, null/*TODO new Prologue(prefixMapping)*/);
                p.print(str);
            }
            else
            {
                String str = arqPath.ToString();//PathWriter.asString(arqPath);
                p.print(str);
            }
        }

        private ISparqlPath createPath(IResource path)
        {
            if (path.IsUri())
            {
                return new Property(path.AsNode());
            }
            else
            {
                IResource typeS = path.GetResource(RDF.PropertyType);
                if (typeS != null && typeS.IsUri())
                {
                    INode type = typeS;
                    if (RDFHelper.SameTerm(SP.ClassAltPath, type))
                    {
                        ISparqlPath leftPath = createPath(path, SP.PropertyPath1);
                        ISparqlPath rightPath = createPath(path, SP.PropertyPath2);
                        return new AlternativePath(leftPath, rightPath);
                    }
                    else if (RDFHelper.SameTerm(SP.ClassModPath, type))
                    {
                        ISparqlPath subPath = createPath(path, SP.PropertySubPath);
                        int? min = path.GetInteger(SP.PropertyModMin);
                        int? max = path.GetInteger(SP.PropertyModMax);
                        if (max == null || max < 0)
                        {
                            if (min == 1)
                            {
                                return new OneOrMore(subPath);  // TODO: is this correct?
                            }
                            else if (min == null || min == -1)
                            {
                                return new ZeroOrMore(subPath);
                            }
                            else
                            { // -2
                                return new NOrMore(subPath, (int)min);  // TODO: is this correct?
                            }
                        }
                        else
                        {
                            if (min == null || min == -1)
                            {
                                return new ZeroToN(subPath, (int)max);
                            }
                            else
                            {
                                return new NToM(subPath, (int)min, (int)max);
                            }
                        }
                    }
                    else if (RDFHelper.SameTerm(SP.ClassReversePath, type))
                    {
                        ISparqlPath subPath = createPath(path, SP.PropertySubPath);
                        return new InversePath(subPath);
                    }
                    else if (RDFHelper.SameTerm(SP.ClassSeqPath, type))
                    {
                        ISparqlPath leftPath = createPath(path, SP.PropertyPath1);
                        ISparqlPath rightPath = createPath(path, SP.PropertyPath2);
                        return new SequencePath(leftPath, rightPath);
                    }
                    else if (RDFHelper.SameTerm(SP.ClassReverseLinkPath, type))
                    {
                        IResource node = path.GetObject(SP.PropertyNode);
                        ISparqlPath subPath = createPath(node); // TODO: is this correct ?
                        return new InversePath(subPath);
                    }
                }
                return null;
            }
        }

        private ISparqlPath createPath(IResource subject, INode predicate)
        {
            IResource s = subject.GetResource(predicate);
            if (s != null)
            {
                return createPath(s);
            }
            else
            {
                return null;
            }
        }
    }
}
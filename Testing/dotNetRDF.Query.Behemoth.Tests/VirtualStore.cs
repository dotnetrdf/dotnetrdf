using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;

namespace dotNetRDF.Query.Behemoth.Tests
{
    public class VirtualGraph : IGraph
    {
        private readonly INodeFactory _nodeFactory;
        private List<ITripleProvider> _tripleProviders;

        public VirtualGraph(IRefNode name = null, Uri baseUri = null, INamespaceMapper namespaceMap = null, INodeFactory nodeFactory = null)
        {
            Name = name;
            BaseUri = baseUri;
            NamespaceMap = namespaceMap ?? (nodeFactory != null ? nodeFactory.NamespaceMap : new NamespaceMapper());
            _nodeFactory = nodeFactory ?? new NodeFactory(BaseUri, NamespaceMap);
        }

        public Uri BaseUri { get; set; }
        public INamespaceMapper NamespaceMap { get; }

        public IBlankNode CreateBlankNode()
        {
            return _nodeFactory.CreateBlankNode();
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            return _nodeFactory.CreateBlankNode(nodeId);
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return _nodeFactory.CreateGraphLiteralNode();
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return _nodeFactory.CreateGraphLiteralNode(subgraph);
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return _nodeFactory.CreateLiteralNode(literal, datatype);
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return _nodeFactory.CreateLiteralNode(literal);
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return _nodeFactory.CreateLiteralNode(literal, langspec);
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return _nodeFactory.CreateUriNode(uri);
        }

        public IUriNode CreateUriNode(string qName)
        {
            return _nodeFactory.CreateUriNode(qName);
        }

        public IUriNode CreateUriNode()
        {
            return _nodeFactory.CreateUriNode();
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return _nodeFactory.CreateVariableNode(varname);
        }

        public string GetNextBlankNodeID()
        {
            return _nodeFactory.GetNextBlankNodeID();
        }


        public bool NormalizeLiteralValues { get; set; }
        public Uri ResolveQName(string qName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public IRefNode Name { get; }
        public bool IsEmpty { get; }
        public IEnumerable<INode> Nodes { get; }
        public IEnumerable<INode> AllNodes { get; }
        public BaseTripleCollection Triples { get; }
        public bool Assert(Triple t)
        {
            throw new NotImplementedException();
        }

        public bool Assert(IEnumerable<Triple> ts)
        {
            throw new NotImplementedException();
        }

        public bool Retract(Triple t)
        {
            throw new NotImplementedException();
        }

        public bool Retract(IEnumerable<Triple> ts)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public IBlankNode GetBlankNode(string nodeId)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode GetLiteralNode(string literal, string langspec)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode GetLiteralNode(string literal)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode GetLiteralNode(string literal, Uri datatype)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriples(Uri uri)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriples(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        public IUriNode GetUriNode(string qname)
        {
            throw new NotImplementedException();
        }

        public IUriNode GetUriNode(Uri uri)
        {
            throw new NotImplementedException();
        }

        public bool ContainsTriple(Triple t)
        {
            throw new NotImplementedException();
        }

        public void Merge(IGraph g)
        {
            throw new NotImplementedException();
        }

        public void Merge(IGraph g, bool keepOriginalGraphUri)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        public bool IsSubGraphOf(IGraph g)
        {
            throw new NotImplementedException();
        }

        public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        public bool HasSubGraph(IGraph g)
        {
            throw new NotImplementedException();
        }

        public bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        public GraphDiffReport Difference(IGraph g)
        {
            throw new NotImplementedException();
        }

        public event TripleEventHandler TripleAsserted;
        public event TripleEventHandler TripleRetracted;
        public event GraphEventHandler Changed;
        public event CancellableGraphEventHandler ClearRequested;
        public event GraphEventHandler Cleared;
        public event CancellableGraphEventHandler MergeRequested;
        public event GraphEventHandler Merged;
    }

    public class VirtualDataset : ISparqlDataset
    {
        private readonly List<ITripleProvider> _tripleProviders = new List<ITripleProvider>();

        public void AddTripleProvider(ITripleProvider tripleProvider)
        {
            _tripleProviders.Add(tripleProvider);
        }

        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            throw new NotImplementedException();
        }

        public void SetActiveGraph(IList<IRefNode> graphNames)
        {
            throw new NotImplementedException();
        }

        public void SetActiveGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void SetActiveGraph(IRefNode graphName)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultGraph(IRefNode graphName)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultGraph(IList<IRefNode> graphNames)
        {
            throw new NotImplementedException();
        }

        public void ResetActiveGraph()
        {
            throw new NotImplementedException();
        }

        public void ResetDefaultGraph()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Uri> DefaultGraphUris { get; }
        public IEnumerable<IRefNode> DefaultGraphNames { get; }
        public IEnumerable<Uri> ActiveGraphUris { get; }
        public IEnumerable<IRefNode> ActiveGraphNames { get; }
        public bool UsesUnionDefaultGraph { get; }
        public bool AddGraph(IGraph g)
        {
            throw new NotImplementedException();
        }

        public bool RemoveGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public bool RemoveGraph(IRefNode graphName)
        {
            throw new NotImplementedException();
        }

        public bool HasGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public bool HasGraph(IRefNode graphName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IGraph> Graphs { get; }
        public IEnumerable<Uri> GraphUris { get; }
        public IEnumerable<IRefNode> GraphNames { get; }

        public IGraph this[Uri graphUri] => throw new NotImplementedException();

        public IGraph this[IRefNode graphName] => throw new NotImplementedException();

        public IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public IGraph GetModifiableGraph(IRefNode graphName)
        {
            throw new NotImplementedException();
        }

        public bool HasTriples { get; }
        public bool ContainsTriple(Triple t)
        {
            return _tripleProviders.Where(tp => tp.Predicates.Contains(t.Predicate)).Any(tp => tp.ContainsTriple(t));
        }

        public IEnumerable<Triple> Triples { get; }
        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return _tripleProviders.SelectMany(tp => tp.GetTriplesWithSubject(subj));
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return _tripleProviders.Where(tp => tp.Predicates.Contains(pred))
                .SelectMany(tp => tp.GetTriplesWithPredicate(pred));
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return _tripleProviders.SelectMany(tp => tp.GetTriplesWithObject(obj));
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return _tripleProviders.Where(tp => tp.Predicates.Contains(pred))
                .SelectMany(tp => tp.GetTriplesWithSubjectPredicate(subj, pred));
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return _tripleProviders.SelectMany(tp => tp.GetTriplesWithSubjectObject(subj, obj));
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return _tripleProviders.Where(tp => tp.Predicates.Contains(pred))
                .SelectMany(tp => tp.GetTriplesWithPredicateObject(pred, obj));
        }

        public void Flush()
        {
            // No-op
        }

        public void Discard()
        {
            // No-op
        }
    }

    public interface ITripleProvider
    {
        /// <summary>
        /// Get the list of predicates that this provider generates
        /// </summary>
        IEnumerable<INode> Predicates { get; }

        IEnumerable<Triple> GetTriplesWithSubject(INode subjectNode);
        IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subjectNode, INode predicateNode);
        IEnumerable<Triple> GetTriplesWithSubjectObject(INode subjectNode, INode objectNode);

        IEnumerable<Triple> GetTriplesWithPredicate(INode predicateNode);
        IEnumerable<Triple> GetTriplesWithPredicateObject(INode predicateNode, INode objectNode);
        IEnumerable<Triple> GetTriplesWithObject(INode objectNode);

        bool ContainsTriple(Triple triple);

    }

    public class IntegerTripleProvider : ITripleProvider
    {
        private readonly int _domainMax;
        private readonly INodeFactory _nodeFactory;
        private readonly IList<INode> _predicates;
        private readonly IUriNode _rdfType, _rdfValue, _mathInteger, _mathDouble;
        private readonly Uri _xsdInteger;
        
        public IntegerTripleProvider(int domainMax, INodeFactory factory)
        {
            _domainMax = domainMax;
            _nodeFactory = factory;
            factory.NamespaceMap.AddNamespace("math", new Uri("http://dotnetrdf.org/schema/math#"));
            factory.NamespaceMap.AddNamespace(@"int", new Uri("http://dotnetrdf.org/schema/integer#"));
            _rdfType = factory.CreateUriNode("rdf:type");
            _rdfValue = factory.CreateUriNode("rdf:value");
            _mathDouble = factory.CreateUriNode("math:double");
            _mathInteger = factory.CreateUriNode("math:Integer");
            _xsdInteger = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);

            _predicates = new List<INode>
            {
                _rdfType, _rdfValue, _mathDouble
            };
        }

        public IEnumerable<INode> Predicates { get { return _predicates; } }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subjectNode)
        {
            if (IsDomainNode(subjectNode, out var subjectValue))
            {
                yield return new Triple(subjectNode, _rdfType, _mathInteger);
                yield return new Triple(subjectNode, _rdfValue, CreateIntegerLiteral(subjectValue));
                yield return new Triple(subjectNode, _mathDouble, CreateIntegerNode(subjectValue * 2));
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subjectNode, INode predicateNode)
        {
            if (IsDomainNode(subjectNode, out var subjectValue))
            {
                if (predicateNode.Equals(_rdfType)) yield return new Triple(subjectNode, _rdfType, _mathInteger);
                else if (predicateNode.Equals(_rdfValue))
                    yield return new Triple(subjectNode, _rdfValue, CreateIntegerLiteral(subjectValue));
                else if (predicateNode.Equals(_mathDouble))
                    yield return new Triple(subjectNode, _mathDouble, CreateIntegerNode(subjectValue * 2));
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subjectNode, INode objectNode)
        {
            if (IsDomainNode(subjectNode, out var subjectValue))
            {
                if (objectNode.Equals(_mathInteger))
                {
                    yield return new Triple(subjectNode, _rdfType, objectNode);
                }
                else if (objectNode is ILiteralNode l && l.DataType.Equals(_xsdInteger) &&
                         int.TryParse(l.Value, out var lv) &&
                         lv == subjectValue)
                {
                    yield return new Triple(subjectNode, _rdfValue, objectNode);
                }
                else if (objectNode is IUriNode u && IsIntegerNode(u, out var objectValue) &&
                         objectValue == subjectValue * 2)
                {
                    yield return new Triple(subjectNode, _mathDouble, objectNode);
                }
            }
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode predicateNode)
        {
            if (predicateNode.Equals(_rdfType))
            {
                for (var i = 0; i <= _domainMax; i++)
                {
                    yield return new Triple(CreateIntegerNode(i), _rdfType, _mathInteger);
                }
            }
            else if (predicateNode.Equals(_rdfValue))
            {
                for (var i = 0; i <= _domainMax; i++)
                {
                    yield return new Triple(CreateIntegerNode(i), _rdfValue, CreateIntegerLiteral(i));
                }
            }
            else if (predicateNode.Equals(_mathDouble))
            {
                for (var i = 0; i <= _domainMax; i++)
                {
                    yield return new Triple(CreateIntegerNode(i), _mathDouble, CreateIntegerNode(i * 2));
                }
            }
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode predicateNode, INode objectNode)
        {
            if (predicateNode.Equals(_rdfType) && objectNode.Equals(_mathInteger))
            {
                for (var i = 0; i <= _domainMax; i++)
                {
                    yield return new Triple(CreateIntegerNode(i), predicateNode, objectNode);
                }
            }
            else if (predicateNode.Equals(_rdfValue) && objectNode is ILiteralNode lit &&
                     lit.DataType.Equals(_xsdInteger) &&
                     int.TryParse(lit.Value, out var litValue) && litValue <= _domainMax)
            {

                yield return new Triple(CreateIntegerNode(litValue), predicateNode, objectNode);
            }
            else if (predicateNode.Equals(_mathDouble) && objectNode is IUriNode u && IsIntegerNode(u, out var objectValue))
            {
                if (TryGetDoubleSubject(objectNode, out INode subjectNode))
                {
                    yield return new Triple(subjectNode, predicateNode, objectNode);
                }
            }

        }

        public IEnumerable<Triple> GetTriplesWithObject(INode objectNode)
        {
            if (objectNode.Equals(_mathInteger))
            {
                for (var i = 0; i <= _domainMax; i++)
                {
                    yield return new Triple(CreateIntegerNode(i), _rdfType, objectNode);
                }
            }
            else if (objectNode is ILiteralNode lit && lit.DataType.Equals(_xsdInteger) &&
                     int.TryParse(lit.Value, out var litValue))
            {
                if (litValue >= 0 && litValue <= _domainMax)
                {
                    yield return new Triple(CreateIntegerNode(litValue), _rdfValue, objectNode);
                }
            }
            else if (TryGetDoubleSubject(objectNode, out INode subjectNode))
            {
                yield return new Triple(subjectNode, _mathDouble, objectNode);
            }
        }

        public bool ContainsTriple(Triple t)
        {
            // All triples have a subject node from our domain
            if (!IsDomainNode(t.Subject, out var subjectValue)) return false;

            // rdf:type
            if (t.Predicate.Equals(_rdfType) && t.Object.Equals(_mathInteger)) return true;

            // rdf:value
            if (t.Predicate.Equals(_rdfValue) && IsIntegerLiteral(t.Object, out var objectValue))
                return objectValue == subjectValue;

            // math:double
            if (t.Predicate.Equals(_mathDouble) && IsIntegerNode(t.Object, out var doubleValue))
                return doubleValue == subjectValue * 2;

            return false;
        }

        private bool TryGetDoubleSubject(INode objectNode, out INode subjectNode)
        {
            if (objectNode is IUriNode u && IsIntegerNode(u, out var objValue) && objValue >= 0 &&
                objValue / 2 <= _domainMax && objValue % 2 == 0)
            {
                subjectNode = CreateIntegerNode(objValue / 2);
                return true;
            }

            subjectNode = null;
            return false;
        }

        private ILiteralNode CreateIntegerLiteral(int value)
        {
            return new LiteralNode(value.ToString("D"), _xsdInteger, false);
        }

        private IUriNode CreateIntegerNode(int value)
        {
            return new UriNode(new Uri("http://dotnetrdf.org/schema/integer#" + value.ToString("D")));
        }

        private bool IsDomainNode(INode node, out int value)
        {
            if (IsIntegerNode(node, out value) && value <= _domainMax) return true;
            value = -1;
            return false;
        }

        private bool IsIntegerNode(INode node, out int value)
        {
            if (node is IUriNode uriNode)
            {
                var parts = uriNode.Uri.AbsoluteUri.Split("#");
                if (parts.Length == 2 &&
                    parts[0] == "http://dotnetrdf.org/schema/integer" &&
                    int.TryParse(parts[1], out value)) { return true; }
            }
            value = -1;
            return false;
        }

        private bool IsIntegerLiteral(INode node, out int value)
        {
            if (node is ILiteralNode lit && _xsdInteger.Equals(lit.DataType) &&
                int.TryParse(lit.Value, out value))
            {
                return true;
            }

            value = -1;
            return false;
        }
        
    }

}

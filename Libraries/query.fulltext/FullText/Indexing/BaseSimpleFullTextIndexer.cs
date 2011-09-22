using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText.Indexing
{
    public abstract class BaseSimpleFullTextIndexer
        : BaseFullTextIndexer
    {
        public override void Index(Triple t)
        {
            if (this.IndexingMode == IndexingMode.Custom) throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer which use Custom IndexingMode must override the Index(Triple t) method");

            if (t.Object.NodeType == NodeType.Literal)
            {
                switch (this.IndexingMode)
                {
                    case IndexingMode.Predicates:
                        this.Index(t.Predicate, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Objects:
                        this.Index(t.Object, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Subjects:
                        this.Index(t.Subject, ((ILiteralNode)t.Object).Value);
                        break;

                    default:
                        throw new FullTextQueryException("Indexers deriving from BaseSimpleFullTextIndexer can only use Subjects, Predicates or Objects indexing mode");
                }
            }
        }

        protected abstract void Index(INode n, String text);

        public override void Unindex(Triple t)
        {
            if (this.IndexingMode == IndexingMode.Custom) throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer which use Custom IndexingMode must override the Unindex(Triple t) method");

            if (t.Object.NodeType == NodeType.Literal)
            {
                switch (this.IndexingMode)
                {
                    case IndexingMode.Predicates:
                        this.Unindex(t.Predicate, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Objects:
                        this.Unindex(t.Object, ((ILiteralNode)t.Object).Value);
                        break;
                    case IndexingMode.Subjects:
                        this.Unindex(t.Subject, ((ILiteralNode)t.Object).Value);
                        break;

                    default:
                        throw new FullTextIndexException("Indexers deriving from BaseSimpleFullTextIndexer can only use Subjects, Predicates or Objects indexing mode");
                }
            }
        }

        protected abstract void Unindex(INode n, String text);
    }
}

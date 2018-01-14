namespace Grom
{
    using System;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    // TODO: Maybe no Engine but static builder on Node?
    public partial class Node
    {
        internal readonly INode graphNode;
        private readonly Engine context;

        internal Node(INode node, Engine context)
        {
            this.graphNode = node;
            this.context = context;
        }

        private object Convert(INode tripleObject)
        {
            var valuedNode = tripleObject.AsValuedNode();

            switch (valuedNode)
            {
                case IUriNode uriNode:
                case IBlankNode blankNode:
                    return this.context.GetByNode(valuedNode);

                case StringNode stringNode:
                    return stringNode.AsString();

                case DoubleNode doubleNode:
                    return doubleNode.AsDouble();

                case FloatNode floatNode:
                    return floatNode.AsFloat();

                case DecimalNode decimalNode:
                    return decimalNode.AsDecimal();

                case BooleanNode booleanNode:
                    return booleanNode.AsBoolean();

                case DateTimeNode dateTimeNode:
                    return dateTimeNode.AsDateTimeOffset();

                case TimeSpanNode timeSpanNode:
                    return timeSpanNode.AsTimeSpan();

                case NumericNode numericNode:
                    return numericNode.AsInteger();

                default:
                    throw new ArgumentOutOfRangeException("tripleObject", "Unrecognized node type.");
            }
        }

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is Node))
        //    {
        //        return false;
        //    }

        //    return this.node.Equals((obj as Node).node);
        //}

        //public static bool operator ==(Node first, Node second)
        //{
        //    return first.Equals(second);
        //}

        //public static bool operator !=(Node first, Node second)
        //{
        //    return !(first == second);
        //}

        //public override int GetHashCode()
        //{
        //    return this.node.GetHashCode();
        //}
    }
}

namespace Grom
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class Engine : IEnumerable<Node>
    {
        private List<Node> gromNodes;

        public IEnumerator<Node> GetEnumerator()
        {
            if (this.gromNodes == null)
            {
                var nodes =
                    from node in this.GetNodes()
                    select new Node(node, this);

                this.gromNodes = nodes.ToList();
            }

            return this.gromNodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

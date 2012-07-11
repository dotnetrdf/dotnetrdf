using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.Common.Trees;

namespace VDS.RDF.Test
{
    [TestClass]
    public class BinaryTreeTests
    {
        private void TestOrderPreservationStructs<TNode, TKey>(IEnumerable<TKey> input, ITree<TNode, TKey, TKey> tree)
            where TNode : class, ITreeNode<TKey, TKey>
            where TKey : struct
        {
            Console.Write("Inputs: ");
            List<TKey> inputs = input.ToList();
            TestTools.PrintEnumerableStruct<TKey>(inputs, ",");
            Console.WriteLine();
            foreach (TKey i in inputs)
            {
                tree.Add(i, i);
            }

            //Force a sort of the inputs
            inputs.Sort();

            List<TKey> outputs = tree.Keys.ToList();
            Console.Write("Outputs: ");
            TestTools.PrintEnumerableStruct<TKey>(outputs, ",");
            Console.WriteLine();
            for (int i = 0; i < inputs.Count; i++)
            {
                TKey expected = inputs[i];
                TKey actual = outputs[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void BinaryTreeUnbalancedInsert1()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestOrderPreservationStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 10), tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedInsert2()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestOrderPreservationStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 100), tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedInsert3()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();

            //Randomize the input order
            List<int> pool = Enumerable.Range(1, 100).ToList();
            List<int> input = new List<int>();
            Random rnd = new Random();
            while (pool.Count > 0)
            {
                int r = rnd.Next(pool.Count);
                input.Add(pool[r]);
                pool.RemoveAt(r);
            }

            this.TestOrderPreservationStructs<IBinaryTreeNode<int, int>, int>(input, tree);
        }
    }
}

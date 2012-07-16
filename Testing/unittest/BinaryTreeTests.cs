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
        private Random _rnd = new Random();

        private void TestOrderPreservationOnInsertStructs<TNode, TKey>(IEnumerable<TKey> input, ITree<TNode, TKey, TKey> tree)
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

            //Force a sort of the inputs because initial inputs may be unsorted
            inputs.Sort();
            Console.Write("Sorted Inputs (Expected Output): ");
            TestTools.PrintEnumerableStruct<TKey>(inputs, ",");

            List<TKey> outputs = tree.Keys.ToList();
            Console.Write("Outputs: ");
            TestTools.PrintEnumerableStruct<TKey>(outputs, ",");
            Console.WriteLine();
            this.TestOrderStructs<TKey>(inputs, outputs);
        }

        private void TestOrderPreservationOnDeleteStructs<TNode, TKey>(IEnumerable<TKey> input, ITree<TNode, TKey, TKey> tree)
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

            //Now randomly delete up to half the nodes
            int toDelete = Math.Max(1, this._rnd.Next(Math.Max(2, inputs.Count / 2)));
            Console.WriteLine("Going to delete " + toDelete + " nodes from the Tree");
            while (toDelete > 0)
            {
                int r = this._rnd.Next(inputs.Count);
                TKey key = inputs[r];
                Console.Write("Deleting Key " + key + "...");
                Assert.IsTrue(tree.Remove(key), "Removing Key " + key + " Failed");
                inputs.RemoveAt(r);
                this.TestOrderStructs<TKey>(inputs.OrderBy(k => k, Comparer<TKey>.Default).ToList(), tree.Keys.ToList());
                Assert.AreEqual(inputs.Count, tree.Nodes.Count(), "Removal of Key " + key + " did not reduce node count as expected");
                toDelete--;
            }

            Console.Write("Inputs after Deletions: ");
            TestTools.PrintEnumerableStruct<TKey>(inputs, ",");

            //Force a sort of the inputs because initial inputs may be unsorted
            inputs.Sort();
            Console.Write("Sorted Inputs (Expected Output): ");
            TestTools.PrintEnumerableStruct<TKey>(inputs, ",");
            Console.WriteLine();

            List<TKey> outputs = tree.Keys.ToList();
            Console.Write("Outputs: ");
            TestTools.PrintEnumerableStruct<TKey>(outputs, ",");
            Console.WriteLine();
            this.TestOrderStructs<TKey>(inputs, outputs);
            Console.WriteLine();
        }

        private void TestOrderStructs<TKey>(List<TKey> inputs, List<TKey> outputs)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (i >= outputs.Count) Assert.Fail("Too few outputs, expected " + inputs.Count + " but got " + outputs.Count);
                TKey expected = inputs[i];
                TKey actual = outputs[i];
                Assert.AreEqual(expected, actual, "Expected " + expected + " at Position " + i + " but got " + actual);
            }
        }

        private void PrintBinaryTreeStructs<TNode, TKey>(ITree<TNode, TKey, TKey> tree)
            where TNode : class, IBinaryTreeNode<TKey, TKey>
            where TKey : struct
        {
            Console.Write("Root: ");
            Console.WriteLine(this.PrintBinaryTreeNodeStructs<TNode, TKey>(tree.Root));
        }

        private String PrintBinaryTreeNodeStructs<TNode, TKey>(TNode node)
            where TNode : class, IBinaryTreeNode<TKey, TKey>
            where TKey : struct
        {
            if (node == null)
            {
                return " null";
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("{");
                builder.AppendLine("  Key: " + node.Key);
                builder.AppendLine("  Value: " + node.Value);
                String lhs = this.PrintBinaryTreeNodeStructs<TNode, TKey>((TNode)node.LeftChild);
                if (lhs.Contains('\n'))
                {
                    builder.Append("  Left Child: ");
                    builder.AppendLine(this.AddIndent(lhs));
                }
                else
                {
                    builder.AppendLine("  Left Child: " + lhs);
                }
                String rhs = this.PrintBinaryTreeNodeStructs<TNode, TKey>((TNode)node.RightChild);
                if (rhs.Contains('\n'))
                {
                    builder.Append("  Right Child: ");
                    builder.AppendLine(this.AddIndent(rhs));
                }
                else
                {
                    builder.AppendLine("  Right Child: " + rhs);
                }
                builder.Append("}");
                return builder.ToString();
            }
        }

        private String AddIndent(String input)
        {
            String[] lines = input.Split('\n');
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    output.Append("  " + lines[i]);
                    if (i < lines.Length - 1) output.AppendLine();
                }
                else
                {
                    output.AppendLine(lines[i]);
                }
            }

            return output.ToString();
        }

        [TestMethod]
        public void BinaryTreeUnbalancedInsert1()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 10), tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedInsert2()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 100), tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedInsert3()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();

            //Randomize the input order
            List<int> pool = Enumerable.Range(1, 100).ToList();
            List<int> input = new List<int>();
            while (pool.Count > 0)
            {
                int r = this._rnd.Next(pool.Count);
                input.Add(pool[r]);
                pool.RemoveAt(r);
            }

            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(input, tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedInsert4()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();

            //Randomize the input order
            List<int> pool = Enumerable.Range(1, 1000).ToList();
            List<int> input = new List<int>();
            while (pool.Count > 0)
            {
                int r = this._rnd.Next(pool.Count);
                input.Add(pool[r]);
                pool.RemoveAt(r);
            }

            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(input, tree);
        }

        private UnbalancedBinaryTree<int, int> GetTreeForDelete()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            tree.Root = new BinaryTreeNode<int, int>(null, 2, 2);
            tree.Root.LeftChild = new BinaryTreeNode<int, int>(tree.Root, 1, 1);
            tree.Root.RightChild = new BinaryTreeNode<int, int>(tree.Root, 3, 3);
            return tree;
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete1()
        {
            UnbalancedBinaryTree<int, int> tree = this.GetTreeForDelete();
            this.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            tree.Remove(1);
            this.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            Assert.IsNull(tree.Root.LeftChild, "Left Child should now be null");

            this.TestOrderStructs<int>(new List<int> { 2, 3 }, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete2()
        {
            UnbalancedBinaryTree<int, int> tree = this.GetTreeForDelete();
            this.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            tree.Remove(3);
            this.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            Assert.IsNull(tree.Root.RightChild, "Right Child should now be null");

            this.TestOrderStructs<int>(new List<int> { 1, 2 }, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete3()
        {
            UnbalancedBinaryTree<int, int> tree = this.GetTreeForDelete();
            this.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            tree.Remove(2);
            this.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            this.TestOrderStructs<int>(new List<int> { 1, 3 }, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete4()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            List<int> inputs = Enumerable.Range(1, 100).ToList();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(inputs, tree);
            Assert.IsTrue(tree.Remove(inputs[0]));
            inputs.RemoveAt(0);
            this.TestOrderStructs<int>(inputs, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete5()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            List<int> inputs = Enumerable.Range(1, 100).ToList();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(inputs, tree);
            Assert.IsTrue(tree.Remove(inputs[50]));
            inputs.RemoveAt(50);
            this.TestOrderStructs<int>(inputs, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete6()
        {
            for (int i = 0; i < 10; i++)
            {
                UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 10), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete7()
        {
            for (int i = 0; i < 10; i++)
            {
                UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 100), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete8()
        {
            for (int i = 0; i < 10; i++)
            {
                //Randomize the input order
                List<int> pool = Enumerable.Range(1, 25).ToList();
                List<int> input = new List<int>();
                while (pool.Count > 0)
                {
                    int r = this._rnd.Next(pool.Count);
                    input.Add(pool[r]);
                    pool.RemoveAt(r);
                }

                UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(input, tree);
            }
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete9()
        {
            for (int i = 0; i < 10; i++)
            {
                //Randomize the input order
                List<int> pool = Enumerable.Range(1, 1000).ToList();
                List<int> input = new List<int>();
                while (pool.Count > 0)
                {
                    int r = this._rnd.Next(pool.Count);
                    input.Add(pool[r]);
                    pool.RemoveAt(r);
                }

                UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(input, tree);
            }
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete10()
        {
            List<int> input = new List<int>() { 19,10,20,14,16,5,2,23,9,1,8,4,15,11,24,7,21,13,6,3,22,18,12,17,25 };
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            foreach (int i in input)
            {
                tree.Add(i, i);
            }
            List<int> deletes = new List<int>() { 3, 11, 25, 2, 15, 19 };
            int count = input.Count;
            foreach (int i in deletes)
            {
                Console.WriteLine("Removing Key " + i);
                Assert.IsTrue(tree.Remove(i), "Failed to remove Key " + i);
                input.Remove(i);
                count--;
                this.TestOrderStructs<int>(input.OrderBy(k => k, Comparer<int>.Default).ToList(), tree.Keys.ToList());
                Assert.AreEqual(count, tree.Nodes.Count(), "Removal of Key " + i + " did not reduce node count as expected");
            }
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete11()
        {
            List<int> input = new List<int>() { 25, 14, 5, 8, 22, 17, 9, 12, 4, 1, 3, 23, 2, 7, 19, 20, 10, 24, 16, 6, 21, 13, 18, 11, 15 };
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            foreach (int i in input)
            {
                tree.Add(i, i);
            }
            List<int> deletes = new List<int>() { 3, 14, 25 };
            int count = input.Count;
            foreach (int i in deletes)
            {
                Console.WriteLine("Removing Key " + i);
                Assert.IsTrue(tree.Remove(i), "Failed to remove Key " + i);
                input.Remove(i);
                count--;
                this.TestOrderStructs<int>(input.OrderBy(k => k, Comparer<int>.Default).ToList(), tree.Keys.ToList());
                Assert.AreEqual(count, tree.Nodes.Count(), "Removal of Key " + i + " did not reduce node count as expected");
            }
        }

        [TestMethod]
        public void BinaryTreeScapegoatInsert1()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 10), tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatInsert2()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 100), tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatInsert3()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();

            //Randomize the input order
            List<int> pool = Enumerable.Range(1, 100).ToList();
            List<int> input = new List<int>();
            while (pool.Count > 0)
            {
                int r = this._rnd.Next(pool.Count);
                input.Add(pool[r]);
                pool.RemoveAt(r);
            }

            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(input, tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatInsert4()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();

            //Randomize the input order
            List<int> pool = Enumerable.Range(1, 1000).ToList();
            List<int> input = new List<int>();
            while (pool.Count > 0)
            {
                int r = this._rnd.Next(pool.Count);
                input.Add(pool[r]);
                pool.RemoveAt(r);
            }

            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(input, tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatInsert5()
        {
            for (int i = 1; i < 10; i++)
            {
                ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
                this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, i), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete1()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            List<int> inputs = Enumerable.Range(1, 100).ToList();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(inputs, tree);
            Assert.IsTrue(tree.Remove(inputs[0]));
            inputs.RemoveAt(0);
            this.TestOrderStructs<int>(inputs, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete2()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            List<int> inputs = Enumerable.Range(1, 100).ToList();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(inputs, tree);
            Assert.IsTrue(tree.Remove(inputs[50]));
            inputs.RemoveAt(50);
            this.TestOrderStructs<int>(inputs, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete3()
        {
            for (int i = 0; i < 10; i++)
            {
                ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 10), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete4()
        {
            for (int i = 0; i < 10; i++)
            {
                ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 100), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete5()
        {
            for (int i = 0; i < 10; i++)
            {
                //Randomize the input order
                List<int> pool = Enumerable.Range(1, 25).ToList();
                List<int> input = new List<int>();
                while (pool.Count > 0)
                {
                    int r = this._rnd.Next(pool.Count);
                    input.Add(pool[r]);
                    pool.RemoveAt(r);
                }

                ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(input, tree);
            }
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete6()
        {
            for (int i = 0; i < 10; i++)
            {
                //Randomize the input order
                List<int> pool = Enumerable.Range(1, 1000).ToList();
                List<int> input = new List<int>();
                while (pool.Count > 0)
                {
                    int r = this._rnd.Next(pool.Count);
                    input.Add(pool[r]);
                    pool.RemoveAt(r);
                }

                ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(input, tree);
            }
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete7()
        {
            List<int> input = new List<int>() { 19, 10, 20, 14, 16, 5, 2, 23, 9, 1, 8, 4, 15, 11, 24, 7, 21, 13, 6, 3, 22, 18, 12, 17, 25 };
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            foreach (int i in input)
            {
                tree.Add(i, i);
            }
            List<int> deletes = new List<int>() { 3, 11, 25, 2, 15, 19 };
            int count = input.Count;
            foreach (int i in deletes)
            {
                Console.WriteLine("Removing Key " + i);
                Assert.IsTrue(tree.Remove(i), "Failed to remove Key " + i);
                input.Remove(i);
                count--;
                this.TestOrderStructs<int>(input.OrderBy(k => k, Comparer<int>.Default).ToList(), tree.Keys.ToList());
                Assert.AreEqual(count, tree.Nodes.Count(), "Removal of Key " + i + " did not reduce node count as expected");
            }
        }

        [TestMethod]
        public void BinaryTreeScapegoatDelete8()
        {
            List<int> input = new List<int>() { 25,14,5,8,22,17,9,12,4,1,3,23,2,7,19,20,10,24,16,6,21,13,18,11,15 };
            ScapegoatTree<int, int> tree = new ScapegoatTree<int,int>();
            foreach (int i in input)
            {
                tree.Add(i, i);
            }
            List<int> deletes = new List<int>() { 3, 14, 25 };
            int count = input.Count;
            foreach (int i in deletes)
            {
                Console.WriteLine("Removing Key " + i);
                Assert.IsTrue(tree.Remove(i), "Failed to remove Key " + i);
                input.Remove(i);
                count--;
                this.TestOrderStructs<int>(input.OrderBy(k => k, Comparer<int>.Default).ToList(), tree.Keys.ToList());
                Assert.AreEqual(count, tree.Nodes.Count(), "Removal of Key " + i + " did not reduce node count as expected");
            }
        }
    }
}

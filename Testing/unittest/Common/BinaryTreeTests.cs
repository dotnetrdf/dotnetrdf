/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.Common.Trees;

namespace VDS.RDF.Test.Common
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
            Assert.AreEqual(inputs.Count, outputs.Count, "Expected " + inputs.Count + " Keys in tree but only found " + outputs.Count);

            for (int i = 0; i < inputs.Count; i++)
            {
                if (i >= outputs.Count) Assert.Fail("Too few outputs, expected " + inputs.Count + " but got " + outputs.Count);
                TKey expected = inputs[i];
                TKey actual = outputs[i];
                Assert.AreEqual(expected, actual, "Expected " + expected + " at Position " + i + " but got " + actual);
            }
        }

        #region Unbalance Binary Tree

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
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            Assert.IsNull(tree.Root.LeftChild, "Left Child should now be null");

            this.TestOrderStructs<int>(new List<int> { 2, 3 }, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete2()
        {
            UnbalancedBinaryTree<int, int> tree = this.GetTreeForDelete();
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            Assert.IsNull(tree.Root.RightChild, "Right Child should now be null");

            this.TestOrderStructs<int>(new List<int> { 1, 2 }, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDelete3()
        {
            UnbalancedBinaryTree<int, int> tree = this.GetTreeForDelete();
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

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
        public void BinaryTreeUnbalancedDelete12()
        {
            List<int> input = new List<int>() { 1, 18, 17, 13, 16, 10, 7, 15, 9, 6, 3, 2, 24, 25, 8, 12, 11, 4, 14, 21, 23, 5, 20, 19, 22 };
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            foreach (int i in input)
            {
                tree.Add(i, i);
            }
            List<int> deletes = new List<int>() { 1, 6, 18, 25, 2 };
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

        #endregion

        #region Scapegoat Tree

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

        [TestMethod]
        public void BinaryTreeScapegoatDelete9()
        {
            List<int> input = new List<int>() { 790, 73, 443, 954, 1, 551, 592, 738, 621, 395, 775, 730, 787, 744, 887, 614, 325, 26, 607, 423, 97, 57, 521, 261, 943, 598, 259, 682, 686, 233, 18, 10, 632, 492, 583, 893, 969, 700, 823, 93, 314, 516, 230, 870, 65, 797, 609, 405, 17, 279, 468, 283, 600, 78, 953, 735, 547, 182, 13, 400, 292, 911, 55, 386, 378, 768, 106, 295, 162, 996, 653, 631, 206, 519, 116, 575, 829, 659, 201, 151, 989, 855, 669, 409, 767, 737, 410, 556, 539, 854, 341, 791, 690, 419, 805, 403, 465, 848, 486, 297, 697, 317, 246, 24, 629, 845, 83, 86, 965, 373, 913, 898, 982, 866, 412, 437, 240, 645, 620, 644, 352, 536, 752, 303, 656, 868, 941, 784, 718, 703, 112, 980, 165, 903, 448, 667, 594, 641, 780, 925, 651, 320, 581, 163, 942, 438, 806, 824, 8, 354, 161, 822, 217, 844, 940, 152, 558, 944, 358, 759, 900, 285, 351, 252, 262, 929, 723, 81, 404, 739, 833, 582, 864, 691, 988, 33, 948, 567, 355, 202, 61, 120, 771, 348, 526, 276, 751, 312, 961, 289, 102, 652, 270, 20, 28, 360, 967, 330, 894, 14, 709, 715, 408, 256, 515, 554, 958, 510, 856, 908, 199, 985, 564, 154, 695, 777, 502, 4, 712, 983, 172, 399, 124, 117, 628, 174, 311, 842, 666, 612, 66, 599, 678, 346, 481, 331, 701, 498, 827, 973, 321, 224, 456, 50, 95, 121, 87, 128, 308, 236, 841, 962, 602, 912, 834, 301, 588, 290, 613, 353, 242, 111, 847, 225, 843, 577, 293, 218, 660, 369, 77, 7, 175, 746, 648, 935, 945, 74, 30, 513, 96, 195, 527, 435, 779, 107, 2, 766, 506, 190, 731, 414, 886, 472, 720, 216, 385, 231, 203, 493, 815, 861, 457, 946, 167, 835, 788, 867, 260, 896, 364, 523, 272, 103, 265, 56, 565, 420, 452, 743, 584, 520, 705, 473, 626, 664, 126, 692, 32, 434, 546, 714, 876, 512, 706, 227, 736, 576, 268, 939, 309, 138, 499, 610, 825, 198, 137, 794, 209, 665, 637, 974, 781, 447, 907, 287, 149, 986, 316, 484, 719, 94, 839, 819, 340, 635, 649, 750, 384, 39, 318, 990, 144, 495, 432, 451, 22, 91, 640, 426, 528, 328, 477, 357, 796, 573, 932, 415, 606, 411, 79, 758, 269, 776, 214, 210, 288, 683, 267, 179, 540, 381, 396, 173, 350, 662, 832, 264, 21, 453, 271, 60, 336, 890, 964, 393, 605, 483, 38, 762, 713, 235, 135, 976, 956, 555, 710, 371, 281, 215, 158, 335, 817, 469, 885, 146, 924, 307, 6, 133, 763, 668, 407, 168, 142, 27, 433, 397, 816, 837, 601, 812, 654, 518, 476, 304, 244, 439, 219, 800, 952, 464, 992, 799, 282, 937, 239, 874, 3, 811, 471, 302, 487, 904, 1000, 49, 849, 968, 830, 930, 134, 323, 550, 460, 615, 359, 676, 291, 995, 570, 899, 497, 84, 200, 177, 299, 677, 603, 881, 189, 769, 552, 389, 5, 366, 300, 337, 753, 525, 430, 467, 273, 593, 274, 655, 636, 906, 531, 164, 927, 249, 949, 875, 425, 910, 971, 196, 248, 721, 696, 372, 212, 76, 625, 150, 572, 920, 548, 90, 322, 34, 919, 773, 562, 110, 617, 895, 801, 557, 537, 392, 139, 440, 947, 928, 814, 684, 972, 85, 387, 80, 62, 41, 813, 54, 223, 188, 966, 119, 70, 708, 362, 442, 859, 902, 344, 170, 313, 315, 294, 67, 428, 702, 171, 319, 213, 793, 132, 694, 363, 884, 957, 478, 707, 222, 955, 880, 950, 511, 871, 427, 922, 674, 494, 820, 45, 717, 566, 42, 587, 642, 382, 657, 785, 332, 670, 370, 852, 514, 826, 444, 59, 253, 761, 166, 745, 879, 724, 619, 959, 450, 204, 782, 424, 869, 125, 726, 192, 342, 243, 501, 207, 99, 100, 681, 509, 284, 658, 388, 549, 585, 36, 446, 673, 455, 361, 661, 901, 306, 72, 147, 997, 931, 75, 141, 441, 579, 226, 915, 183, 933, 145, 529, 251, 343, 194, 975, 159, 71, 663, 475, 220, 367, 68, 380, 436, 783, 115, 987, 535, 488, 818, 883, 254, 101, 530, 280, 19, 938, 258, 917, 52, 747, 185, 622, 463, 534, 176, 368, 234, 417, 517, 809, 650, 630, 921, 647, 857, 728, 740, 638, 480, 241, 406, 733, 109, 247, 255, 148, 821, 916, 936, 205, 804, 732, 413, 491, 136, 482, 905, 878, 604, 770, 748, 118, 981, 9, 238, 624, 155, 850, 431, 310, 197, 611, 489, 798, 374, 485, 522, 169, 257, 16, 542, 329, 466, 278, 11, 909, 221, 496, 533, 422, 795, 836, 37, 693, 538, 266, 755, 104, 15, 633, 999, 977, 541, 671, 12, 184, 286, 591, 114, 704, 963, 742, 643, 595, 275, 461, 699, 722, 23, 347, 421, 725, 379, 35, 532, 398, 596, 792, 333, 88, 741, 402, 160, 646, 40, 569, 810, 808, 277, 831, 679, 639, 356, 838, 470, 58, 180, 462, 449, 53, 589, 51, 122, 716, 211, 327, 623, 543, 140, 675, 627, 508, 298, 46, 991, 586, 578, 29, 608, 765, 689, 891, 458, 187, 734, 803, 383, 571, 580, 459, 178, 131, 377, 250, 559, 749, 305, 544, 445, 786, 970, 334, 727, 711, 181, 143, 391, 338, 872, 892, 545, 156, 882, 772, 429, 757, 680, 926, 25, 490, 951, 48, 923, 764, 454, 500, 108, 44, 561, 326, 296, 69, 672, 877, 846, 365, 228, 853, 998, 553, 401, 82, 802, 760, 960, 31, 324, 698, 865, 862, 64, 376, 524, 984, 851, 897, 390, 978, 186, 774, 63, 345, 807, 507, 597, 474, 590, 994, 918, 98, 418, 858, 889, 503, 89, 860, 237, 191, 756, 394, 129, 504, 130, 688, 229, 914, 574, 618, 92, 863, 979, 232, 563, 193, 479, 47, 245, 157, 123, 568, 505, 339, 789, 828, 375, 153, 616, 43, 934, 888, 687, 127, 208, 778, 685, 634, 113, 105, 416, 840, 993, 873, 754, 263, 349, 560, 729 };

            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            foreach (int i in input)
            {
                tree.Add(i, i);
            }
            this.TestOrderStructs<int>(input.OrderBy(k => k, Comparer<int>.Default).ToList(), tree.Keys.ToList());
            List<int> deletes = new List<int>() { 10 };
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

        #endregion

        #region AVL Tree

        [TestMethod]
        public void BinaryTreeAVLInsert1()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 10), tree);
        }

        [TestMethod]
        public void BinaryTreeAVLInsert2()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 100), tree);
        }

        [TestMethod]
        public void BinaryTreeAVLInsert3()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();

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
        public void BinaryTreeAVLInsert4()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();

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
        public void BinaryTreeAVLInsert5()
        {
            for (int i = 1; i < 10; i++)
            {
                AVLTree<int, int> tree = new AVLTree<int, int>();
                this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, i), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeAVLDelete1()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            List<int> inputs = Enumerable.Range(1, 100).ToList();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(inputs, tree);
            Assert.IsTrue(tree.Remove(inputs[0]));
            inputs.RemoveAt(0);
            this.TestOrderStructs<int>(inputs, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeAVLDelete2()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            List<int> inputs = Enumerable.Range(1, 100).ToList();
            this.TestOrderPreservationOnInsertStructs<IBinaryTreeNode<int, int>, int>(inputs, tree);
            Assert.IsTrue(tree.Remove(inputs[50]));
            inputs.RemoveAt(50);
            this.TestOrderStructs<int>(inputs, tree.Keys.ToList());
        }

        [TestMethod]
        public void BinaryTreeAVLDelete3()
        {
            for (int i = 0; i < 10; i++)
            {
                AVLTree<int, int> tree = new AVLTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 10), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeAVLDelete4()
        {
            for (int i = 0; i < 10; i++)
            {
                AVLTree<int, int> tree = new AVLTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(Enumerable.Range(1, 100), tree);
            }
        }

        [TestMethod]
        public void BinaryTreeAVLDelete5()
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

                AVLTree<int, int> tree = new AVLTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(input, tree);
            }
        }

        [TestMethod]
        public void BinaryTreeAVLDelete6()
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

                AVLTree<int, int> tree = new AVLTree<int, int>();
                this.TestOrderPreservationOnDeleteStructs<IBinaryTreeNode<int, int>, int>(input, tree);
            }
        }

        [TestMethod]
        public void BinaryTreeAVLDelete7()
        {
            List<int> input = new List<int>() { 19, 10, 20, 14, 16, 5, 2, 23, 9, 1, 8, 4, 15, 11, 24, 7, 21, 13, 6, 3, 22, 18, 12, 17, 25 };
            AVLTree<int, int> tree = new AVLTree<int, int>();
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
        public void BinaryTreeAVLDelete8()
        {
            List<int> input = new List<int>() { 25, 14, 5, 8, 22, 17, 9, 12, 4, 1, 3, 23, 2, 7, 19, 20, 10, 24, 16, 6, 21, 13, 18, 11, 15 };
            AVLTree<int, int> tree = new AVLTree<int, int>();
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

        #endregion
    }
}

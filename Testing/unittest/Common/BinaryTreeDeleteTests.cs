/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
    public class BinaryTreeDeleteTests
    {
        #region Tree Preparation

        private void PrepareTree1(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with just a root
            //
            //     (1)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 1, 1);

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int,int>, int>(tree);
        }

        private void PrepareTree2(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and a left child
            //
            //     (2)
            //    /
            //  (1)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 2, 2);
            BinaryTreeNode<int, int> leftChild = new BinaryTreeNode<int, int>(null, 1, 1);
            root.LeftChild = leftChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree3(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and a left child
            //
            //     (2)
            //    /
            //  (1)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 1, 1);
            BinaryTreeNode<int, int> rightChild = new BinaryTreeNode<int, int>(null, 2, 2);
            root.RightChild = rightChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree4(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and two children
            //
            //     (2)
            //    /   \
            //  (1)    (3)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 2, 2);
            BinaryTreeNode<int, int> leftChild = new BinaryTreeNode<int, int>(null, 1, 1);
            BinaryTreeNode<int, int> rightChild = new BinaryTreeNode<int, int>(null, 3, 3);
            root.LeftChild = leftChild;
            root.RightChild = rightChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree5(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and two left children
            //
            //     (3)
            //    /   
            //  (2)
            //  /
            //(1)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 3, 3);
            BinaryTreeNode<int, int> leftInnerChild = new BinaryTreeNode<int, int>(null, 2, 2);
            BinaryTreeNode<int, int> leftLeafChild = new BinaryTreeNode<int, int>(null, 1, 1);
            leftInnerChild.LeftChild = leftLeafChild;
            root.LeftChild = leftInnerChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree6(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and a left child which has a right child
            //
            //     (3)
            //    /   
            //  (1)
            //     \
            //     (2)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 3, 3);
            BinaryTreeNode<int, int> leftChild = new BinaryTreeNode<int, int>(null, 1, 1);
            BinaryTreeNode<int, int> rightChild = new BinaryTreeNode<int, int>(null, 2, 2);
            leftChild.RightChild = rightChild;
            root.LeftChild = leftChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree7(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and a right child which has a right child
            //
            //     (1)
            //       \   
            //       (2)
            //         \
            //         (3)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 1, 1);
            BinaryTreeNode<int, int> rightInnerChild = new BinaryTreeNode<int, int>(null, 2, 2);
            BinaryTreeNode<int, int> rightLeafChild = new BinaryTreeNode<int, int>(null, 3, 3);
            rightInnerChild.RightChild = rightLeafChild;
            root.RightChild = rightInnerChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree8(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and a right child which has a left child
            //
            //     (1)
            //       \   
            //       (3)
            //       /
            //     (2)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 1, 1);
            BinaryTreeNode<int, int> rightChild = new BinaryTreeNode<int, int>(null, 3, 3);
            BinaryTreeNode<int, int> leftChild = new BinaryTreeNode<int, int>(null, 2, 2);
            rightChild.LeftChild = leftChild;
            root.RightChild = rightChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree9(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and two children, left child has a left child
            //
            //     (3)
            //    /   \
            //  (2)    (4)
            //  /
            //(1)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 3, 3);
            BinaryTreeNode<int, int> leftChild = new BinaryTreeNode<int, int>(null, 2, 2);
            BinaryTreeNode<int, int> leftLeafChild = new BinaryTreeNode<int, int>(null, 1, 1);
            BinaryTreeNode<int, int> rightChild = new BinaryTreeNode<int, int>(null, 4, 4);
            leftChild.LeftChild = leftLeafChild;
            root.LeftChild = leftChild;
            root.RightChild = rightChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        private void PrepareTree10(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            //Tree with a root and two children, left child has a right child
            //
            //     (3)
            //    /   \
            //  (1)    (4)
            //    \
            //    (2)
            BinaryTreeNode<int, int> root = new BinaryTreeNode<int, int>(null, 3, 3);
            BinaryTreeNode<int, int> leftChild = new BinaryTreeNode<int, int>(null, 1, 1);
            BinaryTreeNode<int, int> rightLeafChild = new BinaryTreeNode<int, int>(null, 2, 2);
            BinaryTreeNode<int, int> rightChild = new BinaryTreeNode<int, int>(null, 4, 4);
            leftChild.RightChild = rightLeafChild;
            root.LeftChild = leftChild;
            root.RightChild = rightChild;

            tree.Root = root;

            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);
        }

        #endregion

        #region Tree Test Methods

        private void TestTree1(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree1(tree);

            //Remove Root
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int,int>, int>(tree);

            //Should lead to empty tree
            Assert.AreEqual(0, tree.Nodes.Count());
            Assert.IsNull(tree.Root);
        }

        private void TestTree2a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree2(tree);

            //Remove Root
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should lead to Left Child as Root
            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
            Assert.IsNull(tree.Root.LeftChild);
        }

        private void TestTree2b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree2(tree);

            //Remove Left Child
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should just leave Root
            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNull(tree.Root.LeftChild);
        }

        private void TestTree3a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree3(tree);

            //Remove Root
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should lead to Right Child as Root
            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNull(tree.Root.RightChild);
        }

        private void TestTree3b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree3(tree);

            //Remove Right Child
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should just leave Root
            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
            Assert.IsNull(tree.Root.RightChild);
        }

        private void TestTree4a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree4(tree);

            //Remove Root
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should lead to Left Child to Root with Right Child as-is
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(3, tree.Root.RightChild.Value);
        }

        private void TestTree4b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree4(tree);

            //Remove Left Child
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Right Child as-is
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(3, tree.Root.RightChild.Value);
        }

        private void TestTree4c(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree4(tree);

            //Remove Right Child
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Left Child as-is
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
        }

        private void TestTree5a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree5(tree);

            //Remove Root
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Left Child to Root
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
        }

        private void TestTree5b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree5(tree);

            //Remove Left Inner Child
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Leaf as Left Child of Root
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
        }

        private void TestTree5c(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree5(tree);

            //Remove Left Leaf Child
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Left Inner Child as-is
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(2, tree.Root.LeftChild.Value);
        }

        private void TestTree6a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree6(tree);

            //Remove Root
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should move right child of Left child to Root
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
        }

        private void TestTree6b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree6(tree);

            //Remove Left Child
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should move right child of left child to left child
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(2, tree.Root.LeftChild.Value);
        }

        private void TestTree6c(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree6(tree);

            //Remove Right Child
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Left Child as-is
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
        }

        private void TestTree7a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree7(tree);

            //Remove Root
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should move Right Child to Root
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(3, tree.Root.RightChild.Value);
        }

        private void TestTree7b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree7(tree);

            //Remove Right Inner Child
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Leaf as Right Child of Root
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(3, tree.Root.RightChild.Value);
        }

        private void TestTree7c(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree7(tree);

            //Remove Right Leaf Child
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Left Inner Child as-is
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(2, tree.Root.RightChild.Value);
        }

        private void TestTree8a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree8(tree);

            //Remove Root
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should move left child of right child to Root
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(3, tree.Root.RightChild.Value);
        }

        private void TestTree8b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree8(tree);

            //Remove Right Child
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should move left child of right child to right child
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(2, tree.Root.RightChild.Value);
        }

        private void TestTree8c(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree8(tree);

            //Remove Left Child
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Right Child as-is
            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(3, tree.Root.RightChild.Value);
        }

        private void TestTree9a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree9(tree);

            //Remove Root
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should lead to Left Child to Root with Right Child as-is
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(4, tree.Root.RightChild.Value);
        }

        private void TestTree9b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree9(tree);

            //Remove Left Inner Child
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Right Child as-is, Left Leaf child should move up
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(4, tree.Root.RightChild.Value);
        }

        private void TestTree9c(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree9(tree);

            //Remove Left Leaf Child
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and immediate children as-is
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(2, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(4, tree.Root.RightChild.Value);
        }

        private void TestTree9d(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree9(tree);

            //Remove Right Child
            tree.Remove(4);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and left sub-tree as-is
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(2, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.LeftChild.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.LeftChild.Value);
        }

        private void TestTree10a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree10(tree);

            //Remove Root
            tree.Remove(3);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should lead to Rightmost Child of Left subtree to Root with Right Child as-is
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(4, tree.Root.RightChild.Value);
        }

        private void TestTree10b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree10(tree);

            //Remove Left Inner Child
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and Right Child as-is, Right Leaf of left subtree should move up
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(2, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(4, tree.Root.RightChild.Value);
        }

        private void TestTree10c(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree10(tree);

            //Remove Right Left of left subtree should leave root and immediate children as=is
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and immediate children as-is
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.RightChild);
            Assert.AreEqual(4, tree.Root.RightChild.Value);
        }

        private void TestTree10d(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree10(tree);

            //Remove Right Child
            tree.Remove(4);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            //Should leave Root and left sub-tree as-is
            Assert.AreEqual(3, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(1, tree.Root.LeftChild.Value);
            Assert.IsNotNull(tree.Root.LeftChild.RightChild);
            Assert.AreEqual(2, tree.Root.LeftChild.RightChild.Value);
        }

        #endregion

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation1()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree1(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation1()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree1(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation1()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree1(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation2a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree2a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation2b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree2b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation2a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree2a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation2b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree2b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation2a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree2a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation2b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree2b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation3a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree3a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation3b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree3b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation3a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree3a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation3b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree3b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation3a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree3a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation3b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree3b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation4a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree4a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation4b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree4b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation4c()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree4c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation4a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree4a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation4b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree4b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation4c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree4c(tree);
        }

        //Tree 7 tests are not run for Scapegoat Tree because they trigger a rebalance which
        //means the tree will not be as we expect
        //NB - Tree 4c, 5 and 6 tests also trigger the rebalance but they always leave the tree in the state
        //we expect so they can be safely run

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation4c()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree4c(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation5a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree5a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation5b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree5b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation5c()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree5c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation5a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree5a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation5b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree5b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation5c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree5c(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation5a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree5a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation5b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree5b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation5c()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree5c(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation6a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree6a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation6b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree6b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation6c()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree6c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation6a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree6a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation6b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree6b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation6c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree6c(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation6a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree6a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation6b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree6b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteValidation6c()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree6c(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation7a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree7a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation7b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree7b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation7c()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree7c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation7a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree7a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation7b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree7b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation7c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree7c(tree);
        }

        //Tree 7 tests are not run for Scapegoat Tree because they trigger a rebalance which
        //means the tree will not be as we expect
        //NB - Tree 4c, 5 and 6 tests also trigger the rebalance but they always leave the tree in the state
        //we expect so they can be safely run

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation8a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree8a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation8b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree8b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteValidation8c()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree8c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation8a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree8a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation8b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree8b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation8c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree8c(tree);
        }

        //Tree 8 tests are not run for Scapegoat Tree because they trigger a rebalance which
        //means the tree will not be as we expect
        //NB - Tree 4c, 5 and 6 tests also trigger the rebalance but they always leave the tree in the state
        //we expect so they can be safely run

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation9a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree9a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation9b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree9b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation9c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree9c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation9d()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree9d(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation10a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree10a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation10b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree10b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation10c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree10c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteValidation10d()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree10d(tree);
        }
    }
}

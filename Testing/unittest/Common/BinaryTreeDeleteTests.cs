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

        #endregion

        #region Tree Test Methods

        private void TestTree1(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree1(tree);

            //Remove Root
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int,int>, int>(tree);

            Assert.AreEqual(0, tree.Nodes.Count());
        }

        private void TestTree2a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree2(tree);

            //Remove Root
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
        }

        private void TestTree2b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree2(tree);

            //Remove Left Child
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
        }

        private void TestTree3a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree3(tree);

            //Remove Root
            tree.Remove(1);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(2, tree.Root.Value);
        }

        private void TestTree3b(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree3(tree);

            //Remove Right Child
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

            Assert.AreEqual(1, tree.Nodes.Count());
            Assert.AreEqual(1, tree.Root.Value);
        }

        private void TestTree4a(ITree<IBinaryTreeNode<int, int>, int, int> tree)
        {
            this.PrepareTree4(tree);

            //Remove Root
            tree.Remove(2);
            BinaryTreeTools.PrintBinaryTreeStructs<IBinaryTreeNode<int, int>, int>(tree);

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

            Assert.AreEqual(2, tree.Nodes.Count());
            Assert.AreEqual(3, tree.Root.Value);
            Assert.IsNotNull(tree.Root.LeftChild);
            Assert.AreEqual(2, tree.Root.LeftChild.Value);
        }

        #endregion

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple1()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree1(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple1()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree1(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple1()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree1(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple2a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree2a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple2b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree2b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple2a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree2a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple2b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree2b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple2a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree2a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple2b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree2b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple3a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree3a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple3b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree3b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple3a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree3a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple3b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree3b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple3a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree3a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple3b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree3b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple4a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree4a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple4b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree4b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple4c()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree4c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple4a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree4a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple4b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree4b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple4c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree4c(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple4a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree4a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple4b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree4b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple4c()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree4c(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple5a()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree5a(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple5b()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree5b(tree);
        }

        [TestMethod]
        public void BinaryTreeAVLDeleteSimple5c()
        {
            AVLTree<int, int> tree = new AVLTree<int, int>();
            this.TestTree5c(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple5a()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree5a(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple5b()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree5b(tree);
        }

        [TestMethod]
        public void BinaryTreeUnbalancedDeleteSimple5c()
        {
            UnbalancedBinaryTree<int, int> tree = new UnbalancedBinaryTree<int, int>();
            this.TestTree5c(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple5a()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree5a(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple5b()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree5b(tree);
        }

        [TestMethod]
        public void BinaryTreeScapegoatDeleteSimple5c()
        {
            ScapegoatTree<int, int> tree = new ScapegoatTree<int, int>();
            this.TestTree5c(tree);
        }
    }
}

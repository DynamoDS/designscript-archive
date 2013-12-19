﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoScript.Runners;
using ProtoTestFx.TD;
using System.Linq;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using System.Collections;

namespace ProtoTest.LiveRunner
{
    public class MicroFeatureTests
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\GraphCompiler\\";
        double tolerance = 0.000001;

        private ILiveRunner astLiveRunner = null;
        private Random randomGen = new Random();

        [SetUp]
        public void Setup()
        {
            GraphToDSCompiler.GraphUtilities.PreloadAssembly(new List<string> {"ProtoGeometry.dll", "Math.dll"});
            astLiveRunner = new ProtoScript.Runners.LiveRunner();
            astLiveRunner.ResetVMAndResyncGraph(new List<string> {"ProtoGeometry.dll", "Math.dll"});
        }

        [TearDown]
        public void CleanUp()
        {
            GraphToDSCompiler.GraphUtilities.Reset();
        }

        [Test]
        public void GraphILTest_Assign01()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
        }

        [Test]
        public void GraphILTest_Assign01_AstInput()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Update graph using AST node input
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(assign);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
        }

        

        [Test]
        public void GraphILTest_Assign01a()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds a node => a = 10;
            // Creates Subtree, Deletes the node,
            // Creates Subtree and sync data and executes it via delta execution
            ////////////////////////////////////////////////////////////////////

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            System.Guid guid1 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid1));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(deletedList, null, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().IsNull);
        }

        [Test]
        public void GraphILTest_Assign02()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds a node => a = 10 + 20;
            // Creates Subtree and sync data and executes it via delta execution
            ////////////////////////////////////////////////////////////////////

            // Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode("10"),
                    new ProtoCore.AST.AssociativeAST.IntNode("20"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign02_AstInput()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds a node => a = 10 + 20;
            // Creates Subtree and sync data and executes it via delta execution
            ////////////////////////////////////////////////////////////////////

            // Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode("10"),
                    new ProtoCore.AST.AssociativeAST.IntNode("20"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            
            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(assign);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign03_astInput()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => a = 10; c = 20; b = a + c;
            // Creates 3 separate Subtrees 
            ////////////////////////////////////////////////////////////////////

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);
            
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);
            
            // update graph with ast input
            CodeBlockNode cNode = new CodeBlockNode();
            cNode.Body = astList;
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(cNode);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign04_astInput()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => a = 10; 
            // executes it
            // Adds node => c = 20;
            // executes it
            // Adds node => b = a + c;
            // executes it
            ////////////////////////////////////////////////////////////////////

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            // update graph
            liveRunner.UpdateGraph(assign1);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);

            // update graph
            liveRunner.UpdateGraph(assign2);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            // update graph
            liveRunner.UpdateGraph(assign3);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign05()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => a = 10; 
            // executes it
            // Adds node => c = 20;
            // executes it
            // Adds node => b = a + c;
            // executes it
            // deletes node => c = 20;
            // executes updated graph
            ////////////////////////////////////////////////////////////////////

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            // Instantiate GraphSyncData
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            //string o = liveRunner.GetCoreDump();

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            addedList = new List<Subtree>();
            System.Guid guid1 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);

            //string o = liveRunner.GetCoreDump();

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);
            addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);

            //o = liveRunner.GetCoreDump();

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(deletedList, null, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().IsNull);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().IsNull);

            //o = liveRunner.GetCoreDump();
        }

        [Test]
        public void GraphILTest_ModifiedNode01()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => c = 78; d = a;
            // Create subtree, execute
            // Adds nodes => a = 10; 
            // Adds node => b = a;
            // Create subtree, execute
            // Modify subtree => a = b;
            // execute updated graph (cylcic dependency should not occur)
            ////////////////////////////////////////////////////////////////////

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign0 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode("78"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign0);

            assign0 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("d"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign0);

            List<Subtree> addedList = new List<Subtree>();
            System.Guid guid0 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid0));

            GraphSyncData syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 78);

            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue(mirror.GetData().IsNull);

            // Build the AST trees
            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            addedList = new List<Subtree>();
            System.Guid guid1 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 78);
            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);
            List<Subtree> modifiedList = new List<Subtree>();
            modifiedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(null, null, modifiedList);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().IsNull);

            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().IsNull);

            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 78);

            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue(mirror.GetData().IsNull);
        }

        [Test]
        public void GraphILTest_FFIClassUsage_01()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            liveRunner.ResetVMAndResyncGraph(libs);

            //==============================================
            // Build the constructor call nodes
            // Point.ByCoordinates(10,10,10)
            //============================================== 
            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            constructorCall.FormalArguments = listArgs;

            string className = "Point";
            ProtoCore.AST.AssociativeAST.IdentifierNode inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);
            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);
            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = p.X;
            //==============================================
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListNode.Optr = ProtoCore.DSASM.Operator.dot;
            identListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("xval"),
                identListNode,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt2);
            //==============================================
            // emit the DS code from the AST tree
            //
            // import("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // xval = p.X;
            //
            //==============================================

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree

            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue((double)mirror.GetData().Data == 10.0);


            ///////////////////////////////////////////////////////////////////////////////
            libs = new List<string>();
            libs.Add("Math.dll");
            libs.Add("ProtoGeometry.dll");
            liveRunner.ResetVMAndResyncGraph(libs);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            constructorCall.FormalArguments = listArgs;

            className = "Point";
            inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);
            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);
            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = p.X;
            //==============================================
            identListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListNode.Optr = ProtoCore.DSASM.Operator.dot;
            identListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            stmt2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("xval"),
                identListNode,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt2);
            //==============================================
            // emit the DS code from the AST tree
            //
            // import("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // xval = p.X;
            //
            //==============================================

            // Instantiate GraphSyncData
            addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            syncData = new GraphSyncData(null, addedList, null);

            liveRunner.UpdateGraph(syncData);


            mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue((double)mirror.GetData().Data == 10.0);


            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("Sin");
            listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("90.0"));

            constructorCall.FormalArguments = listArgs;

            className = "Math";
            inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);
            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("m"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);

            // Instantiate GraphSyncData
            addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            syncData = new GraphSyncData(null, addedList, null);

            liveRunner.UpdateGraph(syncData);


            mirror = liveRunner.InspectNodeValue("m");
            var res = (double)mirror.GetData().Data;
            Assert.IsTrue(res == 1.0);
        }

        [Test]
        public void GraphILTest_FFIClassUsage_01_astInput()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            List<LibraryMirror> libMirrors = liveRunner.ResetVMAndImportLibrary(libs);

            //==============================================
            // Build the constructor call nodes
            // Point.ByCoordinates(10,10,10)
            //============================================== 
            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            constructorCall.FormalArguments = listArgs;

            string className = "Point";
            ProtoCore.AST.AssociativeAST.IdentifierNode inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);
            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);
            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = p.X;
            //==============================================
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListNode.Optr = ProtoCore.DSASM.Operator.dot;
            identListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("xval"),
                identListNode,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt2);
            //==============================================
            // emit the DS code from the AST tree
            //
            // import("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // xval = p.X;
            //
            //==============================================

            // update graph
            CodeBlockNode cNode = new CodeBlockNode();
            cNode.Body = astList;
            liveRunner.UpdateGraph(cNode);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue((double)mirror.GetData().Data == 10.0);


            ///////////////////////////////////////////////////////////////////////////////
            libs = new List<string>();
            libs.Add("Math.dll");
            libs.Add("ProtoGeometry.dll");
            libMirrors = liveRunner.ResetVMAndImportLibrary(libs);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            constructorCall.FormalArguments = listArgs;

            className = "Point";
            inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);
            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);
            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = p.X;
            //==============================================
            identListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListNode.Optr = ProtoCore.DSASM.Operator.dot;
            identListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            stmt2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("xval"),
                identListNode,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt2);
            //==============================================
            // emit the DS code from the AST tree
            //
            // import("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // xval = p.X;
            //
            //==============================================

            cNode.Body = astList;
            liveRunner.UpdateGraph(cNode);


            mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue((double)mirror.GetData().Data == 10.0);


            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("Sin");
            listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("90.0"));

            constructorCall.FormalArguments = listArgs;

            className = "Math";
            inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);
            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("m"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);

            cNode.Body = astList;
            liveRunner.UpdateGraph(cNode);

            mirror = liveRunner.InspectNodeValue("m");
            var res = (double)mirror.GetData().Data;
            Assert.IsTrue(res == 1.0);
        }

        [Test]
        public void GraphILTest_FFIClassUsage_02()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            liveRunner.ResetVMAndResyncGraph(libs);

            //==============================================
            // Build the constructor call nodes
            // Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            constructorCall.FormalArguments = listArgs;

            string className = "Point";
            ProtoCore.AST.AssociativeAST.IdentifierNode inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);

            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);
            //==============================================
            // Translate the point
            // newPoint = p.Translate(1,2,3);
            //==============================================
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCallTranslate = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCallTranslate.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("Translate");
            listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("1.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("2.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("3.0"));
            functionCallTranslate.FormalArguments = listArgs;

            //ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCallTranslate = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("p", functionCallTranslate);
            className = "p";
            inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCallTranslate = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, functionCallTranslate, liveRunner.Core);

            //==============================================
            // Build the binary expression 
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("newPoint"),
                dotCallTranslate,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt2);

            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = newPoint.X
            //==============================================
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("newPoint");
            identListNode.Optr = ProtoCore.DSASM.Operator.dot;
            identListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("xval"),
                identListNode,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt3);
            //==============================================
            // emit the DS code from the AST tree
            //
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue((double)mirror.GetData().Data == 11.0);

        }

        [Test]
        public void GraphILTest_FFIClassUsage_02_astInput()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            List<LibraryMirror> libMirrors = liveRunner.ResetVMAndImportLibrary(libs);

            //==============================================
            // Build the constructor call nodes
            // Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("10.0"));
            constructorCall.FormalArguments = listArgs;

            string className = "Point";
            ProtoCore.AST.AssociativeAST.IdentifierNode inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);

            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);
            //==============================================
            // Translate the point
            // newPoint = p.Translate(1,2,3);
            //==============================================
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCallTranslate = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCallTranslate.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("Translate");
            listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("1.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("2.0"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.DoubleNode("3.0"));
            functionCallTranslate.FormalArguments = listArgs;

            //ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCallTranslate = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("p", functionCallTranslate);
            className = "p";
            inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCallTranslate = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, functionCallTranslate, liveRunner.Core);

            //==============================================
            // Build the binary expression 
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("newPoint"),
                dotCallTranslate,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt2);

            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = newPoint.X
            //==============================================
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("newPoint");
            identListNode.Optr = ProtoCore.DSASM.Operator.dot;
            identListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("xval"),
                identListNode,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt3);
            //==============================================
            // 
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            
            // update graph
            CodeBlockNode cNode = new CodeBlockNode();
            cNode.Body = astList;
            liveRunner.UpdateGraph(cNode);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue((double)mirror.GetData().Data == 11.0);

        }

        [Test]
        public void GraphILTest_FFIClassUsage_03()
        {
            //
            //  a=2;
            //  tSSA_150=1..10;
            //  x= tSSA_150;
            //  tSSA_151=x;
            //  tSSA_152=a;
            //  tSSA_153=( tSSA_151+ tSSA_152);
            //  var_79153f69593b4fde9bb50646a1aaea96= tSSA_153;
            //  tSSA_154=Point.ByCoordinates(var_79153f69593b4fde9bb50646a1aaea96,a,a);
            //  var_347c1113204a4d15a22f7daf83bbe20e= tSSA_154;
            //

            //
            //  a=2;
            //  x=1..10;
            //  var_79153f69593b4fde9bb50646a1aaea96=(x+a);
            //  var_347c1113204a4d15a22f7daf83bbe20e=Point.ByCoordinates(var_79153f69593b4fde9bb50646a1aaea96,a,a);
            //

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            //ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
            //importNode.ModuleName = "ProtoGeometry.dll";
            //astList.Add(importNode);

            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            liveRunner.ResetVMAndResyncGraph(libs);




            // Build the AST trees
            // a = 2
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("2"),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(assign1);


            // x = 1..10;
            ProtoCore.AST.AssociativeAST.RangeExprNode rangeExpr = new ProtoCore.AST.AssociativeAST.RangeExprNode();
            rangeExpr.FromNode = new ProtoCore.AST.AssociativeAST.IntNode("1");
            rangeExpr.ToNode = new ProtoCore.AST.AssociativeAST.IntNode("10");
            rangeExpr.StepNode = new ProtoCore.AST.AssociativeAST.IntNode("1");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                rangeExpr,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(assign2);

            // var_79153f69593b4fde9bb50646a1aaea96 = (x + a);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("dude"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);




            //==============================================
            // Build the constructor call nodes
            // Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.IdentifierNode("dude"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.IdentifierNode("a"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.IdentifierNode("a"));
            constructorCall.FormalArguments = listArgs;

            string className = "Point";
            ProtoCore.AST.AssociativeAST.IdentifierNode inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);

            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("final"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);
            
           
            //==============================================
            // emit the DS code from the AST tree
            //==============================================

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);



        }

        [Test]
        public void GraphILTest_FFIClassUsage_03_astInput()
        {
            //
            //  a=2;
            //  tSSA_150=1..10;
            //  x= tSSA_150;
            //  tSSA_151=x;
            //  tSSA_152=a;
            //  tSSA_153=( tSSA_151+ tSSA_152);
            //  var_79153f69593b4fde9bb50646a1aaea96= tSSA_153;
            //  tSSA_154=Point.ByCoordinates(var_79153f69593b4fde9bb50646a1aaea96,a,a);
            //  var_347c1113204a4d15a22f7daf83bbe20e= tSSA_154;
            //

            //
            //  a=2;
            //  x=1..10;
            //  var_79153f69593b4fde9bb50646a1aaea96=(x+a);
            //  var_347c1113204a4d15a22f7daf83bbe20e=Point.ByCoordinates(var_79153f69593b4fde9bb50646a1aaea96,a,a);
            //

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            List<LibraryMirror> libMirrors = liveRunner.ResetVMAndImportLibrary(libs);

            // Build the AST trees
            // a = 2
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("2"),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(assign1);


            // x = 1..10;
            ProtoCore.AST.AssociativeAST.RangeExprNode rangeExpr = new ProtoCore.AST.AssociativeAST.RangeExprNode();
            rangeExpr.FromNode = new ProtoCore.AST.AssociativeAST.IntNode("1");
            rangeExpr.ToNode = new ProtoCore.AST.AssociativeAST.IntNode("10");
            rangeExpr.StepNode = new ProtoCore.AST.AssociativeAST.IntNode("1");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                rangeExpr,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(assign2);

            // var_79153f69593b4fde9bb50646a1aaea96 = (x + a);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("dude"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);




            //==============================================
            // Build the constructor call nodes
            // Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("ByCoordinates");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.IdentifierNode("dude"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.IdentifierNode("a"));
            listArgs.Add(new ProtoCore.AST.AssociativeAST.IdentifierNode("a"));
            constructorCall.FormalArguments = listArgs;

            string className = "Point";
            ProtoCore.AST.AssociativeAST.IdentifierNode inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall, liveRunner.Core);

            //==============================================
            // Build the binary expression 
            // p = Point.ByCoordinates(10,10,10)
            //==============================================
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("final"),
                dotCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt1);

            // emit the DS code from the AST tree
            CodeBlockNode cNode = new CodeBlockNode();
            cNode.Body = astList;
            liveRunner.UpdateGraph(cNode);
        }

        [Test]
        public void GraphILTest_FFIClassUsage_04()
        {

            //
            //  a=2;
            //  x=1..10;
            //  var_79153f69593b4fde9bb50646a1aaea96=(x+a);
            //  var_347c1113204a4d15a22f7daf83bbe20e=Point.ByCoordinates(var_79153f69593b4fde9bb50646a1aaea96,a,a);
            //

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            //==============================================
            // Build the import Nodes
            //==============================================
            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            liveRunner.ResetVMAndResyncGraph(libs);

            string code = null;
            ProtoCore.AST.Node codeBlockNode = null;
            ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode = null;
            List<ProtoCore.AST.Node> nodes = new List<ProtoCore.AST.Node>();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            code = null;
            nodes = new List<ProtoCore.AST.Node>();
            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            codeBlockNode = null;

            code = @"
a=2;
x=1..10;
y=(x+a);
z=Point.ByCoordinates(y,a,a);
";
            commentNode = null;
            codeBlockNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            nodes = ProtoCore.Utils.ParserUtils.GetAstNodes(codeBlockNode);

            foreach (ProtoCore.AST.Node node in nodes)
            {
                astList.Add(node as ProtoCore.AST.AssociativeAST.AssociativeNode);
            }


            //==============================================
            // emit the DS code from the AST tree
            //==============================================

            Guid guid = System.Guid.NewGuid();
            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, guid));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            const int rep = 2;
            for (int n = 0; n < rep; ++n)
            {

                //////
                code = null;
                nodes = new List<ProtoCore.AST.Node>();
                astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
                codeBlockNode = null;

                code = @"
a = null;
a=2;
x = null;
x=1..10;
y = null;
y=(x+a);
z= null;
z=Point.ByCoordinates(y,a,a);
";
                commentNode = null;
                codeBlockNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
                nodes = ProtoCore.Utils.ParserUtils.GetAstNodes(codeBlockNode);

                foreach (ProtoCore.AST.Node node in nodes)
                {
                    astList.Add(node as ProtoCore.AST.AssociativeAST.AssociativeNode);
                }


                // Instantiate GraphSyncData
                addedList = new List<Subtree>();
                addedList.Add(new Subtree(astList, guid));
                syncData = new GraphSyncData(null, null, addedList);

                // emit the DS code from the AST tree
                liveRunner.UpdateGraph(syncData);

                ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("z");
                var zValues = mirror.GetData().GetElements();
                Assert.IsTrue(zValues != null && zValues.Count == 10);
                Assert.IsTrue(zValues[0].Class.ClassName == "Point");
            }

        }

        [Test]
        public void TestDeltaExpression_01()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("a=10;");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("c=20;");

            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("b = a+c;");

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);

            //o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("c= 30;");

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 40);

            //o = liveRunner.GetCoreDump();
        }

        [Test]
        public void TestDeltaExpression_02()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("x=99;");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue((Int64)mirror.GetData().Data == 99);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("y=x;");

            mirror = liveRunner.InspectNodeValue("y");
            Assert.IsTrue((Int64)mirror.GetData().Data == 99);
            mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue((Int64)mirror.GetData().Data == 99);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("x = 100;");

            mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue((Int64)mirror.GetData().Data == 100);
            mirror = liveRunner.InspectNodeValue("y");
            Assert.IsTrue((Int64)mirror.GetData().Data == 100);
        }

        [Test]
        public void TestDeltaExpressionFFI_01()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            liveRunner.UpdateCmdLineInterpreter(@"import (""ProtoGeometry.dll"");");
            liveRunner.UpdateCmdLineInterpreter("p = Point.ByCoordinates(10,10,10);");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("p");

            //==============================================
            // Translate the point
            // newPoint = p.Translate(1,2,3);
            //==============================================

            liveRunner.UpdateCmdLineInterpreter("newPoint = p.Translate(1,2,3);");
            mirror = liveRunner.InspectNodeValue("newPoint");

            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = newPoint.X
            //==============================================
            liveRunner.UpdateCmdLineInterpreter("xval = newPoint.X;");
            mirror = liveRunner.InspectNodeValue("xval");

            //==============================================
            //
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            Assert.IsTrue((double)mirror.GetData().Data == 11.0);

        }

        [Test]
        public void TestDeltaExpressionFFI_02()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            //string code = @"class Point{ X : double; constructor ByCoordinates(x : double, y : double, z : double){X = x;} def Translate(x : double, y : double, z : double){return = Point.ByCoordinates(11,12,13);} }";

            //liveRunner.UpdateCmdLineInterpreter(code);
            liveRunner.UpdateCmdLineInterpreter(@"import (""ProtoGeometry.dll"");");
            liveRunner.UpdateCmdLineInterpreter("p = Point.ByCoordinates(10,10,10);");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("p");

            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = newPoint.X
            //==============================================
            liveRunner.UpdateCmdLineInterpreter("xval = p.X;");
            mirror = liveRunner.InspectNodeValue("xval");

            //==============================================
            //
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            Assert.IsTrue((double)mirror.GetData().Data == 10.0);

            //==============================================
            // Translate the point
            // newPoint = p.Translate(1,2,3);
            //==============================================

            liveRunner.UpdateCmdLineInterpreter("p = p.Translate(1,2,3);");

            mirror = liveRunner.InspectNodeValue("p");

            mirror = liveRunner.InspectNodeValue("xval");

            //==============================================
            //
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            Assert.IsTrue((double)mirror.GetData().Data == 11.0);

        }

        [Test]
        public void GraphILTest_ComplexWatch01()
        {
            // Build the AST trees
            // x = 1..10;
            ProtoCore.AST.AssociativeAST.RangeExprNode rangeExpr = new ProtoCore.AST.AssociativeAST.RangeExprNode();
            rangeExpr.FromNode = new ProtoCore.AST.AssociativeAST.IntNode("0");
            rangeExpr.ToNode = new ProtoCore.AST.AssociativeAST.IntNode("5");
            rangeExpr.StepNode = new ProtoCore.AST.AssociativeAST.IntNode("1");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                rangeExpr,
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            var collection = mirror.GetData().GetElements();
            Assert.IsTrue((Int64)collection[1].Data == 1);
        }

        [Test]
        public void GraphILTest_ComplexWatch02()
        {
            // Build the AST trees
            // x = 1..10;
            ProtoCore.AST.AssociativeAST.RangeExprNode rangeExpr = new ProtoCore.AST.AssociativeAST.RangeExprNode();
            rangeExpr.FromNode = new ProtoCore.AST.AssociativeAST.IntNode("10");
            rangeExpr.ToNode = new ProtoCore.AST.AssociativeAST.IntNode("100");
            rangeExpr.StepNode = new ProtoCore.AST.AssociativeAST.IntNode("10");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                rangeExpr,
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a[1 + 7]");
            Assert.IsTrue((Int64)mirror.GetData().Data == 90);
        }

        [Test]
        public void GraphILTest_DeletedNode01()
        {
            //====================================
            // Create a = 10 
            // Execute and verify a = 10
            // Delete a = 10
            // Create b = a
            // Execute and verify b = null
            //====================================

            // Create a = 10 
            // Execute and verify a = 10
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            Guid guid = System.Guid.NewGuid();

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, guid));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);




            // Delete a = 10
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(null, guid));
            syncData = new GraphSyncData(deletedList, null, null);
            liveRunner.UpdateGraph(syncData);



            // Create b = a 
            // Execute and verify b = null
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);
            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);

            guid = System.Guid.NewGuid();

            // Instantiate GraphSyncData
            addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, guid));
            syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().IsNull);

        }

        private Subtree CreateSubTreeFromCode(Guid guid, string code)
        {
            CodeBlockNode commentCode;
            var cbn = GraphToDSCompiler.GraphUtilities.Parse(code, out commentCode) as CodeBlockNode; 
            var subtree = null == cbn? new Subtree(null, guid) : new Subtree(cbn.Body, guid);
            return subtree;
        }

        private void AssertValue(string varname, object value)
        {
            var mirror = astLiveRunner.InspectNodeValue(varname);
            MirrorData data = mirror.GetData();
            object svValue = data.Data;
            if (value is double)
            {
                Assert.AreEqual((double)svValue, Convert.ToDouble(value));
            }
            else if (value is int)
            {
                Assert.AreEqual((Int64)svValue, Convert.ToInt64(value));
            }
            else if (value is bool)
            {
                Assert.AreEqual((bool)svValue, Convert.ToBoolean(value));
            }
            else if (value is IEnumerable<int>)
            {
                Assert.IsTrue(data.IsCollection);
                var values = (value as IEnumerable<int>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(Int64)));
            }
            else if (value is IEnumerable<double>)
            {
                Assert.IsTrue(data.IsCollection);
                var values = (value as IEnumerable<double>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(double)));
            }
        }

        [Test]
        public void TestAdd01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "x = a; y = a; z = a; p = Point.ByCoordinates(x, y, z); px = p.X;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);

            int shuffleCount = codes.Count;

            // in which add order, LiveRunner should get the same result.
            for (int i = 0; i < shuffleCount; ++i)
            {
                ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
                liveRunner.ResetVMAndResyncGraph(new List<string> { "ProtoGeometry.dll", "Math.dll" });

                index = index.OrderBy(_ => randomGen.Next());
                var added = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                var syncData = new GraphSyncData(null, added, null);
                liveRunner.UpdateGraph(syncData);

                ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("px");
                var value = (double)mirror.GetData().Data;
                Assert.AreEqual(value, 1);
            }
        }

        [Test]
        public void TestImperative01()
        {
            List<string> codes = new List<string>() 
            {
                "i = [Imperative] {a = 1;b = a;a = 10;return = b;}"
            };
            Guid guid = System.Guid.NewGuid();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guid, codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("i");
            StackValue value = mirror.GetData().GetStackValue();
            Assert.AreEqual(value.opdata, 1);
        }

        [Test]
        public void TestModify01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "x = a; y = a; z = a; p = Point.ByCoordinates(x, y, z); px = p.X;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("px");
            var value = (double)mirror.GetData().Data;
            Assert.AreEqual(value, 1);

            for (int i = 0; i < 10; ++i)
            {
                codes[0] = "a = " + i.ToString() + ";";
                var modified = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                mirror = astLiveRunner.InspectNodeValue("px");
                value = (double)mirror.GetData().Data;
                Assert.AreEqual(value, i);
            }
        }

        [Test]
        public void RegressMAGN747()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
            };
            Guid guid = System.Guid.NewGuid();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guid, codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("a");
            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 1);

            // Simulate delete a = 1 and add CBN a = 2
            int newval = 2;
            codes[0] = "a = " + newval.ToString() + ";";
            var modified = index.Select(idx => CreateSubTreeFromCode(guid, codes[idx])).ToList();

            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(null, guid));

            syncData = new GraphSyncData(deletedList, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            Console.WriteLine("a = " + astLiveRunner.InspectNodeValue("a").GetStringData());
            AssertValue("a", newval);

        }

        [Test]
        public void RegressMAGN750()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "b = a; b = b + 1;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("b");
            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 2);

            for (int i = 0; i < 10; ++i)
            {
                codes[0] = "a = " + i.ToString() + ";";
                var modified = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                Console.WriteLine("b = " + astLiveRunner.InspectNodeValue("b").GetStringData());
                AssertValue("b", i + 1);
            }
        }

       

        [Test]
        public void RegressMAGN750_1()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "b = a; b = b + 1;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("b");
            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 2);

            int newval = 2;
            codes[0] = "a = " + newval.ToString() + ";";
            var modified = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            Console.WriteLine("b = " + astLiveRunner.InspectNodeValue("b").GetStringData());
            AssertValue("b", newval + 1);
            
        }

        [Test]
        public void RegressMAGN753()
        {
            List<string> codes = new List<string>() 
            {
                "t = 1..2;",
                "x = t; a = Math.Abs(x);",
                "z = a; pts = Point.ByCoordinates(z, 10, 2); ptsx = pts.X;"
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            for (int i = 1; i <= 10; ++i)
            {
                codes[0] = "t = 0.." + i.ToString() + ";";
                var modified = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                Console.WriteLine("a = " + astLiveRunner.InspectNodeValue("a").GetStringData());
                AssertValue("a", Enumerable.Range(0, i + 1));

                Console.WriteLine("ptsx = " + astLiveRunner.InspectNodeValue("ptsx").GetStringData());
                AssertValue("ptsx", Enumerable.Range(0, i + 1));

                Console.WriteLine("pts = " + astLiveRunner.InspectNodeValue("pts").GetStringData());
                Assert.IsTrue(!string.IsNullOrEmpty(astLiveRunner.InspectNodeValue("pts").GetStringData()));
            }
        }

        [Test]
        public void RegressMAGN765()
        {
            List<string> codes = new List<string>() 
            {
                "a=10;b=20;c=30;",
                "var1=Point.ByCoordinates(a,b,c);",
                "var2=Point.ByCoordinates(a,a,c);"
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            for (int i = 1; i <= 10; ++i)
            {
                codes[0] = "a=10;b=20;c=" + i.ToString() + ";";
                var modified = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                var var1_value = astLiveRunner.InspectNodeValue("var1").GetStringData();
                Console.WriteLine("var1 = " + var1_value);
                Assert.IsTrue(!string.IsNullOrEmpty(var1_value));

                var var2_value = astLiveRunner.InspectNodeValue("var2").GetStringData();
                Console.WriteLine("var2 = " + var2_value);
                Assert.IsTrue(!string.IsNullOrEmpty(var2_value));
            }
        }

        [Test]
        public void RegressMAGN773()
        {
            List<string> codes = new List<string>() 
            {
                "h=1;",
                "k=h;ll=k+2;",
                "v=ll;hf=v+2;",
                "a45=hf;vv=Point.ByCoordinates(a45, 3, 1);"
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            for (int i = 1; i <= 10; ++i)
            {
                codes[0] = "h=1.." + i.ToString() + ";";

                index = Enumerable.Range(0, 2);
                var modified = index.Select(idx => CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                var strValue = astLiveRunner.InspectNodeValue("vv").GetStringData();
                Console.WriteLine("vv = " + strValue);
                Assert.IsTrue(!string.IsNullOrEmpty(strValue));

                strValue = astLiveRunner.InspectNodeValue("hf").GetStringData();
                Console.WriteLine("hf = " + strValue);
                Assert.IsTrue(!string.IsNullOrEmpty(strValue));
            }
        }

        [Test]
        public void TestFunctionDefinition01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 1; } x = f();"
            };

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);
        }

        [Test]
        public void TestFunctionModification01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { t = 41; return = t;} x = f();",
                "def f() { t1 = 41; t2 = 42; return = {t1, t2}; } x = f();",
                "def f() { t1 = 41; t2 = 42; t3 = 43; return = {t1, t2, t3};} x = f();"
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", 41);
            }

            {
                // Modify the function and verify
                List<Subtree> modified = new List<Subtree>();
                modified.Add(CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", new int[] {41, 42});
            }

            {
                // Modify the function and verify
                List<Subtree> modified = new List<Subtree>();
                modified.Add(CreateSubTreeFromCode(guid, codes[2]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", new int[] { 41, 42, 43 });
            }
        }

        [Test]
        public void TestFunctionModification02()
        {
            // Test function re-defintion but without parameters
            List<string> codes = new List<string>() 
            {
                "def f() { t = 41; return = t;} x = f(); r = Equals(x, 41);",
                "def f() { t1 = 41; t2 = 42; return = {t1, t2}; } x = f(); r = Equals(x, {41, 42});",
                "def f() { t1 = 41; t2 = 42; t3 = 43; return = {t1, t2, t3};} x = f(); r = Equals(x, {41, 42, 43});",
                "def f() { t1 = 43; t2 = 42; t3 = 41; return = {t1, t2, t3};} x = f(); r = Equals(x, {43, 42, 41});",
                "def f() { t1 = t2 + t3; return = t;} x = f(); r = Equals(x, null);",
                "def f() { t1 = 2; t2 = 5; t3 = t1..t2; return = t3;} x = f(); r = Equals(x, {2, 3, 4, 5});",
                "def f() { t1 = 2; t2 = 5; t3 = t1..t2..#2; return = t3;} x = f(); r = Equals(x, {2, 5});",
                "def f() { a = 1; b = 2; c = (a == b) ? 3 : 4; return = c; } x = f(); r = Equals(x, 4);",
                "def f() { a = 2; b = 2; c = (a == b) ? 3 : 4; return = c; } x = f(); r = Equals(x, 3);",
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("r", true);
            }

            IEnumerable<int> indexes = Enumerable.Range(0, codes.Count);
            int shuffleCount = codes.Count;

            for (int i = 0; i < shuffleCount; ++i)
            {
                indexes = indexes.OrderBy(_ => randomGen.Next());

                foreach (var index in indexes)
                {
                    List<Subtree> modified = new List<Subtree>();
                    modified.Add(CreateSubTreeFromCode(guid, codes[index]));

                    var syncData = new GraphSyncData(null, null, modified);
                    astLiveRunner.UpdateGraph(syncData);

                    AssertValue("r", true);
                }
            }
        }

        [Test]
        public void TestFunctionModification03()
        {
            // Test function with same name but with different parameters
            List<string> codes = new List<string>() 
            {
                "def f() { t1 = 1; t2 = 5; return = t1..t2;} x = f(); r = Equals(x, {1, 2, 3, 4, 5});",
                "def f(x) { t = (x > 0) ? 41 : 42; return = t;} x = f(-1); r = Equals(x, 42);",
                "def f(x, y) { t1 = x; t2 = y; return = t1 + t2;} x = f(1, 2); r = Equals(x, 3);",
                "def f(x, y) { return = x..y;} m = 2; n = 6; z1 = f(m, n); z2 = f(); r1 = Equals(z1, {2, 3, 4, 5, 6}); r2 = Equals(z2, {1,2,3,4,5}); r = r1 && r2;",
                "def f(x, y, z) { t1 = x; t2 = y; t3 = z; return = t1 + t2 + t3;} x = f(1, 2, 3); r = Equals(x, 6);",
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("r", true);
            }


            IEnumerable<int> indexes = Enumerable.Range(0, codes.Count);
            int shuffleCount = codes.Count;

            for (int i = 0; i < shuffleCount; ++i)
            {
                indexes = indexes.OrderBy(_ => randomGen.Next());

                foreach (var index in indexes)
                {
                    List<Subtree> modified = new List<Subtree>();
                    modified.Add(CreateSubTreeFromCode(guid, codes[index]));

                    var syncData = new GraphSyncData(null, null, modified);
                    astLiveRunner.UpdateGraph(syncData);

                    AssertValue("r", true);
                }
            }
        }

        [Test]
        public void TestFunctionModification04()
        {
            // Test function re-define and should remove the old function
            List<string> codes = new List<string>() 
            {
                "def f() { return = 41; } x = f(); r1 = Equals(x, 41); y = f(0); r2 = Equals(y, null); r = r1 && r2;",
                "def f(x) { return = 42;} x = f(0); r1 = Equals(x, 42); y = f(); r2 = Equals(y, null); r = r1 && r2;",
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("r", true);
            }


            IEnumerable<int> indexes = Enumerable.Range(0, codes.Count);
            int shuffleCount = codes.Count;

            for (int i = 0; i < shuffleCount; ++i)
            {
                indexes = indexes.OrderBy(_ => randomGen.Next());

                foreach (var index in indexes)
                {
                    List<Subtree> modified = new List<Subtree>();
                    modified.Add(CreateSubTreeFromCode(guid, codes[index]));

                    var syncData = new GraphSyncData(null, null, modified);
                    astLiveRunner.UpdateGraph(syncData);

                    AssertValue("r", true);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoScript.Runners;
using ProtoTestFx.TD;

namespace ProtoTest.LiveRunner
{
    public class MicroFeatureTests
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\GraphCompiler\\";
        double tolerance = 0.000001;
        [SetUp]
        public void Setup()
        {
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(deletedList, null, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);
        }

        [Test]
        public void GraphILTest_Assign03()
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
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            
            astList.Add(assign3);
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            // Instantiate GraphSyncData
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);
        }

        [Test]
        public void GraphILTest_Assign04()
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);
            
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);

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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);

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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);

            //o = liveRunner.GetCoreDump();

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(deletedList, null, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 78);
            
            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);            

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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 78);
            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);

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
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
            
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 78);
            
            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10.0);


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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10.0);


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
            var res = mirror.GetData().GetStackValue().opdata;
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
            //ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
            //importNode.ModuleName = "ProtoGeometry.dll";
            //astList.Add(importNode);

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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 11.0);

        }

        [Test]
        public void TestDeltaExpression_01()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            
            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("a=10;");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("c=20;");

            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);
            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);            

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("b = a+c;");

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);

            //o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("c= 30;");

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 40);

            //o = liveRunner.GetCoreDump();
        }

        [Test]
        public void TestDeltaExpression_02()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("x=99;");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 99);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("y=x;");

            mirror = liveRunner.InspectNodeValue("y");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 99);
            mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 99);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("x = 100;");

            mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 100);
            mirror = liveRunner.InspectNodeValue("y");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 100);
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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 11.0);

        }

        [Test]
        public void TestDeltaExpressionFFI_02()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10.0);

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
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 11.0);

        }
    }
    
}

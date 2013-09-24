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
            addedList.Add(new Subtree(astList));
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
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(astList));

            syncData = new GraphSyncData(deletedList, null, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void GraphILTest_Assign02()
        {
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
            addedList.Add(new Subtree(astList));
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
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList));

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            addedList.Add(new Subtree(astList));

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            
            astList.Add(assign3);
            addedList.Add(new Subtree(astList));

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
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList));

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
            addedList.Add(new Subtree(astList));

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
            addedList.Add(new Subtree(astList));

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
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList));

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
            addedList.Add(new Subtree(astList));

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
            addedList.Add(new Subtree(astList));

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
            deletedList.Add(new Subtree(astList));

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
        public void GraphILTest_FFIClassUsage_01()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
            importNode.ModuleName = "ProtoGeometry.dll";
            astList.Add(importNode);

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

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall);
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
            addedList.Add(new Subtree(astList));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10.0);
            
        }

        [Test]
        public void GraphILTest_FFIClassUsage_02()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            //==============================================
            // Build the import Nodes
            //==============================================
            ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
            importNode.ModuleName = "ProtoGeometry.dll";
            astList.Add(importNode);

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

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, constructorCall);

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

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCallTranslate = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, functionCallTranslate);

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
            addedList.Add(new Subtree(astList));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
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

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);

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
    }
    
}

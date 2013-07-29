using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using GraphToDSCompiler;
using ProtoTestFx.TD;

namespace ProtoTest.GraphCompiler
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

        //
        // TODO Jun: Integrate this helper function into the reflection system
        private List<Obj> GetArrayElements(ProtoCore.Mirror.RuntimeMirror mirror, ProtoCore.DSASM.StackValue svArrayPointer)
        {
            Assert.IsTrue(svArrayPointer.optype == ProtoCore.DSASM.AddressType.ArrayPointer);
            Obj array = new Obj(svArrayPointer);
            return mirror.GetUtils().GetArrayElements(array);
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

            // emit the DS code from the AST tree
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            string code = gc.Emit(astList);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
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

            // emit the DS code from the AST tree
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            string code = gc.Emit(astList);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 30);
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


            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("Point", constructorCall);

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
            // newPoint = p.Translate(1.0, 2.0, 3.0);
            // xval = newPoint.X;
            //
            //==============================================
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            string code = gc.Emit(astList);


            //==============================================
            // Verify the results - get the value of the x property
            //==============================================
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("xval");
            Assert.IsTrue((Double)o.Payload == 10.0);
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
            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("Point", constructorCall);




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
            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCallTranslate = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("p", functionCallTranslate);



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
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            string code = gc.Emit(astList);


            //==============================================
            // Verify the results - get the value of the x property
            //==============================================
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("xval");
            Assert.IsTrue((Double)o.Payload == 11.0);
        }

        [Test]
        public void GraphILTest_FFIClassUsage_03()
        {
            /* def f() {
             *     X = 10;
             *     return = X;
             * }
             */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("X"),
                    new ProtoCore.AST.AssociativeAST.IntNode("10"),
                    ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.IdentifierNode returnExpr = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            ProtoCore.AST.AssociativeAST.ReturnNode returnNode = new ProtoCore.AST.AssociativeAST.ReturnNode();
            returnNode.ReturnExpr = returnExpr;

            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
            cbn.Body.Add(assign1);
            cbn.Body.Add(returnNode);

            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.FunctionBody = cbn;
            funcDefNode.Name = "f";
            funcDefNode.ReturnType = new ProtoCore.Type()
            {
                Name = "int",
                UID = (int)ProtoCore.PrimitiveType.kTypeInt,
                IsIndexable = false,
                rank = 0
            };

            /*Class C { }*/

            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Name = "X";
            ProtoCore.AST.AssociativeAST.IdentifierNode varDeclId = new ProtoCore.AST.AssociativeAST.IdentifierNode()
            {
                Value = "X",
                Name = "X",
                datatype = new ProtoCore.Type()
                {
                    Name = "int",
                    IsIndexable = false,
                    rank = 0,
                    UID = (int)ProtoCore.PrimitiveType.kTypeInt
                }
            };
            varDeclNode.NameNode = varDeclId;

            varDeclNode.ArgumentType = new ProtoCore.Type()
            {
                Name = "int",
                IsIndexable = false,
                rank = 0,
                UID = (int)ProtoCore.PrimitiveType.kTypeVar
            };

            ProtoCore.AST.AssociativeAST.ClassDeclNode classDeclNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDeclNode.className = "C";
            classDeclNode.funclist.Add(funcDefNode);
            classDeclNode.varlist.Add(varDeclNode);

            // p = new C.C(); t = p.f(); val = p.X;

            ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallP = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            funcCallP.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("C");

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            funcCallP.FormalArguments = listArgs;

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode funcDotCallNode = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("C", funcCallP);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignP = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                    funcDotCallNode,
                    ProtoCore.DSASM.Operator.assign
                );
            //p = C.C()

            ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallT = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            funcCallT.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            funcCallT.FormalArguments = listArgs;

            funcDotCallNode = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("p", funcCallT);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignT = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("t"),
                    funcDotCallNode,
                    ProtoCore.DSASM.Operator.assign
                );

            //t = p.f();

            ProtoCore.AST.AssociativeAST.IdentifierListNode idListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            idListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            idListNode.Optr = ProtoCore.DSASM.Operator.dot;
            idListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignVal = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("val"),
                    idListNode,
                    ProtoCore.DSASM.Operator.assign);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(classDeclNode);
            astList.Add(assignP);
            astList.Add(assignT);
            astList.Add(assignVal);

            //==============================================
            // emit the DS code from the AST tree
            //
            // Class C {
            //      X : int
            //      def f() {
            //          x = 2;
            //          return = X;
            //      }
            // }
            // p = new C();
            // t = p.f();
            // val = p.X;            
            //==============================================
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            string code = gc.Emit(astList);



            //==============================================
            // Verify the results - get the value of the x property
            //==============================================
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("val");
            Assert.IsTrue((Int64)o.Payload == 10);
        }

        [Test]
        public void GraphILTest_FFIClassUsage_04()
        {
            /*Class C {
             *  def f: int (X: int, Y:int) {
             *      Z = X + Y + X*Y;
             *      return = Z;
             *  }
             * }
             * p = C.C();
             * t = p.f(4, 5);
             */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("Z"),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("X"),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("Y"),
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("X"),
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("Y"),
                                ProtoCore.DSASM.Operator.mul),
                            ProtoCore.DSASM.Operator.add),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.IdentifierNode returnExpr = new ProtoCore.AST.AssociativeAST.IdentifierNode("Z");
            ProtoCore.AST.AssociativeAST.ReturnNode returnNode = new ProtoCore.AST.AssociativeAST.ReturnNode();
            returnNode.ReturnExpr = returnExpr;

            /**/
            ProtoCore.AST.AssociativeAST.ArgumentSignatureNode argSignatureNode = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode1 = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            ProtoCore.Type type = new ProtoCore.Type();
            type.Initialize();
            type.Name = "int";
            varDeclNode1.ArgumentType = type;
            ProtoCore.AST.AssociativeAST.IdentifierNode nameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode
            {
                Value = "X",
                Name = "X",
                datatype = new ProtoCore.Type()
                {
                    Name = "int",
                    IsIndexable = false,
                    rank = 0,
                    UID = (int)ProtoCore.PrimitiveType.kTypeInt
                }
            };
            varDeclNode1.NameNode = nameNode;
            argSignatureNode.AddArgument(varDeclNode1);

            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode2 = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode2.ArgumentType = type;
            ProtoCore.AST.AssociativeAST.IdentifierNode nameNode2 = new ProtoCore.AST.AssociativeAST.IdentifierNode()
            {
                Name = "Y",
                Value = "Y",
                datatype = new ProtoCore.Type()
                {
                    Name = "int",
                    IsIndexable = false,
                    rank = 0,
                    UID = (int)ProtoCore.PrimitiveType.kTypeInt
                }
            };
            varDeclNode2.NameNode = nameNode2;
            argSignatureNode.AddArgument(varDeclNode2);

            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
            cbn.Body.Add(assign1);
            cbn.Body.Add(returnNode);

            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.FunctionBody = cbn;
            funcDefNode.Name = "f";
            funcDefNode.Singnature = argSignatureNode;
            funcDefNode.ReturnType = new ProtoCore.Type()
            {
                Name = "int",
                UID = (int)ProtoCore.PrimitiveType.kTypeInt,
                IsIndexable = false,
                rank = 0
            };

            // C { }
            ProtoCore.AST.AssociativeAST.ClassDeclNode classDeclNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDeclNode.className = "C";
            classDeclNode.funclist.Add(funcDefNode);

            //p = C.C()
            ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallP = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            funcCallP.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("C");

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            funcCallP.FormalArguments = listArgs;

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode funcDotCallNode = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("C", funcCallP);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignP = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                    funcDotCallNode,
                    ProtoCore.DSASM.Operator.assign
                );

            // t = p.f(4, 5);
            ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallT = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            funcCallT.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs2 = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs2.Add(new ProtoCore.AST.AssociativeAST.IntNode("4"));
            listArgs2.Add(new ProtoCore.AST.AssociativeAST.IntNode("5"));
            funcCallT.FormalArguments = listArgs2;

            funcDotCallNode = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode("p", funcCallT);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignT = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("t"),
                    funcDotCallNode,
                    ProtoCore.DSASM.Operator.assign
            );

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(classDeclNode);
            astList.Add(assignP);
            astList.Add(assignT);
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            string code = gc.Emit(astList);
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("t");
            Assert.IsTrue((Int64)o.Payload == 29);
        }


        [Test]
        public void TestAddition01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "+", "temp");
            object o1 = 10;
            gc.CreateLiteralNode(2, o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            gc.CreateIdentifierNode(4, "A");
            gc.ConnectNodes(1, 0, 4, 1);
            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("A");
            Assert.IsTrue((Int64)o.Payload == 21);
        }
        [Test]
        public void TestSubtraction01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "-", "temp");
            object o1 = 10;
            gc.CreateLiteralNode(2, o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            gc.CreateIdentifierNode(4, "A");
            gc.ConnectNodes(1, 0, 4, 1);
            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("A");
            Assert.IsTrue((Int64)o.Payload == -1);
        }
        [Test]
        public void TestMultiplication01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "*", "temp");
            object o1 = 10;
            gc.CreateLiteralNode(2, o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            gc.CreateIdentifierNode(4, "A");
            gc.ConnectNodes(1, 0, 4, 1);
            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("A");
            Assert.IsTrue((Int64)o.Payload == 110);
        }
        [Test]
        public void TestDivision01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "/", "temp");
            object o1 = 10;
            gc.CreateLiteralNode(2, o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            gc.CreateIdentifierNode(4, "A");
            gc.ConnectNodes(1, 0, 4, 1);
            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("A");
            Assert.IsTrue((Double)o.Payload == 0.90909090909090906);//(10 / 11));
        }
        [Test]
        public void TestSin01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateImportNode(51, "Math.dll");
            gc.CreateFunctionNode(1, "Math.Sin", 1, "temp");
            object o1 = 0;
            gc.CreateLiteralNode(2, o1);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.CreateIdentifierNode(3, "A");
            gc.ConnectNodes(1, 0, 3, 1);
            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("A");
            Assert.IsTrue((Double)o.Payload == 0.0);

        }
        [Test]
        public void TestSin02()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            //gc.CreateImportNode(51, "Math.dll");
            gc.CreateFunctionNode(1, "Math.Sin", 1, "temp2147483649");
            object o1 = 0;
            gc.CreateLiteralNode(2, o1);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.CreateIdentifierNode(3, "A");
            gc.ConnectNodes(1, 0, 3, 0);
            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            string errmsg = "IDE-946 Design script code emission : Math and Protogeometry library functions cannot be queried yet";

            //string myscript = @"temp20001=Math.Sin( 0 );
            //                        A=temp20001;";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(mmx, errmsg);
            Assert.IsTrue(mmx.Equals("temp2147483650=0;\ntemp2147483649=Math.Sin( temp2147483650  );\nA=temp2147483649;"));

            Obj o = mirror.GetValue("temp2147483649");
            Assert.IsTrue(o.Payload == null);
            thisTest.VerifyBuildWarningCount(1);
            // failing here
        }

        [Test]
        public void TestImportMathLib()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();

            gc.CreateImportNode(51, "Math.dll");
            string content = "p = Math.Sin(90);";
            gc.CreateCodeblockNode(1, content);
            gc.CreateIdentifierNode(2, "A");
            gc.ConnectNodes(1, 0, 2, 0);

            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("A");
            Assert.IsTrue((Double)o.Payload == 1.0);
        }

        [Test]
        public void TestCycleDetection()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateIdentifierNode(1, "A");
            gc.CreateIdentifierNode(2, "B");
            gc.CreateIdentifierNode(3, "C");
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 2, 0);
            gc.ConnectNodes(1, 0, 3, 0);
            string mmx = gc.gcs.Errors[0];
            Assert.IsTrue(mmx.Equals("Edge between 'C' and 'A' introduces a cycle in the graph"));
        }

        [Test]
        public void TestInputOutputInfo()
        {
            Dictionary<string, int> inputVariables = new Dictionary<string, int>();
            Dictionary<int, string> outputLines = new Dictionary<int, string>();
            List<ProtoCore.BuildData.WarningEntry> warningslist = new List<ProtoCore.BuildData.WarningEntry>();
            List<string> errorslist = new List<string>();
            GraphUtilities.GetInputOutputInfo(@"a = b + 2;
//
                c = 4 * z; 
                d = 5*2;
f=d+g+e;
//            ", out warningslist, out errorslist, inputVariables, outputLines);

            Assert.IsTrue(outputLines[5] == "f");
            Assert.IsTrue(outputLines[4] == "d");
            Assert.IsTrue(outputLines[3] == "c");
            Assert.IsTrue(outputLines[1] == "a");
            Assert.IsTrue(inputVariables["g"] == 5);
            Assert.IsTrue(inputVariables["e"] == 5);
            inputVariables.Clear();
            outputLines.Clear();
            warningslist.Clear();
            errorslist.Clear();
        }

        [Test]
        public void TestInputOutputInfoForArrays1()
        {
            Dictionary<string, int> inputVariables = new Dictionary<string, int>(); Dictionary<int, string> outputLines = new Dictionary<int, string>();
            List<ProtoCore.BuildData.WarningEntry> warningslist = new List<ProtoCore.BuildData.WarningEntry>();
            List<string> errorslist = new List<string>();
            GraphUtilities.GetInputOutputInfo(@"a = {1,2,3};
//
                a[0] = 10;
                a[1] = 20;
a[2] = 30;
//            ", out warningslist, out errorslist, inputVariables, outputLines);

            Assert.IsTrue(outputLines[5] == "a[2]");
            Assert.IsTrue(outputLines[4] == "a[1]");
            Assert.IsTrue(outputLines[3] == "a[0]");
            Assert.IsTrue(outputLines[1] == "a");
            Assert.IsTrue(inputVariables.Count == 0);
            inputVariables.Clear();
            outputLines.Clear();
            warningslist.Clear();
            errorslist.Clear();
        }



        [Test]
        public void TestDeltaExecution01()
        {
            //===========================================================================
            // Creates: 
            //      a = 10
            //
            // Adds: 
            //      b = 20    
            //===========================================================================


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();


            //===========================================================================
            // Build the first snapshot nodes a = 10
            //===========================================================================
            GraphToDSCompiler.SynchronizeData data = new GraphToDSCompiler.SynchronizeData();

            uint uidIdent = 1;
            uint uidLiteral = 2;

            Connection k1 = new Connection();
            k1.LocalIndex = 0;
            k1.OtherIndex = 0;
            k1.OtherNode = uidLiteral;
            Connection k2 = new Connection();
            k2.LocalIndex = 0;
            k2.OtherIndex = 0;
            k2.OtherNode = uidIdent;

            SnapshotNode n1 = new SnapshotNode(uidIdent, SnapshotNodeType.Identifier, "a");
            n1.InputList.Add(k1);

            SnapshotNode n2 = new SnapshotNode(uidLiteral, SnapshotNodeType.Literal, "10");
            n2.OutputList.Add(k2);
            data.AddedNodes.Add(n1);
            data.AddedNodes.Add(n2);


            //===========================================================================
            // Compile the current graph
            //===========================================================================
            liveRunner.UpdateGraph(data);


            //===========================================================================
            // Build the first snapshot nodes b = 10
            //===========================================================================

            uint uidIdent2 = 10;
            uint uidLiteral2 = 20;

            data = new GraphToDSCompiler.SynchronizeData();
            k1 = new Connection();
            k1.LocalIndex = 0;
            k1.OtherIndex = 0;
            k1.OtherNode = uidLiteral2;
            k2 = new Connection();
            k2.LocalIndex = 0;
            k2.OtherIndex = 0;
            k2.OtherNode = uidIdent2;

            n1 = new SnapshotNode(uidIdent2, SnapshotNodeType.Identifier, "b");
            n1.InputList.Add(k1);

            n2 = new SnapshotNode(uidLiteral2, SnapshotNodeType.Literal, "20");
            n2.OutputList.Add(k2);
            data.AddedNodes.Add(n1);
            data.AddedNodes.Add(n2);


            //===========================================================================
            // Compile the current graph and added graph
            //===========================================================================
            liveRunner.UpdateGraph(data);



            //===========================================================================
            // Verify the value of 'a'
            //===========================================================================
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue(uidIdent);
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);



            //===========================================================================
            // Verify the value of 'b'
            //===========================================================================
            mirror = liveRunner.QueryNodeValue(uidIdent2);
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);

        }

        [Test]
        public void TestDeltaExecution02()
        {
            //===========================================================================
            // Creates: 
            //      literal node 10
            //      literal node 20
            //      and using '+' operator node and assign it to idetifier

            //===========================================================================
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            //===========================================================================
            // Build the first literal node  10
            //===========================================================================
            GraphToDSCompiler.SynchronizeData data = new GraphToDSCompiler.SynchronizeData();

            uint uidLiteral1 = 1;
            uint uidLiteral2 = 2;
            uint uidOp1 = 3;
            uint uidIdent1 = 4;

            Connection k1 = new Connection();
            k1.LocalIndex = 0;// output slot
            k1.OtherIndex = 0;// input slot
            k1.OtherNode = uidOp1;

            Connection k2 = new Connection();
            k2.LocalIndex = 0;
            k2.OtherIndex = 1;
            k2.OtherNode = uidOp1;


            Connection k4 = new Connection();
            k4.LocalIndex = 0;
            k4.OtherIndex = 0;
            k4.OtherNode = uidLiteral1;

            Connection k5 = new Connection();
            k5.LocalIndex = 1;
            k5.OtherIndex = 0;
            k5.OtherNode = uidLiteral2;

            Connection k6 = new Connection();
            k6.LocalIndex = 0;
            k6.OtherIndex = 0;
            k6.OtherNode = uidIdent1;

            Connection k7 = new Connection();
            k7.LocalIndex = 0;
            k7.OtherIndex = 0;
            k7.OtherNode = uidOp1;

            SnapshotNode n1 = new SnapshotNode(uidLiteral1, SnapshotNodeType.Literal, "10");
            n1.OutputList.Add(k1);

            SnapshotNode n2 = new SnapshotNode(uidLiteral2, SnapshotNodeType.Literal, "20");
            n2.OutputList.Add(k2);

            SnapshotNode n3 = new SnapshotNode(uidOp1, SnapshotNodeType.Function, ";+;double,double;temp");
            n3.InputList.Add(k4);
            n3.InputList.Add(k5);
            n3.OutputList.Add(k6);

            SnapshotNode n4 = new SnapshotNode(uidIdent1, SnapshotNodeType.Identifier, "A");
            n4.InputList.Add(k7);
            data.AddedNodes.Add(n1);
            data.AddedNodes.Add(n2);
            data.AddedNodes.Add(n3);
            data.AddedNodes.Add(n4);

            //===========================================================================
            // Compile the current graph
            //===========================================================================

            liveRunner.UpdateGraph(data);


            //===========================================================================
            // Verify the value of Identifier
            //===========================================================================
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue(uidIdent1);
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);

        }
        [Test]
        public void TestDeltaExecution03()
        {
            //===========================================================================
            // Creates: 
            //      driver  node a = 10
            //      driver node b= 20
            //      and using '+' operator node and assign it to idetifier

            //===========================================================================
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            //===========================================================================
            // Build the driver node a=10 
            //===========================================================================
            GraphToDSCompiler.SynchronizeData data = new GraphToDSCompiler.SynchronizeData();

            uint uidDriver1 = 1;
            uint uidDriver2 = 2;
            uint uidOp1 = 3;
            uint uidIdent1 = 4;

            Connection k1 = new Connection();
            k1.LocalIndex = 0;// output slot
            k1.OtherIndex = 0;// input slot
            k1.OtherNode = uidOp1;
            k1.LocalName = "a";

            Connection k2 = new Connection();
            k2.LocalIndex = 0;
            k2.OtherIndex = 1;
            k2.OtherNode = uidOp1;
            k2.LocalName = "b";

            Connection k4 = new Connection();
            k4.LocalIndex = 0;
            k4.OtherIndex = 0;
            k4.OtherNode = uidDriver1;


            Connection k5 = new Connection();
            k5.LocalIndex = 1;
            k5.OtherIndex = 0;
            k5.OtherNode = uidDriver2;


            Connection k6 = new Connection();
            k6.LocalIndex = 0;
            k6.OtherIndex = 0;
            k6.OtherNode = uidIdent1;


            Connection k7 = new Connection();
            k7.LocalIndex = 0;
            k7.OtherIndex = 0;
            k7.OtherNode = uidOp1;

            SnapshotNode n1 = new SnapshotNode(uidDriver1, SnapshotNodeType.CodeBlock, "a=10;");
            n1.OutputList.Add(k1);

            SnapshotNode n2 = new SnapshotNode(uidDriver2, SnapshotNodeType.CodeBlock, "b=20;");
            n2.OutputList.Add(k2);

            SnapshotNode n3 = new SnapshotNode(uidOp1, SnapshotNodeType.Function, ";+;double,double;temp");
            n3.InputList.Add(k4);
            n3.InputList.Add(k5);
            n3.OutputList.Add(k6);

            SnapshotNode n4 = new SnapshotNode(uidIdent1, SnapshotNodeType.Identifier, "A");
            n4.InputList.Add(k7);
            data.AddedNodes.Add(n1);
            data.AddedNodes.Add(n2);
            data.AddedNodes.Add(n3);
            data.AddedNodes.Add(n4);

            //===========================================================================
            // Compile the current graph
            //===========================================================================

            liveRunner.UpdateGraph(data);


            //===========================================================================
            // Verify the value of Identifier
            //===========================================================================
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue(uidIdent1);
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);

        }


        [Test]
        public void TestAnalyzeString01()
        {
            //Assert.IsTrue(SnapshotNodeType.Identifier == GraphUtilities.AnalyzeString("abc;"));
            GraphUtilities.AnalyzeString("ab;");
            GraphUtilities.AnalyzeString("1 + 4;");
            GraphUtilities.AnalyzeString("a = 1 + 2;");
            GraphUtilities.AnalyzeString("1..5;");
            GraphUtilities.AnalyzeString("def f(){return = 10;}");
            GraphUtilities.AnalyzeString("class F{constructor doo(){}}");
        }

        [Test]
        public void TestGetInputOutputInfo()
        {
            string compliableInput = "t1 = a + b;\n t2 = c;\n t3 = d \n + \n e;\n t4 = f \n ; \n g = h ; \n i = j + h; \n \n k = m = l; \n n = n + n; \n";

            Dictionary<int, List<VariableLine>> inputLines = new Dictionary<int, List<VariableLine>>();
            HashSet<VariableLine> outputLines = new HashSet<VariableLine>();
            bool complete = GraphUtilities.GetInputOutputInfo(compliableInput, inputLines, outputLines);

            //compare input
            Assert.AreEqual(8, inputLines.Count);
            // IL: [ 1, [ { a, 1 }, { b, 1 } ] ]
            Assert.AreEqual("a", (inputLines[1])[0].variable);
            Assert.AreEqual(1, (inputLines[1])[0].line);
            Assert.AreEqual("b", (inputLines[1])[1].variable);
            Assert.AreEqual(1, (inputLines[1])[1].line);
            //[ 2, [ { c, 2 } ] ]
            Assert.AreEqual("c", (inputLines[2])[0].variable);
            Assert.AreEqual(2, (inputLines[2])[0].line);
            //[ 3, [ { d, 3 }, { e, 5 } ] ]
            Assert.AreEqual("d", (inputLines[3])[0].variable);
            Assert.AreEqual(3, (inputLines[3])[0].line);
            Assert.AreEqual("e", (inputLines[3])[1].variable);
            Assert.AreEqual(5, (inputLines[3])[1].line);
            //[ 4, [ { f, 6 } ] ]
            Assert.AreEqual("f", (inputLines[4])[0].variable);
            Assert.AreEqual(6, (inputLines[4])[0].line);
            //[ 5, [ { h, 8 } ] ]
            Assert.AreEqual("h", (inputLines[5])[0].variable);
            Assert.AreEqual(8, (inputLines[5])[0].line);
            //[ 6, [ { j, 9 } ] ]
            Assert.AreEqual("j", (inputLines[6])[0].variable);
            Assert.AreEqual(9, (inputLines[6])[0].line);
            //[ 7, [ { l, 11 } ] ]
            Assert.AreEqual("l", (inputLines[7])[0].variable);
            Assert.AreEqual(11, (inputLines[7])[0].line);
            //[ 8, [ { n, 12 } ] ]
            Assert.AreEqual("n", (inputLines[8])[0].variable);
            Assert.AreEqual(12, (inputLines[8])[0].line);

            //Compare output
            Assert.AreEqual(9, outputLines.Count);
            //{ t1,  1 },
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("t1", 1)));
            //{ t2,  2 },
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("t2", 2)));
            //{ t3,  3 },
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("t3", 3)));
            //{ t4,  6 },
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("t4", 6)));
            //{  g,  8 },
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("g", 8)));
            //{  i,  9 },
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("i", 9)));
            //{  k, 11 }, { m, 11 }, // Multiple output slots for this line?
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("k", 11)));
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("m", 11)));
            //{  n, 12 }
            Assert.AreEqual(true, outputLines.Contains(new VariableLine("n", 12)));
        }

        [Test]
        public void TestGetInputOutputInfo_02()
        {
            string compliableInput = "p1 = Cuboid.ByLengths(WCS, 10, 10, 10);";

            Dictionary<int, List<VariableLine>> inputLines = new Dictionary<int, List<VariableLine>>();
            HashSet<VariableLine> outputLines = new HashSet<VariableLine>();
            bool complete = GraphUtilities.GetInputOutputInfo(compliableInput, inputLines, outputLines);

            Assert.AreEqual(0, inputLines.Count);
            Assert.AreEqual(1, outputLines.Count);
        }

        [Test]
        public void T001_MultipleAssignments()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();


            gc.CreateIdentifierNode(1, "A");
            gc.CreateIdentifierNode(2, "B");
            gc.CreateIdentifierNode(3, "C");
            gc.ConnectNodes(1, 0, 2, 0);
            gc.ConnectNodes(2, 0, 3, 0);

            object o1 = 10;
            gc.CreateLiteralNode(4, o1);
            gc.ConnectNodes(4, 0, 1, 0);

            string mmx = gc.PrintGraph();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("A");
            Assert.IsTrue((Int64)o.Payload == 10);

        }

        [Test]
        public void TestSD()
        {
            // Creation - Create   code block and assign it to identifier
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSD.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 3);
        }
        [Test]
        public void TestSD_function()
        {
            // creation-function node- create two code block nodes and assign it to + operator
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDFunction.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 3);
        }
        [Test]
        public void T002_TestArrayIndexing()
        {
            //1. Create a CBN : a = {1,2}
            //2. Create another CBN : b = [0];
            //3. Connect output of last CBN to input of an identifier 'c' => verify 'c' = 1

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T002_TestArrayIndexing.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
        }

        [Test]
        public void TestSD_Modify()
        {
            // modify code block - 
            // 1. create two code block nodes with values 10 and 20
            // 2. add it using + operator 
            // 3. assign it to identifier
            // 4. modify the code block value from 20 to 30 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDModify.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("I268435460");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 40);
        }
        [Test]
        public void TestSD_Delete()
        {
            // create code block assign it to identifier
            // delete code block 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDDelete.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("I268435458");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
            //Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }
        [Test]
        public void TestSD_Replace()
        {
            // create code block assign it to identifier
            // create another code block and connect to identifer

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDReplace.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("I268435458");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);
        }
        [Test]
        public void TestReconnect_1278()
        {
            //1. Create two code block 
            //2. Connect one to the identifer
            //3. Reconnect the other code block to the identifer
            //4. undo , unless i click on the canvas its not updated

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDReconnect_Undo.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
        }
        [Test]
        public void TestSD_Reconnect()
        {
            // create code block assign it to identifier
            // create another code block and reconnect to identifer

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDReconnect.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 2);
        }
        [Test]
        public void TestSD_DeleteConnection()
        {
            // create code block assign it to identifier
            // delete connection

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDDeleteConnection.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("I268435458");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }
        [Test]
        public void TestSD_RenameVariableName()
        {
            // create code block "a=10"assign it to identifier
            // change the variable name of the code block

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testSDRenamevariable.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("I268435458");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
        }
        [Test]
        public void Crossreference()
        {
            // 1. Create a code block node a=1
            // 2. Create another one b=a and assign an indentifer

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "CrossReference.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("I268435459");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
        }
        [Test]
        public void Crossreference_delete_1294()
        {
            // 1. Create a code block node a=1
            // 2. Create another one b = a and assign an indentifer
            // 3. Delete the first node and  identifier does not update 


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "CrossReference_Delete_1294.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }
        [Test]
        public void Crossreference_modify()
        {
            // 1. Create a code block node a=1
            // 2. Create another one b=a and assign an indentifer
            // 3. modify value of a


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "CrossReference_modify.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("I268435459");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
        }
        [Test]
        public void GraphNotComputing_Islandnodes_1280()
        {
            //deffect IDE-1280
            // 1. Create a code block node
            // 2. Create an identifier
            // 3. drag and drop a code block node and leave it empty


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "GraphNotComputing_1280.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
        }


        [Test]
        public void TestUndoDelete_1296()
        {
            //deffect IDE-1296
            // 1. Create two driver nodes values 1 and 2
            // 2. Connect each to identifier node
            // 3. select all delete
            // 4. Undo


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "UndoDelete_1296.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue(mirror2.GetData().GetStackValue().opdata == 2);
        }

        [Test]
        public void TestNonExistentnodes()
        {
            // non existent node - 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "null.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("var1");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void Reconnect_CB_DifferentSlot_1294()
        {
            // Deffect -1294
            // 1. Create multiline code block with a=10; b=20;
            // 2. Create an identifier
            // 3. Connect the first one to the identifier change the connection to the second the preview does not work 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "Reconnect_CB_DifferentSlot.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 20);
        }
        [Test]
        public void CreateGraphWithNoOrder_1277()
        {
            // Deffect -1277
            // 1. create code block node
            // 2. create math/+ node
            // 3. create identifier
            // 4. connection code block  to math and then to identifer works fine 
            // 5. but connect math node to identifier and then code block  to math does not update preview of math


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "CreateGraph.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var1");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 3);
        }
        [Test]
        public void OneInputOutputNodes_1279()
        {
            // Deffect -1279
            // 1. create a code block and two identifier
            // 2. connect code block to two identifiers preview for  second one does not load 


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "manyoutputnodes.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror2.GetData().GetStackValue().opdata == 1);
        }
        [Test]
        public void T010_VariableDeclarationDependancy_1369()
        {
            //1.  Create a code block node a= 10
            //2.  Assign it to Identifier named b
            //3.  create one more code block and assign the value of identifier to it , eg: c= b


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testDependancy.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T014_VariableDeclarationDependancy_2()
        {
            //1.  Create two code block nodes assign it + operator 
            //2.  Assign it to an identifier Var3
            //3.  Create one more code block and assign the value of identifier to it , eg: c= Var3


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testDependancy2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 11);

        }

        [Test]
        public void T015_VariableDeclarationDependancy_3()
        {
            //1.  Create a driver node var1
            //2.  Create another code block node and assign the same to var2



            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testDependancy3.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 0);

        }
        [Test]
        public void T016_VariableDeclarationDependancy_4()
        {
            //1.  Create a code block node a= 10
            //2.  Create another code block and assign b=a



            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "testDependancy4.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);

        }
        [Test]
        public void T017_TestDriverNode()
        {
            //1.  Create a driver node
            //2.  Change the value of variable to a and value to 2



            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "DriverNode.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 2);

        }
        [Test]
        public void T018_TestDriverNode()
        {
            //1. Create a code block node and assign value 1 
            //2. Create a driver node
            //3. Change the value of driver node to variable a 
            //4. Assign it to identifier - Var3 



            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "DriverNode_Crossreference.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T019_TestDriverNodeDeletion()
        {

            //1. Create a driver node
            //2. Assign it to identifier - Var2 
            //3. Connect them
            //4. Delete the driver node


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "DriverNode_Delete.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }
        [Test]
        public void T020_DriverNodeArray_TestCaseDefect_IDE_1856()
        {
            // Failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856

            //Steps : Create a driver node - Var 1 - {0,1} => verify the valye is null, as this is will throw a compiler error
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "DriverNode_Array.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var1");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 0);
            mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        public void T021_DeleteConnection()
        {

            //1. Create a code block and assign it to identifier node named var2
            //2. Delete the connection between the codeblock and identifier

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T021_DeleteConnection_1276.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }
        [Test]
        public void T022_DeleteConnection_1395()
        {

            //1. Create two code block nodes and assign it + operator 
            //2. Connect the + operator node to identifier named Var4
            //3. Delete connection between the + and identifier 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T022_DeleteConnection.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }
        [Test]
        public void T023_DeleteConnection_Undo()
        {

            //1. Create a code block and assign it to identifier node named var2
            //2. Delete the connection between the codeblock and identifier
            //3. Undo 
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T023_DeleteConnection_Undo.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T024_DeleteConnection_CBN2()
        {

            // 1. Create a code block node with expression a=1 and conenct it to identifier
            // 2. Delete connection
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T024_Deleteconnection_CBN2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }
        [Test]
        public void T025_MultipleOutput_DeleteConnection()
        {
            // 1. Create a code block node a=1 and connect it two identifiers 
            // 2. Delete one connection and the preview does not udpate for identifier

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T25_MultipleOutput_DeleteConnection.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }


        [Test]
        public void T004_TestSimpleMathNode()
        {
            //NOTE to JUN : This test case is failing, though the script generated is as expected. Can you please check what is the 
            // issue with the Mirror here ? 

            // 1. Create a driver node '0'
            // 2.  Create a driver node '1'
            // 3. Create a Math.Max node and connect output from driver nodes to it => verify the value is '1'           


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T004_TestSimpleMathNode.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
        }

        [Test]
        public void T005_TestReplicationGuideInCBNUsingAddition()
        {
            // 1. Create a CBN : a = {1,2};
            // 2. Create a CBN : b = {3,4};
            // 3. Create a CBN : c = a<1> + b<2>
            // 4. Connect output of last CBN to identifier node 'd' => vrify value is { {4,5}, { 5,6} }

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T005_TestReplicationGuideInCBNUsingAddition.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("d");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            // There are 2 elements in the top level array
            Assert.IsTrue(elements.Count == 2);

            // Get the array in the first index and verify its values
            List<Obj> dim1 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)dim1[0].Payload == 4);
            Assert.IsTrue((Int64)dim1[1].Payload == 5);


            // Get the array in the second index and verify its values
            List<Obj> dim2 = GetArrayElements(mirror, elements[1].DsasmValue);
            Assert.IsTrue((Int64)dim2[0].Payload == 5);
            Assert.IsTrue((Int64)dim2[1].Payload == 6);
        }

        [Test]
        public void T006_TestReplicationGuideInCBNWithUpdate()
        {
            // 1. Create a CBN : a = {1,2};
            // 2. Create a CBN : b = {3,4};
            // 3. Create a CBN : c = a<1> + b<2>
            // 4. Connect output of last CBN to identifier node 'd'
            // 5. Now update a = {2}; => vrify value is { { 5,6} }

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T006_TestReplicationGuideInCBNWithUpdate.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("d");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            // There is 1 element in the top level array
            Assert.IsTrue(elements.Count == 1);

            // Get the array in the first index and verify its values
            List<Obj> dim1 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)dim1[0].Payload == 5);
            Assert.IsTrue((Int64)dim1[1].Payload == 6);

        }

        [Test]
        public void T007_TestReplicationGuideFromRadialMenuUsingMathFunctionWithEdit()
        {
            // 1. Create a CBN : a = {1,2};
            // 2. Create a CBN : b = {-1,4};
            // 3. Create a Math.Max node
            // 4. Connect output of a and b to the 2 inputs of the Max node and connect it's output to an identifier 'c'
            // 5. Click on the top right hand corner of the Max node and then click once on the 'Replication Guide' item on the radial menu
            // 6. Now update the second replication guide to be '2'
            // 7. Verify the final output value : { {1,4}, {2,4} }

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T007_TestReplicationGuideFromRadialMenuUsingMathFunctionWithEdit.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("c");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            // There are 2 elements in the top level array
            Assert.IsTrue(elements.Count == 2);

            // Get the array in the first index and verify its values
            List<Obj> dim1 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)dim1[0].Payload == 1);
            Assert.IsTrue((Int64)dim1[1].Payload == 4);

            // Get the array in the second index and verify its values
            List<Obj> dim2 = GetArrayElements(mirror, elements[1].DsasmValue);
            Assert.IsTrue((Int64)dim2[0].Payload == 2);
            Assert.IsTrue((Int64)dim2[1].Payload == 4);

        }

        [Test]
        public void T008_TestReplicationGuideFromRadialMenuUsingMathFunctionWithEditAndUndo()
        {
            // 1. Create a CBN : a = {1,2};
            // 2. Create a CBN : b = {-1,4};
            // 3. Create a Math.Max node
            // 4. Connect output of a and b to the 2 inputs of the Max node
            // 5. Click on the top right hand corner of the Max node and then click once on the 'Replication Guide' item on the radial menu
            // 6. Now update the second replication guide to be '2'
            // 7. Now press undo , and the final output value : {1, 4}

            //Faling due to defect : IDE-1351 Replication guide preview : The replication guide preview is not showing the expected result when Math.Max(a<1>, b<2>) is called

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T008_TestReplicationGuideFromRadialMenuUsingMathFunctionWithEditAndUndo.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("c");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)elements[0].Payload == 1);
            Assert.IsTrue((Int64)elements[1].Payload == 4);


        }

        [Test]
        public void VerifyPreviewWithArray_1303()
        {
            // Deffect -http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1303
            // 1. create code block node with value 1 and another with 10
            // 2. create Range Node
            // 3. Connect 1 to start and 10 to End
            // 4. create identifier
            // 5. Connect output of Range node to input of Identifier node.
            // 6. Check the preview of Indetifier node.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "Defect1303.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)elements[0].Payload == 1);
            Assert.IsTrue((Int64)elements[1].Payload == 2);
            Assert.IsTrue((Int64)elements[2].Payload == 3);
            Assert.IsTrue((Int64)elements[3].Payload == 4);
            Assert.IsTrue((Int64)elements[4].Payload == 5);
            Assert.IsTrue((Int64)elements[5].Payload == 6);
            Assert.IsTrue((Int64)elements[6].Payload == 7);
            Assert.IsTrue((Int64)elements[7].Payload == 8);
            Assert.IsTrue((Int64)elements[8].Payload == 9);
            Assert.IsTrue((Int64)elements[9].Payload == 10);

        }

        [Test]
        public void T009_TestMathNodeAndDeleteDriverNodeConnection()
        {
            // 1. Create a driver node '0'
            // 2. Create a driver node '1'
            // 3. Create a Math.Min node and connect output from driver nodes to it 
            // 4. Delete the connection to the driver node '1' = > verify the output of math node is null


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T009_TestMathNodeAndDeleteDriverNodeConnection.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void T010_TestMathNodeAndDeleteDriverNode()
        {
            // 1. Create a driver node '0'
            // 2. Create a driver node '1'
            // 3. Create a Math.Divrem node and connect output from driver nodes to it 
            // 4. Delete the the driver node '1' = > verify the output of math node is null


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T010_TestMathNodeAndDeleteDriverNode.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void T011_TestMathNodeAndCreateNewConnection()
        {
            // 1. Create a driver node '0'
            // 2. Create a driver node '1'
            // 3. Create a Math.Max node and connect output from driver nodes to it 
            // 4. Now create another driver node, and connect from it's output to the Math node's two inputs
            // => verify the output of math node is 1.5


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T011_TestMathNodeAndCreateNewConnection.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata_d == 1.5);
        }

        [Test]
        public void T012_TestMathNodeAndModifyInputs()
        {
            // 1. Create a CBN '0'
            // 2. Create a driver node '1'
            // 3. Create a Math.Max node and connect output from driver nodes to it 
            // 4. Now modify the inputs to '-1' and '1.5' and verify the output of the math node = 1.5


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T012_TestMathNodeAndModifyInputs.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata_d == 1.5);
        }

        [Test]
        public void T013_TestMathNodeWithRangeNodeUsingReplicationAndEdit()
        {
            // 1. Create 2 CBNs,  '0' and '2'
            // 2. Create a driver node '1'
            // 3. Create a Range node with start = 0, end = 2, increment = 1;
            // 4. Create a Math.Factorial and pass thr output of range node to it
            // 5. Create an identifier node and connect output of Math.Factorial node to input of this driver node => var6
            // 6. Now create a '+' operator node and connect output of var6, and output of the CBN '2' to it's inputs => var7
            // 7. Now edit the input to the range node so that the start = -1 => verify var7 is updated to : {1,3,3,4}


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T013_TestMathNodeWithRangeNodeUsingReplicationAndEdit.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var7");
            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)elements[0].Payload == 1);
            Assert.IsTrue((Int64)elements[1].Payload == 3);
            Assert.IsTrue((Int64)elements[2].Payload == 3);
            Assert.IsTrue((Int64)elements[3].Payload == 4);
        }

        [Test]
        public void T014_TestMathNodeWithOperatorAndDriverAndIdentifierAndCBNWithEdit()
        {
            // 1. Create a driver node, rename to 'a' and set its value to 1'
            // 2. Create a CBN : b = a + 1;
            // 3. Create an identifier , rename to 'c', and connect output from CBN to it;s input 
            // 4. Create an operator node, and connect output from driver node 'a' and identifier node 'c' to it's inputs
            // 5. Create Math.Factorial node ( Var5 ) , and connect output from operator node to it's input
            // 6. Now edit value of 'a' = 2 => verify var5 is updated to : 120


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T014_TestMathNodeWithOperatorAndDriverAndIdentifierAndCBNWithEdit.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var5");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 120);

            mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 5);

            mirror = liveRunner.QueryNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 3);
        }

        [Test]
        public void T015_TestMathNodeWithOperatorAndDriverAndIdentifierAndCBNWithRemove()
        {
            // 1. Create a driver node, rename to 'a' and set its value to 1'
            // 2. Create a CBN : b = a + 1;
            // 3. Create an identifier , rename to 'c', and connect output from CBN to it;s input 
            // 4. Create an operator node, and connect output from driver node 'a' and identifier node 'c' to it's inputs
            // 5. Create Math.Factorial node ( Var5 ) , and connect output from operator node to it's input
            // 6. Now remove the driver node 'a' => verify var5 is updated to : null

            // defect :IDE-1294 deleting a node that was referenced by another does not update the preview

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T015_TestMathNodeWithOperatorAndDriverAndIdentifierAndCBNWithRemove.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var5");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

            mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

            mirror = liveRunner.QueryNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        public void T016_TestReplicationGuideFromRadialMenuUsingMathFunctionWithEditUndoRedo()
        {
            // 1. Create a range node {0,1,2};
            // 2. Create a driver node b = 2;
            // 3. createa CBN : c = b..b+3;
            // 4. Create a Math.Max node
            // 4. Connect output of range node and c to the 2 inputs of the Max node
            // 5. Click on the top right hand corner of the Max node and then click once on the 'Replication Guide' item on the radial menu
            // 6. Now update the second replication guide to be '2'
            // 7. Now press undo once and then press redo and the final output value : {{2,3,4,5}, {2,3,4,5}, {2,3,4,5}}

            //Faling due to defect : IDE-1388 Update issue with range expressions in CBN

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T016_TestReplicationGuideFromRadialMenuUsingMathFunctionWithEditUndoRedo.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var7");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            // There are 4 elements in the top level array
            Assert.IsTrue(elements.Count == 3);

            // Get the array in the first index and verify its values
            List<Obj> dim1 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)dim1[0].Payload == 2);
            Assert.IsTrue((Int64)dim1[1].Payload == 3);
            Assert.IsTrue((Int64)dim1[2].Payload == 4);
            Assert.IsTrue((Int64)dim1[3].Payload == 5);

            // Get the array in the lasy index and verify its values
            List<Obj> dim2 = GetArrayElements(mirror, elements[2].DsasmValue);
            Assert.IsTrue((Int64)dim2[0].Payload == 2);
            Assert.IsTrue((Int64)dim2[1].Payload == 3);
            Assert.IsTrue((Int64)dim2[2].Payload == 4);
            Assert.IsTrue((Int64)dim2[3].Payload == 5);


        }

        [Test]
        public void T017_TestReplicationGuideFromRadialMenuUsingMathFunctionWithInvalidGuidesAndUndoRedo()
        {
            // 1. Create a CBN : a = 1..2;
            // 2. Create a CBN : b = 3..4;
            // 3. Create a Math.Max node
            // 4. Connect output of 'a' and 'b' to the 2 inputs of the Max node
            // 5. Click on the top right hand corner of the Max node and then click once on the 'Replication Guide' item on the radial menu
            // 6. Now update the second replication guide to be '2'
            // 7. Then update it to '-1' => we get a warning that the guide is invalid, the the guide remains at '2'
            // 8. Then update the first replication guide to '2'. so that both guides ar now '2'
            // 9. Now press undo once and then redo => the final guides should both be '2', and the output : {3,4};

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T017_TestReplicationGuideFromRadialMenuUsingMathFunctionWithInvalidGuidesAndUndoRedo.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");

            List<Obj> dim1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)dim1[0].Payload == 3);
            Assert.IsTrue((Int64)dim1[1].Payload == 4);



        }

        [Test]
        public void T018_TestReplicationGuide_SingletonAndCollection_TestCaseDefect_IDE_1397()
        {
            // 1. Create a CBN : 0;
            // 2. Create a CBN : a = {0,1};
            // 3. Create a '+' node
            // 4. Connect output of CBN '0' and 'a' to the 2 inputs of the '+' node
            // 5. Click on the top right hand corner of the '+' node and then click once on the 'Replication Guide' item on the radial menu
            // 6. Replication guides <1> are added to both inputs
            // => verify the final output should still be {0,1}

            // Failing due to defect : IDE-1397 Replication guide : When replication guide is applied on a singleton and collection, the output is not as expected

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T018_TestReplicationGuide_SingletonAndCollection.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var1");

            List<Obj> dim1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)dim1[0].Payload == 0);
            Assert.IsTrue((Int64)dim1[1].Payload == 1);



        }
        [Test]
        public void T026_Unsuccessful_1402()
        {
            //1. create a code block node with value "@#"
            //2. create another code blcok node with value 1 and connect it to identifier 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "unsuccessfulnode.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");

            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T026_Unsuccessful_1402_2()
        {
            //1. create a code block node with value "@#"
            //2. create another code blcok node with value 1 and connect it to identifier 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "unsuccessfulnode2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");

            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T026_Unsuccessful_1402_3()
        {
            //1. create a code block node with value "@#"
            //3. create an identifier var 3 - it shoule be null
            //2. create another code blcok node with value 1 and connect it to identifier 
            

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "unsuccessfulnode3.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");

            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("Var3");

            Assert.IsTrue(mirror2.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        public void T019_TestRangeExpressionValueWithEdits()
        {
            // 1. Create a Range Expression : 0..2..1;
            // 2. Then edit end to '0'
            // 3. Then edit increment to '-1'
            // 4. Then edit start to '2'
            // 5. Then edit end to '0'

            // Verify the final value = {2, 1, 0 } 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T019_RangeExpressionValueWithEdits.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var1");

            List<Obj> dim1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)dim1[0].Payload == 2);
            Assert.IsTrue((Int64)dim1[1].Payload == 1);
            Assert.IsTrue((Int64)dim1[2].Payload == 0);
        }

        [Test]
        public void T020_TestIdentifierValueWithEdits()
        {
            // 1. Create a CBN : a = 1..2;
            // 2. Create a CBN : b = 3..7;
            // 3. Create a '+' node and connect 'a' and 'b' to it
            // 4. Then edit the CBN to finally have a = {1,2} and b = {2,1} and then add replication guides <1> and <2> respectively

            // Verify the final value = {{3,2},{4,3}} 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T020_IdentifierValueWithEdits.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            // Get the array in the first index and verify its values
            List<Obj> dim1 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)dim1[0].Payload == 3);
            Assert.IsTrue((Int64)dim1[1].Payload == 2);

            // Get the array in the lasy index and verify its values
            List<Obj> dim2 = GetArrayElements(mirror, elements[1].DsasmValue);
            Assert.IsTrue((Int64)dim2[0].Payload == 4);
            Assert.IsTrue((Int64)dim2[1].Payload == 3);
        }

        [Test]
        public void T021_TestFunctionNodeValueWithEdits_TestCaseDefect_DNL_1467667()
        {
            //Failing due to defect :DNL-1467667 Replication on heterogeneous array causes unintended type conversion
            
            // 1. Create a CBN : a = 1..2;
            // 2. Create a CBN : b = 3..7;
            // 3. Create a 'Math.Max(n1,n2)' node (Var3 ) and connect 'a' and 'b' to it
            // 4. Then edit the CBN to finally have a = {0, 3, 2} and b = {-1,4.5} 
            // 5. Now add another Math.Max : Var4(d1,d2) 
            // Verify the final value = {0,4.5} 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T021_TestFunctionNodeValueWithEdits.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");

            List<Obj> dim1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)dim1[0].Payload == 0);
            Assert.IsTrue((Int64)dim1[1].Payload == 5);

            mirror = liveRunner.QueryNodeValue("Var4");

            List<Obj> dim2 = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)dim2[0].Payload == 0);
            Assert.IsTrue((Int64)dim2[1].Payload == 4.5);
        }
        [Test]
        public void T027_ChainedAssignment()
        {
            // 1. Create a code block node 
            // 2. Create another code block with chanined assignment a=b=1 
            // 3. Edit first node to b 


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "chainedassignment.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");

            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);


        }

        [Test]
        public void T028_DriverNodeExpression()
        {

            //1. Create a driver node - Var 1 - 0;
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "DriverNodeExpression.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("c");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 0);

        }
        [Test]
        public void T029_DriverArray()
        {

            //1. Create a driver node - Var 1 - {0,1}
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "DriverNode_Array2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            List<Obj> var1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)var1[0].Payload == 0);
            Assert.IsTrue((Int64)var1[1].Payload == 1);


        }

        [Test]
        public void Island_FunctionNode_1282()
        {
            // Deffect -1282
            // 1. create + node 
            // 2. create a code block and identfier and connect them


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "unconnectedfunction.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);


            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
        }
        [Test]
        public void T030_MultilineCodeblock()
        {

            // 1. Create a codeblock with multiple lines a =10 and b=20
            //  2. Connect it to + operator and connect to it => verify its value  = 30
            // create anoher identifier and connect from first output of CBN to it => verify its value = 10
            
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "MultiLineCodeBlock.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 10);
            mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 30);
        }

        [Test]
        public void RangeNode_InvalidInput_1422()
        {
            // Deffect_1422: Incorrect preview value for Range Node when there is no input for End slot.
            // Create Code Block node with value 1 and another with value of 10.
            // Drag and drop Range node.
            // Connect Code Block node with value 1 to  start slot of Range node and with value 10 to Increment slot of Range node.
            // After above step you will get the preview value for Range node starting from 1..10. which is incorrect. Because for Range node, Start and End slots are mandatory and Increment slot is optional. 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "Defect_IDE_1422.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("Var3");

            Assert.IsTrue(mirror2.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        public void T030_TestValuesFromTwoOutputsOfCBN()
        {
            // IDE-1332
            // 1. Create a  CBN like this : "a = 1; b = 2;"
            // 2. Create 2 identifier nodes : Var2 and Var3
            // 3. Connect the output of the first output slot of the CBN to identifier 'Var2' : verify Var2 = 1;
            // 4. Connect the output of the second output slot of the CBN to identifier 'Var3' : verify Var3 = 2;
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T030_TestValuesFromTwoOutputsOfCBN.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 2);
        }

        [Test]
        public void T031_TestValuesFromTwoOutputsOfCBN_DoubleAndCollectionValues()
        {
            // IDE-1332
            // 1. Create a  CBN like this : "aa = 1.5; bb = 0..3;"
            // 2. Create 2 identifier nodes : a and b
            // 3. Connect the output of the first output slot of the CBN to identifier 'a' : verify a = 1.5;
            // 4. Connect the output of the second output slot of the CBN to identifier 'b' : verify b = {0,1,2,3};
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T031_TestValuesFromTwoOutputsOfCBN_DoubleAndCollectionValues.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata_d == 1.5);

            mirror = liveRunner.QueryNodeValue("b");
            List<Obj> var1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)var1[0].Payload == 0);
            Assert.IsTrue((Int64)var1[1].Payload == 1);
            Assert.IsTrue((Int64)var1[2].Payload == 2);
            Assert.IsTrue((Int64)var1[3].Payload == 3);
        }

        [Test]
        public void T032_TestValuesFrom4OutputsOfCBN_DoubleIntBoolCollectionValues()
        {
            // IDE-1332
            // 1. Create a  CBN : x = 0;
            // 2. Create another CBN like this : 
            // " a = 1 + 2;
            // b = { - 1, 2.5, true };
            // c = x;
            // d = { 0..1, 3..4 };"
            // 2. Create 4 identifier nodes and connect output from the 4 outputs nodes of the CBN to each of the identifiers and verify the outputs
            // Var3 = 3, Var4 = {-1,2.5,true}, Var5 = 0; Var6 = {{0,1},{3,4}}
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T032_TestValuesFrom4OutputsOfCBN_DoubleIntBoolCollectionValues.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 3);

            mirror = liveRunner.QueryNodeValue("Var4");
            List<Obj> var1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var1[0].Payload == -1);
            Assert.IsTrue((Double)var1[1].Payload == 2.5);
            Assert.IsTrue((Boolean)var1[2].Payload == true);

            mirror = liveRunner.QueryNodeValue("Var5");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 0);

            mirror = liveRunner.QueryNodeValue("Var6");
            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            List<Obj> dim1 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)dim1[0].Payload == 0);
            Assert.IsTrue((Int64)dim1[1].Payload == 1);

            List<Obj> dim2 = GetArrayElements(mirror, elements[1].DsasmValue);
            Assert.IsTrue((Int64)dim2[0].Payload == 3);
            Assert.IsTrue((Int64)dim2[1].Payload == 4);

        }
        [Test]
        public void T033_ImplicitConnection()
        {

            // 1. create a code block node a=1;
            // 2. create another code block node b=a
            // 3. connect the second cbn to identifier - verify value is 1
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "ImplicitConnection.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T034_RangeExpression_1444()
        {

            //1. Create three code block nodes a =1, b=1 and c=10
            //2. Create range block node 
            //3. Connect a to start , b to Increment and c to End 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "RangeExpression_1444.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            List<Obj> var1 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            //Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);



            Assert.IsTrue((Int64)var1[0].Payload == 1);
            Assert.IsTrue((Int64)var1[1].Payload == 2);
            Assert.IsTrue((Int64)var1[2].Payload == 3);
            Assert.IsTrue((Int64)var1[3].Payload == 4);
            Assert.IsTrue((Int64)var1[4].Payload == 5);
            Assert.IsTrue((Int64)var1[5].Payload == 6);
            Assert.IsTrue((Int64)var1[6].Payload == 7);
            Assert.IsTrue((Int64)var1[7].Payload == 8);
            Assert.IsTrue((Int64)var1[8].Payload == 9);
            Assert.IsTrue((Int64)var1[9].Payload == 10);

        }

        [Test]
        public void T035_IDE_1322()
        {

            //1. Create CBN : 34
            // 2. Create CBN : a = 33; b = 4;
            // 3. Create '+' nodea nd connect teh 2 CBN to it.
            //=> verify output of '+' node: 38

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T035_IDE_1322.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 38);

        }

        [Test]
        public void T035_IDE_1322_2()
        {

            // 1. Create CBN : a = 1; 1+1;
            // 2. Create CBN : 1;
            // 3. Create '+' node and connect the 2nd output of 1st CBN and the second CBN to this '+' node
            //=> verify output of '+' node: 3

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T035_IDE_1322_2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 3);

        }

        [Test]
        public void T035_IDE_1322_3()
        {

            // 1. Create CBN : a = 1..5; 1..2..#2
            // 2. Create CBN : 1..3
            // 3. Create '+' node and connect the 2nd output of 1st CBN and the second CBN to this '+' node
            // 4. Now update the second output of the CBN to : 0..2..#3
            // => verify updated output of '+' node: {1, 3, 5}

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T035_IDE_1322_3.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            List<Obj> var3 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var3[0].Payload == 1);
            Assert.IsTrue((Int64)var3[1].Payload == 3);
            Assert.IsTrue((Int64)var3[2].Payload == 5);

        }

        [Test]
        public void T036_IDE_1314()
        {

            //1. Create CBN : a = 1; b= 2;
            //2. Create '+' node and connect 'a' and 'b' to it's 2 inputs.
            //=> verify output of '+' node: 3

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T036_IDE_1314.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 3);
        }

        [Test]
        public void T037_IDE_1585()
        {

            // 1. Create CBN : a=1;
            // 2. Create CBN : a;
            // 3. Undo
            // 4. Redo
            //=> verify preview for CBN: a.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "Defect_IDE_1566.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }

        [Test]
        public void T038_IDE_1596()
        {

            // 1. Create CBN : a=1;
            // 2. Create CBN : a =2;
            // 3. Update second CBN to : b=2;
            //=> verify preview for first CBN: a=1.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T038_IDE_1596.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }

        [Test]
        public void T039_IDE_1230()
        {

            // 1. Create CBN : a={1,2,3};a[0] =4; a[1]=5;
            // 2. Create 2 Identifier nodes and connect output of second and third line of CBN to input of each Identifier.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T039_IDE_1230.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 4);

            mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 5);
        }

        [Test]
        public void T040_IDE_1325()
        {

            // 1. Create CBN : 4+4; 1..20..2; {1,3,5..}
            //2. Create 3 Identifier nodes and connect 3 outputs of CBN to each Identifier.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T040_IDE_1325.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 8);

            mirror = liveRunner.QueryNodeValue("Var3");
            List<Obj> var3 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var3[0].Payload == 1);
            Assert.IsTrue((Int64)var3[9].Payload == 19);

            mirror = liveRunner.QueryNodeValue("Var4");
            List<Obj> var4 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var4[0].Payload == 1);
            Assert.IsTrue((Int64)var4[5].Payload == -1);
        }

        [Test]
        public void T040_IDE_1325_2()
        {

            // 1. Create CBN : a = 1
            // 2. Create a nother CBN :  a
            // 3. Create another CBN : a + 1           

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T040_IDE_1325_2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 2);

            mirror = liveRunner.QueryNodeValue("Var5");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);


        }

        [Test]
        public void T041_IDE_1450()
        {

            // Create CBN : 1..5
            // Create CBN : 1..5..#3
            // Create CBN : 1/2
            // Create CBN : 0.56
            // Create CBN : -1
            // connect these CBN s to 5 variables and verify the values

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T041_IDE_1450.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var6");
            Assert.IsTrue((Double)mirror.GetData().GetStackValue().opdata == 1 / 2);

            mirror = liveRunner.QueryNodeValue("Var7");
            Assert.IsTrue((Double)mirror.GetData().GetStackValue().opdata_d == 0.56);

            mirror = liveRunner.QueryNodeValue("Var10");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == -1);

            mirror = liveRunner.QueryNodeValue("Var8");
            List<Obj> var8 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var8[0].Payload == 1);
            Assert.IsTrue((Int64)var8[4].Payload == 5);

            mirror = liveRunner.QueryNodeValue("Var9");
            List<Obj> var9 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var9[0].Payload == 1);
            Assert.IsTrue((Int64)var9[2].Payload == 5);

        }

        [Test]
        public void Array_update_item_1602()
        {

            // Create CBN a : 1..3
            // Create CBN b : {1,2,3}
            // modify b[0]=a

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "Array_update-item_1602.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            List<Obj> level2 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)elements[1].Payload == 2);
            Assert.IsTrue((Int64)elements[2].Payload == 3);

            Assert.IsTrue((Int64)level2[0].Payload == 1);
            Assert.IsTrue((Int64)level2[1].Payload == 2);
            Assert.IsTrue((Int64)level2[2].Payload == 3);

        }

        [Test]
        public void replicationguides_1595()
        {

            // Create CBN  {1,2}
            // Create CBN  {10,20}
            // add using + node 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "rep.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            List<Obj> level21 = GetArrayElements(mirror, elements[0].DsasmValue);
            Assert.IsTrue((Int64)level21[0].Payload == 11);
            Assert.IsTrue((Int64)level21[1].Payload == 21);
            List<Obj> level22 = GetArrayElements(mirror, elements[1].DsasmValue);
            Assert.IsTrue((Int64)level22[0].Payload == 12);
            Assert.IsTrue((Int64)level22[1].Payload == 22);

        }

        [Test]
        public void MultiLineCodeBlock_1285_2()
        {

            // 1. Create multiline code block with a=10;b=20;
            // 2. Create an identifier
            // 3. Connect the first one to the identifier change the connection to the second the preview doesn not work

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "MultiLineCodeBlock_2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 20);
        }

        [Test]
        public void T042_IDE_1447()
        {

            // Create CBN : a = 1..2; b = a+1;
            // Create CBN : 1
            // Create + : a + 1
            // Create + : b + 1;
            // Create + : a + b
            // verify the values

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T042_IDE_1447.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            List<Obj> var3 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var3[0].Payload == 2);
            Assert.IsTrue((Int64)var3[1].Payload == 3);

            mirror = liveRunner.QueryNodeValue("Var4");
            List<Obj> var4 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var4[0].Payload == 3);
            Assert.IsTrue((Int64)var4[1].Payload == 4);


            mirror = liveRunner.QueryNodeValue("Var5");
            List<Obj> var5 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var5[0].Payload == 3);
            Assert.IsTrue((Int64)var5[1].Payload == 5);

        }

        [Test]
        public void T043_IDE_1616()
        {

            // create a CBN : '0' 
            // Create a Point.ByCoordinates and connect the '0' to it's 3 inputs
            // Click on the 'X' property of the Point and a new identifier Var3 is created with preview '0' 
            // Now create another identifier Var4 and connect Var3 to it. It's preview should be '0' now.
            // Now create a CBN : a = Var3; => It's preview should also be 0
            // Now update the the first CBN to 10. Verify Var3, Var4 and a are all '10' now

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T043_IDE_1616.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 10);

            mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 10);

            mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 10);

        }

        [Test]
        public void T044_PreviewAfterUndoRedo()
        {

            // Create CBN a=1 -> Undo ->Redo

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "PreviewAfterUndoRedo.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T045_preview_MultilineCBN_TestCaseDefect_1678()
        {
            //Failing defect : 1678

            // 1. Create a mutliline CBN with value 1;2;a+b 
            // 2. create a Node a=1 
            // 3. Create another node b=13. the preview for CBN 'a' is not displayed
           

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "preview_MultilineCBN.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }

        [Test]
        public void T046_IDE_1676()
        {

            // 1. Create a Math.Abs node with input '0'
            // 2. Create another Math.Abs node with input '1.5'
            // 3. Now delete the first Math.Abs and verify the preview for the second is still 1.5


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T046_IDE_1676.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1.5);

        }

        [Test]
        public void T047_IDE_1603_TestCaseDefect_1856()
        {
            // Failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            // 1. Create 2 Point nodes and a line from it
            // 2. Verify the Length property from the radial menu : 10
            // 3. Createa  surface from it using ExtrudeAsSurface(line, 10, Vector.ByCoordinates(0,0,1)) : verify area = 100           


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T047_IDE_1603.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var6");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 10.0);
            mirror = liveRunner.QueryNodeValue("Var11");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 100.0);
        }

        [Test]
        public void T048_IDE_1695_TestCaseDefect_1856()
        {
            // Failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            
            // Create two Point nodes.
            // Connect two point nodes to CBN with values like (0,0,0) and another point ode should have (10,0,0)
            // Create ByStartPointEndPoint node from Line class and connect above two points to it.
            // Access PointAtParameter from Line node.
            // create CBn with value 0.5 and connect it to input of ParameterAtPoint node.
            // Access property X from ParameterAtPoint node.
            // Verify that the preview for X node should be 5.0

            GraphToDSCompiler.GraphUtilities.PreloadAssembly("ProtoGeometry.dll");
            GraphToDSCompiler.GraphUtilities.PreloadAssembly("Math.dll");

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T048_IDE_1695.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var8");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 5.0);
        }


        [Ignore]
        public void T051_CoordinateSystemPropertytest()
        {

            // Create CBN for each line below.
            // scale               = { 2, 3, 4 };
            // rotation            = { 180, 0, 0 };
            // rsequence           = { 3, 2, 1 };
            // translationVector   = { 5, 5, 0 };
            // true;
            // Now drag and drop ByUniversalTransform (first node) from CoordianteSystem and coonec all above respective nodes to CoordianteSystem node.
            // Now access ScalFactors property from CoordinateSystem node and verify its preview. ( it should return {2,3,4})


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T049_CoordinateSystemPropertytest.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var14");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            List<Obj> level21 = GetArrayElements(mirror, elements[0].DsasmValue);

            Assert.IsTrue((Int64)level21[0].Payload == 2);
            Assert.IsTrue((Int64)level21[1].Payload == 3);
            Assert.IsTrue((Int64)level21[2].Payload == 4);

        }
        [Test]
        public void T049_IDE_1721()
        {

            // 1. Creata a CBN 1 
            // 2. Assign it to another CBN. preview is expected here 
            // 3. Assign to another identifier and vrify it's value = 1
            // 4. create another CBN : c = 2; b = 1; and verify value of 'b' = 1


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "preview_CBN_variable.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);
            mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }

        [Test]
        public void T050_IDE_1614()
        {

            // Create CBN 
            // a = 1..10..2;
            // b = a * 2;
            // b[1] = b[1] + 3;
            // Connect first two line to Identifier nodes.
            // Now add few more identifier nodes to Canvas and verify that value for "b" shouldn't change.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T050_IDE_1614.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            List<Obj> var3 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var3[0].Payload == 2);
            Assert.IsTrue((Int64)var3[1].Payload == 7);
            Assert.IsTrue((Int64)var3[2].Payload == 6);
            Assert.IsTrue((Int64)var3[3].Payload == 8);
        }

        [Test]
        public void T051_IDE_1445()
        {

            // 1. Create a  CBN : a = 1;
            // 2. Update the CBN : b = a + 1;


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T051_IDE_1445.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 2);
        }

        [Test]
        public void T052_IDE_1754_TestCaseDefect_1856()
        {

            // Failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            
            // Create Line of length 10 and then Reverse it. After reversing access its Start Point and on Start Point access its property X.
            // Verify that the value preview for X should be 10. Right now it is coming 0.


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T052_IDE_1754.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var8");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 10.0);
        }

        [Test]
        public void T053_IDE_1737()
        {

            // Drag and drop Range and Identifier node.
            // Connect output of Range node to input of Identifier node.
            // Now create CBn with value 0;10;.
            // Now connect 0 to start and 10 to end slot of Range node.
            // After above step preview for Range node is coming but there is no preview for Identifier node. (If we create another Identifier node and then connecting it to Range node is displaying correct preview.) So this problem only happens if Identifier node connected before connecting input of Range node.


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T053_IDE_1737.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            List<Obj> var2 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var2[0].Payload == 0);
            Assert.IsTrue((Int64)var2[1].Payload == 1);
            Assert.IsTrue((Int64)var2[2].Payload == 2);
            Assert.IsTrue((Int64)var2[3].Payload == 3);
            Assert.IsTrue((Int64)var2[4].Payload == 4);

        }

        [Test]
        public void T054_IDE_1770()
        {

            // create a CBN : count = 10
            // create another cbn : 0..10..#count
            // create two more cbns : 0 and 0 
            // create another cbn : 10..0..#count;
            // => verify the values of the first and last cbns 


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T054_IDE_1770.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var7");
            List<Obj> var7 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var7.Count == 10);
            Assert.IsTrue((Double)var7[0].Payload == 0);
            Assert.IsTrue((Double)var7[1].Payload == 1.1111111111111112);
            Assert.IsTrue((Double)var7[9].Payload == 10);

            mirror = liveRunner.QueryNodeValue("Var8");
            List<Obj> var8 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var8.Count == 10);
            Assert.IsTrue((Double)var8[0].Payload == 10);
            Assert.IsTrue((Double)var8[1].Payload == 8.8888888888888875);
            Assert.IsTrue((Double)var8[9].Payload == 0);

        }

        [Test]
        public void T055_IDE_1769()
        {

            // create a CBN : count = 10
            // create another cbn : 1..count; 0; 0; 
            // assing the 3 CBN outputs to 3 different identifiers and verify the values


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T055_IDE_1769.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            List<Obj> var3 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)var3.Count == 10);
            Assert.IsTrue((Int64)var3[0].Payload == 1);
            Assert.IsTrue((Int64)var3[9].Payload == 10);

            mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata == 0);

            mirror = liveRunner.QueryNodeValue("Var5");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata == 0);
        }

        [Test]
        public void T056_IDE_1590()
        {

            // Create CBN:2
            // Create CBN: a
            // Connect above tow CBN.
            // Create CBN: 3
            // Create + node.
            // Connect a and 2 to + node.
            // Delete CBN :a
            // Connect first CBn to + node.
            // Verify that + node should display the preview with value 5.


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T056_IDE_1590.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 5);
        }

        [Test]
        public void T057_IDE_1723()
        {

            // Create CBN:0
            // Create CBN: 3
            // Create ByCoordiante node.
            // Connect first CBN to all input of Point Node.
            // Access X property from point node.
            // Connect CBN: 3 to first input of Point node.
            // Verify that preview for property node X should display value 3.


            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T057_IDE_1723.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 3);
        }

        [Test]
        public void T058_IDE_1731()
        {

            // Create CBN:a+b
            // Create Identifier node.
            // Create CBN: a= 2*5;
            // Create CBN : b=a*2;
            // Undate B = a*2 to b=a+2;
            // Update a = 2*5 to a=5*5;
            // Verify that preview for Identifier node should display value 52.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T058_IDE_1731.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 52);
        }

        [Test]
        public void T059_IDE_1608()
        {

            // Create CBN:
            //
            // Verify that preview for Identifier node should display value 52.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T059_IDE_1608.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            List<Obj> var3 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            //Assert.IsTrue((Int64)var3.Count == 10);
            Assert.IsTrue((Int64)var3[0].Payload == 1);
            Assert.IsTrue((Int64)var3[1].Payload == 2);
            Assert.IsTrue((Int64)var3[2].Payload == 100);

            mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata == 100);
        }

        [Test]
        public void T060_IDE_1630_TestCaseDefect_1856()
        {
            // Test case failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1630

            GraphToDSCompiler.GraphUtilities.PreloadAssembly("ProtoGeometry.dll");
            GraphToDSCompiler.GraphUtilities.PreloadAssembly("Math.dll");

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T060_IDE_1630.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var8");
            List<Obj> var8 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            //Assert.IsTrue((Int64)var3.Count == 10);
            Assert.IsTrue((Int64)var8[0].Payload == 10);
            Assert.IsTrue((Int64)var8[1].Payload == 10);
            Assert.IsTrue((Int64)var8[2].Payload == 10);
            Assert.IsTrue((Int64)var8[3].Payload == 10);
        }

        [Test]
        public void T061_IDE_1485()
        {

            // create a Point.ByCoordinates (0,0,0)
            // click on the translate property ( it's base geometry class property ) and translate by 5
            // click on the 'x' property of the translated point => value should be 5
            // delete the initial point and then undo => the value should still be 5

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T061_IDE_1485.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var6");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 5);
        }

        [Test]
        public void T062_IDE_1632_TestCaseDefect_1856()
        {

            // Test case failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            
            // Steps are mentioned in the defect. It is a long list of steps to it is better to refere defect for steps.

            GraphToDSCompiler.GraphUtilities.PreloadAssembly("ProtoGeometry.dll");
            GraphToDSCompiler.GraphUtilities.PreloadAssembly("Math.dll");

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T062_IDE_1632.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var8");
            List<Obj> var8 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            //Assert.IsTrue((Int64)var3.Count == 10);
            Assert.IsTrue((Int64)var8[0].Payload == 10);
            Assert.IsTrue((Int64)var8[1].Payload == 10);
            Assert.IsTrue((Int64)var8[2].Payload == 10);
            Assert.IsTrue((Int64)var8[3].Payload == 10);

            //mirror = liveRunner.QueryNodeValue("Var4");
            //Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata == 100);
        }

        [Test]
        public void T063_IDE_1823()
        {

            // Steps are mentioned in the defect.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T063_IDE_1823.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 10);
        }

        [Test]
        public void T064_IDE_1806()
        {

            /*
 1. Create a Point.ByCoordinates node
 2. Click on the 'X' property from it's radial menu
 3. create a CBN : '0' and connect it to all 3 inputs of the Point.ByCoordinates node 
 => no geometry/textual preview from neither the geometry node, nor the property node
             */

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T064_IDE_1806.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 0);
        }

        [Test]
        public void T065_IDE_1586()
        {

            // Steps are mentioned in the defect.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T065_IDE_1586.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            List<Obj> b = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)b[0].Payload == 1);
            Assert.IsTrue((Int64)b[1].Payload == 2);

        }

        [Test]
        public void T066_IDE_1830()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1830

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T066_IDE_1830.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");

            Assert.IsTrue(Math.Abs((double)mirror.GetData().GetStackValue().opdata_d - 0.5) < tolerance);
        }

        [Test]
        public void T067_IDE_1800()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1800

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T067_IDE_1800.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var5");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);
        }

        [Test]
        public void T068_IDE_1800_1()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1800

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T068_IDE_1800_1.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var6");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 10);
        }

        [Test]
        public void T069_IDE_1855_TestCaseDefect_1856()
        {
            // Failing due to defect  : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1855

            GraphToDSCompiler.GraphUtilities.PreloadAssembly("ProtoGeometry.dll");
            GraphToDSCompiler.GraphUtilities.PreloadAssembly("Math.dll");

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T069_IDE_1855.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var5");
            List<Obj> b = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue(Convert.ToInt64(b[0].Payload) == 1);
            Assert.IsTrue(Convert.ToInt64(b[4].Payload) == 5);

        }

        [Test]
        public void T070_IDE_1792()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1792

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T070_IDE_1792.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);
        }

        [Test]
        public void T071_IDE_1794_TestCaseDefect_1856()
        {

            // Failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1794

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T071_IDE_1794.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 180);
        }

        [Test]
        public void TestTypeOfPreview()
        {

            // Create Point.ByCoordiantes(0,0,0);
            // Verify that the return type and the preivew is Point. 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "TestTypeOfPreview.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            string type = mirror.GetDynamicType();
            Assert.IsTrue(type == "Point");
        }

        [Test]
        public void T072_IDE_1846()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1846

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T072_IDE_1846.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("x");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 0);
        }

        [Test]
        public void T073_IDE_1545()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1545

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T073_IDE_1545.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Double)mirror.GetData().GetStackValue().opdata == 1);
            mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);
        }

        [Test]
        public void T073_IDE_1545_2()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1545

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T073_IDE_1545_2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Double)mirror.GetData().GetStackValue().opdata == 1);
            mirror = liveRunner.QueryNodeValue("Var1");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);
        }

        [Test]
        public void T074_IDE_1606_TestCaseDefect_IDE_1984()
        {
            // Failing due to Defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1984

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1606

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T074_IDE_1606.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var2");
            List<Obj> Var2 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)Var2[0].Payload == 5);
            Assert.IsTrue((Int64)Var2[1].Payload == 2);
            Assert.IsTrue((Int64)Var2[2].Payload == 3);
        }

        [Test]
        public void T075_IDE_1264()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1264

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T075_IDE_1264.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 10);

        }

        [Test]
        public void T076_IDE_1867()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1867

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T076_IDE_1867.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }

        [Test]
        public void T076_IDE_1867_2()
        {

            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1867

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T076_IDE_1867_2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 0.5);
            mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 0.5);

        }

        [Test]
        public void T077_IDE_1793_TestCaseDefect_1856()
        {

            // Failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            
            // Create Point.ByCoordiantes(0,0,0);
            // Verify that the return type and the preivew is Point. 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T077_IDE_1793.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("points");
            string type = mirror.GetDynamicType();
            Assert.IsTrue(type == "Point");
        }

        [Test]
        public void T078_IDE_1530()
        {

            // Create Point.ByCoordiantes(0,0,0);
            // Verify that the return type and the preivew is Point. 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T078_IDE_1530.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            List<Obj> b = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)b[0].Payload == 1);
            Assert.IsTrue((Int64)b[9].Payload == 10);

            mirror = liveRunner.QueryNodeValue("a");
            List<Obj> a = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)a[0].Payload == 1);
            Assert.IsTrue((Int64)a[9].Payload == 10);
        }

        [Test]
        public void T078_IDE_1530_2()
        {

            // Create Point.ByCoordiantes(0,0,0);
            // Verify that the return type and the preivew is Point. 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T078_IDE_1530_2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            List<Obj> b = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)b[0].Payload == 1);
            Assert.IsTrue((Int64)b[99].Payload == 100);

            mirror = liveRunner.QueryNodeValue("a");
            List<Obj> a = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)a[0].Payload == 1);
            Assert.IsTrue((Int64)a[99].Payload == 100);
        }

        [Test]
        public void T079_IDE_1822()
        {

            // Steps are mentioned in the defect.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T079_IDE_1822.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 10);
        }

        [Test]
        public void T080_IDE_1840_TestCaseDefect_IDE_1856()
        {

            // Test Case is failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            
            // Create a Point.ByCoordinate(0,0,0)
            // Create a CBN : 10
            // Create a Circle.ByCentrePointRadius using the point and CBN '10' above
            // Create a CBN : a = 0..1..#4
            // Create a CBN : b = a[0..2]
            // Click on the Circle.CoordinateSystemAtParameter(b)
            // Now create CoordinateSystem.Origin.X  and verify the number of elements in it : 3

            GraphToDSCompiler.GraphUtilities.PreloadAssembly("ProtoGeometry.dll");
            GraphToDSCompiler.GraphUtilities.PreloadAssembly("Math.dll");

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T080_IDE_1840.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var9");
            List<Obj> Var8 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)Var8.Count == 3);
            // verify the individual values in another test case, as that has some issues currently

        }

        [Test]
        public void T081_IDE_1583()
        {

            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1583

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T081_IDE_1583.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        public void T082_IDE_1553_Defect_Open()
        {

            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1553

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T082_IDE_1553.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var4");
            List<Obj> Var4 = GetArrayElements(mirror, mirror.GetData().GetStackValue());
            Assert.IsTrue((Int64)Var4.Count == 50);
            //Assert.IsTrue((double)Var4[0].Payload == 0.866025);   

        }

        [Test]
        public void T083_TestPreviewInSimpleScenario()
        {

            // Create CBN: 2;
            // Creaet CBN: a;
            // Create CBN: b;
            // Connect all above three CBN. first to second second to third.
            // a and b should display the preivew with value 2.

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T083_TestPreviewInSimpleScenario.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 2);

            mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 2);
        }

        [Test]
        public void T084_IDE_1910()
        {

            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1910

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T084_IDE_1910.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 7);

        }

        [Test]
        public void T085_IDE_1762()
        {

            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1762

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T085_IDE_1762.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("c");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 10);

        }

        [Test]
        public void T086_IDE_1849_Defect_Open()
        {
            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1849

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T086_IDE_1849.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 20);

        }

        [Test]
        public void T087_IDE_1870()
        {

            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1870

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T087_IDE_1870.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

        }
        /*  [Test]
          public void T077_stringconcat()
          {

              ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

              List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "stringconcat.xml");
              foreach (SynchronizeData sd in ls)
                  liveRunner.UpdateGraph(sd);

              ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
              Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == "testtest2func");

          }*/

        [Test]
        public void T078_1899()
        {

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1899.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T079_1950_Defect_Open()
        {

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "preview_1950.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T079_1276_Defect_Open()
        {

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1276.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
            //Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);

        }
        [Test]
        public void T088_1435()
        {

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "preview_1435.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((Int64)mirror2.GetData().GetStackValue().opdata == 1);

        }


        [Test]
        public void TestCreateReplicationGuideTextNormalData()
        {
            List<int> data1 = new List<int>();
            data1.Add(1);
            data1.Add(1);
            data1.Add(2);
            List<int> data2 = new List<int>();
            data2.Add(1);
            data2.Add(2);
            data2.Add(0);
            data2.Add(0);

            string result1 = SnapshotNode.CreateReplicationGuideText(data1);
            string result2 = SnapshotNode.CreateReplicationGuideText(data2);
            
            Assert.AreEqual("1,1,2", result1);
            Assert.AreEqual("1,2", result2);
        }

        [Test]
        public void TestCreateReplicationGuideExtremeData()
        {
            List<int> data1 = new List<int>();
            data1.Add(100);
            data1.Add(200);
            data1.Add(300);
            List<int> data2 = new List<int>();
            data2.Add(-100);
            data2.Add(-200);
            data2.Add(0);
            data2.Add(0);

            string result1 = SnapshotNode.CreateReplicationGuideText(data1);
            string result2 = SnapshotNode.CreateReplicationGuideText(data2);
            
            Assert.AreEqual("100,200,300", result1);
            Assert.AreEqual("-100,-200", result2);
        }

        [Test]
        public void TestParseReplicationGuideTextNormalData()
        {
            string data1 = "1,2,3,4";
            string data2 = "10,3,1";
            string data3 = "2";

            string result1 = SnapshotNode.ParseReplicationGuideText(data1);
            string result2 = SnapshotNode.ParseReplicationGuideText(data2);
            string result3 = SnapshotNode.ParseReplicationGuideText(data3);

            Assert.AreEqual("<1><2><3><4>", result1);
            Assert.AreEqual("<10><3><1>", result2);
            Assert.AreEqual("<2>", result3);
        }


        [Test]
        public void T089_IDE_1934_TestCaseDefect_IDE_1856()
        {
            // Failing due to defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1856
            
            // steps are mentioned in the defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1934

            GraphToDSCompiler.GraphUtilities.PreloadAssembly("ProtoGeometry.dll");
            GraphToDSCompiler.GraphUtilities.PreloadAssembly("Math.dll");

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T089_IDE_1934.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

            mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 2);

            mirror = liveRunner.QueryNodeValue("x");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 2);
        }

        [Ignore]
        public void T090_IDE_2001()
        {

            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2001

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T090_IDE_2001.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 7);

        }

        [Test]
        public void T091_IDE_2035()
        {

            // steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2035

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T091_IDE_2035.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

        }
        [Test]
        public void T092_IDE_1979()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1979



            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1979.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)elements[0].Payload == 3);

        }
        [Test]
        public void T093_IDE_1435()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1435



            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1435.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");

            
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

        }
        [Test]
        public void T094_IDE_1784()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1784

            // the case in deffect is negative test 
            // so to verify - 1.Create CBN ‘a’
            //2.Create CBN {1,2}
            //3.Connect them
            //4. Change ‘a’ to ‘{a,1}’
            //5. Change ‘{1,2}’ to ‘{1}’
            // create CBN a 
            // change {1,2} to a={1,2}

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1784.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");

            List<Obj> elements = GetArrayElements(mirror, mirror.GetData().GetStackValue());

            Assert.IsTrue((Int64)elements[0].Payload == 1);
            Assert.IsTrue((Int64)elements[1].Payload == 2);

        }

        [Test]
        public void T095_IDE_2096()
        {

            // CBN: a=1;b=2;c=3;
            // CBN: d=2;e=a+b+c;
            // Varify e should have the preivew value 6

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T095_IDE_2096.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("e");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 6);

        }

        [Test]
        public void T096_IDE_1852()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1852

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T096_IDE_1852.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("d");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

            mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

            mirror = liveRunner.QueryNodeValue("a");
            Assert.IsFalse((double)mirror.GetData().GetStackValue().opdata_d == 1);

            mirror = liveRunner.QueryNodeValue("c");
            Assert.IsFalse((double)mirror.GetData().GetStackValue().opdata_d == 1);
        }
        [Test]
        public void T095_IDE_1950()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1950
            //Create with CBN 1 ->CBN a->CBN B=a ,connect 1 to a  here the preview for a is not created 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1950.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror2.GetData().GetStackValue().opdata_d == 1);


        }
        [Test]
        public void T095_IDE_1990()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1990
            //Create a CBN a Create a CBN 1 connect the above delete the connection the preview for a still shown as 1 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "defect_1990.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);



        }
        [Test]
        public void T095_IDE_1854_1()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1854
            //1. create CBN[a = 1] 2. delete it3. create CBN[a = 1]4. verify a is 1 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1854_1.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);



        }
        [Test]
        public void T095_IDE_1854_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1854
            //1. create CBN[a = 1] 2. delete it3. create CBN[a = 10]4. verify a is 10

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1854_2.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 10);



        }

        [Test]
        public void T097_IDE_2065()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2065

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T097_IDE_2065.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);
            
            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("h");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 3);
        }

        [Test]
        public void T098_IDE_2134()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2134

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T098_IDE_2134.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 2);
        }

        [Test]
        public void T100_IDE_1733()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1733

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T100_IDE_1733.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 10);
        }

        [Test]
        public void T101_IDE_1969()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1969

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T101_IDE_1969.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            // Verification pending, just adding to catch regression while running snap shot data.

        }

        [Test]
        public void T102_IDE_2001()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2001

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T102_IDE_2001.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            // Verification pending, just adding to catch regression while running snap shot data.
        }

        [Test]
        public void T103_IDE_1984()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1984

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T103_IDE_1984.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 3);

            mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

            mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 3);
        }

        [Test]
        public void T104_IDE_2105()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2105

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T104_IDE_2105.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("d");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

        }

        [Ignore] // because of failing for valid scenario. Need to investigate more after release.
        public void T105_IDE_2043()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2043

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T105_IDE_2043.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            // Verification pending, just adding to catch regression while running snap shot data.

        }

        [Test]
        public void T106_IDE_2092()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2092

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T106_IDE_2092.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var6");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 440);

        }

        [Test]
        public void T107_IDE_2010()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2010

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T107_IDE_2010.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var6");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

            mirror = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

        }

        [Test]
        public void T108_IDE_2020()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2020

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T108_IDE_2020.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 10);

            mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 1);

        }

        [Test]
        public void T109_IDE_1931()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1931

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T109_IDE_1931.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("q");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 0);

            // Cannot count how many lines are there in the CBN which was the main focus of defect but adding this test case to catch regression in N2C.
        }

        [Test]
        public void T110_IDE_1786()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1786

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T110_IDE_1786.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var6");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 0);
        }

        [Test]
        public void T111_IDE_1847()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1847

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T111_IDE_1847.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var5");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 5);
        }

        [Test]
        public void T112_IDE_2125()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2125

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T112_IDE_2125.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void T113_IDE_2181()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2181
            /*
             * 1. Start DSS and create a CBN : 
                    a1 = Point.ByCoordinates(0, 0, 0);
                    a2 = Point.ByCoordinates(1, 1, 1);
                    l1 = Line.ByCoordinates(a1, a2);

               2. Now create an identifier node assign the last line to it -> there is an error saying there is not match for Line.ByCoordinates.

                3. Then I go and update the CBN to  : 
                a1 = Point.ByCoordinates(0, 0, 0);
                a2 = Point.ByCoordinates(1, 1, 1);
                l1 = Line.ByStartPointEndPoint(a1, a2);
             
             3. Create a node from the Line.Length and verify it's value
             **/
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T113_IDE_2181.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(Math.Round(mirror.GetData().GetStackValue().opdata_d, 3) == 1.732);
        }

        [Test]
        public void T114_IDE_1551()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1551
            // Create various CBNs :
            // 1. CBN : // This is a comment
            // 2. CBN : // This is a comment 1
            //          // This is comment 2
            //          a = 1;
            // 3. CBN : /* This is commennt 1
            //           * this is a comment 2
            //          */
            //          b  = a;
            // 4. CBN : // this is comment
            //          c = b;
            //          // this is comment
            //          d = c + 1;
            //          // this is comment
            // 5. Verify d = 2
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T114_IDE_1551.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("d");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 2);

        }

        [Test]
        public void T115_IDE_2075()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2075

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T115_IDE_2075.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);

            mirror = liveRunner.QueryNodeValue("b");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 2);
        }

        [Test]
        public void T116_IDE_2137()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2137

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T116_IDE_2137.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("test");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 7);
        }

        [Test]
        public void T113_IDE_TrialNTC()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2125

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T113_IDE_TrialNTC.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue((double)mirror.GetData().GetStackValue().opdata_d == 3);
        }
        [Test]
        public void T117_IDE_2187()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2187

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T117_IDE_2187.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var1");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror2.GetData().GetStackValue().opdata == 2);
            ProtoCore.Mirror.RuntimeMirror mirror3 = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror3.GetData().GetStackValue().opdata == 3);
            ProtoCore.Mirror.RuntimeMirror mirror4 = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror4.GetData().GetStackValue().opdata == 3);
            List<SynchronizeData> ls2 = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T117_IDE_2187_2.xml");

            foreach (SynchronizeData sd2 in ls2)
                liveRunner.UpdateGraph(sd2);

            ProtoCore.Mirror.RuntimeMirror mirror5 = liveRunner.QueryNodeValue("Var1");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror6 = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror2.GetData().GetStackValue().opdata == 2);
            ProtoCore.Mirror.RuntimeMirror mirror7 = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror3.GetData().GetStackValue().opdata == 3);
            ProtoCore.Mirror.RuntimeMirror mirror8 = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror4.GetData().GetStackValue().opdata == 3);

        }
        [Test]
        public void T118_IDE_1942()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1942

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T118_IDE_1942.xml");
            foreach (SynchronizeData sd in ls)
            {
                liveRunner.UpdateGraph(sd);
                
            }
            //int a = sd.AddedNodes.Count - sd.RemovedNodes.Count;

            GraphToDSCompiler.GraphCompiler graphcompiler = (liveRunner as ProtoScript.Runners.LiveRunner).GetCurrentGraphCompilerInstance();
            int count = graphcompiler.Graph.nodeList.Count;
            //Assert.IsTrue(count == 2);
            
            ProtoCore.Mirror.RuntimeMirror mirror= liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
            List<SynchronizeData> ls2 = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T118_IDE_1942_2.xml");

            foreach (SynchronizeData sd2 in ls2)
                liveRunner.UpdateGraph(sd2);

            //int b = sd2.AddedNodes.Count - sd2.RemovedNodes.Count;
            graphcompiler = (liveRunner as ProtoScript.Runners.LiveRunner).GetCurrentGraphCompilerInstance();
            count = graphcompiler.Graph.nodeList.Count;
            //Assert.IsTrue(count == 1);
            
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("Var2");
            Assert.IsTrue(mirror2.GetData().GetStackValue().optype == ProtoCore.DSASM.AddressType.Null);
            ProtoCore.Mirror.RuntimeMirror mirror3 = liveRunner.QueryNodeValue("Var3");
            Assert.IsTrue(mirror3.GetData().GetStackValue().opdata == 1);
            

        }
        [Test]
        public void T126_IDE_1936()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1936

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_1936.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("Var1");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("b");
            Assert.IsTrue(mirror2.GetData().GetStackValue().opdata == 1);



        }
        /*
        [Test]
        public void T126_IDE_2152()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2152
          // commenting this test as the variable name is nto consistent every time 

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "deffect_2152.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("_temp_f111ee543d4946f9aa624c6a273dfffb");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 6);
            



        }*/

        [Test]
        public void T127_IDE_1934()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1934

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T127_IDE_1934.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("a");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 1);
            ProtoCore.Mirror.RuntimeMirror mirror2 = liveRunner.QueryNodeValue("b");
            Assert.IsTrue(mirror2.GetData().GetStackValue().opdata == 2);
            mirror2 = liveRunner.QueryNodeValue("x");
            Assert.IsTrue(mirror2.GetData().GetStackValue().opdata == 2);



        }

        [Test]
        public void T128_IDE_1845()
        {

            // Steps are mentioned in the defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1845

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<SynchronizeData> ls = GraphToDSCompiler.GraphUtilities.GetSDList(testPath + "T128_IDE_1845.xml");
            foreach (SynchronizeData sd in ls)
                liveRunner.UpdateGraph(sd);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.QueryNodeValue("x");
            Assert.IsTrue(mirror.GetData().GetStackValue().opdata == 2);
        }
        
    }
    /*
     * Test Cases for Struct VariableLine
     */
    class VariableLineTests
    {
        [SetUp]
        public void SetupTest()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestEqual()
        {
            //same name, same line, equal
            VariableLine variableLine1 = new VariableLine("variable1", 123312);
            VariableLine variableLine2 = new VariableLine("variable1", 123312);
            Assert.AreEqual(variableLine1, variableLine2);
        }

        [Test]
        public void TestDiffName()
        {
            //diff name, same line, not equal
            VariableLine variableLine1 = new VariableLine("variable1", 123312);
            VariableLine variableLine2 = new VariableLine("variable2", 123312);
            Assert.AreNotEqual(variableLine1, variableLine2);
        }

        [Test]
        public void TestDiddLine()
        {
            //same name, diff line, not equal
            VariableLine variableLine1 = new VariableLine("variable1", 123312);
            VariableLine variableLine2 = new VariableLine("variable1", 68678);
            Assert.AreNotEqual(variableLine1, variableLine2);
        }

        [Test]
        public void TestDiffNameLine()
        {
            //diff name, diff line, not equal
            VariableLine variableLine1 = new VariableLine("variable1", 123312);
            VariableLine variableLine2 = new VariableLine("variable2", 68678);
            Assert.AreNotEqual(variableLine1, variableLine2);
        }

        [Test]
        public void TestNullObject()
        {
            //2 null object, equal
            VariableLine variableLine1 = new VariableLine();
            VariableLine variableLine2 = new VariableLine();
            Assert.AreEqual(variableLine1, variableLine2);
        }

        [Test]
        public void Test1NullObject()
        {
            //1 null object, not equal
            VariableLine variableLine1 = new VariableLine("variable1", 123312);
            VariableLine variableLine2 = new VariableLine();
            Assert.AreNotEqual(variableLine1, variableLine2);
        }

        [Test]
        public void TestCapitalLetter()
        {
            //diff name, same line not equal
            VariableLine variableLine1 = new VariableLine("VARIablE", 123312);
            VariableLine variableLine2 = new VariableLine("variable1", 123312);
            Assert.AreNotEqual(variableLine1, variableLine2);
            //same name, same line, equal
            variableLine1 = new VariableLine("VARIablE", 123312);
            variableLine2 = new VariableLine("VARIablE", 123312);
            Assert.AreEqual(variableLine1, variableLine2);
            //same name, diff line, not equal
            variableLine1 = new VariableLine("VARIablE", 123312);
            variableLine2 = new VariableLine("VARIablE", 68678);
            Assert.AreNotEqual(variableLine1, variableLine2);
        }

        [Test]
        public void TestNegativeLine()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                VariableLine v = new VariableLine("variable", -100);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                VariableLine v = new VariableLine("", 100);
            });
        }

        [Test]
        public void TestInsertToDict()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Dictionary<VariableLine, int> dictionary = new Dictionary<VariableLine, int>();
                VariableLine vL = new VariableLine("variable", 213);
                dictionary.Add(vL, 1);
                dictionary.Add(vL, 2);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Dictionary<VariableLine, int> dictionary = new Dictionary<VariableLine, int>();
                VariableLine vL = new VariableLine("variable", 213);
                dictionary.Add(vL, 1);
                dictionary.Add(vL, 1);
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                Dictionary<VariableLine, int> dictionary = new Dictionary<VariableLine, int>();
                VariableLine vL = new VariableLine("variable", 213);
                VariableLine vL2 = new VariableLine();
                dictionary.Add(vL, 1);
                dictionary.Add(vL2, 1);
            });
        }

    }
}



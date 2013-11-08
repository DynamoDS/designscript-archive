using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoScript.Runners;
using ProtoTestFx.TD;

namespace ProtoTest.ProtoAST
{
    public class ProtoASTExecutionTests
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestProtoASTExecute_Assign01()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }

        [Test]
        public void TestProtoASTExecute_Assign02()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
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

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 30);
        }

        [Test]
        public void TestProtoASTExecute_Assign03()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            /*b = 20;*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 50)*(b + 20)*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode("50"),
                        ProtoCore.DSASM.Operator.add),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode("20"),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.mul),
                ProtoCore.DSASM.Operator.assign);
            /*c = a - 200*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode("200"),
                    ProtoCore.DSASM.Operator.sub),
                ProtoCore.DSASM.Operator.assign);
            /*d = b*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign4 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("d"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);
            astList.Add(assign1);
            astList.Add(assign3);
            astList.Add(assign4);
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            //a = 2800, c = 2600, d = b = 20
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 2800);
            o = mirror.GetValue("c");
            Assert.IsTrue((Int64)o.Payload == 2600);
            Obj p = mirror.GetValue("b");
            Assert.IsTrue((Int64)p.Payload == 20);
            o = mirror.GetValue("d");
            Assert.IsTrue((Int64)o.Payload == 20);
        }

        [Test]
        public void TestProtoASTExecute_Assign04()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("30"),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b - 10) * 20 + (b + 10) * (b - 20) */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment =
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                                ProtoCore.DSASM.Operator.sub),
                            new ProtoCore.AST.AssociativeAST.IntNode("20"),
                            ProtoCore.DSASM.Operator.mul),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                                ProtoCore.DSASM.Operator.add),
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                                ProtoCore.DSASM.Operator.sub),
                            ProtoCore.DSASM.Operator.mul),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.assign);
            /*c = a*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);
            /*a = a + 1000*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode("1000"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(assignment);
            astList.Add(nodeC);
            astList.Add(assignment2);
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            //a = 1800, c = a = 1800
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 1800);
            o = mirror.GetValue("c");
            Assert.IsTrue((Int64)o.Payload == 1800);
        }

        [Test]
        public void TestProtoASTExecute_Assign05()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("30"),
                ProtoCore.DSASM.Operator.assign);
            /*c = b + 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                    new ProtoCore.AST.AssociativeAST.IntNode("30"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 20) - (c - 10) + c*5 */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                            new ProtoCore.AST.AssociativeAST.IntNode("20"),
                            ProtoCore.DSASM.Operator.add),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                            new ProtoCore.AST.AssociativeAST.IntNode("10"),
                            ProtoCore.DSASM.Operator.sub),
                        ProtoCore.DSASM.Operator.sub),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                        new ProtoCore.AST.AssociativeAST.IntNode("5"),
                        ProtoCore.DSASM.Operator.mul),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(nodeC);
            astList.Add(assignment);
            /*a = 300, b = 30, c= 60 */
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 300);
        }

        [Test]
        public void TestProtoASTExecute_FunctionDefAndCall_01()
        {
            //  def foo()
            //  {
            //    b = 10;
            //    return = b + 10;
            //  }
            //  
            //  x = foo();

            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();


            // Build the function body
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                returnExpr,
                ProtoCore.DSASM.Operator.assign);
            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);


            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = functionName;
            funcDefNode.FunctionBody = cbn;

            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(funcDefNode);

            // Build the statement that calls the function foo
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode(functionName);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode callstmt = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                functionCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(callstmt);


            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            Obj o = mirror.GetValue("x");
            Assert.IsTrue((Int64)o.Payload == 20);

        }

        [Test]
        public void TestProtoASTExecute_FunctionDefAndCall_02()
        {
            //  def foo(a : int)
            //  {
            //    b = 10;
            //    return = b + a;
            //  }
            //  
            //  x = foo(1);

            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();


            // Build the function body
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.add);


            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                returnExpr,
                ProtoCore.DSASM.Operator.assign);

            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);


            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = functionName;
            funcDefNode.FunctionBody = cbn;

            // build the args signature
            funcDefNode.Singnature = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
            ProtoCore.AST.AssociativeAST.VarDeclNode arg1Decl = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            arg1Decl.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");

            // Build the type of arg1
            ProtoCore.Type arg1Type = new ProtoCore.Type();
            arg1Type.Initialize();
            arg1Type.UID = (int)ProtoCore.PrimitiveType.kTypeInt;
            arg1Type.Name = ProtoCore.DSDefinitions.Keyword.Int;
            arg1Decl.ArgumentType = arg1Type;
            funcDefNode.Singnature.AddArgument(arg1Decl);


            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            // Build the statement that calls the function foo
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode(functionName);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(funcDefNode);

            // Function call
            // Function args
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            args.Add(new ProtoCore.AST.AssociativeAST.IntNode("1"));
            functionCall.FormalArguments = args;

            // Call the function
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode callstmt = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                functionCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(callstmt);


            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            Obj o = mirror.GetValue("x");
            Assert.IsTrue((Int64)o.Payload == 11);

        }

        [Test]
        public void TestProtoASTExecute_ClassDecl_PropertyAccess_01()
        {

            //  class bar
            //  {
            //       f : var;
            //  }
            //
            //  p = bar.bar();
            //  p.f = 10;
            //  a = p.f;


            // Create the class node AST
            ProtoCore.AST.AssociativeAST.ClassDeclNode classDefNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDefNode.className = "bar";

            // Create the property AST
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Name = "f";
            varDeclNode.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            varDeclNode.ArgumentType = new ProtoCore.Type()
            {
                Name = "int",
                IsIndexable = false,
                rank = 0,
                UID = (int)ProtoCore.PrimitiveType.kTypeInt
            };
            classDefNode.varlist.Add(varDeclNode);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(classDefNode);


            // p = bar.bar();
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListConstrcctorCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListConstrcctorCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");
            identListConstrcctorCall.RightNode = constructorCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtInitClass = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                identListConstrcctorCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtInitClass);


            //  p.f = 10;
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListPropertySet = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListPropertySet.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListPropertySet.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertySet = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                identListPropertySet,
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertySet);


            //  a = p.f; 
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListPropertyAccess = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListPropertyAccess.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListPropertyAccess.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertyAccess = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                identListPropertyAccess,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertyAccess);

            // Execute the AST
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
        }

        [Test]
        public void TestProtoASTExecute_ClassDecl_MemFunctionCall_01()
        {

            //  class bar
            //  {
            //       f : var
            //       def foo (b:int)
            //       {
            //           b = 10;
            //           return = b + 10;
            //       }
            //  }
            //
            //  p = bar.bar();
            //  a = p.foo();


            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();


            // Build the function body
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                returnExpr,
                ProtoCore.DSASM.Operator.assign);
            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);


            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = functionName;
            funcDefNode.FunctionBody = cbn;

            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            // Create the class node AST
            ProtoCore.AST.AssociativeAST.ClassDeclNode classDefNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDefNode.className = "bar";

            // Add the member function 'foo'
            classDefNode.funclist.Add(funcDefNode);


            // Create the property AST
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Name = "f";
            varDeclNode.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            varDeclNode.ArgumentType = new ProtoCore.Type()
            {
                Name = "int",
                IsIndexable = false,
                rank = 0,
                UID = (int)ProtoCore.PrimitiveType.kTypeInt
            };
            classDefNode.varlist.Add(varDeclNode);


            // Add the constructed class AST
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(classDefNode);


            // p = bar.bar();
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListConstrcctorCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListConstrcctorCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");
            identListConstrcctorCall.RightNode = constructorCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtInitClass = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                identListConstrcctorCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtInitClass);

            //  a = p.f; 

            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("foo");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListFunctionCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListFunctionCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListFunctionCall.RightNode = functionCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertyAccess = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                identListFunctionCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertyAccess);


            // Execute the AST
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 20);
        }

        [Test]
        public void TestCodeGenDS_Assign01()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            // emit the DS code from the AST tree
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();
            // Verify the results
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }

        [Test]
        public void TestCodeGenDS_Assign02()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
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
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();
            // Verify the results
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 30);
        }

        [Test]
        public void TestCodeGenDS_Assign03()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();

            /*b = 20;*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 50)*(b + 20)*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode("50"),
                        ProtoCore.DSASM.Operator.add),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode("20"),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.mul),
                ProtoCore.DSASM.Operator.assign);
            /*c = a - 200*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode("200"),
                    ProtoCore.DSASM.Operator.sub),
                ProtoCore.DSASM.Operator.assign);
            /*d = b*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign4 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("d"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);
            astList.Add(assign1);
            astList.Add(assign3);
            astList.Add(assign4);

            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //a = 2800, c = 2600, d = b = 20
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 2800);
            o = mirror.GetValue("c");
            Assert.IsTrue((Int64)o.Payload == 2600);
            Obj p = mirror.GetValue("b");
            Assert.IsTrue((Int64)p.Payload == 20);
            o = mirror.GetValue("d");
            Assert.IsTrue((Int64)o.Payload == 20);
        }

        [Test]
        public void TestCodeGenDS_Assign04()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("30"),
                ProtoCore.DSASM.Operator.assign);

            /*a = (b - 10) * 20 + (b + 10) * (b - 20) */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment =
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                                ProtoCore.DSASM.Operator.sub),
                            new ProtoCore.AST.AssociativeAST.IntNode("20"),
                            ProtoCore.DSASM.Operator.mul),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                                ProtoCore.DSASM.Operator.add),
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode("20"),
                                ProtoCore.DSASM.Operator.sub),
                            ProtoCore.DSASM.Operator.mul),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.assign);
            /*c = a*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);
            /*a = a + 1000*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode("1000"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(assignment);
            astList.Add(nodeC);
            astList.Add(assignment2);
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //a = 1800, c = a = 1800
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 1800);
            o = mirror.GetValue("c");
            Assert.IsTrue((Int64)o.Payload == 1800);
        }

        [Test]
        public void TestCodeGenDS_Assign05()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("30"),
                ProtoCore.DSASM.Operator.assign);
            /*c = b + 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                    new ProtoCore.AST.AssociativeAST.IntNode("30"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 20) - (c - 10) + c*5 */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                            new ProtoCore.AST.AssociativeAST.IntNode("20"),
                            ProtoCore.DSASM.Operator.add),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                            new ProtoCore.AST.AssociativeAST.IntNode("10"),
                            ProtoCore.DSASM.Operator.sub),
                        ProtoCore.DSASM.Operator.sub),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                        new ProtoCore.AST.AssociativeAST.IntNode("5"),
                        ProtoCore.DSASM.Operator.mul),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(nodeC);
            astList.Add(assignment);
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
            /*a = 300, b = 30, c= 60 */
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 300);
        }

        [Test]
        public void TestCodeGenDS_FunctionDefNode1()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.add);
            ProtoCore.AST.AssociativeAST.ReturnNode returnNode = new ProtoCore.AST.AssociativeAST.ReturnNode();
            returnNode.ReturnExpr = returnExpr;
            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);
            ///
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = "foo";
            funcDefNode.FunctionBody = cbn;
            /* def foo()             * {             *   b = 10;             *   return = b + 10;             * }*/
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(funcDefNode);
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
        }


        [Test]
        public void TestCodeGenDS_ClassDecl_PropertyAccess_01()
        {

            //  class bar            //  {            //       f : var;            //  }            //
            //  p = bar.bar();
            //  p.f = 10;            //  a = p.f;            

            // Create the class node AST
            ProtoCore.AST.AssociativeAST.ClassDeclNode classDefNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDefNode.className = "bar";

            // Create the property AST
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Name = "f";
            varDeclNode.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            varDeclNode.ArgumentType = new ProtoCore.Type()
            {
                Name = "int",
                IsIndexable = false,
                rank = 0,
                UID = (int)ProtoCore.PrimitiveType.kTypeInt
            };
            classDefNode.varlist.Add(varDeclNode);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(classDefNode);


            // p = bar.bar();
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListConstrcctorCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListConstrcctorCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");
            identListConstrcctorCall.RightNode = constructorCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtInitClass = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                identListConstrcctorCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtInitClass);


            //  p.f = 10;
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListPropertySet = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListPropertySet.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListPropertySet.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertySet = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                identListPropertySet,
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertySet);


            //  a = p.f; 
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListPropertyAccess = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListPropertyAccess.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListPropertyAccess.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertyAccess = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                identListPropertyAccess,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertyAccess);

            // Generate the script
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();


            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
        }

        [Test]
        public void TestCodeGenDS_ClassDecl_MemFunctionCall_01()
        {

            //  class bar
            //  {
            //       f : var
            //       def foo (b:int)
            //       {
            //           b = 10;
            //           return = b + 10;
            //       }
            //  }
            //
            //  p = bar.bar();
            //  a = p.foo();


            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();


            // Build the function body
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                returnExpr,
                ProtoCore.DSASM.Operator.assign);
            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);


            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = functionName;
            funcDefNode.FunctionBody = cbn;

            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            // Create the class node AST
            ProtoCore.AST.AssociativeAST.ClassDeclNode classDefNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDefNode.className = "bar";

            // Add the member function 'foo'
            classDefNode.funclist.Add(funcDefNode);


            // Create the property AST
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Name = "f";
            varDeclNode.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            varDeclNode.ArgumentType = new ProtoCore.Type()
            {
                Name = "int",
                IsIndexable = false,
                rank = 0,
                UID = (int)ProtoCore.PrimitiveType.kTypeInt
            };
            classDefNode.varlist.Add(varDeclNode);


            // Add the constructed class AST
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(classDefNode);


            // p = bar.bar();
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListConstrcctorCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListConstrcctorCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");
            identListConstrcctorCall.RightNode = constructorCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtInitClass = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                identListConstrcctorCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtInitClass);

            //  a = p.f; 

            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("foo");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListFunctionCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListFunctionCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListFunctionCall.RightNode = functionCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertyAccess = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                identListFunctionCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertyAccess);


            // Generate the script
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 20);
        }
    }

}
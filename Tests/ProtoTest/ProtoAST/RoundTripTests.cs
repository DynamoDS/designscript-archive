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
    public class RoundTripTests
    {
        public TestFrameWork thisTest = new TestFrameWork();

        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void TestProtoASTExecute_RoundTrip_Assign01()
        {
            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================

            int result1 = 10;
            ExecutionMirror mirror = null;

            // 1. Build AST 
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode("10"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);

            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);
        }

        [Test]
        public void TestProtoASTExecute_RoundTrip_Assign02()
        {
            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================

            int result1 = 30;
            ExecutionMirror mirror = null;

            // 1. Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode("10"),
                    new ProtoCore.AST.AssociativeAST.IntNode("20"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);

            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);
        }


        [Test]
        public void TestProtoASTExecute_RoundTrip_FunctionDefAndCall_01()
        {

            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================
            int result1 = 20;
            ExecutionMirror mirror = null;



            // 1. Build the AST tree

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


            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);


            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);

        }

        [Test]
        public void TestProtoASTExecute_RoundTrip_FunctionDefAndCall_02()
        {
            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================
            int result1 = 11;
            ExecutionMirror mirror = null;

            // 1. Build the AST tree


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



            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);


            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);

        }
    }

}
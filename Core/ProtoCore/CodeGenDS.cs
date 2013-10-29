using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ProtoCore.Utils;

namespace ProtoCore
{
    /// <summary>
    /// The code generator takes Abstract Syntax Tree and generates the DesignScript code
    /// </summary>
    public class CodeGenDS
    {
        public List<ProtoCore.AST.AssociativeAST.AssociativeNode> astNodeList { get; private set; }
        string code = string.Empty;

        public string Code { get { return code; } }

        /// <summary>
        /// This is used during ProtoAST generation to connect BinaryExpressionNode's 
        /// generated from Block nodes to its child AST tree - pratapa
        /// </summary>
        //protected ProtoCore.AST.AssociativeAST.BinaryExpressionNode ChildTree { get; set; }

        public CodeGenDS(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList)
        {
            this.astNodeList = astList;
        }

        //public CodeGenDS(ProtoCore.AST.AssociativeAST.BinaryExpressionNode bNode) 
        //{
        //    ChildTree = bNode;
        //}

        public CodeGenDS() 
        {}
        
        /// <summary>
        /// This function prints the DS code into the destination stream
        /// </summary>
        /// <param name="code"></param>
        protected virtual void EmitCode(string code)
        {
            this.code += code;
        }

        public string GenerateCode()
        {
            Validity.Assert(null != astNodeList);
            
            for (int i = 0; i < astNodeList.Count; i++)
            {
                DFSTraverse(astNodeList[i]);
            }
            return code;
        }

        /// <summary>
        /// Depth first traversal of an AST node
        /// </summary>
        /// <param name="node"></param>
        public void DFSTraverse( ProtoCore.AST.Node node, bool useByProtoAst = false)
        {
            if (node is ProtoCore.AST.AssociativeAST.ImportNode)
            {
                EmitImportNode(node as ProtoCore.AST.AssociativeAST.ImportNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                //if (useByProtoAst)
                //{
                //    ProtoCore.AST.AssociativeAST.IdentifierNode iNode = ChildTree.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
                //    Validity.Assert(iNode != null);

                //    if ((node as ProtoCore.AST.AssociativeAST.IdentifierNode).Value == iNode.Value)
                //        node = ChildTree;
                //}
                //else
                    EmitIdentifierNode(node as ProtoCore.AST.AssociativeAST.IdentifierNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                EmitIdentifierListNode(node as ProtoCore.AST.AssociativeAST.IdentifierListNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.IntNode)
            {
                EmitIntNode(node as ProtoCore.AST.AssociativeAST.IntNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.DoubleNode)
            {
                EmitDoubleNode(node as ProtoCore.AST.AssociativeAST.DoubleNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                EmitFunctionCallNode(node as ProtoCore.AST.AssociativeAST.FunctionCallNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.FunctionDotCallNode)
            {
                EmitFunctionDotCallNode(node as ProtoCore.AST.AssociativeAST.FunctionDotCallNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
            {
                ProtoCore.AST.AssociativeAST.BinaryExpressionNode binaryExpr = node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                if (binaryExpr.Optr != DSASM.Operator.assign)
                    EmitCode("(");
                EmitBinaryNode(binaryExpr);
                if (binaryExpr.Optr == DSASM.Operator.assign)
                {
                    EmitCode(ProtoCore.DSASM.Constants.termline);
                }
                if (binaryExpr.Optr != DSASM.Operator.assign)
                    EmitCode(")");
            }
            else if (node is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode)
            {
                EmitFunctionDefNode(node as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ClassDeclNode)
            {
                EmitClassDeclNode(node as ProtoCore.AST.AssociativeAST.ClassDeclNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.NullNode)
            {
                EmitNullNode(node as ProtoCore.AST.AssociativeAST.NullNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.RangeExprNode)
            {
                EmitRangeExprNode(node as ProtoCore.AST.AssociativeAST.RangeExprNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ArrayIndexerNode)
            {
                EmitArrayIndexerNode(node as ProtoCore.AST.AssociativeAST.ArrayIndexerNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ArrayNode)
            {
                EmitArrayNode(node as ProtoCore.AST.AssociativeAST.ArrayNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ExprListNode)
            {
                EmitExprListNode(node as ProtoCore.AST.AssociativeAST.ExprListNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ModifierStackNode)
            {
                EmitModifierStackNode(node as ProtoCore.AST.AssociativeAST.ModifierStackNode);
            }
        }


        /// <summary>
        /// These functions emit the DesignScript code on the destination stream
        /// </summary>
        /// <param name="identNode"></param>
#region ASTNODE_CODE_EMITTERS

        private void EmitExprListNode(AST.AssociativeAST.ExprListNode exprListNode)
        {
            EmitCode("{");
            foreach (AST.AssociativeAST.AssociativeNode node in exprListNode.list)
            {
                DFSTraverse(node);
                EmitCode(",");
            }
            code = code.TrimEnd(',');
            EmitCode("}");
        }

        private void EmitModifierStackNode(AST.AssociativeAST.ModifierStackNode modifierStackNode)
        {
            EmitCode("{");
            foreach (AST.AssociativeAST.AssociativeNode node in modifierStackNode.ElementNodes)
            {
                DFSTraverse(node);
                EmitCode(";");
            }
            EmitCode("}");
        }
        private void EmitArrayNode(AST.AssociativeAST.ArrayNode arrayNode)
        {
            if (null != arrayNode)
            {
                EmitCode("[");
                DFSTraverse(arrayNode.Expr);
                EmitCode("]");
                if (arrayNode.Type != null)
                {
                    DFSTraverse(arrayNode.Type);
                }
            }
        }

        private void EmitArrayIndexerNode(AST.AssociativeAST.ArrayIndexerNode arrayIndexerNode)
        {
            EmitCode((arrayIndexerNode.Array as ProtoCore.AST.AssociativeAST.IdentifierNode).Value);
            EmitCode("[");
            DFSTraverse(arrayIndexerNode.ArrayDimensions.Expr);
            EmitCode("]");
            if (arrayIndexerNode.ArrayDimensions.Type != null)
            {
                DFSTraverse(arrayIndexerNode.ArrayDimensions.Type);
            }
        }

        private void EmitRangeExprNode(AST.AssociativeAST.RangeExprNode rangeExprNode)
        {
            Validity.Assert(null != rangeExprNode);
            if (rangeExprNode.FromNode is AST.AssociativeAST.IntNode)
                EmitCode((rangeExprNode.FromNode as AST.AssociativeAST.IntNode).value);
            else if (rangeExprNode.FromNode is AST.AssociativeAST.IdentifierNode)
                EmitCode((rangeExprNode.FromNode as AST.AssociativeAST.IdentifierNode).Value);
            EmitCode("..");

            if (rangeExprNode.ToNode is AST.AssociativeAST.IntNode)
                EmitCode((rangeExprNode.ToNode as AST.AssociativeAST.IntNode).value);
            else if (rangeExprNode.ToNode is AST.AssociativeAST.IdentifierNode)
                EmitCode((rangeExprNode.ToNode as AST.AssociativeAST.IdentifierNode).Value);

            if (rangeExprNode.StepNode != null)
            {
                EmitCode("..");
                if (rangeExprNode.stepoperator == ProtoCore.DSASM.RangeStepOperator.num)
                    EmitCode("#");
                if (rangeExprNode.StepNode is AST.AssociativeAST.IntNode)
                    EmitCode((rangeExprNode.StepNode as AST.AssociativeAST.IntNode).value);
                else if (rangeExprNode.StepNode is AST.AssociativeAST.IdentifierNode)
                    EmitCode((rangeExprNode.StepNode as AST.AssociativeAST.IdentifierNode).Value);
            }
        }

        protected virtual void EmitImportNode(ProtoCore.AST.AssociativeAST.ImportNode importNode)
        {
            Validity.Assert(null != importNode);
            EmitCode("import(");
            EmitCode("\"");
            EmitCode(importNode.ModuleName);
            EmitCode("\"");
            EmitCode(");\n");
        }

        protected virtual void EmitIdentifierNode(ProtoCore.AST.AssociativeAST.IdentifierNode identNode)
        {
            Validity.Assert(null != identNode);
            EmitCode(identNode.Value);
            EmitArrayNode(identNode.ArrayDimensions);
        }

        protected virtual void EmitIdentifierListNode(ProtoCore.AST.AssociativeAST.IdentifierListNode identList)
        {
            Validity.Assert(null != identList);
            //ProtoCore.AST.AssociativeAST.AssociativeNode left = identList.LeftNode;
            DFSTraverse(identList.LeftNode);
            EmitCode(".");
            DFSTraverse(identList.RightNode);
        }

        protected virtual void EmitIntNode(ProtoCore.AST.AssociativeAST.IntNode intNode)
        {
            Validity.Assert(null != intNode);
            EmitCode(intNode.value);
        }

        protected virtual void EmitDoubleNode(ProtoCore.AST.AssociativeAST.DoubleNode doubleNode)
        {
            Validity.Assert(null != doubleNode);
            EmitCode(doubleNode.value);
        }

        protected virtual void EmitFunctionCallNode(ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallNode)
        {
            Validity.Assert(null != funcCallNode);

            Validity.Assert(funcCallNode.Function is ProtoCore.AST.AssociativeAST.IdentifierNode);
            string functionName = (funcCallNode.Function as ProtoCore.AST.AssociativeAST.IdentifierNode).Value;

            Validity.Assert(!string.IsNullOrEmpty(functionName));
            if (functionName.StartsWith("%"))
            {
                EmitCode("(");
                DFSTraverse(funcCallNode.FormalArguments[0], true);
                switch (functionName)
                {
                    case "%add":
                        EmitCode("+");
                        break;
                    case "%sub":
                        EmitCode("-");
                        break;
                    case "%mul":
                        EmitCode("*");
                        break;
                    case "%div":
                        EmitCode("/");
                        break;
                    case "%mod":
                        EmitCode("%");
                        break;
                    case "%Not":
                        EmitCode("!");
                        break;
                }

                if (funcCallNode.FormalArguments.Count > 1)
                {
                    DFSTraverse(funcCallNode.FormalArguments[1], true);
                }
                EmitCode(")");
            }
            else
            {
                EmitCode(functionName);

                EmitCode("(");
                for (int n = 0; n < funcCallNode.FormalArguments.Count; ++n) 
                {
                    ProtoCore.AST.AssociativeAST.AssociativeNode argNode = funcCallNode.FormalArguments[n];
                    DFSTraverse(argNode, true);
                    if (n+1 < funcCallNode.FormalArguments.Count)
                    {
                        EmitCode(",");
                    }
                }
                EmitCode(")");
            }
        }

        protected virtual void EmitFunctionDotCallNode(ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall)
        {
            Validity.Assert(null != dotCall);

            //EmitCode(dotCall.lhsName);
            EmitCode((dotCall.DotCall.FormalArguments[0] as AST.AssociativeAST.IdentifierNode).Value);
            EmitCode(".");
            EmitFunctionCallNode(dotCall.FunctionCall);
        }

        protected virtual void EmitBinaryNode(ProtoCore.AST.AssociativeAST.BinaryExpressionNode binaryExprNode)
        {
            Validity.Assert(null != binaryExprNode);
            DFSTraverse(binaryExprNode.LeftNode);
            EmitCode(ProtoCore.Utils.CoreUtils.GetOperatorString(binaryExprNode.Optr));
            DFSTraverse(binaryExprNode.RightNode);
        }

        protected virtual void EmitFunctionDefNode(ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode)
        {
            EmitCode("def ");
            EmitCode(funcDefNode.Name);

            if (funcDefNode.ReturnType.UID != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                EmitCode(": " + funcDefNode.ReturnType.Name);
            }

            if (funcDefNode.Singnature != null)
            {
                EmitCode(funcDefNode.Singnature.ToString());
            }
            else 
            {
                EmitCode("()\n");
            }
            
            if (null != funcDefNode.FunctionBody)
            {
                List<ProtoCore.AST.AssociativeAST.AssociativeNode> funcBody = funcDefNode.FunctionBody.Body;
            
                EmitCode("{\n");
                foreach (ProtoCore.AST.AssociativeAST.AssociativeNode bodyNode in funcBody)
                {
                    if (bodyNode is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                        EmitBinaryNode(bodyNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode);
                    if (bodyNode is ProtoCore.AST.AssociativeAST.ReturnNode)
                        EmitReturnNode(bodyNode as ProtoCore.AST.AssociativeAST.ReturnNode);
                    EmitCode(";\n");
                }
                EmitCode("}");
            }
            EmitCode("\n");
        }

        protected virtual void EmitReturnNode(ProtoCore.AST.AssociativeAST.ReturnNode returnNode)
        {
            EmitCode("return = ");
            ProtoCore.AST.AssociativeAST.AssociativeNode rightNode = returnNode.ReturnExpr;
            DFSTraverse(rightNode);
        }

        protected virtual void EmitVarDeclNode(ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode)
        {
            EmitCode(varDeclNode.NameNode.Name + " : " + varDeclNode.ArgumentType.Name + ";\n");
        }

        protected virtual void EmitClassDeclNode(ProtoCore.AST.AssociativeAST.ClassDeclNode classDeclNode)
        {
            //EmitCode(classDeclNode.ToString());
            EmitCode("class ");
            EmitCode(classDeclNode.className);
            EmitCode("\n{\n");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> varList = classDeclNode.varlist;
            foreach (ProtoCore.AST.AssociativeAST.AssociativeNode varMember in varList)
            {
                //how is var member stored?
                if (varMember is ProtoCore.AST.AssociativeAST.VarDeclNode)
                {
                    EmitVarDeclNode(varMember as ProtoCore.AST.AssociativeAST.VarDeclNode);
                }
            }
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> funcList = classDeclNode.funclist;
            foreach (ProtoCore.AST.AssociativeAST.AssociativeNode funcMember in funcList)
            {
                if (funcMember is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode)
                {
                    EmitFunctionDefNode(funcMember as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode);
                }
            }
            EmitCode("}\n");
        }

        protected virtual void EmitNullNode(ProtoCore.AST.AssociativeAST.NullNode nullNode)
        {
            Validity.Assert(null != nullNode);
            EmitCode("null");
        }

#endregion

    }
}

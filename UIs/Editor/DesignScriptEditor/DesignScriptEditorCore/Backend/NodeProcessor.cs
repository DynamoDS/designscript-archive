using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Core
{
    using DesignScript.Parser;

    class NodeProcessor
    {

        List<Node> nodeList = null;
        List<string> functionTable = null;

        public NodeProcessor(List<Node> nodeList)
        {
            this.nodeList = nodeList;
            functionTable = new List<string>();
        }

        public void GenerateFragments(out CodeFragment[] fragmentsArray)
        {
            List<CodeFragment> fragments = new List<CodeFragment>();
            ProcessNode(nodeList, fragments);
            fragmentsArray = new CodeFragment[fragments.Count];
            fragmentsArray = fragments.ToArray();
        }

        void ProcessNode(List<Node> nodeList, List<CodeFragment> fragments)
        {
            foreach (Node node in nodeList)
            {
                ProcessNode(node, fragments);
            }
        }

        void ProcessNode(Node node, List<CodeFragment> fragments)
        {
            if (null != node)
            {
                if ((node.GetType().Namespace) == "DesignScript.Parser.Imperative")
                    ProcessImperativeNode(node, fragments);
                else if ((node.GetType().Namespace) == "DesignScript.Parser.Associative")
                    ProcessAssociativeNode(node, fragments);
                else // CommentNode
                {
                    ProcessCommentNode(node as CommentNode, fragments);
                }
            }

        }

        #region Imperative
        void ProcessImperativeNode(Node node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            switch (node.GetType().ToString())
            {
                case "DesignScript.Parser.Imperative.LanguageBlockNode":
                    ProcessImperativeLanguageBlockNode(node as DesignScript.Parser.Imperative.LanguageBlockNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.IdentifierNode":
                    ProcessImperativeIdentifierNode(node as DesignScript.Parser.Imperative.IdentifierNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.IntNode":
                    ProcessImperativeIntNode(node as DesignScript.Parser.Imperative.IntNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.DoubleNode":
                    ProcessImperativeDoubleNode(node as DesignScript.Parser.Imperative.DoubleNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.BooleanNode":
                    ProcessImperativeBooleanNode(node as DesignScript.Parser.Imperative.BooleanNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.CharNode":
                    ProcessImperativeCharNode(node as DesignScript.Parser.Imperative.CharNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.StringNode":
                    ProcessImperativeStringNode(node as DesignScript.Parser.Imperative.StringNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.NullNode":
                    ProcessImperativeNullNode(node as DesignScript.Parser.Imperative.NullNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.ArrayNode":
                    ProcessImperativeArrayNode(node as DesignScript.Parser.Imperative.ArrayNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.FunctionCallNode":
                    ProcessImperativeFunctionCallNode(node as DesignScript.Parser.Imperative.FunctionCallNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.VarDeclNode":
                    ProcessImperativeVarDeclNode(node as DesignScript.Parser.Imperative.VarDeclNode, fragments);
                    break;
                /*case "ReturnNode":
                    ProcessImperativeReturnNode(node as ReturnNode, fragments);
                    break;*/
                case "DesignScript.Parser.Imperative.ArgumentSignatureNode":
                    ProcessImperativeArgumentSignatureNode(node as DesignScript.Parser.Imperative.ArgumentSignatureNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.ExprListNode":
                    ProcessImperativeExprListNode(node as DesignScript.Parser.Imperative.ExprListNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.CodeBlockNode":
                    ProcessImperativeCodeBlockNode(node as DesignScript.Parser.Imperative.CodeBlockNode, fragments);
                    break;
                /*case "ConstructorDefinitionNode":
                    ProcessImperativeConstructorDefinitionNode(node as ConstructorDefinitionNode, fragments);
                    break;*/
                case "DesignScript.Parser.Imperative.FunctionDefinitionNode":
                    ProcessImperativeFunctionDefinitionNode(node as DesignScript.Parser.Imperative.FunctionDefinitionNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.BinaryExpressionNode":
                    ProcessImperativeBinaryExpressionNode(node as DesignScript.Parser.Imperative.BinaryExpressionNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.ElseIfBlock":
                    ProcessImperativeElseIfBlock(node as DesignScript.Parser.Imperative.ElseIfBlock, fragments);
                    break;
                case "DesignScript.Parser.Imperative.IfStmtNode":
                    ProcessImperativeIfStmtNode(node as DesignScript.Parser.Imperative.IfStmtNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.WhileStmtNode":
                    ProcessImperativeWhileStmtNode(node as DesignScript.Parser.Imperative.WhileStmtNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.UnaryExpressionNode":
                    ProcessImperativeUnaryExpressionNode(node as DesignScript.Parser.Imperative.UnaryExpressionNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.RangeExprNode":
                    ProcessImperativeRangeExprNode(node as DesignScript.Parser.Imperative.RangeExprNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.ForLoopNode":
                    ProcessImperativeForLoopNode(node as DesignScript.Parser.Imperative.ForLoopNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.IdentifierListNode":
                    ProcessImperativeIdentifierListNode(node as DesignScript.Parser.Imperative.IdentifierListNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.IDEHelpNode":
                    ProcessImperativeIDEHelpNode(node as DesignScript.Parser.Imperative.IDEHelpNode, fragments);
                    break;
                // ParenExpressionNode __Fuqiang
                case "DesignScript.Parser.Imperative.ParenExpressionNode":
                    ProcessImperativeParenExpressionNode(node as DesignScript.Parser.Imperative.ParenExpressionNode, fragments);
                    break;
                // Modified by Jiong -- Begin
                case "DesignScript.Parser.Imperative.PostFixNode":
                    ProcessImperativePostFixNode(node as DesignScript.Parser.Imperative.PostFixNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.InlineConditionalNode":
                    ProcessImperativeInlineConditionalNode(node as DesignScript.Parser.Imperative.InlineConditionalNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.BreakNode":
                    ProcessImperativeBreakNode(node as DesignScript.Parser.Imperative.BreakNode, fragments);
                    break;
                case "DesignScript.Parser.Imperative.ContinueNode":
                    ProcessImperativeContinueNode(node as DesignScript.Parser.Imperative.ContinueNode, fragments);
                    break;
                // Modified by Jiong -- End
                default:
                    break;
            }
        }

        private void ProcessImperativeContinueNode(DesignScript.Parser.Imperative.ContinueNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeIDEHelpNode(node.KwContinue, fragments);
            ProcessImperativeIDEHelpNode(node.EndLine, fragments);
        }

        private void ProcessImperativeBreakNode(DesignScript.Parser.Imperative.BreakNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeIDEHelpNode(node.KwBreak, fragments);
            ProcessImperativeIDEHelpNode(node.EndLine, fragments);
        }
        void ProcessImperativeLanguageBlockNode(DesignScript.Parser.Imperative.LanguageBlockNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.IdentPos, fragments);
            if (node.ParaPosList != null)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode n in node.ParaPosList)
                    ProcessImperativeIDEHelpNode(n, fragments);
            }
            // Modified by Jiong -- Begin
            if (node.TextStringList != null)
            {
                foreach (DesignScript.Parser.Imperative.StringNode stringNode in node.TextStringList)
                {
                    ProcessImperativeStringNode(stringNode, fragments);
                }
            }
            // Modified by Jiong --End

            if (node.CodeBlock.language == ProtoCore.Language.kAssociative)
                ProcessAssociativeNode(node.CodeBlockNode, fragments);
            else if (node.CodeBlock.language == ProtoCore.Language.kImperative || node.CodeBlock.language == ProtoCore.Language.kInvalid)
                ProcessImperativeNode(node.CodeBlockNode, fragments);
        }
        void ProcessImperativeTypeNode(DesignScript.Parser.Imperative.TypeNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeIDEHelpNode(node.colon, fragments);

            // Modified by Jiong -- Begin
            //ProcessAssociativeIDEHelpNode(node.name, fragments);
            ProcessImperativeIDEHelpNode(node.typeName, fragments);
            // Modified by Jiong -- End
            if (node.brackets != null)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode n in node.brackets)
                    ProcessImperativeIDEHelpNode(n, fragments);
            }
            ProcessImperativeIDEHelpNode(node.op, fragments);
            if (node.multiDim != null)
            {

                foreach (DesignScript.Parser.Imperative.IDEHelpNode n in node.multiDim)
                    ProcessImperativeIDEHelpNode(n, fragments);
            }
        }
        void ProcessImperativeIdentifierNode(DesignScript.Parser.Imperative.IdentifierNode node, List<CodeFragment> fragments)
        {
            // Modified by Mark -- Begin
            if ((node.typeName.Value != null) || (node.typeName_kw.Value != null))
            {
                if (node.typeName_kw.Value != null) ProcessImperativeKeywordNode(node.typeName_kw, fragments);
                else ProcessImperativeTypeNode(node.typeName, fragments);
                ProcessImperativePunctuationNode(node.colon, fragments);
            }
            // Modified by Mark -- End
            if (null == node)
                return;
            ProcessImperativeArrayNode(node.ArrayDimensions, fragments);
            // Modified by Jiong -- Begin
            // fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.UserType, node.Line - 1, node.Col - 1));
            if (node.Value != null)
                fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Local, node.Line - 1, node.Col - 1));
            else
                ProcessImperativeIDEHelpNode(node.Return, fragments);
            // Modified by Jiong -- End
        }
        void ProcessImperativeIntNode(DesignScript.Parser.Imperative.IntNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;

            ProcessImperativeIDEHelpNode(node.SignPos, fragments);
            ProcessImperativeIDEHelpNode(node.IDEValue, fragments);
        }
        void ProcessImperativeDoubleNode(DesignScript.Parser.Imperative.DoubleNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;

            ProcessImperativeIDEHelpNode(node.SignPos, fragments);
            ProcessImperativeIDEHelpNode(node.IDEValue, fragments);
        }
        void ProcessImperativeBooleanNode(DesignScript.Parser.Imperative.BooleanNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            fragments.Add(new CodeFragment(node.value, CodeFragment.Type.Keyword, node.Line - 1, node.Col - 1));
        }
        void ProcessImperativeCharNode(DesignScript.Parser.Imperative.CharNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            fragments.Add(new CodeFragment(node.value, CodeFragment.Type.Text, node.Line - 1, node.Col - 1));
        }
        void ProcessImperativeStringNode(DesignScript.Parser.Imperative.StringNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            fragments.Add(new CodeFragment(node.value, CodeFragment.Type.Text, node.Line - 1, node.Col - 1));
        }
        void ProcessImperativeNullNode(DesignScript.Parser.Imperative.NullNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            fragments.Add(new CodeFragment("null", CodeFragment.Type.Keyword, node.Line - 1, node.Col - 1));
        }
        void ProcessImperativeArrayNode(DesignScript.Parser.Imperative.ArrayNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;

            ProcessImperativeIDEHelpNode(node.OpenBracketPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseBracketPos, fragments);
            ProcessImperativeNode(node.Ident, fragments);
            ProcessImperativeNode(node.Expr, fragments);
            ProcessImperativeNode(node.Type, fragments);

        }
        void ProcessImperativeFunctionCallNode(DesignScript.Parser.Imperative.FunctionCallNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            fragments.Add(new CodeFragment(((DesignScript.Parser.Imperative.IdentifierNode)node.Function).Value, CodeFragment.Type.Function, ((DesignScript.Parser.Imperative.IdentifierNode)node.Function).Line - 1, ((DesignScript.Parser.Imperative.IdentifierNode)node.Function).Col - 1));
            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.EndlinePos, fragments);

            if (node.FormalArguments.Count != 0)
            {
                foreach (Node n in node.FormalArguments)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }
            if (node.ArgCommaPosList.Count != 0)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode n in node.ArgCommaPosList)
                {
                    ProcessImperativeIDEHelpNode(n, fragments);
                }
            }

            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);

        }
        void ProcessImperativeVarDeclNode(DesignScript.Parser.Imperative.VarDeclNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;

            ProcessImperativeIDEHelpNode(node.KeywordPos, fragments);
            ProcessImperativeIDEHelpNode(node.ArgumentTypePos, fragments);
            ProcessImperativeIDEHelpNode(node.TypeColonPos, fragments);
            // ProcessImperativeIDEHelpNode(node.EndlinePos, fragments);
            ProcessImperativeNode(node.NameNode, fragments);
            ProcessImperativeNode(node.name, fragments);
            ProcessImperativeTypeNode(node.IDEArgumentType, fragments);
            ProcessImperativeNode(node.equal, fragments);
            // Modified by Jiong -- Begin
            if (node.Brackets != null)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode idenode in node.Brackets)
                {
                    ProcessImperativeIDEHelpNode(idenode, fragments);
                }
            }// Modified by Jiong -- End

        }

        void ProcessImperativeArgumentSignatureNode(DesignScript.Parser.Imperative.ArgumentSignatureNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            //fragments.Add(new CodeFragment(node.Name, CodeFragment.Type.Keyword, node.Line, node.Col));
            foreach (DesignScript.Parser.Imperative.VarDeclNode varNode in node.Arguments)
            {
                ProcessImperativeVarDeclNode(varNode, fragments);
            }
        }
        void ProcessImperativeExprListNode(DesignScript.Parser.Imperative.ExprListNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeIDEHelpNode(node.OpenCurlyBracePos, fragments);

            if (node.list.Count != 0)
            {
                foreach (Node n in node.list)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }

            if (node.ExprCommaPosList.Count != 0)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode n in node.ExprCommaPosList)
                {
                    ProcessImperativeIDEHelpNode(n, fragments);
                }
            }

            ProcessImperativeIDEHelpNode(node.CloseCurlyBracePos, fragments);
        }
        void ProcessImperativeCodeBlockNode(DesignScript.Parser.Imperative.CodeBlockNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            if (node.Body.Count != 0)
            {
                foreach (Node n in node.Body)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }

        }

        void ProcessImperativeFunctionDefinitionNode(DesignScript.Parser.Imperative.FunctionDefinitionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            //Process local vars
            ProcessImperativeIDEHelpNode(node.OpenCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.KeywordPos, fragments);
            functionTable.Add(node.NamePos.Value);
            fragments.Add(new CodeFragment(node.NamePos.Value, CodeFragment.Type.Function, node.NamePos.Line - 1, node.NamePos.Col - 1));
            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.TypeColonPos, fragments);
            ProcessImperativeIDEHelpNode(node.ReturnTypePos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);

            if (node.ArgCommaPosList.Count != 0)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode n in node.ArgCommaPosList)
                {
                    ProcessImperativeIDEHelpNode(n, fragments);
                }
            }

            ProcessImperativeCodeBlockNode(node.FunctionBody, fragments);
            ProcessImperativeArgumentSignatureNode(node.Signature, fragments);

            if (node.Brackets != null)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode iden in node.Brackets)
                {
                    ProcessImperativeIDEHelpNode(iden, fragments);
                }
            }

        }
        void ProcessImperativeBinaryExpressionNode(DesignScript.Parser.Imperative.BinaryExpressionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;

            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.EndlinePos, fragments);
            ProcessImperativeIDEHelpNode(node.OperatorPos, fragments);
            ProcessImperativeNode(node.LeftNode, fragments);
            ProcessImperativeNode(node.RightNode, fragments);
        }
        void ProcessImperativeElseIfBlock(DesignScript.Parser.Imperative.ElseIfBlock node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeNode(node.Expr, fragments);
            ProcessImperativeIDEHelpNode(node.OpenCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.KeywordPos, fragments);

            if (node.Body.Count != 0)
            {
                foreach (Node n in node.Body)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }
        }
        void ProcessImperativeIfStmtNode(DesignScript.Parser.Imperative.IfStmtNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeNode(node.IfExprNode, fragments);
            ProcessImperativeIDEHelpNode(node.KeywordPos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.ElseOpenCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.ElsePos, fragments);
            ProcessImperativeIDEHelpNode(node.ElseCloseCurlyBracePos, fragments);

            if (node.IfBody.Count != 0)
            {
                foreach (Node n in node.IfBody)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }

            if (node.ElseIfList.Count != 0)
            {
                foreach (DesignScript.Parser.Imperative.ElseIfBlock n in node.ElseIfList)
                {
                    ProcessImperativeElseIfBlock(n, fragments);
                }
            }

            if (node.ElseBody.Count != 0)
            {
                foreach (Node n in node.ElseBody)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }
        }
        void ProcessImperativeWhileStmtNode(DesignScript.Parser.Imperative.WhileStmtNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            ProcessImperativeNode(node.Expr, fragments);
            ProcessImperativeIDEHelpNode(node.KeywordPos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseCurlyBracePos, fragments);

            if (node.Body.Count != 0)
            {
                foreach (Node n in node.Body)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }
        }

        // Modified by Jiong -- Begin
        void ProcessImperativeInlineConditionalNode(DesignScript.Parser.Imperative.InlineConditionalNode node, List<CodeFragment> fragements)
        {
            if (node == null)
                return;

            ProcessImperativeNode(node.ConditionExpression, fragements);
            ProcessImperativeNode(node.TrueExpression, fragements);
            ProcessImperativeNode(node.FalseExpression, fragements);
            ProcessImperativeIDEHelpNode(node.QuestionPos, fragements);
            ProcessImperativeIDEHelpNode(node.ColonPos, fragements);
        }

        // Modified by Jiong -- End

        void ProcessImperativeUnaryExpressionNode(DesignScript.Parser.Imperative.UnaryExpressionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;

            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.OperatorPos, fragments);
            ProcessImperativeIDEHelpNode(node.Operator, fragments);
            ProcessImperativeNode(node.Expression, fragments);
        }
        void ProcessImperativeRangeExprNode(DesignScript.Parser.Imperative.RangeExprNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeNode(node.FromNode, fragments);
            ProcessImperativeNode(node.ToNode, fragments);
            ProcessImperativeNode(node.StepNode, fragments);
            ProcessImperativeIDEHelpNode(node.FirstRangeOpPos, fragments);
            ProcessImperativeIDEHelpNode(node.SecondRangeOpPos, fragments);
            ProcessImperativeIDEHelpNode(node.StepOpPos, fragments);
        }
        void ProcessImperativeForLoopNode(DesignScript.Parser.Imperative.ForLoopNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            ProcessImperativeNode(node.id, fragments);
            ProcessImperativeNode(node.expression, fragments);
            ProcessImperativeIDEHelpNode(node.KeywordPos, fragments);
            ProcessImperativeIDEHelpNode(node.KwInPos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenCurlyBracePos, fragments);
            ProcessImperativeIDEHelpNode(node.OpenParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseParenPos, fragments);
            ProcessImperativeIDEHelpNode(node.CloseCurlyBracePos, fragments);

            if (node.body.Count != 0)
            {
                foreach (Node n in node.body)
                {
                    ProcessImperativeNode(n, fragments);
                }
            }
        }
        void ProcessImperativeIdentifierListNode(DesignScript.Parser.Imperative.IdentifierListNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            if (node.DotPosList.Count != 0)
            {
                foreach (DesignScript.Parser.Imperative.IDEHelpNode n in node.DotPosList)
                {
                    ProcessImperativeIDEHelpNode(n, fragments);
                }
            }
            ProcessImperativeNode(node.LeftNode, fragments);
            //Process Operator
            ProcessImperativeNode(node.RightNode, fragments);
        }

        void ProcessImperativeIDEHelpNode(DesignScript.Parser.Imperative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (null == node)
                return;
            switch (node.Type)
            {
                case DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.IdentNode:
                    ProcessImperativeIdentNode(node, fragments);
                    break;
                case DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.KeywordNode:
                    ProcessImperativeKeywordNode(node, fragments);
                    break;
                case DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode:
                    ProcessImperativePunctuationNode(node, fragments);
                    break;
                case DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.TypeNode:
                    ProcessImperativeTypeNode(node, fragments);
                    break;
                case DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.NumberNode:
                    ProcessImperativeNumberNode(node, fragments);
                    break;
                default:
                    break;
            }
        }
        void ProcessImperativePunctuationNode(DesignScript.Parser.Imperative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Punctuation, node.Line - 1, node.Col - 1));
        }
        void ProcessImperativeTypeNode(DesignScript.Parser.Imperative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.UserType, node.Line - 1, node.Col - 1));
        }
        void ProcessImperativeKeywordNode(DesignScript.Parser.Imperative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Keyword, node.Line - 1, node.Col - 1));
        }
        void ProcessImperativeIdentNode(DesignScript.Parser.Imperative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Local, node.Line - 1, node.Col - 1));
        }

        void ProcessImperativeNumberNode(DesignScript.Parser.Imperative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Number, node.Line - 1, node.Col - 1));
        }
        // ProcessImperativeParenExpressionNode, __Fuqiang
        void ProcessImperativeParenExpressionNode(DesignScript.Parser.Imperative.ParenExpressionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeIDEHelpNode(node.openParen, fragments);
            ProcessImperativeIDEHelpNode(node.closeParen, fragments);
            ProcessNode(node.expression, fragments);
        }

        // Modified by Jiong -- Begin
        void ProcessImperativePostFixNode(DesignScript.Parser.Imperative.PostFixNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessImperativeIDEHelpNode(node.OperatorPos, fragments);
            ProcessImperativeNode(node.Identifier, fragments);
        }
        // Modified by Jiong -- End
        void ProcessImperativeClassDeclNode(DesignScript.Parser.Imperative.ClassDeclNode node, List<CodeFragment> fragments)
        {
            fragments.Add(new CodeFragment(node.name, CodeFragment.Type.Keyword, node.Line, node.Col));

            if (node.varlist.Count != 0)
            {
                foreach (Node varNode in node.varlist)
                {
                    ProcessImperativeNode(varNode, fragments);
                }
            }

            if (node.funclist.Count != 0)
            {
                foreach (Node funcNode in node.funclist)
                {
                    ProcessImperativeNode(funcNode, fragments);
                }
            }

        }

        #endregion

        #region Associative

        void ProcessAssociativeNode(Node node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            switch (node.GetType().ToString())
            {
                case "DesignScript.Parser.Associative.LanguageBlockNode":
                    ProcessAssociativeLanguageBlockNode(node as DesignScript.Parser.Associative.LanguageBlockNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.IdentifierNode":
                    ProcessAssociativeIdentifierNode(node as DesignScript.Parser.Associative.IdentifierNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ReplicationGuideNode":
                    ProcessAssociativeReplicationGuideNode(node as DesignScript.Parser.Associative.ReplicationGuideNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.IntNode":
                    ProcessAssociativeIntNode(node as DesignScript.Parser.Associative.IntNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.DoubleNode":
                    ProcessAssociativeDoubleNode(node as DesignScript.Parser.Associative.DoubleNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.BooleanNode":
                    ProcessAssociativeBooleanNode(node as DesignScript.Parser.Associative.BooleanNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.CharNode":
                    ProcessAssociativeCharNode(node as DesignScript.Parser.Associative.CharNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.StringNode":
                    ProcessAssociativeStringNode(node as DesignScript.Parser.Associative.StringNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.NullNode":
                    ProcessAssociativeNullNode(node as DesignScript.Parser.Associative.NullNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ArrayNode":
                    ProcessAssociativeArrayNode(node as DesignScript.Parser.Associative.ArrayNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.FunctionCallNode":
                    ProcessAssociativeFunctionCallNode(node as DesignScript.Parser.Associative.FunctionCallNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.VarDeclNode":
                    ProcessAssociativeVarDeclNode(node as DesignScript.Parser.Associative.VarDeclNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ReturnNode":
                    ProcessAssociativeReturnNode(node as DesignScript.Parser.Associative.ReturnNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ArgumentSignatureNode":
                    ProcessAssociativeArgumentSignatureNode(node as DesignScript.Parser.Associative.ArgumentSignatureNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ExprListNode":
                    ProcessAssociativeExprListNode(node as DesignScript.Parser.Associative.ExprListNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.CodeBlockNode":
                    ProcessAssociativeCodeBlockNode(node as DesignScript.Parser.Associative.CodeBlockNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.FunctionDefinitionNode":
                    ProcessAssociativeFunctionDefinitionNode(node as DesignScript.Parser.Associative.FunctionDefinitionNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.BinaryExpressionNode":
                    ProcessAssociativeBinaryExpressionNode(node as DesignScript.Parser.Associative.BinaryExpressionNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.UnaryExpressionNode":
                    ProcessAssociativeUnaryExpressionNode(node as DesignScript.Parser.Associative.UnaryExpressionNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.RangeExprNode":
                    ProcessAssociativeRangeExprNode(node as DesignScript.Parser.Associative.RangeExprNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ForLoopNode":
                    ProcessAssociativeForLoopNode(node as DesignScript.Parser.Associative.ForLoopNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.IdentifierListNode":
                    ProcessAssociativeIdentifierListNode(node as DesignScript.Parser.Associative.IdentifierListNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.IDEHelpNode":
                    ProcessAssociativeIDEHelpNode(node as DesignScript.Parser.Associative.IDEHelpNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.StatementNode":
                    ProcessAssociativeStatementNode(node as DesignScript.Parser.Associative.StatementNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ClassDeclNode":
                    ProcessAssociativeClassDeclNode(node as DesignScript.Parser.Associative.ClassDeclNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ConstructorDefinitionNode":
                    ProcessAssociativeConstructorDefinitionNode(node as DesignScript.Parser.Associative.ConstructorDefinitionNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ModifierStackNode":
                    ProcessAssociativeModifierStackNode(node as DesignScript.Parser.Associative.ModifierStackNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ParenExpressionNode":
                    ProcessAssociativeParenExpressionNode(node as DesignScript.Parser.Associative.ParenExpressionNode, fragments);
                    break;
                // Modified by Jiong -- Begin
                case "DesignScript.Parser.Associative.PostFixNode":
                    ProcessAssociativePostFixNode(node as DesignScript.Parser.Associative.PostFixNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.InlineConditionalNode":
                    ProcessAssociativeInlineConditionalNode(node as DesignScript.Parser.Associative.InlineConditionalNode, fragments);
                    break;
                case "DesignScript.Parser.Associative.ImportNode":
                    ProcessAssociativeImportNode(node as DesignScript.Parser.Associative.ImportNode, fragments);
                    break;
                
                // Modified by Jiong -- End
                default:
                    break;
            }
        }

        // Modified by Jiong -- Begin

        private void ProcessAssociativeImportNode(DesignScript.Parser.Associative.ImportNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;

            ProcessAssociativeIDEHelpNode(node.KwImport, fragments);
            ProcessAssociativeIDEHelpNode(node.KwFrom, fragments);
            ProcessAssociativeIDEHelpNode(node.KwPrefix, fragments);
            ProcessAssociativeIDEHelpNode(node.OpenParen, fragments);
            ProcessAssociativeIDEHelpNode(node.CloseParen, fragments);
            ProcessAssociativeIDEHelpNode(node.Identifier, fragments);
            ProcessAssociativeIDEHelpNode(node.PrefixIdent, fragments);
            ProcessAssociativeIDEHelpNode(node.EndLine, fragments);
            ProcessAssociativeStringNode(node.Path, fragments);
        }

        // Modified by Jiong -- End

        void ProcessAssociativeLanguageBlockNode(DesignScript.Parser.Associative.LanguageBlockNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.openBracket, fragments);
            ProcessAssociativeIDEHelpNode(node.language, fragments);
            ProcessAssociativeIDEHelpNode(node.closeBracket, fragments);
            ProcessAssociativeIDEHelpNode(node.openBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.closeBrace, fragments);

            // Modified By Jiong -- Begin

            //if (node.Mixed_property != null)
            //{
            //    foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.Mixed_property)
            //        ProcessAssociativeIDEHelpNode(n, fragments);
            //}

            if (node.property != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode property in node.property)
                {
                    ProcessAssociativeIDEHelpNode(property, fragments);
                }
            }

            if (node.propertyValue != null)
            {
                foreach (DesignScript.Parser.Associative.StringNode sn in node.propertyValue)
                {
                    ProcessAssociativeStringNode(sn, fragments);
                }
            }

            // Modified By Jiong -- End
            if (node.languageblock.language == ProtoCore.Language.kAssociative || node.languageblock.language == ProtoCore.Language.kInvalid)
                ProcessAssociativeNode(node.code, fragments);
            else if (node.languageblock.language == ProtoCore.Language.kImperative)
                ProcessImperativeNode(node.code, fragments);
        }

        void ProcessAssociativeMergeNode(DesignScript.Parser.Associative.MergeNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.MergedNodes, fragments);
        }

        void ProcessAssociativeParenExpressionNode(DesignScript.Parser.Associative.ParenExpressionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.openParen, fragments);
            ProcessAssociativeIDEHelpNode(node.closeParen, fragments);
            ProcessNode(node.expression, fragments);
        }

        void ProcessAssociativeIdentifierNode(DesignScript.Parser.Associative.IdentifierNode node, List<CodeFragment> fragments)
        {

            // Modified by Mark -- Begin
            if ((node.typeName.Value != null)||(node.typeName_kw.Value != null))
            {
                if (node.typeName_kw.Value != null) ProcessAssociativeKeywordNode(node.typeName_kw, fragments);
                else ProcessAssociativeTypeNode(node.typeName, fragments);                
                ProcessAssociativePunctuationNode(node.colon, fragments);
            }
            // Modified by Mark -- End
            if (node == null)
                return;
            ProcessAssociativeArrayNode(node.ArrayDimensions, fragments);

            //if (node.brackets != null)
            //{
            //    foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.brackets)
            //        ProcessAssociativeIDEHelpNode(n, fragments);
            //
            //}
            //
            //if (node.ReplicationGuides != null)
            //{
            //
            //    ProcessNode(node.ReplicationGuides, fragments);
            //}

            // Modified by Jiong -- Begin
            // fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.UserType, node.Line - 1, node.Col - 1));
            if (node.IdentValue != null && node.IdentValue.Value != null)
                ProcessAssociativeIDEHelpNode(node.IdentValue, fragments);
            else
                ProcessAssociativeIDEHelpNode(node.IdentValueReturn, fragments);
            // Modified by Jiong -- End
        }

        void ProcessAssociativeReplicationGuideNode(DesignScript.Parser.Associative.ReplicationGuideNode node, List<CodeFragment> fragments)
        {

            if (node == null)
                return;
            
            if (node.brackets != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.brackets)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            
            }
            
            if (node.ReplicationGuides != null)
            {
            
                ProcessNode(node.ReplicationGuides, fragments);
            }

            ProcessNode(node.factor, fragments);
        }
        
        void ProcessAssociativeIdentifierListNode(DesignScript.Parser.Associative.IdentifierListNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.LeftNode, fragments);
            ProcessNode(node.RightNode, fragments);
            ProcessAssociativeIDEHelpNode(node.dot, fragments);
        }

        void ProcessAssociativeIntNode(DesignScript.Parser.Associative.IntNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.sign, fragments);
            ProcessAssociativeIDEHelpNode(node.IDEValue, fragments);
        }

        void ProcessAssociativeDoubleNode(DesignScript.Parser.Associative.DoubleNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.sign, fragments);
            ProcessAssociativeIDEHelpNode(node.IDEValue, fragments);
        }

        void ProcessAssociativeBooleanNode(DesignScript.Parser.Associative.BooleanNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            fragments.Add(new CodeFragment(node.value, CodeFragment.Type.Keyword, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeCharNode(DesignScript.Parser.Associative.CharNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            fragments.Add(new CodeFragment(node.value, CodeFragment.Type.Text, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeStringNode(DesignScript.Parser.Associative.StringNode node, List<CodeFragment> fragments)
        {
            if (node == null || (null == node.value))
                return;
            fragments.Add(new CodeFragment(node.value, CodeFragment.Type.Text, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeNullNode(DesignScript.Parser.Associative.NullNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            fragments.Add(new CodeFragment(node.value, CodeFragment.Type.Keyword, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeReturnNode(DesignScript.Parser.Associative.ReturnNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.ReturnExpr, fragments);
        }

        void ProcessAssociativeFunctionCallNode(DesignScript.Parser.Associative.FunctionCallNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;

            //ProcessNode(node.Function, fragments);
            DesignScript.Parser.Associative.IdentifierNode identNode = (DesignScript.Parser.Associative.IdentifierNode)node.Function;
            if (null == identNode || (null == identNode.IdentValue))
                return; // Typing "return (" would cause this.
            if (string.IsNullOrEmpty(identNode.IdentValue.Value))
                return; // Typing "return (" would cause this.

            fragments.Add(new CodeFragment(identNode.IdentValue.Value, CodeFragment.Type.Function,
                identNode.IdentValue.Line - 1, identNode.IdentValue.Col - 1));
            if (node.FormalArguments != null)
            {
                foreach (Node n in node.FormalArguments)
                {
                    ProcessNode(n, fragments);
                }
            }
            if (node.comma != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.comma)
                {
                    ProcessAssociativeIDEHelpNode(n, fragments);
                }
            }
            ProcessAssociativeIDEHelpNode(node.openParen, fragments);
            ProcessAssociativeIDEHelpNode(node.closeParen, fragments);
        }

        void ProcessAssociativePatternNode(DesignScript.Parser.Associative.Pattern node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.Expression, fragments);
            ProcessAssociativeIDEHelpNode(node.bitOr, fragments);
        }

        void ProcessAssociativeQualifiedNode(DesignScript.Parser.Associative.QualifiedNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.Value, fragments);
            if (node.ReplicationGuides != null)
            {
                foreach (Node n in node.ReplicationGuides)
                    ProcessNode(n, fragments);
            }
        }

        void ProcessAssociativeVarDeclNode(DesignScript.Parser.Associative.VarDeclNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;

            ProcessNode(node.NameNode, fragments);
            ProcessAssociativeIDEHelpNode(node.KwStatic, fragments);
            ProcessAssociativeIDEHelpNode(node.name, fragments);
            ProcessAssociativeTypeNode(node.IDEArgumentType, fragments);
            ProcessAssociativeIDEHelpNode(node.equal, fragments);
        }

        void ProcessAssociativeArgumentSignatureNode(DesignScript.Parser.Associative.ArgumentSignatureNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            if (node.Arguments != null)
            {
                foreach (DesignScript.Parser.Associative.VarDeclNode n in node.Arguments)
                    ProcessAssociativeVarDeclNode(n, fragments);
            }
            ProcessAssociativeIDEHelpNode(node.closeBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.openBrace, fragments);

            if (node.comma != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.comma)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }

        }

        void ProcessAssociativeCodeBlockNode(DesignScript.Parser.Associative.CodeBlockNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.closeBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.openBrace, fragments);

            if (node.Body != null)
            {
                foreach (Node n in node.Body)
                    ProcessNode(n, fragments);
            }
        }

        void ProcessAssociativeClassDeclNode(DesignScript.Parser.Associative.ClassDeclNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.openBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwextend, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwclass, fragments);
            ProcessAssociativeIDEHelpNode(node.closeBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.ClassName, fragments);
            //ProcessAssociativeIDEHelpNode(node., fragments);

            if (node.SuperClass != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.SuperClass)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }
            if (node.funclist != null)
            {
                foreach (Node n in node.funclist)
                    ProcessNode(n, fragments);
            }
            if (node.varlist != null)
            {
                foreach (Node n in node.varlist)
                    ProcessNode(n, fragments);
            }
            if (node.VarDeclCommas != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.VarDeclCommas)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }

            // Modified by Jiong -- Begin
            if (node.accessLabel != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode accesslabel in node.accessLabel)
                {
                    ProcessAssociativeIDEHelpNode(accesslabel, fragments);
                }
            }
            // Modified by Jiong -- End
        }

        void ProcessAssociativeConstructorDefinitionNode(DesignScript.Parser.Associative.ConstructorDefinitionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.KwStatic, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwconstructor, fragments);
            ProcessAssociativeIDEHelpNode(node.name, fragments);
            ProcessAssociativeIDEHelpNode(node.Dot, fragments);
            ProcessAssociativeIDEHelpNode(node.Colon, fragments);
            ProcessAssociativeIDEHelpNode(node.KwBase, fragments);
            ProcessAssociativeFunctionCallNode(node.BaseConstructorNode, fragments);
            ProcessAssociativeCodeBlockNode(node.FunctionBody, fragments);
            ProcessAssociativeArgumentSignatureNode(node.Signature, fragments);
            ProcessAssociativeTypeNode(node.IDEReturnType, fragments);
            ProcessNode(node.Pattern, fragments);
        }

        void ProcessAssociativeTypeNode(DesignScript.Parser.Associative.TypeNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.colon, fragments);

            // Modified by Jiong -- Begin
            //ProcessAssociativeIDEHelpNode(node.name, fragments);
            ProcessAssociativeIDEHelpNode(node.typeName, fragments);
            // Modified by Jiong -- End
            if (node.brackets != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.brackets)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }
            ProcessAssociativeIDEHelpNode(node.op, fragments);
            if (node.multiDim != null)
            {
                
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.multiDim)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }
        }

        void ProcessAssociativeFunctionDefinitionNode(DesignScript.Parser.Associative.FunctionDefinitionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.KwStatic, fragments);
            ProcessAssociativeIDEHelpNode(node.endLine, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwdef, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwexternal, fragments);
            ProcessAssociativeIDEHelpNode(node.libOpenBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.libOpenQuote, fragments);
            ProcessAssociativeIDEHelpNode(node.libCloseQuote, fragments);
            // Modified by Jiong -- Begin
            // ProcessAssociativeIDEHelpNode(node.libName, fragments);
            ProcessAssociativeStringNode(node.libName, fragments);
            // Modified by Jiong -- End
            ProcessAssociativeIDEHelpNode(node.libCloseBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwnative, fragments);
            //ProcessAssociativeIDEHelpNode(node.name, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwnative, fragments);

            DesignScript.Parser.Associative.IDEHelpNode functionNameNode = (DesignScript.Parser.Associative.IDEHelpNode)node.name;

            if (functionNameNode.Value != null)
                fragments.Add(new CodeFragment(functionNameNode.Value, CodeFragment.Type.Function, functionNameNode.Line - 1, functionNameNode.Col - 1));

            /*fragments.Add(new CodeFragment(((DesignScript.Parser.Associative.IDEHelpNode)node.name).Value,
                CodeFragment.Type.Function, ((DesignScript.Parser.Associative.IDEHelpNode)node.name).Line - 1,
                ((DesignScript.Parser.Associative.IDEHelpNode)node.name).Col - 1));*/
            ProcessAssociativeIDEHelpNode(node.endLine, fragments);
            ProcessAssociativeCodeBlockNode(node.FunctionBody, fragments);
            ProcessAssociativeArgumentSignatureNode(node.Singnature, fragments);
            ProcessAssociativeTypeNode(node.IDEReturnType, fragments);
            ProcessNode(node.Pattern, fragments);


        }

        void ProcessAssociativeBinaryExpressionNode(DesignScript.Parser.Associative.BinaryExpressionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.LeftNode, fragments);
            ProcessNode(node.RightNode, fragments);
            ProcessAssociativeIDEHelpNode(node.op, fragments);
        }

        void ProcessAssociativeUnaryExpressionNode(DesignScript.Parser.Associative.UnaryExpressionNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.Expression, fragments);
            ProcessAssociativeIDEHelpNode(node.op, fragments);
        }

        void ProcessAssociativeModifierStackNode(DesignScript.Parser.Associative.ModifierStackNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.closeBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.openSharpBrace, fragments);
            if (node.arrow != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.arrow)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }
            if (node.endline != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.endline)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }

            ProcessAssociativeNode(node.ReturnNode, fragments);

            if (node.ElementNodes != null)
            {
                foreach (Node n in node.ElementNodes)
                    ProcessNode(n, fragments);
            }
        }

        void ProcessAssociativeRangeExprNode(DesignScript.Parser.Associative.RangeExprNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.op, fragments);
            ProcessAssociativeIDEHelpNode(node.stepOp, fragments);
            ProcessAssociativeIDEHelpNode(node.stepOp2, fragments);
            ProcessNode(node.ToNode, fragments);
            ProcessNode(node.StepNode, fragments);
            ProcessNode(node.FromNode, fragments);
        }

        void ProcessAssociativeExprListNode(DesignScript.Parser.Associative.ExprListNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            if (node.comma != null)
            {
                foreach (DesignScript.Parser.Associative.IDEHelpNode n in node.comma)
                    ProcessAssociativeIDEHelpNode(n, fragments);
            }

            if (node.list != null)
            {
                foreach (Node n in node.list)
                    ProcessNode(n, fragments);
            }

            ProcessAssociativeIDEHelpNode(node.openBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.closeBrace, fragments);
        }

        void ProcessAssociativeForLoopNode(DesignScript.Parser.Associative.ForLoopNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.closeBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.closeParen, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwfor, fragments);
            ProcessAssociativeIDEHelpNode(node.Kwin, fragments);
            ProcessAssociativeIDEHelpNode(node.openBrace, fragments);
            ProcessAssociativeIDEHelpNode(node.openParen, fragments);

            if (node.body != null)
            {
                foreach (Node n in node.body)
                    ProcessNode(n, fragments);
            }

            ProcessNode(node.expression, fragments);
            ProcessNode(node.id, fragments);
        }

        void ProcessAssociativeStatementNode(DesignScript.Parser.Associative.StatementNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeIDEHelpNode(node.endLine, fragments);
            ProcessNode(node.Statement, fragments);
        }

        void ProcessAssociativeArrayNode(DesignScript.Parser.Associative.ArrayNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessNode(node.Expr, fragments);
            ProcessNode(node.Type, fragments);

            ProcessNode(node.Ident, fragments);
            ProcessAssociativeIDEHelpNode(node.openBracket, fragments);
            ProcessAssociativeIDEHelpNode(node.closeBracket, fragments);
        }

        // Modified by Jiong -- Begin
        void ProcessAssociativePostFixNode(DesignScript.Parser.Associative.PostFixNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;

            ProcessAssociativeIDEHelpNode(node.OperatorPos, fragments);
            ProcessAssociativeNode(node.Identifier, fragments);
        }

        void ProcessAssociativeInlineConditionalNode(DesignScript.Parser.Associative.InlineConditionalNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            ProcessAssociativeNode(node.ConditionExpression, fragments);
            ProcessAssociativeNode(node.TrueExpression, fragments);
            ProcessAssociativeNode(node.FalseExpression, fragments);
            ProcessAssociativeIDEHelpNode(node.Question, fragments);
            ProcessAssociativeIDEHelpNode(node.Colon, fragments);
        }

        // Modified by Jiong -- End

        void ProcessAssociativeIDEHelpNode(DesignScript.Parser.Associative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node == null)
                return;
            switch (node.Type)
            {
                case DesignScript.Parser.Associative.IDEHelpNode.NodeType.IdentNode:
                    ProcessAssociativeIdentNode(node, fragments);
                    break;
                case DesignScript.Parser.Associative.IDEHelpNode.NodeType.KeywordNode:
                    ProcessAssociativeKeywordNode(node, fragments);
                    break;
                case DesignScript.Parser.Associative.IDEHelpNode.NodeType.PunctuationNode:
                    ProcessAssociativePunctuationNode(node, fragments);
                    break;
                case DesignScript.Parser.Associative.IDEHelpNode.NodeType.TypeNode:
                    ProcessAssociativeTypeNode(node, fragments);
                    break;
                case DesignScript.Parser.Associative.IDEHelpNode.NodeType.NumberNode:
                    ProcessAssociativeNumberNode(node, fragments);
                    break;
                default:
                    break;
            }
        }

        void ProcessAssociativePunctuationNode(DesignScript.Parser.Associative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Punctuation, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeTypeNode(DesignScript.Parser.Associative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.UserType, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeKeywordNode(DesignScript.Parser.Associative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Keyword, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeIdentNode(DesignScript.Parser.Associative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Local, node.Line - 1, node.Col - 1));
        }

        void ProcessAssociativeNumberNode(DesignScript.Parser.Associative.IDEHelpNode node, List<CodeFragment> fragments)
        {
            if (node.Value == null)
                return;
            fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Number, node.Line - 1, node.Col - 1));
        }

        #endregion Associative

        void ProcessCommentNode(CommentNode node, List<CodeFragment> fragments)
        {
            // Modified By Jiong -- Begin
            if (node == null)
                return;
            if (node.Type == CommentNode.CommentType.Inline)
            {
                fragments.Add(new CodeFragment(node.Value, CodeFragment.Type.Comment, node.Line - 1, node.Col - 1));
            }
            else
            {
                if (node == null || node.Value == null)
                    return;

                int curLine = node.Line - 1;
                int curCol = node.Col - 1;

                for (int ix = 0; ix < node.Value.Length; ++ix)
                {
                    if (node.Value[ix] == '\n')
                    {
                        ++curLine;
                        curCol = 0;
                    }
                    else if (node.Value[ix] == '\t')
                    {
                        ++curCol;
                    }
                    else if (node.Value[ix] >= 32)
                    {
                        int tempCol = curCol;
                        string str = new string(node.Value[ix], 1);
                        while (++ix < node.Value.Length && node.Value[ix] >= 32)
                        {
                            str += node.Value[ix];
                            ++curCol;
                        }
                        --ix;
                        fragments.Add(new CodeFragment(str, CodeFragment.Type.Comment, curLine, tempCol));
                    }
                }
            }
            //char[] charArray = new char[node.Value.Length];
            //charArray = node.Value.ToCharArray();
            //List<char> charList = new List<char>();
            //int line = node.Line - 1;
            //int col = node.Col - 1;
            //int tempCol = 0;
            //foreach (char c in charArray)
            //{
            //    if (col != 0 || charList.Count != 0)
            //    {
            //        if (c == '\n' || c == '/')
            //        {
            //            fragments.Add(new CodeFragment(charList.ToString(), CodeFragment.Type.Comment, line, col));
            //            line++; col = 0;
            //            charList = new List<char>();
            //        }
            //        else if (c != '\r' && c != '\n')
            //        {
            //            charList.Add(c);
            //        }
            //    }
            //    else
            //    {
            //        if (c == '\t')
            //            tempCol++;
            //        else if (c != '\t' && c != '\n' && c != '\r' && c != ' ')
            //            col = tempCol;
            //        else
            //            charList.Add(c);
            //    }


            //}

            // Modified By Jiong -- End
        }

    }
}

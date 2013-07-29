
using System;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoCore;

namespace DesignScript.Parser.Imperative
{
    public abstract class ImperativeNode : DesignScript.Parser.Node
    {
    }

    public class LanguageBlockNode : ImperativeNode
    {
        public IDEHelpNode OpenParenPos;
        public IDEHelpNode CloseParenPos;
        public IDEHelpNode OpenCurlyBracePos;
        public IDEHelpNode CloseCurlyBracePos;
        public IDEHelpNode IdentPos;
        public List<IDEHelpNode> ParaPosList;
        public List<StringNode> TextStringList;
        public LanguageBlockNode()
        {
            CodeBlock = new ProtoCore.LanguageCodeBlock();
            OpenParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            OpenCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            IdentPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.KeywordNode };
            ParaPosList = new List<IDEHelpNode>();
            TextStringList = new List<StringNode>();
        }
        public ProtoCore.LanguageCodeBlock CodeBlock { get; set; }
        public Node CodeBlockNode { get; set; }
    }

    public class IntNode : ImperativeNode
    {
        public IntNode()
        {
            SignPos = new IDEHelpNode(IDEHelpNode.HelpNodeType.NumberNode);
            IDEValue = new IDEHelpNode(IDEHelpNode.HelpNodeType.NumberNode);
        }

        public IDEHelpNode SignPos { get; set; }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class DoubleNode : ImperativeNode
    {
        public DoubleNode()
        {
            SignPos = new IDEHelpNode(IDEHelpNode.HelpNodeType.NumberNode);
            IDEValue = new IDEHelpNode(IDEHelpNode.HelpNodeType.NumberNode);
        }
        public IDEHelpNode SignPos { get; set; }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class BooleanNode : ImperativeNode
    {
        public string value { get; set; }
    }

    public class CharNode : ImperativeNode
    {
        public CharNode()
        {
            IDEValue = new IDEHelpNode(IDEHelpNode.HelpNodeType.TextNode);
        }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class StringNode : ImperativeNode
    {
        public StringNode()
        {
            IDEValue = new IDEHelpNode(IDEHelpNode.HelpNodeType.TextNode);
        }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class NullNode : ImperativeNode
    {
    }

    public class ArrayNode : ImperativeNode
    {
        public IDEHelpNode OpenBracketPos { get; set; }
        public IDEHelpNode CloseBracketPos { get; set; }
        public ArrayNode()
        {
            Expr = null;
            Ident = null;
            Type = null;
            OpenBracketPos = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
            CloseBracketPos = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
        }
        public Node Ident { get; set; }
        public Node Expr { get; set; }
        public Node Type { get; set; }
    }

    public class ReturnNode : ImperativeNode
    {
        public Node ReturnExpr
        {
            get;
            set;
        }
    }

    public class ExprListNode : ImperativeNode
    {
        public IDEHelpNode OpenCurlyBracePos { get; set; }
        public IDEHelpNode CloseCurlyBracePos { get; set; }
        public List<IDEHelpNode> ExprCommaPosList { get; set; }
        public ExprListNode()
        {
            list = new List<Node>();
            OpenCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            ExprCommaPosList = new List<IDEHelpNode>();
        }

        public List<Node> list
        {
            get;
            set;
        }
    }

    public class RangeExprNode : ImperativeNode
    {
        public IDEHelpNode FirstRangeOpPos { get; set; }
        public IDEHelpNode SecondRangeOpPos { get; set; }
        public IDEHelpNode StepOpPos { get; set; }
        public RangeExprNode()
        {
            IntNode defaultStep = new IntNode();
            defaultStep.value = "1";
            StepNode = defaultStep;
            stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize;
            FirstRangeOpPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
        }

        public Node FromNode { get; set; }
        public Node ToNode { get; set; }
        public Node StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }
    }

    public class ClassDeclNode : ImperativeNode
    {
        public ClassDeclNode()
        {
            varlist = new List<Node>();
            funclist = new List<Node>();
        }
        public string name { get; set; }
        public List<Node> varlist { get; set; }
        public List<Node> funclist { get; set; }
    }

    public class IdentifierNode : ImperativeNode
    {
        public IdentifierNode()
        {
            type = (int)ProtoCore.PrimitiveType.kInvalidType;
            ArrayDimensions = null;
            Return = new IDEHelpNode(IDEHelpNode.HelpNodeType.KeywordNode);
            
			//Modified by Mark -- start
            colon = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
            typeName = new IDEHelpNode(IDEHelpNode.HelpNodeType.TypeNode);
            typeName_kw = new IDEHelpNode(IDEHelpNode.HelpNodeType.KeywordNode);
            //end
        }

        public int type { get; set; }
        public ProtoCore.PrimitiveType datatype { get; set; }
        public string Value { get; set; }
        public IDEHelpNode Return { get; set; }
        public ArrayNode ArrayDimensions { get; set; }

        //Modified by Mark -- start
        public IDEHelpNode colon { get; set; }
        public IDEHelpNode typeName { get; set; }
        public IDEHelpNode typeName_kw { get; set; }
        
        //end
    }


    public class FunctionCallNode : ImperativeNode
    {
        public IDEHelpNode EndlinePos { get; set; }
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public List<IDEHelpNode> ArgCommaPosList { get; set; }
        public Node Function
        {
            get;
            set;
        }

        public List<Node> FormalArguments
        {
            get;
            set;
        }

        public FunctionCallNode()
        {
            FormalArguments = new List<Node>();
            EndlinePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            OpenParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            ArgCommaPosList = new List<IDEHelpNode>();
        }
    }

    public class VarDeclNode : ImperativeNode
    {
        public VarDeclNode()
        {
            name = new IDEHelpNode(IDEHelpNode.HelpNodeType.IdentNode);
            equal = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
        }
        public IDEHelpNode KeywordPos { get; set; }
        //public IDEHelpNode EndlinePos { get; set; }
        public IDEHelpNode TypeColonPos { get; set; }
        public IDEHelpNode ArgumentTypePos { get; set; }
        //public VarDeclNode()
        //{
        //    memregion = ProtoCore.DSASM.MemoryRegion.kInvalidRegion;
        //    //EndlinePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
        //    //TypeColonPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
        //    //ArgumentTypePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.TypeNode };
        //}

        //public ProtoCore.DSASM.MemoryRegion memregion
        //{
        //    get;
        //    set;
        //}

        public List<IDEHelpNode> Brackets { get; set; }

        public TypeNode IDEArgumentType { get; set; }

        public IDEHelpNode equal { get; set; }
        public IDEHelpNode name { get; set; }
        public Node NameNode
        {
            get;
            set;
        }
    }

    public class TypeNode : ImperativeNode
    {

        public TypeNode()
        {
            InitializeIDEHelpNode();
        }
        void InitializeIDEHelpNode()
        {
            colon = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
            brackets = new List<IDEHelpNode>();
            multiDim = new List<IDEHelpNode>();
            op = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
        }
        public void AddBracket(string _open, int open_line, int open_col, string _close, int close_line, int close_col)
        {
            brackets.Add(new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode) { Value = _open, Col = open_col, Line = open_line });
            brackets.Add(new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode) { Value = _close, Col = close_col, Line = close_line });
        }
        public void AddMultiDimNodes(string _mdopen, int mdopen_line, int mdopen_col, string _mdclose, int mdclose_line, int mdclose_col)
        {
            multiDim.Add(new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode) { Value = _mdopen, Col = mdopen_col, Line = mdopen_line });
            multiDim.Add(new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode) { Value = _mdclose, Col = mdclose_col, Line = mdclose_line });
        }
        public void BuildInTypeSetValue(string _value, int _line, int _col)
        {
            typeName = new IDEHelpNode(IDEHelpNode.HelpNodeType.KeywordNode) { Value = _value, Line = _line, Col = _col };
        }
        public void UserDefinedTypeSetValue(string _value, int _line, int _col)
        {
            typeName = new IDEHelpNode(IDEHelpNode.HelpNodeType.TypeNode) { Value = _value, Line = _line, Col = _col };
        }
        public IDEHelpNode colon { get; set; }
        public IDEHelpNode typeName { get; set; }
        public IDEHelpNode varName { get; set; }
        public IDEHelpNode op { get; set; }
        public List<IDEHelpNode> brackets { get; set; }
        public List<IDEHelpNode> multiDim { get; set; }
    }

    public class ArgumentSignatureNode : ImperativeNode
    {
        public ArgumentSignatureNode()
        {
            Arguments = new List<VarDeclNode>();
        }

        public List<VarDeclNode> Arguments
        {
            get;
            set;
        }

        public void AddArgument(VarDeclNode arg)
        {
            Arguments.Add(arg);
        }
    }

    public class CodeBlockNode : ImperativeNode
    {
        public CodeBlockNode()
        {
            Body = new List<Node>();
        }

        public List<Node> Body
        {
            get;
            set;
        }
    }

    //public class ConstructorDefinitionNode : Node
    //{
    //    public int localVars
    //    {
    //        get;
    //        set;
    //    }

    //    public ArgumentSignatureNode Signature
    //    {
    //        get;
    //        set;
    //    }

    //    public CodeBlockNode FunctionBody
    //    {
    //        get;
    //        set;
    //    }
    //}

    public class FunctionDefinitionNode : ImperativeNode
    {
        public IDEHelpNode KeywordPos { get; set; }
        public IDEHelpNode NamePos { get; set; }
        public IDEHelpNode TypeColonPos { get; set; }
        public IDEHelpNode ReturnTypePos { get; set; }
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public IDEHelpNode OpenCurlyBracePos { get; set; }
        public IDEHelpNode CloseCurlyBracePos { get; set; }
        public List<IDEHelpNode> ArgCommaPosList { get; set; }
        public FunctionDefinitionNode()
        {
            Brackets = new List<IDEHelpNode>();
            KeywordPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.KeywordNode };
            NamePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.IdentNode };
            //TypeColonPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            //ReturnTypePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.TypeNode };
            OpenParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            OpenCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            ArgCommaPosList = new List<IDEHelpNode>();
        }
        public int localVars
        {
            get;
            set;
        }

        public CodeBlockNode FunctionBody
        {
            get;
            set;
        }

        public ProtoCore.Type ReturnType
        {
            get;
            set;
        }

        public List<IDEHelpNode> Brackets { get; set; }

        public ArgumentSignatureNode Signature
        {
            get;
            set;
        }
    }

    public class InlineConditionalNode : ImperativeNode
    {
        public InlineConditionalNode()
        {
            QuestionPos = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
            ColonPos = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
        }

        public IDEHelpNode QuestionPos { get; set; } 
        public IDEHelpNode ColonPos { get; set; }
        public Node ConditionExpression { get; set; }
        public Node TrueExpression { get; set; }
        public Node FalseExpression { get; set; }
    }

    public class BinaryExpressionNode : ImperativeNode
    {
        public IDEHelpNode EndlinePos { get; set; }
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public IDEHelpNode OperatorPos { get; set; }
        public Node LeftNode
        {
            get;
            set;
        }

        public ProtoCore.DSASM.Operator Optr
        {
            get;
            set;
        }

        public Node RightNode
        {
            get;
            set;
        }
    }


    public class ElseIfBlock : ImperativeNode
    {
        public IDEHelpNode KeywordPos { get; set; }
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public IDEHelpNode OpenCurlyBracePos { get; set; }
        public IDEHelpNode CloseCurlyBracePos { get; set; }
        public ElseIfBlock()
        {
            Body = new List<Node>();
            KeywordPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.KeywordNode };
            OpenParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
        }
        public Node Expr
        {
            get;
            set;
        }

        public List<Node> Body
        {
            get;
            set;
        }
    }

    public class IfStmtNode : ImperativeNode
    {
        public IDEHelpNode KeywordPos { get; set; }
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public IDEHelpNode OpenCurlyBracePos { get; set; }
        public IDEHelpNode CloseCurlyBracePos { get; set; }
        public IDEHelpNode ElsePos { get; set; }
        public IDEHelpNode ElseOpenCurlyBracePos { get; set; }
        public IDEHelpNode ElseCloseCurlyBracePos { get; set; }
        public IfStmtNode()
        {
            ElseIfList = new List<ElseIfBlock>();
            IfBody = new List<Node>();
            ElseBody = new List<Node>();
            KeywordPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.KeywordNode };
            OpenParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
        }

        public Node IfExprNode
        {
            get;
            set;
        }

        public List<Node> IfBody
        {
            get;
            set;
        }

        public List<ElseIfBlock> ElseIfList
        {
            get;
            set;
        }

        public List<Node> ElseBody
        {
            get;
            set;
        }
    }

    public class WhileStmtNode : ImperativeNode
    {
        public IDEHelpNode KeywordPos { get; set; }
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public IDEHelpNode OpenCurlyBracePos { get; set; }
        public IDEHelpNode CloseCurlyBracePos { get; set; }
        public WhileStmtNode()
        {
            Body = new List<Node>();
            KeywordPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.KeywordNode };
            OpenParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            OpenCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseCurlyBracePos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
        }

        public Node Expr
        {
            get;
            set;
        }

        public List<Node> Body
        {
            get;
            set;
        }
    }

    public class UnaryExpressionNode : ImperativeNode
    {
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public IDEHelpNode OperatorPos { get; set; }
        public IDEHelpNode Operator { get; set; }

        public Node Expression
        {
            get;
            set;
        }
    }

    public class ForLoopNode : ImperativeNode
    {
        public IDEHelpNode KeywordPos { get; set; }
        public IDEHelpNode KwInPos { get; set; }
        public IDEHelpNode OpenParenPos { get; set; }
        public IDEHelpNode CloseParenPos { get; set; }
        public IDEHelpNode OpenCurlyBracePos { get; set; }
        public IDEHelpNode CloseCurlyBracePos { get; set; }
        public ForLoopNode()
        {
            body = new List<Node>();
            KeywordPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.KeywordNode };
            OpenParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            CloseParenPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.PunctuationNode };
            KwInPos = new IDEHelpNode() { Type = IDEHelpNode.HelpNodeType.KeywordNode };
        }
        public Node id
        {
            get;
            set;
        }

        public Node expression
        {
            get;
            set;
        }

        public List<Node> body
        {
            get;
            set;
        }
    }

    public class IdentifierListNode : ImperativeNode
    {
        public List<IDEHelpNode> DotPosList;
        public Node LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public Node RightNode { get; set; }
    }

    public class IDEHelpNode : ImperativeNode
    {
        public IDEHelpNode()
        { }
        public IDEHelpNode(HelpNodeType _type)
        { Type = _type; }
        public enum HelpNodeType { PunctuationNode, TypeNode, KeywordNode, IdentNode, NumberNode, TextNode }

        public HelpNodeType Type { get; set; }
        public string Value { get; set; }
        public void setValue(int _col, int _line, string _value)
        { Col = _col; Line = _line; Value = _value; }
    }

    public class PostFixNode : ImperativeNode
    {
        public IDEHelpNode OperatorPos { get; set; }
        public ImperativeNode Identifier { get; set; }
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
    }

    public class ParenExpressionNode : ImperativeNode
    {
        public ParenExpressionNode()
        {
            openParen = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
            closeParen = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
        }

        public Node expression { get; set; }
        public IDEHelpNode openParen { get; set; }
        public IDEHelpNode closeParen { get; set; }
    }

    public class BreakNode : ImperativeNode
    {
        public BreakNode()
        {
            KwBreak = new IDEHelpNode(IDEHelpNode.HelpNodeType.KeywordNode);
            EndLine = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
        }

        public IDEHelpNode KwBreak { get; set; }
        public IDEHelpNode EndLine { get; set; }
    }

    public class ContinueNode : ImperativeNode
    {
        public ContinueNode()
        {
            KwContinue = new IDEHelpNode(IDEHelpNode.HelpNodeType.KeywordNode);
            EndLine = new IDEHelpNode(IDEHelpNode.HelpNodeType.PunctuationNode);
        }

        public IDEHelpNode KwContinue { get; set; }
        public IDEHelpNode EndLine { get; set; }
    }
}


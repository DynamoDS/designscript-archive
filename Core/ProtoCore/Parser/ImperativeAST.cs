using System.Collections.Generic;

namespace ProtoCore.AST.ImperativeAST
{
    public abstract class ImperativeNode : Node
    {
        public ImperativeNode()
        {
        }

        public ImperativeNode(ImperativeNode rhs) : base(rhs)
        {
        }

        public override bool Equals(object other)
        {
            return base.Equals(other);
        }
    }

    public class LanguageBlockNode : ImperativeNode
    {
        public LanguageBlockNode()
        {
            codeblock = new ProtoCore.LanguageCodeBlock();
            Attributes = new List<ImperativeNode>();
        }

        public LanguageBlockNode(LanguageBlockNode rhs) : base(rhs)
        {
            codeblock = new ProtoCore.LanguageCodeBlock(rhs.codeblock);
            Attributes = new List<ImperativeNode>();
            foreach (ImperativeNode aNode in rhs.Attributes)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(aNode);
                Attributes.Add(newNode);
            }
        }

        public List<ImperativeNode> Attributes { get; set; }
        public ProtoCore.LanguageCodeBlock codeblock { get; set; }
        public Node CodeBlockNode { get; set; }
    }

    public class ArrayNameNode : ImperativeNode
    {
        public ArrayNode ArrayDimensions { get; set; }

        public ArrayNameNode()
        {
            ArrayDimensions = null;
        }

        public ArrayNameNode(ArrayNameNode rhs) : base(rhs)
        {
            ArrayDimensions = null;
            if (null != rhs.ArrayDimensions)
            {
                ArrayDimensions = new ArrayNode(rhs.ArrayDimensions);
            }
        }
    }

    public class GroupExpressionNode : ArrayNameNode
    {
        public ImperativeNode Expression { get; set; }
    }

    public class IdentifierNode : ArrayNameNode 
    {
        public IdentifierNode()
        {
            ArrayDimensions = null;
            datatype = new ProtoCore.Type
            {
                UID = (int)PrimitiveType.kInvalidType,
                rank = 0,
                IsIndexable = false,
                Name = null
            };
        }

        public IdentifierNode(string identName = null)
        {
            ArrayDimensions = null;
            datatype = new ProtoCore.Type
            {
                UID = (int)PrimitiveType.kInvalidType,
                rank = 0,
                IsIndexable = false,
                Name = null
            };
            Value = Name = identName;
        }


        public IdentifierNode(IdentifierNode rhs) : base(rhs)
        {
            datatype = new ProtoCore.Type
            {
                UID = rhs.datatype.UID,
                rank = rhs.datatype.rank,
                IsIndexable = rhs.datatype.IsIndexable,
                Name = rhs.datatype.Name
            };

            Value = rhs.Value;
        }

        public ProtoCore.Type datatype { get; set; }
        public string Value { get; set; }
        public string ArrayName { get; set; }
    }

    public class TypedIdentifierNode: IdentifierNode
    {
    }

    public class IntNode : ImperativeNode
    {
        public string value { get; set; }
        public IntNode()
        {
            value = string.Empty;
        }

        public IntNode(string val = null)
        {
            value = val;
        }

        public IntNode(IntNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }
    }

    public class DoubleNode : ImperativeNode
    {
        public string value { get; set; }
        public DoubleNode()
        {
            value = string.Empty;
        }
        public DoubleNode(DoubleNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }
    }

    public class BooleanNode : ImperativeNode
    {
        public string value { get; set; }
        public BooleanNode()
        {
            value = string.Empty;
        }
        public BooleanNode(BooleanNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }
    }

    public class CharNode : ImperativeNode
    {
        public string value { get; set; }
        public CharNode()
        {
            value = string.Empty;
        }
        public CharNode(CharNode rhs)
        {
            value = rhs.value;
        }
    }

    public class StringNode : ImperativeNode
    {
        public string value { get; set; }
        public StringNode()
        {
            value = string.Empty;
        }
        public StringNode(StringNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }
    }

    public class NullNode : ImperativeNode
    {
    }

    public class ArrayNode : ImperativeNode
    {
        public ArrayNode()
        {
            Expr = null;
            Type = null;
        }

        public ArrayNode(ArrayNode rhs)
            : base(rhs)
        {
            Expr = null;
            Type = null;
            if (null != rhs)
            {
                if (null != rhs.Expr)
                {
                    Expr = ProtoCore.Utils.NodeUtils.Clone(rhs.Expr);
                }

                if (null != rhs.Type)
                {
                    Type = ProtoCore.Utils.NodeUtils.Clone(rhs.Type);
                }
            }
        }

        public ImperativeNode Expr { get; set; }
        public ImperativeNode Type { get; set; }
    }

    public class FunctionCallNode : ArrayNameNode 
    {
        public ImperativeNode Function
        {
            get;
            set;
        }

        public List<ImperativeNode> FormalArguments
        {
            get;
            set;
        }

        public FunctionCallNode()
        {
            FormalArguments = new List<ImperativeNode>();
        }

        public FunctionCallNode(FunctionCallNode rhs) : base(rhs)
        {
            Function = ProtoCore.Utils.NodeUtils.Clone(rhs.Function);
            FormalArguments = new List<ImperativeNode>();
            foreach (ImperativeNode argNode in rhs.FormalArguments)
            {
                ImperativeNode tempNode = ProtoCore.Utils.NodeUtils.Clone(argNode);
                FormalArguments.Add(tempNode);
            }
        }
    }

    public class VarDeclNode : ImperativeNode
    {
        public VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kInvalidRegion;
        }

        public ProtoCore.DSASM.MemoryRegion memregion { get; set; }
        public ProtoCore.Type ArgumentType { get; set; }
        public ImperativeNode NameNode { get; set; }
    }

    public class ReturnNode : ImperativeNode
    {
        public ImperativeNode ReturnExpr { get; set; }
    }

    public class ArgumentSignatureNode : ImperativeNode
    {
        public ArgumentSignatureNode()
        {
            Arguments = new List<VarDeclNode>();
        }

        public List<VarDeclNode> Arguments { get; set; }

        public void AddArgument(VarDeclNode arg)
        {
            Arguments.Add(arg);
        }
    }

    public class ExprListNode : ArrayNameNode
    {
        public ExprListNode()
        {
            list = new List<ImperativeNode>();
        }


        public ExprListNode(ExprListNode rhs)
            : base(rhs)
        {
            list = new List<ImperativeNode>();
            foreach (ImperativeNode argNode in rhs.list)
            {
                ImperativeNode tempNode = ProtoCore.Utils.NodeUtils.Clone(argNode);
                list.Add(tempNode);
            }
        }

        public List<ImperativeNode> list { get; set; }
    }

    public class CodeBlockNode : ImperativeNode
    {
        public CodeBlockNode()
        {
            Body = new List<ImperativeNode>();
        }

        public CodeBlockNode(CodeBlockNode rhs) : base(rhs)
        {
            Body = new List<ImperativeNode>();
            foreach (ImperativeNode aNode in rhs.Body)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(aNode);
                Body.Add(newNode);
            }
        }

        public List<ImperativeNode> Body { get; set; }
    }

    public class ConstructorDefinitionNode : ImperativeNode
    {
        public int localVars { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
    }

    public class FunctionDefinitionNode : ImperativeNode
    {
        public int localVars { get; set; }
        public List<ImperativeNode> Attributes { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
    }

    public class InlineConditionalNode : ImperativeNode
    {
        public ImperativeNode ConditionExpression { get; set; }
        public ImperativeNode TrueExpression { get; set; }
        public ImperativeNode FalseExpression { get; set; }
    }

    public class BinaryExpressionNode : ImperativeNode
    {
        public ImperativeNode LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public ImperativeNode RightNode { get; set; }

        public BinaryExpressionNode()
        {
        }

        public BinaryExpressionNode(ImperativeNode left = null, ImperativeNode right = null, ProtoCore.DSASM.Operator optr = DSASM.Operator.none)
        {
            LeftNode = left;
            Optr = optr;
            RightNode = right;
        }

        public BinaryExpressionNode(BinaryExpressionNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = rhs.LeftNode == null ? null : ProtoCore.Utils.NodeUtils.Clone(rhs.LeftNode);
            RightNode = rhs.RightNode == null ? null : ProtoCore.Utils.NodeUtils.Clone(rhs.RightNode);
        }
    }


    public class ElseIfBlock : ImperativeNode
    {
        public ElseIfBlock()
        {
            Body = new List<ImperativeNode>();
            ElseIfBodyPosition = new IfStmtPositionNode();
        }


        public ElseIfBlock(ElseIfBlock rhs) : base(rhs)
        {
            Expr = ProtoCore.Utils.NodeUtils.Clone(rhs.Expr);
            ElseIfBodyPosition = ProtoCore.Utils.NodeUtils.Clone(rhs.ElseIfBodyPosition);

            Body = new List<ImperativeNode>();
            foreach (ImperativeNode iNode in rhs.Body)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(iNode);
                Body.Add(newNode);
            }
        }

        public ImperativeNode Expr { get; set; }
        public List<ImperativeNode> Body { get; set; }
        public ImperativeNode ElseIfBodyPosition { get; set; }
    }

    public class IfStmtPositionNode: ImperativeNode
    {
        public IfStmtPositionNode()
        {
        }

        public IfStmtPositionNode(IfStmtPositionNode rhs):base(rhs)
        {
        }
    }

    public class IfStmtNode : ImperativeNode
    {
        public IfStmtNode()
        {
            ElseIfList = new List<ElseIfBlock>();
            IfBody = new List<ImperativeNode>();
            IfBodyPosition = new IfStmtPositionNode();
            ElseBody = new List<ImperativeNode>();
            ElseBodyPosition = new IfStmtPositionNode();
        }


        public IfStmtNode(IfStmtNode rhs) : base(rhs)
        {
            //
            IfExprNode = ProtoCore.Utils.NodeUtils.Clone(rhs.IfExprNode);


            //
            IfBody = new List<ImperativeNode>();
            foreach (ImperativeNode stmt in rhs.IfBody)
            {
                ImperativeNode body = ProtoCore.Utils.NodeUtils.Clone(stmt as ImperativeNode);
                IfBody.Add(body);
            }

            //
            IfBodyPosition = ProtoCore.Utils.NodeUtils.Clone(rhs.IfBodyPosition);

            //
            ElseIfList = new List<ElseIfBlock>();
            foreach (ElseIfBlock elseBlock in rhs.ElseIfList)
            {
                ImperativeNode elseNode = ProtoCore.Utils.NodeUtils.Clone(elseBlock as ImperativeNode);
                ElseIfList.Add(elseNode as ElseIfBlock);
            }

            //
            ElseBody = new List<ImperativeNode>();
            foreach (ImperativeNode stmt in rhs.ElseBody)
            {
                ImperativeNode tmpNode = ProtoCore.Utils.NodeUtils.Clone(stmt);
                ElseBody.Add(tmpNode);
            }

            //
            ElseBodyPosition = ProtoCore.Utils.NodeUtils.Clone(rhs.ElseBodyPosition);
        }

        public ImperativeNode IfExprNode { get; set; }
        public List<ImperativeNode> IfBody { get; set; }
        public ImperativeNode IfBodyPosition { get; set; }
        public List<ElseIfBlock> ElseIfList { get; set; }
        public List<ImperativeNode> ElseBody { get; set; }
        public ImperativeNode ElseBodyPosition { get; set; }
    }

    public class WhileStmtNode : ImperativeNode
    {
        public WhileStmtNode()
        {
            Body = new List<ImperativeNode>();
        }

        public WhileStmtNode(WhileStmtNode rhs) : base(rhs)
        {
            Expr = ProtoCore.Utils.NodeUtils.Clone(rhs.Expr);
            Body = new List<ImperativeNode>(); 
            foreach (ImperativeNode iNode in rhs.Body)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(iNode);
                Body.Add(newNode);
            }
        }

        public ImperativeNode Expr { get; set; }
        public List<ImperativeNode> Body { get; set; }
    }

    public class UnaryExpressionNode : ImperativeNode
    {
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
        public ImperativeNode Expression { get; set; }

        public UnaryExpressionNode()
        {

        }

        public UnaryExpressionNode(UnaryExpressionNode rhs) : base(rhs)
        {
            Operator = rhs.Operator;
            Expression = ProtoCore.Utils.NodeUtils.Clone(rhs.Expression);
        }
    }

    public class RangeExprNode : ArrayNameNode
    {
        public ImperativeNode FromNode { get; set; }
        public ImperativeNode ToNode { get; set; }
        public ImperativeNode StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }

        public RangeExprNode()
        {
        }

        public RangeExprNode(RangeExprNode rhs) : base(rhs)
        {
            FromNode = ProtoCore.Utils.NodeUtils.Clone(rhs.FromNode);
            ToNode = ProtoCore.Utils.NodeUtils.Clone(rhs.ToNode);
            if (null != rhs.StepNode)
            {
                StepNode = ProtoCore.Utils.NodeUtils.Clone(rhs.StepNode);
            }
            stepoperator = rhs.stepoperator;
        }
    }

    public class ForLoopNode : ImperativeNode
    {
        public ForLoopNode()
        {
            body = new List<ImperativeNode>();
        }


        public ForLoopNode(ForLoopNode rhs) : base(rhs)
        {
            body = new List<ImperativeNode>();
            foreach (ImperativeNode iNode in rhs.body)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(iNode);
                body.Add(newNode);
            }
            loopVar = ProtoCore.Utils.NodeUtils.Clone(rhs.loopVar);
            expression = ProtoCore.Utils.NodeUtils.Clone(rhs.expression);

            KwForLine = rhs.KwForLine;
            KwForCol = rhs.KwForCol;
            KwInLine = rhs.KwInLine;
            KwInCol = rhs.KwInCol;
        }

        public int KwForLine { get; set; }
        public int KwForCol { get; set; }
        public int KwInLine { get; set; }
        public int KwInCol { get; set; }
        public ImperativeNode loopVar { get; set; }
        public ImperativeNode expression { get; set; }
        public List<ImperativeNode> body { get; set; }
    }

    public class IdentifierListNode : ImperativeNode
    {
        public ImperativeNode LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public ImperativeNode RightNode { get; set; }

        public IdentifierListNode()
        {
        }

        public IdentifierListNode(IdentifierListNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = ProtoCore.Utils.NodeUtils.Clone(rhs.LeftNode);
            RightNode = ProtoCore.Utils.NodeUtils.Clone(rhs.RightNode);
        }
    }

    public class PostFixNode : ImperativeNode
    {
        public ImperativeNode Identifier { get; set; }
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
    }

    public class BreakNode: ImperativeNode
    {
    }

    public class ContinueNode: ImperativeNode
    {
    }

    public class DefaultArgNode : ImperativeNode
    {// not supposed to be used in parser 
    }

    public class ThrowNode : ImperativeNode
    {
        public ImperativeNode expression { get; set; }
    }

    public class TryBlockNode : ImperativeNode
    {
        public List<ImperativeNode> body { get; set; }
    }

    public class CatchFilterNode : ImperativeNode
    {
        public IdentifierNode var { get; set; }
        public ProtoCore.Type type { get; set; }
    }

    public class CatchBlockNode : ImperativeNode
    {
        public CatchFilterNode catchFilter { get; set; }
        public List<ImperativeNode> body { get; set; }
    }

    public class ExceptionHandlingNode : ImperativeNode
    {
        public TryBlockNode tryBlock { get; set; }
        public List<CatchBlockNode> catchBlocks { get; set; }

        public ExceptionHandlingNode()
        {
            catchBlocks = new List<CatchBlockNode>();
        }
    }
}

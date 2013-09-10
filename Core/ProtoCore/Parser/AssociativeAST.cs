using System;
using System.Collections.Generic;
using ProtoCore.DesignScriptParser;
using ProtoCore.Utils;

namespace ProtoCore.AST.AssociativeAST
{
    public abstract class AssociativeNode : Node
    {
        public AssociativeNode()
        {
        }

        public AssociativeNode(AssociativeNode rhs) : base(rhs)
        {
        }

        public override bool Compare(Node other)
        {
            throw new NotImplementedException();
        }
    }

    public class CommentNode : AssociativeNode
    {
        public enum CommentType { Inline, Block }
        public CommentType Type { get; private set; }
        public string Value { get; private set; }
        public CommentNode(int col, int line, string value, CommentType type)
        {
            this.col = col;
            this.line = line;
            Value = value;
            Type = type;
        }
    }

    public class LanguageBlockNode : AssociativeNode
    {
        public LanguageBlockNode()
        {
            codeblock = new ProtoCore.LanguageCodeBlock();
            Attributes = new List<AssociativeNode>();
        }

        public LanguageBlockNode(LanguageBlockNode rhs) : base(rhs)
        {
            CodeBlockNode = NodeUtils.Clone(rhs.CodeBlockNode);

            codeblock = new ProtoCore.LanguageCodeBlock(rhs.codeblock);

            Attributes = new List<AssociativeNode>();
            foreach (AssociativeNode aNode in rhs.Attributes)
            {
                AssociativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(aNode);
                Attributes.Add(newNode);
            }
        }

        public Node CodeBlockNode { get; set; }
        public ProtoCore.LanguageCodeBlock codeblock { get; set; }
        public List<AssociativeNode> Attributes { get; set; }

        //only comparing attributes and codeblock at the moment
        public override bool Compare(Node other)
        {
            if (other is LanguageBlockNode)
            {
                LanguageBlockNode otherNode = other as LanguageBlockNode;
                bool result = true;
                //bool result = this.codeblock.Compare(otherNode.codeblock);

                if (this.Attributes.Count != otherNode.Attributes.Count)
                    return false;

                bool attrCompare = true;
                for (int i = 0; i < this.Attributes.Count; i++)
                {
                    attrCompare = attrCompare && (this.Attributes[i].Compare(otherNode.Attributes[i]));
                }
                result = result && attrCompare;
                return result;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// This node will be used by the optimiser
    /// </summary>
    public class MergeNode : AssociativeNode
    {
        public List<AssociativeNode> MergedNodes
        {
            get;
            private set;
        }

        public MergeNode()
        {
            MergedNodes = new List<AssociativeNode>();
        }
    }

    /// <summary>
    /// This class is only used in GraphCompiler
    /// </summary>
    public class ArrayIndexerNode : AssociativeNode 
    {
        public ArrayNode ArrayDimensions;
        public AssociativeNode Array;

        public override bool Compare(Node other)
        {
            if (other is ArrayIndexerNode)
            {
                ArrayIndexerNode otherNode = other as ArrayIndexerNode;
                return (this.Array.Compare(otherNode.Array) && this.ArrayDimensions.Compare(otherNode.ArrayDimensions));
            }
            else
                return false;
        }
    }

    public class ArrayNameNode : AssociativeNode
    {
        public ArrayNode ArrayDimensions
        {
            get;
            set;
        }

        public List<AssociativeNode> ReplicationGuides
        {
            get;
            set;
        }

        public ArrayNameNode()
        {
            ArrayDimensions = null;
            ReplicationGuides = null;
        }


        public ArrayNameNode(ArrayNameNode rhs) : base(rhs)
        {
            ArrayDimensions = null;
            if (null != rhs.ArrayDimensions)
            {
                ArrayDimensions = new ArrayNode(rhs.ArrayDimensions);
            }

            ReplicationGuides = null;
            if (null != rhs.ReplicationGuides)
            {
                ReplicationGuides = new List<AssociativeNode>();
                foreach (AssociativeNode argNode in rhs.ReplicationGuides)
                {
                    AssociativeNode tempNode = NodeUtils.Clone(argNode);
                    ReplicationGuides.Add(tempNode);
                }
            }
        }

        public override bool Compare(Node other)
        {
            if (other is ArrayNameNode)
            {
                ArrayNameNode otherNode = other as ArrayNameNode;
                bool result = this.ArrayDimensions.Compare(otherNode.ArrayDimensions);

                if (this.ReplicationGuides.Count != otherNode.ReplicationGuides.Count)
                    return false;

                bool replicationCompare = true;
                for (int i = 0; i < this.ReplicationGuides.Count; i++)
                {
                    replicationCompare = replicationCompare && (this.ReplicationGuides[i].Compare(otherNode.ReplicationGuides[i]));
                }
                result = result && replicationCompare;

                return result;
            }
            else
                return false;
        }
    }

    public class GroupExpressionNode : ArrayNameNode
    {
        public AssociativeNode Expression
        {
            get;
            set;
        }

        public GroupExpressionNode()
        {
        }

        public GroupExpressionNode(GroupExpressionNode rhs)
            : base(rhs)
        {
        }

        public override bool Compare(Node other)
        {
            if (other is GroupExpressionNode)
            {
                GroupExpressionNode otherNode = other as GroupExpressionNode;
                bool result = this.Expression.Compare(otherNode.Expression);
                return result;
            }
            else
                return false;
        }
    }


    public class IdentifierNode : ArrayNameNode
    {
        public IdentifierNode(string identName = null)
        {
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

        public ProtoCore.Type datatype
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public override bool Compare(Node other)
        {
            if (other is IdentifierNode)
            {
                IdentifierNode otherNode = other as IdentifierNode;
                return this.datatype.Equals(otherNode.datatype) && this.Value.Trim() == otherNode.Value.Trim();
            }
            else
                return false;
        }
    }

    public class TypedIdentifierNode : IdentifierNode
    {
    }

    public class IdentifierListNode : AssociativeNode
    {
        public bool isLastSSAIdentListFactor { get; set; }

        public AssociativeNode LeftNode
        {
            get;
            set;
        }

        public ProtoCore.DSASM.Operator Optr
        {
            get;
            set;
        }

        public AssociativeNode RightNode
        {
            get;
            set;
        }

        public IdentifierListNode()
        {
            isLastSSAIdentListFactor = false;
        }

        public IdentifierListNode(IdentifierListNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = NodeUtils.Clone(rhs.LeftNode);
            RightNode = NodeUtils.Clone(rhs.RightNode);
            isLastSSAIdentListFactor = rhs.isLastSSAIdentListFactor;
        }

        public override bool Compare(Node other)
        {
            if (other is IdentifierListNode)
            {
                IdentifierListNode otherNode = other as IdentifierListNode;
                return this.LeftNode.Compare(otherNode.LeftNode) && (this.RightNode.Compare(otherNode.RightNode)) && (this.Optr == otherNode.Optr);
            }
            else
                return false;
        }
    }

    public class IntNode : AssociativeNode
    {
        public string value { get; set; }

        public IntNode(string val = null)
        {
            value = val;
        }
        public IntNode(IntNode rhs) : base(rhs)
        {
            value = rhs.value;
        }

        public override bool Compare(Node other)
        {
            return this.value == (other as IntNode).value;
        }
    }

    public class DoubleNode : AssociativeNode
    {
        public string value { get; set; }
        public DoubleNode(string val = null)
        {
            value = val;
        }
        public DoubleNode(DoubleNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }

        public override bool Compare(Node other)
        {
            if (other is DoubleNode)
                return (this.value == (other as DoubleNode).value);
            else
                return false;
        }
    }

    public class BooleanNode : AssociativeNode
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

        public override bool Compare(Node other)
        {
            if (other is BooleanNode)
                return this.value.Trim() == (other as BooleanNode).value.Trim();
            else
                return false;
        }
    }

    public class CharNode : AssociativeNode
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
        public override bool Compare(Node other)
        {
            if (other is CharNode)
                return this.value == (other as CharNode).value;
            else
                return false;
        }
    }

    public class StringNode : AssociativeNode
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

        public override bool Compare(Node other)
        {
            if (other is StringNode)
                return this.value == (other as StringNode).value;
            else
                return false;
        }
    }

    public class NullNode : AssociativeNode
    {
        public override bool Compare(Node other)
        {
            if (other is NullNode)
            {
                return true;
            }
            else
                return false;
        }
    }

    public class ReturnNode : AssociativeNode
    {
        public AssociativeNode ReturnExpr
        {
            get;
            set;
        }

        public override bool Compare(Node other)
        {
            if (other is ReturnNode)
            {
                ReturnNode otherNode = other as ReturnNode;
                return (this.ReturnExpr.Compare(otherNode.ReturnExpr));
            }
            else
                return false;
        }
    }

    public class FunctionCallNode : ArrayNameNode
    {
        public int DynamicTableIndex { get; set; }
        public AssociativeNode Function { get; set; }
        public List<AssociativeNode> FormalArguments { get; set; }

        public FunctionCallNode()
        {
            FormalArguments = new List<AssociativeNode>();
            DynamicTableIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public FunctionCallNode(FunctionCallNode rhs)
            : base(rhs)
        {
            Function = NodeUtils.Clone(rhs.Function);
            FormalArguments = new List<AssociativeNode>();
            foreach (AssociativeNode argNode in rhs.FormalArguments)
            {
                AssociativeNode tempNode = NodeUtils.Clone(argNode);
                FormalArguments.Add(tempNode);
            }

            DynamicTableIndex = rhs.DynamicTableIndex;
        }

        public override bool Compare(Node other)
        {
            bool result = true;
            if (other is FunctionCallNode)
            {
                FunctionCallNode otherNode = other as FunctionCallNode;
                result = (this.DynamicTableIndex == otherNode.DynamicTableIndex) && (this.Function.Compare(otherNode.Function));

                if (this.FormalArguments.Count != otherNode.FormalArguments.Count)
                    return false;

                bool formalArgResult = true;
                for (int i=0; i<this.FormalArguments.Count; i++)
                {
                    formalArgResult = formalArgResult && (FormalArguments[i].Compare(otherNode.FormalArguments[i]));
                }
                result = result && formalArgResult;
            }
            else
                result = false;
            return result;
        }
    }

    public class FunctionDotCallNode : AssociativeNode
    {
        public FunctionCallNode DotCall { get; set; }
        public FunctionCallNode FunctionCall { get; set; }
        public FunctionCallNode NameMangledCall { get; set; }
        public bool isLastSSAIdentListFactor { get; set; }
        public string lhsName { get; set; }

        public FunctionDotCallNode(FunctionCallNode callNode)
        {
            DotCall = new FunctionCallNode();
            FunctionCall = callNode;
            isLastSSAIdentListFactor = false;
        }

        public FunctionDotCallNode(string lhsName, FunctionCallNode callNode)
        {
            this.lhsName = lhsName;
            FunctionCall = callNode;
            isLastSSAIdentListFactor = false;
        }

        public FunctionDotCallNode(FunctionDotCallNode rhs): base(rhs)
        {
            DotCall = new FunctionCallNode(rhs.DotCall);
            FunctionCall = new FunctionCallNode(rhs.FunctionCall);
            lhsName = rhs.lhsName;
            isLastSSAIdentListFactor = rhs.isLastSSAIdentListFactor;
        }

        public IdentifierListNode GetIdentList()
        {
            IdentifierListNode inode = new IdentifierListNode();
            inode.LeftNode = DotCall.FormalArguments[0];
            inode.Optr = DSASM.Operator.dot;
            inode.RightNode = FunctionCall.Function;
            return inode;
        }

        public override bool Compare(Node other)
        {
            if (other is FunctionDotCallNode)
            {
                FunctionDotCallNode otherNode = other as FunctionDotCallNode;
                return (this.DotCall.Compare(otherNode.DotCall) && this.FunctionCall.Compare(otherNode.FunctionCall) &&
                    (this.lhsName == otherNode.lhsName));
            }
            else
                return false;
        }
    }

    public class VarDeclNode : AssociativeNode
    {
        public VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kInvalidRegion;
            Attributes = new List<AssociativeNode>();
        }

        public VarDeclNode(VarDeclNode rhs)
            : base(rhs)
        {
            Attributes = new List<AssociativeNode>();
            foreach (AssociativeNode aNode in rhs.Attributes)
            {
                AssociativeNode newNode = NodeUtils.Clone(aNode);
                Attributes.Add(newNode);
            }
            memregion = rhs.memregion;
            ArgumentType = new ProtoCore.Type
            {
                UID = rhs.ArgumentType.UID,
                rank = rhs.ArgumentType.rank,
                IsIndexable = rhs.ArgumentType.IsIndexable,
                Name = rhs.ArgumentType.Name
            };
            NameNode = NodeUtils.Clone(rhs.NameNode);
            access = rhs.access;
            IsStatic = rhs.IsStatic;
        }

        public List<AssociativeNode> Attributes { get; set; }
        public ProtoCore.DSASM.MemoryRegion memregion { get; set; }
        public ProtoCore.Type ArgumentType { get; set; }
        public AssociativeNode NameNode { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
        public bool IsStatic { get; set; }
        public override string ToString()
        {
            string str = IsStatic ? "static " : "";
            return string.Format("{0}{1} : {2}", str, NameNode.Name, ToString(ArgumentType));
        }
        public static string ToString(ProtoCore.Type type)
        {
            if (!type.IsIndexable)
                return type.Name;
            string typename = type.Name;
            for (int i = 0; i < type.rank; ++i)
                typename += "[]";
            if (type.rank == -1) //variable rank array
                typename += "[]..[]";
            return typename;
        }
        public override bool Compare(Node other)
        {
            if (other is VarDeclNode)
            {
                VarDeclNode otherNode = other as VarDeclNode;
                bool result = true;
                result = (this.memregion == otherNode.memregion) && (this.ArgumentType.Equals(otherNode.ArgumentType)) && (this.NameNode.Compare(otherNode.NameNode))
                    && (this.IsStatic == otherNode.IsStatic);
                bool attributesCompare = true;
                if (this.Attributes.Count != otherNode.Attributes.Count)
                {
                    return false;
                }

                for (int i = 0; i < Attributes.Count; i++)
                {
                    attributesCompare = attributesCompare && (this.Attributes[i].Compare(otherNode.Attributes[i]));
                }
                result = result && attributesCompare;
                return result;
            }
            else
            {
                return false;
            }
        }
    }

    public class ArgumentSignatureNode : AssociativeNode
    {
        public ArgumentSignatureNode()
        {
            Arguments = new List<VarDeclNode>();
        }

        public ArgumentSignatureNode(ArgumentSignatureNode rhs)
            : base(rhs)
        {
            Arguments = new List<VarDeclNode>();

            foreach (VarDeclNode aNode in rhs.Arguments)
            {
                VarDeclNode newNode = new VarDeclNode(aNode);
                Arguments.Add(newNode);
            }
        }

        public List<VarDeclNode> Arguments { get; set; }

        public void AddArgument(VarDeclNode arg)
        {
            Arguments.Add(arg);
        }
        public override string ToString()
        {
            string signature = "";
            int nArgs = Arguments.Count;
            for (int i = 0; i < nArgs; ++i)
            {
                signature += Arguments[i].ToString();
                if (i >= 0 && i < nArgs - 1)
                    signature += ", ";
            }

            return string.Format("({0})", signature);
        }

        public override bool Compare(Node other)
        {
            if (other is ArgumentSignatureNode)
            {
                ArgumentSignatureNode otherNode = other as ArgumentSignatureNode;
                bool result = true;
                if (this.Arguments.Count != otherNode.Arguments.Count)
                    return false;

                for (int i = 0; i < this.Arguments.Count; i++)
                {
                    result = result && (this.Arguments[i].Compare(otherNode.Arguments[i]));
                }
                return result;
            }
            else
                return false;
        }
    }



    public class CodeBlockNode : AssociativeNode
    {
        public ProtoCore.DSASM.SymbolTable symbols { get; set; }
        public ProtoCore.DSASM.ProcedureTable procTable { get; set; }
        public List<AssociativeNode> Body { get; set; }

        public CodeBlockNode()
        {
            Body = new List<AssociativeNode>();
            symbols = new ProtoCore.DSASM.SymbolTable("AST generated", ProtoCore.DSASM.Constants.kInvalidIndex);
            procTable = new ProtoCore.DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        public CodeBlockNode(CodeBlockNode rhs) : base(rhs)
        {
            Body = new List<AssociativeNode>();
            foreach (AssociativeNode aNode in rhs.Body)
            {
                AssociativeNode newNode = NodeUtils.Clone(aNode);
                Body.Add(newNode);
            }
        }

        public override bool Compare(Node other)
        {
            //temp, not sure how to compare symbol table and proc table
            if (other is CodeBlockNode)
            {
                CodeBlockNode otherNode = other as CodeBlockNode;
                bool result = true;
                if (this.Body.Count != otherNode.Body.Count)
                    return false;

                for (int i = 0; i < this.Body.Count; i++)
                {
                    result = result && (Body[i].Compare(otherNode.Body[i]));
                }
                return result;
            }
            else
                return false;
        }
    }

    public class ClassDeclNode : AssociativeNode
    {
        public ClassDeclNode()
        {
            varlist = new List<AssociativeNode>();
            funclist = new List<AssociativeNode>();
            IsImportedClass = false;
        }

        public bool IsImportedClass { get; set; }
        public string className { get; set; }
        public List<AssociativeNode> Attributes { get; set; }
        public List<string> superClass { get; set; }
        public List<AssociativeNode> varlist { get; set; }
        public List<AssociativeNode> funclist { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //if (IsExternLib)
            //    sb.Append("extern ");
            sb.AppendFormat("class {0}", className);
            if (null != superClass)
            {
                if (superClass.Count > 0)
                    sb.Append(" extends ");
                for (int i = 0; i < superClass.Count; ++i)
                {
                    if (i > 0 && i < superClass.Count - 1)
                        sb.Append(", ");
                    sb.Append(superClass[i]);
                }
            }
            sb.AppendLine();
            sb.AppendLine("{");
            foreach (var item in varlist)
            {
                sb.AppendLine(string.Format("{0};", item));
            }

            foreach (var item in funclist)
            {
                if (item is ConstructorDefinitionNode)
                    sb.AppendLine(item.ToString() + "{}");
                else if (!item.Name.StartsWith("%"))
                    sb.AppendLine(item.ToString() + ";");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        //not comparing isImportedClass, isExternLib, ExternLibName
        public override bool Compare(Node other)
        {
            if (other is ClassDeclNode)
            {
                ClassDeclNode otherNode = other as ClassDeclNode;
                bool result = (this.className == otherNode.className);

                if (this.Attributes.Count != otherNode.Attributes.Count || this.superClass.Count != otherNode.superClass.Count
                        || this.varlist.Count != otherNode.varlist.Count || this.funclist.Count != otherNode.varlist.Count)
                    return false;

                bool attrCompare = true;
                for (int i = 0; i < this.Attributes.Count; i++)
                {
                    attrCompare = attrCompare && (this.Attributes[i].Compare(otherNode.Attributes[i]));
                }

                bool superClassCompare = true;
                for (int i = 0; i < this.Attributes.Count; i++)
                {
                    superClassCompare = superClassCompare && (this.superClass[i] == otherNode.superClass[i]);
                }

                bool varCompare = true;
                for (int i = 0; i < this.Attributes.Count; i++)
                {
                    varCompare = varCompare && (this.varlist[i].Compare(otherNode.varlist[i]));
                }

                bool funcCompare = true;
                for (int i = 0; i < this.Attributes.Count; i++)
                {
                    funcCompare = funcCompare && (this.funclist[i].Compare(otherNode.funclist[i]));
                }
                
                result = result && attrCompare && superClassCompare && varCompare && funcCompare;
                return result;
            }
            else
                return false;
        }
    }

    public class ConstructorDefinitionNode : AssociativeNode
    {
        public int localVars { get; set; }
        public List<AssociativeNode> Attributes { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public AssociativeNode Pattern { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public FunctionCallNode baseConstr { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }

        public override string ToString()
        {
            return string.Format("constructor {0}{1}", Name, Signature);
        }

        public override bool Compare(Node other)
        {
            if (other is ConstructorDefinitionNode)
            {
                ConstructorDefinitionNode otherNode = other as ConstructorDefinitionNode;
                bool result = true;
                result = (this.localVars == otherNode.localVars) && (this.Signature.Compare(otherNode.Signature)) && (this.ReturnType.Equals(otherNode.ReturnType))
                    && (this.FunctionBody.Compare(otherNode.FunctionBody));

                if (this.Attributes.Count != otherNode.Attributes.Count)
                    return false;

                bool attrCompare = true;
                for (int i = 0; i < this.Attributes.Count; i++)
                {
                    attrCompare = attrCompare && (this.Attributes[i].Compare(otherNode.Attributes[i]));
                }
                result = result && attrCompare;
                return result;
            }
            else
                return false;
        }
    }

    public class FunctionDefinitionNode : AssociativeNode
    {
        public CodeBlockNode FunctionBody { get; set; }

        public ProtoCore.Type                                   ReturnType      { get; set; }
        public List<AssociativeNode>                            Attributes      { get; set; }
        public ArgumentSignatureNode                            Singnature      { get; set; }
        public AssociativeNode                                  Pattern         { get; set; }
        public bool                                             IsExternLib     { get; set; }
        public bool                                             IsBuiltIn       { get; set; }
        public ProtoCore.Lang.BuiltInMethods.BuiltInMethodID    BuiltInMethodId { get; set; }
        public bool                                             IsDNI           { get; set; }
        public string                                           ExternLibName   { get; set; }
        public ProtoCore.DSASM.AccessSpecifier                  access          { get; set; }
        public bool                                             IsStatic        { get; set; }
        public bool                                             IsAutoGenerated { get; set; }
        public bool IsAssocOperator { get; set; }
        public bool IsAutoGeneratedThisProc { get; set; }

        public FunctionDefinitionNode()
        {
            BuiltInMethodId = ProtoCore.Lang.BuiltInMethods.BuiltInMethodID.kInvalid;
            IsAutoGenerated = false;
            IsAutoGeneratedThisProc = false;

            ReturnType = new Type();
            ReturnType.Initialize();
            IsBuiltIn = false;
        }

        public FunctionDefinitionNode(FunctionDefinitionNode rhs)
        {
            this.Name = rhs.Name;
            if (null != rhs.FunctionBody)
            {
                this.FunctionBody = new CodeBlockNode(rhs.FunctionBody);
            }
            else
            {
                this.FunctionBody = new CodeBlockNode();
            }

            this.ReturnType = rhs.ReturnType;

            this.Attributes = rhs.Attributes;
            this.Singnature = new ArgumentSignatureNode(rhs.Singnature);
            this.Pattern = rhs.Pattern;
            this.IsExternLib = rhs.IsExternLib;
            this.BuiltInMethodId = rhs.BuiltInMethodId;
            this.IsDNI = rhs.IsDNI;
            this.ExternLibName = rhs.ExternLibName;
            this.access = rhs.access;
            this.IsStatic = rhs.IsStatic;
            this.IsAutoGenerated = rhs.IsAutoGenerated;
            this.IsAssocOperator = rhs.IsAssocOperator;
            this.IsAutoGeneratedThisProc = IsAutoGeneratedThisProc;
            this.IsBuiltIn = rhs.IsBuiltIn;
        }

        public override string ToString()
        {
            string str = IsStatic ? "static " : "";
            return string.Format("{0}def {1} : {2}{3}", str, Name, VarDeclNode.ToString(ReturnType), Singnature);
        }

        //only compare return type, attributes and signature
        public override bool Compare(Node other)
        {
            if (other is FunctionDefinitionNode)
            {
                FunctionDefinitionNode otherNode = other as FunctionDefinitionNode;
                bool result = this.ReturnType.Equals(otherNode.ReturnType) && (this.Singnature.Compare(otherNode.Singnature));

                if (this.Attributes.Count != otherNode.Attributes.Count)
                    return false;

                bool attributeCompare = true;
                for (int i = 0; i < this.Attributes.Count; i++)
                { 
                    attributeCompare = attributeCompare && this.Attributes[i].Compare(otherNode.Attributes[i]);
                }
                result = result && attributeCompare;
                return result;
            }
            else
                return false;
        }
    }

    public class IfStatementNode : AssociativeNode
    {
        public AssociativeNode ifExprNode { get; set; }
        public List<AssociativeNode> IfBody { get; set; }
        public List<AssociativeNode> ElseBody { get; set; }

        public override bool Compare(Node other)
        {
            if (other is IfStatementNode)
            {
                IfStatementNode otherNode = other as IfStatementNode;
                bool result = true;
                result = this.ifExprNode.Compare(otherNode.ifExprNode);

                if (this.IfBody.Count != otherNode.IfBody.Count || this.ElseBody.Count != otherNode.ElseBody.Count)
                    return false;

                bool ifBodyCompare = true;
                for (int i = 0; i < this.IfBody.Count; i++)
                {
                    ifBodyCompare = ifBodyCompare && (this.IfBody[i].Compare(otherNode.IfBody[i]));
                }

                bool elseBodyCompare = true;
                for (int i = 0; i < this.ElseBody.Count; i++)
                {
                    elseBodyCompare = elseBodyCompare && (this.ElseBody[i].Compare(otherNode.ElseBody[i]));
                }
                result = result && ifBodyCompare && elseBodyCompare;
                return result;
            }
            else
                return false;
        }
    }

    public class InlineConditionalNode : AssociativeNode
    {
        public AssociativeNode ConditionExpression { get; set; }
        public AssociativeNode TrueExpression { get; set; }
        public AssociativeNode FalseExpression { get; set; }
        public bool IsAutoGenerated { get; set; }

        public InlineConditionalNode()
        {
            IsAutoGenerated = false;
        }


        public InlineConditionalNode(InlineConditionalNode rhs) : base(rhs)
        {
            IsAutoGenerated = false;
            ConditionExpression = NodeUtils.Clone(rhs.ConditionExpression);
            TrueExpression = NodeUtils.Clone(rhs.TrueExpression);
            FalseExpression = NodeUtils.Clone(rhs.FalseExpression);
        }

        public override bool Compare(Node other)
        {
            if (other is InlineConditionalNode)
            {
                InlineConditionalNode otherNode = other as InlineConditionalNode;
                return this.ConditionExpression.Compare(otherNode.ConditionExpression) && (this.TrueExpression.Compare(otherNode.TrueExpression))
                    && (this.FalseExpression.Compare(otherNode.FalseExpression));
            }
            else
                return false;
        }
    }

    public class BinaryExpressionNode : AssociativeNode
    {
        public int exprUID { get; set; }
        public int modBlkUID { get; set; }
        public bool isSSAAssignment { get; set; }
        public bool isSSAPointerAssignment { get; set; }
        public bool isSSAFirstAssignment { get; set; }
        public bool isMultipleAssign { get; set; }
        public AssociativeNode LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public AssociativeNode RightNode { get; set; }

        // These properties are used only for the GraphUI ProtoAST
        public uint Guid { get; set; }
        //private uint splitFromUID = 0;
        //public uint SplitFromUID { get { return splitFromUID; } set { splitFromUID = value; } }

        public BinaryExpressionNode(AssociativeNode left = null, AssociativeNode right = null, ProtoCore.DSASM.Operator optr = DSASM.Operator.none)
        {
            isSSAAssignment = false;
            isSSAPointerAssignment = false;
            isSSAFirstAssignment = false;
            isMultipleAssign = false;
            exprUID = ProtoCore.DSASM.Constants.kInvalidIndex;
            modBlkUID = ProtoCore.DSASM.Constants.kInvalidIndex;
            LeftNode = left;
            Optr = optr;
            RightNode = right;
        }

        public BinaryExpressionNode(BinaryExpressionNode rhs) : base(rhs)
        {
            isSSAAssignment = rhs.isSSAAssignment;
            isSSAPointerAssignment = rhs.isSSAPointerAssignment;
            isSSAFirstAssignment = rhs.isSSAFirstAssignment;
            isMultipleAssign = rhs.isMultipleAssign;
            exprUID = rhs.exprUID;
            modBlkUID = rhs.modBlkUID;

            Optr = rhs.Optr;
            LeftNode = NodeUtils.Clone(rhs.LeftNode);
            RightNode = null;
            if (null != rhs.RightNode)
            {
                RightNode = NodeUtils.Clone(rhs.RightNode);
            }
        }

        public override bool Compare(Node other)
        {
            if (other is BinaryExpressionNode)
            {
                BinaryExpressionNode otherNode = other as BinaryExpressionNode;
                return (this.LeftNode.Compare(otherNode.LeftNode) && this.Optr == otherNode.Optr && this.RightNode.Compare(otherNode.RightNode));
            }
            else
                return false;
        }
    }

    public class UnaryExpressionNode : AssociativeNode
    {
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
        public AssociativeNode Expression { get; set; }

        public override bool Compare(Node other)
        {
            if (other is UnaryExpressionNode)
            {
                UnaryExpressionNode otherNode = other as UnaryExpressionNode;
                return (this.Operator == otherNode.Operator) && (this.Expression.Compare(otherNode.Expression));
            }
            else
                return false;
        }
    }


    public class ModifierStackNode : AssociativeNode
    {
        public ModifierStackNode()
        {
            ElementNodes = new List<AssociativeNode>();
        }

        public ModifierStackNode(ModifierStackNode rhs)
            : base(rhs)
        {
            ElementNodes = new List<AssociativeNode>();
            foreach (AssociativeNode elemNode in rhs.ElementNodes)
            {
                AssociativeNode tempNode = NodeUtils.Clone(elemNode);
                ElementNodes.Add(tempNode);
            }

            ReturnNode = null;
            if (null != rhs.ReturnNode)
            {
                ReturnNode = ProtoCore.Utils.NodeUtils.Clone(rhs.ReturnNode);
            }
        }

        public IdentifierNode CreateIdentifierNode(Token token, AssociativeNode leftNode)
        {
            if (null == token || (string.IsNullOrEmpty(token.val)))
                return null;

            IdentifierNode leftIdentifier = leftNode as IdentifierNode;
            if (null == leftIdentifier)
                return null;

            IdentifierNode identNode = new IdentifierNode
            {
                Value = token.val,
                Name = token.val,
                datatype = leftIdentifier.datatype
            };

            ProtoCore.Utils.NodeUtils.SetNodeLocation(identNode, token);
            return identNode;
        }

        public IdentifierNode CreateIdentifierNode(AssociativeNode leftNode, ProtoLanguage.CompileStateTracker compileState)
        {
            IdentifierNode leftIdentifier = leftNode as IdentifierNode;
            if (null == leftIdentifier)
                return null;

            string modifierName = leftIdentifier.Name;
            string stackName = compileState.GetModifierBlockTemp(modifierName);
            IdentifierNode identNode = new IdentifierNode
            {
                Value = stackName,
                Name = stackName,
                datatype = leftIdentifier.datatype
            };

            return identNode;
        }

        public BinaryExpressionNode AddElementNode(AssociativeNode n, IdentifierNode identNode)
        {
            BinaryExpressionNode o = n as BinaryExpressionNode;
            BinaryExpressionNode elementNode = new BinaryExpressionNode();

            elementNode.LeftNode = identNode;
            elementNode.RightNode = o.RightNode;
            elementNode.Optr = ProtoCore.DSASM.Operator.assign;

            if (ProtoCore.DSASM.Constants.kInvalidIndex == identNode.line)
            {
                // If the identifier was created as a temp, then we don't have the 
                // corresponding source code location. In that case we'll just use 
                // the entire "RightNode" as the location indicator.
                NodeUtils.CopyNodeLocation(elementNode, elementNode.RightNode);
            }
            else
            {
                // If we do have the name explicitly specified, then we have just 
                // the right location we're after. Only catch here is, for the case 
                // of "foo() => a2", the "RightNode" would have been "foo()" and 
                // the "LeftNode" would have been "a2". So in order to set the 
                // right start and end column, we need these two swapped.
                NodeUtils.SetNodeLocation(elementNode, elementNode.RightNode, elementNode.LeftNode);
            }

            ElementNodes.Add(elementNode);
            return elementNode;

            // TODO: For TP1 we are temporarily updating the modifier block variable 
            // only for its final state instead of updating it for each of its states
            // (which is what we eventually wish to do). This is in order to make MB's behave
            // properly when assigned to class properties - pratapa
            /*BinaryExpressionNode e2 = new BinaryExpressionNode();
            e2.LeftNode = o.LeftNode as IdentifierNode;
            e2.RightNode = e1.LeftNode;
            e2.Optr = ProtoCore.DSASM.Operator.assign;
            ElementNodes.Add(e2);*/
        }

        public List<AssociativeNode> ElementNodes { get; private set; }
        public AssociativeNode ReturnNode { get; set; }

        public override bool Compare(Node other)
        {
            if (other is ModifierStackNode)
            {
                ModifierStackNode otherNode = other as ModifierStackNode;
                bool result = true;
                result = this.ReturnNode.Compare(otherNode.ReturnNode);

                bool elementCompare = true;
                if (this.ElementNodes.Count != otherNode.ElementNodes.Count)
                    return false;

                for (int i = 0; i < this.ElementNodes.Count; i++)
                {
                    elementCompare = elementCompare && (this.ElementNodes[i].Compare(otherNode.ElementNodes[i]));
                }
                result = result && elementCompare;
                return result;
            }
            else
                return false;
        }
    }
    public class RangeExprNode : ArrayNameNode
    {
        public AssociativeNode FromNode { get; set; }
        public AssociativeNode ToNode { get; set; }
        public AssociativeNode StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }

        public RangeExprNode()
        {
        }

        public RangeExprNode(RangeExprNode rhs) : base(rhs)
        {
            FromNode = ProtoCore.Utils.NodeUtils.Clone(rhs.FromNode);
            ToNode = ProtoCore.Utils.NodeUtils.Clone(rhs.ToNode);

            // A step can be optional
            if (null != rhs.StepNode)
            {
                StepNode = ProtoCore.Utils.NodeUtils.Clone(rhs.StepNode);
            }
            stepoperator = rhs.stepoperator;
        }

        public override bool Compare(Node other)
        {
            if (other is RangeExprNode)
            {
                bool result = true;
                RangeExprNode otherNode = other as RangeExprNode;
                result = result && this.FromNode.Compare(otherNode.FromNode) && (this.ToNode.Compare(otherNode.ToNode)) &&
                    (this.stepoperator == otherNode.stepoperator);
                result = result && ((this.StepNode == otherNode.StepNode) || this.StepNode.Compare(otherNode.StepNode));
                return result;
            }
            else
                return false;
        }
    }

    public class ExprListNode : ArrayNameNode
    {
        public ExprListNode()
        {
            list = new List<AssociativeNode>();
        }

        public ExprListNode(ExprListNode rhs) : base(rhs)
        {
            list = new List<AssociativeNode>();
            foreach (AssociativeNode argNode in rhs.list)
            {
                AssociativeNode tempNode = NodeUtils.Clone(argNode);
                list.Add(tempNode);
            }
        }

        public List<AssociativeNode> list { get; set; }

        public override bool Compare(Node other)
        {
            if (other is ExprListNode)
            {
                ExprListNode otherNode = other as ExprListNode;
                bool result = true;
                if (otherNode.list.Count != this.list.Count)
                    return false;

                for (int i = 0; i < list.Count; i++)
                {
                    result = result && list[i].Compare(otherNode.list[i]);
                }
                return result;
            }
            else
                return false;
        }
    }

    public class ForLoopNode : AssociativeNode
    {
        public AssociativeNode loopVar { get; set; }
        public AssociativeNode expression { get; set; }
        public List<AssociativeNode> body { get; set; }

        public override bool Compare(Node other)
        {
            if (other is ForLoopNode)
            {
                ForLoopNode otherNode = other as ForLoopNode;
                bool result = true;
                result = result && loopVar.Compare(otherNode.loopVar) && expression.Compare(otherNode.expression);

                bool bodyCompare = true;
                if (otherNode.body.Count != this.body.Count)
                    return false;
                for (int i = 0; i < body.Count; i++)
                {
                    bodyCompare = this.body[i].Compare(otherNode.body[i]);
                }
                result = result && bodyCompare;
                return result;
            }
            else
                return false;
        }
    }

    public class ArrayNode : AssociativeNode
    {
        public ArrayNode()
        {
            Expr = null;
            Type = null;
        }

        public ArrayNode(AssociativeNode expr, AssociativeNode type)
        {
            Expr = expr;
            Type = type;
        }

        public ArrayNode(ArrayNode rhs) : base(rhs)
        {
            Expr = null;
            Type = null;
            if (null != rhs)
            {
                if (null != rhs.Expr)
                {
                    Expr = NodeUtils.Clone(rhs.Expr);
                }

                if (null != rhs.Type)
                {
                    Type = NodeUtils.Clone(rhs.Type);
                }
            }
        }

        public AssociativeNode Expr { get; set; }
        public AssociativeNode Type { get; set; }

        public override bool Compare(Node other)
        {
            if (other is ArrayNode)
            {
                ArrayNode otherNode = other as ArrayNode;
                return (this.Expr.Compare(otherNode.Expr) && (this.Type == otherNode.Type || this.Type.Compare(otherNode.Type)));
            }
            else
                return false;
        }
    }

    public class ImportNode : AssociativeNode
    {
        public ImportNode()
        {
            HasBeenImported = false;
            Identifiers = new HashSet<string>();
        }

        public CodeBlockNode CodeNode { get; set; }
        public bool HasBeenImported { get; set; }
        public HashSet<string> Identifiers { get; set; }
        public string ModuleName { get; set; }

        private string modulePathFileName;
        public string ModulePathFileName
        {
            get
            {
                return modulePathFileName;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    modulePathFileName = ProtoCore.Utils.FileUtils.GetFullPathName(value.Replace("\"", String.Empty));
            }
        }

        public override bool Compare(Node other)
        {
            if (other is ImportNode)
            {
                ImportNode otherNode = other as ImportNode;
                bool result = true;
                result = result && this.CodeNode.Compare(otherNode.CodeNode);
                result = result && (this.Identifiers.Equals(otherNode.Identifiers));
                result = result && this.ModuleName == otherNode.ModuleName;
                result = result && this.modulePathFileName == otherNode.modulePathFileName;
                return result;
            }
            else
                return false;
        }
    }

    public class PostFixNode : AssociativeNode
    {
        public AssociativeNode Identifier { get; set; }
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }

        public override bool Compare(Node other)
        {
            if (other is PostFixNode)
            {
                PostFixNode otherNode = other as PostFixNode;
                return this.Identifier.Compare(otherNode.Identifier) && (this.Operator == otherNode.Operator);
            }
            else
                return false;
        }
    }

    public class BreakNode : AssociativeNode
    {
    }

    public class ContinueNode : AssociativeNode
    {
    }

    public class DefaultArgNode : AssociativeNode
    {// not supposed to be used in parser 
    }

    public class DynamicNode : AssociativeNode
    {
        public DynamicNode()
        {
        }

        public DynamicNode(DynamicNode rhs) : base(rhs)
        {
        }
    }

    public class DynamicBlockNode : AssociativeNode
    {
        public int block { get; set; }
        public DynamicBlockNode(int blockId = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            block = blockId;
        }

        public override bool Compare(Node other)
        {
            if (other is DynamicBlockNode)
                return this.block == (other as DynamicBlockNode).block;
            else
                return false;
        }
    }

    public class DotFunctionBodyNode : AssociativeNode
    {
        public AssociativeNode leftNode { get; set; }
        public AssociativeNode rightNode { get; set; }
        public AssociativeNode rightNodeDimExprList { get; set; }
        public AssociativeNode rightNodeDim { get; set; }
        public AssociativeNode rightNodeArgList { get; set; }
        public AssociativeNode rightNodeArgNum { get; set; }
        public DotFunctionBodyNode(AssociativeNode lhs, AssociativeNode rhs, AssociativeNode dimExprList, AssociativeNode dim, AssociativeNode rhsArgList = null, AssociativeNode rhsArgNum = null)
        {
            leftNode = lhs;
            rightNode = rhs;
            rightNodeDimExprList = dimExprList;
            rightNodeDim = dim;
            rightNodeArgList = rhsArgList;
            rightNodeArgNum = rhsArgNum;
        }

        public override bool Compare(Node other)
        {
            if (other is DotFunctionBodyNode)
            {
                DotFunctionBodyNode otherNode = other as DotFunctionBodyNode;
                bool result = true;
                result = result && this.leftNode.Compare(otherNode.leftNode);
                result = result && this.rightNode.Compare(otherNode.rightNode);
                result = result && this.rightNodeDimExprList.Compare(otherNode.rightNodeDimExprList);
                result = result && this.rightNodeDim.Compare(otherNode.rightNodeDim);
                result = result && this.rightNodeArgList.Compare(otherNode.rightNodeArgList);
                result = result && this.rightNodeArgNum.Compare(otherNode.rightNodeArgNum);
                return result;
            }
            else
                return false;
        }
    }

    public class ThisPointerNode : AssociativeNode
    {
        public ThisPointerNode()
        {
        }

        public ThisPointerNode(ThisPointerNode rhs) : base (rhs)
        {
        }
    }

    public class ThrowNode : AssociativeNode
    {
        public AssociativeNode expression { get; set; }

        public override bool Compare(Node other)
        {
            if (other is ThrowNode)
            {
                return this.expression.Compare((other as ThrowNode).expression);
            }
            else
                return false;
        }
    }

    public class TryBlockNode : AssociativeNode
    {
        public List<AssociativeNode> body { get; set; }

        public override bool Compare(Node other)
        {
            if (other is TryBlockNode)
            {
                bool result = true;
                TryBlockNode otherNode = other as TryBlockNode;
                if (otherNode.body.Count != this.body.Count)
                    return false;

                for (int i = 0; i < body.Count; i++)
                {
                    result = result && this.body[i].Compare(otherNode.body[i]);
                }
                return result;
            }
            else
                return false;
        }
    }

    public class CatchFilterNode : AssociativeNode
    {
        public IdentifierNode var { get; set; }
        public ProtoCore.Type type { get; set; }

        public override bool Compare(Node other)
        {
            if (other is CatchFilterNode)
            {
                CatchFilterNode otherNode = other as CatchFilterNode;
                return this.var.Compare(otherNode.var) && (this.type.Equals(otherNode.type));
            }
            else
                return false;
        }
    }

    public class CatchBlockNode : AssociativeNode
    {
        public CatchFilterNode catchFilter { get; set; }
        public List<AssociativeNode> body { get; set; }

        public override bool Compare(Node other)
        {
            if (other is CatchBlockNode)
            {
                CatchBlockNode otherNode = other as CatchBlockNode;
                bool result = true;
                result = result && this.catchFilter.Compare(otherNode.catchFilter);

                if (this.body.Count != otherNode.body.Count)
                    return false;

                bool bodyCompare = true;
                for (int i = 0; i < body.Count; i++)
                {
                    bodyCompare = bodyCompare && (this.body[i].Compare(otherNode.body[i]));
                }
                result = result && bodyCompare;
                return result;
            }
            else
                return false;
        }
    }

    public class ExceptionHandlingNode : AssociativeNode
    {
        public TryBlockNode tryBlock { get; set; }
        public List<CatchBlockNode> catchBlocks { get; set; }

        public ExceptionHandlingNode()
        {
            catchBlocks = new List<CatchBlockNode>();
        }

        public override bool Compare(Node other)
        {
            if (other is ExceptionHandlingNode)
            {
                ExceptionHandlingNode otherNode = other as ExceptionHandlingNode;
                bool result = true;
                result = result && this.tryBlock.Compare(otherNode.tryBlock);

                if (this.catchBlocks.Count != otherNode.catchBlocks.Count)
                    return false;

                bool catchBlockCompare = true;
                for (int i = 0; i < catchBlocks.Count; i++)
                {
                    catchBlockCompare = catchBlockCompare && (this.catchBlocks[i].Compare(otherNode.catchBlocks[i]));
                }
                result = result && catchBlockCompare;
                return result;
            }
            else
                return false;
        }
    }
}
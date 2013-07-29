
#define TEST_DIRECT

using System;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoCore;

namespace DesignScript.Parser.Associative
{
    public abstract class AssociativeNode : DesignScript.Parser.Node
    {
        //private static int sID = 0;
        //allow the assignment node to be part of dependency struture?
        //this lead to entiely different set of results in optimization
        //protected static bool AssignNodeDependencyEnabled = true;

        //even if the '=' is not a link between LHS and RHS, can we keep it in dependency graph?
        //protected static bool AssignNodeDependencyEnabledLame = true;

        /* public virtual void GenerateDependencyGraph(DependencyTracker tracker)
         {
             tracker.AddNode(this);//get rid of this later

             IEnumerable<Node> contingents = getContingents();

             foreach (Node node in contingents)
             {
                 tracker.AddNode(node);
                 if (node == null)
                     continue;
                 tracker.AddDirectContingent(this, node);
                 tracker.AddDirectDependent(node, this);
                 node.GenerateDependencyGraph(tracker);
             }
         }*/

        //public virtual IEnumerable<Node> getContingents()
        //{
        //    return new List<Node>();
        //}

        //public virtual void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        //{
        //}

        //protected static void Consolidate(ref Dictionary<string, List<Node>> names, ref IdentifierNode node)
        //{
        //    if (null != node.Name)
        //    {
        //        if (names.ContainsKey(node.Name))
        //        {
        //            List<Node> candidates = names[node.Name];
        //            node = candidates[candidates.Count - 1] as IdentifierNode;
        //        }
        //        else
        //        {
        //            //symbol not defined.
        //            //should we ignore this until somebody else defines a symbol? 
        //            //or add the symbol?
        //            //throw new KeyNotFoundException();
        //            List<Node> candidates = new List<Node>();
        //            candidates.Add(node);
        //            names.Add(node.Name, candidates);
        //        }
        //    }
        //}
    }

    public class IDEHelpNode : AssociativeNode
    {
        public IDEHelpNode(NodeType _type)
        { Type = _type; Value = null; }

        public enum NodeType { PunctuationNode, IdentNode, TypeNode, KeywordNode, NumberNode, TextNode}
        public NodeType Type { get; set; }
        public string Value { get; set; }

        public void SetValue(string _value, int _line, int _col)
        { Value = _value; Line = _line; Col = _col; }
    }

    public class CommentNode : AssociativeNode
    {
        public enum CommentType { Inline, Block }
        public CommentType Type { get; private set; }
        public string Value { get; private set; }
        public CommentNode(int col, int line, string value, CommentType type)
        {
            Col = col;
            Line = line;
            Value = value;
            Type = type;
        }
    }

    public class LanguageBlockNode : AssociativeNode
    {
        public LanguageBlockNode()
        {
            languageblock = new LanguageCodeBlock(); 
            openBracket = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            language = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            closeBracket = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            property = new List<IDEHelpNode>();
            propertyValue = new List<StringNode>();
            openBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            closeBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }

        public ProtoCore.LanguageCodeBlock languageblock;
        public Node code { get; set; }

        public void AddProperty(string _comma, int comma_line, int comma_col, string _name, int name_line, int name_col, string _assign, int assign_line, int assign_col, StringNode _value)
        {
            property.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _comma, Line = comma_line, Col = comma_col });
            property.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _assign, Line = assign_line, Col = assign_col });
            property.Add(new IDEHelpNode(IDEHelpNode.NodeType.IdentNode) { Value = _name, Line = name_line, Col = name_col });
            propertyValue.Add(_value);
        }
        public IDEHelpNode openBracket { get; set; }
        public IDEHelpNode language { get; set; }
        public IDEHelpNode closeBracket { get; set; }
        public List<IDEHelpNode> property { get; private set; }
        public List<StringNode> propertyValue { get; private set; }
        public IDEHelpNode openBrace { get; set; }
        public IDEHelpNode closeBrace { get; set; }


    }

    /// <summary>
    /// This node will be used by the optimiser
    /// </summary>
    public class MergeNode : AssociativeNode
    {
        public List<Node> MergedNodes
        {
            get;
            private set;
        }

        public MergeNode()
        {
            MergedNodes = new List<Node>();
        }

        //public override IEnumerable<Node> getContingents()
        //{
        //    return MergedNodes;
        //}
        /* public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
         {
             foreach (Node node in MergedNodes)
                 node.ConsolidateNames(ref(names));
         }*/

    }

    public class ReplicationGuideNode : AssociativeNode
    {
        public ReplicationGuideNode()
        {
            brackets = new List<IDEHelpNode>();
        }
        public List<Node> ReplicationGuides { get; set; }
        public List<IDEHelpNode> brackets { get; set; }
        public Node factor { get; set; }
        public void AddBrackets(string _open, int open_line, int open_col, string _num, int num_line, int num_col, string _close, int close_line, int close_col)
        {
            brackets.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _open, Line = open_line, Col = open_col });
            brackets.Add(new IDEHelpNode(IDEHelpNode.NodeType.NumberNode) { Value = _num, Line = num_line, Col = num_col });
            brackets.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _close, Line = close_line, Col = close_col });
        }
        

        public IDEHelpNode openParen { get; set; }
        public IDEHelpNode closeParen { get; set; }
    }

    public class IdentifierNode : AssociativeNode
    {
        public IdentifierNode()
        {
            
            IdentValueReturn = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            IdentValue = new IDEHelpNode(IDEHelpNode.NodeType.IdentNode);
            colon = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            typeName = new IDEHelpNode(IDEHelpNode.NodeType.TypeNode);
            typeName_kw = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
        }

        public IDEHelpNode IdentValue { get; set; }

        public IDEHelpNode IdentValueReturn { get; set; }

        public ArrayNode ArrayDimensions
        {
            get;
            set;
        }

        public IDEHelpNode colon { get; set; }
        public IDEHelpNode typeName { get; set; }
        public IDEHelpNode typeName_kw { get; set; }
        public void BuildInTypeSetValue(string _value, int _line, int _col)
        {
            typeName_kw = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode) { Value = _value, Line = _line, Col = _col };
        }
        public void UserDefinedTypeSetValue(string _value, int _line, int _col)
        {
            typeName = new IDEHelpNode(IDEHelpNode.NodeType.TypeNode) { Value = _value, Line = _line, Col = _col };
        }
        //public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        //{
        //    throw new NotImplementedException(); //we should not be here at all. the parent node should take care.
        //    //disabling execption as functioncalls will still need to add the nodes to 
        //}
    }

    public class IdentifierListNode : AssociativeNode
    {
        public IdentifierListNode()
        {
            InitializeIDEHelpNode();
        }
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


        void InitializeIDEHelpNode()
        {
            dot = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode dot { get; set; }

        public Node RightNode
        {
            get;
            set;
        }
    }

    public class IntNode : AssociativeNode
    {
        public IntNode()
        { sign = new IDEHelpNode(IDEHelpNode.NodeType.NumberNode); IDEValue = new IDEHelpNode(IDEHelpNode.NodeType.NumberNode); }
        public IDEHelpNode sign { get; set; }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class DoubleNode : AssociativeNode
    {
         public DoubleNode()
        { sign = new IDEHelpNode(IDEHelpNode.NodeType.NumberNode); IDEValue = new IDEHelpNode(IDEHelpNode.NodeType.NumberNode);  }
        public IDEHelpNode sign { get; set; }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class BooleanNode : AssociativeNode
    {
        public string value { get; set; }
    }

    public class CharNode : AssociativeNode
    {
        public CharNode()
        {
            IDEValue = new IDEHelpNode(IDEHelpNode.NodeType.TextNode);
        }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class StringNode : AssociativeNode
    {
        public StringNode()
        {
            IDEValue = new IDEHelpNode(IDEHelpNode.NodeType.TextNode); 
        }
        public IDEHelpNode IDEValue { get; set; }
        public string value { get; set; }
    }

    public class NullNode : AssociativeNode
    {
        public string value { get; set; }
    }

    public class ReturnNode : AssociativeNode
    {
        public Node ReturnExpr
        {
            get;
            set;
        }
    }

    public class FunctionCallNode : AssociativeNode
    {
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
            InitializeIDEHelpNode();
        }

        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>(FormalArguments);
        //    contingents.Add(Function);
        //    return contingents;
        //}
        /*
        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            List<Node> newFormalArguments = new List<Node>();
            //replace the names in arguments by current names in calling context
            foreach (Node argument in FormalArguments)
            {
                Node newArgument = argument;
                IdentifierNode terminalNode = newArgument as IdentifierNode;
                if (terminalNode != null)
                {
                    Consolidate(ref(names), ref(terminalNode));
                    newArgument = terminalNode;
                }
                else
                {
                    argument.ConsolidateNames(ref(names));
                }
                newFormalArguments.Add(newArgument);
            }
            FormalArguments = newFormalArguments;
        }*/

        public void AddComma(string _value, int _line, int _col)
        {
            comma.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _value, Line = _line, Col = _col });
        }
        void InitializeIDEHelpNode()
        {
            openParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            closeParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            comma = new List<IDEHelpNode>();
        }
        public IDEHelpNode openParen { get; set; }
        public IDEHelpNode closeParen { get; set; }
        public List<IDEHelpNode> comma { get; set; }
    }

    public class Pattern : AssociativeNode
    {
        public Pattern()
        {
            InitializeIDEHelpNode();
        }
        public Node Expression
        {
            get;
            set;
        }

        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>(1);
        //    contingents.Add(Expression);
        //    return contingents;
        //}
        /*
        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            Expression.ConsolidateNames(ref(names));
        }*/

        void InitializeIDEHelpNode()
        {
            bitOr = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode bitOr { get; set; }


    }

    public class QualifiedNode : AssociativeNode
    {
        public Node Value
        {
            get;
            set;
        }

        public List<Node> ReplicationGuides
        {
            get;
            set;
        }

        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>(ReplicationGuides);
        //    contingents.Add(Value);
        //    return contingents;
        //}
        /*
        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            Value.ConsolidateNames(ref(names));
        }
        */
    }

    public class VarDeclNode : AssociativeNode
    {
        public VarDeclNode()
        {
            name = new IDEHelpNode(IDEHelpNode.NodeType.IdentNode);
            equal = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            KwStatic = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
        }

        //public ProtoCore.DSASM.MemoryRegion memregion
        //{
        //    get;
        //    set;
        //}

        //public ProtoCore.Type ArgumentType
        //{
        //    get;
        //    set;
        //}

        public Node NameNode
        {
            get;
            set;
        }

        ////public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        ////{
        ////    if (names.ContainsKey(NameNode.Name))
        ////        throw new Exception();
        ////    List<Node> records = new List<Node>();
        ////    records.Add(NameNode);
        ////    names.Add(NameNode.Name, records);

        ////    Dictionary<string, List<Node>> localnames = new Dictionary<string, List<Node>>();
        ////    localnames.Add(NameNode.Name, records);
        //}

        public IDEHelpNode KwStatic { get; set; }
        public IDEHelpNode name { get; set; }
        public TypeNode IDEArgumentType { get; set; }
        public IDEHelpNode equal { get; set; }
    }

    public class ArgumentSignatureNode : AssociativeNode
    {
        public ArgumentSignatureNode()
        {
            Arguments = new List<Node>();
            InitializeHelpNode();
        }

        public List<Node> Arguments
        {
            get;
            set;
        }

        public void AddArgument(Node arg)
        {
            Arguments.Add(arg);
        }

        //public List<KeyValuePair<ProtoCore.Type, Pattern>> ArgumentStructure
        //{
        //    get
        //    {
        //        List<KeyValuePair<ProtoCore.Type, Pattern>> argStructure = new List<KeyValuePair<ProtoCore.Type, Pattern>>();
        //        foreach (VarDeclNode i in Arguments)
        //        {
        //            argStructure.Add(new KeyValuePair<ProtoCore.Type, Pattern>(i.ArgumentType, i.Pattern));
        //        }
        //        return argStructure;
        //    }
        //}

        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>(Arguments);
        //    return contingents;
        //}

        //public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        //{
        //    foreach (Node node in Arguments)
        //        node.ConsolidateNames(ref(names));
        //}

        public void AddComma(string _comma, int _line, int _col)
        {
            comma.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _comma, Line = _line, Col = _col });
        }
        void InitializeHelpNode()
        {
            openBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            comma = new List<IDEHelpNode>();
            closeBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode openBrace { get; set; }
        public List<IDEHelpNode> comma { get; private set; }
        public IDEHelpNode closeBrace { get; set; }



    }

    public class CodeBlockNode : AssociativeNode
    {
        public ProtoCore.DSASM.SymbolTable symbols { get; set; }
        public ProtoCore.DSASM.ProcedureTable procTable { get; set; }

        public CodeBlockNode()
        {
            Body = new List<Node>();
            symbols = new ProtoCore.DSASM.SymbolTable("AST generated", ProtoCore.DSASM.Constants.kInvalidIndex);
            //procTable = new ProtoCore.DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kInvalidIndex);
            InitializeIDEHelpNode();
        }

        public List<Node> Body
        {
            get;
            set;
        }

        //public override IEnumerable<Node> getContingents()
        //{
        //    return new List<Node>(Body);
        //}

        //public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        //{
        //    //todo make a decision whether to pass out the local names.
        //    foreach (Node node in Body)
        //    {
        //        node.ConsolidateNames(ref(names));
        //    }
        //}


        void InitializeIDEHelpNode()
        {
            openBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            closeBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode openBrace { get; set; }
        public IDEHelpNode closeBrace { get; set; }
    }

    public class ClassDeclNode : AssociativeNode
    {
        public ClassDeclNode()
        {
            varlist = new List<Node>();
            funclist = new List<Node>();
            InitializeIDEHelpNode();
        }

        // utilities added to store the pos info of tokens, for IDE use
        void InitializeIDEHelpNode()
        {
            Kwclass = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            ClassName = new IDEHelpNode(IDEHelpNode.NodeType.TypeNode);
            SuperClass = new List<IDEHelpNode>();
            Kwextend = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            openBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            closeBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            VarDeclCommas = new List<IDEHelpNode>();
            accessLabel = new List<IDEHelpNode>();
        }
        public void AddExt(string _name, int _line, int _col)
        {
            SuperClass.Add(new IDEHelpNode(IDEHelpNode.NodeType.TypeNode) { Value = _name, Type = IDEHelpNode.NodeType.TypeNode, Line = _line, Col = _col });
        }
        public void AddVarDeclComma(string _value, int _line, int _col)
        {
            VarDeclCommas.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _value, Col = _col, Line = _line });
        }

        public void AddAccessLabel(string _value, int _line, int _col)
        {
            accessLabel.Add(new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode) { Value = _value, Line = _line, Col = _col });
        }
       
        public IDEHelpNode Kwclass { get; set; }
        public IDEHelpNode ClassName { get; set; }
        public IDEHelpNode Kwextend { get; set; }
        public List<IDEHelpNode> SuperClass { get; private set; }
        public IDEHelpNode openBrace { get; set; }
        public IDEHelpNode closeBrace { get; set; }
        public List<IDEHelpNode> VarDeclCommas { get; private set; }
        public List<IDEHelpNode> accessLabel { get; set; }
        public List<Node> varlist { get; set; }
        public List<Node> funclist { get; set; }
    }

    public class ConstructorDefinitionNode : AssociativeNode
    {
        private DesignScript.Parser.Associative.FunctionCallNode baseConstr = null;

        public ConstructorDefinitionNode()
        {
            InitializeIDEHelpNode();
        }
        public ArgumentSignatureNode Signature
        {
            get;
            set;
        }
        public Pattern Pattern
        {
            get;
            set;
        }
        public TypeNode IDEReturnType { get; set; }
        public CodeBlockNode FunctionBody
        {
            get;
            set;
        }
        void InitializeIDEHelpNode()
        {
            Kwconstructor = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            name = new IDEHelpNode(IDEHelpNode.NodeType.IdentNode);
            KwStatic = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            KwBase = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
        }
        public void CreateBaseConstructorNode()
        {
            this.baseConstr = new DesignScript.Parser.Associative.FunctionCallNode();
        }
        public void SetBaseConstructor(Node bnode)
        {
            if(null != this.baseConstr)
                this.baseConstr.Function = bnode;
        }
        public void SetColonToken(Token token)
        {
            this.Colon = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            this.Colon.SetValue(token.val, token.line, token.col);
        }
        public void SetDotToken(Token token)
        {
            this.Dot = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            this.Dot.SetValue(token.val, token.line, token.col);
        }

        public IDEHelpNode KwStatic { get; set; }
        public IDEHelpNode Kwconstructor { get; set; }
        public IDEHelpNode name { get; set; }
        public IDEHelpNode Colon { get; set; }
        public IDEHelpNode Dot { get; set; }
        public IDEHelpNode KwBase { get; set; }
        public DesignScript.Parser.Associative.FunctionCallNode BaseConstructorNode { get { return this.baseConstr; } }
    }

    public class TypeNode : AssociativeNode
    {

        public TypeNode()
        {
            InitializeIDEHelpNode();
        }
        void InitializeIDEHelpNode()
        {
            colon = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            brackets = new List<IDEHelpNode>();
            multiDim = new List<IDEHelpNode>();
            op = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public void AddBracket(string _open, int open_line, int open_col, string _close, int close_line, int close_col)
        {
            brackets.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _open, Col = open_col, Line = open_line });
            brackets.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _close, Col = close_col, Line = close_line });
        }
        public void AddMultiDimNodes(string _mdopen, int mdopen_line, int mdopen_col, string _mdclose, int mdclose_line, int mdclose_col)
        {
            multiDim.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _mdopen, Col = mdopen_col, Line = mdopen_line });
            multiDim.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _mdclose, Col = mdclose_col, Line = mdclose_line });
        }
        public void BuildInTypeSetValue(string _value, int _line, int _col)
        {
            typeName = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode) { Value = _value, Line = _line, Col = _col };
        }
        public void UserDefinedTypeSetValue(string _value, int _line, int _col)
        {
            typeName = new IDEHelpNode(IDEHelpNode.NodeType.TypeNode) { Value = _value, Line = _line, Col = _col };
        }
        public IDEHelpNode colon { get; set; }
        public IDEHelpNode typeName { get; set; }
        public IDEHelpNode varName { get; set; }
        public IDEHelpNode op { get; set; }
        public List<IDEHelpNode> brackets { get; set; }
        public List<IDEHelpNode> multiDim { get; set; }
    }

    public class FunctionDefinitionNode : AssociativeNode
    {
        public FunctionDefinitionNode()
        {
            InitializeIDEHelpNode();
        }

        public CodeBlockNode FunctionBody
        {
            get;
            set;
        }

        public TypeNode IDEReturnType { get; set; }

        public ProtoCore.Type ReturnType
        {
            get;
            set;
        }
        public ArgumentSignatureNode Singnature
        {
            get;
            set;
        }
        public Node Pattern
        {
            get;
            set;
        }
        public bool IsExternLib
        {
            get;
            set;
        }
        public bool IsDNI
        {
            get;
            set;
        }
        public string ExternLibName
        {
            get;
            set;
        }
        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>();
        //    contingents.Add(FunctionBody);
        //    contingents.Add(Singnature);
        //    contingents.Add(Pattern);
        //    return contingents;
        //}

        //public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        //{
        //    Dictionary<string, List<Node>> localNames = new Dictionary<string, List<Node>>();
        //    Singnature.ConsolidateNames(ref(localNames));
        //    //Pattern.ConsolidateNames(ref(localNames));
        //    FunctionBody.ConsolidateNames(ref(localNames));
        //    if (names.ContainsKey(Name))
        //    {
        //        throw new Exception();
        //    }
        //    List<Node> namelist = new List<Node>();
        //    namelist.Add(this);
        //    names.Add(Name, namelist);
        //}

        void InitializeIDEHelpNode()
        {
            KwStatic = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            Kwexternal = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            Kwnative = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            libOpenBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            libCloseBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            Kwdef = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            name = new IDEHelpNode(IDEHelpNode.NodeType.IdentNode);
            endLine = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode KwStatic { get; set; }
        public IDEHelpNode Kwexternal { get; set; }
        public IDEHelpNode Kwnative { get; set; }
        public IDEHelpNode libOpenBrace { get; set; }
        public IDEHelpNode libOpenQuote { get; set; }
        public IDEHelpNode libCloseQuote { get; set; }
        public StringNode libName { get; set; }
        public IDEHelpNode libCloseBrace { get; set; }
        public IDEHelpNode Kwdef { get; set; }
        public IDEHelpNode name { get; set; }
        public IDEHelpNode endLine { get; set; }

    }

    public class BinaryExpressionNode : AssociativeNode
    {
        public BinaryExpressionNode()
        {
            op = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }

        public Node LeftNode
        {
            get;
            set;
        }

        public Node RightNode
        {
            get;
            set;
        }
        public IDEHelpNode op { get; set; }

        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>();
        //    if (Optr != ProtoCore.DSASM.Operator.assign)
        //    {
        //        contingents.Add(LeftNode);
        //    }
        //    //if we have enabled the '=' node to be a part of depencency, then we return RHS, no matter what
        //    if (AssignNodeDependencyEnabled || Optr != ProtoCore.DSASM.Operator.assign)
        //    {
        //        contingents.Add(RightNode);
        //    }
        //    return contingents;
        //}


        //public override void GenerateDependencyGraph(DependencyTracker tracker)
        //{
        //    base.GenerateDependencyGraph(tracker);
        //    if (Optr == ProtoCore.DSASM.Operator.assign)
        //    {
        //        //so do we set dependency between LeftNode and '=' or LeftNode and RightNode : may be later is better
        //        if (AssignNodeDependencyEnabled)
        //        {
        //            //if we have enabled the '=' node to be a part of depencency, then we already handled RHS as a contingent
        //            //so skip it
        //            tracker.AddNode(LeftNode);
        //            tracker.AddDirectContingent(LeftNode, this);
        //            tracker.AddDirectDependent(this, LeftNode);
        //        }
        //        else
        //        {
        //            //if(AssignNodeDependencyEnabledLame)
        //            //{
        //            //    tracker.AddDirectContingent(this, RightNode);  //? still keep in dependency?
        //            //    tracker.AddDirectContingent(LeftNode, RightNode); 
        //            //}
        //            tracker.AddNode(RightNode);
        //            tracker.AddNode(LeftNode);
        //            tracker.AddDirectContingent(LeftNode, RightNode);
        //            tracker.AddDirectDependent(RightNode, LeftNode);
        //            RightNode.GenerateDependencyGraph(tracker);
        //        }
        //    }
        //}


        //    public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        //    {
        //        IdentifierNode rightTerminalNode = RightNode as IdentifierNode;
        //        if (rightTerminalNode != null)
        //        {
        //            if (Optr != ProtoCore.DSASM.Operator.dot)
        //            {
        //                //replace RHS
        //                Consolidate(ref(names), ref(rightTerminalNode));
        //                RightNode = rightTerminalNode;
        //            }
        //        }
        //        else
        //        {
        //            RightNode.ConsolidateNames(ref(names));
        //        }

        //        //left has to be done 2nd, because in case of modifiers, we dont want to 
        //        //replace the node on RHS by a node on LHS. So a modifier stack name is not unique.
        //        IdentifierNode leftTerminalNode = LeftNode as IdentifierNode;
        //        if (leftTerminalNode != null)
        //        {
        //            if (Optr != ProtoCore.DSASM.Operator.assign)
        //            {
        //                //replace LHS
        //                Consolidate(ref(names), ref(leftTerminalNode));
        //                LeftNode = leftTerminalNode;
        //            }
        //            else
        //            {
        //                if (leftTerminalNode.Name != null)
        //                {
        //                    if (names.ContainsKey(leftTerminalNode.Name))
        //                    {
        //                        List<Node> candidates = names[leftTerminalNode.Name];
        //                        candidates.Add(leftTerminalNode);
        //                    }
        //                    else
        //                    {
        //                        //append LHS
        //                        List<Node> candidates = new List<Node>();
        //                        candidates.Add(leftTerminalNode);
        //                        names.Add(leftTerminalNode.Name, candidates);
        //                    }
        //                }

        //            }
        //        }
        //        else
        //        {
        //            LeftNode.ConsolidateNames(ref(names));
        //        }
        //    }

        //}
    }

    public class ParenExpressionNode : AssociativeNode
    {
        public ParenExpressionNode()
        {
            openParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            closeParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }

        public Node expression { get; set; }
        public IDEHelpNode openParen { get; set; }
        public IDEHelpNode closeParen { get; set; }
    }

    public class UnaryExpressionNode : AssociativeNode
    {
        public UnaryExpressionNode()
        {
            op = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        
        public Node Expression
        {
            get;
            set;
        }

        public IDEHelpNode op { get; set; }

        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>(1);
        //    contingents.Add(Expression);
        //    return contingents;
        //}

        //public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        //{
        //    Expression.ConsolidateNames(ref(names));
        //}
    }

    public class InlineConditionalNode : AssociativeNode
    {
        public InlineConditionalNode()
        {
            Question = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            Colon = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode Question { get; set; }
        public IDEHelpNode Colon { get; set; }
        public Node ConditionExpression { get; set; }
        public Node TrueExpression { get; set; }
        public Node FalseExpression { get; set; }
    }

    public class ModifierStackNode : AssociativeNode
    {
        public ModifierStackNode()
        {
            ElementNodes = new List<Node>();
            AtNames = new Dictionary<string, Node>();
            InitializeIDEHelpNode();
        }

        public void AddEndLine(string _value, int _line, int _col)
        {
            endline.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _value, Col = _col, Line = _line });
        }
        public void AddArrow(string _arr_value, int _arr_line, int _arr_col, string _label_value, int _label_line, int _label_col)
        {
            arrow.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _arr_value, Line = _arr_line, Col = _arr_col });
            arrow.Add(new IDEHelpNode(IDEHelpNode.NodeType.IdentNode) { Value = _label_value, Line = _label_line, Col = _label_col });
        }
        void InitializeIDEHelpNode()
        {
            openSharpBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            endline = new List<IDEHelpNode>();
            arrow = new List<IDEHelpNode>();
            closeBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode openSharpBrace { get; set; }
        public List<IDEHelpNode> endline { get; private set; }
        public List<IDEHelpNode> arrow { get; private set; }
        public IDEHelpNode closeBrace { get; set; }


        public void AddElementNode(Node n, string name)
        {
            ElementNodes.Add(n);
            if (!string.IsNullOrEmpty(name))
            {
                if (!AtNames.ContainsKey(name))
                {
                    AtNames.Add(name, n);
                    BinaryExpressionNode o = n as BinaryExpressionNode;
                    IdentifierNode t = o.LeftNode as IdentifierNode;
                    BinaryExpressionNode e = new BinaryExpressionNode();
                    e.LeftNode = new IdentifierNode() { Name = name };
                    e.RightNode = t;
                    ElementNodes.Add(e);
                }
            }
        }

        public List<Node> ElementNodes
        {
            get;
            private set;
        }

        public Node ReturnNode
        {
            get;
            set;
        }

        public Dictionary<string, Node> AtNames
        {
            get;
            private set;
        }

        //public override IEnumerable<Node> getContingents()
        //{
        //    List<Node> contingents = new List<Node>(ElementNodes);
        //    contingents.Add(ReturnNode);
        //    return contingents;
        //}
    }

    public class RangeExprNode : AssociativeNode
    {
        public RangeExprNode()
        {
            IntNode defaultStep = new IntNode();
            defaultStep.value = "1";
            StepNode = defaultStep;
            InitializeIDEHelpNode();
        }

        public Node FromNode { get; set; }
        public Node ToNode { get; set; }
        public Node StepNode { get; set; }

        void InitializeIDEHelpNode()
        {
            op = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            stepOp = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            stepOp2 = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode op { get; set; }
        public IDEHelpNode stepOp { get; set; }
        public IDEHelpNode stepOp2 { get; set; }

    }

    public class ExprListNode : AssociativeNode
    {
        public ExprListNode()
        {
            list = new List<Node>();
            InitializeIDEHelpNode();
        }

        public List<Node> list
        {
            get;
            set;
        }

        public void AddComma(string _value, int _line, int _col)
        {
            comma.Add(new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode) { Value = _value, Line = _line, Col = _col });
        }
        void InitializeIDEHelpNode()
        {
            openBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            comma = new List<IDEHelpNode>();
            closeBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode openBrace { get; set; }
        public List<IDEHelpNode> comma { get; private set; }
        public IDEHelpNode closeBrace { get; set; }
    }

    public class ForLoopNode : AssociativeNode
    {
        public ForLoopNode()
        {
            InitializeIDEHelpNode();
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

        void InitializeIDEHelpNode()
        {
            Kwfor = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            openParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            Kwin = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            closeParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            openBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            closeBrace = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode Kwfor { get; set; }
        public IDEHelpNode openParen { get; set; }
        public IDEHelpNode Kwin { get; set; }
        public IDEHelpNode closeParen { get; set; }
        public IDEHelpNode openBrace { get; set; }
        public IDEHelpNode closeBrace { get; set; }
    }

    public class StatementNode : AssociativeNode
    {
        public StatementNode()
        {
            Statement = null;
            endLine = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }

        public Node Statement { get; set; }
        public IDEHelpNode endLine { get; set; }
    }

    public class ArrayNode : AssociativeNode
    {
        public ArrayNode()
        {
            Expr = null;
            Ident = null;
            Type = null;
            InitializeIDEHelpNode();
        }
        public Node Ident { get; set; }
        public Node Expr { get; set; }
        public Node Type { get; set; }

        void InitializeIDEHelpNode()
        {
            openBracket = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            closeBracket = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode openBracket { get; set; }
        public IDEHelpNode closeBracket { get; set; }
    }

    public class ImportNode : AssociativeNode
    {
        public ImportNode()
        {
            OpenParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            CloseParen = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            KwImport = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            KwFrom = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            Identifier = new IDEHelpNode(IDEHelpNode.NodeType.IdentNode);
            KwPrefix = new IDEHelpNode(IDEHelpNode.NodeType.KeywordNode);
            PrefixIdent = new IDEHelpNode(IDEHelpNode.NodeType.IdentNode);
            EndLine = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
            HasBeenImported = false;
        }

        public CodeBlockNode CodeNode { get; set; }
        public bool HasBeenImported { get; set; }

        public IDEHelpNode OpenParen { get; set; }
        public IDEHelpNode CloseParen { get; set; }
        public IDEHelpNode KwImport { get; set; }
        public IDEHelpNode KwFrom { get; set; }
        public IDEHelpNode KwPrefix { get; set; }
        public IDEHelpNode PrefixIdent { get; set; }
        public StringNode Path { get; set; }
        public IDEHelpNode Identifier { get; set; }
        public IDEHelpNode EndLine { get; set; }
    }

    public class PostFixNode : AssociativeNode
    {
        public PostFixNode()
        {
            OperatorPos = new IDEHelpNode(IDEHelpNode.NodeType.PunctuationNode);
        }
        public IDEHelpNode OperatorPos { get; set; }
        public Node Identifier { get; set; }
    }
}


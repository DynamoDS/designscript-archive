using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.AST.AssociativeAST;

namespace DesignScriptStudio.Graph.Core
{
    class Statement : ISerializable
    {
        private enum Flags
        {
            None = 0x00000000,
            Swappable = 0x00000001,
            Complex = 0x00000002,
            FuncDefinition = 0x00000004,
            ClassDefinition = 0x00000008,
        }

        #region Public Interface Methods

        public virtual bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage (58D0A328653F)"); // @TODO(done): GUID?
            if (storage.ReadUnsignedInteger(FieldCode.StatementSignature) != Configurations.StatementSignature)
                throw new InvalidOperationException("Invalid input data (8404892D29B6)"); // @TODO(done): GUID?

            this.flags = (Flags)storage.ReadInteger(FieldCode.StatementFlag);
            this.definedVariable = storage.ReadString(FieldCode.DefinedVariable);
            this.outputExpression = ReadVariableSlotInfo(storage);

            this.references.Clear();
            int referenceCount = storage.ReadInteger(FieldCode.ReferencesCount);
            for (int i = 0; i < referenceCount; i++)
                this.references.Add(ReadVariableSlotInfo(storage));

            // @TODO(Sean): Write Serialize/Deserialize test cases with and without children.
            this.children.Clear();
            int childrenCount = storage.ReadInteger(FieldCode.ChildrenCount);
            for (int i = 0; i < childrenCount; i++)
                this.children.Add(new Statement(storage));

            return true;
        }

        public virtual bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            storage.WriteUnsignedInteger(FieldCode.StatementSignature, Configurations.StatementSignature);
            storage.WriteInteger(FieldCode.StatementFlag, (int)this.flags);
            storage.WriteString(FieldCode.DefinedVariable, this.definedVariable);
            WriteVariableSlotInfo(storage, this.outputExpression);

            storage.WriteInteger(FieldCode.ReferencesCount, this.references.Count);
            foreach (VariableSlotInfo reference in this.references)
                WriteVariableSlotInfo(storage, reference);

            // @TODO(Sean): Write Serialize/Deserialize test cases with and without children.
            storage.WriteInteger(FieldCode.ChildrenCount, this.children.Count);
            foreach (Statement childStatement in this.children)
                childStatement.Serialize(storage);

            return true;
        }

        public virtual AuditStatus Audit()
        {
            return AuditStatus.NoChange;
        }

        #endregion

        #region Public/Internal Methods

        internal Statement(IStorage storage)
        {
        }

        internal Statement(ProtoCore.AST.Node astNode)
        {
            if (astNode is BinaryExpressionNode)
                Initialize((BinaryExpressionNode)astNode);
            else if (astNode is ClassDeclNode)
                Initialize((ClassDeclNode)astNode);
            else if (astNode is FunctionDefinitionNode)
                Initialize((FunctionDefinitionNode)astNode);
            else
            {
                string type = (astNode == null ? "null" : astNode.GetType().ToString());
                string message = string.Format("AST Node '{0}' is not supported (C71AB50DA47C)", type);
                throw new NotImplementedException(message); // @TODO(Done): GUID?
            }
        }

        internal void SetOutputSlotId(uint slotId)
        {
            // @TODO(Sean): Exception if output already has a slot id.
        }

        internal void GetReferenceVariables(List<string> referencedVars, bool includeTemp)
        {
            foreach (VariableSlotInfo reference in this.references)
            {
                if (includeTemp || (!Utilities.IsTempVariable(reference.Variable)))
                    referencedVars.Add(reference.Variable);
            }
        }

        #endregion

        #region Public Class Properties

        internal bool IsSwappable
        {
            get { return flags.HasFlag(Flags.Swappable); }
        }
        internal bool IsComplex
        {
            get { return flags.HasFlag(Flags.Complex); }
        }
        internal string DefinedVariable
        {
            get { return this.definedVariable; }
        }
        internal VariableSlotInfo OutputExpression
        {
            get { return this.outputExpression; }
        }
        internal List<VariableSlotInfo> References
        {
            get { return this.references; }
        }
        internal List<Statement> Children
        {
            get { return this.children; }
        }

        #endregion

        #region Private Helper Methods

        private VariableSlotInfo ReadVariableSlotInfo(IStorage storage)
        {
            string variable = storage.ReadString(FieldCode.VariableSlotInfoVar);
            int line = storage.ReadInteger(FieldCode.VariableSlotInfoLine);
            uint slotId = storage.ReadUnsignedInteger(FieldCode.VariableSlotInfoSlotId);
            return new VariableSlotInfo(variable, line, slotId);
        }

        private bool WriteVariableSlotInfo(IStorage storage, VariableSlotInfo variableSlotInfo)
        {
            storage.WriteString(FieldCode.VariableSlotInfoVar, variableSlotInfo.Variable);
            storage.WriteInteger(FieldCode.VariableSlotInfoLine, variableSlotInfo.Line);
            storage.WriteUnsignedInteger(FieldCode.VariableSlotInfoSlotId, variableSlotInfo.SlotId);
            return true;
        }

        private void Initialize(BinaryExpressionNode binaryExpressionNode)
        {
            this.definedVariable = GetTopLevelVariable(binaryExpressionNode.LeftNode);
            this.outputExpression = GetOutputExpression(binaryExpressionNode.LeftNode);
            AddReferencesFromAst(binaryExpressionNode.RightNode);

            UpdateSwappableFlag(binaryExpressionNode);
            UpdateComplexFlag(binaryExpressionNode);
        }

        private void Initialize(FunctionDefinitionNode functionDefinitionNode)
        {
        }

        private void Initialize(ClassDeclNode classDeclNode)
        {
        }

        private void UpdateSwappableFlag(BinaryExpressionNode binaryNode)
        {
            flags &= ~Flags.Swappable;
            if (binaryNode.RightNode is IdentifierListNode || binaryNode.RightNode is ProtoCore.AST.AssociativeAST.IdentifierNode)
                flags |= Flags.Swappable;
        }

        private void UpdateComplexFlag(AssociativeNode astNode)
        {
            flags |= Flags.Complex;
            BinaryExpressionNode binaryExpression = astNode as BinaryExpressionNode;
            if (binaryExpression != null && (binaryExpression.RightNode != null))
            {
                AssociativeNode rightHandSide = binaryExpression.RightNode;
                if (IsPrimitiveType(rightHandSide))
                    flags &= ~Flags.Complex;
            }
        }

        private string GetTopLevelVariable(AssociativeNode astNode)
        {
            if (astNode is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                ProtoCore.AST.AssociativeAST.IdentifierNode identifierNode = (ProtoCore.AST.AssociativeAST.IdentifierNode)astNode;
                return ((ProtoCore.AST.AssociativeAST.IdentifierNode)astNode).Value;
            }
            else if (astNode is IdentifierListNode)
            {
                IdentifierListNode identListNode = (IdentifierListNode)astNode;
                return GetTopLevelVariable(identListNode.LeftNode);
            }
            else
            {
                throw new InvalidOperationException("Input is not IdentifierNode or IdentifierListNode");
            }
        }

        private VariableSlotInfo GetOutputExpression(AssociativeNode astNode)
        {
            string variable = GetExpressionFromNode(astNode);
            int line = astNode.line;
            uint slot = uint.MaxValue;
            return new VariableSlotInfo(variable, line, slot);
        }

        private string GetExpressionFromNode(AssociativeNode astNode)
        {
            List<AssociativeNode> astNodes = new List<AssociativeNode>();
            astNodes.Add(astNode);
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astNodes);
            return codegen.GenerateCode();
        }

        private void AddReferencesFromAst(AssociativeNode astNode)
        {
            if (IsPrimitiveType(astNode))
                return;

            ProtoCore.AST.AssociativeAST.IdentifierNode identNode = astNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
            if (identNode != null)
            {
                this.references.Add(GetVariableSlotInfo(identNode));
                return;
            }

            FunctionDotCallNode funcDotCallNode = astNode as FunctionDotCallNode;
            if (funcDotCallNode != null)
            {
                // @TODO(done): Avoid hard casting.
                this.references.Add(GetVariableSlotInfo(funcDotCallNode));
                return;
            }

            FunctionCallNode functionCallNode = (FunctionCallNode)astNode;
            if (funcDotCallNode != null)
            {
                // @TODO(done): Avoid hard casting.
                foreach (AssociativeNode node in functionCallNode.FormalArguments)
                    AddReferencesFromAst(node);
                return;
            }

            // @TODO(Sean): Print out type info to help yourself.
            throw new ArgumentException("Unsupported node type  (79CCBEA9F50E)", "astNode");
        }

        private VariableSlotInfo GetVariableSlotInfo(ProtoCore.AST.AssociativeAST.IdentifierNode identifierNode)
        {
            string var = identifierNode.Value;
            int line = identifierNode.line;
            uint slot = uint.MaxValue;
            return new VariableSlotInfo(var, line, slot);
        }

        private VariableSlotInfo GetVariableSlotInfo(FunctionDotCallNode functionDotCallNode)
        {
            // @TODO(done): No hard-casting.
            ProtoCore.AST.AssociativeAST.IdentifierNode idenNode = functionDotCallNode.DotCall.FormalArguments[0] as ProtoCore.AST.AssociativeAST.IdentifierNode;

            if (idenNode == null)
                throw new InvalidOperationException("Unsupported AST Node Type");

            string var = idenNode.Value;
            int line = functionDotCallNode.line;
            uint slot = uint.MaxValue;
            return new VariableSlotInfo(var, line, slot);
        }

        private bool IsPrimitiveType(AssociativeNode astNode)
        {
            if (astNode == null)
                throw new ArgumentNullException("astNode");

            if (astNode is IntNode || astNode is DoubleNode)
                return true;
            return false;
        }

        #endregion

        #region Class Data Members

        private List<VariableSlotInfo> references = new List<VariableSlotInfo>();
        private VariableSlotInfo outputExpression;
        private List<Statement> children = new List<Statement>();
        private string definedVariable = string.Empty;
        private Flags flags = Flags.None;

        #endregion
    }
}

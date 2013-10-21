using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.AST;
using GraphToDSCompiler;
using Ui = DesignScriptStudio.Graph.Core;
using DesignScriptStudio.Graph.Core;
using System.IO;

namespace DesignScriptStudio.Tests.UnitTests
{
    class StatementTest
    {
        [Test]
        public void TestConstructor1()
        {
            string code = "a=1;";
            Dictionary<int, List<VariableLine>> unboundIdentifiers = null;
            List<ProtoCore.AST.Node> astNodes = null;
            ProtoCore.BuildStatus buildStatus;
            GraphUtilities.ParseCodeBlockNodeStatements(code, out unboundIdentifiers, out astNodes, out buildStatus);

            Ui.Statement statement = new Ui.Statement(astNodes[0]);

            VariableSlotInfo outputExpression = new VariableSlotInfo("a", 1, uint.MaxValue);
            Assert.AreEqual("a", statement.DefinedVariable);
            Assert.AreEqual(outputExpression, statement.OutputExpression);
            Assert.AreEqual(0, statement.References.Count);
            Assert.AreEqual(0, statement.Children.Count);
            Assert.AreEqual(false, statement.IsSwappable);
            Assert.AreEqual(false, statement.IsComplex);
        }

        [Test]
        public void TestConstructor2()
        {
            string code = "a=b;";
            Dictionary<int, List<VariableLine>> unboundIdentifiers = null;
            List<ProtoCore.AST.Node> astNodes = null;
            ProtoCore.BuildStatus buildStatus;
            GraphUtilities.ParseCodeBlockNodeStatements(code, out unboundIdentifiers, out astNodes, out buildStatus);

            Ui.Statement statement = new Ui.Statement(astNodes[0]);

            VariableSlotInfo outputExpression = new VariableSlotInfo("a", 1, uint.MaxValue);
            VariableSlotInfo reference = new VariableSlotInfo("b", 1, uint.MaxValue);

            Assert.AreEqual("a", statement.DefinedVariable);
            Assert.AreEqual(outputExpression, statement.OutputExpression);
            Assert.AreEqual(1, statement.References.Count);
            Assert.AreEqual(reference, statement.References[0]);
            Assert.AreEqual(0, statement.Children.Count);
            Assert.AreEqual(true, statement.IsSwappable);
            Assert.AreEqual(true, statement.IsComplex);
        }

        [Test]
        public void TestConstructor3()
        {
            string code = "a[1][2] = 1;";
            Dictionary<int, List<VariableLine>> unboundIdentifiers = null;
            List<ProtoCore.AST.Node> astNodes = null;
            ProtoCore.BuildStatus buildStatus;
            GraphUtilities.ParseCodeBlockNodeStatements(code, out unboundIdentifiers, out astNodes, out buildStatus);

            Ui.Statement statement = new Ui.Statement(astNodes[0]);

            VariableSlotInfo outputExpression = new VariableSlotInfo("a[1][2].x[2].p", 1, uint.MaxValue);
            Assert.AreEqual("a", statement.DefinedVariable);
            Assert.AreEqual(outputExpression, statement.OutputExpression);
            Assert.AreEqual(0, statement.References.Count);
            Assert.AreEqual(0, statement.Children.Count);
            Assert.AreEqual(false, statement.IsSwappable);
            Assert.AreEqual(false, statement.IsComplex);
        }

        [Test]
        public void TestDeserialize1()
        {
            string code = "a=1;";
            Dictionary<int, List<VariableLine>> unboundIdentifiers = null;
            List<ProtoCore.AST.Node> astNodes = null;
            ProtoCore.BuildStatus buildStatus;
            GraphUtilities.ParseCodeBlockNodeStatements(code, out unboundIdentifiers, out astNodes, out buildStatus);

            Ui.Statement statement = new Ui.Statement(astNodes[0]);

            IStorage storage = new BinaryStorage();
            statement.Serialize(storage);
            Ui.Statement newStatement = new Ui.Statement(storage);
            storage.Seek(0, SeekOrigin.Begin);
            newStatement.Deserialize(storage);

            VariableSlotInfo outputExpression = new VariableSlotInfo("a", 1, uint.MaxValue);
            Assert.AreEqual("a", statement.DefinedVariable);
            Assert.AreEqual(outputExpression, statement.OutputExpression);
            Assert.AreEqual(0, statement.References.Count);
            Assert.AreEqual(0, statement.Children.Count);
            Assert.AreEqual(false, statement.IsSwappable);
            Assert.AreEqual(false, statement.IsComplex);
        }

        [Test]
        public void TestDeserialize2()
        {
            string code = "a=b;";
            Dictionary<int, List<VariableLine>> unboundIdentifiers = null;
            List<ProtoCore.AST.Node> astNodes = null;
            ProtoCore.BuildStatus buildStatus;
            GraphUtilities.ParseCodeBlockNodeStatements(code, out unboundIdentifiers, out astNodes, out buildStatus);

            Ui.Statement statement = new Ui.Statement(astNodes[0]);

            IStorage storage = new BinaryStorage();
            statement.Serialize(storage);
            Ui.Statement newStatement = new Ui.Statement(storage);
            storage.Seek(0, SeekOrigin.Begin);
            newStatement.Deserialize(storage);

            VariableSlotInfo outputExpression = new VariableSlotInfo("a", 1, uint.MaxValue);
            VariableSlotInfo reference = new VariableSlotInfo("b", 1, uint.MaxValue);

            Assert.AreEqual("a", statement.DefinedVariable);
            Assert.AreEqual(outputExpression, statement.OutputExpression);
            Assert.AreEqual(1, statement.References.Count);
            Assert.AreEqual(reference, statement.References[0]);
            Assert.AreEqual(0, statement.Children.Count);
            Assert.AreEqual(true, statement.IsSwappable);
            Assert.AreEqual(true, statement.IsComplex);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class GraphCommandTests
    {
        [Test]
        public void TestToStringNoArgument()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.UndoOperation);
            Assert.AreEqual("UndoOperation", command.ToString());
        }

        [Test]
        public void TestToStringWithArguments()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.UndoOperation);
            command.AppendArgument(1234);
            command.AppendArgument(56.78);
            command.AppendArgument(true);
            command.AppendArgument("DesignScript Rocks!");
            Assert.AreEqual("UndoOperation|i:1234|d:56.78|b:True|s:DesignScript Rocks!", command.ToString());
        }

        [Test]
        public void TestFromStringNoArgument()
        {
            GraphCommand command = GraphCommand.FromString("UndoOperation");
            Assert.IsNotNull(command);
            Assert.AreEqual(GraphCommand.Name.UndoOperation, command.CommandName);
        }

        [Test]
        public void TestFromStringWithArguments()
        {
            GraphCommand command = GraphCommand.FromString("UndoOperation|i:1234|d:56.78|b:True|s:DesignScript Rocks!");
            Assert.IsNotNull(command);
            Assert.AreEqual(GraphCommand.Name.UndoOperation, command.CommandName);
            Assert.AreEqual(1234, command.GetArgument(0));
            Assert.AreEqual(56.78, command.GetArgument(1));
            Assert.AreEqual(true, command.GetArgument(2));
            Assert.AreEqual("DesignScript Rocks!", command.GetArgument(3));
        }

        [Test]
        public void TestToStringForEnum()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.UndoOperation);
            command.AppendArgument(NodeType.Function);
            command.AppendArgument("Math.Sin");
            command.AppendArgument(12.34);
            command.AppendArgument(56.78);
            Assert.AreEqual("UndoOperation|e:DesignScriptStudio.Graph.Core.NodeType,Function|s:Math.Sin|d:12.34|d:56.78", command.ToString());
        }

        [Test]
        public void TestFromStringForEnum()
        {
            GraphCommand command = GraphCommand.FromString("CreateFunctionNode|e:DesignScriptStudio.Graph.Core.NodeType,Function|s:Math.Sin|d:12.34|d:56.78");
            Assert.IsNotNull(command);
            Assert.AreEqual(GraphCommand.Name.CreateFunctionNode, command.CommandName);
            Assert.AreEqual(NodeType.Function, command.GetArgument(0));
            Assert.AreEqual("Math.Sin", command.GetArgument(1));
            Assert.AreEqual(12.34, command.GetArgument(2));
            Assert.AreEqual(56.78, command.GetArgument(3));
        }

        [Test]
        public void TestToStringForUnsigned()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.UndoOperation);
            command.AppendArgument("Math.Sin");
            command.AppendArgument(12.34);
            command.AppendArgument((uint)5678);
            Assert.AreEqual("UndoOperation|s:Math.Sin|d:12.34|u:0x0000162e", command.ToString());
        }

        [Test]
        public void TestFromStringForUnsigned()
        {
            GraphCommand command = GraphCommand.FromString("UndoOperation|s:Math.Sin|d:12.34|u:0x0000162e");
            Assert.IsNotNull(command);
            Assert.AreEqual(GraphCommand.Name.UndoOperation, command.CommandName);
            Assert.AreEqual("Math.Sin", command.GetArgument(0));
            Assert.AreEqual(12.34, command.GetArgument(1));
            Assert.AreEqual(5678, command.GetArgument(2));
        }

        [Test]
        public void TestFromStringWithEmptyArgs()
        {
            GraphCommand command = GraphCommand.FromString("UndoOperation|i:1234|d:56.78|b:True|s:");
            Assert.IsNotNull(command);
            Assert.AreEqual(GraphCommand.Name.UndoOperation, command.CommandName);
            Assert.AreEqual(1234, command.GetArgument(0));
            Assert.AreEqual(56.78, command.GetArgument(1));
            Assert.AreEqual(true, command.GetArgument(2));
            Assert.AreEqual(String.Empty, command.GetArgument(3));
        }

        [Test]
        public void TestToStringWithLineBreak()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.EndNodeEdit);
            command.AppendArgument((uint)0x10000001);
            command.AppendArgument("a=3;\nb=4;");
            command.AppendArgument(true);
            Assert.AreEqual("EndNodeEdit|u:0x10000001|s:a=3;\\nb=4;|b:True", command.ToString());
        }

        [Test]
        public void TestFromStringWithLineBreak()
        {
            GraphCommand command = GraphCommand.FromString("EndNodeEdit|u:0x10000001|s:a=3;\\nb=4;|b:True");
            Assert.IsNotNull(command);
            Assert.AreEqual(GraphCommand.Name.EndNodeEdit, command.CommandName);
            Assert.AreEqual(0x10000001, command.GetArgument(0));
            Assert.AreEqual("a=3;\nb=4;", command.GetArgument(1));
            Assert.AreEqual(true, command.GetArgument(2));
        }
    }
}

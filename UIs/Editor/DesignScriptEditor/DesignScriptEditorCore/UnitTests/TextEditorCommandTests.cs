using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Core.UnitTest
{
    class TextEditorCommandTests
    {
        private TextEditorCommand textEditorCommand = null;

        [SetUp]
        public void Setup()
        {
            textEditorCommand = new TextEditorCommand();
        }

        [Test]
        public void TestAppendArgument()
        {

            string obj1 = "1";
            string obj2 = "2";
            Object obj3 = new Object();
            obj3 = "3";
            textEditorCommand.AppendArgument(obj1);
            textEditorCommand.AppendArgument(obj2);
            textEditorCommand.AppendArgument(obj3);
            Assert.AreEqual("1", textEditorCommand.Arguments[0]);
            Assert.AreEqual("2", textEditorCommand.Arguments[1]);
            Assert.AreEqual("3", textEditorCommand.Arguments[2]);
        }

        [Test]
        public void TestAppendAsserts()
        {
            List<CommandAssert> commandAsserts = new List<CommandAssert>(3);
            CommandAssert obj = new CommandAssert("a", "1");
            CommandAssert obj1 = new CommandAssert("b", "2");
            commandAsserts.Add(obj);
            commandAsserts.Add(obj1);

            textEditorCommand.AppendAsserts(commandAsserts);
            Assert.AreEqual("a", textEditorCommand.Asserts[0].PropertyName);
            Assert.AreEqual("1", textEditorCommand.Asserts[0].PropertyValue);
            Assert.AreEqual("b", textEditorCommand.Asserts[1].PropertyName);
            Assert.AreEqual("2", textEditorCommand.Asserts[1].PropertyValue);
        }

        [Test]
        public void TestMethodName()
        {

            TextEditorCommand textEditorCommand1 = null;
            textEditorCommand1 = new TextEditorCommand(TextEditorCommand.Method.Run);

            Assert.AreEqual(TextEditorCommand.Method.Run, textEditorCommand1.MethodName);
        }

        [Test]
        public void TestModifierModifier()
        {
            Assert.AreEqual((uint)TextEditorCommand.Modifier.None, 0x00000000);
            Assert.AreEqual((uint)TextEditorCommand.Modifier.Shift, 0x00000001);
            Assert.AreEqual((uint)TextEditorCommand.Modifier.Control, 0x00000002);
            Assert.AreEqual((uint)TextEditorCommand.Modifier.Alt, 0x00000004);
            Assert.AreEqual((uint)TextEditorCommand.Modifier.LeftButtonDown, 0x00000008);
            Assert.AreEqual((uint)TextEditorCommand.Modifier.MiddleButtonDown, 0x000000010);
            Assert.AreEqual((uint)TextEditorCommand.Modifier.RightButtonDown, 0x000000020);
        }

        [Test]
        public void TestIndividualModifier()
        {
            textEditorCommand.Modifiers = ((uint)TextEditorCommand.Modifier.Control);
            Assert.IsTrue(textEditorCommand.IsControlKeyDown);
            Assert.IsFalse(textEditorCommand.IsShiftKeyDown);

            textEditorCommand.Modifiers = ((uint)TextEditorCommand.Modifier.Alt);
            Assert.IsTrue(textEditorCommand.IsAltKeyDown);
            Assert.IsFalse(textEditorCommand.IsShiftKeyDown);

            textEditorCommand.Modifiers = ((uint)TextEditorCommand.Modifier.Shift);
            Assert.IsTrue(textEditorCommand.IsShiftKeyDown);
            Assert.IsFalse(textEditorCommand.IsAltKeyDown);

            textEditorCommand.Modifiers = ((uint)TextEditorCommand.Modifier.LeftButtonDown);
            Assert.IsTrue(textEditorCommand.IsLeftButtonDown);
            Assert.IsFalse(textEditorCommand.IsShiftKeyDown);

            textEditorCommand.Modifiers = ((uint)TextEditorCommand.Modifier.MiddleButtonDown);
            Assert.IsTrue(textEditorCommand.IsMiddleButtonDown);
            Assert.IsFalse(textEditorCommand.IsShiftKeyDown);

            textEditorCommand.Modifiers = ((uint)TextEditorCommand.Modifier.RightButtonDown);
            Assert.IsTrue(textEditorCommand.IsRightButtonDown);
            Assert.IsFalse(textEditorCommand.IsShiftKeyDown);

            textEditorCommand.Modifiers = ((uint)TextEditorCommand.Modifier.None);
            Assert.IsFalse(textEditorCommand.IsRightButtonDown);
            Assert.IsFalse(textEditorCommand.IsShiftKeyDown);
        }

        [Test]
        public void TestControlPlusShiftModifier()
        {
            textEditorCommand.Modifiers = (uint)TextEditorCommand.Modifier.Shift;
            textEditorCommand.Modifiers |= (uint)TextEditorCommand.Modifier.Control;
            Assert.IsTrue(textEditorCommand.IsControlKeyDown);
            Assert.IsTrue(textEditorCommand.IsShiftKeyDown);
            Assert.IsFalse(textEditorCommand.IsMiddleButtonDown);
        }

        [Test]
        public void TestControlPlusShiftPlusAltModifier()
        {
            textEditorCommand.Modifiers = (uint)TextEditorCommand.Modifier.Shift;
            textEditorCommand.Modifiers |= (uint)TextEditorCommand.Modifier.Control;
            textEditorCommand.Modifiers |= (uint)TextEditorCommand.Modifier.Alt;
            Assert.IsTrue(textEditorCommand.IsControlKeyDown);
            Assert.IsTrue(textEditorCommand.IsShiftKeyDown);
            Assert.IsTrue(textEditorCommand.IsAltKeyDown);
            Assert.IsFalse(textEditorCommand.IsMiddleButtonDown);
        }
    }
}

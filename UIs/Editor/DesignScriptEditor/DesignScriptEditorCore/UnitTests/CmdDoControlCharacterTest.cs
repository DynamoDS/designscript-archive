using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Windows.Input;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdDoControlCharacterTest
    {
        public TextEditorCore textCore;
        public Solution testSolution;

        [SetUp]
        public void Setup()
        {
            string fileContent =
                "hello\n" +
                "\n" +
                "\t\tcruel\n" +
                "\tworld";

            testSolution = Solution.CreateTemporary();
            testSolution.AddNewScript(fileContent);
            testSolution.ActivateScript(0);

            textCore = TextEditorCore.CreateTemporary();
            textCore.ChangeScript(0);
        }

        [TearDown]
        public void TearDown()
        {
            TextEditorCore.InvalidateInstance();
        }

        [Test, RequiresSTA]
        public void DoControlBackspaceTest01()
        {
            textCore.SetCursorPosition(0,2);
            textCore.DoControlCharacter(Key.Back);
            Assert.AreEqual(textCore.GetLine(1), "\t\tcruel\n");
            Assert.AreEqual(textCore.LineCount, 3);
        }

        [Test, RequiresSTA]
        public void DoControlBackspaceTest02()
        {
            textCore.SetCursorPosition(0, 1);
            textCore.DoControlCharacter(Key.Back);
            Assert.AreEqual(textCore.GetLine(1), "\t\tcruel\n");
            Assert.AreEqual(textCore.LineCount, 3);
        }

        [Test, RequiresSTA]
        public void DoControlBackspaceTest03()
        {
            textCore.SetCursorPosition(0, 3);
            textCore.DoControlCharacter(Key.Back);
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\tworld");
            Assert.AreEqual(textCore.LineCount, 3);
        }

        [Test, RequiresSTA]
        public void DoControlBackspaceTest04()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.DoControlCharacter(Key.Back);
            Assert.AreEqual(textCore.GetLine(2), "\tcruel\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void DoControlBackspaceTest05()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.DoControlCharacter(Key.Back);
            Assert.AreEqual(textCore.GetLine(0), "hellocruel\n");
            Assert.AreEqual(textCore.LineCount, 2);
        }

        [Test, RequiresSTA]
        public void DoControlBackspaceTest06()
        {
            textCore.SetCursorPosition(0, 0);
            textCore.DoControlCharacter(Key.Back);
            Assert.AreEqual(textCore.GetLine(3), "\tworld");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            Assert.AreEqual(textCore.GetLine(1), "\n");
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void DoControlDeleteTest01()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.DoControlCharacter(Key.Delete);
            textCore.DoControlCharacter(Key.Delete);
            Assert.AreEqual(textCore.GetLine(2), "\t\tuel\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void DoControlDeleteTest02()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Left);
            textCore.DoControlCharacter(Key.Delete);
            Assert.AreEqual(textCore.GetLine(0), "hellocruel\n");
            Assert.AreEqual(textCore.LineCount, 2);
        }

        [Test, RequiresSTA]
        public void DoControlDeleteTest03()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.DoControlCharacter(Key.Delete);
            Assert.AreEqual(textCore.GetLine(3), "\tworld");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            Assert.AreEqual(textCore.GetLine(1), "\n");
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

       
    }
}

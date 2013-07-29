using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Windows.Input;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdCopyPasteTest
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
            textCore.ParseScriptImmediate();
        }

        [TearDown]
        public void TearDown()
        {
            TextEditorCore.InvalidateInstance();
        }

        [Test, RequiresSTA]
        public void TestCopyPaste01()
        {
            textCore.SetCursorPosition(0,0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            Assert.AreEqual(textCore.GetSelectionText(),"hel");
            textCore.DoCopyText(false);
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            textCore.SetCursorPosition(0,3);
            textCore.DoPasteText();
            Assert.AreEqual(textCore.GetLine(3), "hel\tworld");
        }

        [Test, RequiresSTA]
        public void TestCopyPaste02()
        {
            textCore.SetCursorPosition(0, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            Assert.AreEqual(textCore.GetSelectionText(), "hel");
            textCore.DoCopyText(false);
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            textCore.SetCursorPosition(0, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.DoPasteText();
            Assert.AreEqual(textCore.GetLine(2), "hel\tcruel\n");
        }

        [Test, RequiresSTA]
        public void TestCutPaste01()
        {
            textCore.SetCursorPosition(0, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            Assert.AreEqual(textCore.GetSelectionText(), "hel");
            textCore.DoCopyText(true);
            textCore.SetCursorPosition(0, 1);
            textCore.DoPasteText();
            Assert.AreEqual(textCore.GetLine(1), "hel\n");
            Assert.AreEqual(textCore.GetLine(0), "lo\n");
        }

        [Test, RequiresSTA]
        public void TestCutPaste02()
        {
            textCore.SetCursorPosition(0, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            Assert.AreEqual(textCore.GetSelectionText(), "hel");
            textCore.DoCopyText(true);
            textCore.SetCursorPosition(0, 3);
            //Select 3 character to replace from line 3
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.DoNavigation(Key.Right);
            textCore.DoPasteText();
            Assert.AreEqual(textCore.GetLine(3), "helrld");
            Assert.AreEqual(textCore.GetLine(0), "lo\n");
        }
    }
}

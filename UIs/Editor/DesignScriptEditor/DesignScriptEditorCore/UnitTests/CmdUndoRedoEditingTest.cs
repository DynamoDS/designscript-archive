using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdUndoRedoEditingTest
    {
        public TextEditorCore textCore;
        public Solution testSolution;

        [SetUp]
        public void Setup()
        {
            string fileContent =
                "hello\n" +
                "test\n" +
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
        public void TestUndoEditing()
        {
            textCore.SetCursorPosition(3,0);
            textCore.InsertText("testest");
            Assert.AreEqual(textCore.GetLine(0), "heltestestlo\n");
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestRedoEditing()
        {
            textCore.SetCursorPosition(3, 0);
            textCore.InsertText("testest");
            Assert.AreEqual(textCore.GetLine(0), "heltestestlo\n");
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            textCore.RedoEditing();
            Assert.AreEqual(textCore.GetLine(0), "heltestestlo\n");
        }
    }
}

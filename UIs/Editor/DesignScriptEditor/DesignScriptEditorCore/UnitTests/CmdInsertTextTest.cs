using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdInsertTextTest
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
        public void TestInsertText01()
        {
            textCore.SetCursorPosition(0, 0);
            //Cursor Position should be 0,0

            textCore.InsertText("this is now the first line\n");
            Assert.AreEqual(textCore.GetLine(0), "this is now the first line\n");
        }

        [Test, RequiresSTA]
        public void TestInsertText02()
        {
            textCore.SetCursorPosition(3, 2);
            //Cursor Position should be 0,0

            textCore.InsertText("this is now the first line\n");
            Assert.AreEqual(textCore.GetLine(2), "    cthis is now the first line\n");
        }

        [Test, RequiresSTA]
        public void TestInsertText03()
        {
            textCore.SetCursorPosition(8, 2);
            //Cursor Position should be 0,0

            textCore.InsertText("this is now the first line");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruelthis is now the first line\n");
        }
    }
}

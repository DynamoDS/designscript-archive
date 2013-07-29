using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdSetCursorPositionTest
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
        public void TestSetCursorPosition01()
        {
            textCore.SetCursorPosition(0, 0);
            //Cursor Position should be 0,0
            textCore.SetCursorPosition(4, 2);
            Assert.AreEqual(new System.Drawing.Point(4, 2), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void TestSetCursorPosition02()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.SetCursorPosition(5, 3);
            Assert.AreEqual(new System.Drawing.Point(5, 3), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void TestSetCursorPosition03()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.SetCursorPosition(13, 3);
            Assert.AreEqual(new System.Drawing.Point(6, 3), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void TestSetCursorPosition04()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.SetCursorPosition(3, 4);
            Assert.AreEqual(new System.Drawing.Point(3, 3), textCore.CursorPosition);
        }
                
    }
}

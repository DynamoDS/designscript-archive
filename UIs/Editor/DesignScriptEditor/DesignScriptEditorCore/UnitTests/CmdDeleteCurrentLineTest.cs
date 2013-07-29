using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdDeleteCurrentLineTest
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
        public void TestDeleteCurrentLine01()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.DeleteCurrentLine();
            Assert.AreEqual(textCore.GetLine(2), "\tworld".ToCharArray());
        }

        [Test, RequiresSTA]
        public void TestDeleteCurrentLine02()
        {
            textCore.SetCursorPosition(5, 3);
            textCore.DeleteCurrentLine();
            Assert.AreEqual(textCore.GetLine(3), "".ToCharArray());
        }
    }
}

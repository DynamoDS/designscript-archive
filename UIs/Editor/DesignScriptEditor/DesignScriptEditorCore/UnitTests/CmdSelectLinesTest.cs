using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdSelectLinesTest
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
        public void TestSelectLines01()
        {
            textCore.SelectLines(2,2);
            Assert.AreEqual(textCore.GetSelectionText(), "\t\tcruel\n\tworld");
        }

        [Test, RequiresSTA]
        public void TestSelectLines02()
        {
            textCore.SelectLines(2, -2);
            Assert.AreEqual(textCore.GetSelectionText(), "hello\n\n\t\tcruel\n");
        }

        [Test, RequiresSTA]
        public void TestSelectLines03()
        {
            textCore.SelectLines(-1, 5);
            Assert.AreEqual(textCore.GetSelectionText(), "hello\n\n\t\tcruel\n\tworld");
        }

        [Test, RequiresSTA]
        public void TestSelectLines04()
        {
            textCore.SelectLines(2, -3);
            Assert.AreEqual(textCore.GetSelectionText(), "hello\n\n\t\tcruel\n");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdSelectFragmentTest
    {
        public TextEditorCore textCore = null;
        public Solution testSolution = null;

        [SetUp]
        public void Setup()
        {
            string fileContent =
                "// I told the doctor I broke my leg in two places.\n" +
                "// He told me to stop going to those places.\n" +
                "//                               - Henny Youngman\n" +
                "\n" +
                "[Imperative]\n" +
                "{\n" +
                "\t    aVar = 2;\n" +
                "\t    nVar = -4.5678; // Assign value of -4.5678\n" +
                "\t    bVar = aVar;\n" +
                "\t    sVar = (32.7654 + 5) + bVar;\n" +
                "\n" +
                "\t    // An associative block.\n" +
                "\t    [Associative]\n" +
                "\t    {\n" +
                "\t    \tsVar = 3;\n" +
                "\t    }\n" +
                "}\n";

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
        public void TestSelectFragmentDouble()
        {
            // Double-clicking between "6" and "7" within "4.5678".
            textCore.SelectFragment(16, 7);
            Assert.AreEqual(textCore.SelectionText, "4.5678");
            Assert.AreEqual(textCore.CursorPosition.X, 19);
            Assert.AreEqual(textCore.CursorPosition.Y, 7);
        }

        [Test, RequiresSTA]
        public void TestSelectFragmentIdentifier()
        {
            // Double-clicking after "ass" within "Associative".
            textCore.SelectFragment(9, 12);
            Assert.AreEqual(textCore.SelectionText, "Associative");
            Assert.AreEqual(textCore.CursorPosition.X, 17);
            Assert.AreEqual(textCore.CursorPosition.Y, 12);
        }

        [Test, RequiresSTA]
        public void TestSelectFragmentDigits()
        {
            // Double-clicking between "6" and "7" within "4.5678" in comments.
            textCore.SelectFragment(45, 7);
            Assert.AreEqual(textCore.SelectionText, "5678");
            Assert.AreEqual(textCore.CursorPosition.X, 47);
            Assert.AreEqual(textCore.CursorPosition.Y, 7);
        }

        [Test, RequiresSTA]
        public void TestSelectFragmentLetters()
        {
            // Double-clicking in the middle of "value" in comments.
            textCore.SelectFragment(34, 7);
            Assert.AreEqual(textCore.SelectionText, "value");
            Assert.AreEqual(textCore.CursorPosition.X, 36);
            Assert.AreEqual(textCore.CursorPosition.Y, 7);
        }

        [Test, RequiresSTA]
        public void TestSelectFragmentPunctuation()
        {
            // Double-clicking in the middle of "//" in front of comments.
            textCore.SelectFragment(22, 7);
            Assert.AreEqual(textCore.SelectionText, "//");
            Assert.AreEqual(textCore.CursorPosition.X, 23);
            Assert.AreEqual(textCore.CursorPosition.Y, 7);
        }

        [Test, RequiresSTA]
        public void TestSelectFragmentWhiteSpace()
        {
            // Double-clicking in the middle of white spaces on the line.
            textCore.SelectFragment(3, 7);
            Assert.AreEqual(textCore.SelectionText, "\t    ");
            Assert.AreEqual(textCore.CursorPosition.X, 5);
            Assert.AreEqual(textCore.CursorPosition.Y, 7);
        }
    }
}

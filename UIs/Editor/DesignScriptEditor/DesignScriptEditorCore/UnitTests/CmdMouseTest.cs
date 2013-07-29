using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdMouseTest
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
        public void MouseEventTest01()
        {
            textCore.SetCursorPosition(2, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetMouseDownPosition(2, 3, null);
            textCore.SetMouseUpPosition(2, 3, null);
            Assert.AreEqual(textCore.GetSelectionText(), "llo\n\n\t\tcruel\n\tw");
            Assert.AreEqual(textCore.SelectionEnd, new Point(2, 3));
            Assert.AreEqual(textCore.SelectionStart, new Point(2, 0));
        }

        [Test, RequiresSTA]
        public void MouseEventTest02()
        {
            textCore.SetMouseDownPosition(3, 0, null);
            textCore.SetMouseMovePosition(3, 3, null);
            textCore.SetMouseUpPosition(3, 3, null);
            Assert.AreEqual(textCore.SelectionStart, new Point(3, 0));
            Assert.AreEqual(textCore.SelectionEnd, new Point(3, 3));
            Assert.AreEqual(textCore.GetSelectionText(), "lo\n\n\t\tcruel\n\two");
        }

        [Test, RequiresSTA]
        public void MouseEventTest03()
        {
            textCore.SetMouseDownPosition(3, 0, null);
            textCore.SetMouseMovePosition(7, 3, null);
            textCore.SetMouseUpPosition(7, 3, null);
            Assert.AreEqual(textCore.SelectionStart, new Point(3, 0));
            Assert.AreEqual(textCore.SelectionEnd, new Point(6, 3));
            Assert.AreEqual(textCore.GetSelectionText(), "lo\n\n\t\tcruel\n\tworld");
        }

        [Test, RequiresSTA]
        public void MouseEventTest04()
        {
            textCore.SetMouseDownPosition(7, 3, null);
            textCore.SetMouseMovePosition(3, 0, null);
            textCore.SetMouseUpPosition(3, 0, null);
            Assert.AreEqual(textCore.SelectionStart, new Point(3, 0));
            Assert.AreEqual(textCore.SelectionEnd, new Point(6, 3));
            Assert.AreEqual(textCore.GetSelectionText(), "lo\n\n\t\tcruel\n\tworld");
        }

        [Test, RequiresSTA]
        public void MouseEventTest05()
        {
            textCore.SetMouseDownPosition(7, 4, null);
            textCore.SetMouseMovePosition(0, 0, null);
            textCore.SetMouseUpPosition(0, 0, null);
            Assert.AreEqual(textCore.SelectionStart, new Point(0, 0));
            Assert.AreEqual(textCore.SelectionEnd, new Point(6, 3));
            Assert.AreEqual(textCore.GetSelectionText(), "hello\n\n\t\tcruel\n\tworld");
        }
    }
}

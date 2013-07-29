using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Windows.Input;
using System.Drawing;

namespace DesignScript.Editor.Core.UnitTest
{
    class CmdDoNavigationTest
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
        public void DoNavigationTest01()
        {
            textCore.SetCursorPosition(2, 0);
            textCore.DoNavigation(Key.Right);
            Assert.AreEqual(new Point(3, 0), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void DoNavigationTest02()
        {
            textCore.SetCursorPosition(2, 0);
            textCore.DoNavigation(Key.Left);
            Assert.AreEqual(new Point(1, 0), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void DoNavigationTest03()
        {
            textCore.SetCursorPosition(2, 0);
            textCore.DoNavigation(Key.Down);
            Assert.AreEqual(new Point(0, 1), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void DoNavigationTest04()
        {
            textCore.SetCursorPosition(2, 0);
            textCore.DoNavigation(Key.Up);
            Assert.AreEqual(new Point(0, 0), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void DoNavigationTest05()
        {
            textCore.SetCursorPosition(5, 0);
            textCore.DoNavigation(Key.Right);
            Assert.AreEqual(new Point(0, 1), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void DoNavigationTest06()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.DoNavigation(Key.Right);
            Assert.AreEqual(new Point(6, 3), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void DoNavigationTest07()
        {
            textCore.SetCursorPosition(4, 2);
            textCore.DoNavigation(Key.End);
            Assert.AreEqual(new Point(7, 2), textCore.CursorPosition);
        }

        [Test, RequiresSTA]
        public void DoNavigationTest08()
        {
            textCore.SetCursorPosition(0, 3);
            textCore.DoNavigation(Key.Down);
            Assert.AreEqual(new Point(6, 3), textCore.CursorPosition);
        }
    }
}

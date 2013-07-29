using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Core.UnitTest
{
    class SolutionTests
    {
        private int totalOpenScripts = 0;
        private Solution testSolution;

        [SetUp]
        public void Setup()
        {           
            testSolution = Solution.CreateTemporary();
            testSolution.AddNewScript("Assignment09.ds");
            testSolution.AddNewScript("Assignment19.ds");
            testSolution.AddNewScript("Assignment13.ds");
            TextEditorCore.CreateTemporary();
        }

        [TearDown]
        public void TearDown()
        {
            TextEditorCore.InvalidateInstance();
        }

        [Test]
        public void TestScriptCount()
        {
            Assert.IsTrue(testSolution.ScriptCount == 3);
        }

        [Test]
        public void TestWatchExpression()
        {
            testSolution.AddWatchExpressions("a");
            Assert.IsTrue(testSolution.RemoveWatchExpressions("a"));
            Assert.IsFalse(testSolution.RemoveWatchExpressions("a"));
        }

        [Test]
        public void TestWatchExpressionMultiple()
        {
            testSolution.AddWatchExpressions("a");
            testSolution.AddWatchExpressions("b");
            testSolution.AddWatchExpressions("c");

            Assert.IsTrue(testSolution.RemoveWatchExpressions("a"));
            Assert.IsFalse(testSolution.RemoveWatchExpressions("z"));
        }

        [Test]
        public void TestActiveScript()
        {
            Assert.AreEqual(testSolution.ActiveScriptIndex, -1);
            testSolution.ActivateScript(2);
            Assert.AreEqual(testSolution.ActiveScriptIndex, 2);
        }

        [Test]
        public void TestNonExistentScript()
        {
            Assert.AreEqual(testSolution.ActiveScriptIndex, -1);
            testSolution.ActivateScript(33);
            Assert.AreEqual(testSolution.ActiveScriptIndex, -1);
        }

        [Test]
        public void TestCurrentSolution()
        {
            Solution first = Solution.Current;
            Solution second = Solution.Current;
            Assert.AreEqual(first, second);
        }

        [Test]
        public void TestRemoveWatchExpression()
        {
            List<string> testString = new List<string>();
            int counter = 0;

            testString.Add("x");
            testString.Add("y");
            testString.Add("z");
            testSolution.AddWatchExpressions("a");
            testSolution.AddWatchExpressions("b");
            testSolution.AddWatchExpressions("c");
            testSolution.GetWatchExpressions(testString);

            Assert.AreEqual("a", testString[counter]);
            Assert.AreEqual("b", testString[counter + 1]);
            Assert.AreEqual("c", testString[counter + 2]);
        }

        [Test]
        public void TestEventHandler()
        {
            Assert.AreEqual(totalOpenScripts, 0);
            Solution.ScriptCountChanged -= new ScriptCountChangedHandler(OnScriptCountChanged);
            Solution.ScriptCountChanged += new ScriptCountChangedHandler(OnScriptCountChanged);
            testSolution.AddNewScript("Assignment02.ds");
            Assert.AreEqual(totalOpenScripts, 1);
            testSolution.CloseScript(0);
            Assert.AreEqual(totalOpenScripts, 0);
        }

        void OnScriptCountChanged(object sender, ScriptCountChangedEventArgs e)
        {
            this.totalOpenScripts += e.ScriptsAdded.Count;
            this.totalOpenScripts -= e.ScriptsRemoved.Count;
        }
    }
}

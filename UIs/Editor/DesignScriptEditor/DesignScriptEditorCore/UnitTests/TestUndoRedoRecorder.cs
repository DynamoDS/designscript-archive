using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class TestUndoRedoRecorder
    {
        UndoRedoRecorder undoRedoRecorder;
        List<string> lineList;

        [SetUp]
        public void setup()
        {
            lineList = new List<string>();
            lineList.Add("first line\n");
            lineList.Add("second line\n");
            lineList.Add("third line");
            undoRedoRecorder = new UndoRedoRecorder(lineList);

        }

        [Test]
        public void TestRestorefromUndoStack()
        {
            Point cursorPosition = new Point(0,0);
            List<string> tempList = new List<string>();
            tempList.Add("line has been changed");
            undoRedoRecorder.RecordInsertion(1, 2, tempList, cursorPosition);
            undoRedoRecorder.Undo(ref cursorPosition);
            Assert.IsTrue(lineList[1].Length == "line has been changed".ToCharArray().Length);
            Assert.AreEqual("Line list size: 2, Undo stack size: 0, Redo stack size: 1 ", undoRedoRecorder.ToString());
        }
    }
}

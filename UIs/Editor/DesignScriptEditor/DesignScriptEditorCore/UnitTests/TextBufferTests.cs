using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class TextBufferTests
    {
        TextBuffer textBuffer;

        [SetUp]
        public void Setup()
        {
            textBuffer = new TextBuffer("\thello");
        }

        [Test]
        public void TestTextBuffer01()
        {
            Assert.IsTrue(textBuffer.GetLineCount() == 1);
        }

        [Test]
        public void TestTextBuffer02()
        {
            int totalLines = textBuffer.GetLineCount();
            int characters = textBuffer.GetCharacterCount(totalLines - 1, false, true);
            Assert.IsTrue(textBuffer.GetText(0, 0, totalLines - 1, characters) == '\t' + "hello");
        }

        [Test]
        public void TestTextBuffer03()
        {
            textBuffer.InsertText(0, 0, "\thello");
            Assert.IsTrue(textBuffer.GetText(0, 1, 0, 5) == ("hell"));
        }

        [Test]
        public void TestTextBuffer04()
        {
            textBuffer.InsertText(0, 6, "\n");
            Assert.IsTrue(textBuffer.GetLineCount() == 2);
        }

        [Test]
        public void TestTextBuffer05()
        {
            Assert.IsTrue(textBuffer.GetContent().Length == ("\thello").Length);
        }

        [Test]
        public void TestTextBuffer06()
        {
            Assert.IsTrue(textBuffer.GetCharAt(1, 0) == 'h');
        }

        [Test]
        public void TestTextBuffer07()
        {
            Assert.IsTrue(textBuffer.GetCharacterCount(0, true, false) == 9);
        }

        [Test]
        public void TestTextBuffer08()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, '\t' + "");
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.IsTrue(textBuffer.GetCharacterCount(0, true, false) == 9);
        }

        [Test]
        public void TestTextBuffer09()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\ntomorrow comes everyday");
            textBuffer.UndoTextEditing(ref cursorPosition);
            textBuffer.RedoTextEditing(ref cursorPosition);
            Assert.IsTrue(textBuffer.GetLineCount() == 2);
        }

        [Test]
        public void TestTextBuffer10()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\ntomorrow comes everyday\n");
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.IsTrue(textBuffer.GetLineCount() == 1);
        }

        [Test]
        public void TestTextBuffer11()
        {
            textBuffer.InsertText(0, 6, "\n");
            Assert.IsTrue(textBuffer.GetLineCount() == 2);
        }

        [Test]
        public void TestTextBuffer12()
        {
            textBuffer.InsertText(0, 6, "\n");
            Assert.IsTrue(textBuffer.GetCharacterCount(1, false, true) == 0);
        }

        [Test]
        public void TestTextBuffer13()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\n");
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.IsTrue(textBuffer.GetLineCount() == 1);
        }

        [Test]
        public void TestTextBuffer14()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\nsecond line");
            textBuffer.InsertText(1, 12, "\nthird line");
            textBuffer.ReplaceText(1, 0, 1, 12, "\t");
            Assert.AreEqual(textBuffer.GetLineContent(1), "\tthird line");
            Assert.IsTrue(textBuffer.GetLineCount() == 2);
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(1), "second line\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 3);
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line");
        }

        [Test]
        public void TestGetLineCount()
        {
            Assert.AreEqual(1, textBuffer.GetLineCount(true));
            Assert.AreEqual(1, textBuffer.GetLineCount(false));

            textBuffer.InsertText(0, 6, "\n");
            Assert.AreEqual(2, textBuffer.GetLineCount(true));
            Assert.AreEqual(1, textBuffer.GetLineCount(false));
        }

        [Test]
        public void TestMoveText()
        {
            //\thelThis islo\n
            //This is epic!\n
            // even more epic!
            textBuffer.InsertText(0, 6, "\nThis is epic!");
            textBuffer.InsertText(1, 13, "\nThis is even more epic!");
            textBuffer.MoveText(new System.Drawing.Point(0, 2), new System.Drawing.Point(7, 2), new System.Drawing.Point(4, 0));
            Assert.AreEqual(textBuffer.GetCharacterCount(0, false, true), 14);
        }

        [Test]
        public void TestMoveText01()
        {
            //\thello\n
            //econd line\n
            //third line\n
            //fourth linse
            textBuffer.InsertText(0, 6, "\nsecond line");
            textBuffer.InsertText(1, 12, "\nthird line");
            textBuffer.InsertText(2, 10, "\nfourth line");
            textBuffer.MoveText(new System.Drawing.Point(1, 1), new System.Drawing.Point(10, 3), new System.Drawing.Point(0, 1));
            Assert.AreEqual(textBuffer.GetLineContent(3), "fourth linse");
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "econd line\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 4);
        }

        [Test]
        public void TestMoveText02()
        {
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(2, 0, "third line\n");
            textBuffer.InsertText(3, 0, "fourth line");
            textBuffer.MoveText(new System.Drawing.Point(0, 1), new System.Drawing.Point(8, 3), new System.Drawing.Point(11, 3));
            Assert.AreEqual(textBuffer.GetLineContent(3), "fourth l");
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "inesecond line\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 4);
        }

        [Test]
        public void TestMoveText03()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.InsertText(2, 11, "fourth line");
            textBuffer.MoveText(new System.Drawing.Point(0, 1), new System.Drawing.Point(12, 1), new System.Drawing.Point(0, 3));
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(3), "fourth line");
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "second line\n");
            Assert.AreEqual(textBuffer.GetLineContent(0), "\thello\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 4);
        }

        [Test]
        public void TestMoveText04()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.InsertText(2, 11, "fourth line");
            textBuffer.MoveText(new System.Drawing.Point(0, 3), new System.Drawing.Point(11, 3), new System.Drawing.Point(2, 1));
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(3), "fourth line");
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "second line\n");
            Assert.AreEqual(textBuffer.GetLineContent(0), "\thello\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 4);
        }

        [Test]
        public void TestMoveText05()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.InsertText(2, 11, "fourth line\n");
            textBuffer.MoveText(new System.Drawing.Point(0, 2), new System.Drawing.Point(12, 3), new System.Drawing.Point(0, 1));
            Assert.AreEqual(textBuffer.GetLineContent(2), "fourth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(3), "second line\n");
            Assert.AreEqual(textBuffer.GetLineContent(0), "\thello\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 5);
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(3), "fourth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "second line\n");
            Assert.AreEqual(textBuffer.GetLineContent(0), "\thello\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 5);
        }

        [Test]
        public void TestReplaceAllText01()
        {
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.InsertText(2, 11, "fourth line\n");

            textBuffer.ReplaceText(0, 0, 4, 0, "");
            Assert.AreEqual(textBuffer.GetLineCount(), 0);
        }

        [Test]
        public void TestReplaceAllText02()
        {
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.InsertText(2, 11, "fourth line");

            textBuffer.ReplaceText(0, 0, 3, 11, "");
            Assert.AreEqual(textBuffer.GetLineCount(), 0);
        }

        [Test]
        public void TestMoveText06()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.InsertText(2, 11, "fourth line\n");
            textBuffer.InsertText(3, 12, "fifth line\n");
            textBuffer.InsertText(4, 11, "sixth line\n");
            textBuffer.InsertText(5, 11, "seventh line\n");
            textBuffer.InsertText(6, 13, "eighth line\n");
            textBuffer.InsertText(7, 12, "ninth line\n");
            textBuffer.InsertText(8, 11, "tenth line\n");
            textBuffer.InsertText(9, 11, "eleventh line\n");
            textBuffer.InsertText(10, 14, "twelvth line\n");
            textBuffer.MoveText(new System.Drawing.Point(0, 8), new System.Drawing.Point(11, 9), new System.Drawing.Point(0, 4));
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(11), "twelvth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(10), "eleventh line\n");
            Assert.AreEqual(textBuffer.GetLineContent(9), "tenth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(8), "ninth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(7), "eighth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(6), "seventh line\n");
            Assert.AreEqual(textBuffer.GetLineContent(5), "sixth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(4), "fifth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(3), "fourth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "second line\n");
            Assert.AreEqual(textBuffer.GetLineContent(0), "\thello\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 13);
        }

        [Test]
        public void TestMoveText07()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.InsertText(2, 11, "fourth line\n");
            textBuffer.InsertText(3, 12, "fifth line\n");
            textBuffer.InsertText(4, 11, "sixth line\n");
            textBuffer.InsertText(5, 11, "seventh line\n");
            textBuffer.InsertText(6, 13, "eighth line\n");
            textBuffer.InsertText(7, 12, "ninth line\n");
            textBuffer.InsertText(8, 11, "tenth line\n");
            textBuffer.InsertText(9, 11, "eleventh line\n");
            textBuffer.InsertText(10, 14, "twelvth line\n");
            textBuffer.MoveText(new System.Drawing.Point(0, 10), new System.Drawing.Point(0, 12), new System.Drawing.Point(0, 4));
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(11), "twelvth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(10), "eleventh line\n");
            Assert.AreEqual(textBuffer.GetLineContent(9), "tenth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(8), "ninth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(7), "eighth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(6), "seventh line\n");
            Assert.AreEqual(textBuffer.GetLineContent(5), "sixth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(4), "fifth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(3), "fourth line\n");
            Assert.AreEqual(textBuffer.GetLineContent(2), "third line\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "second line\n");
            Assert.AreEqual(textBuffer.GetLineContent(0), "\thello\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 13);
        }

        [Test]
        public void TestMoveText08()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\n");
            textBuffer.MoveText(new System.Drawing.Point(0, 0), new System.Drawing.Point(6, 0), new System.Drawing.Point(0, 1));
            Assert.AreEqual(textBuffer.GetLineContent(0), "\n");
            Assert.AreEqual(textBuffer.GetLineContent(1), "\thello");
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(0), "\thello\n");
            Assert.AreEqual(textBuffer.GetLineCount(), 2);
        }

        [Test]
        public void TestCutPasteSameLine()
        {
            Point cursorPosition = new Point(0, 0);
            textBuffer.InsertText(0, 6, "\nsecond line\n");
            textBuffer.InsertText(1, 12, "third line\n");
            textBuffer.ReplaceText(1, 0, 1, 11, "");
            Assert.AreEqual(textBuffer.GetLineContent(1), "\n");
            textBuffer.InsertText(1, 0, "second line");
            Assert.AreEqual(textBuffer.GetLineContent(1), "second line\n");
            textBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(textBuffer.GetLineContent(1), "\n");
        }

        [Test]
        public void TestSameLineDeleteUndo()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line\n";

            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(5, localBuffer.GetLineCount());
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Fourth line\n", localBuffer.GetLineContent(3));

            // Continuously deleting the line-break on first line.
            localBuffer.ReplaceText(0, 10, 0, 11, string.Empty);
            localBuffer.ReplaceText(0, 21, 0, 22, string.Empty);
            localBuffer.ReplaceText(0, 31, 0, 32, string.Empty);

            // Now we're left with just one line-break at the end of "Fourth line\n".
            Assert.AreEqual(2, localBuffer.GetLineCount());
            Assert.AreEqual("First lineSecond lineThird lineFourth line\n", localBuffer.GetLineContent(0));

            System.Drawing.Point dummyCursor = new Point();

            // Undo once, should break the first line up again.
            localBuffer.UndoTextEditing(ref dummyCursor);
            Assert.AreEqual(3, localBuffer.GetLineCount());
            Assert.AreEqual("First lineSecond lineThird line\n", localBuffer.GetLineContent(0));

            // Undo once more, should break the first line up again.
            localBuffer.UndoTextEditing(ref dummyCursor);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            Assert.AreEqual("First lineSecond line\n", localBuffer.GetLineContent(0));

            // Undo once more, should break the first line up again.
            localBuffer.UndoTextEditing(ref dummyCursor);
            Assert.AreEqual(5, localBuffer.GetLineCount());
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line\n", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestSecondLastLineDeleteUndo()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";

            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line", localBuffer.GetLineContent(3));

            // Replace the second last line when there's no "\n" on the fourth line.
            localBuffer.ReplaceText(2, 0, 2, 11, "Three line\n");
            Assert.AreEqual(4, localBuffer.GetLineCount());
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Three line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line", localBuffer.GetLineContent(3));

            // Undo once should restore the original third line content.
            System.Drawing.Point cursorPosition = new Point();
            localBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestInsertTextLastLineUndo01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line\n";

            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(5, localBuffer.GetLineCount());
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line\n", localBuffer.GetLineContent(3));

            localBuffer.InsertText(4, 0, "Fifth line\nSixth line\n");
            Assert.AreEqual(7, localBuffer.GetLineCount());
            Assert.AreEqual("Fifth line\n", localBuffer.GetLineContent(4));
            Assert.AreEqual("Sixth line\n", localBuffer.GetLineContent(5));

            System.Drawing.Point cursorPosition = new Point();
            localBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(5, localBuffer.GetLineCount());
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line\n", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestInsertTextLastLineUndo02()
        {
            string initialContent = "Master Wayne";

            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(1, localBuffer.GetLineCount());
            Assert.AreEqual("Master Wayne", localBuffer.GetLineContent(0));

            localBuffer.ReplaceText(0, 0, 0, 12, "\n\n\n\n");
            Assert.AreEqual(5, localBuffer.GetLineCount());
            Assert.AreEqual("\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("\n", localBuffer.GetLineContent(3));

            System.Drawing.Point cursorPosition = new Point();
            localBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(1, localBuffer.GetLineCount());
            Assert.AreEqual("Master Wayne", localBuffer.GetLineContent(0));

            localBuffer.RedoTextEditing(ref cursorPosition);
            Assert.AreEqual("\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("\n", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestInsertTextLastLineUndo03()
        {
            string initialContent = "Master Wayne";

            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(1, localBuffer.GetLineCount());
            Assert.AreEqual("Master Wayne", localBuffer.GetLineContent(0));

            localBuffer.ReplaceText(0, 0, 0, 12, "\n\nBatman\n\n");
            Assert.AreEqual(5, localBuffer.GetLineCount());
            Assert.AreEqual("\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Batman\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("\n", localBuffer.GetLineContent(3));

            System.Drawing.Point cursorPosition = new Point();
            localBuffer.UndoTextEditing(ref cursorPosition);
            Assert.AreEqual(1, localBuffer.GetLineCount());
            Assert.AreEqual("Master Wayne", localBuffer.GetLineContent(0));

            localBuffer.RedoTextEditing(ref cursorPosition);
            Assert.AreEqual("\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Batman\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("\n", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestFindAllOccurences01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", null, FindOptions.ReplaceOnce);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(new Point(6, 0), findResults[0].startPoint);
            Assert.AreEqual(new Point(9, 0), findResults[0].endPoint);
            Assert.AreEqual(new Point(7, 1), findResults[1].startPoint);
            Assert.AreEqual(new Point(10, 1), findResults[1].endPoint);
            Assert.AreEqual(new Point(6, 2), findResults[2].startPoint);
            Assert.AreEqual(new Point(9, 2), findResults[2].endPoint);
            Assert.AreEqual(new Point(7, 3), findResults[3].startPoint);
            Assert.AreEqual(new Point(10, 3), findResults[3].endPoint);
        }

        [Test]
        public void TestFindAllOccurences02()
        {
            string initialContent = "First line design\nSecond line\nThird line\nFourth design design";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", null, FindOptions.ReplaceOnce);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(new Point(11, 0), findResults[0].startPoint);
            Assert.AreEqual(new Point(16, 0), findResults[0].endPoint);
            Assert.AreEqual(new Point(7, 3), findResults[1].startPoint);
            Assert.AreEqual(new Point(12, 3), findResults[1].endPoint);
            Assert.AreEqual(new Point(14, 3), findResults[2].startPoint);
            Assert.AreEqual(new Point(19, 3), findResults[2].endPoint);
        }

        [Test]
        public void TestFindAllOccurences03()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", null, FindOptions.ReplaceOnce);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(0, findResults.Count);
        }

        [Test]
        public void TestFindNext01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", null, FindOptions.FindNext);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);
            Assert.AreEqual(new Point(7, 1), findResults[localBuffer.CurrentSearchIndex].startPoint);
            Assert.AreEqual(new Point(10, 1), findResults[localBuffer.CurrentSearchIndex].endPoint);
        }

        [Test]
        public void TestFindNext02()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", null, FindOptions.FindNext);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(0, findResults.Count);
            Assert.AreEqual(-1, localBuffer.CurrentSearchIndex);
        }

        [Test]
        public void TestFindPrevious01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", null, FindOptions.FindPrevious);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(new Point(7, 3), findResults[localBuffer.CurrentSearchIndex].startPoint);
            Assert.AreEqual(new Point(10, 3), findResults[localBuffer.CurrentSearchIndex].endPoint);
        }

        [Test]
        public void TestFindPrevious02()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", null, FindOptions.FindPrevious);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(0, findResults.Count);
            Assert.AreEqual(-1, localBuffer.CurrentSearchIndex);
        }

        [Test]
        public void TestReplaceCurrent01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", "design", FindOptions.ReplaceOnce);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(4, findResults.Count);
            Assert.AreEqual("First design\n", localBuffer.GetLineContent(0));
        }

        [Test]
        public void TestReplaceCurrent02()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line design";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", "line", FindOptions.ReplaceAll);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual("Fourth line line", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestReplaceAll01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", "design", FindOptions.ReplaceAll);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(4, findResults.Count);
            Assert.AreEqual("First design\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second design\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third design\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth design", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestReplaceAll02()
        {
            string initialContent = "First line design\nSecond line\nThird line\nFourth line design";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", "line", FindOptions.ReplaceAll);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual("First line line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Fourth line line", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestReplaceAll03()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth design design";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", "line", FindOptions.ReplaceAll);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual("Fourth line line", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestReplaceAll04()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", "design", FindOptions.ReplaceAll);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual("Fourth design design", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestReplaceAll05()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("design", "line", FindOptions.ReplaceAll);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(0, findResults.Count);
            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestReplaceUndo01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", "design", FindOptions.ReplaceOnce);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(4, findResults.Count);
            Assert.AreEqual("First design\n", localBuffer.GetLineContent(0));

            Point cursor = new Point(0, 0);
            localBuffer.UndoTextEditing(ref cursor);

            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
        }

        [Test]
        public void TestReplaceAllUndo01()
        {
            string initialContent = "First line\nSecond line\nThird line\nFourth line";
            TextBuffer localBuffer = new TextBuffer(initialContent);
            Assert.AreEqual(4, localBuffer.GetLineCount());
            localBuffer.FindReplace("line", "design", FindOptions.ReplaceAll);
            List<FindPosition> findResults = new List<FindPosition>();
            localBuffer.GetSearchResults(ref findResults);

            Assert.AreEqual(4, findResults.Count);
            Assert.AreEqual("First design\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second design\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third design\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth design", localBuffer.GetLineContent(3));

            Point cursor = new Point(0, 0);
            localBuffer.UndoTextEditing(ref cursor);

            Assert.AreEqual("First line\n", localBuffer.GetLineContent(0));
            Assert.AreEqual("Second line\n", localBuffer.GetLineContent(1));
            Assert.AreEqual("Third line\n", localBuffer.GetLineContent(2));
            Assert.AreEqual("Fourth line", localBuffer.GetLineContent(3));
        }

        [Test]
        public void TestGetIdentifierBeforeColumn01()
        {
            TextBuffer currentBuffer = new TextBuffer("array = 0..a.b.c..2;");
            Assert.AreEqual("a.b.c", currentBuffer.GetIdentifierBeforeColumn(0, 16));
        }

        [Test]
        public void TestGetIdentifierBeforeColumn02()
        {
            TextBuffer currentBuffer = new TextBuffer("element = foo(sampleArray[20]);");
            Assert.AreEqual("sampleArray[20]", currentBuffer.GetIdentifierBeforeColumn(0, 29));
        }

        [Test]
        public void TestGetIdentifierBeforeColumn03()
        {
            TextBuffer currentBuffer = new TextBuffer("element = foo(sampleArray[  20  ]     );");
            Assert.AreEqual("sampleArray[20]", currentBuffer.GetIdentifierBeforeColumn(0, 38));
        }

        [Test]
        public void TestGetIdentifierBeforeColumn04()
        {
            TextBuffer currentBuffer = new TextBuffer("element = foo(sampleArray[20][a._b.c]);");
            Assert.AreEqual("sampleArray[20][a._b.c]", currentBuffer.GetIdentifierBeforeColumn(0, 37));
        }

        [Test]
        public void TestGetIdentifierBeforeColumn05()
        {
            TextBuffer currentBuffer = new TextBuffer("xCoords = abs(myMeshes[6][3].Triangles[0][6].Sides[2].StartPoint.X);");
            Assert.AreEqual("myMeshes[6][3].Triangles[0][6].Sides[2].StartPoint", currentBuffer.GetIdentifierBeforeColumn(0, 64));
        }

        [Test]
        public void TestGetTextSingleLine01()
        {
            string textInRegion = textBuffer.GetText(0, 2, 0, 5);
            Assert.AreEqual("ell", textInRegion);
        }

        [Test]
        public void TestGetTextInvalidRange01()
        {
            string textInRegion = textBuffer.GetText(0, 5, 0, 2);
            Assert.AreEqual(string.Empty, textInRegion);
        }

        [Test]
        public void TestGetTextInvalidRange02()
        {
            string textInRegion = textBuffer.GetText(-20, 2, -20, 5);
            Assert.AreEqual(string.Empty, textInRegion);
        }

        [Test]
        public void TestGetTextInvalidRange03()
        {
            string textInRegion = textBuffer.GetText(20, 2, 20, 5);
            Assert.AreEqual(string.Empty, textInRegion);
        }

        [Test]
        public void TestGetTextInvalidRange04()
        {
            string textInRegion = textBuffer.GetText(-20, 2, -10, 5);
            Assert.AreEqual(string.Empty, textInRegion);
        }

        [Test]
        public void TestGetTextInvalidRange05()
        {
            string textInRegion = textBuffer.GetText(10, 2, 20, 5);
            Assert.AreEqual(string.Empty, textInRegion);
        }
    }
}

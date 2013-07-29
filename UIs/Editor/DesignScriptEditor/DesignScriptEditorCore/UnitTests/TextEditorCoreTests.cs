using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Core.UnitTest
{
    class TextEditorCoreTests
    {
        private TextEditorCore textCore;
        private Solution testSolution;

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
            textCore.ParseScriptImmediate();
        }

        [TearDown]
        public void TearDown()
        {
            TextEditorCore.InvalidateInstance();
        }

        [Test, RequiresSTA]
        public void TestBackwardSelectionOnSameLine()
        {
            // Selection by going backward on the same line.
            textCore.SetCursorPosition(6, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(1, 2);

            Assert.AreEqual(textCore.SelectionStart.X, 1);
            Assert.AreEqual(textCore.SelectionStart.Y, 2);
            Assert.AreEqual(textCore.SelectionEnd.X, 6);
            Assert.AreEqual(textCore.SelectionEnd.Y, 2);
            Assert.AreEqual(textCore.GetSelectionText(), "\tcrue");
        }

        [Test, RequiresSTA]
        public void TestControlLeftOnEmptyLine()
        {
            textCore.SetCursorPosition(0, 1);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Left);

            Assert.AreEqual(textCore.CursorPosition.X, 5);
            Assert.AreEqual(textCore.CursorPosition.Y, 0);
        }

        [Test, RequiresSTA]
        public void TestControlLeftOnLastLine01()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.InsertText("\n");
            Assert.AreEqual(textCore.CurrentTextBuffer.GetLineContent(3), "    world\n");
            textCore.SetCursorPosition(0, 4);

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Left);

            Assert.AreEqual(textCore.CursorPosition.X, 9);
            Assert.AreEqual(textCore.CursorPosition.Y, 3);
        }

        [Test, RequiresSTA]
        public void TestControlLeftOnLastLine02()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.InsertText("\n");
            Assert.AreEqual(textCore.CurrentTextBuffer.GetLineContent(3), "    world\n");

            // Remove the final line that's made up of just spaces.
            textCore.SelectLines(4, 0);
            textCore.DoControlCharacter(System.Windows.Input.Key.Back);

            // We should end up with 4 entries in text buffer, but since 
            // the last entry ends with a '\n', we should get '5' here.
            Assert.AreEqual(5, textCore.CurrentTextBuffer.GetLineCount());

            // Position the cursor on the 5th line (non-existent line), 
            // and then control-left should bring us back onto line 4.
            textCore.SetCursorPosition(0, 4);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Left);

            // The cursor is positioned on line 4 last character.
            Assert.AreEqual(textCore.CursorPosition.X, 9);
            Assert.AreEqual(textCore.CursorPosition.Y, 3);
        }

        [Test, RequiresSTA]
        public void TestControlRightOnSquareBracket()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.InsertText("\n[Imperative]");
            Assert.AreEqual("    [Imperative]", textCore.CurrentTextBuffer.GetLineContent(4));

            // Cursor should move right after "]".
            Assert.AreEqual(textCore.CursorPosition.X, 16);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);

            // Places cursor right before "tive".
            textCore.SetCursorPosition(11, 4);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Right);

            // Cursor should be right after "tive".
            Assert.AreEqual(textCore.CursorPosition.X, 15);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);

            // Does another CTRL+RIGHT...
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Right);

            // Cursor should be right after "]".
            Assert.AreEqual(textCore.CursorPosition.X, 16);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);
        }


        [Test, RequiresSTA]
        public void TestControlRightOnNewLineLastLine()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.InsertText("\n");
            Assert.AreEqual(textCore.CursorPosition.X, 4);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Right);
            Assert.AreEqual(textCore.CursorPosition.X, 4);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Left);
            Assert.AreEqual(textCore.CursorPosition.X, 0);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Right);
            Assert.AreEqual(textCore.CursorPosition.X, 4);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);
        }

        [Test, RequiresSTA]
        public void TestControlRightOnNewLine01()
        {
            textCore.SetCursorPosition(7, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Right);
            Assert.AreEqual(textCore.CursorPosition.X, 1);
            Assert.AreEqual(textCore.CursorPosition.Y, 3);
        }

        [Test, RequiresSTA]
        public void TestControlRightOnNewLine02()
        {
            textCore.SetCursorPosition(0, 1);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Right);
            Assert.AreEqual(textCore.CursorPosition.X, 2);
            Assert.AreEqual(textCore.CursorPosition.Y, 2);
        }

        [Test, RequiresSTA]
        public void TestDown()
        {
            textCore.SetCursorPosition(7, 2);
            textCore.DoNavigation(System.Windows.Input.Key.Down);
            Assert.AreEqual(textCore.CursorPosition.X, 6);
            Assert.AreEqual(textCore.CursorPosition.Y, 3);
        }

        [Test, RequiresSTA]
        public void TestUp()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(textCore.CursorPosition.X, 3);
            Assert.AreEqual(textCore.CursorPosition.Y, 2);
        }

        [Test, RequiresSTA]
        public void TestControlLeftOnLogicalOr()
        {
            textCore.SetCursorPosition(6, 3);
            textCore.InsertText("\nEarth || Mars");
            textCore.SetCursorPosition(9, 4);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Left);
            Assert.AreEqual(textCore.CursorPosition.X, 6);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Control);
            textCore.DoNavigation(System.Windows.Input.Key.Right);
            Assert.AreEqual(textCore.CursorPosition.X, 9);
            Assert.AreEqual(textCore.CursorPosition.Y, 4);
        }

        [Test, RequiresSTA]
        public void TestBackwardSelectionAcrossLines()
        {
            // Selection by going upward across different lines.
            textCore.SetCursorPosition(5, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(1, 0);

            Assert.AreEqual(textCore.SelectionStart.X, 1);
            Assert.AreEqual(textCore.SelectionStart.Y, 0);
            Assert.AreEqual(textCore.SelectionEnd.X, 5);
            Assert.AreEqual(textCore.SelectionEnd.Y, 2);
            Assert.AreEqual(textCore.GetSelectionText(), "ello\n\n\t\tcru");
        }

        [Test, RequiresSTA]
        public void TestForwardSelectionAcrossLines()
        {
            // Selection by going forward across different lines.
            textCore.SetCursorPosition(1, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(5, 2);

            Assert.AreEqual(textCore.SelectionStart.X, 1);
            Assert.AreEqual(textCore.SelectionStart.Y, 0);
            Assert.AreEqual(textCore.SelectionEnd.X, 5);
            Assert.AreEqual(textCore.SelectionEnd.Y, 2);
            Assert.AreEqual(textCore.GetSelectionText(), "ello\n\n\t\tcru");
        }

        [Test, RequiresSTA]
        public void TestForwardSelectionAcrossLines2()
        {
            // Selection by going forward across different lines.
            //textCore.SetMouseDownCharacter(2, 0);
            textCore.SetCursorPosition(2, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(1, 2);

            Assert.AreEqual(textCore.SelectionStart.X, 2);
            Assert.AreEqual(textCore.SelectionStart.Y, 0);
            Assert.AreEqual(textCore.SelectionEnd.X, 1);
            Assert.AreEqual(textCore.SelectionEnd.Y, 2);
            Assert.AreEqual(textCore.GetSelectionText(), "llo\n\n\t");
        }

        [Test, RequiresSTA]
        public void TestMoveSelectedText01()
        {
            //hruel\n
            //\twello\n
            //\n
            //\t\tcorld
            textCore.SetCursorPosition(1, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(3, 2);
            textCore.MoveSelectedText(2, 3, false);
            Assert.AreEqual(textCore.GetLine(3), "\t\tcorld");
            Assert.AreEqual(textCore.GetLine(2), "\n");
            Assert.AreEqual(textCore.GetLine(1), "\twello\n");
            Assert.AreEqual(textCore.GetLine(0), "hruel\n");
            Assert.AreEqual(textCore.LineCount, 4);
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(3), "\tworld");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            Assert.AreEqual(textCore.GetLine(1), "\n");
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestMoveSelectedText02()
        {
            textCore.SetCursorPosition(4, 3);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(1, 2);
            textCore.MoveSelectedText(2, 0, false);
            Assert.AreEqual(textCore.GetLine(3), "\tld");
            Assert.AreEqual(textCore.GetLine(2), "\n");
            Assert.AreEqual(textCore.GetLine(1), "\tworllo\n");
            Assert.AreEqual(textCore.GetLine(0), "he\tcruel\n");
            Assert.AreEqual(textCore.LineCount, 4);
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(3), "\tworld");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            Assert.AreEqual(textCore.GetLine(1), "\n");
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestDeleteKeyOnEmptyLine()
        {
            textCore.SetCursorPosition(0, 1);
            Assert.AreEqual(textCore.GetLine(1), "\n");
            textCore.DoControlCharacter(System.Windows.Input.Key.Delete);
            Assert.AreEqual(textCore.GetLine(1), "\t\tcruel\n");
            Assert.AreEqual(textCore.LineCount, 3);
        }

        [Test, RequiresSTA]
        public void TestDeleteKeyAtEndOfLine()
        {
            textCore.SetCursorPosition(0, 2);
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            textCore.DoNavigation(System.Windows.Input.Key.End);
            textCore.DoControlCharacter(System.Windows.Input.Key.Delete);
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\tworld");
            Assert.AreEqual(textCore.LineCount, 3);
        }

        [Test, RequiresSTA]
        public void TestCopyPasteSingleLine()
        {
            textCore.SetCursorPosition(0, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(5, 0);
            Assert.AreEqual(textCore.GetSelectionText(), "hello");
            textCore.DoCopyText(false);
            textCore.SetCursorPosition(0, 2);
            textCore.DoPasteText();
            Assert.AreEqual(textCore.GetLine(2), "hello\t\tcruel\n");
            Assert.AreEqual(textCore.CursorPosition.X, 5);
            Assert.AreEqual(textCore.CursorPosition.Y, 2);
        }

        [Test, RequiresSTA]
        public void TestCopyPasteMultipleLines()
        {
            textCore.SetCursorPosition(0, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(1, 1);
            Assert.AreEqual(textCore.GetSelectionText(), "hello\n");
            textCore.DoCopyText(false);
            textCore.SetCursorPosition(0, 2);
            textCore.DoPasteText();
            Assert.AreEqual(textCore.GetLine(2), "hello\n");
            Assert.AreEqual(textCore.CursorPosition.X, 0);
            Assert.AreEqual(textCore.CursorPosition.Y, 3);
        }

        [Test, RequiresSTA]
        public void TestTabNoSelectionNoShift()
        {
            textCore.SetCursorPosition(4, 2);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcr  uel\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTabNoSelectionAtFront()
        {
            textCore.SetCursorPosition(0, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTabNoSelectionNoSpace()
        {
            textCore.SetCursorPosition(2, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTabNoSelectionTabSpaces()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.InsertText("  ");
            Assert.AreEqual(textCore.GetLine(2), "\t\t  cruel\n");

            textCore.SetCursorPosition(4, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(2), "\tcruel\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(2), "cruel\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(2), "cruel\n");

            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTabSelectionIndent01()
        {
            textCore.SelectLines(1, 1);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(1), "    \n");
            Assert.AreEqual(textCore.GetLine(2), "    \t\tcruel\n");

            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(1), "        \n");
            Assert.AreEqual(textCore.GetLine(2), "        \t\tcruel\n");

            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTabSelectionIndent02()
        {
            textCore.SelectLines(1, 1);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(1), "    \n");
            Assert.AreEqual(textCore.GetLine(2), "    \t\tcruel\n");

            textCore.SetCursorPosition(1, 1);
            textCore.InsertText("  ");
            Assert.AreEqual(textCore.GetLine(1), "      \n");
            Assert.AreEqual(textCore.GetLine(2), "    \t\tcruel\n");

            textCore.SelectLines(1, 1);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(1), "    \n");
            Assert.AreEqual(textCore.GetLine(2), "        cruel\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(1), "\n");
            Assert.AreEqual(textCore.GetLine(2), "    cruel\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(1), "\n");
            Assert.AreEqual(textCore.GetLine(2), "cruel\n");

            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTabSelectionIndent03()
        {
            textCore.SelectLines(0, 2);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(0), "    hello\n");
            Assert.AreEqual(textCore.GetLine(1), "    \n");
            Assert.AreEqual(textCore.GetLine(2), "    \t\tcruel\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.GetLine(1), "\n");
            Assert.AreEqual(textCore.GetLine(2), "        cruel\n");

            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTabSelectionIndent04()
        {
            string fileContent =
                "    class Calculator\n" +
                "    {\n" +
                "        def Add : int(one : int, two : int)\n" +
                "        {\n" +
                "    \t    return = one + two;\n" +
                "    \t}\n" +
                "    }\n";

            ClearExistingContent();
            textCore.SetCursorPosition(0, 0);
            textCore.InsertText(fileContent);

            Assert.AreEqual(textCore.GetLine(0), "    class Calculator\n");
            Assert.AreEqual(textCore.GetLine(1), "    {\n");
            Assert.AreEqual(textCore.GetLine(2), "        def Add : int(one : int, two : int)\n");
            Assert.AreEqual(textCore.GetLine(3), "        {\n");
            Assert.AreEqual(textCore.GetLine(4), "    \t    return = one + two;\n");
            Assert.AreEqual(textCore.GetLine(5), "    \t}\n");
            Assert.AreEqual(textCore.GetLine(6), "    }\n");

            textCore.SelectAllText();
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");

            Assert.AreEqual(textCore.GetLine(0), "class Calculator\n");
            Assert.AreEqual(textCore.GetLine(1), "{\n");
            Assert.AreEqual(textCore.GetLine(2), "    def Add : int(one : int, two : int)\n");
            Assert.AreEqual(textCore.GetLine(3), "    {\n");
            Assert.AreEqual(textCore.GetLine(4), "        return = one + two;\n");
            Assert.AreEqual(textCore.GetLine(5), "    }\n");
            Assert.AreEqual(textCore.GetLine(6), "}\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");

            Assert.AreEqual(textCore.GetLine(0), "class Calculator\n");
            Assert.AreEqual(textCore.GetLine(1), "{\n");
            Assert.AreEqual(textCore.GetLine(2), "def Add : int(one : int, two : int)\n");
            Assert.AreEqual(textCore.GetLine(3), "{\n");
            Assert.AreEqual(textCore.GetLine(4), "    return = one + two;\n");
            Assert.AreEqual(textCore.GetLine(5), "}\n");
            Assert.AreEqual(textCore.GetLine(6), "}\n");

            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.InsertText("\t");

            Assert.AreEqual(textCore.GetLine(0), "class Calculator\n");
            Assert.AreEqual(textCore.GetLine(1), "{\n");
            Assert.AreEqual(textCore.GetLine(2), "def Add : int(one : int, two : int)\n");
            Assert.AreEqual(textCore.GetLine(3), "{\n");
            Assert.AreEqual(textCore.GetLine(4), "return = one + two;\n");
            Assert.AreEqual(textCore.GetLine(5), "}\n");
            Assert.AreEqual(textCore.GetLine(6), "}\n");
        }

        [Test, RequiresSTA]
        public void TestTypingUndoRedoSameLine()
        {
            // Place the cursor right after "hello".
            textCore.SetCursorPosition(5, 0);
            Assert.AreEqual(textCore.GetLine(0), "hello\n");

            // Typing in " there" with a space.
            textCore.InsertText(" ");
            textCore.InsertText("t");
            textCore.InsertText("h");
            textCore.InsertText("e");
            textCore.InsertText("r");
            textCore.InsertText("e");
            Assert.AreEqual(textCore.GetLine(0), "hello there\n");

            // Undo once, should get the line without " there".
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Undo again, nothing should happen.
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Redo once, should get the line with " there".
            textCore.RedoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello there\n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Redo again, nothing should happen.
            textCore.RedoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello there\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestTypingPastingSameLine()
        {
            // Place the cursor right after "hello".
            textCore.SetCursorPosition(5, 0);
            Assert.AreEqual(textCore.GetLine(0), "hello\n");

            // Typing in " there" with a space.
            textCore.InsertText(" ");
            textCore.InsertText("t");
            textCore.InsertText("h");
            textCore.InsertText("e");
            textCore.InsertText("r");
            textCore.InsertText("e");
            textCore.InsertText(" ");
            Assert.AreEqual(textCore.GetLine(0), "hello there \n");

            // Copying "cruel" text from the third line.
            textCore.SetCursorPosition(2, 2);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(7, 2);
            Assert.AreEqual(textCore.SelectionText, "cruel");
            textCore.DoCopyText(false);

            // Paste onto the first line after "there".
            textCore.SetCursorPosition(13, 0);
            textCore.DoPasteText();
            Assert.AreEqual(textCore.GetLine(0), "hello there cruel\n");

            // Undo once, should get the line without "cruel".
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello there \n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Undo again, should get the line without " there".
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Undo again, nothing should happen.
            textCore.UndoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello\n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Redo once, should get the line with " there".
            textCore.RedoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello there \n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Redo again, should get the line with "cruel".
            textCore.RedoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello there cruel\n");
            Assert.AreEqual(textCore.LineCount, 4);

            // Redo again, nothing should happen.
            textCore.RedoEditing();
            Assert.AreEqual(textCore.GetLine(0), "hello there cruel\n");
            Assert.AreEqual(textCore.LineCount, 4);
        }

        [Test, RequiresSTA]
        public void TestSelectAllClearText()
        {
            ClearExistingContent();
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            Assert.AreEqual(0, textBuffer.GetLineCount());
            Assert.AreEqual(string.Empty, textBuffer.GetContent());
        }

        [Test, RequiresSTA]
        public void TestDeleteAllUndoCrash()
        {
            ClearExistingContent();

            System.Windows.Clipboard.SetText("a = 1;\r\nb = 2;\r\n");
            textCore.SetCursorPosition(0, 0);
            textCore.DoPasteText();
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            Assert.AreEqual(3, textBuffer.GetLineCount());

            textCore.SelectAllText();
            Assert.AreEqual(0, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(0, textCore.SelectionEnd.X);
            Assert.AreEqual(2, textCore.SelectionEnd.Y);

            textCore.DoControlCharacter(System.Windows.Input.Key.Delete);
            Assert.AreEqual(0, textBuffer.GetLineCount());
            Assert.AreEqual(string.Empty, textBuffer.GetContent());
            Assert.AreEqual(0, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);

            textCore.UndoEditing();
            Assert.AreEqual(3, textBuffer.GetLineCount());
            Assert.AreEqual("a = 1;\nb = 2;\n", textBuffer.GetContent());
            Assert.AreEqual(0, textCore.CursorPosition.X);
            Assert.AreEqual(2, textCore.CursorPosition.Y);

            textCore.RedoEditing();
            Assert.AreEqual(0, textBuffer.GetLineCount());
            Assert.AreEqual(string.Empty, textBuffer.GetContent());
            Assert.AreEqual(0, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void TestSelectFragmentMoveUp()
        {
            textCore.SetCursorPosition(1, 3);
            Assert.AreEqual(4, textCore.VisualOffset);
            textCore.SelectFragment(1, 3);
            Assert.AreEqual(9, textCore.VisualOffset);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(9, textCore.VisualOffset);
            Assert.AreEqual(3, textCore.CursorPosition.X);
            Assert.AreEqual(2, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void TestSelectLinesMoveUp()
        {
            textCore.SetCursorPosition(2, 2);
            Assert.AreEqual(8, textCore.VisualOffset);
            textCore.SelectLines(2, 1);
            Assert.AreEqual(9, textCore.VisualOffset);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(9, textCore.VisualOffset);
            Assert.AreEqual(3, textCore.CursorPosition.X);
            Assert.AreEqual(2, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void TestInsertTextMoveUp()
        {
            textCore.SetCursorPosition(6, 3);
            Assert.AreEqual(9, textCore.VisualOffset);
            textCore.InsertText("!!!");
            Assert.AreEqual(12, textCore.VisualOffset);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(12, textCore.VisualOffset);
            Assert.AreEqual(6, textCore.CursorPosition.X);
            Assert.AreEqual(2, textCore.CursorPosition.Y);

        }

        [Test, RequiresSTA]
        public void TestDoPasteTextMoveUp()
        {
            textCore.SetCursorPosition(0, 0);
            Assert.AreEqual(0, textCore.VisualOffset);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(5, 0);
            Assert.AreEqual(textCore.GetSelectionText(), "hello");
            textCore.DoCopyText(false);
            textCore.SetCursorPosition(6, 3);
            Assert.AreEqual(9, textCore.VisualOffset);
            textCore.DoPasteText();
            Assert.AreEqual(14, textCore.VisualOffset);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(14, textCore.VisualOffset);
            Assert.AreEqual(textCore.CursorPosition.X, 7);
            Assert.AreEqual(textCore.CursorPosition.Y, 2);
        }

        [Test, RequiresSTA]
        public void SelectAllTextMoveUp()
        {
            textCore.SetCursorPosition(0, 0);
            Assert.AreEqual(0, textCore.VisualOffset);
            textCore.SelectAllText();
            Assert.AreEqual(9, textCore.VisualOffset);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(9, textCore.VisualOffset);
            Assert.AreEqual(textCore.CursorPosition.X, 3);
            Assert.AreEqual(textCore.CursorPosition.Y, 2);
        }

        [Test, RequiresSTA]
        public void UndoEditingMoveUp()
        {
            textCore.SetCursorPosition(1, 0);
            Assert.AreEqual(1, textCore.VisualOffset);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(3, 2);
            Assert.AreEqual(9, textCore.VisualOffset);
            textCore.MoveSelectedText(2, 3, false);
            Assert.AreEqual(textCore.GetLine(3), "\t\tcorld");
            Assert.AreEqual(textCore.GetLine(2), "\n");
            Assert.AreEqual(textCore.GetLine(1), "\twello\n");
            Assert.AreEqual(textCore.GetLine(0), "hruel\n");
            textCore.UndoEditing();
            Assert.AreEqual(9, textCore.VisualOffset);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(9, textCore.VisualOffset);
            Assert.AreEqual(textCore.CursorPosition.X, 0);
            Assert.AreEqual(textCore.CursorPosition.Y, 1);
        }

        [Test, RequiresSTA]
        public void RedoEditingMoveUp()
        {
            textCore.SetCursorPosition(1, 3);
            Assert.AreEqual(4, textCore.VisualOffset);
            textCore.InsertText("Bad ");
            Assert.AreEqual(8, textCore.VisualOffset);
            textCore.UndoEditing();
            Assert.AreEqual(4, textCore.VisualOffset);
            textCore.RedoEditing();
            Assert.AreEqual(8, textCore.VisualOffset);
            textCore.DoNavigation(System.Windows.Input.Key.Up);
            Assert.AreEqual(8, textCore.VisualOffset);
            Assert.AreEqual(textCore.CursorPosition.X, 2);
            Assert.AreEqual(textCore.CursorPosition.Y, 2);
        }

        [Test, RequiresSTA]
        public void PartialSelectionInvalidForward()
        {
            // Pre-conditions...
            Assert.AreEqual(0, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(0, textCore.SelectionEnd.X);
            Assert.AreEqual(0, textCore.SelectionEnd.Y);

            int startColumn = -100;
            int startLine = -100;
            int endColumn = 1000;
            int endLine = 1000;
            textCore.SelectPartial(startColumn, startLine, endColumn, endLine);

            // Post-conditions...
            Assert.AreEqual(0, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(6, textCore.SelectionEnd.X);
            Assert.AreEqual(3, textCore.SelectionEnd.Y);
            Assert.AreEqual(6, textCore.CursorPosition.X);
            Assert.AreEqual(3, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void PartialSelectionInvalidBackward()
        {
            // Pre-conditions...
            Assert.AreEqual(0, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(0, textCore.SelectionEnd.X);
            Assert.AreEqual(0, textCore.SelectionEnd.Y);

            int startColumn = 1000;
            int startLine = 1000;
            int endColumn = -100;
            int endLine = -100;
            textCore.SelectPartial(startColumn, startLine, endColumn, endLine);

            // Post-conditions...
            Assert.AreEqual(0, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(6, textCore.SelectionEnd.X);
            Assert.AreEqual(3, textCore.SelectionEnd.Y);
            Assert.AreEqual(0, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void PartialSelectionValidForward()
        {
            // Pre-conditions...
            Assert.AreEqual(0, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(0, textCore.SelectionEnd.X);
            Assert.AreEqual(0, textCore.SelectionEnd.Y);

            int startColumn = 3;
            int startLine = 0;
            int endColumn = 5;
            int endLine = 2;
            textCore.SelectPartial(startColumn, startLine, endColumn, endLine);

            // Post-conditions...
            Assert.AreEqual(3, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(5, textCore.SelectionEnd.X);
            Assert.AreEqual(2, textCore.SelectionEnd.Y);
            Assert.AreEqual(5, textCore.CursorPosition.X);
            Assert.AreEqual(2, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void PartialSelectionValidBackward()
        {
            // Pre-conditions...
            Assert.AreEqual(0, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(0, textCore.SelectionEnd.X);
            Assert.AreEqual(0, textCore.SelectionEnd.Y);

            int startColumn = 5;
            int startLine = 2;
            int endColumn = 3;
            int endLine = 0;
            textCore.SelectPartial(startColumn, startLine, endColumn, endLine);

            // Post-conditions...
            Assert.AreEqual(3, textCore.SelectionStart.X);
            Assert.AreEqual(0, textCore.SelectionStart.Y);
            Assert.AreEqual(5, textCore.SelectionEnd.X);
            Assert.AreEqual(2, textCore.SelectionEnd.Y);
            Assert.AreEqual(3, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void CommentLinesUsingSelectLines()
        {
            // Pre-conditions
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            Assert.AreEqual(textCore.GetLine(3), "\tworld");

            textCore.SelectLines(2, 1);
            textCore.CommentLines(true);

            // Post-conditions
            Assert.AreEqual(textCore.GetLine(2), "\t\t//cruel\n");
            Assert.AreEqual(textCore.GetLine(3), "\t//world");
        }

        [Test, RequiresSTA]
        public void UncommentLinesUsingSelectLines()
        {
            textCore.SelectLines(2, 1);
            textCore.CommentLines(true);

            // Pre-conditions
            Assert.AreEqual(textCore.GetLine(2), "\t\t//cruel\n");
            Assert.AreEqual(textCore.GetLine(3), "\t//world");

            textCore.CommentLines(false);

            // Post-conditions
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
            Assert.AreEqual(textCore.GetLine(3), "\tworld");
        }

        [Test, RequiresSTA]
        public void CommentLinesWithCursorSelection()
        {
            int startColumn = 2;
            int startLine = 2;
            int endColumn = 4;
            int endLine = 3;
            textCore.SelectPartial(startColumn, startLine, endColumn, endLine);

            Assert.AreEqual(textCore.SelectionText, "cruel\n\twor");

            textCore.CommentLines(true);

            Assert.AreEqual(textCore.SelectionText, "\t\t//cruel\n\t//world");
        }

        [Test, RequiresSTA]
        public void UncommentLinesWithCursorSelection()
        {
            int startColumn = 2;
            int startLine = 2;
            int endColumn = 4;
            int endLine = 3;
            textCore.SelectPartial(startColumn, startLine, endColumn, endLine);
            textCore.CommentLines(true);

            Assert.AreEqual(textCore.SelectionText, "\t\t//cruel\n\t//world");

            textCore.SetCursorPosition(2, 2);
            textCore.SelectPartial(startColumn, startLine, endColumn, endLine);
            textCore.CommentLines(false);

            Assert.AreEqual(textCore.SelectionText, "\t\tcruel\n\tworld");
        }

        [Test, RequiresSTA]
        public void CommentLineWithCursorPosition()
        {
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");

            textCore.SetCursorPosition(2, 2);
            textCore.CommentLines(true);

            Assert.AreEqual(textCore.GetLine(2), "\t\t//cruel\n");
        }

        [Test, RequiresSTA]
        public void UncommentLineWithCursorPosition()
        {
            textCore.SetCursorPosition(2, 2);
            textCore.CommentLines(true);
            Assert.AreEqual(textCore.GetLine(2), "\t\t//cruel\n");

            textCore.SetCursorPosition(2, 2);
            textCore.CommentLines(false);
            Assert.AreEqual(textCore.GetLine(2), "\t\tcruel\n");
        }

        [Test, RequiresSTA]
        public void InsertTextForwardSelection()
        {
            ClearExistingContent();

            // We are making sure the smart formatting is enabled at this time.
            Assert.AreEqual(true, textCore.TextEditorSettings.EnableSmartFormatting);

            textCore.SetCursorPosition(0, 0);
            textCore.InsertText("length=calculateLength(first,second);\n");

            // Smart formatting should automatically format the line.
            Assert.AreEqual("length = calculateLength(first, second);\n", textCore.GetLine(0));

            // Select the word "calculate" forward.
            textCore.SetCursorPosition(9, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(18, 0);

            Assert.AreEqual("calculate", textCore.SelectionText);

            // Type in "calc".
            textCore.InsertText("calc");
            Assert.AreEqual(13, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);
            Assert.AreEqual("length = calcLength(first, second);\n", textCore.GetLine(0));
        }

        [Test, RequiresSTA]
        public void InsertTextBackwardSelection()
        {
            ClearExistingContent();

            // We are making sure the smart formatting is enabled at this time.
            Assert.AreEqual(true, textCore.TextEditorSettings.EnableSmartFormatting);

            textCore.SetCursorPosition(0, 0);
            textCore.InsertText("length=calculateLength(first,second);\n");

            // Smart formatting should automatically format the line.
            Assert.AreEqual("length = calculateLength(first, second);\n", textCore.GetLine(0));

            // Select the word "calculate" backward.
            textCore.SetCursorPosition(18, 0);
            textCore.SetOverrideModifierFlag(TextEditorCommand.Modifier.Shift);
            textCore.SetCursorPosition(9, 0);

            Assert.AreEqual("calculate", textCore.SelectionText);

            // Type in "calc".
            textCore.InsertText("calc");
            Assert.AreEqual(13, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);
            Assert.AreEqual("length = calcLength(first, second);\n", textCore.GetLine(0));
        }

        [Test, RequiresSTA]
        public void InsertTextFormattingCursorPosition()
        {
            ClearExistingContent();

            // We are making sure the smart formatting is enabled at this time.
            Assert.AreEqual(true, textCore.TextEditorSettings.EnableSmartFormatting);

            textCore.SetCursorPosition(0, 0);
            textCore.InsertText("a={1=>a;3=>a};\n");

            // Smart formatting should automatically format the line.
            Assert.AreEqual("a = { 1 => a; 3 => a };\n", textCore.GetLine(0));

            // Placing cursor after "1 => a;".
            textCore.SetCursorPosition(13, 0);
            Assert.AreEqual(13, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);

            // Type in string "2=>a" (without semi-colon).
            textCore.InsertText('2');
            textCore.InsertText('=');
            textCore.InsertText('>');
            textCore.InsertText('a');
            Assert.AreEqual("a = { 1 => a;2=>a 3 => a };\n", textCore.GetLine(0));

            // Cursor should be right after 'a' before '3'.
            Assert.AreEqual(17, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);

            // Now punch in ';' to force a smart-format.
            textCore.InsertText(';');
            Assert.AreEqual("a = { 1 => a; 2 => a; 3 => a };\n", textCore.GetLine(0));

            // Cursor should remain right after ';' instead of jumping to the end.
            Assert.AreEqual(21, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);
        }

        [Test, RequiresSTA]
        public void InsertBlockCommentFormatting01()
        {
            ClearExistingContent();

            // We are making sure the smart formatting is enabled at this time.
            Assert.AreEqual(true, textCore.TextEditorSettings.EnableSmartFormatting);
            Assert.AreEqual(0, textCore.CurrentTextBuffer.GetLineCount());

            textCore.SetCursorPosition(0, 0);
            textCore.InsertText('/');
            Assert.AreEqual("/", textCore.GetLine(0));
            textCore.InsertText('*');
            Assert.AreEqual("/*", textCore.GetLine(0));
            Assert.AreEqual(1, textCore.CurrentTextBuffer.GetLineCount());

            textCore.InsertText('\n');
            Assert.AreEqual("/*\n", textCore.GetLine(0));
            Assert.AreEqual(2, textCore.CurrentTextBuffer.GetLineCount());
        }

        [Test, RequiresSTA]
        public void InsertBlockCommentFormatting02()
        {
            ClearExistingContent();

            // We are making sure the smart formatting is enabled at this time.
            Assert.AreEqual(true, textCore.TextEditorSettings.EnableSmartFormatting);
            Assert.AreEqual(0, textCore.CurrentTextBuffer.GetLineCount());

            textCore.SetCursorPosition(0, 0);
            textCore.InsertText("/**/");
            Assert.AreEqual(4, textCore.CursorPosition.X);
            Assert.AreEqual(0, textCore.CursorPosition.Y);
            Assert.AreEqual("/**/", textCore.GetLine(0));
            Assert.AreEqual(1, textCore.CurrentTextBuffer.GetLineCount());

            textCore.SetCursorPosition(2, 0);
            textCore.InsertText('\n'); // Hit <Enter> key.
            Assert.AreEqual(2, textCore.CurrentTextBuffer.GetLineCount());
            Assert.AreEqual("/*\n", textCore.GetLine(0));
            Assert.AreEqual("    */", textCore.GetLine(1));
        }

        private void ClearExistingContent()
        {
            textCore.SelectAllText();
            textCore.DoControlCharacter(System.Windows.Input.Key.Delete);
        }
    }
}

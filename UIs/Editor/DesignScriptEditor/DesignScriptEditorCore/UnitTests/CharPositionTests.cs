using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class CharPositionTests
    {
        public CharPosition coordinates;
        [SetUp]
        public void Setup()
        {
            TextBuffer textBuffer = new TextBuffer("\thello");
            coordinates = new CharPosition(textBuffer);
        }

        [Test]
        public void TestCharPosition01()
        {
            //Test to make sure origin is correct
            //Character coordinates input: (0,0)
            //Screen coordinates expected: (Configurations.LeftMargin, 0)

            coordinates.SetCharacterPosition(new System.Drawing.Point(0, 0));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft, 0));
        }

        [Test]
        public void TestCharPosition02()
        {
            //Test to see if effect of negative coordinates is treated as origin
            //Character coordinates input: (-10,-10)
            //Screen coordinates expected: (Configurations.LeftMargin+Configurations.FormatFontWidth, 0)

            coordinates.SetCharacterPosition(new System.Drawing.Point(-10, -10));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft, 0));
        }

        [Test]
        public void TestCharPosition03()
        {
            //Test to see if a tab space character returns correct screen coordinates
            //Character coordinates input: (1,0)
            //Screen coordinates expected: (Configurations.LeftMargin+Configurations.FormatFontWidth*Configurations.TabSpaces.Length, 0)

            coordinates.SetCharacterPosition(new System.Drawing.Point(1, 0));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point((Configurations.CanvasMarginLeft + Configurations.FormatFontWidth * Configurations.TabSpaces.Length), 0));
        }

        [Test]
        public void TestCharPosition04()
        {
            //Test to see if a line with tabspaces and normal characters give correct character coordinates
            //Screen coordinates input: (Configurations.LeftMargin+(Configurations.FormatFontWidth*Configurations.TabSpaces.Length) +Configurations.FormatFontWidth, 0)
            //Character coordinates expected: (2,0)

            coordinates.SetScreenPosition(new Point(Configurations.CanvasMarginLeft + (Configurations.FormatFontWidth * Configurations.TabSpaces.Length) + Configurations.FormatFontWidth, 0));
            Assert.IsTrue((System.Drawing.Point)coordinates.GetCharacterPosition() == new System.Drawing.Point(2, 0));
        }

        [Test]
        public void TestCharPosition05()
        {
            //Test to see if effect of negative coordinates is treated as origin
            //Character coordinates input: (-10, 0)

            coordinates.SetCharacterPosition(new System.Drawing.Point(-10, 0));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft, 0));
        }

        [Test]
        public void TestCharPosition06()
        {
            //Character coordinates input: (-10, 2)

            coordinates.SetCharacterPosition(new System.Drawing.Point(-10, 2));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft, 0));
        }

        [Test]
        public void TestCharPosition07()
        {
            //Character coordinates input: (2, 2)

            coordinates.SetCharacterPosition(new System.Drawing.Point(2, 2));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft + (Configurations.FormatFontWidth * Configurations.TabSpaces.Length) + Configurations.FormatFontWidth, 0));
        }

        [Test]
        public void TestCharPosition08()
        {
            //Character coordinates input: (7, 2)

            coordinates.SetCharacterPosition(new System.Drawing.Point(7, 2));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft + (Configurations.FormatFontWidth * Configurations.TabSpaces.Length) + (5 * Configurations.FormatFontWidth), 0));
        }

        [Test]
        public void TestCharPosition09()
        {
            //Character coordinates input: (7, 0)

            coordinates.SetCharacterPosition(new System.Drawing.Point(7, 0));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft + (Configurations.FormatFontWidth * Configurations.TabSpaces.Length) + (5 * Configurations.FormatFontWidth), 0));
        }

        [Test]
        public void TestCharPosition10()
        {
            //Character coordinates input: (7, -10)

            coordinates.SetCharacterPosition(new System.Drawing.Point(7, 10));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft + (Configurations.FormatFontWidth * Configurations.TabSpaces.Length) + (5 * Configurations.FormatFontWidth), 0));
        }

        [Test]
        public void TestCharPosition11()
        {
            //Character coordinates input: (2, -10)

            coordinates.SetCharacterPosition(new System.Drawing.Point(2, -10));
            Assert.IsTrue((System.Windows.Point)coordinates.GetScreenPosition() == new Point(Configurations.CanvasMarginLeft + (Configurations.FormatFontWidth * Configurations.TabSpaces.Length) + (Configurations.FormatFontWidth), 0));
        }

        [Test]
        public void TestCharPosition12()
        {
            // Test the additional zero-length line beyond the last line.
            TextBuffer textBuffer = new TextBuffer("\thello\n");
            coordinates = new CharPosition(textBuffer);

            // Test out clicking on the bottom-left corner of invalid area.
            coordinates.SetCharacterPosition(new System.Drawing.Point(-100, 500));
            Assert.AreEqual(0, coordinates.GetCharacterPosition().X);
            Assert.AreEqual(1, coordinates.GetCharacterPosition().Y);
        }

        [Test]
        public void TestCharPosition13()
        {
            // Test the additional zero-length line beyond the last line.
            TextBuffer textBuffer = new TextBuffer("\thello\n");
            coordinates = new CharPosition(textBuffer);

            // Test out clicking on the bottom-middle part of invalid area.
            coordinates.SetCharacterPosition(new System.Drawing.Point(3, 500));
            Assert.AreEqual(0, coordinates.GetCharacterPosition().X);
            Assert.AreEqual(1, coordinates.GetCharacterPosition().Y);
        }

        [Test]
        public void TestCharPosition14()
        {
            // Test the additional zero-length line beyond the last line.
            TextBuffer textBuffer = new TextBuffer("\thello\n");
            coordinates = new CharPosition(textBuffer);

            // Test out clicking on the bottom-right corner of invalid area.
            coordinates.SetCharacterPosition(new System.Drawing.Point(300, 5));
            Assert.AreEqual(0, coordinates.GetCharacterPosition().X);
            Assert.AreEqual(1, coordinates.GetCharacterPosition().Y);
        }

        [Test]
        public void TestCharPosition15()
        {
            // Test the additional zero-length line beyond the last line.
            TextBuffer textBuffer = new TextBuffer("\thello\n");
            coordinates = new CharPosition(textBuffer);

            // Test out clicking on the bottom-left corner of invalid area.
            coordinates.SetScreenPosition(new System.Windows.Point(-1000.0, 500.0));
            Assert.AreEqual(0, coordinates.GetCharacterPosition().X);
            Assert.AreEqual(1, coordinates.GetCharacterPosition().Y);
        }

        [Test]
        public void TestCharPosition16()
        {
            // Test the additional zero-length line beyond the last line.
            TextBuffer textBuffer = new TextBuffer("\thello\n");
            coordinates = new CharPosition(textBuffer);

            // Test out clicking on the bottom-middle part of invalid area.
            coordinates.SetScreenPosition(new System.Windows.Point(7 * Configurations.FormatFontWidth, 500.0));
            Assert.AreEqual(0, coordinates.GetCharacterPosition().X);
            Assert.AreEqual(1, coordinates.GetCharacterPosition().Y);
        }

        [Test]
        public void TestCharPosition17()
        {
            // Test the additional zero-length line beyond the last line.
            TextBuffer textBuffer = new TextBuffer("\thello\n");
            coordinates = new CharPosition(textBuffer);

            // Test out clicking on the bottom-right corner of invalid area.
            coordinates.SetScreenPosition(new System.Windows.Point(3000.0, 500.0));
            Assert.AreEqual(0, coordinates.GetCharacterPosition().X);
            Assert.AreEqual(1, coordinates.GetCharacterPosition().Y);
        }

        [Test]
        public void TestCharPosition18()
        {
            //Testing that tab is converted to 4 characters by chartovisualoffset
            TextBuffer textBuffer = new TextBuffer("hello\n\n\t\tcruel\n\tworld");
            coordinates = new CharPosition(textBuffer);
            int offset = coordinates.CharToVisualOffset(2, 2);
            Assert.AreEqual(8, offset);
        }

        [Test]
        public void TestCharPosition19()
        {
            //Testing CharToVisualOffset if the line is empty
            TextBuffer textBuffer = new TextBuffer("hello\n\n\t\tcruel\n\tworld");
            coordinates = new CharPosition(textBuffer);
            int offset = coordinates.CharToVisualOffset(4, 4);
            Assert.AreEqual(0, offset);
        }

        [Test]
        public void TestCharPosition20()
        {
            //Testing VisualToCharOffset for tab character
            TextBuffer textBuffer = new TextBuffer("hello\n\n\t\tcruel\n\tworld");
            coordinates = new CharPosition(textBuffer);
            int offset = coordinates.CharToVisualOffset(2, 2);
            Assert.AreEqual(8, offset);
            int charoffset = coordinates.VisualToCharOffset(2, offset);
            Assert.AreEqual(2, charoffset);

        }

        [Test]
        public void TestCharPosition21()
        {
            //Testing VisualToCharOffset if line is empty
            TextBuffer textBuffer = new TextBuffer("hello\n\n\t\tcruel\n\tworld");
            coordinates = new CharPosition(textBuffer);
            int offset = coordinates.CharToVisualOffset(4, 4);
            int charoffset = coordinates.VisualToCharOffset(4, offset);
            Assert.AreEqual(0, charoffset);
        }

        [Test]
        public void TestCharPosition22()
        {
            TextBuffer textBuffer = new TextBuffer("hello\n\n\t\tcruel\n\tworld");
            coordinates = new CharPosition(textBuffer);
            int offset = coordinates.CharToVisualOffset(2, -1);
            Assert.AreEqual(0, offset);
            int charoffset = coordinates.VisualToCharOffset(2, offset);
            Assert.AreEqual(0, charoffset);
        }

        [Test]
        public void TestCharPositionToNonSpaceCharCount()
        {
            // \tb = a + 3;
            // \t    c\t = a + \t b;
            TextBuffer textBuffer = new TextBuffer("\tb = a + 3;\n\t    c\t = a + \t b;");
            CharPosition position = new CharPosition(textBuffer);

            position.SetCharacterPosition(0, 0);
            Assert.AreEqual(0, position.ToNonSpaceCharCount());
            position.SetCharacterPosition(1, 0);
            Assert.AreEqual(0, position.ToNonSpaceCharCount());
            position.SetCharacterPosition(2, 0);
            Assert.AreEqual(1, position.ToNonSpaceCharCount());
            position.SetCharacterPosition(8, 0);
            Assert.AreEqual(4, position.ToNonSpaceCharCount());

            position.SetCharacterPosition(0, 1);
            Assert.AreEqual(0, position.ToNonSpaceCharCount());
            position.SetCharacterPosition(5, 1);
            Assert.AreEqual(0, position.ToNonSpaceCharCount());
            position.SetCharacterPosition(6, 1);
            Assert.AreEqual(1, position.ToNonSpaceCharCount());
            position.SetCharacterPosition(17, 1);
            Assert.AreEqual(5, position.ToNonSpaceCharCount());
        }

        [Test]
        public void TestCharPositionFromNonSpaceCharCount()
        {
            // \tb = a + 3;
            // \t    c\t = a + \t b;
            TextBuffer textBuffer = new TextBuffer("\tb = a + 3;\n\t    c\t = a + \t b;");
            CharPosition position = new CharPosition(textBuffer);

            position.SetCharacterPosition(0, 0); // First line.
            position.FromNonSpaceCharCount(1);
            Assert.AreEqual(2, position.CharacterX);
            position.FromNonSpaceCharCount(4);
            Assert.AreEqual(8, position.CharacterX);

            position.SetCharacterPosition(0, 1); // Second line.
            position.FromNonSpaceCharCount(1);
            Assert.AreEqual(6, position.CharacterX);
            position.FromNonSpaceCharCount(3);
            Assert.AreEqual(11, position.CharacterX);
            position.FromNonSpaceCharCount(5);
            Assert.AreEqual(17, position.CharacterX);
        }
    }
}

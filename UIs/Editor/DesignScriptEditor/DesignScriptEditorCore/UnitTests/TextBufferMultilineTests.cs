using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class TextBufferMultilineTests
    {
        TextBuffer textBuffer;

        [SetUp]
        public void Setup()
        {
            textBuffer = new TextBuffer(
                "    var a = 12.34;\n" + 
                "    var b = 56.78;\n" + 
                "    var c = 90.11;\n" + 
                "    var d = 12345;\n" + 
                "    var e = 67890;\n");
        }

        [Test]
        public void TestGetTextInvalidRange01()
        {
            string textInRegion = textBuffer.GetText(1, 8, 3, 25);
            Assert.AreEqual("b = 56.78;\n    var c = 90.11;\n    var d = 12345;\n", textInRegion);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Core.UnitTest
{
    using NUnit.Framework;

    class ScriptFileReaderTests
    {
        [SetUp]
        public void Setup()
        {
            // Do nothing here, again?
        }

        [Test]
        public void TestWindowsFormat()
        {
            string inputString =
                "First line\r\n" +
                "Second line\r\n" +
                "Third line\r\n";

            TestTheInputStringLikeTheRealTesterWould(inputString);
        }

        [Test]
        public void TestUnitFormat()
        {
            string inputString =
                "First line\n" +
                "Second line\n" +
                "Third line\n";

            TestTheInputStringLikeTheRealTesterWould(inputString);
        }

        [Test]
        public void TestMacintoshFormat()
        {
            string inputString =
                "First line\r" +
                "Second line\r" +
                "Third line\r";

            TestTheInputStringLikeTheRealTesterWould(inputString);
        }

        private void TestTheInputStringLikeTheRealTesterWould(string fileContent)
        {
            ScriptFileReader scriptReader = new ScriptFileReader(fileContent, false);
            List<string> lines = scriptReader.ReadInput();

            Assert.AreEqual(lines[0][0], 'F');
            Assert.AreEqual(lines[1][1], 'e');
            Assert.AreEqual(lines[2][2], 'i');
        }
    }
}

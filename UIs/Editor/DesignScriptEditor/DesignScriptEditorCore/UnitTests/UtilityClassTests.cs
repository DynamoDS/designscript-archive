using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class UtilityClassTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestNegativeConstructions()
        {
            BinaryVersion version = BinaryVersion.FromString(null);
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("12");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("ab");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("12.34");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("ab.cd");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("12.34.56");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("ab.cd.ef");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("ab.cd.ef.gh");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("12.34.56.78.90");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("ab.cd.ef.gh.ij");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("12.cd.ef.gh");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("12.34.ef.gh");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("12.34.56.gi");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString(".");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("..");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("...");
            Assert.AreEqual(null, version);

            version = BinaryVersion.FromString("....");
            Assert.AreEqual(null, version);
        }

        [Test]
        public void TestPositiveConstructions()
        {
            BinaryVersion version = BinaryVersion.FromString("12.34.56.78");
            Assert.AreEqual(12, version.FileMajor);
            Assert.AreEqual(34, version.FileMinor);
            Assert.AreEqual(56, version.FileBuild);
            Assert.AreEqual(78, version.FilePrivate);

            ulong value = ((((ulong)12) << 48) & 0xffff000000000000) |
                          ((((ulong)34) << 32) & 0x0000ffff00000000) |
                          ((((ulong)56) << 16) & 0x00000000ffff0000) |
                          ((((ulong)78) << 0) & 0x000000000000ffff);

            Assert.AreEqual(value, version.Value);
        }

        [Test]
        public void TestComparisonOperators()
        {
            BinaryVersion first = BinaryVersion.FromString("12.34.56.78");
            BinaryVersion second = BinaryVersion.FromString("87.65.43.21");
            Assert.AreEqual(true, first < second);
            Assert.AreEqual(false, first > second);

            first = BinaryVersion.FromString("1000.9.9.9");
            second = BinaryVersion.FromString("1001.1.1.1");
            Assert.AreEqual(true, first < second);
            Assert.AreEqual(false, first > second);
        }

        [Test]
        public void TestEqualityOperator()
        {
            BinaryVersion first = BinaryVersion.FromString("12.34.56.78");
            BinaryVersion second = BinaryVersion.FromString("12.34.56.78");

            Assert.AreEqual(false, first < second);
            Assert.AreEqual(true, first <= second);
            Assert.AreEqual(true, first == second);
            Assert.AreEqual(false, first > second);
            Assert.AreEqual(true, first >= second);
            Assert.AreEqual(false, first != second);
        }

        [Test]
        public void TestComparisonToNull()
        {
            BinaryVersion first = BinaryVersion.FromString("12.34.56.78");
            BinaryVersion second = BinaryVersion.FromString("ab.cd.ef.gh");

            Assert.AreNotEqual(null, first);
            Assert.AreEqual(null, second);
            Assert.AreEqual(false, first < second);
            Assert.AreEqual(false, first <= second);
            Assert.AreEqual(false, first == second);
            Assert.AreEqual(false, first > second);
            Assert.AreEqual(false, first >= second);
            Assert.AreEqual(true, first != second);
        }

        [Test]
        public void TestStringRepresentation()
        {
            BinaryVersion version = BinaryVersion.FromString("12.34.56.78");
            Assert.AreNotEqual(null, version);
            Assert.AreEqual("12.34.56.78", version.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class VariableSlotInfoTest
    {
        [SetUp]
        public void SetupTest()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestEqual()
        {
            //same name, same line, equal
            IdGenerator idGenerator = new IdGenerator();
            uint slotId = idGenerator.GetNextId(ComponentType.Slot);

            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo("a", 1, slotId);
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo("a", 1, slotId);
            Assert.AreEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void TestDifferentName()
        {
            uint slotId = 0x30000001;

            //diff name, same line, not equal
            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo("a", 1, slotId);
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo("b", 1, slotId);
            Assert.AreNotEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void TestDifferentLine()
        {
            uint slotId = 0x30000001;
            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo("a", 1, slotId);
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo("a", 2, slotId);
            Assert.AreNotEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void TestDifferentSlot()
        {
            uint slotId1 = 0x30000001;
            uint slotId2 = 0x30000002;

            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo("a", 1, slotId1);
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo("a", 1, slotId2);
            Assert.AreNotEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void TestDifferentNameLine()
        {
            //diff name, diff line, not equal
            uint slotId = 0x30000001;
            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo("a", 2, slotId);
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo("b", 1, slotId);
            Assert.AreNotEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void TestNullObject()
        {
            //2 null object, equal
            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo();
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo();
            Assert.AreEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void Test1NullObject()
        {
            //1 null object, not equal
            uint slotId = 0x30000001;

            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo("a", 2, slotId);
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo();
            Assert.AreNotEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void TestCapitalLetter()
        {
            uint slotId = 0x30000001;

            //diff name, same line not equal
            VariableSlotInfo varSlotInfo1 = new VariableSlotInfo("VARIablE", 123312, slotId);
            VariableSlotInfo varSlotInfo2 = new VariableSlotInfo("variable1", 123312, slotId);
            Assert.AreNotEqual(varSlotInfo1, varSlotInfo2);
            //same name, same line, equal
            varSlotInfo1 = new VariableSlotInfo("VARIablE", 123312, slotId);
            varSlotInfo2 = new VariableSlotInfo("VARIablE", 123312, slotId);
            Assert.AreEqual(varSlotInfo1, varSlotInfo2);
            //same name, diff line, not equal
            varSlotInfo1 = new VariableSlotInfo("VARIablE", 123312, slotId);
            varSlotInfo2 = new VariableSlotInfo("VARIablE", 68678, slotId);
            Assert.AreNotEqual(varSlotInfo1, varSlotInfo2);
        }

        [Test]
        public void TestArgumentValidation()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                VariableSlotInfo v = new VariableSlotInfo("variable", -100, 0x01);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                VariableSlotInfo v = new VariableSlotInfo("", 100, 0x01);
            });

            // null, invalid slot id
        }

        [Test]
        public void TestGetProperties()
        {
            uint slotId = 0x30000001;
            string varName = "a";
            int line = 123;
            VariableSlotInfo varSlotInfo = new VariableSlotInfo(varName, line, slotId);

            Assert.AreEqual(varName, varSlotInfo.Variable);
            Assert.AreEqual(line, varSlotInfo.Line);
            Assert.AreEqual(slotId, varSlotInfo.SlotId);
        }
    }
}

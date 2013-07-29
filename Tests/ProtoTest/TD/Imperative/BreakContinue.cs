using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    class BreakContinueTest
    {
        public TestFrameWork thisTest  = new TestFrameWork();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T01_WhileBreakContinue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BreakContinue\\T01_WhileBreakContinue.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 11);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_WhileBreakContinue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BreakContinue\\T02_WhileBreakContinue.ds");

            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 40);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T03_ForLoopBreakContinue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BreakContinue\\T03_ForLoopBreakContinue.ds");

            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 55);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_ForLoopBreakContinue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BreakContinue\\T04_ForLoopBreakContinue.ds");

            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 60);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_FunctionBreakContinue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BreakContinue\\T05_FunctionBreakContinue.ds");

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionAbnormalExit);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 4);
            Assert.IsTrue(mirror.GetValue("d").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }
    }
}

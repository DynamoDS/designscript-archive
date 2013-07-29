using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    class BlockSyntax
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T01_TestImpInsideImp()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_TestImpInsideImp.ds");

                // thisTest.Verify("x", 5);
                // thisTest.Verify("y", 5);
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_TestAssocInsideImp()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T02_TestAssocInsideImp.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 35);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 35);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 5);
            Assert.IsTrue(mirror.GetValue("w").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("f").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_TestImpInsideAssoc()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T03_TestImpInsideAssoc.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 35);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("w").Payload == 10);
            Assert.IsTrue(mirror.GetValue("f").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_TestImperativeBlockWithMissingBracket_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T04_TestImperativeBlockWithMissingBracket_negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_TestImperativeBlockWithMissingBracket_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T05_TestImperativeBlockWithMissingBracket_negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_TestNestedImpBlockWithMissingBracket_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T06_TestNestedImpBlockWithMissingBracket_negative.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_TestBlockWithIncorrectBlockName_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T07_TestBlockWithIncorrectBlockName_negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T08_TestBlockWithIncorrectBlockName_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirro = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T08_TestBlockWithIncorrectBlockName_negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T09_Defect_1449829()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T09_Defect_1449829.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_Defect_1449732()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T10_Defect_1449732.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_Defect_1450174()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T11_Defect_1450174.ds");

            Assert.IsTrue((double)mirror.GetValue("c").Payload == 27.5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T12_Defect_1450599()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T12_Defect_1450599.ds");

                Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 5);
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_Defect_1450527()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T13_Defect_1450527.ds");
            thisTest.Verify("temp", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_Defect_1450550()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T14_Defect_1450550.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T15_Defect_1452044()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T15_Defect_1452044.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T16__Defect_1452588()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T16__Defect_1452588.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T17__Defect_1452588_2()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T17__Defect_1452588_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T18__Negative_Block_Syntax()
        {

            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\BlockSyntax\\T18__Negative_Block_Syntax.ds");
            });
            
        }
        [Test]
        [Category ("SmokeTest")]
 public void T19_Imperative_Nested()
        {

            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T19_Imperative_Nested_1467063.ds");
            });

        }


    }
}

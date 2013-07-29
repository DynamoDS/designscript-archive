using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    class WhileTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\";

        [SetUp]
        public void Setup()
        {
        }        

        [Test]
        [Category ("SmokeTest")]
 public void T01_NegativeSyntax_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T01_NegativeSyntax_Negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_AssociativeBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T02_AssociativeBlock_Negative.ds");
            });
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_UnnamedBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T03_UnnamedBlock_Negative.ds");
            });
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_OutsideBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T04_OutsideBlock_Negative.ds");
            });
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_WithinFunction()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T05_WithinFunction.ds");

            Assert.IsTrue((Int64)mirror.GetValue("testvar").Payload == 6);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_InsideNestedBlock()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T06_InsideNestedBlock.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 14);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_BreakStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T07_BreakStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 3);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T08_ContinueStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T08_ContinueStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 6);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T09_NestedWhileStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T09_NestedWhileStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 125);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("p").Payload == 6);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_WhilewithAssgnmtStatement_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T10_WhilewithAssgnmtStatement_Negative.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_WhilewithLogicalOperators()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T11_WhilewithLogicalOperators.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("temp2").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("temp3").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("temp4").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T12_WhileWithFunctionCall()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T12_WhileWithFunctionCall.ds");

            Assert.IsTrue((Int64)mirror.GetValue("testvar").Payload == 7);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_DoWhileStatment_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T13_DoWhileStatment_negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_TestFactorialUsingWhileStmt()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T14_TestFactorialUsingWhileStmt.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("factorial_a").Payload == 720);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T15_TestWhileWithDecimalvalues()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T15_TestWhileWithDecimalvalues.ds");

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 324.84375);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T16_TestWhileWithLogicalOperators()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T16_TestWhileWithLogicalOperators.ds");
           
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 5.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 59.0625);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T17_TestWhileWithBool()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T17_TestWhileWithBool.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T18_TestWhileWithNull()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T18_TestWhileWithNull.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T19_TestWhileWithIf()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\T19_TestWhileWithIf.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 15);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T20_Test()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\WhileStatement\\test.ds");

      

        }

        [Test]
       
 public void T20_TestWhileToCreate2DimArray()
        {
            // Assert.Fail("1463672 - Sprint 20 : rev 2140 : 'array' seems to be reserved as a keyword in a specific case ! ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T20_TestWhileToCreate2DimArray.ds");
            Object[] v1 = new Object[] { new Object[]{ 1, 2 },  new Object[]{ 1, 2 } };

            thisTest.Verify("x", v1);
           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_TestWhileToCallFunctionWithNoReturnType()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T21_TestWhileToCallFunctionWithNoReturnType.ds");
         
            thisTest.Verify("x", 1);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_Defect_1463683()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T22_Defect_1463683.ds");
         
            thisTest.Verify("x", 1);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_Defect_1463683_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T22_Defect_1463683_2.ds");

            thisTest.Verify("x", 1);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 3);

            thisTest.Verify("y", 1);
            thisTest.Verify("y1", 1);
            thisTest.Verify("y2", 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_Defect_1463683_3()
        {
            string errmsg = "1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2)";
            thisTest.VerifyRunScriptFile(testPath, "T22_Defect_1463683_3.ds", errmsg);

            Object[] v1 = new Object[] { 1, 3, 4 };
            
            thisTest.Verify("x", v1);
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", 0);

            thisTest.Verify("y", v1);
            thisTest.Verify("y1", v1);
            thisTest.Verify("y2", 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_Defect_1463683_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T22_Defect_1463683_4.ds");

            thisTest.Verify("x1", 3);
            thisTest.Verify("x2", 1);

        }
    
      

        

       

    }
}


using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Associative
{
    public class Assignment
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_SampleTestUsingCodeWithinTestFunction()
        {

            String code =
                @"variable;[Associative]
                {
	                variable = 5;
                }
                ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("variable");
            Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_SampleTestUsingCodeFromExternalFile()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T02_SampleTestUsingCodeFromExternalFile.ds");

            Assert.IsTrue((Int64)mirror.GetValue("variable").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_TestAssignmentToUndefinedVariables_negative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T03_TestAssignmentToUndefinedVariables_negative.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_TestAssignmentStmtInExpression_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T04_TestAssignmentStmtInExpression_negative.ds");

                // expected "StatementUsedInAssignment" warning
                Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T05_TestRepeatedAssignment()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T05_TestRepeatedAssignment.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_TestInUnnamedBlock_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T06_TestInUnnamedBlock_negative.ds");

                Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T07_TestOutsideBlock()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T07_TestOutsideBlock.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);

        }

        [Test]
        [Category("Update")]
        public void T08_TestCyclicReference()
        {
            String errmsg = "1460274 - Sprint 18 : rev 1590 : Update issue : Cyclic dependency cases are going into infinite loop";
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T08_TestCyclicReference.ds");
            thisTest.VerifyRunScriptFile(testPath, "T08_TestCyclicReference.ds", errmsg);
            thisTest.Verify("b",null);
            thisTest.Verify("a", null);
         }

        [Test]
        [Category("SmokeTest")]
        public void T09_TestInNestedBlock()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T09_TestInNestedBlock.ds");
            thisTest.Verify("a", 4);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);
            Assert.IsTrue(mirror.GetValue("g3").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_TestInFunctionScope()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T10_TestInFunctionScope.ds");

            Assert.IsTrue((double)mirror.GetValue("test").Payload == 4.5);


        }

        [Test]
        [Category("SmokeTest")]
        public void T11_TestInClassScope()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T11_TestInClassScope.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b1").Payload == 2);


        }

        [Test]
        [Category("SmokeTest")]
        public void T12_TestUsingMathAndLogicalExpr()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
              {
                  ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T12_TestUsingMathAndLogicalExpr.ds");

                  Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("b").Payload) == 5);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("c").Payload) == 1);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("d").Payload) == 7);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("e").Payload) == 3);
              });
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_TestUsingMathAndLogicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T13_TestUsingMathAndLogicalExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 3.5);
            Assert.IsTrue((double)mirror.GetValue("f").Payload == -0.66666666666666663);


        }

        [Test]
        [Category("SmokeTest")]
        public void T14_TestUsingMathAndLogicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T14_TestUsingMathAndLogicalExpr.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("f").Payload) == 0);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c1").Payload));
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c2").Payload) == false);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c3").Payload) == false);


        }

        [Test]
        [Category("SmokeTest")]
        public void T15_TestInRecursiveFunctionScope()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T15_TestInRecursiveFunctionScope.ds");

            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 120);



        }

        [Test]
        [Category("SmokeTest")]
        public void T16_TestInvalidSyntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T16_TestInvalidSyntax.ds");
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T17_TestInvalidSyntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T17_TestInvalidSyntax.ds");
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T18_TestMethodCallInExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T18_TestMethodCallInExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("test0").Payload == 6.5); 
            Assert.IsTrue((double)mirror.GetValue("test1").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test2").Payload == 7.0);
            Assert.IsTrue((double)mirror.GetValue("test3").Payload == 7.5);
            Assert.IsTrue((double)mirror.GetValue("test4").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test5").Payload == 7.0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T19_TestAssignmentToCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T19_TestAssignmentToCollection.ds");

            Assert.IsTrue((double)mirror.GetValue("b").Payload == 8.5);
            //Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T20_TestInvalidSyntax()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T20_TestInvalidSyntax.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);


        }

        [Test]
        [Category("SmokeTest")]
        public void T21_TestAssignmentToBool()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T21_TestAssignmentToBool.ds");
            // need to capture the warning

            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("a").Payload));
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b").Payload) == false);


        }

        [Test]
        [Category("SmokeTest")]
        public void T22_TestAssignmentToNegativeNumbers()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T22_TestAssignmentToNegativeNumbers.ds");
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -111);
            Assert.IsTrue((double)mirror.GetValue("c").Payload == -0.1);
            Assert.IsTrue((double)mirror.GetValue("d").Payload == -1.99);
            Assert.IsTrue((double)mirror.GetValue("e").Payload == 1.99);

        }

        [Test]
        [Category("SmokeTest")]
        public void T23_TestUsingMathAndLogicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T23_TestUsingMathAndLogicalExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("c1").Payload == -7.5);
            Assert.IsTrue((double)mirror.GetValue("c2").Payload == 0.5);
            Assert.IsTrue((double)mirror.GetValue("c3").Payload == 14.0);
            Assert.IsTrue((double)mirror.GetValue("c4").Payload == 0.875);


        }

        [Test]
        [Category("SmokeTest")]
        public void T24_TestUsingMathematicalExpr()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T24_TestUsingMathematicalExpr.ds");
            thisTest.Verify("c1", 5);
            thisTest.Verify("c2", 1);
            thisTest.Verify("c3", 6);
            thisTest.Verify("c4", 1.5);

        }

        [Test]
        [Category("SmokeTest")]
        public void T25_TestUsingMathematicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T25_TestUsingMathematicalExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("c1").Payload == 5);
            Assert.IsTrue((double)mirror.GetValue("c2").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("c3").Payload == 6);
            Assert.IsTrue((double)mirror.GetValue("c4").Payload == 1.5);


        }


        [Test]
        [Category("SmokeTest")]
        public void T26_Negative_TestPropertyAccessOnPrimitive()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T26_Negative_TestPropertyAccessOnPrimitive.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue(mirror.GetValue("y").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

            Assert.IsTrue((Int64)mirror.GetValue("x1").Payload == 1);
            Assert.IsTrue(mirror.GetValue("y1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);



        }



        [Test]
        [Category("SmokeTest")]
        public void T26_Defect_1450854()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T26_Defect_1450854.ds");

                Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            });
        }

        [Ignore]
        public void T27_Defect_1450847()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T27_Defect_1450847.ds");
            Assert.IsTrue((Int64)mirror.GetValue("a1").Payload == -3);
            Assert.IsTrue((Int64)mirror.GetValue("b1").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("c1").Payload == 0);
            Assert.IsTrue(mirror.GetValue("d1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("e1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("f1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Ignore]
        public void T29_Defect_1449887()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T29_Defect_1449887.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 15);


        }

        [Ignore]
        public void T30_Defect_1449887_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T30_Defect_1449887_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 15);


        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Defect_1449877()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T31_Defect_1449877.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == -6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 13);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("e").Payload) == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T32_Defect_1449877_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T32_Defect_1449877_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T33_Defect_1450003()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T33_Defect_1450003.ds");
            thisTest.Verify("_a_test", 12.5);
            thisTest.Verify("_b", 4.5);
            thisTest.Verify("_c", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Defect_1450727()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T34_Defect_1450727.ds");

            Assert.IsTrue((double)mirror.GetValue("z").Payload == -9.7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Defect_1450727_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T35_Defect_1450727_2.ds");

            Assert.IsTrue((double)mirror.GetValue("z").Payload == -8.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T36_Defect_1450555()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T36_Defect_1450555.ds");

                Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
            });
        }


        [Test]
        [Category("SmokeTest")]
        public void T37_TestOperationOnNullAndBool()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T37_TestOperationOnNullAndBool.ds");
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Defect_1449928()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T38_Defect_1449928.ds");

            Assert.IsTrue((double)mirror.GetValue("c").Payload == -0.33333333333333331);
        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Defect_1449704()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T39_Defect_1449704.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T40_Defect_1450552()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T40_Defect_1450552.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        [Category("SmokeTest")]
        public void T41__Defect_1452423()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T41__Defect_1452423.ds");

            Assert.IsTrue(mirror.GetValue("d").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T42__Defect_1452423_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T42__Defect_1452423_2.ds");
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T43__Defect_1452423_3()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T43__Defect_1452423_3.ds");

                Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T44__Defect_1452423_4()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T44__Defect_1452423_4.ds");

            Assert.IsTrue(mirror.GetValue("x").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T45__Defect_1452423_5()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T45__Defect_1452423_5.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T46_TestBooleanOperationOnNull()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T46_TestBooleanOperationOnNull.ds");

                Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 3);
            
        }

        [Test]
        [Category("SmokeTest")]
        public void T47_TestBooleanOperationOnNull()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\T47_TestBooleanOperationOnNull.ds");

                Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 0);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T48_MultipleAssignments()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T48_MultipleAssignments.ds");

            thisTest.Verify("a", 4);
            thisTest.Verify("b", 4);

            thisTest.Verify("x", 5);
            thisTest.Verify("y", 5);

            thisTest.Verify("b1", 9);

        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1455264()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T49_Defect_1455264.ds");
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T50_Defect_1456713()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T50_Defect_1456713.ds");
            thisTest.Verify("b", 6.9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Using_Special_Characters_In_Identifiers()
        {
            // Assert.Fail("1465125 - Sprint 21 : rev 2294 : @ is not allowed as a starting symbol for identifiers ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T51_Using_Special_Characters_In_Identifiers.ds");
            thisTest.Verify("@a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Negative_Associative_Syntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Negative_Associative_Syntax.ds");

            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Collection_Indexing_On_LHS_Using_Function_Call()
        {
            // Assert.Fail("DNL-1467064 - Sprint 23 : rev 2607 : array element cannot be indexed using function on the LHS of an assignment statement");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Collection_Indexing_On_LHS_Using_Function_Call.ds");
            Object[] v1 = { 3, 2 };
            thisTest.Verify("y", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T033_Wrong_Spelling_Of_Language_Block()
        {
            // Assert.Fail("DNL-1467065 - Sprint23 : rev :2610 : 'invalid hydrogen' error message coming from wrong spelling of language scope name");
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T033_Wrong_Spelling_Of_Language_Block.ds");
                });
        }

        [Test]
        [Category("SmokeTest")]
        public void T54_Associative_Nested_deffect_1467063()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T54_Associative_Nested_deffect_1467063.ds");

            });
        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Language_specifier_invalid_1467065_1()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T001_Language_specifier_invalid_1467065_1.ds");
            });

        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Language_specifier_invalid_1467065_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T001_Language_specifier_invalid_1467065_2.ds");
            });

        }
        [Test]
        [Category("Negative")]
        public void T55_Associative_assign_If_condition_1467002()
        {
            String errmsg = "1467361 - Sprint 27 - Rev 4037 - [Design Issue]conditionals with empty arrays and ararys with different ranks";

            thisTest.VerifyRunScriptFile(testPath, "T55_Associative_assign_If_condition_1467002.ds", errmsg);
           
            Object n1 = null;
            thisTest.Verify("x", n1);
            

        }

        [Test]
        [Category("Negative")]
        public void T56_Defect_1467242()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
a = 2;
b = 4;
if(a == 2)
    b = 3;
"; 
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            }); 
        }

        [Test]
        [Category("Negative")]
        public void T56_Defect_1467242_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
a = {0, 1};
for(i in a)
{
    a[i] = 0;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        public void T56_Defect_1467242_3()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
i = 0;
while(i < 3)
{
    i = i + 1;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        public void T57_Defect_1467255()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
a = 0;
b = 10;
c = 2
y1 = a..b..2;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T58_Modifier_Block_On_User_Defined_Classes()
        {
            String code =
            @"
class B
{
    x:int;
	constructor B ( x1)
	{
	    x = x1;
	}
}
class A
{
    b : B;
	d : bool;
	constructor A ( bb:B, d1:bool)
	{
	    b = bb;
		d = d1;
	}
	def foo ( x1,x2,x3)
	{
	    b1 = B.B(x1+x2+x3);
		return = A.A(b1, d);
	}
	def Scale ( xx )
	{
	    b2 = B.B(xx+b.x);
		return = A.A(b2, d);
	}
}
bb = B.B(1);
a =
{
    A.A(bb,false).foo(10,0,0) => a1;
    a1.Scale(2);
}
test = a.b.x;
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("test", 12);
        }

        [Test]
        public void T59_Defect_1467540()
        {
            String code =
            @"
c1 = 0;
x = 0..1;
y = x[c];
";
            Object n1 = null;
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", n1);
        }

        [Test]
        public void T59_Defect_1467540_2()
        {
            String code =
            @"
y;
[Imperative]
{
    c1 = 0;
    x = 0..1;
    y = x[c];
}
";
            Object n1 = null;
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", n1, 0);
        }

        [Test]
        public void T59_Defect_1467540_3()
        {
            String code =
            @"
def foo ()
{
    c1 = 0;
    x = 0..1;
    y = x[c];
    return = y;
}
test = y;
";
            Object n1 = null;
            thisTest.RunScriptSource(code);
            thisTest.Verify("test", n1);
        }

        [Test]
        public void T60_Defect_1467525()
        {
            String code =
            @"
def Unflatten : var[][](input1Darray : var[], length : int)
{
    return2Darray = { };
    
    iCount = Count(input1Darray) / length;
    index = 0;
  
    for(i in 0..iCount)
    {
        for(j in 0..length)
        {
            returm2Darray[i][j] = input1Darray[index];
            
            index = index + 1;
        }    
    }
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });           
        }

        [Test]
        public void T60_Defect_1467525_2()
        {
            String code =
            @"
c = 0;
a = 2;
for(i in 0..a)
{
    c = c +1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T60_Defect_1467525_3()
        {
            String code =
            @"
c = 0;
a = 2;
while(c < 1)
{
    c = c +1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T60_Defect_1467525_4()
        {
            String code =
            @"
c = 0;
a = 2;
if(c < 1)
{
    c = c +1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }
        [Test]
        public void T60_Defect_1467525_5()
        {
            String code =
            @"
class A
{
    c : int;
    constrcutor A ()
    {
        c = 0;
        a = 2;
        if(c < 1)
        {
            c = c +1;
        }
    }
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }
        [Test]
        public void T60_Defect_1467525_6()
        {
            String code =
            @"
class A
{
    
    def foo ()
    {
        c = 0;
        a = 2;
        if(c < 1)
        {
            c = c +1;
        }
    }
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T61_Defect_1467546_1()
        {
            String code =
            @"
a = 10
b = a + 1;
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("b", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_2()
        {
            String code =
            @"
[Imperative]
{
a = 10
b = a + 1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("b", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_3()
        {
            String code =
            @"
def foo ()
{
  a = 10
  b = a + 1;
}
test = foo();
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_4()
        {
            String code =
            @"
class A
{
    static def foo ()
    {
      a = 10
      b = a + 1;
    }
}
test = A.foo();
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_5()
        {
            String code =
            @"
class A
{
    constructor A ()
    {
      a = 10
      b = a + 1;
    }
}
test = A.A();
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_6()
        {
            String code =
            @"
x = 1;
a = 10
b = a + 1;
 
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_7()
        {
            String code =
            @"
def foo()
{
    return = 1;
}
x = 1;
a = foo()
b = a + 1;
 
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_8()
        {
            String code =
            @"
class A
{
    static def foo()
    {
        return = 1;
    }
}
x = 1;
a = A.foo()
b = a + 1;
 
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T64_Defect_1467588()
        {
            String code =
            @"
class A
{
    static def foo()
    {
        return = ""Hello \""DesignScript\""!"";
    }
}
def foo()
{
    return = ""Hello \""DesignScript\""!"";
}
def foo2(s : string)
{
    return = ""New Hello \""DesignScript\""!"";
}
a = A.foo();
b = foo();
c = [Associative]
{
    return = [Imperative]
    {
        return = ""Hello \""DesignScript\""!"";
    }
}
d = foo2(""Hello \""DesignScript\""!"");
 
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "Hello \"DesignScript\"!");
            thisTest.Verify("b", "Hello \"DesignScript\"!");
            thisTest.Verify("c", "Hello \"DesignScript\"!");
            thisTest.Verify("d", "New Hello \"DesignScript\"!");
        }
        [Test]
        public void T65_Defect_1467597()
        {
            String code =
            @"
class A 
{
    a;;
    constructor A ( x )
    {
        a = x;;
    }
    static def foo()
    {
        returnValue = 0;
        [Imperative]
        {
            for(i in { 1, 2 })
            {
                returnValue = returnValue + i;; 
            }
        }
        return = returnValue;
    }  
}
x = A.foo(); 
y = A.A(1).a;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
            thisTest.Verify("y", 1);
            
        }
        [Test]
        public void T66_Defect_1467597()
        {
            String code =
            @"
class A 
{
    a = { }; ;
    b : int[]; ;
    c = 4..6; ;
    d = { 0, 2 }; ;
    f : var[]; ;
    constructor A ( x )
    {
        a = { x }; ;
        i = 2..3; ;
        b[i] = i; ;
        c[i] = i; ;
        d[i] = i; ;
        f[i] = d[i]; ;

    }
    def foo()
    {
        returnValue = 0;
        [Imperative]
        {
            for(i in { 0,1 })
            {
                returnValue = returnValue + i; ;
                b[i] = i;;
                c[i] = i;;
                d[i] = i;;
                f[i] = d[i]; ;
                
            }
        }
        return = returnValue;
    }  
}
x = A.A(1); ;
y1 = x.a; ;
y2 = x.b; ;
y3 = x.c; ;
y4 = x.d; ;
y5 = x.f; ;
y6 = x.foo(); ;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y1", new Object[] { 1 });
            thisTest.Verify("y2", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y3", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y4", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y5", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y6", 1);

        }
        [Test]
        public void T67_Defect_1467597()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Assignment\\InstanceMethod_test_1.ds");

            Assert.IsTrue((Int64)mirror.GetValue("otherVar").Payload == 26);
            

        }
    }
}
        

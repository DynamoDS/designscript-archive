using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    public class Assignment
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\";

        [SetUp]
        public void Setup()
        {
        }     



        [Test]
        [Category ("SmokeTest")]
 public void T01_SampleTestUsingCodeWithinTestFunction()
        {

            String code =
             @"variable;[Imperative]
             {
	            variable = 5;
             }
             ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Obj o = mirror.GetValue("variable");
            Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_SampleTestUsingCodeFromExternalFile()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T02_SampleTestUsingCodeFromExternalFile.ds");

            Assert.IsTrue((Int64)mirror.GetValue("variable").Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_TestAssignmentToUndefinedVariables_negative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T03_TestAssignmentToUndefinedVariables_negative.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_TestAssignmentStmtInExpression_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T04_TestAssignmentStmtInExpression_negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_TestRepeatedAssignment_negative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T05_TestRepeatedAssignment_negative.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_TestInUnnamedBlock_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T06_TestInUnnamedBlock_negative.ds");
            });
          

        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_TestOutsideBlock()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T07_TestOutsideBlock.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);

        }

        [Test]
        [Category("Update")]
        public void T08_TestCyclicReference()
        {
            string err = "1460274 - Sprint 18 : rev 1590 : Update issue : Cyclic dependency cases are going into infinite loop";
            
            thisTest.VerifyRunScriptFile(testPath,"T08_TestCyclicReference.ds",err);
            thisTest.Verify("b",6);
            thisTest.Verify("a", 2.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T09_TestInNestedBlock()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T09_TestInNestedBlock.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);
            Assert.IsTrue(mirror.GetValue("g3").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);


            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);



        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_TestInFunctionScope()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T10_TestInFunctionScope.ds");

            Assert.IsTrue((double)mirror.GetValue("test").Payload == 4.5);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_TestInClassScope()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T11_TestInClassScope.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b1").Payload == 2);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T12_TestUsingMathAndLogicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T12_TestUsingMathAndLogicalExpr.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue(Convert.ToDouble(mirror.GetValue("b").Payload) == 5);
            Assert.IsTrue(Convert.ToDouble(mirror.GetValue("c").Payload) == 1);
            Assert.IsTrue(Convert.ToDouble(mirror.GetValue("d").Payload) == 7);
            Assert.IsTrue(Convert.ToDouble(mirror.GetValue("e").Payload) == 4);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_TestUsingMathAndLogicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T13_TestUsingMathAndLogicalExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 3.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == -0.66666666666666663);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_TestUsingMathAndLogicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T14_TestUsingMathAndLogicalExpr.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("b").Payload) == 0);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c1").Payload));
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c2").Payload) == false);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c3").Payload) == false);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T15_TestInRecursiveFunctionScope()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T15_TestInRecursiveFunctionScope.ds");

            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 120);



        }

        [Test]
        [Category ("SmokeTest")]
 public void T16_TestInvalidSyntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T16_TestInvalidSyntax.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T17_TestInvalidSyntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T17_TestInvalidSyntax.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T18_TestMethodCallInExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T18_TestMethodCallInExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("test0").Payload == 6.5); //failing here
            Assert.IsTrue((double)mirror.GetValue("test1").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test2").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test3").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test4").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test5").Payload == 6.5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T19_TestAssignmentToCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T19_TestAssignmentToCollection.ds");

            Assert.IsTrue((double)mirror.GetValue("b").Payload == 8.5);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T20_TestInvalidSyntax()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T20_TestInvalidSyntax.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_TestAssignmentToBool()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T21_TestAssignmentToBool.ds");
            // need to capture the warning

            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("a").Payload));
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b").Payload) == false);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_TestAssignmentToNegativeNumbers()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T22_TestAssignmentToNegativeNumbers.ds");
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -111);
            Assert.IsTrue((double)mirror.GetValue("c").Payload == -0.1);
            Assert.IsTrue((double)mirror.GetValue("d").Payload == -1.99);
            Assert.IsTrue((double)mirror.GetValue("e").Payload == 1.99);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T23_TestUsingMathAndLogicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T23_TestUsingMathAndLogicalExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("c1").Payload == -7.5);
            Assert.IsTrue((double)mirror.GetValue("c2").Payload == 0.5);
            Assert.IsTrue((double)mirror.GetValue("c3").Payload == 14.0);
            Assert.IsTrue((double)mirror.GetValue("c4").Payload == 0.875);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_TestUsingMathematicalExpr()
        {
            thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T24_TestUsingMathematicalExpr.ds");
            thisTest.Verify("c1", 5);
            thisTest.Verify("c2", 1);
            thisTest.Verify("c3", 6);
            thisTest.Verify("c4", 1.5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T25_TestUsingMathematicalExpr()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T25_TestUsingMathematicalExpr.ds");

            Assert.IsTrue((double)mirror.GetValue("c1").Payload == 5);
            Assert.IsTrue((double)mirror.GetValue("c2").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("c3").Payload == 6);
            Assert.IsTrue((double)mirror.GetValue("c4").Payload == 1.5);


        }


        [Ignore]
 public void T26_Defect_1450854()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T26_Defect_1450854.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);         

        }

        [Ignore]
 public void T27_Defect_1450847()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T27_Defect_1450847.ds");
            
            Assert.IsTrue((Int64)mirror.GetValue("a1").Payload == -3);
            Assert.IsTrue((Int64)mirror.GetValue("b1").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("c1").Payload == 0);
            Assert.IsTrue(mirror.GetValue("d").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("e1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("f1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            
            
        }

        [Ignore]
 public void T29_Defect_1449887()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T29_Defect_1449887.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 15);


        }

        [Ignore]
 public void T30_Defect_1449887_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T30_Defect_1449887_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 15);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T31_Defect_1449877()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T31_Defect_1449877.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == -6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 13);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("e").Payload) == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T32_Defect_1449877_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T32_Defect_1449877_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T33_Defect_1450003()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T33_Defect_1450003.ds");
            thisTest.Verify("_a_test", 12.5);
            thisTest.Verify("_b", 4.5);
            thisTest.Verify("_c", true);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T34_Defect_1450727()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T34_Defect_1450727.ds");

            Assert.IsTrue((double)mirror.GetValue("z").Payload == -9.7);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T35_Defect_1450727_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T35_Defect_1450727_2.ds");

            Assert.IsTrue((double)mirror.GetValue("z").Payload == -8.1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T36_Defect_1450555()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T36_Defect_1450555.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T37_TestOperationOnNullAndBool()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T37_TestOperationOnNullAndBool.ds");
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T38_Defect_1449928()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T38_Defect_1449928.ds");

            Assert.IsTrue((double)mirror.GetValue("c").Payload == -0.33333333333333331);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T39_Defect_1449704()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T39_Defect_1449704.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T40_Defect_1450552()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T40_Defect_1450552.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T41__Defect_1452423()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T41__Defect_1452423.ds");

            Assert.IsTrue(mirror.GetValue("d").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T42__Defect_1452423_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T42__Defect_1452423_2.ds");

            Assert.IsTrue(mirror.GetValue("x").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T43__Defect_1452423_3()
        {
           
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T43__Defect_1452423_3.ds");
                
                Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T44__Defect_1452423_4()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T44__Defect_1452423_4.ds");
            Assert.IsTrue(mirror.GetValue("x").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T45__Defect_1452423_5()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T45__Defect_1452423_5.ds");
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T46_TestBooleanOperationOnNull()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T46_TestBooleanOperationOnNull.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T47_TestBooleanOperationOnNull()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\Assignment\\T47_TestBooleanOperationOnNull.ds");

            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 0);

           
        }

        [Test]
        [Category ("SmokeTest")]
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
        public void T49_TestForStringObjectType()
        {

            //Assert.Fail("1455594 - Sprint15 : Rev 804: String object type is missing in the new language ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T49_TestForStringObjectType.ds");

            thisTest.Verify("b", "sarmistha", 0);            

        }

        [Ignore]
 public void T50_Defect_1449889()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T50_Defect_1449889.ds");

            thisTest.Verify("d", 5, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T51_Assignment_Using_Negative_Index()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T51_Assignment_Using_Negative_Index.ds");

            thisTest.Verify("c1", 3, 0);
            thisTest.Verify("c2", 2, 0);
            thisTest.Verify("c3", 1, 0);
            thisTest.Verify("c4", 0, 0);
            Assert.IsTrue(mirror.GetValue("c5").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            thisTest.Verify("c6", 2, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T52_Defect_1449889()
        {

            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{

                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Defect_1449889.ds");
                Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                thisTest.Verify("d", 1);

            //});

        }

        [Test]
        [Category ("SmokeTest")]
 public void T53_Defect_1454691()
        {

           ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Defect_1454691.ds");

           thisTest.Verify("x", 3);
           thisTest.Verify("a", 4);
           thisTest.Verify("b", 5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T54_Defect_1454691()
        {
            object[] expectedResult = { 6, 5 };

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T54_Defect_1454691.ds");

            thisTest.Verify("x", 3);
            thisTest.Verify("a1", 4);
            thisTest.Verify("a2", 6);
            thisTest.Verify("b1", 5);
            thisTest.Verify("b2", 2);

            thisTest.Verify("c", expectedResult);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T55_Defect_1454691()
        {
           

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T55_Defect_1454691.ds");

            thisTest.Verify("y1", 1);
            thisTest.Verify("y2", 7);
            thisTest.Verify("y3", 4);
            thisTest.Verify("y4", 2);
            

        }

        [Test]
        [Category ("SmokeTest")]
 public void T56_Defect_1454691()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T56_Defect_1454691.ds");

            thisTest.Verify("b", 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T57_Defect_1454691_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T57_Defect_1454691_2.ds");

            thisTest.Verify("x2", 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T58_Defect_1454691_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T58_Defect_1454691_3.ds");

            thisTest.Verify("b1", 11);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T59_Defect_1455590()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T59_Defect_1455590.ds");

            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 5);
            thisTest.Verify("d", 5);
            thisTest.Verify("e", 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T60_Defect_1455590_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T60_Defect_1455590_2.ds");

            thisTest.Verify("x", 9);
            thisTest.Verify("y", 12);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T61_TestBooleanOperationOnNull()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T61_TestBooleanOperationOnNull.ds");
           
            thisTest.Verify("b1", 0, 0); 
            thisTest.Verify("b2", 1, 0);     
            thisTest.Verify("b3", -1, 0);     

        }

        [Test]
        [Category ("SmokeTest")]
 public void T62_Defect_1456721()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T62_Defect_1456721.ds");

            thisTest.Verify("c", 3, 0);
            thisTest.Verify("c1", 3, 0);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("a1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("a2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("b2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("c2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T63_Defect_1452643()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T63_Defect_1452643.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T64_Defect_1450715()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T64_Defect_1450715.ds");

            Object[] a = new Object[] { 1, 0.5, null, new Object[] {2, 3}, new Object[]{new Object[]{0.4, 5}, true}};

            thisTest.Verify("a", a);
        }

        [Test]
        [Category("Method Resolution")]
        public void T65_Operation_On_Null()
        {
            String code =
@"
a = null + 1; 

";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "";
            ExecutionMirror mirror = thisTest.RunScriptSource(code,err);
            
            Object n1 = null;
            
            thisTest.Verify("a", n1);

        }
        [Test]
        public void T66_Imperative_1467368_negative()
        {
            String code =
            @"
            [Imperative] 
            {
                [Imperative]
                {
                    a = 1; 
                }
            }

            ";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "1467368 - Imperative inside Imperative goes into loop ";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            });
        }
        [Test]
        public void T66_Associative_1467368_negative_2()
        {
            String code =
            @"
            [Associative] 
            {
                [Associative]
                {
                    a = 1; 
                }
            }

            ";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "1467368 - Imperative inside Imperative goes into loop ";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            });
        }

        [Test]
        public void T67_DNL_1467458()
        {
            String code =
            @"
            class A
{
    x:int;
    def foo(a : int)
    {
        x = a;
        return = x;
    }
}

b;
[Imperative]
{
    p = A.A();
    p.foo(9);
    b = p.x;//expected 9 , received:0
}
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 9);
        }
        [Test]
        public void T67_DNL_1467458_2()
        {
            String code =
            @"
            class A
{
    x:int;
    def foo(a : int)
    {
        x = a;
        return = x;
    }
}

b;
[Imperative]
{
    p = {A.A()};
    p.foo(9);
    b = p.x;//expected 9 , received:0
}
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 9);
        }
        [Test]
        public void T67_DNL_1467458_3()
        {
            String code =
            @"
            class A
{
    a;
    def foo()
    {
        a = 1;
        return = a;
    }
}
c;
class B{
    b;
    def foo()
    {
        b = 5;
        return = b;
    }
}
[Imperative]
{
    p = { A.A(),B.B() };
    {p[0].foo(),p[1].foo()};  // compilation error
    c = { p[0].a, p[1].b };
}
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new object[] {1,5});
        }
        [Test]
        public void T67_DNL_1467458_4()
        {
            String code =
            @"
 class B{ a = 1; }
class A
{
    b;
    c;
    def foo()
    {
        b = 1;
        [Imperative]
        {
            p = B.B();
            p.foo();
            c =  p.a;
        }
        return = c;
    }
}

z = A.A();
y =z.foo();
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("y", 1);
        }
        [Test]
        public void T68_DNL_1467523()
        {
            String code =
            @"
class A
{
}
[Imperative]
{
    arr1 : double[] ;
    arr2 : double[] = null;
    arr3 : double[]..[];
    arr4 : double[]..[] = null;
    arr5 : double[]  = { };
    arr6 : double[]..[]  = { };

    arr11 : int[] ;
    arr12 : int[] = null;
    arr13 : int[]..[];
    arr14 : int[]..[] = null;
    arr15 : int[]  = { };
    arr16 : int[]..[]  = { };

    arr111 : bool[] ;
    arr112 : bool[] = null;
    arr113 : bool[]..[];
    arr114 : bool[]..[] = null;
    arr115 : bool[]  = { };
    arr116 : bool[]..[]  = { };

    arr1111 : A[] ;
    arr1112 : A[] = null;
    arr1113 : A[]..[];
    arr1114 : A[]..[] = null;
    arr1115 : A[]  = { };
    arr1116 : A[]..[]  = { };
   
}
dummy = 1;
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("dummy", 1);
        }
    }  
 
  
}

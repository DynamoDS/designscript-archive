using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;

namespace ProtoTest.TD.OtherMiscTests
{
    public class MiscTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testCasePath = "..\\..\\..\\Scripts\\TD\\OtherMiscTest\\";
        [SetUp]
        public void Setup()
        {
        }


        [Test, Ignore]
        [Category("SmokeTest")]
        public void Fibunacci()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Fibonacci.ds");
            thisTest.Verify("fib10_r", 10946);
            thisTest.Verify("fib10_i", 10946);
        }

        [Test]
        [Category("SmokeTest")]
        public void SquareRoot()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "SquareRoot.ds");
            thisTest.Verify("sqrt_10", Math.Sqrt(10.0));
            thisTest.Verify("sqrt_20", Math.Sqrt(20.0));
        }



        [Test]
        [Category("SmokeTest")]
        public void BasicAssign()
        {
            string code =
                @"a = 16;
b = 42;
c = a + b;";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 16);
            thisTest.Verify("b", 42);
            thisTest.Verify("c", 58);

        }


        [Test]
        [Category("SmokeTest")]
        public void Demo_SinWave_WithoutGeometry()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Demo_SinWave_WithoutGeometry.ds");
            // to be verified

        }

        [Test]
        //This served as a sample test case for a test which produces non-homogenous output and verifies with baseline.
        [Category("SmokeTest")]
        public void TestArray()
        {
            String code =
@"a;
[Associative]
{
	a = {1001.1,1002, true};

    x = a[0];
    y = a[1];
    z = a[2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            object[] expectedA = { 1001.1, 1002, true };

            thisTest.Verify("a", expectedA);

        }


        [Test]
        //This served as a sample test case for a test which produces non-homogenous output and verifies with baseline.
        [Category("SmokeTest")]
        public void DefectVerification()
        {
            String code =
@"a =null;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            object test = null;

            thisTest.Verify("a", test);

        }

        [Test]
        [Category("SmokeTest")]
        public void DemoTest_Create1DArray()
        {
            String code =
@"def Create1DArray(numberOfItemInArray : int)
    {
        start = 0.0;
        end = 10.0;
        stepSize = 10/(numberOfItemInArray-1);
        return = 0.0..10..stepSize;
    
    
    }

b = 10.0;
a = 0.0;
numberOfItemInArray = 3;
c = b/(numberOfItemInArray-1);
d = a..b..c;

result = Create1DArray (3);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            object[] test = { 0.0, 5.0, 10.0 };

            thisTest.Verify("result", test);

        }



        [Test]
        [Category("SmokeTest")]
        public void DemoTest_Create2DArray()
        {
            String code =
@"def Create1DArray(numberOfItemInArray : int)
    {
        stepSize = 10.0/(numberOfItemInArray-1);
        return = 0.0..10.0..stepSize;
    }

def Create2DArray(rows : int, columns : int)
    
    {
    
    
   result = [Imperative]
       {
       arrayRows = Create1DArray(rows);
       counter = 0;
       while( counter < rows)
       {
           arrayRows[counter] = Create1DArray(columns);
           counter = counter + 1;
       }
       
       return = arrayRows;
       }
    
    return = result;
        

}

OneD = Create1DArray(5);
TwoD = Create2DArray(2,2);
TwoD0 = TwoD[0];
TwoD1 = TwoD[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            object[] expected = { 0.0, 10.0 };


            thisTest.Verify("TwoD0", expected);
            thisTest.Verify("TwoD1", expected);

        }

        [Test]
        [Category("SmokeTest")]
        public void DemoTest_Count()
        {
            String code =
@"def Count(inputArray : double[])
{
	numberOfItemsInArray = [Imperative]
	{
		index = 0;
		for (item in inputArray)
		{
			index = index + 1;
		}
		
		return = index;
	}
	
	return = numberOfItemsInArray;
}

input1 = {0.0, 10.0, 20.0, 30.0, 50.0};
input2 = {10.0, 20};

result1 = Count(input1);
result2 = Count(input2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);



            thisTest.Verify("result1", 5);
            thisTest.Verify("result2", 2);

        }


        [Test]

        public void TempTest()
        {
            String code =
@"
input1d = {0.0, 10.0, 20.0, 30.0, 50.0};
input2d = {{10, 20},{30,40}};
input2dJagged = {{10.0, 20},{null,40}, {true}, {0}};
inputDouble = 10.0;
inputNull = null;
inputInteger = 1;
inputBool = true;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            object inputDouble = 10.0;
            object inputInteger = 1;
            object inputBool = true;
            object[] input1d = { 0.0, 10.0, 20.0, 30.0, 50.0 };
            object[][] input2d = 
            { 
            new object [] { 10, 20 }, 
            new object [] { 30, 40 } 
            };
            object[][] input2dNegative =
            { 
            new object [] { 10, 21 }, 
            new object [] { 30, 40 } 
            };
            object[][] input2dJagged =
            { 
            new object [] { 10.0, 20}, 
            new object [] { null, 40 },
            new object [] { true },
            new object [] { 0}
            };

            object[][] input2dJaggedNegative =
            { 
            new object [] { 10.0}, 
            new object [] { null, 40 },
            new object [] { true },
            new object [] { 0}
            };


            //Positive Test
            thisTest.Verify("input1d", input1d);
            thisTest.Verify("input2d", input2d);
            //thisTest.Verify("input2d", input2dNegative);

            thisTest.Verify("input2dJagged", input2dJagged);
            thisTest.Verify("input2dJagged", input2dJagged);
            //thisTest.NewVerification(mirror, "inputNull", inputNull);
            //thisTest.NewVerification(mirror, "inputInteger", inputInteger);
            //thisTest.NewVerification(mirror, "inputBool", inputBool);

            ////Negative Test with same type
            //thisTest.NewVerification(mirror, "inputDouble", 11.0);
            //thisTest.NewVerification(mirror, "inputInteger", 2);
            //thisTest.NewVerification(mirror, "inputBool", false);

            ////Negative Test with different type
            //thisTest.NewVerification(mirror, "inputDouble", inputNull);
            //thisTest.NewVerification(mirror, "inputDouble", inputInteger);
            //thisTest.NewVerification(mirror, "inputDouble", inputBool);

            //thisTest.NewVerification(mirror, "inputInteger", inputNull);
            //thisTest.NewVerification(mirror, "inputInteger", inputDouble);
            //thisTest.NewVerification(mirror, "inputInteger", inputBool);

            //thisTest.NewVerification(mirror, "inputNull", inputInteger);
            //thisTest.NewVerification(mirror, "inputNull", inputDouble);
            //thisTest.NewVerification(mirror, "inputNull", inputBool);

            //thisTest.NewVerification(mirror, "inputBool", inputInteger);
            //thisTest.NewVerification(mirror, "inputBool", inputDouble);
            //thisTest.NewVerification(mirror, "inputBool", inputNull);

            //TODO: Multi-level verification. 

        }

        [Test]
        [Category("SmokeTest")]
        public void DynamicReferenceResolving_Complex_Case()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "DynamicReferenceResolving_Complex_Case.ds");

            //Verification
            object[] tm = { 1, 2, 3 };
            object testFoo1 = 5;
            object testInFunction1 = 6;
            object testInFunction2 = 7;
            object[] itm = { 1, 2, 3 };
            object itestFoo1 = 5;
            object itestInFunction1 = 6;
            object itestInFunction2 = 7;
            thisTest.Verify("tm", tm);
            thisTest.Verify("testFoo1", testFoo1);
            thisTest.Verify("testInFunction1", testInFunction1);
            thisTest.Verify("testInFunction2", testInFunction2);
            thisTest.Verify("itm", itm);
            thisTest.Verify("itestFoo1", itestFoo1);
            thisTest.Verify("itestInFunction1", itestInFunction1);
            thisTest.Verify("itestInFunction2", itestInFunction2);
        }

        [Test]
        [Category("SmokeTest")]
        public void DynamicReference_Variable()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "DynamicReference_Variable.ds");

            //Verification
            object kk = 3;
            thisTest.Verify("kk", kk);
        }

        [Test]
        [Category("SmokeTest")]
        public void DynamicReference_FunctionCall()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "DynamicReference_FunctionCall.ds");

            //Verification
            object testFoo1 = 6;
            thisTest.Verify("testFoo1", testFoo1);
        }

        [Test]
        [Category("SmokeTest")]
        public void DynamicReference_FunctionCall_With_Default_Arg()
        {
            string err = "1467384 - Sprint 27 - Rev 4210 default arguments are not working inside class ";
            thisTest.VerifyRunScriptFile(testCasePath, "DynamicReference_FunctionCall_With_Default_Arg.ds", err);

            //Verification
            object testFoo1 = 6;
            object testFoo2 = 4;
            thisTest.Verify("testFoo1", testFoo1);
            thisTest.Verify("testFoo2", testFoo2);
        }
        [Test]
        [Category("SmokeTest")]
        public void FunctionCall_With_Default_Arg()
        {
            string err = "";
            thisTest.VerifyRunScriptFile(testCasePath, "FunctionCall_With_Default_Arg.ds", err);

            //Verification
            object testFoo1 = 6;
            object testFoo2 = 4;
            thisTest.Verify("testFoo1", testFoo1);
            thisTest.Verify("testFoo2", testFoo2);
        }
        [Test]
        [Category("Update")]
        public void TestDynamicSetValueAfterExecution()
        {

            //Assert.Fail("1466857 - Sprint 23 : rev 2486 : Dynamic variable update issue"); 

            String code =
                @"
                a = 2;
                b = a;
                a = 5;
                ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Obj o = mirror.GetValue("a");
            Obj o2 = mirror.GetValue("b");
            thisTest.Verify("a", 5);
            thisTest.Verify("b", 5);

            mirror.SetValueAndExecute("a", 10);
            o = mirror.GetValue("a");
            o2 = mirror.GetValue("b");
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 10);

        }
        [Test]
        [Category("SmokeTest")]
        public void Comments_1467117()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Comments_1467117.ds");
            //Verification
            thisTest.Verify("a", 5);
        }
        [Test]
        [Category("SmokeTest")]
        public void Comments_basic()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Comments_Basic.ds");
            //Verification
            thisTest.Verify("a", 5);
        }
        [Test]
        [Category("SmokeTest")]
        public void Comments_Nested()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Comments_Nested.ds");
            });


        }
        [Test]

        public void Comments_Negative()
        {
            Assert.Fail("1467117 -IDE doesn't print any output if first few lines are commented out in way for ex. /* /* */  ");
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Comments_Negative.ds");
            });

            //Verification

        }
        [Test]
        public void error_LineNumber_2()
        {

            string err = "1467130 - Sprint 24 - Rev 2908 - Missing Line number information while throwing warning ";
            thisTest.VerifyRunScriptFile(testCasePath, "error_LineNumber_2.ds", err);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
            //Verification

        }
        [Test]
        public void Use_Keyword_Array_1463672()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Use_Keyword_Array_1463672.ds");
            object a = new Object[] { new Object[] { 1, 2 }, new Object[] { 1, 2 } };
            thisTest.Verify("x", a);
        }
        [Test]
        public void Comments_1467117_1()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Comments_1467117_1.ds");
            });


        }
        [Test]
        public void GarbageCollection_1467148()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "GarbageCollection_1467148.ds");
            thisTest.Verify("n", 4);


        }
        [Test, Ignore]
        public void imperative_Replication_1467070()
        {

            // need to move this to post R1 project
            Assert.Fail("1467070 Sprint 23 - rev 2636 - 328558 Replication must be disabled in imperative scope ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "imperative_Replication_1467070.ds");
            thisTest.Verify("t", new Object[] { null, null, 3, 4, 5 });

        }
        [Test, Ignore]
        public void imperative_Replication_1467070_2()
        {

            // need to move this to post R1 project
            Assert.Fail("1467070 Sprint 23 - rev 2636 - 328558 Replication must be disabled in imperative scope ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "imperative_Replication_1467070_2.ds");
            thisTest.Verify("t", new Object[] { null, null, 3, 4, 5 });

        }

        [Test]
        public void TestFrameWork_IntDouble_1467413()
        {
            String code =
            @"
                a = 1.0;
                b=1;

            ";

            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 1);
            Assert.IsTrue(mirror.GetValue("a", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Double);
            Assert.IsFalse(mirror.GetValue("a", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Int);
            Assert.IsTrue(mirror.GetValue("b", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Int);
            Assert.IsFalse(mirror.GetValue("b", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Double);
            
            //thisTest.Verify("b", 1.0);
        }
        [Test]
        public void TestFrameWork_IntDouble_Array_1467413()
        {
            String code =
            @"
                a = {1.0,2.0};
                b={1,2};

            ";

            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[]{1.0,2.0});
            thisTest.Verify("b", new object [] {1,2});

            Assert.IsFalse(mirror.GetValue("a", 0).DsasmValue.Equals(new object[] {1,1}));
            Assert.IsFalse(mirror.GetValue("b", 0).DsasmValue.Equals(new object[] { 1.0, 1.0 }));
 

            //thisTest.Verify("b", 1.0);
        }
        [Test]
        public void TestKeyword_reserved_1467551()
        {
            String code =
            @"
                a = 2;
                base=1;

            ";

            string errmsg = "";
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
           //{
               ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
               thisTest.VerifyBuildWarningCount(0);
          // });
         
        }
        [Test]
        public void TestKeyword_reserved_1467551_2()
        {
            String code =
            @"
                [Associative]
                {
                a = 2;
                base=1;
                }

            ";

            string errmsg = "";
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
                thisTest.VerifyBuildWarningCount(0);
            //});

        }
        [Test]
        public void TestKeyword_reserved_1467551_3()
        {
            String code =
            @"
                [Imperative]
                {
                a = 2;
                base=1;
                }

            ";

            string errmsg = "";
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
                thisTest.VerifyBuildWarningCount(0);
            //});

        }
      
        [Test]
        public void TestKeyword_reserved_1467551_4()
        {
            String code =
            @"
                import(""ProtoGeometry.dll"");
                wcs = CoordinateSystem.WCS;
                base = Cylinder.ByRadiusHeight(wcs, 10, 5);
            ";

            string errmsg = "";
           //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
           //{
                ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
                thisTest.VerifyBuildWarningCount(0);
            //});

        }
        [Test]
        public void functionNotFound_1467444()
        {
            String code =
            @"
                a = foo();
            ";

            string errmsg = "";
           
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
      
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);


        }
        [Test]
        public void functionNotFound_1467444_2()
        {
            String code =
            @"
               z=[Imperative]
               {
                        def AnotherFunction(test:int)
                        {
                            result = test * test;
                            return = result;    
                        }
                        x = Function(5);
                        return = x;
               }
                                ";

            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);


        }
    }
}
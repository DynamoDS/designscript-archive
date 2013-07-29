using System;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    public class TestFunction
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\";       
        ProtoScript.Config.RunConfiguration runnerConfig;       
        ProtoScript.Runners.DebugRunner fsr;

        
        [SetUp]
        public void Setup()
        {
            // Specify some of the requirements of IDE.
            ProtoCore.Options options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;

            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new ProtoScript.Runners.DebugRunner(core);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T01_Function_In_Assoc_Scope()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_Function_In_Assoc_Scope.ds");
           
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 20);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_Function_In_Imp_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T02_Function_In_Imp_Scope.ds");

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 5.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_Function_In_Nested_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T03_Function_In_Nested_Scope.ds");

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 2.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 2.5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_Function_In_Nested_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T04_Function_In_Nested_Scope.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_Function_outside_Any_Block()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T05_Function_outside_Any_Block.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_Function_Imp_Inside_Assoc()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\T06_Function_Imp_Inside_Assoc.ds"); 
            

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_Function_Assoc_Inside_Imp()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\T07_Function_Assoc_Inside_Imp.ds");
            
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T08_Function_From_Inside_Class_Constructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T08_Function_From_Inside_Class_Constructor.ds");

            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 2);
           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T09_Function_From_Inside_Class_Constructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T09_Function_From_Inside_Class_Constructor.ds");

            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_Function_From_Inside_Class_Method()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T10_Function_From_Inside_Class_Method.ds");

            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 3);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_Function_From_Inside_Class_Method()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T11_Function_From_Inside_Class_Method.ds");

            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 3);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T12_Function_From_Inside_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T12_Function_From_Inside_Function.ds");

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 3.5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_Function_From_Inside_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T13_Function_From_Inside_Function.ds"); 

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 3.5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_Function_Recursive_imperative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T14_Function_Recursive_imperative.ds"); 

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T18_Function_Recursive_associative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T18_Function_Recursive_associative.ds"); 

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T15_Function_From_Parallel_Blocks()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T15_Function_From_Parallel_Blocks.ds");

                Assert.IsTrue(mirror.GetFirstValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T16_Function_From_Parallel_Blocks()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T16_Function_From_Parallel_Blocks.ds");
                Assert.IsTrue(mirror.GetFirstValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});

        }

        [Test]
        [Category ("SmokeTest")]
 public void T17_Function_From_Parallel_Blocks()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T17_Function_From_Parallel_Blocks.ds");
                Assert.IsTrue(mirror.GetFirstValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});

        }

        [Test]
        [Category ("SmokeTest")]
 public void T19_Function_From_Imperative_While_And_For_Loops()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T19_Function_From_Imperative_While_And_For_Loops.ds");
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 55);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 30);
        }
        
        [Test]
        [Category ("SmokeTest")]
 public void T20_Function_From_Imperative_If_Block()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T20_Function_From_Imperative_If_Block.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 3025);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 900);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 25);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_Function_From_Nested_Imperative_Loops()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T21_Function_From_Nested_Imperative_Loops.ds");
            thisTest.Verify("x", 180, 0);
            thisTest.Verify("y", 50, 0);            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_Function_Call_As_Instance_Arguments()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T22_Function_Call_As_Instance_Arguments.ds");
            thisTest.Verify("A1", 1);
            thisTest.Verify("A2", 1.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T23_Function_Call_As_Function_Call_Arguments()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T23_Function_Call_As_Function_Call_Arguments.ds");
            thisTest.Verify("c1", 28.0, 0);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Function_Call_In_Range_Expression()
        {
            // Assert.Fail("1463472 - Sprint 20 : rev 2112 : Function calls are not working inside range expressions in Associative scope ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T24_Function_Call_In_Range_Expression.ds");
            object[] expectedResult1 = { 1.0, 2.0, 3.0 };
            object[] expectedResult2 = { 1.0, 3.0, 5.0 };
            object[] expectedResult3 = { 1.0, 5.0 };

            thisTest.Verify("a1", expectedResult2);
            thisTest.Verify("a2", expectedResult3);
            thisTest.Verify("a3", expectedResult2);
            thisTest.Verify("a4", new object[] {1.0, 2.0, 3});
        }

        [Test]
        [Category ("SmokeTest")]
 public void T25_Function_Call_In_Mathematical_And_Logical_Expr()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T25_Function_Call_In_Mathematical_And_Logical_Expr.ds");

            thisTest.Verify("a1", 6.0);
            thisTest.Verify("a2", 0.4);
            thisTest.Verify("a3", true);
            thisTest.Verify("a4", 1);
            

        }

        [Test]
        [Category ("SmokeTest")]
 public void T26_Function_Call_In_Mathematical_And_Logical_Expr()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T26_Function_Call_In_Mathematical_And_Logical_Expr.ds");

            thisTest.Verify("x", 6, 0);
            thisTest.Verify("a4", 2, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T27_Function_Call_Before_Declaration()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T27_Function_Call_Before_Declaration.ds");

            
        }



        [Test]
        [Category ("SmokeTest")]
 public void T29_Function_With_Different_Arguments()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T29_Function_With_Different_Arguments.ds");
            
            thisTest.Verify("y", 3.0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T30_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{

                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T30_Function_With_Mismatching_Return_Type.ds");
                Assert.IsTrue(mirror.GetValue("b1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);                
            //});

        }

        [Test]
        [Category ("SmokeTest")]
 public void T31_Function_With_Mismatching_Return_Type() 
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T31_Function_With_Mismatching_Return_Type.ds");

            thisTest.Verify("b2", 5.0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T32_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{

                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T32_Function_With_Mismatching_Return_Type.ds");
                thisTest.Verify("b2", 6, 0);
            //});

        }


        [Test]
        [Category ("SmokeTest")]
 public void T33_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T33_Function_With_Mismatching_Return_Type.ds");
                Assert.IsTrue(mirror.GetValue("b2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});

        }


        [Test]
        [Category ("SmokeTest")]
 public void T34_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T34_Function_With_Mismatching_Return_Type.ds");
                Assert.IsTrue(mirror.GetValue("b1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});

        }


        [Test]
        [Category ("SmokeTest")]
 public void T35_Function_With_Mismatching_Return_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T35_Function_With_Mismatching_Return_Type.ds");

            thisTest.Verify("b2", 5.0, 0);

        }


        [Test]
        [Category ("SmokeTest")]
 public void T36_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T36_Function_With_Mismatching_Return_Type.ds");
                thisTest.Verify("b2", 6, 0);
            //});

        }


        [Test]
        [Category ("SmokeTest")]
 public void T37_Function_With_Mismatching_Return_Type()
        {
            //Assert.Fail("DNL-1467208 Auto-upcasting of int -> int[] is not happening on function return");

            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T37_Function_With_Mismatching_Return_Type.ds");
                Assert.IsTrue(mirror.GetValue("b2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});

        }

        [Test]
 public void T38_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T38_Function_With_Mismatching_Return_Type.ds");
            thisTest.Verify("b2", new Object[] {2});
        }

        [Test]
        [Category ("SmokeTest")]
 public void T39_Function_With_Mismatching_Return_Type()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T39_Function_With_Mismatching_Return_Type.ds");
            Object v1 = null;
            thisTest.Verify("b2", v1);
        }

        [Test]
        [Category("Type System")]
        public void T40_Function_With_Mismatching_Return_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T40_Function_With_Mismatching_Return_Type.ds");
            
        }

        [Test]
        [Category("Type System")]
        public void T41_Function_With_Mismatching_Return_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T41_Function_With_Mismatching_Return_Type.ds");
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T42_Function_With_Mismatching_Return_Type()
        {
             ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T42_Function_With_Mismatching_Return_Type.ds");

             thisTest.Verify("b2", new object[] { new object[] { true }, true });   
        }

        [Test]
        [Category("SmokeTest")]
        public void T43_Function_With_Matching_Return_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T43_Function_With_Matching_Return_Type.ds");
            thisTest.Verify("b2", 0);

        }        

        [Test]
        [Category ("SmokeTest")]
 public void T44_Function_With_Null_Argument()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T44_Function_With_Null_Argument.ds");

            thisTest.Verify("b2", 1.5, 0); 


        }

        [Test]
        [Category ("SmokeTest")]
 public void T45_Function_With_Mismatching_Argument_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T45_Function_With_Mismatching_Argument_Type.ds");
            thisTest.Verify("b2", 1.5, 0); 
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T46_Function_With_Mismatching_Argument_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T46_Function_With_Mismatching_Argument_Type.ds");
            thisTest.Verify("c", 3, 0); 

        }

        [Test]
        [Category ("SmokeTest")]
 public void T47_Function_With_Mismatching_Argument_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T47_Function_With_Mismatching_Argument_Type.ds");
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T48_Function_With_Mismatching_Argument_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T48_Function_With_Mismatching_Argument_Type.ds");
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T49_Function_With_Matching_Return_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T49_Function_With_Matching_Return_Type.ds");
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T50_Function_With_Mismatching_Argument_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T50_Function_With_Mismatching_Argument_Type.ds");
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T51_Function_With_Mismatching_Argument_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T51_Function_With_Mismatching_Argument_Type.ds");
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T52_Function_With_Mismatching_Argument_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Function_With_Mismatching_Argument_Type.ds");
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T53_Function_Updating_Argument_Values()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Function_Updating_Argument_Values.ds");
            thisTest.Verify("aa", 1.5, 0);
            thisTest.Verify("b2", 4.5, 0);
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T54_Function_Updating_Argument_Values()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T54_Function_Updating_Argument_Values.ds");
            thisTest.Verify("aa", 1.5, 0);
            thisTest.Verify("b2", 4.5, 0);
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category("Type System")]
        public void T55_Function_Updating_Argument_Values()
        {
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T55_Function_Updating_Argument_Values.ds");
            thisTest.Verify("aa", 5.0, 0);
            thisTest.Verify("b2", 5, 0);
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T56_Function_Updating_Argument_Values()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T56_Function_Updating_Argument_Values.ds");
            object[] expectedResult1 = { 1, 2 };
            object[] expectedResult2 = { 0, 2 };

            thisTest.Verify("aa", expectedResult1, 0);
            thisTest.Verify("bb", expectedResult2, 0);            
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T57_Function_Using_Local_Var_As_Same_Name_As_Arg()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T57_Function_Using_Local_Var_As_Same_Name_As_Arg.ds");
            thisTest.Verify("aa", 1, 0);
            thisTest.Verify("bb", 4, 0); 
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T58_Function_With_No_Argument()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T58_Function_With_No_Argument.ds");
            
            thisTest.Verify("c1", 3);
            thisTest.Verify("c2", 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T59_Function_With_No_Return_Stmt()
        {
           ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T59_Function_With_No_Return_Stmt.ds");

           Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T60_Function_With_No_Return_Stmt()
        {
          ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T60_Function_With_No_Return_Stmt.ds");

          Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
          

        }

        [Test]
        [Category ("SmokeTest")]
 public void T61_Function_With_Void_Return_Stmt()
        {
            //Assert.Fail("1463474 - Sprint 20 : rev 2112 : negative case: when user tries to create a void function DS throws ArgumentOutOfRangeException ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T61_Function_With_Void_Return_Stmt.ds");
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T62_Function_Modifying_Globals_Values()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T62_Function_Modifying_Globals_Values.ds");

            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("x", 2);
            thisTest.Verify("z", 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T63_Function_Modifying_Globals_Values()
        {
            //Assert.Fail("1465794 - Sprint 22 : rev 2359 : Global variable support is not there in purely Imperative scope"); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T63_Function_Modifying_Globals_Values.ds");
            Object[] v1 = new Object[] { 2, 2 };

            thisTest.Verify("x", v1);
            

        }

        [Test]
        [Category ("SmokeTest")]
 public void T64_Function_Modifying_Globals_Values_Negative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T64_Function_Modifying_Globals_Values_Negative.ds");

            //Assert.Fail("1465794 - Sprint 22 : rev 2359 : Global variable support is not there in purely Imperative scope"); 

            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);
            
          

        }

        [Test]
        [Category ("SmokeTest")]
 public void T65_Function_With_No_Return_Type()
        {
             ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T65_Function_With_No_Return_Type.ds");

             thisTest.Verify("a", true, 0);
             thisTest.Verify("b", 3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T66_Function_Returning_Null()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T66_Function_Returning_Null.ds");

            thisTest.Verify("a", 1, 0);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T67_Function_Returning_Collection()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T67_Function_Returning_Collection.ds");
            
            object[] expectedResult = { 2, 4 };

            thisTest.Verify("b", expectedResult, 0);
            thisTest.Verify("a", 1, 0);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T68_Function_Returning_Null()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T68_Function_Returning_Null.ds");

            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            thisTest.Verify("a", 1, 0);

        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category ("SmokeTest")]
 public void T69_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T69_Function_Name_Checking.ds");
                //thisTest.Verify("a", 1, 0);
                //thisTest.Verify("a", 4, 0);
                //thisTest.Verify("foo", 2, 0);
            });
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category ("SmokeTest")]
 public void T70_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T70_Function_Name_Checking.ds");
                //thisTest.Verify("a", 1, 0);
                //thisTest.Verify("b", 4, 0);
                //thisTest.Verify("foo", 2, 0);
            });
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category ("SmokeTest")]
 public void T71_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T71_Function_Name_Checking.ds");

                //thisTest.Verify("a", 1, 0);
                //thisTest.Verify("foo", 2, 0);
                //thisTest.Verify("c", 3, 0);
                //thisTest.Verify("b", 4, 0);
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T72_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T72_Function_Name_Checking.ds");

            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T73_Function_Name_Checking_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T73_Function_Name_Checking_Negative.ds");
            });           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T74_Function_With_Simple_Replication_Imperative()
        {           
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T74_Function_With_Simple_Replication_Imperative.ds");
            object[] expectedResult1 = { 1, 2, 3 };
            object[] expectedResult2 = { 2, 3, 4 };

            thisTest.Verify("y", expectedResult2);
            thisTest.Verify("x", expectedResult1);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T74_Function_With_Simple_Replication_Associative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T74_Function_With_Simple_Replication_Associative.ds");
            object[] expectedResult1 = { 1, 2, 3 };
            object[] expectedResult2 = { 2, 3, 4 };

            thisTest.Verify("y", expectedResult2);
            thisTest.Verify("x", expectedResult1);  

        }

        [Test]
        [Category ("SmokeTest")]
 public void T75_Function_With_Replication_In_Two_Args()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T75_Function_With_Replication_In_Two_Args.ds");
            object[] expectedResult = { 2, 4, 6 };

            thisTest.Verify("y", expectedResult);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T76_Function_With_Replication_In_One_Arg()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T76_Function_With_Replication_In_One_Arg.ds");
            object[] expectedResult = { 2, 3, 4 };

            thisTest.Verify("y", expectedResult);
        }

        [Test]
        [Category("Replication")]
        public void T77_Function_With_Simple_Replication_Guide()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T77_Function_With_Simple_Replication_Guide.ds");
        
            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", 3, 0);
            thisTest.Verify("a3", 3, 0);
            thisTest.Verify("a4", 4, 0);
           
        }

        [Test]
        [Category("Update")]
        public void T78_Function_call_By_Reference()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T78_Function_call_By_Reference.ds");

            //Assert.Fail("1463498 - Sprint 20 : rev 8112 : Updating variables inside function call is returning the wrong value");

            thisTest.Verify("c", 5);
            thisTest.Verify("d", 3);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T79_Function_call_By_Reference()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T79_Function_call_By_Reference.ds");           
            
            thisTest.Verify("c", 9, 0);
            thisTest.Verify("d", 3, 0);
          
        }

        [Test]
        [Category ("SmokeTest")]
 public void T80_Function_call_By_Reference()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80_Function_call_By_Reference.ds");
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T81_Function_Calling_Imp_From_Assoc()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T81_Function_Calling_Imp_From_Assoc.ds");
            thisTest.Verify("b", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T82_Function_Calling_Assoc_From_Imp()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T82_Function_Calling_Assoc_From_Imp.ds");
            thisTest.Verify("b", 5, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T83_Function_With_Null_Arg()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T83_Function_With_Null_Arg.ds");
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T84_Function_With_User_Defined_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T84_Function_With_User_Defined_Class.ds");
            thisTest.Verify("z", 2, 0);

        }

        [Test]
        [Category("Replication")]
        public void T85_Function_With_No_Type()
        {
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T85_Function_With_No_Type.ds");
           
            thisTest.Verify("a11", 2);
            thisTest.Verify("a21", 3);
            thisTest.Verify("a31", 4);

            thisTest.Verify("a12", 2);
            thisTest.Verify("a22", 3);
            thisTest.Verify("a32", 4);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T86_Function_With_For_Loop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T86_Function_With_For_Loop.ds");

            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", 3, 0);
            thisTest.Verify("a3", 4, 0);
        }

        [Test]
        public void T87_Function_Returning_From_Imp_Block_Inside_Assoc()
        {
            Assert.Fail("1456110 - Sprint16: Rev 889 : Returning from an imperative block inside an associative function is failing");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T87_Function_Returning_From_Imp_Block_Inside_Assoc.ds");

            thisTest.Verify("x", 0, 0);
     
        }
        
        [Test]
        [Category ("SmokeTest")]
 public void T88_Function_With_Collection_Argument()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T88_Function_With_Collection_Argument.ds");

            thisTest.Verify("sum", 0.0, 0);
     
        }

        [Test]
        [Category("Replication")]
        public void T89_Function_With_Replication()
        {
            //Assert.Fail("1456115 - Sprint16: Rev 889 : Replication over a collection is not working as expected");
            //Assert.Fail("1467075 - Sprint23 : rev 2660 : replication with nested array is not working as expected");

            string error = "1467355 Sprint 27 Rev 4014 , it replicates with maximum combination where as its expected to zipped ";
            thisTest.VerifyRunScriptFile(testPath, "T89_Function_With_Replication.ds",error);

            thisTest.Verify("t1", 5.0, 0);
            thisTest.Verify("t2", 3.0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T90_Function_PassingNullToUserDefinedType()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T90_Function_PassingNullToUserDefinedType.ds");

            //Verification
            object testP = 2;
            object testNull = null;
            thisTest.Verify("testP", testP, 0);
            thisTest.Verify("testNull", testNull, 0);
        }

        [Test]
        public void T91_Function_With_Default_Arg()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T91_Function_With_Default_Arg.ds");

            //Verification
            object a = 7.0;
            object b = 4.0;
            object c = 7.0;
            object d = 7.0;
            object e = 4.0;
            object f = 7.0;
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e", e);
            thisTest.Verify("f", f);
        }

        [Test]
        [Category("Type System")]
        public void T92_Function_With_Default_Arg_Overload()
        {
            //Assert.Fail("DNL-1467202 - Argument type casting is not happening in some cases with function calls");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T92_Function_With_Default_Arg_Overload.ds");
            //Verification
            object a = 0.0;
            object b = 5.0;
            object c = 5.0;
            object d = 7.0;
            object e = null;
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e", e);
        }

        [Test]
        public void T93_Function_With_Default_Arg_In_Class()
        {
            string str = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "T93_Function_With_Default_Arg_In_Class.ds", str);

            //Verification
            object i = 16.0;
            object j = 13.0;
            object k = 16.0;
            object a = 16.0;
            object b = 13.0;
            object c = 16.0;
            thisTest.Verify("i", i);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        
        [Test]
        [Category ("SmokeTest")]
 public void TV00_Function_With_If_Statements()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV00_Function_With_If_Statements.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == -9);

        }


        [Test]
        [Category ("SmokeTest")]
 public void TV01_Function_With_While_Statements()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV01_Function_With_While_Statements.ds");

            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 8);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV02_Function_With_For_Statements()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV02_Function_With_For_Statements.ds");

            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV03_Function_With_Recursion()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV03_Function_With_Recursion.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 120);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV04_Function_With_RangeExpression()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV04_Function_With_RangeExpression.ds");

            Assert.IsTrue((double)mirror.GetValue("d").Payload == 9.5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV05_Function_With_RangeExpression_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV05_Function_With_RangeExpression_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 9);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV06_Function_With_Logical_Operators()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV06_Function_With_Logical_Operators.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV07_Function_With_Math_Operators()
        {
            thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV07_Function_With_Math_Operators.ds");
            thisTest.Verify("a", 36.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV08_Function_With_Outer_Function_Calls()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV08_Function_With_Outer_Function_Calls.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 11);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV09_Function_With_Argument_Update_Imperative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV09_Function_With_Argument_Update_Imperative.ds");

            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 5);

        }


        [Test]
        [Category ("SmokeTest")]
 public void TV10_Function_With_Class_Instances()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV10_Function_With_Class_Instances.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV11_Function_Update_Local_Variables()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV11_Function_Update_Local_Variables.ds");

            Assert.IsTrue(mirror.GetValue("r").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV12_Function_With_Argument_Update_Associative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV12_Function_With_Argument_Update_Associative.ds");

            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV13_Empty_Functions_Imperative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV13_Empty_Functions_Imperative.ds");

            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV14_Empty_Functions_Associative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV14_Empty_Functions_Associative.ds");

            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV15_Function_No_Brackets()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV15_Function_No_Brackets.ds");

                Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV16_Function_With_No_Return_Statement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV16_Function_With_No_Return_Statement.ds");
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV17_Function_Access_Local_Variables_Outside()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV17_Function_Access_Local_Variables_Outside.ds");

            Assert.IsTrue(mirror.GetValue("e").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("f").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV18_Function_Access_Global_Variables_Inside()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV18_Function_Access_Global_Variables_Inside.ds");

            thisTest.Verify("d", 18); 
            
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV19_Function_Modify_Global_Variables_Inside()
        {

            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope "); 

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV19_Function_Modify_Global_Variables_Inside.ds");

            thisTest.Verify("e", 6); 
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV20_Function_With_Illegal_Syntax_1()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV20_Function_With_Illegal_Syntax_1.ds");

            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV21_Function_With_Illegal_Syntax_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV21_Function_With_Illegal_Syntax_2.ds");

            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV22_Function_With_Class_Object_As_Argument()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Function\\TV22_Function_With_Class_Object_As_Argument.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV23_Defect_1455152()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV23_Defect_1455152.ds");

            Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("d").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV24_Defect_1454958()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                //{
                    ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV24_Defect_1454958.ds");
                    Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                    Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                //});
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV25_Defect_1454923()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV25_Defect_1454923.ds");
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV26_Defect_1454923_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV26_Defect_1454923_2.ds");

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV27_Defect_1454688()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                //{
                    ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV27_Defect_1454688.ds");
                    Assert.IsTrue(mirror.GetFirstValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                //});
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV28_Defect_1454688_2()
       { 
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV28_Defect_1454688_2.ds");
                Assert.IsTrue(mirror.GetFirstValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV29_Overloading_Different_Number_Of_Parameters()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV29_Overloading_Different_Number_Of_Parameters.ds");

            thisTest.Verify("c", 2);
            thisTest.Verify("d", 5);
            thisTest.Verify("a", 6);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV30_Overloading_Different_Parameter_Types()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV30_Overloading_Different_Parameter_Types.ds");

            thisTest.Verify("b", 4);
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV31_Overloading_Different_Return_Types()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV31_Overloading_Different_Return_Types.ds");
        }

        [Test]
        public void TV32_Function_With_Default_Argument_Value()
        {
            //Assert.Fail("1455742 - Sprint15 : Rev 836 : Function with default values not allowed"); 
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV32_Function_With_Default_Argument_Value.ds");
            Object v1 = null;

            thisTest.Verify("c1", 10);
            thisTest.Verify("c2", 6);
            thisTest.Verify("c3", 3);
            thisTest.Verify("c4", v1);
     
        }

        [Test]
        public void TV32_Function_With_Default_Argument_Value_2()
        {
            //Assert.Fail("1455742 - Sprint15 : Rev 836 : Function with default values not allowed"); 
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV32_Function_With_Default_Argument_Value_2.ds");

            thisTest.Verify("c1", 5);
            thisTest.Verify("c2", 1);
            thisTest.Verify("c3", 1.5);
            thisTest.Verify("c4", 2.0);
     
        }

        [Test]
        public void TV32_Function_With_Default_Argument_Value_3()
        {
            //Assert.Fail("1455742 - Sprint15 : Rev 836 : Function with default values not allowed"); 
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV32_Function_With_Default_Argument_Value_3.ds");
            Object[] v1 = new Object [] { null, 1, 2, 5.0 };

            thisTest.Verify("d5", v1);
            thisTest.Verify("d2", 1);
            thisTest.Verify("d3", 2);
            thisTest.Verify("d4", 5);

        }

        [Test]
        [Category("Method Resolution")]
        public void TV33_Function_Overloading()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV33_Function_Overloading.ds");
            // Assert.Fail("1467176 - sprint24: Regression : rev 3152 : inline condition causing wrong output in function with default arguments");
            thisTest.Verify("c1", 5);
            thisTest.Verify("c2", 1);
            thisTest.Verify("c3", 1.5);
            thisTest.Verify("c4", 2.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV33_Function_Overloading_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV33_Function_Overloading_2.ds");

            thisTest.Verify("c4", 2.0);
            thisTest.Verify("c5", 2.5);
            thisTest.Verify("c6", 2);
        }

 
        [Test]
        [Category ("SmokeTest")]
 public void TV33_Overloading_Different_Order_Of_Parameters()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV33_Overloading_Different_Order_Of_Parameters.ds");

            thisTest.Verify("f", 2, 0);
            thisTest.Verify("c", 2, 0);
            thisTest.Verify("d", 3, 0);
        }

    

        [Test]
        [Category ("SmokeTest")]
 public void TV34_Implicit_Conversion_Int_To_Bool()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV34_Implicit_Conversion_Int_To_Bool.ds");

            thisTest.Verify("b", 1);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV35_Implicit_Conversion_Int_To_Double()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV35_Implicit_Conversion_Int_To_Double.ds");
            
            thisTest.Verify("b", 4.5);
            thisTest.Verify("c", 5.5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV36_Implicit_Conversion_Return_Type()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV36_Implicit_Conversion_Return_Type.ds");
            thisTest.Verify("c", true);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV37_Overloading_With_Type_Conversion()
        {       
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV37_Overloading_With_Type_Conversion.ds");

            thisTest.Verify("a", 2, 0);
            thisTest.Verify("b", 2, 0);
            thisTest.Verify("c", 3, 0);
            thisTest.Verify("d", 1, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV38_Defect_1449956()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV38_Defect_1449956.ds");

            thisTest.Verify("x", 120, 0);
            thisTest.Verify("y", 5040, 0);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV39_Defect_1449956_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV39_Defect_1449956_2.ds");
            Assert.Fail("1467237 - Sprint25: rev 3418 : Regression : Recursion not being supported in Imperative code");

            thisTest.Verify("x", 55, 0);
            thisTest.Verify("y", 0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV40_Defect_1449956_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV40_Defect_1449956_3.ds");

            thisTest.Verify("x", 10, 0);
            thisTest.Verify("y", 0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV41_Defect_1454959()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV41_Defect_1454959.ds");

            thisTest.Verify("result", 5);
            thisTest.Verify("a", 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV42_Defect_1454959()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV42_Defect_1454959.ds");
            thisTest.Verify("a", 6);
            thisTest.Verify("b", 12);
            thisTest.Verify("c", 12);     

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV43_Defect_1455143()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV43_Defect_1455143.ds");

            thisTest.Verify("a1", 1);
            thisTest.Verify("b1", 0);
            thisTest.Verify("c1", 0);

            thisTest.Verify("a2", 1);
            thisTest.Verify("b2", 0);
            thisTest.Verify("c2", 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV44_Defect_1455245()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV44_Defect_1455245.ds");

            thisTest.Verify("a", 1, 0);
            thisTest.Verify("b", 2, 0); 
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV45_Defect_1455278()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV45_Defect_1455278.ds");

            thisTest.Verify("b1", 3);
            thisTest.Verify("b2", 3);
        }

        [Test]
        public void TV46_Defect_1455278()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV46_Defect_1455278.ds");

            thisTest.Verify("b1", 2, 0);
            thisTest.Verify("b2", 1, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV47_Defect_1456087()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV47_Defect_1456087.ds");

            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV48_Defect_1456110()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV48_Defect_1456110.ds");
            thisTest.Verify("x", 0, 0);
           
        }

        [Test, Ignore]
        [Category ("SmokeTest")]
 public void TV49_Defect_1456110()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV49_Defect_1456110.ds");
            Assert.Fail("1467237 - Sprint25: rev 3418 : Regression : Recursion not being supported in Imperative code");
            thisTest.Verify("x", 55);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV50_Defect_1456108()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV50_Defect_1456108.ds");

            object[] expectedResult = { 2, 3, 4 };
            thisTest.Verify("c", expectedResult);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV51_Defect_1456108_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV51_Defect_1456108_2.ds");

            object[] expectedResult = { 2.0, 3.0, 4.0 };
            thisTest.Verify("c", expectedResult, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_3()
        {           
            // object[] expectedResult1 = { 2.0, 3, 4 };
            object expectedResult2 = null;
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV51_Defect_1456108_3.ds");

            // b1 is updated as well. 
            thisTest.Verify("a1", expectedResult2, 0);
            thisTest.Verify("b2", expectedResult2, 0);           
        }

        [Test]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_4()
        {
            object[] expectedResult1 = { 1.0, 2.0, 3.0 };
            object[] expectedResult2 = { 2.0, 3.0, 4.0 };

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV51_Defect_1456108_4.ds");
 
            thisTest.Verify("t", expectedResult1, 0);
            thisTest.Verify("t2", expectedResult2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_5()
        {
            object[] expectedResult1 = { 0.0, 0.0, 0.0 };          

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV51_Defect_1456108_5.ds");
            //Assert.Fail("1467238 - Sprint25: rev 3420 : REGRESSION : Update issue when class collection property is updated");

            thisTest.Verify("t", expectedResult1, 0);
            thisTest.Verify("b2", expectedResult1, 0);
            thisTest.Verify("b1", expectedResult1, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV52_Defect_1456397()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV52_Defect_1456397.ds");

            thisTest.Verify("b1", 12);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV53_Defect_1456397_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV53_Defect_1456397_2.ds");

            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV54_Defect_1456397_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV54_Defect_1456397_3.ds");

            thisTest.Verify("b1", 2, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV55_Defect_1456571()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV55_Defect_1456571.ds");

            thisTest.Verify("x", 5, 0);
        }


        [Test]
        [Category ("SmokeTest")]
 public void TV56_Defect_1456571_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV56_Defect_1456571_2.ds");

            thisTest.Verify("x", 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV56_Defect_1456571_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV56_Defect_1456571_3.ds");

            thisTest.Verify("f1", 5);
            thisTest.Verify("f2", 1);

            thisTest.Verify("f3", 5);
            thisTest.Verify("f4", 1);
            
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category ("SmokeTest")]
 public void TV57_Defect_1454932()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV57_Defect_1454932.ds");

                //thisTest.Verify("b", 4, 0);
                //thisTest.Verify("c", 5, 0);
            });
        }


        [Test]
        [Category ("SmokeTest")]
 public void TV58_Defect_1455278()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV58_Defect_1455278.ds");

            thisTest.Verify("b1", 2, 0);
            thisTest.Verify("b2", 1, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV59_Defect_1455278_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV59_Defect_1455278_2.ds");

            object[] ExpectedResult = { 6.25, 100.0 };
            thisTest.Verify("x_squared", ExpectedResult, 0);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV60_Defect_1455278_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV60_Defect_1455278_3.ds");

            thisTest.Verify("x", 9.0, 0);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV61_Defect_1456100()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV61_Defect_1456100.ds");

            object[] ExpectedResult = { 2, 3, 4 };

            thisTest.Verify("d",ExpectedResult , 0);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV62_Defect_1455090()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV62_Defect_1455090.ds");

            object[] expectedResult2 = { 1, 2, 3 };


            thisTest.Verify("c", expectedResult2, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV63_Defect_1455090_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV63_Defect_1455090_2.ds");

            object[] expectedResult2 = { 2.5,3.5 };
            object[] expectedResult = { 2.5 };



            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", expectedResult);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV64_Defect_1455090_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV64_Defect_1455090_3.ds");

            object[] expectedResult2 = { 2.5, 3.5 };
            object[] expectedResult = { 2.5 };



            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", expectedResult);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV65_Defect_1455090_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV65_Defect_1455090_4.ds");

            thisTest.Verify("a", 1, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV66_Defect_1455090_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV66_Defect_1455090_5.ds");

            thisTest.Verify("c", 5, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV67_Defect_1455090_6()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV67_Defect_1455090_6.ds");

            thisTest.Verify("c", 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV68_Defect_1455090_7()
        {

            //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Imperative code is not allowed in class constructor ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV68_Defect_1455090_7.ds");

            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV69_Defect_1456799()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV69_Defect_1456799.ds");

            thisTest.Verify("bcurvePtX", 5.0, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV69_Defect_1456799_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV69_Defect_1456799_2.ds");

            thisTest.Verify("bcurvePtX", 5.0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV69_Defect_1456799_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV69_Defect_1456799_3.ds");

            thisTest.Verify("bcurvePtX", 5.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV70_Defect_1456798()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV70_Defect_1456798.ds");
            thisTest.Verify("X", 10.0, 0);
            

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV71_Defect_1456108()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV71_Defect_1456108.ds");

            object[] expectedResult = { 2, 3, 4 };

            thisTest.Verify("c", expectedResult, 0);
        }

        

        [Test]
        [Category ("SmokeTest")]
 public void TV71_Defect_1456108_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV71_Defect_1456108_2.ds");
            object[] expectedResult1 = { 1, 2, 3 };
            object[] expectedResult2 = { 2, 3, 4 };

            thisTest.Verify("d", expectedResult1);
            thisTest.Verify("b", expectedResult2);
            thisTest.Verify("c", expectedResult2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV72_Defect_1454541()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV72_Defect_1454541.ds");

            thisTest.Verify("d1", 20);
            thisTest.Verify("d2", 20);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV72_Defect_1454541_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV72_Defect_1454541_1.ds");

            thisTest.Verify("d1", 20);
            thisTest.Verify("d2", 20);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV72_Defect_1454541_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV72_Defect_1454541_2.ds");

            thisTest.Verify("d", 20);
            thisTest.Verify("d2", 20);

        }
        
        [Test]
        [Category ("SmokeTest")]
 public void TV73_Defect_1451831()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV73_Defect_1451831.ds");

            thisTest.Verify("y", 3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV74_Defect_1456426()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV74_Defect_1456426.ds");

            thisTest.Verify("b1", 4);
            thisTest.Verify("b2", 3);
            thisTest.Verify("b3", 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV75_Defect_1456870()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV75_Defect_1456870.ds");

            thisTest.Verify("a1", 5, 0);
            thisTest.Verify("a2", 8, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV76_Defect_1456112()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV76_Defect_1456112.ds");

            Object[] sum1 = new Object[] {0.0, 0.0, 0.0, 0.0};

            thisTest.Verify("sum", 0.0, 0);
            thisTest.Verify("sum1", sum1, 0);
            thisTest.Verify("sum2", 0.0, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV76_Defect_1456112_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV76_Defect_1456112_2.ds");
            Object n1 = null;

            thisTest.Verify("sum1", n1);
            thisTest.Verify("sum2", 1.0);
            
        }

        [Test]
        [Category("Method Resolution")]
        public void TV77_Defect_1455259()
        {
            
            //Assert.Fail("1463703 - Sprint 20 : rev 2147 : regression : ambiguous warning and wrong output for cases where the class method name and global method name is same ");
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV77_Defect_1455259.ds");

            thisTest.Verify("x1", 1);
            thisTest.Verify("y11", 2);
            thisTest.Verify("y21", 2);

            thisTest.Verify("x2", 1);
            thisTest.Verify("y12", 2);
            thisTest.Verify("y22", 2);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV77_Defect_1455259_2()
        {
            // Assert.Fail("1463703 - Sprint 20 : rev 2147 : regression : ambiguous warning and wrong output for cases where the class method name and global method name is same ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV77_Defect_1455259_2.ds");

            thisTest.Verify("b", 1, 0);
            thisTest.Verify("d", 2, 0);
            thisTest.Verify("c", 2, 0);


        }

        [Test]
        [Category("Method Resolution")]
        public void TV78_Defect_1460866()
        {
            //Assert.Fail("1466878 - Sprint 23 : rev 2497 : regression : NUnit hanging due to function overloading issue ");
            Assert.Fail("1467026 - Sprint23 : rev 2530 : CompilerInternalException : Function with same name and signature is declared in parallel language blocks");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV78_Defect_1460866.ds");
            Object v1 = null;

            thisTest.Verify("x", v1);
            thisTest.Verify("y", v1);
            thisTest.Verify("z1", v1);
            thisTest.Verify("z2", 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV78_Defect_1460866_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV78_Defect_1460866_2.ds");
            Object v1 = null;

            thisTest.Verify("y", 2, 0);
            thisTest.Verify("z", true, 0);
            thisTest.Verify("y2", v1, 0);
            thisTest.Verify("z2", v1, 0);     

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV79_Defect_1462300()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV79_Defect_1462300.ds");
            Object v1 = null;
            Object[] v2 = new Object[] { null, null };

            thisTest.Verify("aa", v1);
            thisTest.Verify("bb", v1);
            thisTest.Verify("b", v2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV79_Defect_1462300_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV79_Defect_1462300_2.ds");
            Object[] v2 = new Object[] { null, null };

            thisTest.Verify("b", v2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV79_Defect_1462300_3()
        {
            //Assert.Fail("1462300 - sprint 19 - rev 1948-316037 - if return an array of functions as properties then it does not return all of them");
                        
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV79_Defect_1462300_3.ds");
            Object[] v2 = new Object[] { null, null };

            thisTest.Verify("d", v2);
            thisTest.Verify("e1", v2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV80_Function_Name_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV80_Function_Name_Negative.ds");
            });          
            
        }
        
        [Test]
        [Category ("SmokeTest")]
 public void TV81_Defect_1458187()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV81_Defect_1458187.ds");

            thisTest.Verify("a", 3, 0);
            thisTest.Verify("b", false, 0);
            Assert.IsTrue(mirror.GetValue("c", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV81_Defect_1458187_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV81_Defect_1458187_2.ds");

            thisTest.Verify("a", 2, 0);
            Assert.IsTrue(mirror.GetValue("b", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("c", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV82_Defect_1460892()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV82_Defect_1460892.ds");
            thisTest.Verify("a", 11);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV83_Function_Pointer()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV83_Function_Pointer.ds");
            Object n1 = null;
            thisTest.Verify("a", false);
            thisTest.Verify("a2", n1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV83_Function_Pointer_Collection()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV83_Function_Pointer_Collection.ds");
            thisTest.Verify("a", 3);

        }

        [Test]
        [Category("Type System")]
        public void TV84_Function_Pointer_Implicit_Conversion()
        {
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV84_Function_Pointer_Implicit_Conversion.ds");
            
            thisTest.Verify("a", 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV84_Function_Pointer_Implicit_Conversion_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV84_Function_Pointer_Implicit_Conversion_2.ds");
            
            thisTest.Verify("a", 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV84_Function_Pointer_Implicit_Conversion_3()
        {           
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV84_Function_Pointer_Implicit_Conversion_3.ds");

            thisTest.Verify("a", 3);
            // thisTest.Verify("b", 2);
            thisTest.Verify("d", 5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV84_Function_Pointer_Implicit_Conversion_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV84_Function_Pointer_Implicit_Conversion_4.ds");

            thisTest.Verify("a", 3);
            thisTest.Verify("d", 5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV84_Function_Pointer_Using_2_Functions()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV84_Function_Pointer_Using_2_Functions.ds");

            thisTest.Verify("a", 6.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV84_Function_Pointer_Negative()
        {
            string error = "1467379 Sprint 27 - Rev 4193 - after throwing warning / error in the attached code it should execute rest of the code ";
            thisTest.VerifyRunScriptFile(testPath, "TV84_Function_Pointer_Negative.ds",error);
            Object v = null;
            thisTest.Verify("a", v);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV85_Function_Return_Type_Var_User_Defined_Type_Conversion()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV85_Function_Return_Type_Var_User_Defined_Type_Conversion.ds");
        
            thisTest.Verify("b", 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV86_Defect_1456728()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV86_Defect_1456728.ds");
            Object[] v2 = new Object[] { null , null };

            thisTest.Verify("a", v2);
            thisTest.Verify("b", v2);
        }
       

        [Test]
        [Category ("SmokeTest")]
 public void TV87_Defect_1464027()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV87_Defect_1464027.ds");
            Object v1 = null;

            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", v1);

            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", v1);

            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV87_Defect_1464027_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV87_Defect_1464027_2.ds");

            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);

            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV87_Defect_1464027_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV87_Defect_1464027_3.ds");

            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);

            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV87_Defect_1464027_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV87_Defect_1464027_4.ds");

            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);

            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV87_Defect_1464027_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV87_Defect_1464027_5.ds");

            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);

            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV88_Defect_1463489()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV88_Defect_1463489.ds");
            thisTest.Verify("c", true);
            thisTest.Verify("d", true);
        }


        [Test]
        [Category ("SmokeTest")]
 public void TV88_Defect_1463489_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV88_Defect_1463489_2.ds");
            thisTest.Verify("c", true);
            thisTest.Verify("d", true);
            thisTest.Verify("c1", false);
            thisTest.Verify("d1", false);

        }

        [Test]
     
        [Category("Design Issue")]
        [Category("Update")]
 public void TV88_Defect_1463489_3()
        {
            Assert.Fail("1459777 - Sprint 17 : Rev 1526 : Design Issue : When class property is updated the the variables derived from the class instance should be updated ? ");

            string src = string.Format("{0}{1}", testPath, "TV88_Defect_1463489_3.ds");

            fsr.LoadAndPreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 8,
                LineNo = 35,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };

            fsr.ToggleBreakpoint(cp);
            ProtoScript.Runners.DebugRunner.VMState vms = fsr.Run();

            thisTest.DebugModeVerification(vms.mirror, "y1", true);
            thisTest.DebugModeVerification(vms.mirror, "y3", true);
            thisTest.DebugModeVerification(vms.mirror, "y2", false);
            thisTest.DebugModeVerification(vms.mirror, "y4", false);            

            fsr.Run();

            thisTest.DebugModeVerification(vms.mirror, "y1", false);
            thisTest.DebugModeVerification(vms.mirror, "y3", false);
            thisTest.DebugModeVerification(vms.mirror, "y2", true);
            thisTest.DebugModeVerification(vms.mirror, "y4", true); 
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV89_Implicit_Type_Conversion_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_Implicit_Type_Conversion_1.ds");

            thisTest.Verify("y1", false);
            thisTest.Verify("y3", false);
            thisTest.Verify("y2", true);
            thisTest.Verify("y4", true); 

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV90_Defect_1463474()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV90_Defect_1463474.ds");

            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);        

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV90_Defect_1463474_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV90_Defect_1463474_2.ds");
            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV90_Defect_1463474_3()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope "); 
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV90_Defect_1463474_3.ds");
            Object v1 = null;
            thisTest.Verify("c1", v1);
            thisTest.Verify("b1", 2);        

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV90_Defect_1463474_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV90_Defect_1463474_4.ds");
            Object v1 = null;
            thisTest.Verify("b1", v1);
            thisTest.Verify("d1", 2);  

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV91_Defect_1463703()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV91_Defect_1463703.ds");
           
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 2);
            thisTest.Verify("b", 1);
        }

        [Test]
      
 public void TV91_Defect_1463703_2()
        {
            //This failure is no longer related to this defect. Back to TD.
            //Assert.Fail("1466269 - Sprint 22 - rev 2418 - Regression : DS throws warning Multiple type+pattern match parameters found, non-deterministic dispatch ");
            string error = "1467080 sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks";
            thisTest.VerifyRunScriptFile(testPath, "TV91_Defect_1463703_2.ds",error);

            thisTest.Verify("y1", 2);
            thisTest.Verify("y2", 2);
            thisTest.Verify("y3", 2);
            thisTest.Verify("y4", 2);
        }

        [Test]

        [Category("Method Resolution")]
        public void TV91_Defect_1463703_3()
        {
            string error = "1467080 sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks";
            //Assert.Fail("1466269 - Sprint 22 - rev 2418 - Regression : DS throws warning Multiple type+pattern match parameters found, non-deterministic dispatch ");
            
           thisTest.VerifyRunScriptFile(testPath, "TV91_Defect_1463703_3.ds",error);
            
            Object[] v1 = new Object[] { new Object[] { 1, 9}, new Object[] { 1, 9} };            
            Object[] v2 = new Object[] { new Object[] {4,9}, new Object[] {4,9}, new Object[] {1,2}, new Object[]{3,4} };
          

            thisTest.Verify("x1", v1);            
            thisTest.Verify("x2", v2);            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV92_Accessing_Variables_Declared_Inside_Function_Body()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV92_Accessing_Variables_Declared_Inside_Function_Body.ds");
            
            thisTest.Verify("a", 1);            
            thisTest.Verify("c", 10);            
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV93_Modifying_Global_Var_In_Func_Call.ds");
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");

            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV93_Modifying_Global_Var_In_Func_Call_2.ds");
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");
            
            // This test case doesn't make sense. Should be updated or deleted. -- Yu Ke
            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV93_Modifying_Global_Var_In_Func_Call_3.ds");
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");

            thisTest.Verify("xx", 0);
            thisTest.Verify("yy", 0);
            thisTest.Verify("zz", 0);
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV93_Modifying_Global_Var_In_Func_Call_4.ds");
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");

            thisTest.Verify("xx", 3);
            thisTest.Verify("yy", 3);
            thisTest.Verify("zz", 3);
        }



        [Test]
        [Category ("SmokeTest")]
 public void TV95_Method_Resolution_Derived_Class_Arguments()
        {
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV95_Method_Resolution_Derived_Class_Arguments.ds");
            //Assert.Fail("DNL-1467027 - Sprint23 : rev 2529 : Method resolution issue over derived class arguments");
            
            thisTest.Verify("t", 123);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV96_Defect_DNL_1465794()
        {
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV96_Defect_DNL_1465794.ds");
            Object[] v1 = new Object[] { 2, 2 };
           
            thisTest.Verify("x", v1);
            
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void TV96_Defect_DNL_1465794_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV96_Defect_DNL_1465794_2.ds");
            Object[] v1 = new Object[] { 3, 1 };

            thisTest.Verify("x", v1);


        }

        [Test]
        [Category ("SmokeTest")]
 public void TV96_Defect_DNL_1465794_3()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV96_Defect_DNL_1465794_3.ds");
            Object[] v1 = new Object[] { new Object[]{ 2, 2 } , new Object[]{ 3, 4 } };
            Object[] v2 = new Object[] { v1, 1 };
            thisTest.Verify("x", v2);


        }

        [Test]
        [Category ("SmokeTest")]
 public void TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication.ds");
                        
            Object[] v1 = new Object[] { 2.5, 4.0, 6.0 };
            Object[] v2 = new Object[] { 2.0, 4.0, 3.5 };
            Object[] v3 = new Object[] { 2.0, 4.0, 3.0 };
            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);
            thisTest.Verify("b3", v3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication_2.ds");
            
            thisTest.Verify("b1", 1);


        }

        [Test]
        [Category ("SmokeTest")]
 public void TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication.ds");
                 
            Object[] v1 = new Object[] { 2.5, 4.0 };
            Object[] v2 = new Object[] { 3.0, 4.0, 2.5 };
            Object[] v3 = new Object[] { 3.0, 4.0, 2.0 };
            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);
            thisTest.Verify("b3", v3);


        }

        [Test]
        [Category ("SmokeTest")]
 public void TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication_2.ds");
            Object[] v1 = new Object[] { 1, 1, 1, 1, 1 };
            thisTest.Verify("b1", v1);

        }

        [Test]
        [Category("Method Resolution")]
        public void TV98_Method_Overload_Over_Rank_Of_Array()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV98_Method_Overload_Over_Rank_Of_Array.ds");
            
            thisTest.Verify("x", 0);

        }

        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060()
        {
            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060.ds");
            Object[] b2 = new Object[] { 2.0,4.0,3.5};
            thisTest.Verify("b2", b2);
        }
        [Test]

        public void TV89_typeConversion_FunctionArguments_1467060_2()
        {
            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_2.ds");
            Object[] b2 = new Object[] { 2.0,4.0,3.0};
            thisTest.Verify("b2", b2);
        }
        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_3()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly

            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_3.ds");
            Object[] b1 = new Object[] { 2, 4, 4 };
            thisTest.Verify("b1", b1);
        }
        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_4()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly

            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_4.ds");
            Object[] b1 = new Object[] { 2, 4, null };
            thisTest.Verify("b1", b1);
        }

        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_5()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly

            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_5.ds");
            Object[] b1 = new Object[] { new object[] { 2 }, new Object[] { 4 }, new object [] {1,2}};
            thisTest.Verify("b1", b1);
        }
        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060_6()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly

            //Assert.Fail("DNL-1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string error ="1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            thisTest.VerifyRunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_6.ds", error);
            Object[] b1 = new Object[] { null, 5, 6};
            thisTest.Verify("b1", b1);
        }
        
        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060_9()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly

            //Assert.Fail("DNL-1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_9.ds");
            Object[] b1 = new Object[] { 1, null, 6 };
            thisTest.Verify("b1", b1);
        }

        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060_7()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_7.ds");
            Object[] b1 = new Object[] { null, null, null};
            thisTest.Verify("b1", b1);
        }

        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_8()
        {
            //Assert.Fail("DNL-1467202 Argument type casting is not happening in some cases with function calls");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV89_typeConversion_FunctionArguments_1467060_8.ds");
            Object[] b1 = new Object[] { 1, 2, 3 };
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected");

            thisTest.Verify("b1", b1); 
            
            //string dsFullPathName = testPath + "TV89_typeConversion_FunctionArguments_1467060_8.ds";
            //ExecutionMirror mirror = thisTest.RunScript(dsFullPathName);
            //Assert.IsTrue(core.BuildStatus.ContainsWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound));
        }
        
        [Test]
        public void TV99_Defect_1463456_Array_By_Reference_Issue()
        {
            string errmsg = " 1467318 -  Cannot return an array from a function whose return type is var with undefined rank (-2)  ";
            thisTest.VerifyRunScriptFile(testPath, "TV99_Defect_1463456_Array_By_Reference_Issue.ds", errmsg);
            thisTest.Verify("val", new object []{100,2,3});

            thisTest.Verify("t", 1);

        }

        [Test]
        public void TV99_Defect_1463456_Array_By_Reference_Issue_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV99_Defect_1463456_Array_By_Reference_Issue_2.ds");

            thisTest.Verify("t", 1);

        }
        [Test]
        public void T100_Class_inheritance_replication()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T100_Class_inheritance_replication.ds");

            thisTest.Verify("b", 5);

        }
        [Test]
        public void T100_Class_inheritance_replication_2()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T100_Class_inheritance_replication_2.ds");
            thisTest.Verify("result", 2);

        }

        [Test]
        [Category("Method Resolution")]
        public void T100_Defect_Class_inheritance_dispatch()
        {
            String code =
 @"

class A
{
        def Test(b : B)
        { return = 1; }
}

class B extends A
{
        def Test(a : A)
        { return = 2; }
}

 
a = A.A();
b = B.B();

r1 = a.Test(a);//null
r2 = b.Test(b);//1

";

            thisTest.RunScriptSource(code);
            //Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch\nunecpected result" );
            thisTest.SetErrorMessage("1467307 - Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type");
            Object v1 = null;
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", 1);

        }

        [Test]
        [Category("Method Resolution")]
        public void T100_Defect_Class_inheritance_dispatch_a()
        {
            String code =
 @"

class A
{
        def Test(b : B)
        { return = 1; }
}

class B extends A
{
        def Test(a : A)
        { return = 2; }
}

 
b = B.B();

r2 = b.Test(b);//1

";

            thisTest.RunScriptSource(code);
            //Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch\nunecpected result" );
            thisTest.SetErrorMessage("1467307 - Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type");
            thisTest.Verify("r2", 1);

        }

        [Test]
        public void T100_Defect_Class_inheritance_replication_1()
        {
            String code =
 @"

class A
{
        def Test(b : B)
        { return = 1; }
}

class B extends A
{
        def Test(a : A)
        { return = 2; }
}

class C extends B
{
    def Test(c:C)
    {return = 3;}
}
 
a = A.A();
b = B.B();
c = C.C();

r1 = a.Test(a);//null
r2 = b.Test(b);//1
r3 = c.Test(c);//3
r4 = a.Test(b);//1
r5 = a.Test(c);//1
r6 = b.Test(a);//2
r7 = b.Test(c);//1
r8 = c.Test(a);//2
r9 = c.Test(b);//1
";

            thisTest.RunScriptSource(code);
            //Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch\nunecpected result");
            thisTest.SetErrorMessage("1467307 - Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type");
            Object v1 = null;
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", 1);
            thisTest.Verify("r3", 3);
            thisTest.Verify("r4", 1);
            thisTest.Verify("r5", 1);
            thisTest.Verify("r6", 2);
            thisTest.Verify("r7", 1);
            thisTest.Verify("r8", 2);
            thisTest.Verify("r9", 1);

        }
        [Test]
        public void TV101_Indexing_IntoArray_InFunctionCall_1463234()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_IntoArray_InFunctionCall_1463234.ds");
            thisTest.Verify("t", 1);

        }
        [Test]
        public void TV101_Indexing_IntoArray_InFunctionCall_1463234_2()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_IntoArray_InFunctionCall_1463234_2.ds");
            thisTest.Verify("t", 1);

        }
        [Test]
        public void TV101_Indexing_Intosingle_InFunctionCall_1463234_2()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_Intosingle_InFunctionCall_1463234_2.ds");
            thisTest.Verify("t", 1);

        }
        [Test]
        public void TV101_Indexing_Intoemptyarray_InFunctionCall_1463234_3()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_Intoemptyarray_InFunctionCall_1463234_3.ds");
            Object t = null;
            thisTest.Verify("t", t);

        }
        [Test]
        public void TV101_Indexing_Intovariablenotarray_InFunctionCall_1463234_4()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_Intovariablenotarray_InFunctionCall_1463234_4.ds");
            Object t = null;
            thisTest.Verify("t", t);

        }
        [Test]
        public void TV101_Indexing_IntoNested_FunctionCall_1463234_5()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_IntoNested_FunctionCall_1463234_5.ds");
            
            thisTest.Verify("t", 1);

        }
        [Test]
        public void TV101_Indexing_Into_classCall_1463234_6()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_Into_classCall_1463234_6.ds");
            
            thisTest.Verify("t", 1);

        }
        [Test]
        public void TV101_Indexing_Into_classCall_1463234_7()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV101_Indexing_Into_classCall_1463234_7.ds");

            thisTest.Verify("t", 1);

        }
        [Test]
        public void TV102_GlobalVariable_Function_1466768()
        {
           // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV102_GlobalVariable_Function_1466768.ds");
            
            thisTest.Verify("xx", 0);
            thisTest.Verify("yy", 0);
            thisTest.Verify("zz", 0);

        }
        [Test]
        public void TV102_GlobalVariable_Function_1466768_1()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV102_GlobalVariable_Function_1466768_1.ds");

            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);

        }
       
        [Test]
        public void TV102_GlobalVariable_Function_1466768_2()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV102_GlobalVariable_Function_1466768_2.ds");

            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1467149()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV103_Defect_1467149.ds");
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("b", new Object[] { new Object[] { 1.0, 1.0 }, 1.0 } );
            

        }
        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg()
        {
            String code =
@"
b;
[Associative]
{
def foo : int ( a : double[][] )
{
return = a[0][0] ; 
}
a = { { 0, 1}, {2, 3} };
b = foo ( a );

}
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_2()
        {
            String code =
@"class A
{
    X : var[][][];
    constructor A ( b : double[]..[] )
    {
        X = b;
    }
    def foo : var[][][] (  )
    {
        return = X ; 
    }
}
a = { { { 0, 1} }, { {2, 3} } };
b = A.A ( a );
c = b.foo();";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", new Object[] { new Object[] { new Object[] { 0.0, 1.0 } }, new Object[] { new Object[] {2.0, 3.0 } } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_3()
        {
            String code =
@"class A
{
    X : var[][][];
    constructor A ( b : double[]..[] )
    {
        X = b;
    }
    def foo : var[][][] (  )
    {
        return = X ; 
    }
}
a = { { { 0, 1} },  {2, 3}  };
b = A.A ( a );
c = b.foo();";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("c", new Object[] { new Object[] { new Object[] { 0.0, 1.0 } }, new Object[] {  new object [] {2.0},new object []{ 3.0 } }  });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_4()
        {
            String code =
@"  class A
{
    X : var[][];
    constructor A ( b : double[][] )
    {
        X = b;
    }
    def foo : var[][][] (  )
    {
        return = X ; 
    }
}
a = { { 0, 1} ,  2  };
b = A.A ( a );
c = b.foo();";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            
            thisTest.Verify("c", new Object[] { new Object[] { new object []{ 0.0}, new object []{ 1.0} },new object []{ new object []{2.0}} });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_4a()
        {
            String code =
@"

def foo : var[] (  )
{
    return = 1 ; 
}

c = foo();";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("c", new Object[] {  1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_4b()
        {
            String code =
@"

def foo : int[] (  )
{
    return = 1 ; 
}

c = foo();";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("c", new Object[] { 1 });
        }

        

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_5()
        {
            String code =
                @"class A
                {
                    X : var[][];
                    constructor A ( b : double[][] )
                    {
                        X = b;
                    }
                    def foo : var[][][] (  )
                    {
                        return = X ; 
                    }
                }
                a = { 3, { 0, 1} ,  2  };
                b = A.A ( a );
                c = b.foo();";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            
            thisTest.Verify("c", new Object[] { new object [] {  new object []{3.0}}, new Object[] {new object[]{ 0.0 }, new object[] { 1.0 } },new object [] {new object[]{ 2.0 }} });
        }

        [Test]
        [Category("SmokeTest")]        
        public void TV103_Defect_1455090_Rank_Of_Arg_6()
        {
            String code =
@"
def foo  ( x : int[][] )
{
        return = x ; 
}

a = { 3, { 0, 1} ,  2  };
b = foo ( a );";


            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            
            thisTest.Verify("b", new Object[] { new object[]{3}, new Object[] { 0, 1 }, new object[]{2} });
        } 

        [Test]
        [Category("SmokeTest")]
        public void TV104_Defect_1467112()
        {
            String code =
@"class A
{ 
    public x : var ;     
    public def foo1 (a)
    {
      return = 1;
    }     
}
class B extends A
{
    public def foo1 (a)
    {
        return = 2;
    }        
}

class C extends B
{
    public def foo1 (a)
    {
        return = 3;
    }         
}

b = C.C();
b1 = b.foo1(1);
test = b1;";


            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", 3);
        } 

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467244()
        {
            String code =
@"def foo(x:int = 2.3)
{
return = x;
}

d = foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467244_2()
        {
            String code =
@"def foo(x:double = 2)
{
return = x;
}

d = foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467244_3()
        {
            String code =
@"def foo(x:int = 2)
{
return = x;
}

d = foo(1.5);";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV106_Defect_1467132()
        {
            String code =
@"def foo : double (x :var[])
{
    
    return = Average(x);
}

a = {1,2,2,1};
b = {1,{}};

c = Average(a);
c1= foo(a);
c2 = foo(b);
c3 = Average({});
result = {foo(a),foo(b)};";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "1467301 - rev 3778 : Builtin method 'Average' returns 0.0 when an empty array is passed to it";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("result", new Object[] { 1.5, new Object[] { 1.0, 0.0 }});
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank()
        {
            String code =
@"def foo(x:var[]..[])
{
return = 2;
}
def foo(x:var[])
{
return = 1;
}

d = foo({1,2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_2()
        {
            String code =
@"def foo(x:int[]..[])
{
return = 2;
}
def foo(x:int[])
{
return = 1;
}

d = foo({1,2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_3()
        {
            String code =
@"def foo(x:double[]..[])
{
return = 2;
}
def foo(x:int[])
{
return = 1;
}

d = foo({1.5,2.5});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_4()
        {
            String code =
@"
class A
{
    def foo(x:int[]..[])
    {
        return = 2;
    }
    def foo(x:int[])
    {
        return = 1;
    }
}

a = A.A();
d = a.foo({1,2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }


        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_4a()
        {
            String code =
@"
class A
{
    def foo(x:int[]..[])
    {
        return = 2;
    }
    def foo(x:int[])
    {
        return = 1;
    }
}

a = A.A();
d = a.foo({1.0,2.2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }
        [Test]
        [Category("SmokeTest")]
        public void TV104_Defect_1467379()
        {
            String code =
@"
def foo (f:function)
{   
return = true;
}      
a = foo(test);
b = 1;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "1467379 Sprint 27 - Rev 4193 - after throwing warning / error in the attached code it should execute rest of the code ";
            thisTest.RunScriptSource(code, err);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467409()
        {
            String code =
@"
def foo ( x1: int, y1 : int )
{
    return = x1;
}
class A {}
a=A.A();
r=a.foo(); // calling a non-exist function shouldn't get a warning at complie time
b = foo1();
d = foo(2, A.A() );
f = foo3();
r1;b1;d1;
[Imperative]
{
    r1=a.foo(); 
    b1 = foo1();
    d1 = foo(2, A.A() );

}

def foo3()
{
    r2=a.foo(); 
    b2 = foo1();
    d2 = foo(2, A.A() );
    return = { r2, b2, d2 };
}

";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "DNL-1467409 Please disable static analysis warnings for function resolution"; ;
            thisTest.RunScriptSource(code, err);
            Object n1 = null;

            thisTest.Verify("r", n1);
            thisTest.Verify("b", n1);
            thisTest.Verify("d", n1);

            thisTest.Verify("r1", n1);
            thisTest.Verify("b1", n1);
            thisTest.Verify("d1", n1);

            thisTest.Verify("f", new Object[] { n1, n1, n1 });
            
        }
        [Test]
        [Category("SmokeTest")]
        public void TV106_1467455()
        {
            String code =
                @"
                def foo ()
                {
                    return 2;
                }

                a= foo()
                ";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = ""; ;
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code, err);
            });
            

            

        }

        [Test]
        [Category("Method Resolution")]
        public void T28_Function_Arguments_Declared_Before_Function_Def()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T28_Function_Arguments_Declared_Before_Function_Def.ds");

            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");


            thisTest.Verify("result", 1);
            thisTest.Verify("result2", 7);
            thisTest.Verify("result3", 11);


        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Function_notDeclared()
        {
            String code = @"

            import(""Math.dll"");
            def foo : double(arg : double) = arg + 1;
            a = foo(""a""); ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("a", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);


        }
        [Test]
        [Category("SmokeTest")]
        public void T64_Function_notDeclared_2()
        {
            String code = @"
x = foo(); // null


";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("x", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);

        }
        [Test]
        [Category("SmokeTest")]
        public void T64_Function_notDeclared_3()
        {
            String code = @"
import(""Math.dll"");

c = Math.Floor(3.0);
d = Floor(3);


";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("d", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);

        }
        [Test]
        [Category("SmokeTest")]
        public void T64_Function_notDeclared_imperative_4()
        {
            String code = @"
import(""Math.dll"");
c;d;
[Imperative]
{
    c = Math.Floor(3.0);
    d = Floor(3);
}


";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("d", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);

        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467115_warning_on_function_resolution_1()
        {
            String code = @"
x = { 1,2};
t = 0;
y = x[null];
p = y + 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("p", null);
            thisTest.VerifyBuildWarningCount(0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467115_warning_on_function_resolution_2()
        {
            String code = @"
a ;
b ;

[Associative]
{
    a = 10;
    b = a * 2;
    a = { 5, 10, 15 };
    
    a = 5..15..2;
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("b", new Object[] { 10, 14, 18, 22, 26, 30 });
            thisTest.VerifyBuildWarningCount(0);

        }
        [Test]
        [Category("SmokeTest")]
        public void T65_1467115_warning_on_function_resolution_3()
        {
            String code = @"
def foo (a : int)
{
    return = 1;
}
def foo(b : double)
{
    return = 2;
}
def foo(b : double[])
{
    return = 3;
}
x = { 1.0, 5, 2.4};
p = foo(x);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("p", new Object[] { 2, 1, 1} );
            thisTest.VerifyBuildWarningCount(0);

        }
        
    }
}


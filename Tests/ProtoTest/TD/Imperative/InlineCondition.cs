using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    public class InlineCondition
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\";
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T001_Inline_Using_Function_Call()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T001_Inline_Using_Function_Call.ds");

            // expected "StatementUsedInAssignment" warning
            Assert.IsTrue((Int64)mirror.GetValue("smallest2").Payload == 100);
            Assert.IsTrue((Int64)mirror.GetValue("largest2").Payload == 400);      
        }

        [Ignore]
        [Category ("SmokeTest")]
 public void T002_Inline_Using_Math_Lib_Functions()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T002_Inline_Using_Math_Lib_Functions.ds");

            // expected "StatementUsedInAssignment" warning
            Assert.IsTrue((Double)mirror.GetValue("smallest2").Payload == 10.0);
            Assert.IsTrue((Double)mirror.GetValue("largest2").Payload == 20.0);
        }

        [Ignore]
        public void T003_Inline_Using_Collection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T003_Inline_Using_Collection.ds");

            // expected "StatementUsedInAssignment" warning
            List<Object> result = new List<object> { 1, 0, 1, 1, 0, };
            Assert.IsTrue(mirror.CompareArrays("Results", result, typeof(System.Int64)));
        }

        [Ignore]
        public void T005_Inline_Using_2_Collections_In_Condition()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T005_Inline_Using_2_Collections_In_Condition.ds");

            // expected "StatementUsedInAssignment" warning
            List<Object> c1 = new List<object> { false, false, false };
            List<Object> c2 = new List<object> { false, false, false };
            List<Object> c3 = new List<object> { false, false, false, null };
            Assert.IsTrue(mirror.CompareArrays("c1", c1, typeof(System.Boolean)));
            Assert.IsTrue(mirror.CompareArrays("c2", c2, typeof(System.Boolean)));
            Assert.IsTrue(mirror.CompareArrays("c3", c3, typeof(System.Object)));
        }
        
        [Ignore]
        public void T006_Inline_Using_Different_Sized_1_Dim_Collections()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T006_Inline_Using_Different_Sized_1_Dim_Collections.ds");

            // expected "StatementUsedInAssignment" warning
            List<Object> b1 = new List<object> { 1, 2, 3, true };
            List<Object> b2 = new List<object> { 1, 2, 3 };
            List<Object> b3 = new List<object> { 1, 2 };
            Assert.IsTrue(mirror.CompareArrays("b1", b1, typeof(System.Object)));
            Assert.IsTrue(mirror.CompareArrays("b2", b2, typeof(System.Int64)));
            Assert.IsTrue(mirror.CompareArrays("b3", b3, typeof(System.Int64)));
        }

        [Ignore]
        public void T007_Inline_Using_Collections_And_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T007_Inline_Using_Collections_And_Replication.CollectionFunctionCall.ds");

            // expected "StatementUsedInAssignment" warning
            List<Object> b = new List<object> { 1, 6, 3, 10, 5 };
            List<Object> c = new List<object> { 4, 6, 8, 0, 2 };
            List<Object> d = new List<object> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            List<Object> e1 = new List<object> { 2, 3 };
            Assert.IsTrue(mirror.CompareArrays("b", b, typeof(System.Int64)));
            Assert.IsTrue(mirror.CompareArrays("c", c, typeof(System.Int64)));
            Assert.IsTrue(mirror.CompareArrays("d", d, typeof(System.Int64)));
            Assert.IsTrue(mirror.CompareArrays("e1", e1, typeof(System.Int64)));
        }

        [Test]
        [Category ("SmokeTest")]
 public void T008_Inline_Returing_Different_Ranks()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T008_Inline_Returing_Different_Ranks.ds");

            // expected "StatementUsedInAssignment" warning
            List<Object> x = new List<object> { 1, 1 };
            Assert.IsTrue(mirror.CompareArrays("x", x, typeof(System.Int64)));
        }

        [Ignore]
        public void T009_Inline_Using_Function_Call_And_Collection_And_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T009_Inline_Using_Function_Call_And_Collection_And_Replication.ds");

            // expected "StatementUsedInAssignment" warning
            List<Object> b = new List<object> { 2, 4, 6 };
            Assert.IsTrue(mirror.CompareArrays("b", b, typeof(System.Int64)));
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 13);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 53);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T010_Inline_Using_Literal_Values()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\InlineCondition\\T010_Inline_Using_Literal_Values.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b").Payload) == false);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("c").Payload) == 4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("d").Payload));
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("e").Payload) == false);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("g").Payload == 0.33333333333333331);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("h").Payload) == 1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T011_Inline_Using_Variables()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T011_Inline_Using_Variables.ds");
            Object n1 = null;
            thisTest.Verify("x1", -1);
            thisTest.Verify("x2", true);
            thisTest.Verify("x4", 1);
            thisTest.Verify("x5", 1);
            thisTest.Verify("x3", 1);
            thisTest.Verify("temp", n1);
        }

        [Test]
        [Category("SmokeTest")]
public void T012_Inline_Using_Fun_Calls()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T012_Inline_Using_Fun_Calls.ds");
            Object n1 = null;

            thisTest.SetErrorMessage("1467231 - Sprint 26 - Rev 3393 null to bool conversion should not be allowed");
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 0);
            thisTest.Verify("x3", 0);
            thisTest.Verify("x4", 0);
            thisTest.Verify("x5", 0);
            thisTest.Verify("x6", 1);
            thisTest.Verify("x7", 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T013_Inline_Using_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T013_Inline_Using_Class.ds");

            thisTest.Verify("x1", -2, 0);
            thisTest.Verify("x2", -1, 0);
            thisTest.Verify("x3", -9, 0);
            thisTest.Verify("x4", 0, 0);
        }

        [Test]
        [Category("Replication")]
        public void T014_Inline_Using_Collections()
        {
                     
            string err = "1467166 - Sprint24 : rev 3133 : Regression : comparison of collection with singleton should yield null in imperative scope";
            thisTest.VerifyRunScriptFile(testPath, "T014_Inline_Using_Collections.ds",err);
            
           
            thisTest.Verify("t1", 3);
            thisTest.Verify("t2", 11);
            thisTest.Verify("c1", 2);

            thisTest.Verify("t3", 3);
            thisTest.Verify("t4", 5);
            thisTest.Verify("c2", 2);

            thisTest.Verify("t5", 3);
            thisTest.Verify("c3", 1);

            thisTest.Verify("t7", 0);
            thisTest.Verify("c4", 1);

        }

        [Test]        
 public void T015_Inline_In_Class_Scope()
        {
            // Assert.Fail("1467168 - Sprint24 : rev 3137 : Compiler error from  Inline Condition and class inheritance issue");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T015_Inline_In_Class_Scope.ds");
            
            thisTest.Verify("x1", 4, 0);
            thisTest.Verify("x2", -3, 0);
            thisTest.Verify("x3", 19, 0);


        }

        [Test]
        [Category("Replication")]
        public void T016_Inline_Using_Operators()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T016_Inline_Using_Operators.ds");
            Object[][] array = { new Object[] {7, 8, 9}, new Object[] {7,8,9} };
            Object[] array2 = {0, 1 };

            thisTest.Verify("a", 5.0, 0);
            thisTest.Verify("b", 1, 0);
            thisTest.Verify("c", 1, 0);
            thisTest.Verify("d", 1, 0);
            thisTest.Verify("e1", 0, 0);
            thisTest.Verify("f", 1, 0);
            thisTest.Verify("g", array, 0);
            thisTest.Verify("i", array2, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T017_Inline_In_Function_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T017_Inline_In_Function_Scope.ds");
            thisTest.Verify("a1", 5, 0);
            thisTest.Verify("a2", 8, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T018_Inline_Using_Recursion()
        {
            Assert.Fail("Cauing NUnit failures. Disabled");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T018_Inline_Using_Recursion.ds");

            thisTest.Verify("fac", 3628800, 0);
      
        }

        [Test]
        [Category ("SmokeTest")]
 public void T019_Defect_1456758()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T019_Defect_1456758.ds");

            thisTest.Verify("a1", -1);
            thisTest.Verify("a2", -1);


      
        }

        [Test]
        [Category ("SmokeTest")]
 public void T020_Nested_And_With_Range_Expr()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T020_Nested_And_With_Range_Expr.ds");

            Object[] ExpectedRes_1 = { 0, 1, 2, 3};
            Object[] ExpectedRes_2 = { 0, 1, 2, 3 };


            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", ExpectedRes_1, 0);
            thisTest.Verify("b", ExpectedRes_2, 0);
            thisTest.Verify("a3", ExpectedRes_2, 0);

        }

        [Test]
        [Category("Imperative")]
        public void T021_Defect_1467166_array_comparison_issue()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T021_Defect_1467166_array_comparison_issue.ds");
            thisTest.Verify("xx", 5);
        }

        [Test]
        [Category("Replication")]
        public void T22_Defect_1467166()
        {
            String code =
@"xx;
[Imperative] 
{
    a = { 0, 1, 2}; 
    xx = a < 1 ? 1 : 0;
}
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("xx", 0);

        }

        [Test]
        [Category("Replication")]
        public void T22_Defect_1467166_2()
        {
            String code =
@"xx;
[Imperative] 
{
    a = { 0, 1, 2}; 
    xx = 2 > 1 ? a : 0;
}
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("xx", new Object[] { 0, 1, 2 });

        }

        [Test]
        [Category("Replication")]
        public void T22_Defect_1467166_3()
        {
            String code =
@"
x1;x2;x3;x4;x5;
[Imperative] 
{
   def foo () = null;
   x1 = null == null ? 1 : 0;
   x2 = null != null ? 1 : 0;
   x3 = null == a ? 1 : 0;
   x4 = foo2(1) == a ? 1 : 0;
   x5 = foo() == null ? 1 : 0;
}
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify( "x1", 1 );
            thisTest.Verify( "x2", 0);
            thisTest.Verify("x3", 1);
            thisTest.Verify("x4", 1);
            thisTest.Verify("x5", 1);
        }
       
         [Test]
         [Category("Replication")]
         public void T23_1467403_inline_null()
         {
             String code =
 @"a = null;
d2 = (a!=null)? 1 : 0;
";

             ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
             String errmsg = "";
             ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

             thisTest.Verify("d2", 0);
             thisTest.VerifyBuildWarningCount(0);

         }
    }
}

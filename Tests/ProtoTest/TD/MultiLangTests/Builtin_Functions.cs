using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    class Builtin_Functions
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Builtin_Functions\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category("SmokeTest")]
        public void T001_SomeNulls_IfElse_01()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T001_SomeNulls_IfElse_01.ds");

            thisTest.Verify("result", true, 0);
        }
        [Test]
        public void test()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "test.ds");
        }

        [Test]
        [Category("SmokeTest")]
        public void T001_SomeNulls_IfElse_02()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T001_SomeNulls_IfElse_02.ds");

            thisTest.Verify("result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T002_SomeNulls_ForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T002_SomeNulls_ForLoop.ds");

            Object[] v1 = new Object[] { false, true, false };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T003_SomeNulls_WhileLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T003_SomeNulls_WhileLoop.ds");

            Object[] v1 = new Object[] { false, true, false };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T004_SomeNulls_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T004_SomeNulls_Function.ds");

            thisTest.Verify("result", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T005_SomeNulls_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T005_SomeNulls_Class.ds");

            thisTest.Verify("m", true, 0);
            thisTest.Verify("n", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_SomeNulls_Inline()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T006_SomeNulls_Inline.ds");

            thisTest.Verify("result", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T007_SomeNulls_RangeExpression()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T007_SomeNulls_RangeExpression.ds");

            thisTest.Verify("result", 1, 0);
        }

        [Test]
        [Category("Replication")]
        public void T008_SomeNulls_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T008_SomeNulls_Replication.ds");
            thisTest.Verify("j", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_SomeNulls_DynamicArray()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T009_SomeNulls_DynamicArray.ds");

            thisTest.Verify("result", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_SomeNulls_AssociativeImperative_01()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T010_SomeNulls_AssociativeImperative_01.ds");

            thisTest.Verify("m", true, 0);
            thisTest.Verify("n", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_SomeNulls_AssociativeImperative_02()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T010_SomeNulls_AssociativeImperative_02.ds");

            thisTest.Verify("m", false, 0);
            thisTest.Verify("n", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_SomeNulls_AssociativeImperative_03()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T010_SomeNulls_AssociativeImperative_03.ds");

            thisTest.Verify("m", true, 0);
            thisTest.Verify("n", true, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T011_SomeNulls_ModifierStack()
        {
            //Assert.Fail("1467062 - Sprint23 : rev 2587 : modifier stack issue : when undefined variable is used inside modfier stack it hangs nunit");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T011_SomeNulls_ModifierStack.ds");
            Object[] v1 = { 1, null };
            Object[] v2 = new Object[] { v1, true, false, v1, true };

            thisTest.Verify("result", v2, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T011_Defect_ModifierStack()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T011_Defect_ModifierStack.ds");
            object x = null;
            Object[] v1 = new[] { x, "n", x, true, false };
            //thisTest.Verify("result", v1,0);
            // thisTest.Verify("a1", x,0);
            //thisTest.Verify("a2", "n");
            //thisTest.Verify("a3", null);
            //thisTest.Verify("a4", true);
            //thisTest.Verify("a5",false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_CountTrue_IfElse()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T012_CountTrue_IfElse.ds");

            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_CountTrue_ForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T013_CountTrue_ForLoop.ds");

            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T014_CountTrue_WhileLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T014_CountTrue_WhileLoop.ds");

            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_CountTrue_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T015_CountTrue_Function.ds");

            Object[] v1 = new Object[] { 0, 1, 0, 3, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T016_CountTrue_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T016_CountTrue_Class.ds");

            thisTest.Verify("m", 2, 0);
            thisTest.Verify("n", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T017_CountTrue_Inline()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T017_CountTrue_Inline.ds");

            thisTest.Verify("result", 0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T018_CountTrue_RangeExpression_01()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T018_CountTrue_RangeExpression_01.ds");

            thisTest.Verify("result", 0, 0);
            Object[] v1 = new Object[] { 1, 4, 7 };
            thisTest.Verify("a", v1, 0);
        }

        /* [Test]
         public void T018_CountTrue_RangeExpression_02()
         {
           Assert.Fail("");
           ExecutionMirror mirror = thisTest.RunScript(testPath, "T018_CountTrue_RangeExpression_02.ds");

           Assert.IsTrue((Int64)mirror.GetValue("result").Payload == 0);
           List<Object> li = new List<Object>() { 1, 4, 9 };
           Assert.IsTrue(mirror.CompareArrays("a", li, typeof(System.Int64)));
         }*/

        [Test]
        [Category("SmokeTest")]
        public void T018_CountTrue_RangeExpression_03()
        {
            //Assert.Fail("");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T018_CountTrue_RangeExpression_03.ds");
            Object[] v1 = new Object[] { 1.0, 4.0, 7.0 };
            thisTest.Verify("a", v1, 0);


            //Assert.IsTrue((Int64)mirror.GetValue("result").Payload == 0);
            //List<Object> li = new List<Object>() { 1, 4, 7 };
            //Assert.IsTrue(mirror.CompareArrays("a", li, typeof(System.Int64)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T019_CountTrue_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T019_CountTrue_Replication.ds");

            Object[] v1 = new Object[] { 3, 1, 2, 4 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T020_CountTrue_DynamicArray()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T020_CountTrue_DynamicArray.ds");
            Object[] v1 = new Object[] { true };
            Object[] v2 = new Object[] { };
            Object[] v3 = new Object[] { v1, v2 };
            Object[] v4 = new Object[] { v3 };
            thisTest.Verify("a2", v4, 0);
            thisTest.Verify("result", 1, 0);
        }

        [Test, Ignore]
        [Category("SmokeTest")]
        public void T021_CountTrue_ModifierStack()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T021_CountTrue_ModifierStack.ds");
            Object[] v1 = new Object[] { 2, 1, null, 2.56, 0 };

            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T022_CountTrue_ImperativeAssociative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T022_CountTrue_ImperativeAssociative.ds");

            thisTest.Verify("b", 1, 0);
            thisTest.Verify("c", 1, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T023_CountFalse_IfElse()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T023_CountFalse_IfElse.ds");

            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T024_CountFalse_ForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T024_CountFalse_ForLoop.ds");

            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T025_CountFalse_WhileLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T025_CountFalse_WhileLoop.ds");

            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T026_CountFalse_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T026_CountFalse_Function.ds");

            Object[] v1 = new Object[] { 0, 1, 0, 3, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T027_CountFalse_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T027_CountFalse_Class.ds");

            thisTest.Verify("m", 2, 0);
            thisTest.Verify("n", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T028_CountFalse_Inline()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T028_CountFalse_Inline.ds");

            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T029_CountFalse_RangeExpression_01()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T029_CountFalse_RangeExpression_01.ds");

            thisTest.Verify("result", 0, 0);
            Object[] v1 = new Object[] { 1, 4, 7 };
            thisTest.Verify("a", v1, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T029_CountFalse_RangeExpression_02()
        {
            //Assert.Fail("");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T029_CountFalse_RangeExpression_02.ds");
            Object[] v1 = new Object[] { 1.0, 4.0, 7.0 };
            thisTest.Verify("a", v1, 0);


            //Assert.IsTrue((Int64)mirror.GetValue("result").Payload == 0);
            //List<Object> li = new List<Object>() { 1, 4, 7 };
            //Assert.IsTrue(mirror.CompareArrays("a", li, typeof(System.Int64)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T030_CountFalse_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T030_CountFalse_Replication.ds");

            Object[] v1 = new Object[] { 3, 1, 2, 4 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T031_CountFalse_DynamicArray()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T031_CountFalse_DynamicArray.ds");
            Object[] v1 = new Object[] { false };
            Object[] v2 = new Object[] { };
            Object[] v3 = new Object[] { v1, v2 };
            Object[] v4 = new Object[] { v3 };
            thisTest.Verify("a2", v4, 0);
            thisTest.Verify("result", 1, 0);
        }

        [Test, Ignore]
        [Category("SmokeTest")]
        public void T032_CountFalse_ModifierStack()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T032_CountFalse_ModifierStack.ds");
            Object[] v1 = new Object[] { 2, 1, null, 2.56, 0 };

            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T033_CountFalse_ImperativeAssociative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T033_CountFalse_ImperativeAssociative.ds");

            thisTest.Verify("b", 1, 0);
            thisTest.Verify("c", 1, 0);
        }




        [Test]
        [Category("SmokeTest")]
        public void T034_AllFalse_IfElse()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T034_AllFalse_IfElse.ds");

            Object[] v1 = new Object[] { false, null, false };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T035_AllFalse_ForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T035_AllFalse_ForLoop.ds");

            Object[] v1 = new Object[] { false, false, false, false, false };

            thisTest.Verify("result", v1, 0);
            //Assert.Fail("1467074 - Sprint23 : rev :2650 : Built-in function AllFalse() doesn't behave as expected ");

        }

        [Test]
        [Category("SmokeTest")]
        public void T036_AllFalse_WhileLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T036_AllFalse_WhileLoop.ds");
            Object[] v1 = new Object[] { false, false, false, false };

            thisTest.Verify("result", v1, 0);
            //Assert.Fail("1467071 - Sprint23 : rev 2635 : Build-in function AllFalse issue : When the array is empty,it returns true");


        }

        [Test]
        [Category("SmokeTest")]
        public void T036_1_Null_Check()
        {
            // Assert.Fail("1467095 - Sprint24 : rev :2747 : when 'false' is expected in a verification, if it's 'null' instead, the test case should not pass");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T036_1_Null_Check.ds");
            //thisTest.Verify("result", false, 0)
            thisTest.Verify("result", null);

        }

        [Test]
        [Category("SmokeTest")]
        public void T037_AllFalse_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T037_AllFalse_Function.ds");

            Object[] v1 = new Object[] { true, true, false };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T038_AllFalse_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T038_AllFalse_Class.ds");

            thisTest.Verify("d", true, 0);
            thisTest.Verify("e", 0, 0);
            thisTest.Verify("f", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T039_AllFalse_Inline()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T039_AllFalse_Inline.ds");

            thisTest.Verify("a", true, 0);
            thisTest.Verify("b", false, 0);
            thisTest.Verify("c", true, 0);

            thisTest.Verify("result", true, 0);
        }

        /* [Test]
         public void T040_AllFalse_Replication()
         {
           ExecutionMirror mirror = thisTest.RunScript(testPath, "T040_AllFalse_Replication.ds");

       
           thisTest.Verify("result", true, 0);
         }p*/

        [Test]
        [Category("SmokeTest")]
        public void T042_AllFalse_DynamicArray()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T042_AllFalse_DynamicArray.ds");
            Object[] v1 = new Object[] { false, true, true, false };

            thisTest.Verify("result", false, 0);
            thisTest.Verify("result2", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T043_AllFalse_ModifierStack()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T043_AllFalse_ModifierStack.ds");
            Object[] v1 = new Object[] { true, false, true, true };

            thisTest.Verify("result", v1, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T044_AllFalse_ImperativeAssociative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T044_AllFalse_ImperativeAssociative.ds");

            thisTest.Verify("n", true, 0);
            thisTest.Verify("m", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T045_Defect_CountArray_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T045_Defect_CountArray_1.ds");

            Object[] v1 = new Object[] { 3, 3, 4 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]

        [Category("Array")]
        public void T045_Defect_CountArray_2()
        {
            Assert.Fail("1467093 - Sprint 24 : rev 2747 : dynamic array issue: assigning value to dynamic array hangs ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T045_Defect_CountArray_2.ds");
            thisTest.Verify("result", 3, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T045_Defect_CountArray_3()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T045_Defect_CountArray_3.ds");


            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T046_Sum_IfElse()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T046_Sum_IfElse.ds");

            Object[] v1 = new Object[] { 10, 10.0, 10.0, 0, 10, 10, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T047_Sum_ForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T047_Sum_ForLoop.ds");
            Object[] v1 = new Object[] { 0, 0, 10.0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T048_Sum_WhileLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T048_Sum_WhileLoop.ds");
            Object[] v1 = new Object[] { -2.0, 8.0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T049_Sum_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T049_Sum_Function.ds");

            thisTest.Verify("result", 1.9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Sum_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T050_Sum_Class.ds");
            Object[] v1 = new Object[] { 11.0, true, 11.0 };
            thisTest.Verify("result", v1, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T051_Sum_Inline()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T051_Sum_Inline.ds");

            thisTest.Verify("result", -1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T052_Sum_RangeExpression()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T052_Sum_RangeExpression.ds");

            thisTest.Verify("result", 12.0, 0);
        }

        /*[Test]
        [Category("SmokeTest")]
        public void T053_Sum_Replication()
        {
          ExecutionMirror mirror = thisTest.RunScript(testPath, "T053_Sum_Replication.ds");

          thisTest.Verify("result", 12.0, 0);
        }*/
        [Test]
        [Category("SmokeTest")]
        public void T054_Sum_DynamicArr()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T054_Sum_DynamicArr.ds");

            thisTest.Verify("result", 12.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T055_Sum_ModifierStack()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T055_Sum_ModifierStack.ds");

            thisTest.Verify("result", 6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T056_Sum_AssociativeImperative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T056_Sum_AssociativeImperative.ds");

            thisTest.Verify("sum1", 0.0, 0);
            thisTest.Verify("sum2", 4.0, 0);
            thisTest.Verify("sum3", 0.0, 0);
        }

        //datatype
        [Test]
        [Category("Design Issue")]
        public void T057_Average_DataType_01()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T057_Average_DataType_01.ds");
            Object v1 = null;
            //Assert.Fail("1467164 - Sprint 25 - Rev 3125: Built-in function: Average() should ignore the elements which can't be converted to int/double in the array");
            thisTest.Verify("a1", v1, 0);
            thisTest.Verify("b1", 2.0, 0);
            thisTest.Verify("c1", 0.4, 0);
            thisTest.Verify("d1", 1.0, 0);//significant digits
        }

        [Test]
        [Category("Design Issue")]
        public void T058_Average_DataType_02()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T058_Average_DataType_02.ds");

            //Assert.Fail("11467164 - Sprint 25 - Rev 3125: Built-in function: Average() should ignore the elements which can't be converted to int/double in the array");
            thisTest.Verify("a1", 0.0, 0);
            thisTest.Verify("b1", 2.0, 0);
            thisTest.Verify("c1", 0.5, 0);
            thisTest.Verify("d1", 1.0, 0);//significant digits
        }

        [Test]
        [Category("SmokeTest")]
        public void T059_Defect_Flatten_RangeExpression()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T059_Defect_Flatten_RangeExpression.ds");

            Object[] v1 = new Object[] { 0, 5, 10, 20, 22, 24, 26, 28, 30 };
            thisTest.Verify("d", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T059_Defect_Flatten_RangeExpression_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T059_Defect_Flatten_RangeExpression_1.ds");

            Object[] v1 = new Object[] { null, 1, 2, 3 };
            thisTest.Verify("d", v1, 0);
        }

        [Test]
        [Category("Design Issue")]
        public void T060_Average_ForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T060_Average_ForLoop.ds");
            Assert.Fail("Data type conversion needs to  be decided");
            Object[] v1 = new Object[] { 0.0, 1.5, 10.0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T061_Average_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T061_Average_Function.ds");

            Object[] v1 = new Object[] { 1.5, 1.0 };
            thisTest.Verify("result", v1, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T062_Average_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T062_Average_Class.ds");


            thisTest.Verify("m1", 2.0, 0);
            thisTest.Verify("m2", 0.7, 0);
            thisTest.Verify("n", 1.35, 0);
        }



        [Test]
        [Category("SmokeTest")]
        public void T063_Average_Inline()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T063_Average_Inline.ds");


            thisTest.Verify("result", true, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T064_Average_RangeExpression()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T064_Average_RangeExpression.ds");

            Object[] v1 = new Object[] { 3.0, 4.0, 5.0 };
            thisTest.Verify("m", 3.0, 0);
            thisTest.Verify("n", 5.0, 0);
            thisTest.Verify("c", v1, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T065_Average_ModifierStack()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T065_Average_ModifierStack.ds");

            thisTest.Verify("a", 2.0, 0);
            thisTest.Verify("a1", 1.0, 0);
            thisTest.Verify("a2", 2.0, 0);

        }

        [Test]
        [Category("Built in Functions")]
        public void T066_Print_String()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T066_Print_String.ds");
            //I believe the result for this case is correct (LC, 2012-08-20)
            //Assert.Fail("1467193 - Sprint25 : rev 3205 : built-in function:Print() function doesn't print out correct result");
        }

        [Test]
        [Category("Built in Functions")]
        public void T067_Print_Arr()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T067_Print_Arr.ds");
        }

        [Test]
        [Category("Built in Functions")]
        public void TV1467193_print()
        {
            String code =
 @"

class A{}
r1 = Print(A.A());

a = A.A();
r2 = Print(a);

class B{
fb:var;
constructor B(x:int)
{
    fb = Print(x);
  
}
  def foo()
    {
        return  = 10;
    }
}

 r3 = Print(B.B(2));
r4 = Print(B.B(2).foo());
";

            thisTest.RunScriptSource(code);
        }


        [Test]
        [Category("Built in Functions")]
        public void T068_Average_Negative_Cases()
        {
            String code =
 @"
//x1 = Average(a) ;// returns null, also throws runtime error ? 
x2 = Average(a) ;// returns -1
//x3 = Average(()); // returns null, also throws runtime error ?
x4 = Average(null) ;// returns -1
x5 = Average({}) ;// returns 0.0
x6 = Average({null}) ;// returns 0.0
";

            Object n1 = null;
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "DNL-1467301 rev 3778 : Builtin method 'Average' should return null for all negative cases";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x2", n1);
            thisTest.Verify("x4", n1);
            thisTest.Verify("x5", n1);
            thisTest.Verify("x6", n1);


        }
        /*
       [Test]
       [Category("Built in Functions")]
       public void TV1467301_Average()
       {
           String code =
@"



";

           Object n1 = null;
           ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
           String errmsg = "DNL-1467301 rev 3778 : Builtin method 'Average' should return null for all negative cases";
           ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
           thisTest.Verify("x2", n1);
           thisTest.Verify("x4", n1);
           thisTest.Verify("x5", n1);
           thisTest.Verify("x6", n1);


       }*/

        [Test]
        //Test "IsRectangular"
        public void CountInClass_1467364()
        {
            String code =
                @"class td
                {
                    y : int[];
                    z;
                    constructor td()
                    {
                        y = { 1, 2, 3, 4, 5 };
                        rows = { 1, 2, 3 };
                        z = y[Count(rows)];
                    }
                }
                a = td.td();
                c=a.z; // 4 
                ";

            string error = "1467364 Sprint 27 - Rev 4053 if built-in function is used to index into an array inside class , compile error is thrown ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("c", 4);


        }

        [Test]
        [Category("Built in Functions")]
        public void T068_Sort_UsingFunctionPointer()
        {
            string code = @"
            def Compare(x : bool, y : bool)
            {
                return = [Imperative]
                {
                    if (x == y)
                        return = 0;
        
                    return = !x ? -1 : 1;
                }
            }

            def Compare(x : int, y : bool)
            {
                return = 1;
            }

            def Compare(x : bool, y : int)
            {
                return = -1;
            }

            def Compare(x : int, y : int)
            {
                return = (x - y);
            }

            arr = { 3, 5, 1, true, false, 5, 3, 0, 4, 7, true, 5, false, 12};
            sorted = Sort(Compare, arr);
           ";
            string error = "T068_Sort_UsingFunctionPointer failed!!";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("sorted", new Object[] { false, false, true, true, 0, 1, 3, 3, 4, 5, 5, 5, 7, 12 });
        }

        [Test]
        [Category("Built in Functions")]
        public void TV_1467348_Rank()
        {
            string code = @"
r1 = Rank(); //null
r2 = Rank(1);//0
r3 = Rank({ });//1
r4 = Rank({ {  } });//2
r5 = Rank({ { ""123"" } });//2

               ";

            thisTest.RunScriptSource(code);
            //thisTest.SetErrorMessage("1467348 - Language: Rank(3) should return 0");
            thisTest.Verify("r1", null);
            thisTest.Verify("r2", 0);
            thisTest.Verify("r3", 1);
            thisTest.Verify("r4", 2);
            thisTest.Verify("r5", 2);
        }


        [Test]
        [Category("Built in Functions")]
        public void TV_1467322_CountTrue_1()
        {
            string code = @"
                a = { { }, true, false };//1
b = { 1, true };//1
c = { ""c"" };//0
d = { 0 };//0
e = { { true }, true }; //2
f = { };//0
g = 1; //0
h = null; //null
i = { { } }; //0
j = { null }; //0
k = ""string"";//0

ra = CountTrue(a);
rb = CountTrue(b);
rc = CountTrue(c);
rd = CountTrue(d);
re = CountTrue(e);
rf = CountTrue(f);
rg = CountTrue(g);
rh = CountTrue(h);
ri = CountTrue(i);
rj = CountTrue(j);
rk = CountTrue(k);


               ";

            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467420 - REV:4495 When passing in a single value in a built-in function which takes in an array, the single value should be upgraded to one dimension array");
            thisTest.Verify("ra", 1);
            thisTest.Verify("rb", 1);
            thisTest.Verify("rc", 0);
            thisTest.Verify("rd", 0);
            thisTest.Verify("re", 2);
            thisTest.Verify("rf", 0);
            thisTest.Verify("rg", 0);
            thisTest.Verify("rh", null);
            thisTest.Verify("ri", 0);
            thisTest.Verify("rj", 0);
            thisTest.Verify("rk", 0);
        }

        [Test]
        [Category("Built in Functions")]
        public void TV_1467322_CountTrue_2()
        {
            string code = @"
a = { { }, true, false };//1
b = { 1, true };//1
c = { ""c"" };//0
d = { 0 };//0
e = { { true }, true }; //2
f = { };//0
g = 1; //0
h = null; //null
i = { { } }; //0
j = { null }; //0
k = ""string"";//0

arr = {a,b,c,d,e,f,g,h,i,j,k};

r = CountTrue(arr);


               ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 4);
        }

        [Test]
        [Category("Built in Functions")]
        public void TV_1467322_CountTrue_ModifierStack()
        {
            string code = @"
a = { { }, true, false };//1
b = { 1, true };//1
c = { ""c"" };//0
d = { 0 };//0
e = { { true }, true }; //2
f = { };//0
g = 1; //0
h = null; //null
i = { { } }; //0
j = { null }; //0
k = ""string"";//0


r = 
{
     CountTrue(a) =>ra;
     CountTrue(b)=>rb ;
     CountTrue(c)=>rc;
     CountTrue(d)=>rd;
     CountTrue(e)=>re;
     CountTrue(f)=>rf;
     CountTrue(g)=>rg;
     CountTrue(h)=>rh;
     CountTrue(i)=>ri;
     CountTrue(j)=>rj;
     CountTrue(k)=>rk;
}


               ";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467420 - REV:4495 When passing in a single value in a built-in function which takes in an array, the single value should be upgraded to one dimension array");
            thisTest.Verify("ra", 1);
            thisTest.Verify("rb", 1);
            thisTest.Verify("rc", 0);
            thisTest.Verify("rd", 0);
            thisTest.Verify("re", 2);
            thisTest.Verify("rf", 0);
            thisTest.Verify("rg", 0);
            thisTest.Verify("rh", null);
            thisTest.Verify("ri", 0);
            thisTest.Verify("rj", 0);
            thisTest.Verify("rk", 0);
        }
        [Test]
        [Category("Built in Functions")]
        public void TV_1467348_Rank_2()
        {
            String code =
@"a = Rank(1);
a1 = Rank({ });
a2 = Rank({ { } });
a3 = Rank({ 1 });
a4 = Rank({ { { 1 } } });
";

            thisTest.RunScriptSource(code);

            thisTest.Verify("a", 0);
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 2);
            thisTest.Verify("a3", 1);
            thisTest.Verify("a4", 3);
        }

        [Test]
        [Category("Built in Functions")]
        public void TV_1467350_Flatten()
        {
            String code =
@"

a = {};
b = 1;
c = {{}};
d = {{{}}};
e = {1,2,{3,4}};
f = {null};
g = {{null}};
h = {null,{1}};
i = {""1234"", true};
j = {true,{},null};

fa = Flatten(a);//{}
fb = Flatten(b);//{1}

fc = Flatten(c);//{}
fd = Flatten(d);//{}

fe = Flatten(e);//{1,2,3,4}
ff = Flatten(f);//{}

fg = Flatten(g);//{}
fh = Flatten(h);//{1}

fi = Flatten(i);//{""1234"", true}
fj = Flatten(j);//{true};
";

            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467350 - IDE: t = Faltten(3), the output of t is \"t = {Value not yest supported for tracking}\"");
            thisTest.SetErrorMessage("1467420 - REV:4495 When passing in a single value in a built-in function which takes in an array, the single value should be upgraded to one dimension array");
            Object[] v1 = new Object[] { };
            //Object v2 = null;
            Object[] v3 = new Object[] { 1, 2, 3, 4 };
            Object[] v4 = new Object[] { "1234", true };
            Object[] v5 = new Object[] { 1 };
            Object[] v6 = new Object[] { true };
            Object[] v7 = new Object[] { null };
            Object[] v8 = new Object[] { null, 1 };
            thisTest.Verify("fa", v1);
            thisTest.Verify("fb", v5);
            thisTest.Verify("fc", v1);
            thisTest.Verify("fd", v1);
            thisTest.Verify("fe", v3);
            thisTest.Verify("ff", v7);
            thisTest.Verify("fg", v7);
            thisTest.Verify("fh", v8);
            thisTest.Verify("fi", v4);
            thisTest.Verify("fj", v6);
        }

        [Test]
        [Category("Built in Functions")]
        public void T069_IsRectangular_DataType()
        {
            String code =
@"

a = {1};
b = {1,2};
c = {};
d = {{},{}};
e = 1;
ra = IsRectangular(a);
rb = IsRectangular(b);
rc = IsRectangular(c);
rd = IsRectangular(d);
re = IsRectangular(e);
";

            thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("ra", false);
            thisTest.Verify("rb", false);
            thisTest.Verify("rc", false);
            thisTest.Verify("rd", true);
            thisTest.Verify("re", v1);
        }
        [Test]
        [Category("Built in Functions")]
        public void T070_1467416_Count_Single()
        {
            String code =
@"

a = Count(1);

";
            string error = "1467416 Count returns null if the input argument is single value ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", 1);


        }

        [Test]
        [Category("Built in Functions")]
        public void T071_Insert_NegativeIndex01()
        {
            string code = @"
x = {1, 2};
y = 3;
z = Insert(x, y, -5);";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("z", new object[] { 3, null, null, 1, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T072_Insert_NegativeIndex02()
        {
            string code = @"
x = {1, 2};
y = 3;
z = Insert(x, y, -1);";
            string error = "DNL-1467590 Insert at negative index is giving incorrect result";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("z", new object[] { 1, 2, 3 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T073_Insert_ShadowCopy()
        {
            string code = @"
x = { 1, 2 };
y = { 4, 5 };
z = Insert(x, y, 0);
y[0] = 100;";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("z", new object[] { new object[] { 4, 5 }, 1, 2 });

        }

        [Test]
        [Category("Built in Functions")]
        public void T074_CountTrue()
        {
            string code = @"
C1 = CountTrue(1);
// expect C = 0, get C = null

C2 = CountTrue(true);
// expect C = 1, get C = null";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("C1", 0);
            thisTest.Verify("C2", 1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T075_CountFalse()
        {
            string code = @"
C1 = CountFalse(1);
// expect C = 0, get C = null

C2 = CountFalse(false);
// expect C = 1, get C = null";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("C1", 0);
            thisTest.Verify("C2", 1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T076_CountSingle()
        {
            string code = @"
a = Count(1);
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index()
        {
            string code =
@"
x = { 1, 2 };
y = 3;
z = Insert(x, y, -5);  
// expect z = {3, null, null, 1, 2}
//            -5  -4    -3   -2  -1
// but got warning Index out of range and z = null. 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 3, n1, n1, 1, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_2()
        {
            string code =
@"
x = { 1, 2 };
y = {3,3};
z = Insert(x, y, -1);  

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 3, 3 }, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_3()
        {
            string code =
@"
z;
[Imperative]
{
    x = { 1, 2 };
    y = {3,3};
    z = Insert(x, y, -1);  
}

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 3, 3 }, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_4()
        {
            string code =
@"
def foo ()
{
    x = { 1, 2 };
    y = {3,3};
    z = Insert(x, y, -1);  
    return = z;
}
z = foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 3, 3 }, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_5()
        {
            string code =
@"
def foo ()
{
    x = { 1, 2 };
    y = {3,3};
    z = Insert(1..2, 1..2, -1);  
    return = z;
}
z = foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 1, 2 }, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_6()
        {
            string code =
@"
class A
{
    static def foo ()
    {
        x = { 1, 2 };
        y = {3,3};
        z = Insert({1,2}, 1..2, -1);  
        return = z;
    }
}
z = A.foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 1, 2 }, 2 });
        }
        [Test]
        //Test 1467446
        public void BIM31_Sort()
        {
            String code =
@"a = { 3, 1, 2 };


def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}

sort = Sort(sorterFunction, a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });

        }
        [Test]
        //Test "1467446"
        public void BIM31_Sort_null()
        {
            String code =
@"c = { 3, 1, 2,null };


def sorterFunction(a : int, b : int)
{
    return = [Imperative]
    {
        if (a == null)
            return = -1;
        if (b == null)
            return = 1;
        return = a > b ? 10 : -10;
    }
}

sort = Sort(sorterFunction, c);


";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { null, 1, 2, 3 });

        }
        [Test]
        //Test "1467446"
        public void BIM31_Sort_duplicate()
        {
            String code =
@"c = { 3, 1, 2, 2,null };


def sorterFunction(a : int, b : int)
{
    return = [Imperative]
    {
        if (a == null)
            return = -1;
        if (b == null)
            return = 1;
        return = a - b;
    }
}

sort = Sort(sorterFunction, c);


";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { null, 1, 2, 2, 3 });

        }
        [Test]
        //Test "1467446"
        public void BIM31_Sort_Associative()
        {
            String code =
@"
sort;
[Associative]
{
a = { 3, 1, 2 };


def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}

sort = Sort(sorterFunction, a);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });

        }
        [Test]
        //Test "1467446"
        public void BIM32_Sort_class()
        {
            String code =
@"

class test{
a = { 3, 1, 2 };
sort;

def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}
def create ()
{
sort = Sort(sorterFunction, a);
}
}
z=test.test();
y=z.create();

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });

        }
        [Test]
        //Test "1467446"
        public void BIM33_Sort_class_2()
        {
            String code =
@"

class test{
a = { 3, 1, 2 };
sort;

def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}
def create ()
    {
y=test.test();        
sort = Sort(y.sorterFunction, a);
}
}
z=test.test();
y=z.create();

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new object[] { 1, 2, 3 });

        }
        [Test]
        //Test "1467446"
        public void BIM34_Sort_imperative()
        {
            String code =
@"

sort;
a1;
[Imperative]
{
    a1 =  { 3, 1, 2 };

   // c = Flatten(a1);
def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}

sort = Sort(sorterFunction,a1);
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });

        }


        [Test]
        public void BIM35_Sort_modifierblocks_1467446()
        {//1467446
            String code =
            @"
            sort;
            a1;
            [Imperative]
            {
                a1 =  { 3, 1, 2 };

               // c = Flatten(a1);
            def sorterFunction(a : double, b : int)
            {
                return = a > b ? 1 : -1;
            }

            sort = Sort(sorterFunction,a1);
            }

            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }
        [Test]
        public void BIM36_Sort_conditional_1467446()
        {//1467446
            String code =
            @"
          def sorterFunction(a : double, b : int)
                {
                      return =a > b ? 1 : -1;
                }
                def sorterFunction2(a : double, b : int)
                {

                      return =a < b ? 1 : -1;

                }
                sort = { { 3, 1, 3 } => toSort;
                false => ascend;
                ascend!=false?Sort(sorterFunction, toSort):Sort(sorterFunction2, toSort) => sort;
                }
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }
        [Test]
        public void BIM37_Sort_nested_blocks_1467446()
        {//1467446
            String code =
            @"
                sort;
                a1;
                [Associative]
                {
                [Imperative]
                {
                [Associative]
                {
                [Imperative]
                {
                a1 = { 3, 1, 2 };
                // c = Flatten(a1);
                def sorterFunction(a : double, b : int)
                {
                    return = a > b ? 1 : -1;
                }

                sort = Sort(sorterFunction, a1);
                }
                }
                }
                } 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }
        [Test]
        public void BIM38_Sort_nested_blocks_1467446_2()
        {//1467446
            String code =
            @"
                sort;
                a1;
                [Imperative]
                {
                [Associative]
                {
                [Imperative]
                {
                [Associative]
                {
                a1 = { 3, 1, 2 };
                // c = Flatten(a1);
                def sorterFunction(a : double, b : int)
                {
                    return = a > b ? 1 : -1;
                }

                sort = Sort(sorterFunction, a1);
                }
                }
                }
                } 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }
        [Test]
        public void BIM39_Sort_multiarray_1467446()
        {//1467446
            String code =
            @"
                sort;
                a1;

                a1 = { { 4, 2, 3 }, { 2, 5, 1 }, { 8, 4, 6 } };

                // c = Flatten(a1);
                def sorterFunction(a : int, b : int)
                {
                    return = a > b ? 1 : -1;
                }

                def foo(a : int[])
                {
                    sort = Sort(sorterFunction, a);
                    return = sort;
                }

                d = foo(a1);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { new object[] { 2, 3, 4 }, new object[] { 1, 2, 5 }, new object[] { 4, 6, 8 } });
        }
        [Test]
        public void BIM40_Sort_multiarray_1467446_2()
        {//1467446
            String code =
            @"
                sort;
                a1;

                a1 = {  4, 2, 3 ,2, 5, 1 , 8, 4, 6  };

                // c = Flatten(a1);
                def sorterFunction(a : int, b : int)
                {
                    return = a > b ? 1 : -1;
                }

                def foo(a : int[])
                {
                    sort = Sort(sorterFunction, a);
                    return = sort;
                }

                d = foo(a1);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 2, 3, 4, 4, 5, 6, 8 });
        }
        [Test]
        public void BIM37_Sort_rangeexpression_1467446()
        {//1467446
            String code =
            @"
          def sorterFunction(a : double, b : double)
            {

  
                  return =a > b ? 1 : 0;

  
            }

            sort = { (-5..5) => toSort;

            Sort(sorterFunction, toSort) => sort2;
            }
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("toSort", new object[] { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 });
        }
        [Test]
        public void BIM41_Sort_updateinput_1467446()
        {//1467446
            String code =
            @"
          sort;
a1;

a1 = { { 4, 2, 3 }, { 2, 5, 1 }, { 8, 4, 6 } };


def sorterFunction(a : int, b : int)
{
    return = a > b ? 1 : -1;
}


    def foo(a : int[])
    {
        sort = Sort(sorterFunction, a);
        return = sort;
    }
sorted= foo(a1);
a1 = { { 4, 2, 3 }, { 2, 5, 1 },{ 2,11,7}, { 8, 4, 6 }  };


            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sorted", new object[] { new object[] { 2, 3, 4 }, new object[] { 1, 2, 5 }, new object[] { 2, 7, 11 }, new object[] { 4, 6, 8 } });
        }
        [Test]
        public void BIM42_Sort_imperative_while_1467446()
        {//1467446
            String code =
            @"
 sort;
a1;

a1 = { { 4, 2, 3 }, { 2, 5, 1 }, { 8, 4, 6 } };


def sorterFunction(a : int, b : int)
{
    return = a > b ? 1 : -1;
}

d = { };
[Imperative]
{
    def foo(a : int[])
    {
        sort = Sort(sorterFunction, a);
        return = sort;
    }
    dim = Count(a1);
    i = 0;
    while(i < dim)
    {
        d[i ] = foo(a1[i ]);
        i = i + 1;
    }
}


            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { 2, 3, 4 }, new object[] { 1, 2, 5 }, new object[] { 4, 6, 8 } });
        }

        [Test]
        public void BIM45_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
           input = { true, true, { true, false }, { true,false } };
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { true, new object[] { true, false } });
        }
        [Test]
        public void BIM46_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
           input = {  { false,true }, { true,false } };
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { new object[] { false, true }, new object[] { true, false } });
        }
        [Test]
        public void BIM47_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
           input = {  { false,true , true,false } };
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { new object[] { false, true, true, false } });
        }
        [Test]
        public void BIM48_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input =   { true ,false, true,false };
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { true, false });
        }

        [Test]
        public void BIM50_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input =  { true ,false, true,false,null };
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { true, false, null });
        }
        [Test]
        public void BIM51_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input = { 1,-1,3,6,{1} };
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { 1, -1, 3, 6, new object[] { 1 } });
        }
        [Test]
        public void BIM52_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input = { 1,-1,3,6,0,{1} };
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { 1, -1, 3, 6, 0, new object[] { 1 } });
        }
        [Test]
        public void BIM53_RemoveDuplicates_geoemtry_1467447()
        {//1467446
            String code =
            @"
            import(""ProtoGeometry.dll"");
            pt = Point.ByCoordinates(1, 1, 1);

            input = { pt, pt};
            removeDuplicatesSetInsert = RemoveDuplicates(input);
            count = Count(removeDuplicatesSetInsert);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("count", 1);
        }
        [Test]
        public void BIM54_RemoveDuplicates_imperative_1467447()
        {//1467446
            String code =
            @"
            result;
            [Imperative]
            {
                a = { true, true, { false, true } };
                result = RemoveDuplicates(a); //
            }
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", new object[] { true, new object[] { false, true } });
        }
        [Test]
        public void BIM55_RemoveDuplicates_modifier_1467447()
        {//1467446
            String code =
            @"
            a = { { true, true, { false, true } } => a1;
            RemoveDuplicates(a1) => a2; }
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { true, new object[] { false, true } });
        }
        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV()
        {
            String code =
@"a = ""../../../Scripts/TD/testCSV/test.csv"";
b = ImportFromCSV(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_2()
        {
            String code =
@"a = ""../../../Scripts/TD/testCSV/test.csv"";
b = ImportFromCSV(a);

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_replicated_3()
        {
            String code =
@"a = ""../../../Scripts/TD/testCSV/test.csv"";
b = ""../../../Scripts/TD/testCSV/test2.csv"";
c = { a, b };
d = ImportFromCSV(c);

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_replicated_4()
        {
            String code =
@"a = ""../../../Scripts/TD/testCSV/test.csv"";
b = ""../../../Scripts/TD/testCSV/test2.csv"";
c = { a, b };
d = ImportFromCSV(c);

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_replicated_5()
        {

            String code =
@"a = ""../../../Scripts/TD/testCSV/test.csv"";
b = ""../../../Scripts/TD/testCSV/test2.csv"";
c = { a, b };
d = ImportFromCSV(c);

";
            string err = "";
            string path = @"C:\designscript\autodeskresearch\trunk\DesignScript\Prototype\Scripts\TD\";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err, path);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_imperative_5()
        {

            String code =
@"
d;
[Imperative]
{
a = ""../../../Scripts/TD/testCSV/test.csv"";
b = ""../../../Scripts/TD/testCSV/test2.csv"";
c = { a, b };
d = ImportFromCSV(c);
}

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_nested_imperative_6()
        {

            String code =
@"
d;
[Associative]
{
[Imperative]
{
[Associative]
{
a = ""../../../Scripts/TD/testCSV/test.csv"";
b = ""../../../Scripts/TD/testCSV/test2.csv"";
c = { a, b };
d = ImportFromCSV(c);
}
}
}

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }

);

        }

        [Test]
        //Test "LoadCSV"
        public void BIM24_RemoveIfNot()
        {

            String code =
@"
a = { true,null,false,true};
b = RemoveIfNot(a, ""bool"");

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true, false, true }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM24_RemoveIfNot_Imperative()
        {

            String code =
@"
b;
[Imperative]
{
    a = { true,null,false,true};
    b = RemoveIfNot(a, ""bool"");
}

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true, false, true }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM25_RemoveIfNot_variable()
        {

            String code =
@"
b;

    a = { true,null,false,true};
    c=""bool"";
    b = RemoveIfNot(a, c);

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true, false, true }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM26_RemoveIfNot_int()
        {

            String code =
@"
b;

    a = { true,null,false,true};
    c=""int"";
    b = RemoveIfNot(a, c);

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM26_RemoveIfNot_double()
        {

            String code =
@"
b;

    a = { 1.0,null,1,2};
    c=""double"";
    b = RemoveIfNot(a, c);

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 1.0 }

);

        }
        [Test]
        //Test "LoadCSV"
        public void BIM27_RemoveIfNot_heterogenous()
        {

            String code =
@"
b;

    a = { true,null,{true},false};
    c=""array"";
    b = RemoveIfNot(a, c);

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { true } }

);

        }
        [Test]
        public void BIM27_1467445_Transpose()
        {

            String code =
@"
a = { 1, { 2, 3 }, { 4, 5, { 6, 7 } } };
b = Transpose(a);
c = Transpose(Transpose(a));


";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { 4, 5, new object[] { 6, 7 } } });
            thisTest.Verify("b", new object[] { new object[] { 1, 2, 4 }, new object[] { null, 3, 5 }, new object[] { null, null, new object[] { 6, 7 } } });
            thisTest.Verify("c", new object[] { new object[] { 1, null, null }, new object[] { 2, 3, null }, new object[] { 4, 5, new object[] { 6, 7 } } });
        }
        [Test]
        public void BIM27_1467445_Transpose_2()
        {

            String code =
@"
a = { 1, { 2, 3 }, {  { 6, 7 } } };
b = Transpose(a);
c = Transpose(Transpose(a));


";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { new object[] { 6, 7 } } });
            thisTest.Verify("b", new object[] { new object[] { 1, 2, new object[] { 6, 7 } }, new object[] { null, 3, null } });
            thisTest.Verify("c", new object[] { new object[] { 1, null }, new object[] { 2, 3 }, new object[] { new object[] { 6, 7 }, null } });
        }
        [Test]
        public void BIM27_1467445_Transpose_3()
        {

            String code =
@"
a = { 1, { 2, 3 }, { { { 6, 7 } } } };
b = Transpose(a);
c = Transpose(Transpose(a));


";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { new object[] { new object[] { 6, 7 } } } });
            thisTest.Verify("b", new object[] { new object[] { 1, 2, new object[] { new object[] { 6, 7 } } }, new object[] { null, 3, null } });
        }
        [Test]
        public void BIM27_1467445_Transpose_4()
        {

            String code =
@"
a = 4;
b = Transpose(a);
c = Transpose(Transpose(a));


";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", 4);
        }
        [Test]
        public void BIM27_1467445_Transpose_5()
        {

            String code =
@"
a = null;
b = Transpose(a);
c = Transpose(Transpose(a));


";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
        }
        [Test]
        public void BIM27_1467445_Transpose_6()
        {

            String code =
@"
def foo()
{
    return = { 1, { 2, 3 }, 4 };
}

b = Transpose(foo());
c = Transpose(Transpose(foo()));


";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 1, 2, 4 }, new object[] { null, 3, null } });
            thisTest.Verify("c", new object[] { new object[] { 1, null }, new object[] { 2, 3 }, new object[] { 4, null } });
        }
        [Test]
        public void BIM27_1467445_Transpose_7()
        {

            String code =
@"
 a = { 1, { 2, 3 }, { 4, 5, { 6, 7 } } };
 b;
c;
 [Imperative]
 {
    b = Transpose(a);
    c = Transpose(Transpose(a));
 }


";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { 4, 5, new object[] { 6, 7 } } });
            thisTest.Verify("b", new object[] { new object[] { 1, 2, 4 }, new object[] { null, 3, 5 }, new object[] { null, null, new object[] { 6, 7 } } });
            thisTest.Verify("c", new object[] { new object[] { 1, null, null }, new object[] { 2, 3, null }, new object[] { 4, 5, new object[] { 6, 7 } } });
        }

        [Test]
        public void T068_Abs_1()
        {

            String code =
@"
 import (""Math.dll"");
 a = -1.5;
 b = { -3, 0, -4.5 };

 a1 = Math.Abs( a ) ;
 b1 = Math.Abs( b ) ;
 c1 = Math.Abs( -2..3 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1.5);
            thisTest.Verify("b1", new object[] { 3, 0, 4.5 });
            thisTest.Verify("c1", new object[] { 2, 1, 0, 1, 2, 3 });

        }

        [Test]
        public void T068_Abs_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Abs_2.ds");
            thisTest.Verify("t1", 3);
            thisTest.Verify("t2", 3);
            thisTest.Verify("t3", 3);
            thisTest.Verify("t4", 3);
            thisTest.Verify("t5", 2.5);
            thisTest.Verify("t6", 5.5);
            thisTest.Verify("t7", 2);
        }

        [Test]
        public void T068_Cosh_1()
        {

            String code =
@"
 import (""Math.dll"");
 a = -1.5;
 b = { -3, 0, -4.5 };

 a1 = Math.Cosh( a ) ;
 b1 = Math.Cosh( b ) ;
 c1 = Math.Cosh( -2..3 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 2.3524096152432472);
            thisTest.Verify("b1", new object[] { 10.067661995777765, 1.0, 45.014120148530026 });
            thisTest.Verify("c1", new object[] { 3.7621956910836314, 1.5430806348152437, 1.0, 1.5430806348152437, 3.7621956910836314, 10.067661995777765 });

        }

        [Test]
        public void T068_Cosh_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Cosh_2.ds");
            thisTest.Verify("t1", 4.7621956910836314);
            thisTest.Verify("t2", 5.3052763258988751);
            thisTest.Verify("t3", 10.067661995777765);
            thisTest.Verify("t4", 10.067661995777765);
            thisTest.Verify("t5", 6.1322894796636866);
            thisTest.Verify("t6", 4624.58682487247);
            thisTest.Verify("t7", 3.7621956910836314);
        }

        [Test]
        public void T068_DivRem_1()
        {

            String code =
@"
 import (""Math.dll"");
 x = 0;
 a1 = Math.DivRem( 1000, 300 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 100);
        }

        [Test]
        public void T068_DivRem_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_DivRem_2.ds");
            thisTest.Verify("t1", 101);
            thisTest.Verify("t2", 200);
            thisTest.Verify("t3", 100);
            thisTest.Verify("t4", 100);
            thisTest.Verify("t5", 100);
            thisTest.Verify("t6", 100);
            thisTest.Verify("t7", 100);
        }

        [Test]
        public void T068_IEEERemainder_1()
        {

            String code =
@"
import (""Math.dll"");
a1 = Math.IEEERemainder( 3, 2 ) ;
a2 = Math.IEEERemainder( 3..4, 2..3 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", -1.0);
            thisTest.Verify("a2", new Object[] { -1.0, 1.0 });
        }

        [Test]
        public void T068_IEEERemainder_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_IEEERemainder_2.ds");
            thisTest.Verify("t1", 101.0);
            thisTest.Verify("t2", 200.0);
            thisTest.Verify("t3", 100.0);
            thisTest.Verify("t4", 100.0);
            thisTest.Verify("t5", 100.0);
            thisTest.Verify("t6", 100.0);
            thisTest.Verify("t7", 100.0);
        }

        [Test]
        public void T068_Max_1()
        {

            String code =
@"
import (""Math.dll"");

a1 = Math.Max( 1, 1.0 ) ;
b1 = Math.Max( 2.5, 2.50 ) ;
c1 = Math.Max( -2, -2.0 ) ;
d1 = Math.Max( -2, -2.0 ) ;
t1 = Math.Max( -2, 4.5 ) ;
t2 = Math.Max( -2, -2.1 ) ;
t3 = Math.Max( -2, -2.1 ) ;
t4 = Math.Max( -2..2, -2.1..2.1 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1.0);
            thisTest.Verify("b1", 2.5);
            thisTest.Verify("c1", -2.0);
            thisTest.Verify("d1", -2.0);
            thisTest.Verify("t1", 4.5);
            thisTest.Verify("t2", -2.0);
            thisTest.Verify("t3", -2.0);
            thisTest.Verify("t4", new Object[] { -2.0, -1.0, 0.0, 1.0, 2.0 });

        }
        [Test]
        public void T068_Max_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Max_2.ds");
            thisTest.Verify("t1", new Object[] { 0.0, 1.0, 2.0 });
            thisTest.Verify("t2", -2.0);
            thisTest.Verify("t3", -1.0);
            thisTest.Verify("t4", -1.0);
            thisTest.Verify("t5", new Object[] { -1.0, 0.0, 1.0 });
            thisTest.Verify("t6", new Object[] { -1.0, 0.0, 1.0 });
            thisTest.Verify("t7", new Object[] { -1.0, 0.0, 1.0 });
            thisTest.Verify("t8", new Object[] { -1.0, 0.0, 1.0 });

        }

        [Test]
        public void T068_Min_1()
        {

            String code =
@"
import (""Math.dll"");

a1 = Math.Min( 1, 1.0 ) ;
b1 = Math.Min( 2.5, 2.50 ) ;
c1 = Math.Min( -2, -2.0 ) ;
d1 = Math.Min( -2, -2.0 ) ;
t1 = Math.Min( -2, 4.5 ) ;
t2 = Math.Min( -2, -2.1 ) ;
t4 = Math.Min( -2..2, -2.1..2.1 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1.0);
            thisTest.Verify("b1", 2.5);
            thisTest.Verify("c1", -2.0);
            thisTest.Verify("d1", -2.0);
            thisTest.Verify("t1", -2.0);
            thisTest.Verify("t2", -2.1);
            thisTest.Verify("t4", new Object[] { -2.1, -1.1, -0.1, 0.9, 1.9 });

        }
        [Test]
        public void T068_Min_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Min_2.ds");
            thisTest.Verify("t1", new Object[] { -0.5, 0.5, 1.5 });
            thisTest.Verify("t2", -3.0);
            thisTest.Verify("t3", -1.5);
            thisTest.Verify("t4", -1.5);
            thisTest.Verify("t5", new Object[] { -1.5, -0.5, 0.5 });
            thisTest.Verify("t6", new Object[] { -1.5, -0.5, 0.5 });
            thisTest.Verify("t7", new Object[] { -1.5, -0.5, 0.5 });
            thisTest.Verify("t8", new Object[] { -1.5, -0.5, 0.5 });

        }
        [Test]
        public void T068_Pow_1()
        {

            String code =
@"
import (""Math.dll"");

a1 = Math.Pow( 2, 2 ) ;
b1 = Math.Pow( -2, -2 ) ;
c1 = Math.Pow( 2.5, -2 ) ;
d1 = Math.Pow( -2.5, -2.5 ) ;
t1 = Math.Pow( -2, 0 ) ;
t2 = Math.Pow( 0, -1 ) ;
t4 = Math.Pow( -2..2, -2..2 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 4.0);
            thisTest.Verify("b1", 0.25);
            thisTest.Verify("c1", 0.160);
            //thisTest.Verify("d1", Double.NaN);
            thisTest.Verify("t1", 1.0);
            //thisTest.Verify("t2", Double.PositiveInfinity);
            thisTest.Verify("t4", new Object[] { 0.25, -1.0, 1.0, 1.0, 4.0 });

        }
        [Test]
        public void T068_Pow_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Pow_2.ds");
            thisTest.Verify("t1", new Object[] { 2.0, 2.0, 2.0 });
            thisTest.Verify("t2", 8.0);
            thisTest.Verify("t3", 4.0);
            thisTest.Verify("t4", 4.0);
            thisTest.Verify("t5", new Object[] { 1.0, 1.0, 1.0 });
            thisTest.Verify("t6", new Object[] { 1.0, 1.0, 1.0 });
            thisTest.Verify("t7", new Object[] { 1.0, 1.0, 1.0 });
            thisTest.Verify("t8", new Object[] { 1.0, 1.0, 1.0 });

        }
        [Test]
        public void T068_Round_1()
        {

            String code =
@"
import (""Math.dll"");

a1 = Math.Round( 2.04, 1 ) ;
b1 = Math.Round( -2, 2 ) ;
c1 = Math.Round( -2.578, 2 ) ;
d1 = Math.Round( 2, 2 ) ;
t1 = Math.Round( -2, 0 ) ;
t2 = Math.Round( 0.1, 2 ) ;
t4 = Math.Round( {-2.56,0.1,2.444} , {1, 2, 3} ) ;
t5 = Math.Round( 2.456, -2 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 2.0);
            thisTest.Verify("b1", -2.0);
            thisTest.Verify("c1", -2.58);
            thisTest.Verify("d1", 2.0);
            thisTest.Verify("t1", -2.0);
            thisTest.Verify("t2", 0.1);
            thisTest.Verify("t4", new Object[] { -2.6, 0.1, 2.444 });
            thisTest.Verify("t5", null);
        }
        [Test]
        public void T068_Round_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Round_2.ds");
            thisTest.Verify("t1", new Object[] { 0.0, -1.0, 2.0 });
            thisTest.Verify("t2", 4.0);
            thisTest.Verify("t3", -2.0);
            thisTest.Verify("t4", -2.0);
            thisTest.Verify("t5", new Object[] { -1.0, -2.0, 1.0 });
            thisTest.Verify("t6", new Object[] { -1.0, -2.0, 1.0 });
            thisTest.Verify("t7", new Object[] { -1.0, -2.0, 1.0 });
            thisTest.Verify("t8", new Object[] { -1.0, -2.0, 1.0 });

        }
        [Test]
        public void T068_Round_3()
        {

            String code =
@"
import (""Math.dll"");

a1 = Math.Round( 3.45, 1, MidpointRounding.ToEven) ;
a2 = Math.Round( 3.45, 1, MidpointRounding.AwayFromZero);
a3 = Math.Round( 3.45, MidpointRounding.ToEven) ;
a4 = Math.Round( 3.45, MidpointRounding.AwayFromZero);
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 3.4);
            thisTest.Verify("a2", 3.5);
            thisTest.Verify("a3", 3.4);
            thisTest.Verify("a4", 3.5);

        }

        [Test]
        public void T068_Sign_1()
        {

            String code =
@"
 import (""Math.dll"");
 a = -1.5;
 b = { -3, 0, -4.5 };

 a1 = Math.Sign( a ) ;
 b1 = Math.Sign( b ) ;
 c1 = Math.Sign( -2..3 ) ;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", -1);
            thisTest.Verify("b1", new object[] { -1, 0, -1 });
            thisTest.Verify("c1", new object[] { -1, -1, 0, 1, 1, 1 });

        }

        [Test]
        public void T068_Sign_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Sign_2.ds");
            thisTest.Verify("t1", 0);
            thisTest.Verify("t2", -1);
            thisTest.Verify("t3", -1);
            thisTest.Verify("t4", -1);
            thisTest.Verify("t5", -1);
            thisTest.Verify("t6", 1);
            thisTest.Verify("t7", -1);
        }
        [Test]
        public void T068_Sinh_1()
        {

            String code =
@"
 import (""Math.dll"");
 a = -4.5;
 b = { -1.5, 0, 13.5 };

 a1 = Math.Sinh( a ) ;
 b1 = Math.Sinh( b ) ;

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", -45.003011151991785);
            thisTest.Verify("b1", new object[] { -2.1292794550948173, 0.0, 364708.18492316519 });
        }

        [Test]
        public void T068_Truncate_1()
        {

            String code =
@"
 import (""Math.dll"");
 a = -4.567;
 b = { -1.5, 0.003, 13.098, 8 };

 a1 = Math.Truncate( a ) ;
 b1 = Math.Truncate( b ) ;

";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", -4.0);
            thisTest.Verify("b1", new object[] { -1.0, 0.0, 13.0, 8.0 });
        }

        [Test]
        public void T068_Rand_1()
        {

            String code =
@"
import (""Math.dll"");
b1 = 1 > 0 ? Math.Rand() : 10 ;
b2 = 1 > 0 ? Math.Rand(1..2, 2..3) : 10 ;
test1 = 0;
test2 = 0;
c1 = 1;
c2 = 2;
[Imperative]
{
    if( b1>=0 && b1<=1 )
    {
        test1 = 1;
    }
    for ( i in b2 )
    {
        if( i>=c1 && i<=c2 )
        {
            test2 = test2 + 1;
        }
        c1 = c1+1;
        c2 = c2 + 1;
   }
}
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test1", 1);
            thisTest.Verify("test2", 2);
        }

        [Test]
        public void T068_Factorial_1()
        {
            String code =
@"
import (""Math.dll"");
t1 = Math.Factorial(0.9);
t2 = Math.Factorial(1.9);
t3 = Math.Factorial(1.5);
t4 = Math.Factorial(-1.5);
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 2);
            thisTest.Verify("t3", 2);
            thisTest.Verify("t4", -1);

        }

        [Test]
        public void T068_Factorial_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T068_Factorial_2.ds");
            thisTest.Verify("t1", 0);
            thisTest.Verify("t2", 120);
            thisTest.Verify("t3", -1);
            thisTest.Verify("t4", -1);
            thisTest.Verify("t5", -1);
            thisTest.Verify("t6", 24);
            thisTest.Verify("t7", 120);
            thisTest.Verify("t8", new Object[] { -1, 1, 120 });
        }

        [Test]
        public void T069_Defect_1467556_Sort_Over_Derived_Classes()
        {

            String code =
@"
class Test
{
    X;
    constructor(a)
    {
        X = a;
    }
}
class MyTest extends Test
{
    Y;
    constructor(a,b)
    {
        X = a;
        Y = b;
    }
}
def sorter(p1:Test,p2:Test) 
{
    return = (p1.X < p2.X) ? -1 : ((p1.X > p2.X) ? 1 : 0);
}
t1 = Test(1);
t2 = MyTest(2,2);
a = {t1,t2};
b = Sort(sorter,a).X; 
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b", new object[] { 1, 2 });
        }
        [Test]
        public void T069_Defect_1467556_Sort_Over_Derived_Classes_2()
        {

            String code =
@"
class Test
{
    X;
    constructor(a)
    {
        X = a;
    }
}
class MyTest extends Test
{
    Y;
    constructor(a,b)
    {
        X = a;
        Y = b;
    }
}
def sorter(p1:Test,p2:Test) 
{
    return = p1.X<=p2.X?true:false;
}
t1 = Test(1);
t2 = MyTest(2,2);
a = {t1,t2};
b = Sort(sorter,a).X; 
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyRuntimeWarningCount(2);

            // This sorter function is invalid. Meaningless to verify the 
            // garbage result because it is totally undefined. 
            // thisTest.Verify("b", null);
        }

        [Test]
        public void T069_Defect_1467556_Sort_Over_Derived_Classes_3()
        {

            String code =
@"
class Test
{
    X;
    constructor(a)
    {
        X = a;
    }
}
class MyTest extends Test
{
    Y;
    constructor(a,b)
    {
        X = a;
        Y = b;
    }
}
def sorter(p1:Test,p2:MyTest) 
{
    return = p1.X<=p2.X?1:0;
}
t1 = Test(1);
t2 = MyTest(2,2);
a = {t1,t2};
b = Sort(sorter,a).X; 
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // thisTest.VerifyRuntimeWarningCount(3);
            // This sorter function is invalid. Meaningless to verify the 
            // garbage result because it is totally undefined.
            // thisTest.Verify("b", null);
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly()
        {

            String code =
@"
a = {3,1,2};
def sorterFunction(a:double, b:double)
{
    return = 0;
}

b = Sort(sorterFunction, a);  
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 2, 1, 3 });
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_2()
        {

            String code =
@"
a = {3,1,2};
def sorterFunction(a:double, b:double)
{
    return = 1;
}

b = Sort(sorterFunction, a);  
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_5()
        {

            String code =
@"
a = {3,1,2};
def sorterFunction(a:double, b:double)
{
    return = -1;
}

b = Sort(sorterFunction, a);  
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // thisTest.VerifyRuntimeWarningCount(1);
            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", null);
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_3()
        {

            String code =
@"
a = {3,1,2};
def sorterFunction(a:double, b:double)
{
    return = 35;
}

b = Sort(sorterFunction, a);  
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_4()
        {

            String code =
@"
a = {3,1,2};
def sorterFunction(a:double, b:double)
{
    return = 1.5;
}

b = Sort(sorterFunction, a);  
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            
            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        public void T071_Defect_ExD_10016_Rand()
        {

            String code =
@"
a = {3,1,2};
def sorterFunction(a:double, b:double)
{
    return = 1.5;
}

b = Sort(sorterFunction, a);  
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // The test doesn't make sense. 
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        public void T072_defect_1467577()
        {

            String code =
@"
index= IndexOf(1..10.11 , {1,2});
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("index", new Object[] { 1, 2 });
        }
        [Test]
        public void T072_defect_1467577_2()
        {

            String code =
@"
def testRepl(val : var[], index:int)
{


    return = IndexOf(val,index);
}

z = testRepl(1..5, 1..2);

z1 = IndexOf(1..5, { 1, 2 });
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", new Object[] { 0,1});
            thisTest.Verify("z1", new Object[] { 0, 1 });
        }
        [Test]
        public void T073_Defect_ImportFromCSV_1467579()
        {
            String code =
@"a = ""..\..\..\Scripts\TD\testCSV\trailing_comma.csv"";
b = ImportFromCSV(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 10, 40 }, new object[] { 20.0, 50 }, new object[] { 30.0, 60.00 } }

);

        }
        [Test]
        public void T074_Defect_1467750()
        {
            String code =
@"
class A
{
}
x = A.A();
x1 = Flatten(a) ; 
x2 = Flatten(3) ;
x3 = Flatten(3.5) ;
x4 = Flatten(true) ;
x5 = Flatten(x) ;
x6 = Flatten(null) ;
x7 = Flatten({}) ;
x8 = Flatten({null}) ;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", null );
            thisTest.Verify("x2", null);
            thisTest.Verify("x3", null);
            thisTest.Verify("x4", null);
            thisTest.Verify("x5", null);
            thisTest.Verify("x6", null);
            thisTest.Verify("x8", new Object[] { null });

        }
        [Test]
        public void T074_Defect_1467750_2()
        {
            String code =
@"
class A
{
}
test = 
[Imperative]
{
    x = A.A();
    x1 = Flatten(a) ; 
    x2 = Flatten(3) ;
    x3 = Flatten(3.5) ;
    x4 = Flatten(true) ;
    x5 = Flatten(x) ;
    x6 = Flatten(null) ;
    x7 = Flatten({}) ;
    x8 = Flatten({null}) ;
    return = { x1, x2, x3, x4, x5, x6, x8 };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { null, null, null, null, null, null, new Object[] { null} });            

        }
        [Test]
        public void T074_Defect_1467750_3()
        {
            String code =
@"
class A
{
}
test = foo();

def foo ()
{
    return = [Imperative]
    {
        x = A.A();
        x1 = Flatten(a) ; 
        x2 = Flatten(3) ;
        x3 = Flatten(3.5) ;
        x4 = Flatten(true) ;
        x5 = Flatten(x) ;
        x6 = Flatten(null) ;
        x7 = Flatten({}) ;
        x8 = Flatten({null}) ;
        return = { x1, x2, x3, x4, x5, x6, x8 };
    }
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { null, null, null, null, null, null, new Object[] { null } });

        }
        [Test]
        public void T074_Defect_1467750_4()
        {
            String code =
@"
class A
{
}
test = B.foo();
class B
{
    static def foo ()
    {
        return = [Imperative]
        {
            x = A.A();
            x1 = Flatten(a) ; 
            x2 = Flatten(3) ;
            x3 = Flatten(3.5) ;
            x4 = Flatten(true) ;
            x5 = Flatten(x) ;
            x6 = Flatten(null) ;
            x7 = Flatten({}) ;
            x8 = Flatten({null}) ;
            return = { x1, x2, x3, x4, x5, x6, x8 };
        }
    }
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { null, null, null, null, null, null, new Object[] { null } });

        }
        [Test]
        public void T074_Defect_1467750_5()
        {
            String code =
@"
class A
{
}
test = B.foo();
class B
{
    static def foo ()
    {
        return = [Imperative]
        {
            if ( 1 )
            {
                x = A.A();
                x1 = Flatten(a) ; 
                x2 = Flatten(3) ;
                x3 = Flatten(3.5) ;
                x4 = Flatten(true) ;
                x5 = Flatten(x) ;
                x6 = Flatten(null) ;
                x7 = Flatten({}) ;
                x8 = Flatten({null}) ;
                return = { x1, x2, x3, x4, x5, x6, x8 };
           }
           else return = 1;
        }
    }
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { null, null, null, null, null, null, new Object[] { null } });

        }
        [Test]
        public void T074_Defect_1467750_6()
        {
            String code =
@"
class A
{
}
test = { A.A() => x;
        Flatten(a) => x1; 
        Flatten(3) => x2;
        Flatten(3.5) => x3;
        Flatten(true) => x4;
        Flatten(x) => x5;
        Flatten(null) => x6;
        Flatten({}) => x7;
        Flatten({ null }) => x8;
}
test = { x1, x2, x3, x4, x5, x6, x8 };
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { null, null, null, null, null, null, new Object[] { null } });           

        }  
        [Test]
        public void T074_Defect_1467301()
        {
            String code =
@"
x1 = Average(a) ;// returns null, also throws runtime error ? 
x2 = Average(a) ;// returns -1
x4 = Average(null) ;// returns -1
x5 = Average({}) ;// returns 0.0
x6 = Average({null}) ;// returns 0.0
";
            string errmsg = "DNL-1467301 rev 3778 : Builtin method 'Average' should return null for all negative case";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(0);
            thisTest.Verify("x1", null);
            thisTest.Verify("x2", null);
            thisTest.Verify("x4", null);
            thisTest.Verify("x5", null);
            thisTest.Verify("x6", null);
        }

      
        [Test]
        public void T075_Defect_1467323()
        {
            String code =
@"
class A
{
}
x = A.A();
x1 = Count(a) ; 
x2 = Count(3) ;
x3 = Count(3.5) ;
x4 = Count(true) ;
x5 = Count(x) ;
x6 = Count(null) ;
x7 = Count({}) ;
x8 = Count({null}) ;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1 );
            thisTest.Verify("x2", 1);
            thisTest.Verify("x3", 1);
            thisTest.Verify("x4", 1);
            thisTest.Verify("x5", 1);
            thisTest.Verify("x6", 1);
            thisTest.Verify("x7", 0);
            thisTest.Verify("x8", 1);

        }
        [Test]
        public void T075_Defect_1467323_2()
        {
            String code =
@"
class A
{
}
test = 
[Imperative]
{
    x = A.A();
    x1 = Count(a) ; 
    x2 = Count(3) ;
    x3 = Count(3.5) ;
    x4 = Count(true) ;
    x5 = Count(x) ;
    x6 = Count(null) ;
    x7 = Count({}) ;
    x8 = Count({null}) ;
    return = { x1, x2, x3, x4, x5, x6, x7, x8 };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 1, 0, 1 });            

        }
        [Test]
        public void T075_Defect_1467323_3()
        {
            String code =
@"
class A
{
}
test = foo();

def foo ()
{
    return = [Imperative]
    {
        x = A.A();
        x1 = Count(a) ; 
        x2 = Count(3) ;
        x3 = Count(3.5) ;
        x4 = Count(true) ;
        x5 = Count(x) ;
        x6 = Count(null) ;
        x7 = Count({}) ;
        x8 = Count({null}) ;  
        return = { x1, x2, x3, x4, x5, x6, x7, x8 }; 
    }
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 1, 0, 1 });

        }
        [Test]
        public void T075_Defect_1467323_4()
        {
            String code =
@"
class A
{
}
test = B.foo();
class B
{
    static def foo ()
    {
        return = [Imperative]
        {
            x = A.A();
            x1 = Count(a) ; 
            x2 = Count(3) ;
            x3 = Count(3.5) ;
            x4 = Count(true) ;
            x5 = Count(x) ;
            x6 = Count(null) ;
            x7 = Count({}) ;
            x8 = Count({null}) ;
            return = { x1, x2, x3, x4, x5, x6, x7, x8 };
        }
    }
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 1, 0, 1 });

        }
        [Test]
        public void T075_Defect_1467323_5()
        {
            String code =
@"
class A
{
}
test = B.foo();
class B
{
    static def foo ()
    {
        return = [Imperative]
        {
            if ( 1 )
            {
                x = A.A();
                x1 = Count(a) ; 
                x2 = Count(3) ;
                x3 = Count(3.5) ;
                x4 = Count(true) ;
                x5 = Count(x) ;
                x6 = Count(null) ;
                x7 = Count({}) ;
                x8 = Count({null}) ;
                return = { x1, x2, x3, x4, x5, x6, x7, x8 };                
           }
           else return = 1;
        }
    }
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 1, 0, 1 });

        }
        [Test]
        public void T075_Defect_1467323_6()
        {
            String code =
@"
class A
{
}
test = { A.A() => x;
        Count(a) => x1; 
        Count(3) => x2;
        Count(3.5) => x3;
        Count(true) => x4;
        Count(x) => x5;
        Count(null) => x6;
        Count({}) => x7;
        Count({ null }) => x8;
}

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 1);
            thisTest.Verify("x3", 1);
            thisTest.Verify("x4", 1);
            thisTest.Verify("x5", 1);
            thisTest.Verify("x6", 1);
            thisTest.Verify("x7", 0);
            thisTest.Verify("x8", 1);

        }
        [Test]
        //Test "LoadCSV"
        public void Defect_ImportFromCSV_1467622()
        {
            String code =
            @"a = ""../../../Scripts/TD/testCSV/nonuniform.csv"";
            b = ImportFromCSV(a);
            ";
                        ExecutionMirror mirror = thisTest.RunScriptSource(code);
                        thisTest.Verify("b", new object[] { new object[] { 1.0, 2, 3, 4, 5, 6 }, new object[] { 2, 3.0, 4, 5, 6, 7, 8 } }

            );

        }
        [Test]
        //Test "LoadCSV"
        public void Defect_ImportFromCSV_1467622_2()
        {
            String code =
            @"a = ""../../../Scripts/TD/testCSV/trailing_comma_nonuniform.csv"";
            b = ImportFromCSV(a);
            ";
                        ExecutionMirror mirror = thisTest.RunScriptSource(code);
                        thisTest.Verify("b", new object[] { new object[] { 10, 20, 30, null }, new object[] { 40, 50, 60, 40, null } }

            );

        }

    }
    


}

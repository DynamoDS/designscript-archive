using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    internal class CollectionAssignment
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string filePath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\CollectionAssignment\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T01_Simple_1D_Collection_Assignment()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T01_Simple_1D_Collection_Assignment.ds");


            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 2, 2 };

            thisTest.Verify("c", -2, 0);
            thisTest.Verify("d", expectedResult2, 0);
            thisTest.Verify("e", expectedResult3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_Collection_Assignment_Associative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T02_Collection_Assignment_Associative.ds");

            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 2, 2 };

            thisTest.Verify("c", -2, 0);
            thisTest.Verify("d", expectedResult2, 0);
            thisTest.Verify("e", expectedResult3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_Collection_Assignment_Nested_Block()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T03_Collection_Assignment_Nested_Block.ds");

            object[] expectedResult2 = { 1, 2, 3 };

            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", 6);
            thisTest.Verify("e", expectedResult2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_Collection_Assignment_Using_Indexed_Values()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T04_Collection_Assignment_Using_Indexed_Values.ds");

            object[] expectedResult = { 1, 2, 3 };
            object[] expectedResult2 = { 1, 2, 4 };

            thisTest.Verify("c", expectedResult, 0);
            thisTest.Verify("e", expectedResult2, 0);
            thisTest.Verify("d", 4, 0);
 
 
        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_Collection_Assignment_Using_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T05_Collection_Assignment_Using_Class.ds");

            object[] expectedResult2 = { 1, 2, 3 };

            thisTest.Verify("d", expectedResult2, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_Collection_Assignment_Using_Class_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T06_Collection_Assignment_Using_Class_2.ds");

            object[] expectedResult2 = { 4, 2, 3 };

            thisTest.Verify("d", expectedResult2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_Collection_Assignment_In_Function_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T07_Collection_Assignment_In_Function_Scope.ds");

            object[] expectedResult2 = { 1, 2, 3 };

            thisTest.Verify("a", expectedResult2, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T08_Collection_Assignment_In_Function_Scope_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T08_Collection_Assignment_In_Function_Scope_2.ds");

            object[] expectedResult2 = { 1, 2, 3 };
            object[] expectedResult = { 4, 5, 6 };

            thisTest.Verify("a", expectedResult2);
            thisTest.Verify("b", expectedResult);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T09_2D_Collection_Assignment_In_Class_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T09_2D_Collection_Assignment_In_Class_Scope.ds");

            object[] expectedResult2 = { 3, 4 };
            object[] expectedResult = { 1, 2 };

            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", expectedResult);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_2D_Collection_Assignment_In_Function_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T10_2D_Collection_Assignment_In_Function_Scope.ds");
            object[] expectedResultc = { new object[] { 1 }, 2, 3 };
            object[] expectedResultd = {new object[] {new object[] {1}, 2, 3},
                     
            new object[] {new object[] {1}, 5, 6}
        };


            thisTest.Verify("c", expectedResultc, 0);
            thisTest.Verify("d", expectedResultd, 0);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_2D_Collection_Assignment_Heterogeneous()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T11_2D_Collection_Assignment_Heterogeneous.ds");

            object[] expectedResult2 = { 4 };


            thisTest.Verify("b", expectedResult2, 0);
            thisTest.Verify("c", 3, 0);
            thisTest.Verify("d", 8, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T12_Collection_Assignment_Block_Return_Statement()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T12_Collection_Assignment_Block_Return_Statement.ds");
            
            thisTest.Verify("c1", 2, 0);
            thisTest.Verify("c2", 4, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_2D_Collection_Assignment_Block_Return_Statement()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T13_2D_Collection_Assignment_Block_Return_Statement.ds");

            object[] expectedResult2 = { 0, 2, 3 };

            thisTest.Verify("c1", expectedResult2, 0);
            thisTest.Verify("c2", 6, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_2D_Collection_Assignment_Using_For_Loop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T14_2D_Collection_Assignment_Using_For_Loop.ds");

            thisTest.Verify("p1", 4, 0);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T15_2D_Collection_Assignment_Using_While_Loop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T15_2D_Collection_Assignment_Using_While_Loop.ds");

            thisTest.Verify("p1", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_Assigning_Class_Collection_Property()
        {
            string error = "1467321 rev 3878: class property specified as an empty array with no rank is becoming null when assigned a collection to it ";
            thisTest.VerifyRunScriptFile(filePath, "T16_Assigning_Class_Collection_Property.ds", error);

            // Assert.Fail("1463456 - Sprint 20 : Rev 2105 : Collection assignment is not always by reference ( when the collection is in class or function ) ");

            thisTest.Verify("t", 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T17_Assigning_Collection_And_Updating()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T17_Assigning_Collection_And_Updating.ds");

            thisTest.Verify("t", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_Assigning_Collection_In_Function_And_Updating()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T18_Assigning_Collection_In_Function_And_Updating.ds");

            // Assert.Fail("1463456 - Sprint 20 : Rev 2105 : Collection assignment is not always by reference ( when the collection is in class or function ) ");

            thisTest.Verify("z", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_Assigning_Collection_In_Function_And_Updating()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T19_Assigning_Collection_In_Function_And_Updating.ds");

            // Assert.Fail("1463456 - Sprint 20 : Rev 2105 : Collection assignment is not always by reference ( when the collection is in class or function ) ");

            thisTest.Verify("z", 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T20_Defect_1458567()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T20_Defect_1458567.ds");
            Object v = null;
            
            thisTest.Verify("a", 1);
            thisTest.Verify("b", v);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T20_Defect_1458567_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T20_Defect_1458567_2.ds");
            Object v = null;

            thisTest.Verify("x1", v);
            thisTest.Verify("x2", v);
            thisTest.Verify("x3", 1.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_Defect_1460891()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T21_Defect_1460891.ds");
            Object[] v1 = new Object[] { 1, 3, 5 };
            Object[] v2 = new Object[] { 2, 4, 6 };

            thisTest.Verify("a", v1);
            thisTest.Verify("c", v2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_Create_Multi_Dim_Dynamic_Array()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T22_Create_Multi_Dim_Dynamic_Array.ds");
            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };
            
            thisTest.Verify("test", v1);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_Defect_1460891_2()
        {
            // Assert.Fail("1464429 - Sprint 21 : rev 2205 : Dynamic array cannot be used in associative scope ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T21_Defect_1460891_2.ds");
            Object[] v1 = new Object[] {new object[]{ 0, 1 }, new object[] {0, 1}};
           
            thisTest.Verify("b", v1);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T23_Create_Dynamic_Array_Using_Replication_In_Imperative_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T23_Create_Dynamic_Array_Using_Replication_In_Imperative_Scope.ds");
           
            Object[] v1 = new Object[] { 0, 1 };

            thisTest.Verify("test", v1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Imperative_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Imperative_Scope.ds");

            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 }  };

            thisTest.Verify("t", v1);

        }
        
        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Imperative_Function_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Imperative_Function_Scope.ds");

            Object[] v1 = new Object[] { -1, -2  };

            thisTest.Verify("x", v1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Accessing_Out_Of_Bound_Index()
        {
            // Assert.Fail("1465614 - Sprint 21: rev 2335 : Accessing out-of-bound index os dynamic array is throwing unexpected error ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Accessing_Out_Of_Bound_Index.ds");

            // Object v1 = null;
            // thisTest.Verify("test1", 1);
            // thisTest.Verify("test2", v1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Class_Scope()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Class_Scope.ds");

            Object[] v1 = new Object[] { 0, 1, 2, 3, 4 };
            Object[] v2 = new Object[] { 0, -1, -2, -3, -4 };
            
            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Class_Scope_2()
        {
            // Assert.Fail("1465637 - Sprint 22 : rev 2336 : Issue with populating multiple array properties of class using imperative block ");
            string error = "1467321 rev 3878: class property specified as an empty array with no rank is becoming null when assigned a collection to it ";
            thisTest.VerifyRunScriptFile(filePath, "T24_Dynamic_Array_Class_Scope_2.ds", error);

            Object[] v1 = new Object[] { 0, 1, 2, 3, 4 };
            Object[] v2 = new Object[] { 0, -1, -2, -3, -4 };

            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Class_Scope_3()
        {
            // Assert.Fail("1465704 - Sprint 22: rev 2346 : Adding elements to array from inside class methods is throwing System.IndexOutOfRangeException exception ");
            //Assert.Fail("1467194 - Sprint 25 - rev Regressions created by array copy constructions");           
            string errmsg = "";//1467187 - Sprint24: REGRESSION : rev 3177: When a class collection property is updated, the value if not reflected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(filePath, "T24_Dynamic_Array_Class_Scope_3.ds",errmsg);

            Object[] v1 = new Object[] { 4, 3, 2, 1, 0, 100 };
            Object[] v2 = new Object[] { 0, 1, 2, 3, 4, 100 };

            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Inside_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Inside_Function.ds");

            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };

            thisTest.Verify("a", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Inside_Function_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Inside_Function_2.ds");

            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };

            thisTest.Verify("a", v1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Passed_As_Int_Array_To_Class_Method()
        {
            // Assert.Fail("1465802 - Sprint 22: rev 2359 : Dynamic array issue : when a dynamic array is passed as a collection method/function it throws unknown datatype invalid exception");

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Passed_As_Int_Array_To_Class_Method.ds");

            thisTest.Verify("b1", 22);
            thisTest.Verify("b2", 22);
            thisTest.Verify("b3", 22);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Dynamic_Array_Passed_As_Primitive_Array_To_Function()
        {
            // Assert.Fail("1465802 - Sprint 22: rev 2359 : Dynamic array issue : when a dynamic array is passed as a collection method/function it throws unknown datatype invalid exception");

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Passed_As_Primitive_Array_To_Function.ds");

            thisTest.Verify("b1", 2.5);
            thisTest.Verify("b2", 2.5);
            thisTest.Verify("b3", 2.5);

        }
        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Argument_Function_1465802_1()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Argument_Function_1465802_1.ds");

            thisTest.Verify("b1", 1);


        }
        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Argument_Function_1465802_2()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T24_Dynamic_Array_Argument_Function_1465802_2.ds");

            thisTest.Verify("b1", 1);
            

        }
        
     /*   [Test]
        public void T24_Dynamic_Array_Function_1465706()
        {

            ExecutionMirror mirror = thisTest.RunScript(filePath, "T24_Dynamic_Array_Function_1465706.ds");
            Object[] test = new Object[] { 0, 1 };
            thisTest.Verify("test", test);
        

        }*/
      
        

        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_Elements_To_Array()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_Elements_To_Array.ds");

            Object[] v1 = new Object[] { 0, 1, 2, 3 };
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };

            thisTest.Verify("b", v1);
            thisTest.Verify("y", v5);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_Elements_To_Array_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_Elements_To_Array_Function.ds");

            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };

            thisTest.Verify("x", v5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_Elements_To_Array_Class()
        {
            // Assert.Fail("1465704 - Sprint 22: rev 2346 : Adding elements to array from inside class methods is throwing System.IndexOutOfRangeException exception ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_Elements_To_Array_Class.ds");

            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };

            thisTest.Verify("x", v5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_elements_tomemberofclass_1465704()
        {
            

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_tomemberofclass_1465704.ds");

            Object[] a2 = new Object[] { 0, 0 };
            Object[] a3 = new Object[] { 1, 1, 1 };
            Object[] a4 = new Object[] { 2, 2, 2, 2 };
            Object[] a5 = new Object[] { a2, a3, a4 };

            thisTest.Verify("x", a5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_elements_tomemberofclass_1465704_2()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_tomemberofclass_1465704_2.ds");

            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };

            thisTest.Verify("x", v5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_elements_tomemberofclass_1465704_3()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_tomemberofclass_1465704_3.ds");

            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false,new Object []{ 2, 2 }};
            Object[] v5 = new Object[] { v2, v3, v4 };

            thisTest.Verify("x", v5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_elements_tomemberofclass_1465704_4()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_tomemberofclass_1465704_4.ds");

            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4, 1};

            thisTest.Verify("x", new object[] {v2, v3, v4});
            thisTest.Verify("z", v5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_elements_tomemberofclass_1465704_5()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_tomemberofclass_1465704_5.ds");

            Object[] v1 = new Object[] { 0, 0 };
            Object[] v2 = new Object[] { 4, 4 };
            Object[] v3 = new Object[] { v1, v2 };

            thisTest.Verify("x", new object[] {v1});
            thisTest.Verify("z", v3);
        }
        
        
        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_elements_tomemberofclass_1465704_6()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_tomemberofclass_1465704_6.ds");

            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4};

            thisTest.Verify("x", v5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T25_Adding_elements_tomemberofclass_1465704_7()
        {


            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_tomemberofclass_1465704_7.ds");

            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4 };

            thisTest.Verify("x", v5);

        }
        [Test]
        [Category("SmokeTest")]
        public void T25_Adding_elements_MemberClass_imperative_1465704_8()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_elements_MemberClass_imperative_1465704_8.ds");
            Object[] a = new Object[] { 1, 1, 1 , 1, 1, 1};
            thisTest.Verify("a", a);
        }
        [Test]
        [Category("SmokeTest")]
        public void T25_Adding_elements_MemberClass_imperative_1465704_9()
        {
            string error ="1467309 rev 3786 : Warning:Couldn't decide which function to execute... coming from valid code ";
            thisTest.VerifyRunScriptFile(filePath, "T25_Adding_elements_MemberClass_imperative_1465704_9.ds", error);
            Object[] a = new Object[] { 1, 1, 1, 1, 1, 1 };
            thisTest.Verify("a", a);

        }
        [Test]
        [Category("SmokeTest")]
        public void T25_Adding_elements_MemberClass_imperative_1465704_10()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Adding_Elements_ToMemberOfClass_1465704_10.ds");
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { null, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4 };

            thisTest.Verify("x", v5);

        }
        [Test]
        [Category("SmokeTest")]
        public void T25_Class_Assignment_dynamic_imperative_1465637_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T25_Class_Assignment_dynamic_imperative_1465637_1.ds");
            Object[] b1 = new Object[] { 0, -1, -2, -3, -4 };
            Object[] b2 = new Object[] { 0, 0, 0, 0, 0 };
            thisTest.Verify("b1", b1);
            thisTest.Verify("b2", b2);

        }
        
        [Test]
        [Category ("SmokeTest")]
 public void T26_Defct_DNL_1459616()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T26_Defct_DNL_1459616.ds");

            Object[] v1 = new Object[] { 1, 2 };

            thisTest.Verify("a", v1); 
        }

        [Test]
        [Category ("SmokeTest")]
 public void T26_Defct_DNL_1459616_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T26_Defct_DNL_1459616_2.ds");

            Object[] v1 = new Object[] { new Object[]{1, 2}, 2 };

            thisTest.Verify("b", v1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T26_Defct_DNL_1459616_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T26_Defct_DNL_1459616_3.ds");

            Object[] v1 = new Object[] { 2, 2 };

            thisTest.Verify("c", v1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T26_Defct_DNL_1459616_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T26_Defct_DNL_1459616_4.ds");

            Object[] v1 = new Object[] { null, null };
            Object[] v2 = new Object[] { null, null, null };
            Object[] v3 = new Object[] { v1, v2 };

            thisTest.Verify("c", v3);
        }

        [Test]
        [Category("Variable resolution")]
 public void T26_Defct_DNL_1459616_5()//not
        {

            string error = "1465812 - Sprint 22 : rev 2362 : Global variables cannot be accessed from class scope";
            thisTest.VerifyRunScriptFile(filePath, "T26_Defct_DNL_1459616_5.ds",error);

            Object[] v1 = new Object[] { new Object[] { 1 , 2 }, new Object[] { 1 , 2 } };
            Object[] v2 = new Object[] { 0, 0, new Object[] { 0, 1 } };
            Object[] v3 = new Object[] { v1, v2 };

            thisTest.Verify("c", v3);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T27_defect_1464429_DynamicArray()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_defect_1464429_DynamicArray.ds");
            

            Object[] t1 = new Object[] {};
            Object[] t2 = new Object[] { new Object[] {0}, new Object[] { null, 1 } };
            

            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T27_defect_1464429_DynamicArray_inline()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_defect_1464429_DynamicArray_inline.ds");

            Object[] t1 = new Object[] { };
            Object n1 = null;
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 }, new Object[] { n1, n1, 4 } };

            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T27_defect_1464429_DynamicArray_class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_defect_1464429_DynamicArray_class.ds");

            Object[] t1 = new Object[] { };
            Object n1 = null;
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 }, new Object[] { n1, n1, 4 }  };            

            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T27_defect_1464429_DynamicArray_update()
        {
            //Assert.Fail("1467234 - Sprint25: rev 3408 : Adding to collections using negative indices is not working"); 
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_defect_1464429_DynamicArray_update.ds");
            

            Object[] t1 = new Object[] { };
            Object n1 = null;
            Object[] t2 = new Object[] { new Object[] { -2, n1 }, new Object[] { -1 } };  

            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }
        [Test]
        [Category ("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_memberof_class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_defect_1464429_DynamicArray_memberof_class.ds");

            Object[] count = new Object[] { 0,1,2};

            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { 0,1 } , new Object[] { 0,1,2 }};

            thisTest.Verify("count", count);
            thisTest.Verify("t2", t2);
        }
        
        [Test]
        
        [Category ("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_class_inherit()
        {
            
            string errmsg = " 1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2) ";
            thisTest.VerifyRunScriptFile(filePath, "T27_defect_1464429_DynamicArray_class_inherit.ds", errmsg);

            Object[] count = new Object[] { 0,1,2};
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { 0, -1 }, new Object[] { 0, -1, -2 } };

            thisTest.Verify("count", count);
            thisTest.Verify("t2", t2);
        }
        [Test]
        [Category("SmokeTest")]
        public void T27_DynamicArray_Class_1465802_Argument()//not
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Class_1465802_Argument.ds");

            int b = 22;
            
            thisTest.Verify("b1", b);
            thisTest.Verify("b2", b);
            thisTest.Verify("b31", b);
        }
        public void T27_DynamicArray_Class_1465802_Argument_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Class_1465802_Argument_2.ds");
            int b = 1;
            thisTest.Verify("b1", b);
        }
        
        [Test]
        [Category("SmokeTest")]
        public void T27_DynamicArray_Class_1465802_member()//not
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Class_1465802_member.ds");

            int b = 22;
            
            thisTest.Verify("a1", b);
            thisTest.Verify("b1", b);
            thisTest.Verify("c1", b);
        }
        [Test]
        public void T27_DynamicArray_Invalid_Index_1465614_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Invalid_Index_1465614_1.ds");
            Object b = null;
            thisTest.Verify("b", b);
        }
        [Test]
        public void T27_DynamicArray_Invalid_Index_1465614_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Invalid_Index_1465614_2.ds");

            thisTest.Verify("x", 0.0);
            thisTest.Verify("y", 0.0);
            thisTest.Verify("z", 0.0);
        }
        [Test]
        public void T27_DynamicArray_Invalid_Index_1467104()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Invalid_Index_1467104.ds");
            object aa = null;
            thisTest.Verify("aa", aa);
            
        }
        [Test]
        public void T27_DynamicArray_Invalid_Index_1467104_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Invalid_Index_1467104_2.ds");
            object aa= null;
            thisTest.Verify("aa", aa);
            
        }
        [Test]
        public void T27_DynamicArray_Invalid_Index_1467104_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T27_DynamicArray_Invalid_Index_imperative_1467104_3.ds");
            object aa = null;
            object aa1 = null;
            thisTest.Verify("aa", aa);
            thisTest.Verify("aa1", aa1);

        }

       /* [Test]
        [Category ("SmokeTest")]
 public void T27_defect_1464429_DynamicArray_function()
        {
            ExecutionMirror mirror = thisTest.RunScript(filePath, "T27_defect_1464429_DynamicArray_function.ds");

            Object[] t1 = new Object[] { -2, -1 };
            
            thisTest.Verify("t2", t2);
        }*/
        
        [Test]
        [Category ("SmokeTest")]
 public void T28_defect_1465706__DynamicArray_Imperative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T28_defect_1465706__DynamicArray_Imperative.ds");

            Object[] t = new Object[] { 0, 1};

            thisTest.Verify("test", t);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T28_defect_1465706__DynamicArray_Imperative_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T28_defect_1465706__DynamicArray_Imperative_2.ds");

            Object[] a = new Object[] {3,4,5};
            Object[] r = new Object[] {3,4,5};
            thisTest.Verify("a", a);
            thisTest.Verify("r", r);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T29_DynamicArray_Using_Out_Of_Bound_Indices()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T29_DynamicArray_Using_Out_Of_Bound_Indices.ds");

            Object[] v1 = new Object[] {0,1,null};
            Object v2 = null;
            Object[] v3 = new Object[] {0,1};
            Object[] v4 = new Object[] {1, null};

            thisTest.Verify("a", v2);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", v2);
            thisTest.Verify("d", v3);
            thisTest.Verify("t", v1);
            thisTest.Verify("z", v4);
            
        }
        [Test]
        [Category ("SmokeTest")]
 public void T40_Index_usingFunction_1467064()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T40_Index_usingFunction_1467064.ds");

            Object[] x = new Object[] { 3, 2};
            Object[] y = new Object[] { 3, 2 };
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T40_Index_usingFunction_class_1467064_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T40_Index_usingFunction_class_1467064_2.ds");

            Object[] x = new Object[] { 3, 2};
            Object[] a = new Object[] { 3, 2 };
            thisTest.Verify("x", x);
            thisTest.Verify("a", a);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T40_Index_byFunction_class_imperative_1467064_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T40_Index_byFunction_class_imperative_1467064_3.ds");

            Object[] x = new Object[] { 1, 2,3 };
            Object[] y = new Object[] { 1, 2 ,3};
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T40_Index_byFunction_argument_1467064_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T40_Index_byFunction_argument_1467064_4.ds");

            Object[] x = new Object[] { 1,3 };
            Object[] y = new Object[] { 1,3};
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }
       /* [Test]
        public void T40_Index_DynamicArray_1467064_5()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(filePath, "T40_Index_DynamicArray_byarray_1467064_5.ds");
            });
        }
        [Test]
        public void T40_Index_DynamicArray_1467064_6()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(filePath, "T40_Index_DynamicArray_byarray_1467064_6.ds");
            });
        }*/

        [Test]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Properties_From_Array_Elements()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T41_Accessing_Non_Existent_Properties_From_Array_Elements.ds");
            // Assert.Fail("1467083 - Sprint23 : rev 2681 : error expected when accessing nonexistent properties of array elements!");

            Object v1 = null;
            thisTest.Verify("d", v1);
           
        }
        [Test]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T41_Accessing_Non_Existent_Property_FromArray_1467083.ds");

            Object a = null;

            TestFrameWork.Verify(mirror, "c0", 0);
            TestFrameWork.Verify(mirror, "d", a);

        }
        [Test]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T41_Accessing_Non_Existent_Property_FromArray_1467083_2.ds");

            Object a = null;

            TestFrameWork.Verify(mirror, "e", a);
            TestFrameWork.Verify(mirror, "e1", a);

        }
        [Test]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T41_Accessing_Non_Existent_Property_FromArray_1467083_3.ds");

            Object a = null;

            TestFrameWork.Verify(mirror, "e", a);
            TestFrameWork.Verify(mirror, "e1", a);
            TestFrameWork.Verify(mirror, "f", 0);
            TestFrameWork.Verify(mirror, "f1", a);
        }
        [Test]
        [Category("Variable resolution")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082()
        {
            //Assert.Fail("1467094 - Sprint 24 : Rev 2748 : in some cases if try to access nonexisting item in an array it does not return null ) ");
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T61_Accessing_Non_Existent_Array_Properties_1467082.ds");

            //Verification 
            thisTest.Verify("t1", 0);
            thisTest.Verify("t2", 1);
            thisTest.Verify("t3", 0);
      
            Object a = null;
            thisTest.Verify("t4", a);

        }
        [Test]
        [Category("SmokeTest")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082_2()
        {
            // Assert.Fail("");
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T61_Accessing_Non_Existent_Array_Properties_1467082_2.ds");

            //Verification 

            object a = null;
            thisTest.Verify("q", a);

        }
        [Test]
        [Category("SmokeTest")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082_3()
        {
            // Assert.Fail("");
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T61_Accessing_Non_Existent_Array_Properties_1467082_3.ds");

            //Verification 

            object a = null;
            thisTest.Verify("p", a);
            thisTest.Verify("q", a);
            thisTest.Verify("r", a);
            thisTest.Verify("s", a);

        }
        [Test]
        [Category("SmokeTest")]
        public void T61_Assign_Non_Existent_Array_Properties_1467082_4()
        {
            
   
            //thisTest.VerifyWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);    
            string errmsg = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(filePath, "T61_Assign_Non_Existent_Array_Properties_1467082_4.ds",errmsg);
            
            //Verification 

            object a = null;
            thisTest.Verify("p", new Object[] { new Object[] { 5 } });
            thisTest.Verify("q", new Object[] { new Object[] { 0 , 1 } });
            thisTest.Verify("r", a);
            thisTest.Verify("s", new Object[] { 5 });


        }

        [Test]
        [Category("SmokeTest")]
        public void T61_Assign_Non_Existent_Array_Properties_1467094()
        {
            // Assert.Fail("");

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T61_Assign_Non_Existent_Array_Properties_1467094.ds");
            //thisTest.VerifyWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);    

            //Verification 

            object a = null;
            thisTest.Verify("t4", a);
            


        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T62_Create_Dynamic_Array_OnTheFly.ds");
            thisTest.Verify("i", 7);

        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_AsFunctionArgument()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T62_Create_Dynamic_Array_OnTheFly_AsFunctionArgument.ds");

            object a = null;
            thisTest.Verify("c", a);
            thisTest.Verify("b", 5);
            

        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Change_Avariable_To_Dynamic_Array_OnTheFly()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T62_Change_Avariable_To_Dynamic_Array_OnTheFly.ds");

            thisTest.Verify("c", 1);
            thisTest.Verify("b", 5);
            

        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Create_MultiDimension_Dynamic_Array_OnTheFly()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T63_Create_MultiDimension_Dynamic_Array_OnTheFly.ds");

            thisTest.Verify("c", 1);
            thisTest.Verify("b",  new Object[][]{new Object[]{1,5},null,new Object[]{null,null,null,6}});


        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_1467066()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T63_Dynamic_array_onthefly_1467066.ds");

            thisTest.Verify("z", new Object[]{0,1,2,3,4,5,6,7});
            thisTest.Verify("b", new Object[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            


        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_aliastoanother()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T63_Dynamic_array_onthefly_aliastoanother.ds");

            thisTest.Verify("a", new Object[]{5,null,3});
            thisTest.Verify("b", new Object[] { 5, null, -5 });
            


        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_inaClass()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T62_Create_Dynamic_Array_OnTheFly_inaClass.ds");

            thisTest.Verify("d",5);
            thisTest.Verify("c", 10);
  

        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_passargument()
        {
            // Assert.Fail("1467139-Sprint 24 - Rev 2958 - an array created dynamically on the fly and passed as an arguemnt to method it gets garbage collected ");
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T62_Create_Dynamic_Array_OnTheFly_passargument.ds");

            thisTest.Verify("d",new Object[]{5});
            thisTest.Verify("c", 1);
  

        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_inaClass_methodoverload()
        {
            // Assert.Fail("1467139-Sprint 24 - Rev 2958 - an array created dynamically on the fly and passed as an arguemnt to method it gets garbage collected ");
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T62_Create_Dynamic_Array_OnTheFly_inaClass_methodoverload.ds");

            thisTest.Verify("d",new Object[]{5});
            thisTest.Verify("c", 1);
  

        }
        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_passargument_function()
        {
           
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T62_Create_Dynamic_Array_OnTheFly_passargument_function.ds");

            thisTest.Verify("d",new Object[]{5});
            thisTest.Verify("a", 1);

        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_argument_class__1467139()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T63_Dynamic_array_onthefly_argument_class__1467139.ds");
            thisTest.Verify("d",new Object[]{5});

        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_function_1467139()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T63_Dynamic_array_onthefly_function_1467139.ds");
            thisTest.Verify("t",new Object[]{5});

        }
        [Test]
        public void T63_Dynamic_array_onthefly_update()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T63_Dynamic_array_onthefly_update.ds");

        }

      [Test]
        public void T64_Modify_itemInAnArray_1467093()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T64_Modify_itemInAnArray_1467093.ds");
            thisTest.Verify("a", new Object[] { 1, new Object[] { 1, 2, 3 }, 3 });
        }

      [Test]
      public void T64_Modify_itemInAnArray_1467093_2()
      {
          ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T64_Modify_itemInAnArray_1467093_2.ds");
          thisTest.Verify("a", new Object[] {new Object[] { } });
          thisTest.Verify("b", new Object[] { } );
          thisTest.Verify("c", new Object[] { new Object[] { } });
      }
        [Test]
        public void T65_Array_Alias_ByVal_1467165()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T65_Array_Alias_ByVal_1467165.ds");
            thisTest.Verify("a",new Object[]{9,1,2,3});
            thisTest.Verify("b",new Object[] {10,1,2,3});


        }
        [Test]
        public void T65_Array_Alias_ByVal_1467165_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T65_Array_Alias_ByVal_1467165_2.ds");
            thisTest.Verify("c",200);
            thisTest.Verify("d",200);


        }
        [Test]
        public void T65_Array_Alias_ByVal_1467165_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T65_Array_Alias_ByVal_1467165_3.ds");
            thisTest.Verify("a", new Object []{9,1,2,3});
            thisTest.Verify("b", new Object []{false,1,2,3});
            
        }
        [Test]
        [Category("Design Issue")]
        public void T65_Array_Alias_ByVal_1467165_4()
        {
            //Assert.Fail("1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases ");
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T65_Array_Alias_ByVal_1467165_4.ds");
         
         
        }
        [Test]
        public void T65_Array_Alias_ByVal_1467165_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T65_Array_Alias_ByVal_1467165_5.ds");
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
        }
        [Test]
        public void T65_Array_Alias_ByVal_1467165_6()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T65_Array_Alias_ByVal_1467165_6.ds");
            thisTest.Verify("a", new Object[] { null, 1, 2, 3 });
            thisTest.Verify("b", new Object[] { false, 1, 2, 3 });
        }
        [Test]
        [Category("Replication")]
        [Category("Feature")]
        public void T66_Array_CannotBeUsedToIndex1467069()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T66_Array_CannotBeUsedToIndex1467069.ds");
            thisTest.Verify("x", new Object[] { 10, 2, 2, 2, 14, 15 });

        }
        [Test]
        [Category("Replication")]
        [Category("Feature")]
        public void T66_Array_CannotBeUsedToIndex1467069_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T66_Array_CannotBeUsedToIndex1467069_2.ds");
            thisTest.Verify("x", new Object[] { 10, 2, 2, 2, 14, 15 });

        }
        [Test]
        public void T67_Array_Remove_Item()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T67_Array_Remove_Item.ds");
            thisTest.Verify("a", new Object[] { 2, 3, 4, 5, 7 });

        }
        [Test]
        public void T67_Array_Remove_Item_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T67_Array_Remove_Item_2.ds");
            thisTest.Verify("a", new Object[] { 2, 3, 4, 5, 6, 7, 4 });


        }
        [Test]
        public void T68_copy_byreference_1467105()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T68_copy_byreference_1467105.ds");
            thisTest.Verify("test",1 );


        }

        [Test]
        [Category("Update")]
        public void T43_Defect_1467234_negative_indexing()
        {
            String code =
@"a = { };
a[-1] = 1;
b = a;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a",new Object[] { 1 });

        }

        [Test]
        [Category("Update")]
        public void T44_Defect_1467264()
        {
            String code =
@"class A
{
    X : var[];
    constructor  A ( t1 : var[] )
    {
        X = t1;
    }
}
a1 = { A.A(1..2), A.A(2..3) };

a = a1[0].X[0][1];
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("a", n1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1467393()
        {
            String code =
@"arr = { { } };
[Imperative]
{
    numRings = 3;    
    for(i in(0..(numRings-1)))
    {
        [Associative]
        {
            x = 0..i..#numRings;
            arr[i] = x;
        }       
    }
}
test = arr;
a= 1;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0, 0, 0 }, new Object[] { 0.0, 0.5, 1.0 }, new Object[] { 0, 1, 2 } }
;
            thisTest.Verify("test", v1);                     
            
        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1467393_2()
        {
            String code =
@"arr = { { } };
[Imperative]
{
    numRings = 3;    
    for(i in(0..(numRings-1)))
    {
        s = Print(i);
        [Associative]
        {
            x = 0..i;
            arr[i] = x;
        }       
    }
}
test = arr;
a= 1;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { 0, 1 }, new Object[] { 0, 1, 2 } }
;
            thisTest.Verify("test", v1);

        }
        [Test]
        public void T45_Defect_1467393_3()
        {
            String code =
@"
def foo ()
{
    arr = { { } };
    [Imperative]
    {
        numRings = 3;    
        for(i in(0..(numRings-1)))
        {
            s = Print(i);
            [Associative]
            {
                x = 0..i;
                arr[i] = x;
            }       
        }
    }
    return = arr;
}
test = foo();
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { 0, 1 }, new Object[] { 0, 1, 2 } }
;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T45_Defect_1467393_4()
        {
            String code =
@"
def foo ()
{
    arr = { { } };
    [Imperative]
    {
        x = {0, 1};
        for(i in x)
        {
            [Associative]
            {
                arr[i] = 0..i;
            }       
        }
    }
    return = arr;
}
test = foo();
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { 0,1 }}
;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_1()
        {
            String code =
@"
//arr; //  declaring arr here works
test = [Imperative]
{
    arr; //  declaring arr here does not work.
    //arr = { }; //  declaring arr with = {} works
    
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }
    return = arr;
}
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_2()
        {
            String code =
@"
arr; //  declaring arr here works
test = [Imperative]
{
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }   
}
test = arr;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_3()
        {
            String code =
@"
arr = { }; 
test = [Imperative]
{
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }   
}
test = arr;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_4()
        {
            String code =
@"
test = [Imperative]
{
    arr = { } ;
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }  
    return= arr; 
}

";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_5()
        {
            String code =
@"
test = [Imperative]
{
    arr = { } ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}

";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_6()
        {
            String code =
@"
test = [Imperative]
{
    arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}

";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_7()
        {
            String code =
@"
arr;
[Imperative]
{
    //arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    //return = arr;   
}
test = arr;

";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_8()
        {
            String code =
@"
arr = { };
test = [Imperative]
{
    //arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}

";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_9()
        {
            String code =
@"
arr = { {}, {}};
[Imperative]
{
    //arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}
test = arr;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_9_1()
        {
            String code =
@"
//arr = { {}, {}};
def foo ()
{
    t = [Imperative]
    {
        arr = null ;    
        for(i in (0..1))
        {
            arr[i][i] = i;
        }
        return = arr;   
    }
    return = t;
}
test = foo();
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_9_2()
        {
            String code =
@"
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            arr  ;    
            for(i in (0..1))
            {
                arr[i][i] = i;
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo();
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }
        [Test]
        public void T46_Defect_1467502_9_3()
        {
            String code =
@"
arr = { {}, {}};
def foo ()
{
    t = [Imperative]
    {
        //arr = null ;    
        for(i in (0..1))
        {
            arr[i][i] = i;
        }
        return = arr;   
    }
    return = t;
}
test = foo();
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_9_4()
        {
            String code =
@"
arr;
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            //arr  ;    
            for(i in (0..1))
            {
                arr[i][i] = i;
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo();
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_9_5()
        {
            String code =
@"
class B
{
    x : int = 0;
}
arr = { {}, {}};
def foo ()
{
    t = [Imperative]
    {
        //arr = null ;    
        for(i in (0..1))
        {
            arr[i][i] = B.B();
        }
        return = arr;   
    }
    return = t;
}
test = foo().x;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            ;
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T46_Defect_1467502_9_6()
        {
            String code =
@"
//arr;
class B
{
    x : int = 0;
}
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            arr  ;    
            for(i in (0..1))
            {
                arr[i][i] = B.B();
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo().x;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            
            thisTest.Verify("test", v1);

        }

        [Test]
        public void T47_Defect_1467561_1()
        {
            String code =
@"
//arr;
class B
{
    x : int = 0;
}
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            arr  ;    
            for(i in (0..1))
            {
                if ( i < 3 )
                {
                    arr[i][i] = B.B();
                }
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo().x;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };

            thisTest.Verify("test", v1);

        }

        [Test]
        public void T47_Defect_1467561_2()
        {
            String code =
@"

class B
{
    x : int = 0;
}
class A
{
    def foo ()
    {
        t = [Imperative]
        {
            arr = {} ;    
            for(i in (0..1))
            {
                if ( i < 3 )
                {
                    arr[i][i] = B.B();
                }
            }
            return = arr;   
        }
        return = t;
    }
}
aa = A.A();
test = aa.foo().x;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };

            thisTest.Verify("test", v1);

        }
        [Test]
        public void T47_Defect_1467561_3()
        {
            String code =
@"
arr;
class B
{
    x : int = 0;
}
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            for(i in (0..1))
            {
                if ( i < 3 )
                {
                    arr[i][i] = B.B();
                }
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo().x;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };

            thisTest.Verify("test", v1);

        } 

    }
}






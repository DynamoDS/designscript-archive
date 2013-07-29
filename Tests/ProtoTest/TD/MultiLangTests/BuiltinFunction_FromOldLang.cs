using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoCore.Lang.Replication;
using ProtoTestFx.TD;


namespace ProtoTest.TD.MultiLangTests
{
    class BuiltinFunction_FromOldLang
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\BuiltinFunctionFromOldLang\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void T80580_BuiltinFunc_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80580_BuiltinFunc_1.ds");
            Object[] v1 = new Object[] { 1, 2, 3, 4, 0 };
            Object[] v2 = new Object[] { 3, 4 };
            Object[] v3 = new Object[] { -4, 5 };
            Object[] v4 = new Object[] { v2, v3 };
            Object[] v5 = new Object[] { 1.1 };
            Object[] v6 = new Object[] { 2.3, 3 };
            Object[] v7 = new Object[] { "5" };
            Object[] v8 = new Object[] { true };
            Object[] v9 = new Object[] { v5, v6, v7, v8 };



            thisTest.Verify("t1", v1);
            thisTest.Verify("t2", 3);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", 2);
            thisTest.Verify("t5", 2);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
            thisTest.Verify("t8", "{true,{false,false},true}");
            thisTest.Verify("t9", v4);
            thisTest.Verify("t10", 3);
            thisTest.Verify("t11", 4);
            thisTest.Verify("t12", 2.3025850929940459);
            thisTest.Verify("t13", 5.0);
            thisTest.Verify("t14", 1.0);
            thisTest.Verify("t15", v9);


        }

        [Test]
        public void T80581_BuiltinFunc_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80581_BuiltinFunc_2.ds");
            Object[] v1 = new Object[] { 1, 2, 3, 4, 0 };
            Object[] v2 = new Object[] { 3, 4 };
            Object[] v3 = new Object[] { -4, 5 };
            Object[] v4 = new Object[] { v2, v3 };
            Object[] v5 = new Object[] { 1.1 };
            Object[] v6 = new Object[] { 2.3, 3 };
            Object[] v7 = new Object[] { "5" };
            Object[] v8 = new Object[] { true };
            Object[] v9 = new Object[] { v5, v6, v7, v8 };



            thisTest.Verify("t0", v1);
            thisTest.Verify("t1", 3);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", 2);
            thisTest.Verify("t4", 2);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", "{true,{false,false},true}");
            thisTest.Verify("t8", v4);
            thisTest.Verify("t9", 3);
            thisTest.Verify("t10", 4);
            thisTest.Verify("t11", 2.3025850929940459);
            thisTest.Verify("t12", 5.0);
            thisTest.Verify("t13", 1.0);
            thisTest.Verify("t14", v9);


        }

        [Test]
        public void T80582_Ceil()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80582_ceil.ds");
            Object[] v1 = new Object[] { 1, 2, -1, 4, 4, -3, 0 };
            Object[] v2 = new Object[] { };
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 0);
            thisTest.Verify("d", -1);
            thisTest.Verify("e", -2);
            thisTest.Verify("g", 3);
            thisTest.Verify("h", v1);
            thisTest.Verify("b1", null);
            thisTest.Verify("b2", null);
            thisTest.Verify("b3", null);
            thisTest.Verify("b4", v2);

        }

        [Test]
        public void T80585_Count()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80585_Count.ds");
            Object[] v1 = new Object[] { 2, 2, 2 };
            thisTest.Verify("b", 4);
            thisTest.Verify("d", 4);
            thisTest.Verify("f", 3);
            thisTest.Verify("g", v1);
            thisTest.Verify("i", 2);
            thisTest.Verify("j", 2);

        }


        [Test]
        public void language_functions_test_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "language_functions_test_1.ds");
          
            string errmsg = "1467445 - Transpose() of ragged array doesn't behave correctly";
            string errmsg2 = "1467446 - Sort(sorterFunction : Function, varsToSort : var[])          doesn't return correct result";
            thisTest.VerifyRunScriptFile(testPath, "language_functions_test_1.ds", errmsg);
            thisTest.VerifyRunScriptFile(testPath, "language_functions_test_1.ds", errmsg2);

            Object[] v1 = new Object[] { 2, 3 };
            Object[] v2 = new Object[] { 1 };

            Object[] v3 = new Object[] { v2, v1 };
            Object[] v4 = new Object[] { 2 };
            Object[] v5 = new Object[] { 3 };
            Object[] v6 = new Object[] { v2, v4, v5 };
            Object[] v7 = new Object[] { 1, 2, 3 };
            Object[] v8 = new Object[] { 3, 2, 1 };
            Object[] v9 = new Object[] { 3, 1, 2 };
            Object[] v10 = new Object[] { 1, 2, 0 };
            Object[] v11 = new Object[] { 1,6 };
            Object[] v12 = new Object[] { 7, 8, 9 };
            Object[] v13 = new Object[] { v11,v1,v12 };
            Object[] v14 = new Object[] { 1,2 };
            Object[] v15 = new Object[] { v14, v5 };
            Object[] v16 = new Object[] { v1 };
            Object[] v17 = new Object[] { v2, v16 };
            

            thisTest.Verify("isUniformDepthRagged", false);
            thisTest.Verify("average", 2.0);
            thisTest.Verify("sum", 6);
            thisTest.Verify("ragged0", 1);
            thisTest.Verify("ragged1", v1);
            thisTest.Verify("ragged00", null);
            thisTest.Verify("ragged10", 2);
            thisTest.Verify("ragged11", 3);
            thisTest.Verify("ragged2", null);
            thisTest.Verify("raggedminus1", v1);
            thisTest.Verify("raggedminus1minus1", 3);
            thisTest.Verify("rankRagged", 2);
            
            thisTest.Verify("transposeRagged", null);//not sure
            //thisTest.Verify("transposeRagged", nu
            thisTest.Verify("noramlisedDepthCollection", v3);
            thisTest.Verify("isUniformDepthNormalize", true);
            thisTest.Verify("transposeNormalize", v15);
            thisTest.Verify("noramlised00", 1);
            thisTest.Verify("rankNoramlised", 2);
            thisTest.Verify("flattenedCollection", v7);
            thisTest.Verify("rankFlattened", 1);
            thisTest.Verify("reverseCollection", v8);
            thisTest.Verify("count", 3);
            thisTest.Verify("contains", true);
            thisTest.Verify("indexOf", 1);
            thisTest.Verify("reordedCollection", v9);
            thisTest.Verify("indexByValue", v10);
            thisTest.Verify("sort", v7);
            thisTest.Verify("newArray", v13);
            
            
        }

        [Test]
        public void set_operation_functions_test_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "set_operation_functions_test_1.ds");
            string errmsg1 = "1467448 - built-in function :RemoveIfNot() gives wrong result when null is the only element left after removing the others ";
            thisTest.VerifyRunScriptFile(testPath, "set_operation_functions_test_1.ds", errmsg1);

            string errmsg2 = "1467447 - built-in function : RemoveDuplicates gives wrong result when there is no duplicates";
            thisTest.VerifyRunScriptFile(testPath, "set_operation_functions_test_1.ds", errmsg2);

            Object[] v1 = new Object[] { false,true};
            Object[] v2 = new Object[] { true,null,v1 };

            Object[] v3 = new Object[] { true,null};
            Object[] v4 = new Object[] { true, null, false, true };
            Object[] v5 = new Object[] { true, null, false };
            Object[] v6 = new Object[] { null};
            Object[] v7 = new Object[] { };
            Object[] v8 = new Object[] { 0,2,4 };
            Object[] v9 = new Object[] { 1,3};
            Object[] v10 = new Object[] { 0,1,2,3,4};
            Object[] v11 = new Object[] { true,v1 };
            Object[] v12 = new Object[] { false };
            Object[] v13 = new Object[] { true, null, v12 };
 


            thisTest.Verify("allFalseSet", false);
            thisTest.Verify("someFalseSet", true);
            thisTest.Verify("someTrueSet", true);
            thisTest.Verify("someNullsSet", false);
            thisTest.Verify("setInsert", v2);
            thisTest.Verify("allFalseSetInsert", false);
            thisTest.Verify("someFalseSetInsert", true);
            thisTest.Verify("someTrueSetInsert", true);
            thisTest.Verify("someNullsSetInsert", true); 
            thisTest.Verify("countFalse", 1);
            thisTest.Verify("countTrue", 2);
            thisTest.Verify("containsNull", true);
            thisTest.Verify("removeSetInsert", v3);
            thisTest.Verify("removeNullsSetInsert", v11);
            thisTest.Verify("removeDuplicatesSetInsert", v2);
            thisTest.Verify("flattenSetInsert", v4);
            thisTest.Verify("removeDuplicatesSetInsertFalttened",v5 );
            thisTest.Verify("removeIfNotSetInsert", v6);
            thisTest.Verify("setDifferenceA", v7);
            thisTest.Verify("setDifferenceB", v8);
            thisTest.Verify("setIntersection", v9);
            thisTest.Verify("setUnion", v10);

        }

        /* [Test]
         public void TestFrameWork_IntDouble()
         {
             String code =
 @"
     a = 1.0;

 ";

             thisTest.RunScriptSource(code);
             thisTest.Verify("a", 1);
            

         }*/
    }
}

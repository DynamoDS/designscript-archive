using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    class Old_Language_Manual_Examples
    {
        readonly TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Manual_Examples\\";

        [SetUp]
        public void SetUp()
        {
        }


        [Test]
        [Category("SmokeTest")]
        public void Test_4_4_properties_1()
        {
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_4_properties_1.ds", errmsg);
            thisTest.Verify("myCollectionXvalues", new Object[] { 2.0, 4.0, 6.0, 8.0, 10.0 });
            thisTest.Verify("mySingleXvalue", 4.0);
            thisTest.Verify("myAltSingleXvalue", 4.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_9_count()
        {
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_9_count.ds", errmsg);
            thisTest.Verify("count_test1", 2);
            thisTest.Verify("count_test2", 2);
            thisTest.Verify("count_test3", 2);
            thisTest.Verify("count_test4", 1);
            thisTest.Verify("count_test5", 1);
            thisTest.Verify("count_test6", 0);
            thisTest.Verify("count_test7", 1);
            thisTest.Verify("count_test8", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_10_contains()
        {
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_10_contains.ds", errmsg);
            thisTest.Verify("f", true);
            thisTest.Verify("g", false);
            thisTest.Verify("h", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_11_indexOf()
        {
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_11_indexOf.ds", errmsg);
            thisTest.Verify("f", 1);
            thisTest.Verify("g", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_12_isRectangular()
        {
            string errmsg = "DNL-1467324 rev 3883: Built-in method IsRectagular is not implemented";
            errmsg = "";//DNL-1467282 Replication guides not working in constructor of class";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_12_isRectangular.ds", errmsg);

            thisTest.Verify("test1", new Object[] { new Object[] { 1.0, 1.0 }, new Object[] { 2.0, 2.0 } });
            thisTest.Verify("test2", new Object[] { new Object[] { 1.0, 1.0 }, new Object[] { 2.0, 2.0 } });
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_13_Transpose()
        {
            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_13_Transpose.ds", errmsg);

            thisTest.Verify("b", 1);
            thisTest.Verify("c", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_14_isUniformDepth()
        {
            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_14_isUniformDepth.ds", errmsg);
            Object n1 = null;

            thisTest.Verify("individualMemberB", 2);
            thisTest.Verify("individualMemberD", n1);
            thisTest.Verify("individualMemberE", 6);
            thisTest.Verify("testForDeepestDepth", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_15_someNulls()
        {
            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_15_someNulls.ds", errmsg);


            thisTest.Verify("a", new object[] { 1, 2, 3 });
            thisTest.Verify("b", 3);
            thisTest.Verify("c", false);
            thisTest.Verify("d", 2);
            thisTest.Verify("f", 1);
            thisTest.Verify("g", false);
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_17_arrayAssignment()
        {
            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_17_arrayAssignment.ds", errmsg);

            thisTest.Verify("c", new object[] { 0, -1, 2.500000, null, new object[] { 3.400000, 4.500000 }, 5 });
            thisTest.Verify("b", new object[] { 0, -1, 2.500000, null, new object[] { 3.400000, 4.500000 }, 5 });
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_18_removeByIndex()
        {
            string errmsg = "1467371 Sprint28:Rev:4088: Negative index value is throwing error Index out of Range while using in Remove function.";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_18_removeByIndex.ds", errmsg);

            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);
            thisTest.Verify("d", 4);
            thisTest.Verify("x", new object[] { 1, 2, 3, 4 });
            thisTest.Verify("u", new object[] { 2, 3, 4 });
            thisTest.Verify("v", new object[] { 1, 2, 3 });
            thisTest.Verify("w", new object[] { 4, 1, 2, 3, 4 });

        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_20_zipped_collection()
        {
            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_20_zipped_collection.ds", errmsg);

            thisTest.Verify("a", new object[] { 3, 4, 5 });
            thisTest.Verify("b", new object[] { 2, 6 });
            thisTest.Verify("c", new object[] { 5, 10 });
            thisTest.Verify("c", new object[] { 5, 10 });
            
        }

        [Test]
        [Category("SmokeTest")]
        public void Test_4_22_replication_guide_with_ragged_collection()
        {
            string errmsg = "1467374 Sprint 28:Rev:4088: DS throws Type conversion.. & Index out of range.... error while adding two array jagged array.";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "Test_4_22_replication_guide_with_ragged_collection.ds", errmsg);

            thisTest.Verify("a", new object[] { 1, new object[] { 3, 4 } });
            thisTest.Verify("b", new object[] { new object[] { 5, 6 }, 7 });
            thisTest.Verify("c", new object[] { new object[] { 6, 7 }, new object[] { 10, 11 } });

        }

    }
}
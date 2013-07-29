using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    public class RangeExpressions
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_SimpleRangeExpression()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T01_SimpleRangeExpression.ds");

            List<Object> result = new List<Object> { 1, -1, -3, -5 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result0 = new List<Object> { 2, 4, 6 };
            Assert.IsTrue(mirror.CompareArrays("a1", result0, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.8, 1 };
            Assert.IsTrue(mirror.CompareArrays("a2", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.7, 1 };
            Assert.IsTrue(mirror.CompareArrays("a3", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.6, 1 };
            Assert.IsTrue(mirror.CompareArrays("a4", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a5", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 1, 1.1 };
            Assert.IsTrue(mirror.CompareArrays("a6", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 9, 10 };
            Assert.IsTrue(mirror.CompareArrays("a7", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 9, 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7, 9.8, 9.9, 10 };
            Assert.IsTrue(mirror.CompareArrays("a8", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a9", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a10", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a11", result10, typeof(System.Double)));
            List<Object> result11 = new List<Object> { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a12", result11, typeof(System.Double)));
            List<Object> result12 = new List<Object> { 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a13", result12, typeof(System.Double)));
            List<Object> result13 = new List<Object> { 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a14", result13, typeof(System.Double)));
            List<Object> result14 = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a17", result14, typeof(System.Double)));
        }


        [Test]
        [Category("SmokeTest")]
        public void T02_SimpleRangeExpression()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T02_SimpleRangeExpression.ds");

            List<Object> result = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a15", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.5, 0.25 }; 
            Assert.IsTrue(mirror.CompareArrays("a16", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a18", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a19", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 1, 2, 3, 4, 5, 6 };
            Assert.IsTrue(mirror.CompareArrays("a20", result4, typeof(System.Double)));

        }

        [Test]
        public void T03_SimpleRangeExpressionUsingCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T03_SimpleRangeExpressionUsingCollection.ds");
            thisTest.Verify("w1", new object[] { 3, 2 });
            thisTest.Verify("w2", new object[] { 3, 2 });
            thisTest.Verify("w3", new object[] { new object[] { 1, 2, 3 }, new object[] { 2, 3, 4 } });
            thisTest.Verify("w4", new object[] { 1, 2, 3 });
            thisTest.Verify("w5", new object[] { 1, 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_SimpleRangeExpressionUsingFunctions()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T04_SimpleRangeExpressionUsingFunctions.ds");

            List<Object> result = new List<Object> { 1, 3, 5, 7 };
            Assert.IsTrue(mirror.CompareArrays("z1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 1, 2, 3, 4, 5, 6, 7, 8 }; 
            Assert.IsTrue(mirror.CompareArrays("z2", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 1, 2, 3, 4, 5, 6, 7, 8 };
            Assert.IsTrue(mirror.CompareArrays("z3", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 4, 3, 2, 1 };
            Assert.IsTrue(mirror.CompareArrays("z4", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 4, 3, 2, 1 };
            Assert.IsTrue(mirror.CompareArrays("z5", result4, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 1.3 };
            Assert.IsTrue(mirror.CompareArrays("z7", result8, typeof(System.Double)));
        }


        [Test]
        [Category("SmokeTest")]
        public void T05_RangeExpressionWithIncrement()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T05_RangeExpressionWithIncrement.ds");

            List<Object> result3 = new List<Object> { 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { -0.4, -0.5 };
            Assert.IsTrue(mirror.CompareArrays("e1", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { -0.4, -0.3 };
            Assert.IsTrue(mirror.CompareArrays("f", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.4, 0.6, 0.8, 1 };
            Assert.IsTrue(mirror.CompareArrays("g", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("h", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.4 };
            Assert.IsTrue(mirror.CompareArrays("i", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.6, 1 };
            Assert.IsTrue(mirror.CompareArrays("j", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 0.02, 0.025, 0.03 };
            Assert.IsTrue(mirror.CompareArrays("k", result10, typeof(System.Double)));
            List<Object> result11 = new List<Object> { 0.9, 0.925, 0.95, 0.975, 1 };
            Assert.IsTrue(mirror.CompareArrays("l", result11, typeof(System.Double)));
            List<Object> result12 = new List<Object> { 0.05, 0.09 };
            Assert.IsTrue(mirror.CompareArrays("m", result12, typeof(System.Double)));


        }

        [Test]
        [Category("SmokeTest")]
        public void T06_RangeExpressionWithIncrement()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T06_RangeExpressionWithIncrement.ds");

            List<Object> result = new List<Object> { 0.3, 0.2, 0.1 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.1, 0.3 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.1, 0.2, 0.3 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void T07_RangeExpressionWithIncrementUsingFunctionCall()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T07_RangeExpressionWithIncrementUsingFunctionCall.ds");
            List<Object> l1 = new List<object> { 3.5, new List<Object> { 2.5, 3.5, 5, 4.65 } };
            List<Object> l2 = new List<object> { 7, 16, 10, 2, -1, -0.34 };
            Assert.IsTrue(mirror.CompareArrays("d", l1, typeof(System.Double)));
            Assert.IsTrue(mirror.CompareArrays("f", l2, typeof(System.Int64)));

        }


        [Test]
        public void T08_RangeExpressionWithIncrementUsingVariables()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T08_RangeExpressionWithIncrementUsingVariables.ds");
            thisTest.Verify("h", new object[] { 16.0, 12.0, 8.0, 4.0 });
            thisTest.Verify("i", new object[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0 });
            thisTest.Verify("j", new object[] { 16.0, 14.0, 12.0, 10.0, 8.0, 6.0, 4.0, 2.0 });
            thisTest.Verify("k", new object[] { new object[] { 1 }, new object[] { 2, 3, 4 } });
            thisTest.Verify("l", new object[] { new object[] { 1, 2 }, new object[] { 2, 3 } });
        }



        [Test]
        [Category("SmokeTest")]
        public void T09_RangeExpressionWithApproximateIncrement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T09_RangeExpressionWithApproximateIncrement.ds");

            List<Object> result3 = new List<Object> { 0.0, 0.5, 1.0, 1.5, 2.0 };
            Assert.IsTrue(mirror.CompareArrays("a", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 0, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09, 0.1, };
            Assert.IsTrue(mirror.CompareArrays("b", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0, 0.1 };
            Assert.IsTrue(mirror.CompareArrays("f", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.2, 0.3 };
            Assert.IsTrue(mirror.CompareArrays("g", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.3, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("h", result7, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.8, 0.5 };
            Assert.IsTrue(mirror.CompareArrays("j", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 0.5, 0.8 };
            Assert.IsTrue(mirror.CompareArrays("k", result10, typeof(System.Double)));


        }

        [Test]
        [Category("Replication")]
        public void T10_RangeExpressionWithReplication()
        {
            //Assert.Fail("1454507 - Sprint15 : Rev 666 : Nested range expressions are throwing NullReferenceException ");
            //Assert.Fail("Replication guides are not implmented");

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T10_RangeExpressionWithReplication.ds");

            List<Object> result = new List<Object>() { 1 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object>() { 3 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            //List<Object> result2 = new List<Object>() { { 1, 2, 3 } };
            //Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            // List<Object> result3 = new List<Object>() { { { 0.100 }, { 0.100, 0.600 }, { 0.100, 0.600 }, { 0.100, 0.600 }, { 0.100, 0.600 } }, { { 0.150 }, { 0.150 }, { 0.150, 0.650 }, { 0.150, 0.650 }, { 0.150, 0.650 } }, { { 0.200 }, { 0.200 }, { 0.200, 0.700 }, { 0.200, 0.700 }, { 0.200, 0.700 } } };
            // Assert.IsTrue(mirror.CompareArrays("f", result3, typeof(System.Double)));
            // List<Object> result4 = new List<Object>() { { { 0.100 }, { 0.150 }, { 0.200 } }, { { 0.100, 0.600 }, { 0.150 }, { 0.200 } }, { { 0.100, 0.600 }, { 0.150, 0.650 }, { 0.200, 0.700 } }, { { 0.100, 0.600 }, { 0.150, 0.650 }, { 0.200, 0.700 } }, { { 0.100, 0.600 }, { 0.150, 0.650 }, { 0.200, 0.700 } } };
            // Assert.IsTrue(mirror.CompareArrays("h", result4, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_RangeExpressionUsingClasses()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T11_RangeExpressionUsingClasses.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a3").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("a4").Payload == 3);
            List<Object> result = new List<Object> { 1, 2, 3, 4 };
            Assert.IsTrue(mirror.CompareArrays("a1", result, typeof(System.Int64)));
            Assert.IsTrue(mirror.CompareArrays("a2", result, typeof(System.Int64)));
        }

        [Test]
        public void T12_RangeExpressionUsingNestedRangeExpressions()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T12_RangeExpressionUsingNestedRangeExpressions.ds");
            thisTest.Verify("b", new object[] { 3.0, 2.4, 1.8, 1.2, 0.6 });
            thisTest.Verify("c", new object[] { 5 });
            thisTest.Verify("d", new object[] { 5.5, 5.75, 6.0 });
            thisTest.Verify("e1", new object[] { -6, -7, -8 });
            thisTest.Verify("f", new object[] { 1.0, 0.8 });
            thisTest.Verify("g", new object[] { 1.0, 0.1, -0.8 });
            thisTest.Verify("h", new object[] { 2.5, 2.5 + 0.25 / 3.0, 2.5 + 0.5 / 3.0, 2.75 });
            thisTest.Verify("i", new Object[] {1.000000,1.555556,2.111111,2.666667,3.222222,3.777778,4.333333,4.888889,5.444444,6.000000});
            thisTest.Verify("j", new object[] { 1.0, 1.0 - 0.1 / 3.0, 1.0 - 0.2 / 3.0, 0.9 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_RangeExpressionWithStartEndValuesUsingFunctionCall()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T13_RangeExpressionWithStartEndValuesUsingFunctionCall.ds");

            List<Object> result3 = new List<Object> { 1, 1.5, 2.0 };
            Assert.IsTrue(mirror.CompareArrays("x", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 1 };
            Assert.IsTrue(mirror.CompareArrays("b", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 4.5, 5.1, 5.6999999999999993, 6.2999999999999989, 6.8999999999999986, 7.4999999999999982 };
            Assert.IsTrue(mirror.CompareArrays("c", result5, typeof(System.Double)));

            Assert.IsTrue(mirror.GetValue("e1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            List<Object> result9 = new List<Object> { 4.5, 5.25, 6.0 };
            Assert.IsTrue(mirror.CompareArrays("f", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 2.0, 1.75, 1.5, 1.25, 1.0 };
            Assert.IsTrue(mirror.CompareArrays("g", result10, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void T14_RangeExpressionUsingClassMethods()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T14_RangeExpressionUsingClassMethods.ds");
            List<Object> result = new List<Object> { 5, 6, 7, 8 };
            Assert.IsTrue(mirror.CompareArrays("b", result, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_SimpleRangeExpression_1()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T15_SimpleRangeExpression_1.ds");

            List<Object> result = new List<Object> { 1, 1.6, 2.2 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.1, 0.13333333333333333, 0.16666666666666666, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 2, 2.1, 2.2, 2.3000000000000003 };
            Assert.IsTrue(mirror.CompareArrays("d", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.5, 0.75, 1 };
            Assert.IsTrue(mirror.CompareArrays("f", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 0.5, 0.51, 0.52, 0.53, 0.54, 0.55, 0.56, 0.57, 0.58, 0.59, 0.6 };
            Assert.IsTrue(mirror.CompareArrays("g", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0.51, 0.52 };
            Assert.IsTrue(mirror.CompareArrays("h", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.95, 1 };
            Assert.IsTrue(mirror.CompareArrays("i", result6, typeof(System.Double)));
            //List<Object> result7 = new List<Object>() { 0.9 };
            ////Assert.IsTrue(mirror.CompareArrays("k", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("l", result8, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void T16_SimpleRangeExpression_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T16_SimpleRangeExpression_2.ds");

            List<Object> result = new List<Object> { 1.2, 1.3 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 2, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 1.2, 1.3, 1.4, 1.5 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 1.3 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 1.5, 1.7 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 3.0, 3.2 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 3.6, 3.8 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 3.8, 4.0 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void T17_SimpleRangeExpression_3()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T17_SimpleRangeExpression_3.ds");

            List<Object> result = new List<Object> { 1, 1.2, 1.4, 1.6, 1.8, 2, 2.2 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 1, 1.5, 2 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 2.3, 2.15, 2 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 1.2, 1.4 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.9, 0.91, 0.92, 0.93, 0.94, 0.95, 0.96, 0.97, 0.98, 0.99 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.8, 0.9 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.8, 0.9 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.9, 1, 1.1 };
            Assert.IsTrue(mirror.CompareArrays("i", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 1, 0.95, 0.9 };
            Assert.IsTrue(mirror.CompareArrays("j", result10, typeof(System.Double)));
            List<Object> result11 = new List<Object> { 1.2, 1.3 };
            Assert.IsTrue(mirror.CompareArrays("k", result11, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void T18_SimpleRangeExpression_4()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T18_SimpleRangeExpression_4.ds");

            List<Object> result = new List<Object> { 2.3, 2.6 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 4.3, 4 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 3.7, 4 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 4, 4.3 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 3.2 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.4, 0.45, 0.5, 0.55, 0.6 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.4, 0.45 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void T19_SimpleRangeExpression_5()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T19_SimpleRangeExpression_5.ds");

            //List<Object> result = new List<Object>() { 0.1 };
            //Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.1, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.1, 0.15, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.1, 0.1, 0.1, 0.1 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0.9, 0.925, 0.95, 0.975, 1 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.8, 0.845, 0.89 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.9, 0.85, 0.8 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.9, 0.85, 0.8, 0.75, 0.7 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.6, 0.73333333333333328, 0.8666666666666667, 1.0 };
            Assert.IsTrue(mirror.CompareArrays("i", result9, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void T20_RangeExpressionsUsingPowerOperator()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T20_RangeExpressionsUsingPowerOperator.ds");

            List<Object> result = new List<Object> { 2, 4, 6, 8 };
            Assert.IsTrue(mirror.CompareArrays("e1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0 };
            Assert.IsTrue(mirror.CompareArrays("f", result1, typeof(System.Double)));

            /*List<Object> result2 = new List<Object>() { 0.01,0.02,0.03,0.04 };
            Assert.IsTrue(mirror.CompareArrays("h", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object>() { 0.1, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("i", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object>() { 0.4,0.45 };
            Assert.IsTrue(mirror.CompareArrays("j", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object>() { 1.2,1.4 };
            Assert.IsTrue(mirror.CompareArrays("k", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object>() { 1.2,1.3 };
            Assert.IsTrue(mirror.CompareArrays("l", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object>() { 0.8,0.9 };
            Assert.IsTrue(mirror.CompareArrays("m", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object>() { 0.08, 0.09 };
            Assert.IsTrue(mirror.CompareArrays("n", result9, typeof(System.Double)));*/

        }



        [Test]
        [Category("SmokeTest")]
        public void T21_RangeExpressionsUsingEvenFunction()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T21_RangeExpressionsUsingEvenFunction.ds");
            Object[] c = new Object[] { 2, 2, 4 } ;
            Object[] d = new Object[] { new Object []{2.000000}, new Object [] {2.000000}, new Object[] {4.000000} };
            Object [][] e1 = new Object [][] {new Object[]{2,3,4,5,6,7,8,9,10,11,12},new Object[] {4,5,6,7,8,9,10,11,12,13,14},new Object [] {6,7,8,9,10,11,12,13,14,15,16},new Object[]{8,9,10,11,12,13,14,15,16,17,18},new Object[]{10,11,12,13,14,15,16,17,18,19,20}};
            Object[][] f = new Object[][] { new Object[] { 2, 3, 4 }, new Object[] { 4, 5, 6 }, new Object[] { 4, 5, 6 }, new Object[] { 6, 7, 8 }, new Object[] { 6, 7, 8 }, new Object[] { 8, 9, 10 }, new Object[] { 8, 9, 10 }, new Object[] { 10, 11, 12 }, new Object [] { 10, 11, 12 }, new Object []{ 12, 13, 14 }, new Object []{ 12, 13, 14 } };
            Object [][] g={new Object[]{2,5,8,11},new Object[]{4,7,10,13},new Object[]{6,9,12,15},new Object[] {8,11,14,17},new Object []{10,13,16,19}}; 
            thisTest.Verify("c",c);
            thisTest.Verify("d", d);
            thisTest.Verify("e1", e1);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            //List<Object> result = new List<Object>()  {2,2,4};
            // Assert.IsTrue(mirror.CompareArrays("d", result, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_RangeExpressionsUsingClassMethods_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\T22_RangeExpressionsUsingClassMethods_2.ds");
            List<Object> result = new List<Object> { 5, 4, 6, 8, 10 };
            Assert.IsTrue(mirror.CompareArrays("d", result, typeof(System.Int64)));
        }

        [Test]
        public void T23_RangeExpressionsUsingClassMethods_3()
        {
            //string err = "1467069 - Sprint 23: rev 2634: 328588 An array cannot be used to index into an array, must throw warning";
            string err = "";
            thisTest.VerifyRunScriptFile(testPath, "T23_RangeExpressionsUsingClassMethods_3.ds", err);
            List<Object> result2 = new List<Object>() { 10, 16, 18, 16, 10 };
            // List<Object> result5 = new List<Object>() { { 11, 16, 18, 16, 10 }, { 10, 10, 9, 8, 7 }, { 10, 8, 6, 4, 5 }, { 1, 2, 3, 4, 2 } };
            // Assert.IsTrue(mirror.CompareArrays("j", result5, typeof(System.Double)));

            thisTest.Verify("d", new object[] {10, 8, 6, 4, 5});
            thisTest.Verify("e1", new object[] {1, 2, 3, 4, 2});


        }

        [Test]
        [Category("SmokeTest")]
        public void TA01_RangeExpressionWithIntegerIncrement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA01_RangeExpressionWithIntegerIncrement.ds");

            List<Object> result = new List<Object> { 1, 3, 5 };
            Assert.IsTrue(mirror.CompareArrays("a1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 12.5, 14.5, 16.5, 18.5 };
            Assert.IsTrue(mirror.CompareArrays("a2", result1, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void TA02_RangeExpressionWithDecimalIncrement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA02_RangeExpressionWithDecimalIncrement.ds");

            List<Object> result = new List<Object> { 2, 4.7, 7.4 };
            Assert.IsTrue(mirror.CompareArrays("a1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 10, 10.3, 10.6, 10.9, 11.2, 11.5 };
            Assert.IsTrue(mirror.CompareArrays("a2", result1, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void TA03_RangeExpressionWithNegativeIncrement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA03_RangeExpressionWithNegativeIncrement.ds");

            List<Object> result = new List<Object> { 10, 8, 6, 4, 2, 0 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { -2, -3, -4, -5, -6, -7, -8, -9, -10 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 10, 8.5, 7, 5.5, 4 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void TA04_RangeExpressionWithNullIncrement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA04_RangeExpressionWithNullIncrement.ds");
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA05_RangeExpressionWithBooleanIncrement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA05_RangeExpressionWithBooleanIncrement.ds");
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA06_RangeExpressionWithIntegerTildeValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA06_RangeExpressionWithIntegerTildeValue.ds");

            List<Object> result = new List<Object> { 1, 5.5, 10 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { -2.5, 1.6666666666666666666666666667, 5.8333333333333333333333333334, 10 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));

        }



        [Test]
        [Category("SmokeTest")]
        public void TA07_RangeExpressionWithDecimalTildeValue()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA07_RangeExpressionWithDecimalTildeValue.ds");
            object[] expectedResult = { 0.2, 0.3 };
            object[] expectedResult2 = { 6.0, 7.4, 8.8, 10.2, 11.6, 13.0 };

            thisTest.Verify("a", expectedResult, 0);
            thisTest.Verify("b", expectedResult2, 0);


        }
        [Test]
        [Category("SmokeTest")]
        public void TA08_RangeExpressionWithNegativeTildeValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA08_RangeExpressionWithNegativeTildeValue.ds");

            List<Object> result = new List<Object> { 3, 2.5, 2, 1.5, 1 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 18, 16.75, 15.5, 14.25, 13 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void TA09_RangeExpressionWithNullTildeValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA09_RangeExpressionWithNullTildeValue.ds");
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA10_RangeExpressionWithBooleanTildeValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA10_RangeExpressionWithBooleanTildeValue.ds");
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA11_RangeExpressionWithIntegerHashValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA11_RangeExpressionWithIntegerHashValue.ds");

            List<Object> result = new List<Object> { 1, 1.575, 2.150, 2.725, 3.3 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 3, 3, 3 };
            Assert.IsTrue(mirror.CompareArrays("b", result2, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 3 };
            Assert.IsTrue(mirror.CompareArrays("c", result1, typeof(System.Double)));

        }

        [Test]
        [Category("SmokeTest")]
        public void TA12_RangeExpressionWithDecimalHashValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA12_RangeExpressionWithDecimalHashValue.ds");
            thisTest.VerifyBuildWarningCount(2);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidRangeExpression);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA13_RangeExpressionWithNegativeHashValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA13_RangeExpressionWithNegativeHashValue.ds");
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA14_RangeExpressionWithNullHashValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA14_RangeExpressionWithNullHashValue.ds");
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA15_RangeExpressionWithBooleanHashValue()
        {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA15_RangeExpressionWithBooleanHashValue.ds");
                thisTest.Verify("a", null);
                thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA16_RangeExpressionWithIncorrectLogic_1()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA16_RangeExpressionWithIncorrectLogic_1.ds");
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA17_RangeExpressionWithIncorrectLogic_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA17_RangeExpressionWithIncorrectLogic_2.ds");
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA18_RangeExpressionWithIncorrectLogic_3()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA18_RangeExpressionWithIncorrectLogic_3.ds");
            List<Object> result1 = new List<Object> { 7 };
            Assert.IsTrue(mirror.CompareArrays("a", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 8 };
            Assert.IsTrue(mirror.CompareArrays("b", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 9 };
            Assert.IsTrue(mirror.CompareArrays("c", result3, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA19_RangeExpressionWithIncorrectLogic_4()
        {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\RangeExpressions\\TA19_RangeExpressionWithIncorrectLogic_4.ds");
                thisTest.Verify("a", null);
        }

        [Test]
        public void TA20_RangeExpressionWithTripleDotOperator()
        {
            string err ="1463466 - Sprint 20 : Rev 2112 : triple dots are not supported in range expressions ";
            thisTest.VerifyRunScriptFile(testPath,"TA20_RangeExpressionWithTripleDotOperator.ds",err);
            //List<Object> result1 = new List<Object>() { 3 };
            //Assert.IsTrue(mirror.CompareArrays("a", result1, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA21_Defect_1454692.ds");
            thisTest.Verify("x", 6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA21_Defect_1454692_2.ds");
            thisTest.Verify("num", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA21_Defect_1454692_3.ds");
            thisTest.Verify("num", 4, 0);

        }
      
        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA21_Defect_1454692_4.ds");
            thisTest.Verify("c", 4);
            thisTest.Verify("x", 3);
        }
        [Test]
        public void T25_RangeExpression_WithDefaultDecrement()
        {
            // 1467121
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T25_RangeExpression_WithDefaultDecrement.ds");
            Object[] a = new Object[] { 5, 4, 3, 2, 1 };
            thisTest.Verify("a", a);
            
        }
        [Test]
        public void T25_RangeExpression_WithDefaultDecrement_1467121()
        {
            // 1467121
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T25_RangeExpression_WithDefaultDecrement_1467121.ds");
            Object[] a = new Object[] { 5, 4, 3, 2, 1 };
            thisTest.Verify("a", a);

        }
        [Test]
        public void T25_RangeExpression_WithDefaultDecrement_nested_1467121_2()
        {
            // 1467121
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T25_RangeExpression_WithDefaultDecrement_nested_1467121_2.ds");
            Object[][] a = new Object[][] { new Object [] { 5, 4, 3, 2, 1 },new Object[] { 4, 3, 2 }, new Object []{ 3 }, new Object[]{ 2, 3, 4 }, new Object[] { 1, 2, 3, 4, 5 } };
            //thisTest.Verify("a", a);

        }
        
        [Test]
        public void T26_RangeExpression_Function_tilda_1457845()
        {
            // 1467121
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T26_RangeExpression_Function_tilda_1457845.ds");

            Object[] a = new Object[] { 0.000000, 0.500000, 1.000000, 1.500000, 2.000000 };
            Object[] b = new Object[] {0.000000,0.010000,0.020000,0.030000,0.040000,0.050000,0.060000,0.070000,0.080000,0.090000,0.100000};
            Object[] f = new Object[]{0.000000,0.100000};
            Object[] g = new Object[] { 0.200000, 0.300000 };
            
            Object [] h = new Object []{0.300000,0.200000};
            Object [] j = new Object [] {0.800000,0.500000};
            Object [] k = new Object [] {0.500000,0.800000};
            Object  l = null;
            Object [] m = new Object [] {0.200000,0.300000};
            
            thisTest.Verify("x", 0.100000);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            thisTest.Verify("h", h);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("l", l);
            thisTest.Verify("m", m);
            
        }
        [Test]
        public void T26_RangeExpression_Function_tilda_multilanguage_1457845_2()
        {
            // 1467121
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T26_RangeExpression_Function_tilda_multilanguage_1457845_2.ds");

            Object[] a = new Object[] { 0.000000, 0.500000, 1.000000, 1.500000, 2.000000 };
            Object[] b = new Object[] { 0.000000, 0.010000, 0.020000, 0.030000, 0.040000, 0.050000, 0.060000, 0.070000, 0.080000, 0.090000, 0.100000 };
            Object[] f = new Object[] { 0.000000, 0.100000 };
            Object[] g = new Object[] { 0.200000, 0.300000 };

            Object[] h = new Object[] { 0.300000, 0.200000 };
            Object[] j = new Object[] { 0.800000, 0.500000 };
            Object[] k = new Object[] { 0.500000, 0.800000 };
            Object l = null;
            Object[] m = new Object[] { 0.200000, 0.300000 };

            thisTest.Verify("x", 0.100000);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            thisTest.Verify("h", h);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("l", l);
            thisTest.Verify("m", m);

        }
        [Test]
        public void T26_RangeExpression_Function_tilda_associative_1457845_3()
        {
            // 1467121
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T26_RangeExpression_Function_tilda_associative_1457845_3.ds");

            Object[] a = new Object[] { 0.000000, 0.500000, 1.000000, 1.500000, 2.000000 };
            Object[] b = null;
            Object[] f = new Object[] { 0.000000, 0.100000 };
            Object[] g = new Object[] { 0.200000, 0.300000 };

            Object[] h = new Object[] { 0.300000, 0.200000 };
            Object[] j = new Object[] { 0.800000, 0.500000 };
            Object[] k = new Object[] { 0.500000, 0.800000 };
            Object l = null;
            Object[] m = new Object[] { 0.200000, 0.300000 };

            thisTest.Verify("x", 0.100000);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            thisTest.Verify("h", h);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("l", l);
            thisTest.Verify("m", m);

        }
        [Test]
        public void T27_RangeExpression_Function_Associative_1463472()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T27_RangeExpression_Function_Associative_1463472.ds");
            Object[] a = new Object[] {1.000000,3.000000,5.000000,7.000000};
            thisTest.Verify("z1", a);
        }
        [Test]
        public void T27_RangeExpression_Function_Associative_1463472_2()
        {
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T27_RangeExpression_Function_Associative_1463472_2.ds");
            Object[] c = new Object[] {1,2,3,4};
            thisTest.Verify("c", c);
        }
        [Test]
        public void T27_RangeExpression_Function_return_1463472()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T27_RangeExpression_Function_return_1463472.ds");
            Object[] c = new Object[] {1,2,3,4};
            thisTest.Verify("c", c);
        }
        [Test]
        public void T27_RangeExpression_class_return_1463472_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T27_RangeExpression_class_return_1463472_2.ds");
            Object[] c = new Object[] { 1, 2, 3, 4 };
            thisTest.Verify("c", c);
        }
        
        [Test]
        public void T27_RangeExpression_Function_Associative_replication()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T27_RangeExpression_Function_Associative_replication.ds");
            Object[][] c = new Object[][] { new Object []{2},new Object[] {2,4},new Object []{2,4,6}, new Object[] {2,4,6,8} };
            thisTest.Verify("z1", c);
        }

        [Test]
        public void Regress_1467127()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "Regress_1467127.ds");
            double step = 5.0 / 9.0;
            object[] values = new object[10];
            values[9] = 6.0;
            for (int i = 0; i < 9; ++i)
            {
                values[i] = 1.0 + step * i;
            }

            thisTest.Verify("i", values);
        }
        [Test]
        public void TA22_Range_Expression_floating_point_conversion_1467127()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA22_Range_Expression_floating_point_conversion_1467127.ds");
            Object[] a = new Object []{1.000000,1.555556,2.111111,2.666667,3.222222,3.777778,4.333333,4.888889,5.444444,6.000000};
            Object[] b = new Object [] {0.100000,0.155556,0.211111,0.266667,0.322222,0.377778,0.433333,0.488889,0.544444,0.600000};
            Object [] c = new Object [] {0.010000,-0.057778,-0.125556,-0.193333,-0.261111,-0.328889,-0.396667,-0.464444,-0.532222,-0.600000};
            Object[] d = new Object []{-0.100000,-0.082222,-0.064444,-0.046667,-0.028889,-0.011111,0.006667,0.024444,0.042222,0.060000};

            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
        }
        [Test]
        public void TA22_Range_Expression_floating_point_conversion_1467127_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA22_Range_Expression_floating_point_conversion_1467127_2.ds");
            Object [] a = new Object[]{0.000000,0.777778,1.555556,2.333333,3.111111,3.888889,4.666667,5.444444,6.222222,7.000000};
            Object [] b = new Object[]{0.100000,0.175000,0.250000,0.325000,0.400000,0.475000,0.550000,0.625000,0.700000};
            Object [] c = new Object[]{0.010000};
            Object [] d = new Object[]{-0.100000,0.688889,1.477778,2.266667,3.055556,3.844444,4.633333,5.422222,6.211111,7.000000};
            Object [] e = new Object[] { 1 };
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e", e);

        }
        [Test]
        [Category ("SmokeTest") ]
        public void TA23_Defect_1466085_Update_In_Range_Expr()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA23_Defect_1466085_Update_In_Range_Expr.ds");
            Object [] v = new Object[] { 0, 1, 2 };
            thisTest.Verify("z1", v);   

        }

        [Test]
        [Category("SmokeTest")]
        public void TA23_Defect_1466085_Update_In_Range_Expr_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA23_Defect_1466085_Update_In_Range_Expr_2.ds");
            Object[] v = new Object[] { 7, 14 };
            thisTest.Verify("y1", v);

        }

        [Test]
        [Category("SmokeTest")]
        public void TA23_Defect_1466085_Update_In_Range_Expr_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TA23_Defect_1466085_Update_In_Range_Expr_3.ds");
          
            thisTest.Verify("z1", 3);
            thisTest.Verify("z2", 3);

        }

        [Test]
        [Category("SmokeTest")]
        public void IndexingIntoClassInstanceByRangeExpr()
        {
            string code = @"
class A
{
    x;
    constructor A(i)
    {
        x = i;
    }
}

x = (A.A(1..3))[0];
z = x.x;
";

            thisTest.VerifyRunScriptSource(code, "DNL-1467618 Regression : Use of the array index after replicated constructor yields complier error now");
            thisTest.Verify("z", 1);
        }

        [Test]        
        public void TA24_1467454_negative_case()
        {
            string code = @"
b = 10.0;
a = 0.0;
d1 = a..b..c;
d2 = c..b..c;
d3 = a..c..b;
d4 = c..a..c;
d5 = c..2*c..c;
";

            Object n1 = null;
            thisTest.VerifyRunScriptSource(code, "");
            thisTest.Verify("d1", n1);
            thisTest.Verify("d2", n1);
            thisTest.Verify("d3", n1);
            thisTest.Verify("d4", n1);
            thisTest.Verify("d5", n1);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        public void TA24_1467454_negative_case_2()
        {
            string code = @"
b = 10.0;
a = 0.0;
d1;d2;d3;d4;d5;
[Imperative]
{
    d1 = a..b..c;
    d2 = c..b..c;
    d3 = a..c..b;
    d4 = c..a..c;
    d5 = c..2*c..c;
}
";

            Object n1 = null;
            thisTest.VerifyRunScriptSource(code, "");
            thisTest.Verify("d1", n1);
            thisTest.Verify("d2", n1);
            thisTest.Verify("d3", n1);
            thisTest.Verify("d4", n1);
            thisTest.Verify("d5", n1);
            thisTest.VerifyBuildWarningCount(1);
        }
        
    }
}

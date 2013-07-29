using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    public class IfElseTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\"; 
        
        [SetUp]
        public void Setup()
        {
        }
        
        [Test]
        [Category ("SmokeTest")]
 public void T01_TestAllPassCondition()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T01_TestAllPassCondition.ds");

            Object o = mirror.GetValue("x").Payload;
            ProtoCore.DSASM.Mirror.DsasmArray x = (ProtoCore.DSASM.Mirror.DsasmArray)o;          
            Assert.IsTrue((Int64)x.members[0].Payload == 1);
            Assert.IsTrue((Int64)x.members[1].Payload == 1);
            Assert.IsTrue((Int64)x.members[2].Payload == 1);
            Assert.IsTrue((Int64)x.members[3].Payload == 1);


            Object o1 = mirror.GetValue("y").Payload;
            ProtoCore.DSASM.Mirror.DsasmArray y = (ProtoCore.DSASM.Mirror.DsasmArray)o1;
            Assert.IsTrue((Int64)y.members[0].Payload == 1);
            Assert.IsTrue((Int64)y.members[1].Payload == 1);
            Assert.IsTrue((Int64)y.members[2].Payload == 1);
            Assert.IsTrue((Int64)y.members[3].Payload == 1);
            Assert.IsTrue((Int64)y.members[4].Payload == 1);

            Object o2 = mirror.GetValue("z").Payload;
            ProtoCore.DSASM.Mirror.DsasmArray z = (ProtoCore.DSASM.Mirror.DsasmArray)o2;
            Assert.IsTrue((Int64)z.members[0].Payload == 1);
            Assert.IsTrue((Int64)z.members[1].Payload == 1);
            Assert.IsTrue((Int64)z.members[2].Payload == 1);
            Assert.IsTrue((Int64)z.members[3].Payload == 1);

        }



        [Test]
        //[Category ("SmokeTest")]
            [Category("Warnings and Exceptions")]
 public void T02_IfElseIf()
        {
            string err = "1467181 - Sprint25 : rev 3152: Warning message of 'lack of returning statement' should display when not all paths returns value at compiling time ";
            thisTest.VerifyRunScriptFile(testPath,"T02_IfElseIf.ds",err);
            thisTest.Verify("temp1",13);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_MultipleIfStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T03_MultipleIfStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_IfStatementExpressions()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T04_IfStatementExpressions.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_InsideFunction()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T05_InsideFunction.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("temp2").Payload == 1);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_NestedIfElse()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T06_NestedIfElse.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_ScopeVariableInBlocks()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T07_ScopeVariableInBlocks.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T08_NestedBlocks()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T08_NestedBlocks.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T09_NestedIfElseInsideWhileStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T09_NestedIfElseInsideWhileStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 6);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_TypeConversion()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T10_TypeConversion.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_TestIfElseUsingFunctionCall()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T11_TestIfElseUsingFunctionCall.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T12_TestIfElseUsingClassProperty()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T12_TestIfElseUsingClassProperty.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_IfElseIf()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T13_IfElseIf.ds");

            Assert.IsTrue((double)mirror.GetValue("temp1").Payload == 12.5);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_IfElseStatementExpressions()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T14_IfElseStatementExpressions.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 2);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T15_TestEmptyIfStmt()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T15_TestEmptyIfStmt.ds");
            
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
          

        }

        [Test]
        [Category ("SmokeTest")]
 public void T16_TestIfConditionWithNegation_Negative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T16_TestIfConditionWithNegation_Negative.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -3);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T17_WhileInsideElse()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T17_WhileInsideElse.ds");

            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 5);

            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T18_WhileInsideIf()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T18_WhileInsideIf.ds");

            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T19_BasicIfElseTestingWithNumbers()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T19_BasicIfElseTestingWithNumbers.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);            


        }

        [Test]
        [Category ("SmokeTest")]
 public void T20_BasicIfElseTestingWithNumbers()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T20_BasicIfElseTestingWithNumbers.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 6);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_IfElseWithArray_negative()
        {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T21_IfElseWithArray_negative.ds");

                Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_IfElseWithArrayElements()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T22_IfElseWithArrayElements.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T23_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T23_IfElseSyntax_negative.ds");
    });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T24_IfElseSyntax_negative.ds");
    });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T25_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T25_IfElseSyntax_negative.ds");
    });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T26_IfElseWithNegatedCondition()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T26_IfElseWithNegatedCondition.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T27_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T27_IfElseSyntax_negative.ds");
    });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T28_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T28_IfElseSyntax_negative.ds");
    });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T29_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T29_IfElseSyntax_negative.ds");
    });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T30_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T30_IfElseSyntax_negative.ds");
    });
        }


        [Test]
        [Category ("SmokeTest")]
 public void T31_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T31_IfElseSyntax_negative.ds");
    });
        }


        [Test]
        [Category ("SmokeTest")]
 public void T32_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {

                    ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T32_IfElseSyntax_negative.ds");
                });
        }


        [Test]
        [Category ("SmokeTest")]
 public void T33_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T33_IfElseSyntax_negative.ds");
                });

        }


        [Test]
        [Category ("SmokeTest")]
 public void T34_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T34_IfElseSyntax_negative.ds");
    });
        }


        [Test]
        [Category ("SmokeTest")]
 public void T35_IfElseWithEmptyBody()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T35_IfElseWithEmptyBody.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T36_IfElseInsideFunctionScope()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T36_IfElseInsideFunctionScope.ds");

            //defect 1451089
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 5);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T37_Defect_1450920()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T37_Defect_1450920.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T38_Defect_1450939()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T38_Defect_1450939.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 2);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T39_Defect_1450920_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T39_Defect_1450920_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T40_Defect_1450843()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T40_Defect_1450843.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b2").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b3").Payload == 2);           


        }

        [Test]
        [Category ("SmokeTest")]
 public void T41_Defect_1450778()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T41_Defect_1450778.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 1);


        }
        [Test]
        [Category ("SmokeTest")]
 public void T42_Defect_1449707()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T42_Defect_1449707.ds");

            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T43_Defect_1450706()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T43_Defect_1450706.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 12);
            Assert.IsTrue((Int64)mirror.GetValue("temp2").Payload == 12);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T44_Defect_1450706_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T44_Defect_1450706_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T45_Defect_1450506()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T45_Defect_1450506.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T46_TestIfWithNull()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T46_TestIfWithNull.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T47_Defect_1450858()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T47_Defect_1450858.ds");

            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 5);
        }

        [Test] 
        [Category ("SmokeTest")]
 public void T48_Defect_1450858_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T48_Defect_1450858_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("test").Payload == 24);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T49_Defect_1450783()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T49_Defect_1450783.ds");

            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T50_Defect_1450817()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T50_Defect_1450817.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T51_Defect_1452588()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T51_Defect_1452588.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);

            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T52_Defect_1452588_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T52_Defect_1452588_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T53_Defect_1452575()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T53_Defect_1452575.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 0);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T54_Defect_1451089()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T54_Defect_1451089.ds");

            Assert.IsTrue((double)mirror.GetValue("temp").Payload == 7.5);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T55_Defect_1450506()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\IfStatement\\T55_Defect_1450506.ds");

            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T56_Defect_1460162()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T56_Defect_1460162.ds");
            object v1 = null;
        }

        [Test]
        [Category ("SmokeTest")]
 public void T57_Function_With_If_Else_But_No_Default_Return_Statement()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T57_Function_With_If_Else_But_No_Default_Return_Statement.ds");
       
            //Verification 
            thisTest.Verify("x", 2);
            thisTest.Verify("y", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T58_Defect_1450932_comparing_collection_with_singleton_Imperative()
        {           
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T58_Defect_1450932_comparing_collection_with_singleton_Imperative.ds");

            Object[] v1 = new Object[] { false, false };
            Object v2 = null;
            
            //Verification 
            thisTest.Verify("c", 1);
            thisTest.Verify("d", v1);
            thisTest.Verify("f", v2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T58_Defect_1450932_comparing_collection_with_singleton_Associative()
        {           
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T58_Defect_1450932_comparing_collection_with_singleton_Associative.ds");

            Object[] v1 = new Object[] { new Object[] { false, false }, new Object[] { false, false } };

            //Verification 
            thisTest.Verify("d2", v1);
           
        }

        [Test]
        [Category("SmokeTest")]
        public void T58_Defect_1450932_comparing_collection_with_singleton_Associative_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T58_Defect_1450932_comparing_collection_with_singleton_Associative_2.ds");

            //Verification 
            thisTest.Verify("f2", new Object[] { false, false });
        }

        [Test]      
        [Category("Inline Conditionals")]
        public void T58_Defect_1450932_comparing_collection_with_singleton_Associative_3()
        {
            string err = "";//1467192 - sprint24: rev 3199 : REGRESSION:Inline Condition with Replication is giving wrong output when multiple inline statements are used";
            thisTest.VerifyRunScriptFile(testPath, "T58_Defect_1450932_comparing_collection_with_singleton_Associative_3.ds",err);

            //Verification 
            thisTest.Verify("f2", new Object[] { false, false });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T59_Defect_1453881()
        {
            string err = "1467248 - Sprint25 : rev 3452 : comparison with null should result to false in conditional statements";
            thisTest.VerifyRunScriptFile(testPath, "T59_Defect_1453881.ds",err);
            
            //Verification 
            thisTest.Verify("b", 5.5);
            thisTest.Verify("d", 2);
            thisTest.Verify("d2", 0);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T59_Defect_1453881_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T59_Defect_1453881_2.ds");

            Object[] v1 = new Object[] { 5.5, 2 };

            //Verification 
            thisTest.Verify("test", v1);

        }
        
        
        [Test]
        [Category ("SmokeTest")]
 public void T60_Comparing_Class_Properties()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T60_Comparing_Class_Properties.ds");

            //Verification 
            thisTest.Verify("x1", true);
            thisTest.Verify("x2", true);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T60_Comparing_Class_Properties_With_Null()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T60_Comparing_Class_Properties_With_Null.ds");

            //Verification 
            thisTest.Verify("x1", true);

        }

        [Test]
        public void T61_Accessing_non_existent_properties_of_array_elements()
        {
            // Assert.Fail("");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T61_Accessing_non_existent_properties_of_array_elements.ds");

            //Verification 
            thisTest.Verify("t1", 0);
            thisTest.Verify("t2", 1);

        }
        [Test]
        [Category("Statements")]
        public void T62_Condition_Not_Evaluate_ToBool()
        {
            string err= "1467170 Sprint 25 - Rev 3142 it must skip the else loop if the conditional cannot be evaluate to bool it must be skip both if and else";
            thisTest.VerifyRunScriptFile(testPath, "T62_Condition_Not_Evaluate_ToBool.ds",err);

            //Verification 
            thisTest.Verify("A", 1);

        }
      
        [Test]
        public void T63_return_in_if_1467073()
        {
            string err= "1467073 - sprint 23 rev 2651-328756 throws warning missing return statement ";
            thisTest.VerifyRunScriptFile(testPath, "T63_return_in_if_1467073.ds",err);
            thisTest.Verify("c", 2);

        }


        //TDD for if condition/inline condition
        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleIfCondition_1()
        {
            String code =
@"
r = [Imperative]
{
    //b = 1;
    if (null==false)
    {
        return = ""null==true"";
    }
    else if(!null)
    {
        return = ""!null==true"";
    }

    return = ""expected"";
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "expected");

        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleIfCondition_2()
        {
            String code =
@"
r = [Imperative]
{
    if (!null)
    {
        return = ""!null==true"";
    }
    else if(!(true||null))
    {
        return = ""true||null==false"";
    }else
    {
        return =""expected""; 
    }
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "expected");

        }


        [Test]
        [Category("TDDIfInline")]
        // r = true!?
        public void TDD_NullAsArgs()
        {
            String code =
@"
def foo(x:string)
{
    return = 1;
}

r:bool = foo(null);
";
            thisTest.RunScriptSource(code);
            
            thisTest.Verify("r",true );

        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_UserDefinedTypeConvertedToBool_NotNull_defect()
        {
            String code =
@"
        class A
{
        a:int;
        constructor A (b:int)
        {
                a=b;
    }
}

d:bool=A.A(5);
";
            thisTest.RunScriptSource(code);

            thisTest.Verify("d", true);

        }

        [Test]
        [Category("TDDIfInline")]
        //not null user defined var is not evaluated as true
        public void TDD_UserDefinedTypeConvertedToBool()
        {
            String code =
@"
        class A{}
r = 
[Imperative]
{
a = A.A();
if(a)
{
    return = true;
}
}
";
            thisTest.RunScriptSource(code);

            thisTest.Verify("r", true);

        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_UserDefinedTypeConvertedToBool_Null()
        {
            String code =
@"
n;
r = 
[Imperative]
{
a = A.A();
b:bool = A.A();
def foo(x:bool)
{
    return = ""true"";
}
m = b;
n = foo(a);
return = foo(b);
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "true");
            thisTest.Verify("n", "true");

        }

        //inline
        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleInlineCondition_1()
        {
            String code =
@"
r = [Imperative]
{

return = null==false?""null==false"":""null==false is false"";

}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "null==false is false");

        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleInlineCondition_2()
        {
            String code =
@"
r = [Imperative]
{

    return = !null==false?""!null==false"":""expected"";
   
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "expected");

        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_UserDefinedTypeConvertedToBool_Inline()
        {
            String code =
@"
        class A{}
r = 
[Imperative]
{
a = A.A();
return = a==true?true:""A.A()!=true"";
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", true);

        }

    }
}         



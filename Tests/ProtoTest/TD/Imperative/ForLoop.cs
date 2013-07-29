using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    class ForLoop
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\";
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T01_TestNegativeSyntax_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T01_TestNegativeSyntax_Negative.ds");

            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_TestNegativeUsage_InAssociativeBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T02_TestNegativeUsage_InAssociativeBlock_Negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_TestNegativeUsage_InUnnamedBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T03_TestNegativeUsage_InUnnamedBlock_Negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_TestNegativeUsage_OutsideBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T04_TestNegativeUsage_OutsideBlock_Negative.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_TestForLoopInsideNestedBlocks()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T05_TestForLoopInsideNestedBlocks.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_TestInsideNestedBlocksUsingCollectionFromAssociativeBlock()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T06_TestInsideNestedBlocksUsingCollectionFromAssociativeBlock.ds");

            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 9);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_TestForLoopUsingLocalVariable()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T07_TestForLoopUsingLocalVariable.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 30);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T08_TestForLoopInsideFunctionDeclaration()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T08_TestForLoopInsideFunctionDeclaration.ds");

            Assert.IsTrue((double)mirror.GetValue("y").Payload == 6);
            Assert.IsTrue((double)mirror.GetValue("z").Payload == 7);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T09_TestForLoopWithBreakStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T09_TestForLoopWithBreakStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_TestNestedForLoops()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T10_TestNestedForLoops.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 18);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_TestForLoopWithSingleton()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T11_TestForLoopWithSingleton.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T12_TestForLoopWith2DCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T12_TestForLoopWith2DCollection.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 21);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_TestForLoopWithNegativeAndDecimalCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T13_TestForLoopWithNegativeAndDecimalCollection.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == -9);
            Assert.IsTrue((double)mirror.GetValue("y").Payload == 10.2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_TestForLoopWithBooleanCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T14_TestForLoopWithBooleanCollection.ds");

            Assert.IsTrue(mirror.GetValue("x").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T15_TestForLoopWithMixedCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T15_TestForLoopWithMixedCollection.ds");

            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("x").Payload) == -27);
            Assert.IsTrue(mirror.GetValue("y").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T16_TestForLoopInsideIfElseStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T16_TestForLoopInsideIfElseStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T17_TestForLoopInsideNestedIfElseStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T17_TestForLoopInsideNestedIfElseStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T18_TestForLoopInsideWhileStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T18_TestForLoopInsideWhileStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 15);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T19_TestForLoopInsideNestedWhileStatement()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T19_TestForLoopInsideNestedWhileStatement.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 125);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T20_TestForLoopWithoutBracket()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T20_TestForLoopWithoutBracket.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_TestIfElseStatementInsideForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T21_TestIfElseStatementInsideForLoop.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 10);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_TestWhileStatementInsideForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T22_TestWhileStatementInsideForLoop.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 75);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T23_TestForLoopWithDummyCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T23_TestForLoopWithDummyCollection.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a6").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("a5").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("a4").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("a3").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a2").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("a1").Payload == 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_TestForLoopToModifyCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T24_TestForLoopToModifyCollection.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a6").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("a7").Payload == 8);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T25_TestForLoopEmptyCollection()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T25_TestForLoopEmptyCollection.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T26_TestForLoopOnNullObject()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T26_TestForLoopOnNullObject.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T27_TestCallingFunctionInsideForLoop()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T27_TestCallingFunctionInsideForLoop.ds");

            Assert.IsTrue((double)mirror.GetValue("x").Payload == 17);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T28_Defect_1452966()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T28_Defect_1452966.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 18);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T29_Defect_1452966_2()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T29_Defect_1452966_2.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 21);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T30_ForLoopNull()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T30_ForLoopNull.ds");

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T31_ForLoopSyntax01_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T31_ForLoopSyntax01_Negative.ds");


                });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T32_ForLoopSyntax02_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Imperative\\ForLoop\\T32_ForLoopSyntax02_Negative.ds");


            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T33_ForLoopToReplaceReplicationGuides()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T33_ForLoopToReplaceReplicationGuides.ds");

            thisTest.Verify("a1", 4, 0);
            thisTest.Verify("a2", 5, 0);
            thisTest.Verify("a3", 5, 0);
            thisTest.Verify("a4", 6, 0);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T34_Defect_1452966()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T34_Defect_1452966.ds");

            thisTest.Verify("sum", 40, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T35_Defect_1452966_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T35_Defect_1452966_2.ds");

            thisTest.Verify("sum", 21, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T36_Defect_1452966_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T36_Defect_1452966_3.ds");

            thisTest.Verify("b", 21, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T37_Defect_1454517()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T37_Defect_1454517.ds");

            thisTest.Verify("b", 9, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T38_Defect_1454517_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T38_Defect_1454517_2.ds");

            thisTest.Verify("x", 9, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T38_Defect_1454517_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T38_Defect_1454517_3.ds");

            thisTest.Verify("b", 9);

           
        }        

        [Test]
        [Category ("SmokeTest")]
 public void T39_Defect_1452951()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T39_Defect_1452951.ds");

            thisTest.Verify("x", 9);

            
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T39_Defect_1452951_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T39_Defect_1452951_1.ds");
            object[] expectedResult = { 4, 4 };

            thisTest.Verify("x", expectedResult);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T39_Defect_1452951_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T39_Defect_1452951_2.ds");
            object[] expectedResult = { 11, 5 };

            thisTest.Verify("a4", expectedResult);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T39_Defect_1452951_3()
        {
            //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Regression : Imperative code is not allowed in class constructor ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T39_Defect_1452951_3.ds");
            object[] expectedResult = { 10, 4 };

            thisTest.Verify("a3", 2);
            thisTest.Verify("a4", expectedResult);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T40_Create_3_Dim_Collection_Using_For_Loop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T40_Create_3_Dim_Collection_Using_For_Loop.ds");

            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);

            thisTest.Verify("p9", 9, 0);

            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T41_Create_3_Dim_Collection_Using_For_Loop_In_Func_Call()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T41_Create_3_Dim_Collection_Using_For_Loop_In_Func_Call.ds");

            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T42_Create_3_Dim_Collection_Using_For_Loop_In_Class_Constructor()
        {
            //string errmsg = "1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2)";
            string errmsg = "1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2)";
            thisTest.VerifyRunScriptFile(testPath, "T42_Create_3_Dim_Collection_Using_For_Loop_In_Class_Constructor.ds", errmsg);

            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T43_Create_3_Dim_Collection_Using_For_Loop_In_Class_Method()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T43_Create_3_Dim_Collection_Using_For_Loop_In_Class_Method.ds");

            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
        }
        [Test]
        [Category("Array")]
        public void T43_Create_CollectioninForLoop_1457172()
        {
            string err = "1457172 - Sprint16 : Rev 1061 : For loop cannot be used to create n dimensional collection from inside a class";
            thisTest.VerifyRunScriptFile(testPath, "T43_Create_CollectioninForLoop_1457172.ds",err);

            thisTest.Verify("p1", 3, 0);
            thisTest.Verify("p2", 4, 0);
            thisTest.Verify("p3", 5, 0);
            thisTest.Verify("p4", 4, 0);
            thisTest.Verify("p5", 5, 0);
            thisTest.Verify("p6", 6, 0);
            thisTest.Verify("p7", 5, 0);
            thisTest.Verify("p8", 6, 0);
            thisTest.Verify("p9", 7, 0);
        }
        [Test]
        [Category("Array")]
        public void T43_Create_CollectioninForLoop_1457172_2()
        {
            string err = "1457172 - Sprint16 : Rev 1061 : For loop cannot be used to create n dimensional collection from inside a class";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "T43_Create_CollectioninForLoop_1457172_2.ds",err);

            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T44_Use_Bracket_Around_Range_Expr_In_For_Loop()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T44_Use_Bracket_Around_Range_Expr_In_For_Loop.ds");

            thisTest.Verify("s", 55);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T45_Defect_1458284()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T45_Defect_1458284.ds");

            thisTest.Verify("ids", new Object[] { 0.0, 5.0, 10.0, 15.0, 20.0 } );
        }
      
           
        


    }
}

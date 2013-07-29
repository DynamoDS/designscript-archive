using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    class TestScope
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string filePath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T001_LanguageBlockScope_AssociativeNestedAssociative()
            // It may not make sense to do so. Could treat as a negative case
        {

            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T001_LanguageBlockScope_AssociativeNestedAssociative.ds");

                //Assert.IsTrue((Int64)mirror.GetValue("a_inner", 2).Payload == 10);
                //Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner", 2).Payload) == true);
                //Assert.IsTrue((double)mirror.GetValue("c_inner", 2).Payload == 20.1);
            });
        }
        [Test]
        [Category ("SmokeTest")]
 public void T002_LanguageBlockScope_ImperativeNestedImperaive()
            // It may not make sense to do so. Could treat as a negative case
        {

            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T002_LanguageBlockScope_ImperativeNestedImperaive.ds");

                // Assert.IsTrue((Int64)mirror.GetValue("a_inner", 2).Payload == 10);
                // Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner", 2).Payload) == true);
                // Assert.IsTrue((double)mirror.GetValue("c_inner", 2).Payload == 20.1);
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T003_LanguageBlockScope_ImperativeNestedAssociative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T003_LanguageBlockScope_ImperativeNestedAssociative.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a_inner").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner").Payload == 20.1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T004_LanguageBlockScope_AssociativeNestedImperative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T004_LanguageBlockScope_AssociativeNestedImperative.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a_inner").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner").Payload == 20.1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T005_LanguageBlockScope_DeepNested_IAI()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T005_LanguageBlockScope_DeepNested_IAI.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a_inner1").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner1").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner1").Payload == 20.1);

            Assert.IsTrue((Int64)mirror.GetValue("a_inner2").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner2").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner2").Payload == 20.1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T006_LanguageBlockScope_DeepNested_AIA()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T006_LanguageBlockScope_DeepNested_AIA.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a_inner1").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner1").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner1").Payload == 20.1);

            Assert.IsTrue((Int64)mirror.GetValue("a_inner2").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner2").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner2").Payload == 20.1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T007_LanguageBlockScope_AssociativeParallelImperative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T007_LanguageBlockScope_AssociativeParallelImperative.ds");
           
            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T008_LanguageBlockScope_ImperativeParallelAssociative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T008_LanguageBlockScope_ImperativeParallelAssociative.ds");

            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T009_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_IA()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T009_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_IA.ds");


            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c").Payload) == false);

            Assert.IsTrue((double)mirror.GetValue("newA").Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("newB").Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("newC").Payload) == false);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T010_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_AI()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T010_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_AI.ds");

            
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c").Payload) == false);

            Assert.IsTrue((double)mirror.GetValue("newA", 0).Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("newB", 0).Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("newC", 0).Payload) == false);
   
        }


        [Test]
        [Category ("SmokeTest")]
 public void T011_LanguageBlockScope_AssociativeParallelAssociative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T011_LanguageBlockScope_AssociativeParallelAssociative.ds");

            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }



        [Test]
        [Category ("SmokeTest")]
 public void T012_LanguageBlockScope_ImperativeParallelImperative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T012_LanguageBlockScope_ImperativeParallelImperative.ds");

            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T013_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T013_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA.ds");

            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T014_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T014_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI.ds");

            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T015_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T015_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI2").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA2").Payload == 10);

            
        }
        [Test]
        [Category ("SmokeTest")]
 public void T016_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T016_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA2").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI2").Payload == 10);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T017_LanguageBlockScope_AssociativeNestedAssociative_Function()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T017_LanguageBlockScope_AssociativeNestedAssociative_Function.ds");
                // Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 30);           
            });
        }



        [Test]
        [Category ("SmokeTest")]
 public void T018_LanguageBlockScope_ImperativeNestedImperaive_Function()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T018_LanguageBlockScope_ImperativeNestedImperaive_Function.ds");
                // Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 30);
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T019_LanguageBlockScope_ImperativeNestedAssociative_Function()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T019_LanguageBlockScope_ImperativeNestedAssociative_Function.ds");
            thisTest.Verify("z", 10);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T020_LanguageBlockScope_AssociativeNestedImperative_Function()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T020_LanguageBlockScope_AssociativeNestedImperative_Function.ds");

            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 10);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T021_LanguageBlockScope_DeepNested_IAI_Function()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T021_LanguageBlockScope_DeepNested_IAI_Function.ds");
            thisTest.Verify("z_1", 10);
            thisTest.Verify("z_2", 0);
          
        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_LanguageBlockScope_DeepNested_AIA_Function()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T022_LanguageBlockScope_DeepNested_AIA_Function.ds");

            Assert.IsTrue((Int64)mirror.GetValue("z_1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("z_2").Payload == 0);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T023_LanguageBlockScope_AssociativeParallelImperative_Function()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T023_LanguageBlockScope_AssociativeParallelImperative_Function.ds");
            Assert.IsTrue(mirror.GetValue("z").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //Fuqiang: If function not found, it will return null and continues to execute.
            
            //Assert.Fail("1453777: Sprint 15: Rev 617: Scope: DS is able to call function defined in a parallel language block ");
            //HQ:
            //We need negative verification here. By design, we should not be able to call a function defined in a parallelled language block.
            //Should this script throw a compilation error here?
        }

        [Test]
        [Category ("SmokeTest")]
 public void T024_LanguageBlockScope_ImperativeParallelAssociative_Function()
        {
            
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T024_LanguageBlockScope_ImperativeParallelAssociative_Function.ds");
                Assert.IsTrue(mirror.GetValue("z").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }


        [Test]
        [Category ("SmokeTest")]
 public void T025_LanguageBlockScope_AssociativeParallelAssociative_Function()
        {

            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T025_LanguageBlockScope_AssociativeParallelAssociative_Function.ds");
                Assert.IsTrue(mirror.GetValue("z").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }


        [Test]
        [Category ("SmokeTest")]
 public void T026_LanguageBlockScope_ImperativeParallelImperative_Function()
        {

            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T026_LanguageBlockScope_ImperativeParallelImperative_Function.ds");
                Assert.IsTrue(mirror.GetValue("z").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }



        [Test]
        [Category ("SmokeTest")]
 public void T027_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA_Function()
        {

            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T027_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA_Function.ds");
                Assert.IsTrue(mirror.GetValue("z_1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                Assert.IsTrue(mirror.GetValue("z_2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }


        [Test]
        [Category ("SmokeTest")]
 public void T028_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI_Function()
        {

            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T028_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI_Function.ds");
                Assert.IsTrue(mirror.GetValue("z_1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
                Assert.IsTrue(mirror.GetValue("z_2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }



        [Test]
        [Category ("SmokeTest")]
 public void T029_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II_Function()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T029_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II_Function.ds");

            Assert.IsTrue((Int64)mirror.GetValue("z_I1").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("z_I2").Payload == -12);
            Assert.IsTrue((Int64)mirror.GetValue("z_A1").Payload == 18);
            Assert.IsTrue((Int64)mirror.GetValue("z_A2").Payload == 10);

        }


        [Test]
        [Category ("SmokeTest")]
 public void T030_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "T030_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA.ds");
            thisTest.Verify("z_A1", 18);

            thisTest.Verify("z_I1", 0);

            thisTest.Verify("z_A2", 10);

            thisTest.Verify("z_I2", -12);
            
           
         


        }

        //Defect Regress Test Cases

        [Test]
        [Category ("SmokeTest")]
 public void Z001_LanguageBlockScope_Defect_1453539()
        //1453539 - Sprint 15: Rev 585: Unexpected high memory usage when running a script with some comment out imperative codes
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\Z001_LanguageBlockScope_Defect_1453539.ds");
            
            //No verification needed, just need to run the case. 

        }

        [Test]
        [Category ("SmokeTest")]
 public void T031_Defect_1450594()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T031_Defect_1450594.ds");
            
            Assert.IsTrue(mirror.GetValue("f").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            
            Assert.IsTrue(mirror.GetValue("p").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("q").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("y1").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("y2").Payload == 3);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);    
         

        }

        [Test]
        [Category ("SmokeTest")]
 public void T032_Cross_Language_Variables()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Scope\\T032_Cross_Language_Variables.ds");

            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 11);
           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_01()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"~a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_02()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"`a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_03()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"!a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_04()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"#a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_05()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"$a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_06()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"%a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }
        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_07()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"^a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_08()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"&a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_09()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"*a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_10()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"(a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_11()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @")a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_12()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"-a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_13()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"=a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_14()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"+a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }
        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_15()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"?a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }
        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_16()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"[a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_17()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"]a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_18()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"{a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_19()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"}a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_20()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"\\a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }


        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_21()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"|a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_22()
        {
          String code =
             @";a = 1;";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);

          Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_23()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @":a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_24()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"\'a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_25()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"""a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_26()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @",a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_27()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @".a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_28()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"/a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_29()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"<a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }


        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_30()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @">a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_31()
        {
          
            String code =
             @"@a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Assert.IsTrue((Int64)mirror.GetValue("@a").Payload == 1);
         
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_32()
        {
          String code =
             @"_a = 1;";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);

          Assert.IsTrue((Int64)mirror.GetValue("_a").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_01()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a~ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_02()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a` = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_03()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a! = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_04()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a# = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }
        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_05()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a$ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_06()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a% = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_07()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a^ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }
        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_08()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a& = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_09()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a* = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }
        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_10()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a( = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_11()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a) = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_12()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a- = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_13()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a+ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_14()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a= = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_15()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a? = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_16()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a[ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_17()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a] = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_18()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a{ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_19()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a} = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_20()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a\\ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_21()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a| = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_22()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a; = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_23()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a: = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_24()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a\' = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_25()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a"" = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_26()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a< = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_27()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a> = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_28()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a/ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_29()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a, = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_30()
        {
          Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
          {
            String code =
             @"a. = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
          });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_31()
        {
          String code =
              @"a@ = 1;";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);

          Assert.IsTrue((Int64)mirror.GetValue("a@").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T050_Test_Identifier_Name_Tailer_32()
        {
          String code =
             @"a_ = 1;";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);

          Assert.IsTrue((Int64)mirror.GetValue("a_").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T051_Test_Identifier_Scope_01()
        {
          String code =
@"class A
{
  @a : var;
  constructor A(@b:var)
    {
        @a = @b;
    }
  def foo(@c:var)
  {
    @a = @c;
    return = @a;
  }
}
  @a = 1;
  p = A.A(2);
  @t = 3;
  @a = p.foo(@t);

";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);
          Assert.IsTrue((Int64)mirror.GetValue("@a").Payload == 3);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T051_Test_Identifier_Scope_02()
        {
          String code =
            @"
def foo(@a:var)
{
  @b = @a + 1;
  return = @b;
}
@t = 1;
@c = foo(@t);
";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);
          Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T051_Test_Identifier_Scope_03()
        {
          String code =
            @"
@c = [Imperative]
{
  @a = 10;
  @b = {10,20,30};
  for (@i in @b)
  {
      @a = @a + @i;
  }
  return = @a;
}
";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);
          Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 70);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T051_Test_Identifier_Scope_04()
        {
          String code =
            @"
@c = [Imperative]
{
  @a = 20;
  @b = 10;
  if(@a > 0){
    @a = @a - @b;
    @b = 20;
  }else
  {
    @b = 30;
  }
  return = @a;
}
";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);
          Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 10 );
        }

        [Test]
        [Category ("SmokeTest")]
 public void T051_Test_Identifier_Scope_05()
        {
          String code =
            @"
@b;
[Associative]
{
	@a = 1;
	@b = @a + 2;
	@a = 3;
          
}
";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);
          Assert.IsTrue((Int64)mirror.GetValue("@b").Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T051_Test_Identifier_Scope_06()
        {
          String code =
      @"
@c;
[Imperative]
{	
   @a = 1;
   @b = 2;
   @c = @a < @b ? @a : @b;			
}
                        
";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);
          Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T051_Test_Identifier_Scope_07()
        {
          String code =
      @"
@a;
[Imperative]
{	
   @a1 = 1;
   @a2 = 5;
   @a3 = 1;
   @a = @a1..@a2..@a3;
			
}
                        
";
          ExecutionMirror mirror = thisTest.RunScriptSource(code);
          List<Object> result = new List<Object> { 1,2,3,4,5 };
          Assert.IsTrue(mirror.CompareArrays("@a",result,typeof(System.Double)));
        }

        
        [Test]
        [Category("SmokeTest")]

        public void T052_DNL_1467464()
        {
            string code = @"
class test
{
    f;
    constructor test()
    {
    [Associative]
        {
            [Imperative]
            {
                i = 3;
            }
            f = i;
        }
    }
}

a = test.test();
b = a.f;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", null);
        }

    }
}

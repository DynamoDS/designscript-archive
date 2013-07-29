using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Associative
{
    public class Update
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();
        ProtoScript.Config.RunConfiguration runnerConfig; 
        string testCasePath = "..\\..\\..\\Scripts\\TD\\Associative\\Update\\";
        ProtoScript.Runners.DebugRunner fsr;
        [SetUp]
        public void Setup()
        {
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;

            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper()); 
            CLRModuleType.ClearTypes();
        }

        [Test]
        [Category ("SmokeTest")]
 public void T001_Simple_Update()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T001_Simple_Update.ds");

            //Verification
            thisTest.Verify("a", 2, 0);
            thisTest.Verify("b", 3, 0);         
        }

        [Test]
        [Category("Update")]
        public void T002_Update_Collection()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T002_Update_Collection.ds");

            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            //Verification
            thisTest.Verify("c", 14, 0);  
    
        }

        [Test]
        [Category ("SmokeTest")]
 public void T003_Update_In_Function_Call()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T003_Update_In_Function_Call.ds");

            //Verification
            Object[] v0 = new Object[] { 0, 1, 2, 3, 4 };            
            Object[] v1 = new Object[] { 2, 1, 2, 3, 4 };            

            thisTest.Verify("a", v0, 0);
            thisTest.Verify("d", v1, 0);
            thisTest.Verify("e1", v1, 0);

        }

        [Test]
        [Category("Update")]
        public void T004_Update_In_Function_Call_2()
        {
            string errmsg = "";//1467302 - rev 3778 : invalid cyclic dependency detected";
            
             thisTest.VerifyRunScriptFile(testCasePath, "T004_Update_In_Function_Call_2.ds",errmsg);

            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            
            //Verification
            Object[] v1 = new Object[] { 13, 12, 12, 13, 14 };
            Object[] v2 = new Object[] { 14, 13, 13, 14, 15 };
            Object[] v3 = new Object[] { 10, 12, 12, 13, 14 };

            thisTest.Verify("a", v3, 0);
            thisTest.Verify("e1", v1, 0);
            thisTest.Verify("b", v1, 0);
            thisTest.Verify("c", v2, 0);

        }

        [Test]
        [Category("Update")]
        public void T005_Update_In_collection()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T005_Update_In_collection.ds");

            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            
            //Verification
            Object[] v1 = new Object[] { 1, 2.6, 4 };          

            thisTest.Verify("collection", v1, 0);
            thisTest.Verify("b", 2.1, 0);
            thisTest.Verify("d", 2.7, 0);          

        }

        [Test]
        [Category ("SmokeTest")]
 public void T006_Update_In_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T006_Update_In_Class.ds");

            //Verification         
            thisTest.Verify("x1_start", 1.0, 0);
            thisTest.Verify("x1_end", 11.0, 0);  
            thisTest.Verify("x5_start", 5.0, 0);
            thisTest.Verify("x5_end", 15.0, 0); 

        }

        [Test]
        [Category("Update")]
        public void T007_Update_In_Class()
        {

            string error = "1460783 - Sprint 18 : Rev 1661 : Forward referencing is not being allowed in class property ( related to Update issue )";
            thisTest.VerifyRunScriptFile(testCasePath, "T007_Update_In_Class.ds", error);
            //Verification

        }

        [Test]
        [Category ("SmokeTest")]
 public void T008_Update_Of_Variables()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T008_Update_Of_Variables.ds");

            //Verification
            thisTest.Verify("b", 3, 0);
            Assert.IsTrue(mirror.GetValue("t2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);                  
        }

        [Test]
        [Category ("SmokeTest")]
 public void T009_Update_Of_Undefined_Variables()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T009_Update_Of_Undefined_Variables.ds");

            //Verification
            thisTest.Verify("u1", 3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T010_Update_Of_Singleton_To_Collection()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T010_Update_Of_Singleton_To_Collection.ds");

            //Verification
            Object[] v1 = new Object[] { 2, 3 };
            thisTest.Verify("s2", v1, 0);
        }

        [Test]
        [Category("Update")]
        public void T011_Update_Of_Variable_To_Null()
        {
            string error = "1458695 - Sprint 17: Rev 1316: Handle divide by 0 (infinity and beyond back to origin";
            thisTest.VerifyRunScriptFile(testCasePath, "T011_Update_Of_Variable_To_Null.ds", error);
            thisTest.Verify("y", null);
            thisTest.Verify("v2", null);

        
        }

        [Test]
        [Category ("SmokeTest")]
 public void T012_Update_Of_Variables_To_Bool()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T012_Update_Of_Variables_To_Bool.ds");

            //Verification         
            Assert.IsTrue(mirror.GetValue("p2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("q2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("s2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("t2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("r2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        
        }

        [Test]
        [Category ("SmokeTest")]
 public void T013_Update_Of_Variables_To_User_Defined_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T013_Update_Of_Variables_To_User_Defined_Class.ds");

            //Verification      
            Assert.IsTrue(mirror.GetValue("t2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("r2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        
        }

        [Test]
        [Category ("SmokeTest")]
 public void T014_Update_Of_Class_Properties()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T014_Update_Of_Class_Properties.ds");

            //Verification      
            thisTest.Verify("c1", 4, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T015_Update_Of_Class_Properties()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T015_Update_Of_Class_Properties.ds");

            //Verification   
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("c1", v1, 0);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T016_Update_Of_Variable_Types()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T016_Update_Of_Variable_Types.ds");

            //Verification   
            thisTest.Verify("y", 2, 0);
        }

        [Test]
        [Category("Update")]
        public void T017_Update_Of_Class_Instances()
        {
           thisTest.RunScriptFile(testCasePath, "T017_Update_Of_Class_Instances.ds");
            //Assert.Fail("1467219 - sprint25: rev 3339 : False cyclic dependency with class update");
            thisTest.Verify("aX", 1.0);
            thisTest.Verify("aY", 5.0);
            
            /*
            //ExecutionMirror mirror = thisTest.RunScript(testCasePath, "T017_Update_Of_Class_Instances.ds");
            string src =
                Path.GetFullPath(string.Format("{0}{1}", testCasePath, "T017_Update_Of_Class_Instances.ds")); 
            
            fsr.LoadAndPreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp1 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 25,
                LineNo = 56,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            ProtoCore.CodeModel.CodePoint cp2 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 25,
                LineNo = 57,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            ProtoCore.CodeModel.CodePoint cp3 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 25,
                LineNo = 58,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };

            fsr.ToggleBreakpoint(cp1);
            ProtoScript.Runners.DebugRunner.VMState vms = fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "aX", 1.0);
            thisTest.DebugModeVerification(vms.mirror, "aY", 2);

            fsr.ToggleBreakpoint(cp2);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "aX", 1);
            thisTest.DebugModeVerification(vms.mirror, "aY", 2.0);

            fsr.ToggleBreakpoint(cp3);
            fsr.Run();
          
            thisTest.DebugModeVerification(vms.mirror, "aX", 4.0);
            thisTest.DebugModeVerification(vms.mirror, "aY", 2);

            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "aX", 1);
            thisTest.DebugModeVerification(vms.mirror, "aY", 5.0);

            */
        }

        [Test]
        [Category("Update")]
        public void T018_Update_Inside_Class_Constructor()
        {
            // Assert.Fail("1467220 - sprint25: rev 1399 : Update of class properties inside class methods is not happening"); 
            
           thisTest.RunScriptFile(testCasePath, "T018_Update_Inside_Class_Constructor.ds");

            //Verification   
            thisTest.Verify("z", 9.0, 0);
        }

        [Test]
        [Category("Update")]
        public void T018_Update_Inside_Class_Constructor_2()
        {
            string errmesg = "1467220 - sprint25: rev 1399 : Update of class properties inside class methods is not happening"; 

            thisTest.VerifyRunScriptFile(testCasePath, "T018_Update_Inside_Class_Constructor_2.ds",errmesg);

            //Verification   
            thisTest.Verify("z", 7.0, 0);// though i expected 9, but it is as designed as of now . 
            //Refer to  :DNL-1467220 sprint25: rev 1399 : Update of class properties inside class methods is not happening
        }

        [Test]
        [Category("Update")]
        public void T019_Update_General()
        {
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case"); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T019_Update_General.ds");

            //Verification  
            thisTest.Verify("test", 7, 0);

        }

        [Test]
        [Category("Update")]
        public void T020_Update_Inside_Class_Constructor()
        {
            //Assert.Fail("1467072 - Sprint23 : rev 2650 : Update issue: Class properties not getting updated in class  constructor"); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T020_Update_Inside_Class_Constructor.ds");

            //Verification  
            thisTest.Verify("z", 9.0, 0);

        }

        [Test]
        [Category("Update")]
        public void T021_Update_Inside_Class_Constructor()
        {
            string err = "1467220 - sprint25: rev 1399 : Update of class properties inside class methods is not happening";

            thisTest.VerifyRunScriptFile(testCasePath, "T021_Update_Inside_Class_Constructor.ds", err);

            //Verification  
            thisTest.Verify("z", 9.0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_Defect_1459905()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T022_Defect_1459905.ds");

            //Verification   
            thisTest.Verify("a", 1, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_Defect_1459905_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T022_Defect_1459905_2.ds");

            //Verification   
            thisTest.Verify("a1", 2, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_Defect_1459905_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T022_Defect_1459905_3.ds");

            //Verification   
            thisTest.Verify("b1", 1, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_Defect_1459905_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T022_Defect_1459905_4.ds");

            //Verification   
            thisTest.Verify("y", 2, 0);

        }
        

        [Test]
        [Category ("SmokeTest")]
 public void T023_Defect_1459789()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Defect_1459789.ds");

            //Verification   
            thisTest.Verify("aY", 2.0, 0);
            thisTest.Verify("aX", 2.0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T023_Defect_1459789_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Defect_1459789_2.ds");

            //Verification   
            thisTest.Verify("aY", 4.0, 0);
            thisTest.Verify("aX", 4.0, 0);

        }

        [Test]        
 public void T023_Defect_1459789_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Defect_1459789_3.ds");
            //Assert.Fail("1467187 - Sprint24: REGRESSION : rev 3177: When a class collection property is updated, the value is not reflected");

            //Verification   
            thisTest.Verify("test1", 2.0, 0);      

        }

        [Test]        
 public void T023_Defect_1459789_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Defect_1459789_4.ds");
            Object[] v1 = new Object[] { 1.0, 2.0 };
            //Verification   
            thisTest.Verify("test1", v1);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T023_Defect_1459789_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Defect_1459789_5.ds");
            
            //Verification   
            thisTest.Verify("test1", 0.0);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T023_Defect_1459789_6()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Defect_1459789_6.ds");

            //Verification   
            thisTest.Verify("test1", 0.0);


        }

        [Test]

        [Category("Update")]
        public void T023_Defect_1459789_7()
        {
            string err = "1467061 - Sprint 23 : rev 2587 : Update issue : When a class instance is updated from imperative scope the update is not happening as expected in some cases";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "T023_Defect_1459789_7.ds",err);
            

            //Verification   
            thisTest.Verify("test1", 0.0);


        }

        [Test]

        [Category("Update")]
        public void T023_Defect_1459789_8()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Defect_1459789_8.ds");
            //Assert.Fail("1467116 Sprint24 : rev 2806 : Cross language update issue");

            //Verification   
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kCyclicDependency);


        }

        [Test]

        [Category("Update")]
        public void T023_Defect_1459789_9()
        {
            string err = "";//1466768 - VALIDATION NEEDED - Sprint 23 : rev 2479 : global variables are not getting updated through function calls";
            thisTest.VerifyRunScriptFile(testCasePath, "T023_Defect_1459789_9.ds",err);            

            //Verification   
            thisTest.Verify("test1", 0.0);



        }

        [Test]       
 public void T023_Defect_1459789_10()
        {
            string err = "1467186 - sprint24 : REGRESSION: rev 3172 : Cyclic dependency detected in update cases";
            thisTest.VerifyRunScriptFile(testCasePath, "T023_Defect_1459789_10.ds", err);
       

            //Verification   
            thisTest.Verify("test1", 3.0);


        }



        [Test]
        [Category("Update")]
        public void T024_Defect_1459470()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T024_Defect_1459470.ds");

            //Verification   
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            thisTest.Verify("c", 14);
            


        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_2()
        {
            string errmsg = "";//DNL-1467335 Rev 3971 : REGRESSION : Update not happening inside class constructor";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "T024_Defect_1459470_2.ds",errmsg);

            //Verification   
            thisTest.Verify("c1", 2);

        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_3()
        {
            string errmsg = "DNL-1467337 Rev 3971 : Update of global variables from inside function call is not happening as expectedr";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "T024_Defect_1459470_3.ds", errmsg);

            //Verification   
            thisTest.Verify("y", 2);

        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_4()
        {
            string errmsg = "";//DNL-1467337 Rev 3971 : Update of global variables from inside function call is not happening as expectedr";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "T024_Defect_1459470_4.ds", errmsg);

            //Verification   
            thisTest.Verify("c", 5);
            thisTest.Verify("d", 3);

        }

        [Test]
        [Category("Update")]
 public void T025_Defect_1459704()
        {
            string err = "1467186 - sprint24 : REGRESSION: rev 3172 : Cyclic dependency detected in update cases";
            thisTest.VerifyRunScriptFile(testCasePath, "T025_Defect_1459704.ds",err);
            
            //Verification   
            thisTest.Verify("a", 3, 0);
            thisTest.Verify("c", 3, 0);

        }

        [Test]
        [Category ("Update")]
 public void T025_Defect_1459704_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T025_Defect_1459704_2.ds");
            //Assert.Fail("1467072 - Sprint23 : rev 2650 : Update issue: Class properties not getting updated in class  constructor");
            //Verification   
            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", 11, 0);

        }

        [Test]
        [Category("Update")]
        public void T026_Defect_1459631()
        {
            string err = "1460783 - Sprint 18 : Rev 1661 : Forward referencing is not being allowed in class property ( related to Update issue )";
            
           thisTest.VerifyRunScriptFile(testCasePath, "T026_Defect_1459631.ds",err);

            //Verification   
            thisTest.Verify("t1", 2, 0);
            thisTest.Verify("t2", 3, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T026_Defect_1459631_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T026_Defect_1459631_2.ds");

            //Verification   
            thisTest.Verify("t1", 2, 0);
            thisTest.Verify("t2", 6, 0);
            thisTest.Verify("t3", 8, 0);
            thisTest.Verify("t4", 4, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T026_Defect_1459631_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T026_Defect_1459631_3.ds");
            Object[] v1 = new Object[] { 8, 8, 8 } ;

            //Verification   
            thisTest.Verify("t1", 2, 0);
            thisTest.Verify("t2", 6, 0);
            thisTest.Verify("t3", v1, 0);
            thisTest.Verify("t4", 8, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T027_Defect_1460741()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T027_Defect_1460741.ds");
            Object[] v1 = new Object[] { 3, 4 };

            //Verification   
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v1);
            thisTest.Verify("x3", v1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T027_Defect_1460741_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T027_Defect_1460741_2.ds");
            Object[] v1 = new Object[] { 3, 4 }; 
            Object[] v2 = new Object[] { 1.0, 2.0 };

            //Verification   

            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v2);
            thisTest.Verify("x3", v2);
            thisTest.Verify("x4", v2);
        }

        [Test]
        [Category ("SmokeTest")]
        public void T028_Modifier_Stack_Simple()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T028_Modifier_Stack_Simple.ds");
            
            thisTest.Verify("a", 9);
        }  

        [Test]
        [Category ("Update")]
        public void T029_Defect_1460139_Update_In_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T029_Defect_1460139_Update_In_Class.ds");

            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case");
            
            thisTest.Verify("test", 7);
        }

        [Test]
        [Category("Update")]
        public void T030_Defect_1467236_Update_In_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T030_Defect_1467236_Update_In_Class.ds");
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 9);
        }

        [Test]
        [Category("Update")]
        public void T030_Defect_1467236_Update_In_Class_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T030_Defect_1467236_Update_In_Class_2.ds");
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 9);
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302()
        {
            String code =
@"
def foo ( a : int[] ) 
{
    b = a;
    b[0] = b[1] + 1;
    return = b;
}

a = { 0, 1, 2};
e1 = foo(a);
a = { 1, 2};
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { 3, 2 });
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302_2()
        {
            String code =
@"
def foo ( a : int[] ) 
{
    b = a;
    b[0] = b[1] + 1;
    return = b;
}

i = 1..2;
a = { 0, 1, 2, 3};
e1 = foo(a[i]);
i = 0..2;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { 2, 1, 2 });
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302_3()
        {
            String code =
@"
class A
{
    b : int[] = { 0, 1, 2, 3 };
    
    def foo (i:int ) 
    {
        b[i] = b[i] + 1;
        return = b;
    }

}

i = 1..2;
e1 = A.A().foo(i);
i = 0..2;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { new Object[] { 1, 1, 2, 3 }, new Object[] { 1, 2, 2, 3 }, new Object[] { 1, 2, 3, 3 } });
        }

        [Test]
        [Category("Update")]
        public void T032_Defect_1467335_Update_In_class_Constructor()
        {
            String code =
@"
class A
{
    b : int[] = { 0, 1, 2, 3 };
    
    def foo (i:int ) 
    {
        b[i] = b[i] + 1;
        return = b;
    }

}

i = 1..2;
e1 = A.A().foo(i);
i = 0..2;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { new Object[] { 1, 1, 2, 3 }, new Object[] { 1, 2, 2, 3 }, new Object[] { 1, 2, 3, 3 } });
        }

        [Test]
        [Category("Update")]
        public void T033_Defect_1467187_Update_In_class_collection_property()
        {
            String code =
@"
class Point
{
    X : double;
    
    constructor ByCoordinates( x : double )
    {
        X = x;
    
    } 
def foo ( y )
{
    [Imperative]
    {
        X = y + 1;
    }
    return = true;
}    
    
}

p1 = Point.ByCoordinates(1);
test = p1.X;
dummy = p1.foo(2);
//expected test to update to '3'
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", 3.0);
        }

        [Test]
        [Category("Update")]
        public void T033_Defect_1467187_Update_In_class_collection_property_2()
        {
            String code =
@"
class B
{    
    a1 : int;
    a2 : double[];
    constructor B (a:int, b : double[])    
    {        
        a1 = a;
        a2 = b;
    }
}
b1 = B.B ( 1, {1.0, 2.0} );
test1 = b1.a2[0];
b1.a2[0] = b1.a2[1];
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", 2.0);
        }

        [Test]
        [Category("Update")]
        public void T033_Defect_1467187_Update_In_class_collection_property_3()
        {
            String code =
@"
class Point
{
    X : double[];
    
    constructor ByCoordinates( x : double[] )
    {
        X = x;
    
    } 
    def foo ( y :double[])
    {
        X = y + 1;
        [Imperative]
        {
           count = 0;
           for (i in y)
           {
               X[count] = y[count] + X[count];
               count = count + 1;
           }
        }
        return = true;
    }    
    
}

p1 = Point.ByCoordinates({0,0,0,0});
test = p1.X;
dummy = p1.foo({1,1,1,1});
p1.X[0..1] = -1;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "DNL-1467401 Rev 4347 : Update issue with collection property in nested imperative scope";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { -1, -1, 2, 2 } );
        }

        [Test]
        [Category("Update")]
        public void T034_UpdaetStaticProperty()
        {
            string code = @"
class Base
{
    static x : int[];
}

b = Base.Base();
t = b.x;               // x is a static property
Base.x = { 5.2, 3.9 }; // expect t = {5, 4}, but t = {null}
";
            string errmsg = "DNL-1467431 Modify static property doesn't trigger update";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t", new object[] { 5, 4 });
        }

        [Test]
        [Category("Update")]
        public void T035_FalseCyclicDependency()
        {
            string code = @"
def foo()
{
    a = b;
    return = null;
}

def bar()
{
    b = a;
    return = null;
}

a = 1;
b = 0;
r = bar();
q = a;
";
            string errmsg = "DNL-1467336 Rev 3971 :global and local scope identifiers of same name causing cyclic dependency issue";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("q", 1);
        }

        [Test]
        [Category("Update")]
        public void T036_Defect_1467491()
        {
            string errmsg = "DNL-1467336 Rev 3971 :global and local scope identifiers of same name causing cyclic dependency issue";
            thisTest.RunScript(testCasePath + "T031_Defect_1467491_ImportUpdate_Main.ds", errmsg);
            thisTest.Verify("z", 6);
            
        }
    }
}

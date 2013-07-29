using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    class TestClass
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Class\\";


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T01_Class_In_Various_Scopes()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_Class_In_Various_Scopes.ds");
          
        }

        [Test]
        [Category ("SmokeTest")]
 public void T02_Class_In_Various_Nested_Scopes()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T02_Class_In_Various_Nested_Scopes.ds");

        }

        [Test]
        [Category ("SmokeTest")]
 public void T03_Class_In_Various_Scopes()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T03_Class_In_Various_Scopes.ds");

        }

        [Test]
        [Category ("SmokeTest")]
 public void T04_Class_Properties()
        {
            Object[] t1 = new Object[] { 1, 2 };
            object t6 =null;
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T04_Class_Properties.ds");
            thisTest.Verify("t1", t1, 0);
            thisTest.Verify("t2", 1, 0);
            thisTest.Verify("t3", 1.5, 0);
            thisTest.Verify("t4", true, 0);
            thisTest.Verify("t6", t6, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T05_Class_Properties()
        {
            Object[] t1 = new Object[] { new Object[] {1}, new Object[] {2} };
            Object[] t2 = new Object[] { 1, 2 };
            Object[] t3 = new Object[] { 1.5, 2.5 };
            Object[] t4 = new Object[] { true, false }; 
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T05_Class_Properties.ds");

            thisTest.Verify("t1", t1, 0);
            thisTest.Verify("t2", t2, 0);
            thisTest.Verify("t3", t3, 0);
            thisTest.Verify("t4", t4, 0);
            thisTest.Verify("t6", 1, 0);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T06_Class_Properties()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T06_Class_Properties.ds");
            thisTest.Verify("t1", 5, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T07_Class_Properties()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T07_Class_Properties.ds");

        }

        [Test]
        [Category ("SmokeTest")]
 public void T08_Class_Properties()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T08_Class_Properties.ds");
        }


        [Test]
        [Category ("SmokeTest")]
 public void T09_Class_Inheritance()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T09_Class_Inheritance.ds");

            thisTest.Verify("b1", 1, 0);
            thisTest.Verify("b2", 1.5, 0);
            thisTest.Verify("b3", 2, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T10_Class_Inheritance()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T10_Class_Inheritance.ds");
            thisTest.Verify("b1", 1);
            thisTest.Verify("b2", 1.5);
            thisTest.Verify("b3", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T11_Class_Inheritance()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T11_Class_Inheritance.ds");

            thisTest.Verify("b1", new object[] {1, 1.5});
            thisTest.Verify("b2", 1);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T12_Class_CheckPropertyWhenCreatedInImperativeCode()
        {
            String code =
            @"class Dummy
{
 x : var;
 
 constructor Dummy ()
 {
	x = 2;	
 }
}

obj = [Imperative]{
return = Dummy.Dummy();
}
getX = obj.x;;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Verification
            object getX = 2;
            thisTest.Verify("getX", getX, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T13_Class_Default_Constructors()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T13_Class_Default_Constructors.ds");
            
            thisTest.Verify("x1", 9, 0);
            thisTest.Verify("z1", false, 0);
            Object a = new object[] { };
            thisTest.Verify("p1", a);
            thisTest.Verify("y1", null);
            thisTest.Verify("w1", null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T14_Class_Named_Constructors()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T14_Class_Named_Constructors.ds");

            thisTest.Verify("x1", 1, 0);
            thisTest.Verify("x2", 2, 0);
            thisTest.Verify("x3", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T15_Class_Constructor_Negative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T15_Class_Constructor_Negative.ds");
            Object v1 = null;

            thisTest.Verify("x1", v1);
            thisTest.Verify("xx", v1);
            thisTest.Verify("x2", 3);
        }
        [Test]
        [Category("SmokeTest")]
        public void T15_Class_Constructor_Negative_1467598()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T15_Class_Constructor_Negative.ds");
            Object v1 = null;

            thisTest.Verify("x1", v1);
            thisTest.Verify("xx", v1);
            thisTest.Verify("x2", 3);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T16_Class_Constructor_Negative()
        {
           Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
           {
               ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T16_Class_Constructor_Negative.ds");
           });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T17_Class_Constructor_Negative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T17_Class_Constructor_Negative.ds");

        }

        [Test]
        [Category ("SmokeTest")]
 public void T18_Class_Constructor_Empty()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Class\\T18_Class_Constructor_Empty.ds");
                      
            thisTest.Verify("x1", 1, 0);
            thisTest.Verify("x2", 2, 0);
            Assert.IsTrue(mirror.GetValue("x3").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T19_Class_Constructor_Test_Default_Property_Values()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T19_Class_Constructor_Test_Default_Property_Values.ds");

            thisTest.Verify("x1", null);
            thisTest.Verify("x2", 0);
            thisTest.Verify("x3", false);
            thisTest.Verify("x4", 0.0);
            thisTest.Verify("x5", null);
            Object a = new object[] { };
            thisTest.Verify("x6", a);
            thisTest.Verify("x7", a);
            thisTest.Verify("x8", a);
            thisTest.Verify("x9", a);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T20_Class_Constructor_Fails()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Class\\T20_Class_Constructor_Fails.ds");

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
            Assert.IsTrue(mirror.GetValue("b1").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);          

        }

        [Test]
        [Category ("SmokeTest")]
 public void T21_Class_Constructor_Calling_Base_Constructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T21_Class_Constructor_Calling_Base_Constructor.ds");

            thisTest.Verify("c1", 1, 0);
            thisTest.Verify("c2", 2, 0);
            thisTest.Verify("c3", 3, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T22_Class_Constructor_Not_Calling_Base_Constructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T22_Class_Constructor_Not_Calling_Base_Constructor.ds");

            thisTest.Verify("c1", 2, 0);
            thisTest.Verify("c2", 2, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T23_Class_Constructor_Base_Constructor_Same_Name()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T23_Class_Constructor_Base_Constructor_Same_Name.ds");

            thisTest.Verify("c1", 2, 0);
            thisTest.Verify("c2", 2, 0);

            thisTest.Verify("b1", 1, 0);
            thisTest.Verify("b2", 2, 0);


            thisTest.Verify("d1", 1, 0);
            thisTest.Verify("d2", 2, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T24_Class_Constructor_Calling_Base_Methods()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T24_Class_Constructor_Calling_Base_Methods.ds");

            thisTest.Verify("c1", 1, 0);
            thisTest.Verify("c2", 2, 0);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T25_Class_Properties_Modifiers()
        {
            // Assert.Fail("1467247 - Sprint25: rev 3448 : REGRESSION : Runtime exception thrown when setting a private property of a class instance");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T25_Class_Properties_Modifiers.ds");
            thisTest.Verify("a1", 3, 0);
            thisTest.Verify("a3", 4, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T26_Class_Properties_Access()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T26_Class_Properties_Access.ds");
            
            thisTest.Verify("a11", 2);
            thisTest.Verify("a12", 1);
            thisTest.Verify("a2", 3);          
            thisTest.Verify("a3", 2);
            thisTest.Verify("a4", 4);
            thisTest.Verify("a5", 1);            

        }

        [Test]
        [Category ("SmokeTest")]
 public void T27_Class_Properties_Access()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T27_Class_Properties_Access.ds");

            thisTest.Verify("aa", 7);
            thisTest.Verify("a2", 15);  
        }

        [Test]
        [Category("Method Resolution")]
        public void T28_Class_Properties_Access()
        {
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T28_Class_Properties_Access.ds");
            
            thisTest.Verify("f1", 8);
            thisTest.Verify("v1", 26);
            thisTest.Verify("f2", 4);
        }

        [Test]
        public void T29_Class_Method_Chaining()
        {

            //Assert.Fail("1454966 - Sprint15: Rev 720 : [Geometry Porting] Assignment operation where the right had side is Class.Constructor.Property is not working ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T29_Class_Method_Chaining.ds");
            
            thisTest.Verify("t1", 2, 0);
                    

        }
       
        
        [Test]
        [Category ("SmokeTest")]
 public void T30_Class_Property_Update_Negative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T30_Class_Property_Update_Negative.ds");
            TestFrameWork.Verify(mirror, "x1", null);
            TestFrameWork.Verify(mirror, "x2", null);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T31_Class_By_Composition()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T31_Class_By_Composition.ds");

            thisTest.Verify("x1", 7.5, 0); 
        }

        [Test]
        [Category ("SmokeTest")]
 public void T32_Class_ReDeclaration()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T32_Class_ReDeclaration.ds");
            });

            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T33_Class_Methods()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\MultiLanguage\\Class\\T33_Class_Methods.ds");

            Assert.IsTrue(mirror.GetValue("x2").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("x5").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            thisTest.Verify("x1", 1.0, 0);
            thisTest.Verify("x3", 1.0, 0);
            thisTest.Verify("x4", 1, 0);
            thisTest.Verify("x6", 1.0, 0); 

        }

        [Test]
        [Category ("SmokeTest")]
 public void T34_Class_Static_Methods()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T34_Class_Static_Methods.ds");
            Object v = null;

            thisTest.Verify("x3", v);
            thisTest.Verify("x4", 8.0);
            thisTest.Verify("x5", 1);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T35_Class_Method_Overloading()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T35_Class_Method_Overloading.ds");

            thisTest.Verify("a1", 17.0, 0);
            thisTest.Verify("a2", 35.0, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T36_Class_Method_Calling_Constructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T36_Class_Method_Calling_Constructor.ds");

            thisTest.Verify("a2", 6.0, 0);
            thisTest.Verify("a3", 11.0, 0);
            thisTest.Verify("b2", 30.0, 0);
            thisTest.Verify("b3", 40.0, 0);
            thisTest.Verify("b4", 30.0, 0);
            thisTest.Verify("b5", 40.0, 0); 
        }

        [Test]
        [Category ("SmokeTest")]
 public void T37_Class_Method_Using_This()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T37_Class_Method_Using_This.ds");
                thisTest.Verify("a2", 5.0, 0);
                thisTest.Verify("a3", 10.0, 0);
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T38_Class_Method_Using_This()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T38_Class_Method_Using_This.ds");
            thisTest.Verify("a2", 9.0, 0);
            thisTest.Verify("a3", 14.0, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T39_Class_Method_Returning_Collection()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T39_Class_Method_Returning_Collection.ds");
            thisTest.Verify("a2", 9.0, 0);
            thisTest.Verify("a3", 14.0, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T40_Class_Property_Initialization_With_Another_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T40_Class_Property_Initialization_With_Another_Class.ds");
            thisTest.Verify("t1", 3.0, 0);            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T41_Test_Static_Properties()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T41_Test_Static_Properties.ds");
            thisTest.Verify("t1", 3, 0);
            thisTest.Verify("t2", 3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T41_Test_Static_Properties_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T41_Test_Static_Properties_2.ds");
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("b", v1, 0);            
        }
   
        [Test]
        [Category ("SmokeTest")]
 public void T42_Defect_1461717()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T42_Defect_1461717.ds");
             });                  
        }
        
        [Test]
        [Category ("SmokeTest")]
 public void T43_Defect_1461479()
        {
             ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T43_Defect_1461479.ds");
             thisTest.Verify("x2", 1, 0);                            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T43_Defect_1461479_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T43_Defect_1461479_2.ds");
            thisTest.Verify("t1", 5, 0);
        }

        [Test]
        [Category("Update")]
 public void T43_Defect_1461479_3()
        {
            Assert.Fail("1461984 - Sprint 19 : Rev 1880 : Update issue with static properties "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T43_Defect_1461479_3.ds");
                      
            Object[] v1 = new Object[] { 7, 8 }; 
            
            thisTest.Verify("x2", 7, 0);
            thisTest.Verify("t2", 7, 0);           
            thisTest.Verify("t3", 8, 0);
            thisTest.Verify("t4", 7, 0);
            thisTest.Verify("t5", 6, 0);
            thisTest.Verify("t6", v1, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T43_Defect_1461479_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T43_Defect_1461479_4.ds");
          
            thisTest.Verify("b", 4, 0);
            thisTest.Verify("c", 4, 0);
            thisTest.Verify("d", 4, 0);           
        }


        [Test]
        [Category ("SmokeTest")]
 public void T44_Defect_1461860()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T44_Defect_1461860.ds");
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T44_Defect_1461860_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T44_Defect_1461860_2.ds");
            Object[] v1 = new Object[] { 1, 1 };
            thisTest.Verify("c", v1);
        }  
        
        [Test]
        [Category ("SmokeTest")]
 public void T46_Defect_1461716()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T46_Defect_1461716.ds");
            Object v1 = null;
            
            thisTest.Verify("b1", v1);
            thisTest.Verify("c1", v1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T46_Defect_1461716_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T46_Defect_1461716_2.ds");
            Object v1 = null;
            Object[] v2 = new Object[] { null, null };

            thisTest.Verify("b1", v1);
            thisTest.Verify("c1", v1);
            thisTest.Verify("x", v2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T47_Calling_Imperative_Code_From_Conctructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T47_Calling_Imperative_Code_From_Conctructor.ds");           

            //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Imperative code is not allowed in class constructor ");

            thisTest.Verify("a1", 1);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T48_Defect_1460027()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T48_Defect_1460027.ds");

            thisTest.Verify("b1", 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T48_Defect_1460027_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T48_Defect_1460027_2.ds");

            thisTest.Verify("b1", 4);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T48_Defect_1460027_3()
        {
            //Assert.Fail("1463700 - Sprint 20 : rev 2147 : Update is not happening when a collection property of a class instance is updated using a class method ");

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T48_Defect_1460027_3.ds");
            Object[] expected = new Object[] { 4, 5, 6 };

            thisTest.Verify("b", expected, 0);
            thisTest.Verify("c", 4, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T48_Defect_1460027_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T48_Defect_1460027_4.ds");
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kCyclicDependency);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T49_Defect_1460505()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T49_Defect_1460505.ds");

            thisTest.Verify("basePoint", true, 0);
            thisTest.Verify("derivedPoint2", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1460505_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T49_Defect_1460505_2.ds");
            Object n1 = null;
            thisTest.Verify("basePoint", n1, 0);
            thisTest.Verify("derivedPoint2", true, 0);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T50_Defect_1460510()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T50_Defect_1460510.ds");
                });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T50_Defect_1460510_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T50_Defect_1460510_2.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T51_Defect_1461399()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T51_Defect_1461399.ds");
            Object[] ExpectedResult = new Object[] { null, null, null };
            thisTest.Verify("test", ExpectedResult);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T51_Defect_1461399_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T51_Defect_1461399_2.ds");
            Object[] ExpectedResult = new Object[] { null, null, null };
            thisTest.Verify("test", ExpectedResult);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T51_Defect_1461399_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T51_Defect_1461399_3.ds");
            Object[] ExpectedResult = new Object[] { null, null, null };
            thisTest.Verify("test", ExpectedResult);
        }
  
        [Test]
        [Category ("SmokeTest")]
 public void T52_Defect_1461479()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Defect_1461479.ds");

            Assert.IsTrue(mirror.GetValue("a", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("b", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T52_Defect_1461479_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Defect_1461479_2.ds");

            Assert.IsTrue(mirror.GetValue("test1", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("test2", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T52_Defect_1461479_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Defect_1461479_3.ds");

            thisTest.Verify("test1", 2, 0);
            thisTest.Verify("test2", 2, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T52_Defect_1461479_4()
        {
            // Assert.Fail("1463741 Sprint 20 : Rev 2150 : Error: Non-Static members are accessible via static calls");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Defect_1461479_4.ds");
            Object v1 = null;
           
            thisTest.Verify("test1", v1);
            thisTest.Verify("test2", v1);
            thisTest.Verify("test3", v1);
            thisTest.Verify("test4", v1);
            thisTest.Verify("test5", v1);
            thisTest.Verify("test6", v1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T52_Defect_1461479_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T52_Defect_1461479_5.ds");
            Object v1 = null;
            Object[] v2 = new Object[] { null, null };

            thisTest.Verify("test1", v1, 0);
            thisTest.Verify("test2", v1, 0);
            thisTest.Verify("test3", v1, 0);
            thisTest.Verify("test4", v1, 0);

            thisTest.Verify("test5", v2, 0);
            thisTest.Verify("test6", v2, 0);
            thisTest.Verify("test7", v2, 0);
            thisTest.Verify("test8", v2, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter.ds"); 
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);
            Assert.IsTrue(mirror.GetValue("basePoint", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("derivedPoint2", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_1463738.ds"); 
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);
            Assert.IsTrue(mirror.GetValue("basePoint", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("derivedPoint2", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_1463738_2.ds");
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);
            Assert.IsTrue(mirror.GetValue("basePoint", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("derivedPoint2", 0).DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);

        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_3()
        {

            //Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_1463738_3.ds");
            Object n1 = null;
            thisTest.Verify("x1", n1);
            thisTest.Verify("x2", 9);
            

        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_4()
        {

            //Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_1463738_4.ds");
            
            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);
           

        }
        /*
         going in infinite loop due to defect 1467097
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_5()
        {

            ExecutionMirror mirror = thisTest.RunScript(testPath, "T53_Undefined_Class_As_Parameter_1463738_5.ds");

            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);


        }*/
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_6()
        {

            //Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_1463738_6.ds");
            
            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);


        }
        [Test]
        public void T53_Undefined_Class_As_Parameter_1463738_7()
        {

            //Assert.Fail("1467097 - Sprint 24 - Rev 2761 - if var is used as a argument to function and call function with defined class it goes into a loop and hangs DS ");
            Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_1463738_7.ds");
            
            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);


        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_8()
        {

            //Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_1463738_8.ds");
            
            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);


        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_imperative_1463738_9()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_As_Parameter_imperative_1463738_9.ds");

            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);



        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_1467107_10()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_negative_1467107_10.ds");
            object a = null;
            thisTest.Verify("y2", a);


        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_imperative_1467107_11()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_negative_imperative_1467107_11.ds");
            object a = null;
            thisTest.Verify("y2", a);


        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_imperative_1467091_12()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_negative_imperative_1467091_12.ds");
            object a = null;
            thisTest.Verify("y", a);


        }
        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_associative_1467091_13()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T53_Undefined_Class_negative_associative_1467091_13.ds");
            object a = null;
            thisTest.Verify("y", a);


        }
        
        // Jun: To address on dot op defect
        [Test]       
 public void T55_Defect_1460616()
        {
            //Assert.Fail("1467189 - Sprint24 : rev 3181 : REGRESSION : DS goes into infinite loop with class having 'this' pointer");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T55_Defect_1460616.ds");
            Object v1 = null;

            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v1);
            thisTest.Verify("x3", v1);  
            

        }

        [Test]
        public void TV55_Defect_1460616_1()
        {
            Assert.Fail("1467189 - Sprint24 : rev 3181 : REGRESSION : DS goes into infinite loop with class having 'this' pointer");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "TV55_Defect_1460616_1.ds");
            Object v1 = null;

            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v1);
            thisTest.Verify("x3", v1);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T55_Defect_1460616_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T55_Defect_1460616_2.ds");
            Object n1 = null;
            thisTest.Verify("x3", n1);

        }
        
        [Test]
        [Category ("SmokeTest")]
 public void T56_Local_Class_method_Same_Name_As_Global_Function()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T56_Local_Class_method_Same_Name_As_Global_Function.ds");
            thisTest.Verify("x", 100);
        }

        [Test]
        [Category("Method Resolution")]
        public void T58_Defect_1462445()
        {
            //Assert.Fail("1462445 - Sprint 19 : [Design Issue] Geometry Porting : Rev 1989 : Static method overload issue ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T58_Defect_1462445.ds");
            thisTest.Verify("d", 9);
            thisTest.Verify("v", 2);
            thisTest.Verify("y", 0);
            thisTest.Verify("w", 2);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T59_Defect_1466572()
        {
           ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T59_Defect_1466572.ds");
            thisTest.Verify("xx", -1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", -1);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T60_Defect_1467004()
        {
           ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T60_Defect_1467004.ds");
           thisTest.Verify("s", -123);
            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T59_Defect_1466572_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T59_Defect_1466572_2.ds");
            thisTest.Verify("xx", -1);
            thisTest.Verify("yy", 0);
            thisTest.Verify("zz", 1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T59_Defect_1466572_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T59_Defect_1466572_3.ds");
            thisTest.Verify("p2", 0.5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T61_Defect_1459171()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T61_Defect_1459171.ds");
            thisTest.Verify("b1", 5.000000);
            thisTest.Verify("b2", 10.000000);
            thisTest.Verify("a2", 5.000000);
            thisTest.Verify("a3", 10.000000);

        }
        [Test]
        public void T62_class_assignment_inside_imperative()
        {
            string dsFullPathName = testPath + "T62_class_assignment_inside_imperative.ds";
            ExecutionMirror mirror = thisTest.RunScript(dsFullPathName);
            // thisTest.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }
        [Test]
        [Category("SmokeTest")]
        public void T63_Array_inClass_Imperative_1465637()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T62_Class_Assignment_inside_imperative_1465637.ds");
            Object[] b1 = new Object[] { 0, -1, -2, -3, -4 };
            Object[] b2 = new Object[] { 0, 0, 0, 0, 0 };
            thisTest.Verify("b1", b1);
            thisTest.Verify("b2", b2);
        }
       
        [Test]
       public void T68_Inherit_Base_Constructor_1467153()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T68_Inherit_Base_Constructor_1467153.ds");

            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 2.0);
            thisTest.Verify("c", 3.0);
            thisTest.Verify("d", false);

        }
        [Test]
        public void T68_Inherit_Base_Constructor_1467153_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T68_Inherit_Base_Constructor_1467153_negative.ds");


            });
            
        }
        
        [Test]
        public void T63_Class_methodresolution_1457172()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T63_Class_methodresolution_1457172.ds");
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
            Object a = null;
            thisTest.Verify("p1", a);
            thisTest.Verify("p2", a);
            thisTest.Verify("p3", a);
            thisTest.Verify("p4", a);
            thisTest.Verify("p5", a);
            thisTest.Verify("p6", a);
            thisTest.Verify("p7", a);
            thisTest.Verify("p8", a);

        }
        [Test]
        public void T63_Class_methodresolution_1457172_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T63_Class_methodresolution_1457172_2.ds");
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
            Object a = null;
            thisTest.Verify("p1", a);
            thisTest.Verify("p2", a);
            thisTest.Verify("p3", a);
            thisTest.Verify("p4", a);
        

        }
        [Test]
        public void T63_Class_methodresolution_1457172_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T63_Class_methodresolution_1457172_3.ds");
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
            Object a = null;
            thisTest.Verify("y", a);
            


        }
        [Test]
        public void T69_Redefine_property_inherited_1467141()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T69_Redefine_property_inherited_1467141.ds");
            });

        }

        [Test]
        public void T70_Defect_1467112_Method_Overloading_Issue()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T70_Defect_1467112_Method_Overloading_Issue.ds");
            //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed");
            thisTest.Verify("b1", 2);
        }
        [Test]
        public void T71_class_inherit_arg_var_1467157()
        {
            // Assert.Fail("1467157 - Sprint 25 - rev 3047 class inheritance if the argument is var and the same declared both in base calss and inherited class it throws error ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T71_class_inherit_arg_var_1467157.ds");
            // not possible to add verification this testcase as there is not method to verify that the warning are gone ! 
            
        }
        [Test]
        public void T72_class_inherit_1467097_1()
        {
            //Assert.Fail("1467097 Sprint 24 - Rev 2761 - if var is used as a argument to function and call function with defined class it goes into a loop and hangs DS ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T72_class_inherit_1467097.ds");
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 3);
            thisTest.Verify("c", 4);
            
        }
        [Test]
        public void T72_class_inherit_1467097_2()
        {
            //Assert.Fail("1467097 Sprint 24 - Rev 2761 - if var is used as a argument to function and call function with defined class it goes into a loop and hangs DS ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T72_class_inherit_1467097_2.ds");
            thisTest.Verify("a", 8);
            thisTest.Verify("b", 9);
            thisTest.Verify("c", 10);
            

        }


        [Test]
        public void TV_1467097_class_inherit()
        {
            String code =
@"
class Parent
{
    A : var;
    B: var;

    constructor Create( x:int,y:int)
    {
        A = x;
        B = y;
    }
}



def modify(oldPoint : var)

{

oldPoint.A = oldPoint.A +1;
oldPoint.B = oldPoint.B +1;
return = oldPoint;
}


oldPoint = Parent.Create( 1,2 );
oldPoint = modify( oldPoint );
ra = oldPoint.A;
rb = oldPoint.B;


    
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 2);
            thisTest.Verify("rb", 3);


        }
        


        [Test]
        [Category("SmokeTest")]
        public void T73_Defect_1467210_Expected_Warning()
        {
            String code =
@"
class Test
{
    def DoSomething()
    {
        return = 5;
    }
}

t = Test.Test();
a = Test.DoSomething(t); //no warning is thrown and returned value is null
a = Test.DoSomething(); //wrong warning is thrown: 
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass);
            thisTest.Verify("a", n1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T74_Defect_1469099_Access_Property()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T74_Defect_1469099_Access_Property.ds");
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 0);           

        }

        [Test]
        [Category("SmokeTest")]
        public void T74_Defect_1469099_Access_Property_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T74_Defect_1469099_Access_Property_2.ds");
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T75_Defect_1467188_Class_Instantiation()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T75_Defect_1467188_Class_Instantiation.ds");
            thisTest.Verify("RX", 1.0);
            thisTest.Verify("RY", 1.0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T75_Defect_1467188_Class_Instantiation_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T75_Defect_1467188_Class_Instantiation_2.ds");
            thisTest.Verify("RX", 9.0);
            thisTest.Verify("RY", 9.0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T76_Defect_1467186_Class_Update()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T76_Defect_1467186_Class_Update.ds");
            thisTest.Verify("a1", 5.0);       

        }

        [Test]
        [Category("SmokeTest")]
        public void T77_Defect_1460274_Class_Update()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T77_Defect_1460274_Class_Update.ds");
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency);
            Object n1 = null;
            thisTest.Verify("a", n1);
            thisTest.Verify("b", n1);

        }

        [Test]        
        public void T77_Defect_1460274_Class_Update_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T77_Defect_1460274_Class_Update_2.ds");
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update causing cyclic dependency for valid case");
            Object n1 = null;

            thisTest.Verify("test2", new Object[] { 4, 2 });
            thisTest.Verify("test", n1);
            thisTest.Verify("test3", new Object[] { 4, 2 });

        }

        [Test]
        [Category("SmokeTest")]
        public void T77_Defect_1460274_Class_Update_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T77_Defect_1460274_Class_Update_3.ds");
            thisTest.Verify("test", new Object[] { 4, 2 } );

        }

        [Test]
        [Category("SmokeTest")]
        public void T77_Defect_1460274_Class_Update_4()
        {
           
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T77_Defect_1460274_Class_Update_4.ds");
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency);
            Object n1 = null;

            thisTest.Verify("c", 1);
            thisTest.Verify("c1", n1);

        }

        [Test]        
        public void T77_Defect_1460274_Class_Update_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T77_Defect_1460274_Class_Update_5.ds");
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update causing cyclic dependency for valid case");
            thisTest.Verify("test", new Object[] { 5, 10, 15, null, 11 });     

        }

        [Test]
        [Category("SmokeTest")]
        public void T78_Defect_1467146_Class_Update_With_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T78_Defect_1467146_Class_Update_With_Replication.ds");
            thisTest.Verify("val", 100 );                

        }

        [Test]
        [Category("SmokeTest")]
        public void T78_Defect_1467146_Class_Update_With_Replication_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T78_Defect_1467146_Class_Update_With_Replication_2.ds");
            Object n1 = null;
            thisTest.Verify("val", n1);

        }

        [Test]
        public void T78_Defect_1467146_Class_Update_With_Replication_3()
        {
            string err = "1467224 Sprint25: rev 3352: method dispatch over heterogeneous array is not correct ";
            thisTest.VerifyRunScriptFile(testPath, "T78_Defect_1467146_Class_Update_With_Replication_3.ds",err);
            //Assert.Fail("1467224 - Sprint25: rev 3352: method dispatch over heterogeneous array is not correct");
            Object n1 = null;
            thisTest.Verify("v", new Object[] { 100, 100, n1 });

        }

        [Test]
        [Category("SmokeTest")]        
        public void T78_Defect_1467146_Class_Update_With_Replication_4()
        {
            string str = "DNL-1467475 Regression : Dot Operation using Replication on heterogenous array of instances is yielding wrong output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "T78_Defect_1467146_Class_Update_With_Replication_4.ds", str);
            Object n1 = null;
            thisTest.Verify("v", new Object[] { 100, n1, n1 });

        }

        [Test]    
        public void T78_Defect_1467146_Class_Update_With_Replication_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T78_Defect_1467146_Class_Update_With_Replication_5.ds");
            //Assert.Fail("1467224 - Sprint25: rev 3352: method dispatch over heterogeneous array is not correct"); 
            Object n1 = null;
            thisTest.Verify("v", n1);
            thisTest.Verify("v2", 200);

            thisTest.Verify("p", n1);
            thisTest.Verify("p2", 200);

        }


        [Test]    
        public void T79_Defect_1458581_Unnamed_Constructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T79_Defect_1458581_Unnamed_Constructor.ds");
            thisTest.Verify("b1", 1);
            thisTest.Verify("b2", 1.5);
        }

        [Test]
        [Category("Replication")]
        public void T80_Defect_1444246_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80_Defect_1444246_Replication.ds");
            thisTest.Verify("xs", new Object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }

        [Test]
        [Category("Replication")]
        public void T80_Defect_1444246_Replication_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80_Defect_1444246_Replication_2.ds");
            thisTest.Verify("p2", -20.0);
        }

        [Test]        
        public void T81_Defect_1467246_derived_class_setter()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T81_Defect_1467246_derived_class_setter.ds");
            //Assert.Fail("1467246 - sprint25: rev 3445 : REGRESSION : Property setter/getter not working as expected for derived classes");
            
            thisTest.Verify("x1", 4);
            thisTest.Verify("z1", 6);
        }

        [Test]
        public void T82_Defect_1467174()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T82_Defect_1467174.ds");
            
            thisTest.Verify("t1", 1.5);
           
        }

        [Test]
        [Category("Replication")]
        public void T83_Defect_1463232()
        {
            String code =
@"class A
{
}
a = A.A();
t = a.x;
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;

            thisTest.Verify("t", n1);

        }

        [Test]
        [Category("Replication")]
        public void T83_Defect_1463232_2()
        {
            String code =
@"class A
{
}
a;t;
[Imperative]
{
a = A.A();
t = a.x;
}
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;

            thisTest.Verify("t", n1);

        }

        [Test]
        [Category("Replication")]
        public void T83_Defect_1463232_3()
        {
            String code =
@"class A
{
}
[Associative]
{
a = {A.A(),A.A()};
t = a.x;
}
";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "DNL-1467480 Regression : Dot Operation on instances using replication returns single null where multiple nulls are expected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;

            thisTest.Verify("t", new Object[] { n1, n1} );

        }


        [Test]
        public void T84_ClassNameAsPropertyName_01()
        {
            string code = @"
class A
{
    x:int;
    static y:int;

    constructor A(_x:int)
    {
        x = _x;
    }
}

class B
{
    A : A;

    constructor B()
    {
        A = A.A(100);
    }

    def getx()
    {
        return = A.x;
    }

    def modifyx()
    {
        A.x = 200;
        return = null;
    }
}

b = B.B();
x1 = b.getx();
r = b.modifyx();
x2 = b.getx();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 100);
            thisTest.Verify("x2", 200);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void T85_Defect_1467247()
        {
            string code = @"
class A
{ 
    public x : var ;    
    private y : var ;
    //protected z : var = 0 ;
    constructor A ()
    {
               
    }
    public def foo1 (a)
    {
        x = a;
        y = a + 1;
        return = x + y;
    } 
    private def foo2 (a)
    {
        x = a;
        y = a + 1;
        return = x + y;
    }    
}

a = A.A();
a1 = a.foo1(1);
a2 = a.foo2(1);
a.x = 4;
a.y = 5;
t1 = a.x;
t2 = a.y;
";
            thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("t1", 4);
            thisTest.Verify("t2", n1);
            
        }   
    

        [Test]
        public void T85_Defect_1467247_2()
        {
            string code = @"
class A
{ 
    public x : var ;    
    private y : var ;
    constructor A ()
    {
        x = 10;
        y = 20;   
    }
    public def foo1 ()
    {
       return = foo2();
    } 
    private def foo2 ()
    {
        x = 1;
        y = 2;
        return = x + y;
    }    
}

a = A.A();
a1 = a.foo1();
a2 = a.foo2();
t1 = a.x;
t2 = a.y;
a.x = 4;
a.y = 5;

";
            thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("t1", 4);
            thisTest.Verify("t2", n1);
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", n1);
            
        }

         [Test]
        public void T85_Defect_1467247_3()
        {
            string code = @"
class A
{ 
    public x : var ;   
    constructor A ()
    {
        x = 10;       
    }
    public def foo ()
    {
       return = x + 1;
    }        
}

a = A.A();
a1 = a.foo();
a.x = 4;
";
            string errmsg = "1467254 - Sprint25: rev 3468 : REGRESSION: class property update is not propagating";
            thisTest.VerifyRunScriptSource(code,errmsg);           
            thisTest.Verify("a1", 5);
           

        }

         [Test]
         public void T86_Defect_1467216()
         {
             string code = @"
class Plane
{
    x : int ;
}

class Polygon
{
    Plane : Plane;
    constructor Create(plane : Plane)
    {
        Plane = plane;
        Plane.x = 10;
    }
}

pln = Plane.Plane();
s = Polygon.Create(pln);
p = s.Plane;
test = p.x;
";
             string errmsg = "1467254 - Sprint25: rev 3468 : REGRESSION: class property update is not propagating";
             thisTest.VerifyRunScriptSource(code, errmsg);
             thisTest.Verify("test", 10);


         }

         [Test]
         public void T86_Defect_1467216_2()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 string code = @"
class Plane
{
    x : int ;
}

Plane = Plane.Plane();
test = Plane.x;
";
                 
                 thisTest.RunScriptSource(code);                
             });
         }

         [Test]
         public void T86_Defect_1467216_3()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 string code = @"
class Plane
{
    x : int ;
}

Plane = 10;
test = Plane;
";
                 thisTest.RunScriptSource(code);
             });
         }

         [Test]
         public void T86_Defect_1467216_4()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 string code = @"
class Plane
{
    x : int ;
}
def foo ( ) 
{
    Plane = 10;
    return = Plane;
}
test = foo();
";

                 thisTest.RunScriptSource(code);
             });
         }

         [Test]
         public void T87_Defect_1467243()
         {
string code = @"
class A
{
    fx :var;
    constructor A(x : var)
    {
        fx = x;
    }
}
fa = A.A(1..3);
r1 = fa.fx[0]==fa[0].fx? true:false;
r2 = fa.fx[0];
";
                 string errmsg = "";
                 thisTest.VerifyRunScriptSource(code, errmsg);
                 thisTest.Verify("r1", true);
                 
         }

         [Test]
         public void T87_Defect_1467243_2()
         {
             string code = @"
class A
{
    fx :var[][];
    constructor A(x : var[][])
    {
        fx = x;
    }
}
fa = { A.A({{1},{2}}), A.A({{3},{4}}) };
r1 = fa[0].fx[0]==fa.fx[0][0]? true:false;
r2 = fa.fx[0][0];
";
             string errmsg = "";
             thisTest.VerifyRunScriptSource(code, errmsg);
             thisTest.Verify("r1", new Object[] { true });

         }

        [Test]
        public void T88_Runtime_MemberFunction01()
         {
             string code = @"
class C1
{
    def foo()
    {
        return = 100;
    }
}

class C2
{
    private def foo()
    {
        return = 200;
    }
}

def foo(i)
{
    return = (i > 0) ? C1.C1() : C2.C2();
}

x = foo(-1);
r = x.foo();
";
             string errmsg = "DNL-1467343 private member function can be called at runtime";
             thisTest.VerifyRunScriptSource(code, errmsg);
             thisTest.Verify("r", null);
         }

        [Test]
        public void T89_Runtime_MemberFunction02()
        {
            string code = @"
class Base
{
    private def foo()
    {
        return = 100;
    }
}

class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}

def foo:Base()
{
    return = Derive.Derive();
}

b = foo();
r = b.foo();
";
            string errmsg = "DNL-1467344 Calling a derived class's member function always get null at runtime";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", 200);
        }


        [Test]
        public void T89_1467344_02()
        {
            string code = @"
class Base
{
    private def foo()
    {
        return = 100;
    }
}

class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}

def foo:Base()
{
    return = Derive.Derive();
}
b;
r;

[Imperative]
{
        b = foo();
    r = b.foo();
}
";
            string errmsg = "DNL-1467344 Calling a derived class's member function always get null at runtime";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T90_Runtime_MemberFunction03()
        {
            string code = @"
class C1 
{
    def foo()
    {
        return = 100;
    }
}

class C2 extends C1
{
    def foo()
    {
        return = 200;
    }
}


class C3 extends C2
{
    def foo()
    {
        return = 300;
    }
}

def foo:C2()
{
    return = C3.C3();
}

x = foo();
r = x.foo();
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", 300);
        }
        [Test]
        public void T91_stackoverflow()
        {
            string code = @"
                    class test 
                    { 
                    a; 
                    constructor test(a1 : int, b1 : int, c1 : int) 
                    { 
                    a = a1; 
                    } 
                    } 
                    class Row 
                    { 


                    constructor ByPoints(yy:int, xx: int) 
                    { 

                    [Imperative] 
                    { 
                    for(j in 0..36) 
                    { 
                    tread = test.test(yy, xx, 3); 
                    rise = test.test(tread.a,xx,3); 

                    } 
                    } 
                    } 

                    } 
                    a = 0..18..1; 
                    b = 0..18..1; 

                    Rows = Row.ByPoints(a, b);
                    ";
            string errmsg = "1467365 - Sprint 27 - Rev 3058 Stack overflow detected in the attached code ";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
        }
        [Test]
        [Category("SmokeTest")]
        public void T92_default_argument_1467384()
        {
            String code =
            @"class test
                {
                    def test(t : int = 4)
                    {
                        return = t;
                    }
                }
                a = test.test().t;
                ";
            string error = " 1467384  - Sprint 27 - Rev 4210 default arguments are not working inside class ";
            thisTest.VerifyRunScriptSource(code,error);
            thisTest.Verify("a", 4, 0);

        }
        [Test]
        [Category("SmokeTest")]
        public void T92_default_argument_1467402()
        {
            String code =
            @"class B extends A{ b = 2; }
                class A{
                c : A;
                f= 1;
                }
                b1 = B.B(); 
                c = b1.b;
                d = b1.f;
                ";
            string error = " 1467402 -if the class is used before its define it throws error Index was outside the bounds of the array - negative case ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("c", 2, 0);
            thisTest.Verify("d", 1, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties()
        {
            String code =
@"class Base
{
    static x : int[];
}

t = Base.x;
Base.x = { 5.2, 3.9 }; 
";
            string error = ""; 
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("t", new Object[] { 5, 4 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_2()
        {
            String code =
@"class A
{
static x:int = 3; 
}

a = A.A();
a.x = 2;
b1 = a.x;
a.x = 3; 
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_3()
        {
            String code =
@"class A
{
    static x:int = 3; 
    constructor A ( y1)
    {
        x = y1;
    }
}
y = 2;
a = A.A(y);
b1 = a.x;
y = 3;
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_4()
        {
            String code =
@"class A
{
    static x:int[] = {3,4}; 
    constructor A ( y1:int[])
    {
        x = y1;
    }
}
y = 2..3;
a = A.A(y);
b1 = a.x;
y = 3..4;
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", new Object [] { 3, 4});
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_5()
        {
            String code =
@"class A
{
    static x:int[] = {3,4}; 
    constructor A ( y1:int[])
    {
        x = y1;
    }
}
y = 2..3;
a = A.A(y);
b1 = a.x;
y = {4,4};
a.x[0] = 3;
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", new Object[] { 3, 4 });
        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467343()
        {
            String code =
@"class C1
{
    def foo()
    {
        return = ""C1.foo()"";
    }
}

class C2
{
    private def foo()
    {
        return = ""C2.foo()"";
    }
}

def foo(i)
{
    return = (i > 0) ? C1.C1() : C2.C2();
}

x = foo(-1);
r = x.foo();  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);

            
        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467343_2()
        {
            String code =
@"class C1
{
    def foo()
    {
        return = ""C1.foo()"";
    }
}

class C2
{
    private def foo()
    {
        return = ""C2.foo()"";
    }
}

def foo(i)
{
    return = (i > 0) ? C1.C1() : C2.C2();
}

r;
[Imperative]
{
    x = foo(-1);
    r = x.foo();  
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467344_3()
        {
            String code =
@"
class Base
{
    private def foo()
    {
        return = 100;
    }
}

class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}

def foo:Base()
{
    return = Derive.Derive();
}

b = foo();
r = b.foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            
        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467344_4()
        {
            String code =
@"
class Base
{
    private def foo()
    {
        return = 100;
    }
}

class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}

def foo:Base()
{
    return = Derive.Derive();
}
[Imperative]
{
b = foo();
r = b.foo();
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);

        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467443()
        {
            String code =
@"
class test
{
    private foo;
    
}
a = test.test();
a.foo = 1;
";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94__imperative_1467443_7()
        {
            String code =
@"
class test
{
    private foo;
    
}
a = test.test();
[Imperative]
{
    a.foo = 1;
}
";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_2()
        {
            String code =
@"
class test{    }
a = test.test();
a.b = 1;

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_imperative_1467443_8()
        {
            String code =
@"
class test{    }
a;
[Imperative]
{
a = test.test();
a.b = 1;
}

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_3()
        {
            String code =
@"
class test
{
    private foo;
    
}
class test1 extends test
{
    
}
a = test.test();
a.foo = 1;

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_iperative_1467443_9()
        {
            String code =
@"
class test
{
    private foo;
    
}
class test1 extends test
{
    
}
a;
[Imperative]
{
a = test.test();
a.foo = 1;
}

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_4()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
a = test1.test1();
a.foo = 1;

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_imperative_1467443_10()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
a;
[Imperative]
{
a = test1.test1();
a.foo = 1;
}

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_6()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
def foo1()
{
    return = test.test();
}
a = foo1();
a.foo = 1;

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAccessViolation);


        }
        [Test]
        [Category("SmokeTest")]
        public void T94_imperative_1467443_11()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
def foo1()
{
    return = test.test();
}
a;
[Imperative]
{
a = foo1();
a.foo = 1;
}

";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);


        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_1()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }

            x = (A.A(1..3))[0];
            y=x.a;
            ";
            string error = "DNL-1467618 Regression : Use of the array index after replicated constructor yields complier error now";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", 1);

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_2()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }

            x = (A.A(1..3))[0..2];
            y=x.a;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object [] {1,2,3});

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_3()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }


def foo(i:int)
{
    return = A.A(i);
}

x = foo(1..3)[0];
y = x.a;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", 1);

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_4()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }


def foo(i:int)
{
    return = A.A(i);
}

x = foo(1..3)[0..2];
y = x.a;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object [] {1,2,3});

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_5()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
            y;
            [Imperative]
            {
                x = (A.A(1..3))[0];
                y=x.a;
            }
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", 1);

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_6()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
            y;
            [Imperative]
            {
            x = (A.A(1..3))[0..2];
            y={x[0].a,x[1].a,x[2].a};
            }
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object[] { 1, 2, 3 });

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_7()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }

          def foo(i:int)
          {
            return = A.A(i);
          }

          x = foo(1..3)[{1,2}];
          y = x.a;
          ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object[] { 2, 3 });

        }
        [Test]
        [Category("SmokeTest")]
        public void T96_1467464_1()
        {
            String code =
            @"
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
                    // The value of 'i' cannot be inspected here.
                    // If this line is removed, then 'i' can be inspected.    
                            f = i;
                        }
                    }
                }

                a = test.test();
                b = a.f;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b", null);

        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467440_Class_Not_Defined_1()
        {
            String code =
            @"
def foo(x : NonExistClass[][])
{
    return = x;
}

z = foo({ 1, 2 });
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", n1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467440_Class_Not_Defined_2()
        {
            String code =
            @"
z;
[Imperative]
{
    def foo(x : NonExistClass[][])
    {
        return = x;
    }

    z = foo({ 1, 2 });
}
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", n1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467440_Class_Not_Defined_3
            ()
        {
            String code =
            @"
z;
[Imperative]
{
    def foo(y: f1, x : f2)
    {
        return = {y,x};
    }

    z = foo( 1, 2);
}
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", n1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_1
            ()
        {
            String code =
            @"
a1 = A.A( a, b); 
a2 = A.A( 1, 2); 
a3 = A.A({1,2}, c);
a4 = A.A({1,2}, c..d, 0..1, 5..f);
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_2
            ()
        {
            String code =
            @"
a1;a2;a3;a4;
[Imperative]
{
    a1 = A.A( a, b); 
    a2 = A.A( 1, 2); 
    a3 = A.A({1,2}, c);
    a4 = A.A({1,2}, c..d, 0..1, 5..f);
}
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_3
            ()
        {
            String code =
            @"
def foo ()
{
    a1 = A.A( a, b); 
    a2 = A.A( 1, 2); 
    a3 = A.A({1,2}, c);
    //a4 = A.A({1,2}, c..d, 0..1, 5..f);
}
a1 : int;
a2 : int;
a3 : int;
a4 : int;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_4
            ()
        {
            String code =
            @"
class A
{
    static def foo ()
    {
        a1 = A.A( a, b); 
        a2 = A.A( 1, 2); 
        a3 = A.A({1,2}, c);
        a4 = A.A({1,2}, c..d, 0..1, 5..f);
    }
}
a1 : int;
a2 : int;
a3 : int;
a4 : int;
test = A.foo();
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T99_UnnamedConstructor01()
        {
            String code =
            @"
class A
{
    x;
    constructor(i) { x = i; }
}
a = A(41);
r = a.x; 
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 41);
        }

        [Test]
        [Category("SmokeTest")]
        public void T99_1467469
            ()
        {
            String code =
            @"
class A{ a = 1; }
class B{ a = 1; }

a = A.A();
b = B.B();

c = 1 == 2;
d = a == b;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.Verify("d", false);
            thisTest.Verify("c", false);

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467421()
        {
            String code =
@"
class A
{
    static  count : int = 0;
    constructor A()
    {
        count = count + 1;
    }   

    
}
a1 = A.A();// received - A(count=null), expected: A(count=1)
r = a1.count;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);
            

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_2()
        {
            String code =
@"
class A
{
    private static  count : int = 0;
    constructor A()
    {
        count = count + 1;
    }   

    
}
a1 = A.A();// received - A(count=null), expected: A(count=1)
r = a1.count;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_3()
        {
            String code =
@"
class A
{
   static count : int= 0;
    constructor A()
    {
        count = count + 1;
    }   

    
}
a1 = A.A(); // received - A(count=null), expected: A(count=1)
a1.count = 3;
b1 = A.A();
r = a1.count;
r2 = b1.count;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 4);
            thisTest.Verify("r2", 4);
           

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_4()
        {
            String code =
@"
class A
{
   static count : int[]= {0,1};
    constructor A()
    {
        count = count + 1;
    }   

    
}
a1 = A.A(); // received - A(count=null), expected: A(count=1)
a1.count = {3,4,5};
b1 = A.A();
r = a1.count;
r2 = b1.count;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 4, 5, 6 });
            thisTest.Verify("r2", new object[] { 4, 5, 6 });

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_5()
        {
            String code =
@"
class A
{
    static  count : int = 0;
    constructor A()
    {
        count = count + 1;
    }   

    
}
r;
[Imperative]
{
a1 = A.A();// received - A(count=null), expected: A(count=1)
r = a1.count;
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);

        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467472()
        {
            String code =
@"
class A
{
    static count : int = 0;
    pt;
    constructor A()
    {
        count = count + 1;
        pt = count;
    }
}

a1 = A.A();
r=a1.count;
r2=a1.pt;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);
            thisTest.Verify("r2", 1);
        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467472_2()
        {
            String code =
@"
class A
{
static count : int = 0;
constructor A()
{
count = count + 1;
}
}
class B extends A
{
pt;
constructor B()
{
pt = count;
}
}
a1 = A.A(); // received - A(count=null), expected: A(count=1)r = a1.count;
r1=a1.count;
b1 = B.B();
r2=b1.pt;

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 2);
        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467472_3()
        {
            String code =
@"
class A
{
    static count : int = 0;
    pt;
    constructor A()
    {
        count = count + 1;
        pt = count;
    }
}
r;
r2;
[Imperative]
{
a1 = A.A();
r=a1.count;
r2=a1.pt;
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);
            thisTest.Verify("r2", 1);
        }
        [Test]
        [Category("SmokeTest")]
        public void T95_1467472_4()
        {
            String code =
@"
class A
{
static count : int = 0;
constructor A()
{
count = count + 1;
}
}
class B extends A
{
pt;
constructor B()
{
pt = count;
}
}
r1;r2;
[Imperative]
{
a1 = A.A(); // received - A(count=null), expected: A(count=1)r = a1.count;
r1=a1.count;
b1 = B.B();
r2=b1.pt;
}

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_1()
        {
            String code =
@"
class FixedNode
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }
    constructor FixedNode( x : int)
    {
        i = x+x;;
    }
}

t = FixedNode.FixedNode(41);
test = t.i; 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test", 41);            
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_2()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B extends A
{
    j : int = 0;
    constructor ( x : int) : base.A(x)
    {
        j = x;
    }    
}

t = B.B(4);
test1 = t.i; 
test2 = t.j; 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);
            thisTest.Verify("test2", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_3()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B 
{
    j : A;
    constructor ( x : int) 
    {
        j = A.A(x);
    }    
}

t = B.B(4);
test1 = t.j.i; 

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);
            
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_4()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B 
{
    j : A;
    constructor ( x : int) 
    {
        j = A.A(x);
    }    
}
test1;
[Imperative]
{
    t = B.B(4);
    test1 = t.j.i;
}
 

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);

        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_5()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B 
{
    j : A;
    constructor ( x : int) 
    {
        j = A.A(x);
    }    
}
def foo ()
{
    t = B.B(4);
    return = t.j.i;
}
test1 = foo();
 

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);

        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467522_Indexing_Class_Properties_1()
        {
            String code =
@"
class B
{
    x : double;
    constructor B(xx)
    {
        x = xx;
    }  
}
class A
{
    Start : B;    
    constructor A(start : B)
    {
        Start = start;
        
    }   
}

def foo(a : A[])
{ 
    result = a[0].Start.x;     
    return = result;
}

walls = { };
walls[0] = A.A(B.B(1));
walls[1] = A.A(B.B(3)); 
test = foo(walls); // received {1,3}; expected : 1
test2 = walls[0].Start.x; // received 1
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test", 1.0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467522_Indexing_Class_Properties_2()
        {
            String code =
@"
class A
{
    x : int = 0;
    constructor A(xx)
    {
        x = xx;
    }
    def func(t : A)
    {
        return = x + t.x;
    }
    
}
def foo(wall : A[])
{ 
    result = wall[0].func(wall[1]);    
    return = result;
}

wall = { };
wall[0] = A.A(1);
wall[1] = A.A(2);
test1 = foo(wall);//{ null, null} 
test2 = wall[0].func(wall[1]);  // 3; expected test1=test2
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 3);

        }
        [Test]
        public void T98_Class_Static_Property_Using_Global_Variable()
        {
            String code =
@"
t1 = 3;
class A
{
    static a = t1 ;
}
test1 = A.a;
";
            string error = "DNL-1467557 Update issue : when a static property is defined using a global variable, the value is  not getting updated";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 3);
        }
        [Test]
        public void T98_Class_Static_Property_Using_Other_Properties()
        {
            String code =
@"
class A
{
    b = 3;
    static a = b ;
}
test1 = A.a;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", null);
            thisTest.VerifyBuildWarningCount(1);

        }
        [Test]
        public void T98_1467571_static_nonstatic_issue()
        {
            String code =
@"
class A
{
    b = 3;
    static c = 4;
    static def a () { return = b; }
    def a2 () { return = c; }
}
test1 = A.a();
aa = A.A();
test2 = aa.a2();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", null);
            thisTest.Verify("test2", 4); 
            thisTest.VerifyBuildWarningCount(1);

        }
        [Test]
        public void T99_1467578_this_imperative()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Default(intValue : int)
    {
        IntValue = intValue;
    }
    
    def someOperation : MyInt()
    {
        returnValue;
        [Imperative]
        {
            x = this.IntValue + 1;
            returnValue = MyInt.Default(x);
        }
        return = returnValue;
    }        
}

seed = MyInt.Default(1);
val=seed.someOperation().IntValue;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("val", 2);
            thisTest.VerifyBuildWarningCount(0);

        }
        [Test]
        public void T100_1467578_this_imperative()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Default(intValue : int)
    {
        IntValue = intValue;
    }
    
    def someOperation : MyInt(array : MyInt[])
    {
        returnValue;
        [Imperative]
        {
            for(m in array)
            {
                if (m == this)
                {
                    returnValue = MyInt.Default(this.IntValue + 1);
                }
                else
                {
                    returnValue = MyInt.Default(0);
                }
            }
        }
    }        
}

startArray = MyInt.Default(1..5);

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.VerifyBuildWarningCount(0);

        }
        [Test]
        public void T110_IDE_884_Testing_Bang_Inside_Imperative()
        {
            String code =
@"

class test
{
    static def foo()
    {
        included = true;  
        return = [Imperative]
        {
            a = !included;
            return = a;                

        }
        return = true;
    }
    def foo2()
    {
        included = true;  
        return = [Imperative]
        {
            a;
            if(!included)
                a = !included;
            else
                a = !(!included);                
            return = a;
        }
        return = false;
     }
     def foo3()
     {
        included = false;  
        return = [Imperative]
        {
            a;
            while(!included)
            {
                a = !included;
                included = !included;
            }                           
            return = a;
        }
        return = false;
     }
     def foo4()
     {
        included = false;  
        return = [Imperative]
        {
            a = {0,1};
            x;
            for(i in a )
            {
                x = !included;
                included = !included;                
            }                           
            return = x;
        }
        return = false;
     }
    
}
test1 = test.foo();
t = test.test();
test2 = t.foo2();
test3 = t.foo3();
test4 = t.foo4();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test1", false);
            thisTest.Verify("test2", true);
            thisTest.Verify("test3", true);
            thisTest.Verify("test4", false);

        }
        [Test]
        public void T110_IDE_884_Testing_Bang_Inside_Imperative_2()
        {
            String code =
@"
def foo (a)
{
    x = !a;
    return = [Imperative]
    {
        y = !x;
        if(!y == true)
            return = !y;
        else
            return = !x;
    }
}
a = true;
test1 = [Imperative]
{
    return = !a;
}
test2 = [Imperative]
{
    return = [Associative]
    {
        return = [Imperative]
        {
             return = !a;
        }
    }
} 
test3 = foo(a);  

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test1", false);
            thisTest.Verify("test2", false);
            thisTest.Verify("test3", true);
           

        }
        [Test]
        [Category("SmokeTest")]
        public void T111_Class_Constructor_Negative_1467598()
        {
            String code =
@"

class test
{
    constructor foo()
    {
    }
}

a = test.sum();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
           
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
            thisTest.Verify("a", null);
            
           
        }
        [Test]
        [Category("SmokeTest")]
        public void T112_1467578_this_imperative()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Default(intValue : int)
    {
        IntValue = intValue;
    }
    
    def someOperation : MyInt[](array : MyInt[])
    {
        returnValue = { };
        c = 0;
        [Imperative]
        {
            for(m in array)
            {
                if (Equals ( m, this))
                {
                    returnValue[c] = MyInt.Default(this.IntValue + 1);
                }
                else
                {
                    returnValue[c] = MyInt.Default(0);
                }
                c = c + 1;
            }
        }
        return = returnValue;
    }        
}

startArray = MyInt.Default(1..5);
seed = MyInt.Default(1);
test = seed.someOperation(startArray).IntValue;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.Verify("test", new Object[] { 2,0,0,0,0});


        }
        [Test]
        [Category("SmokeTest")]
        public void T113_1467599_Type_Conversion()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Create()
    {
        [Imperative]
        {
              IntValue = true;
        }
    } 
    constructor MyInt()
    {
        [Imperative]
        {
              IntValue = 1;
        }
    }
    def foo ()
    {
        [Imperative]
        {
            [Associative]
            {
                [Imperative]
                {
                    IntValue = true;
                    return = IntValue;
                }
            }
        }
    }
          
}

a = MyInt.Create().IntValue;
b = MyInt.MyInt().foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(2);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }
        [Test]
        [Category("SmokeTest")]
        public void T114_1467599_Type_Conversion()
        {
            String code =
@"
class MyInt
{
    IntValue : int[];
    
    constructor Create()
    {
        [Imperative]
        {
              IntValue = {true, true};
        }
    } 
    constructor MyInt()
    {
        [Imperative]
        {
              IntValue = {1,1};
        }
    }
    def foo ()
    {
        [Imperative]
        {
            [Associative]
            {
                [Imperative]
                {
                    IntValue = {true,true};
                    //return = IntValue;
                }
            }
        }
        return = IntValue;
    }
          
}

a = MyInt.Create().IntValue;
b = MyInt.MyInt().foo();

";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(4);
            thisTest.Verify("a", new Object[] { null, null } ); 
            thisTest.Verify("b", new Object[] { null, null });

        }
        [Test]
        [Category("SmokeTest")]
        public void T115_1467599_Type_Conversion()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Create()
    {
        [Imperative]
        {
              IntValue = true;
        }
    } 
    def foo()
    {
        return = [Imperative]
        {
              return = MyInt.Create();
        }
    }    
          
}

a = MyInt.Create().IntValue;
b = MyInt.MyInt().foo().IntValue;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(2);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }
        [Test]
        [Category("SmokeTest")]
        public void T116_1467599_Type_Conversion()
        {
            String code =
@"
myValue :int = 1; 
x = 1;
[Imperative]
{           
    myValue = myValue + 0.5; //output = 2, but no type conversion message 
    x = x + 0.5;  
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("myValue", 2);
            thisTest.Verify("x", 1.5);
        }
        [Test]
        [Category("SmokeTest")]
        public void T117_1467599_Type_Conversion()
        {
            String code =
@"
class A
{
    myValue : int = 1;
    x = 1;
    constructor A ()
    {    
        [Imperative]
        {           
            myValue = myValue + 0.5;
            x = x + 0.5;
        }
    }
}
a = A.A();
b = a.myValue;
c = a.x;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 1.5);
        }
        [Test]
        [Category("SmokeTest")]
        public void T118_1467695_setter_inlinecondition()
        {
            String code =
@"

class curve
{
    Color ;
    Length : double;
    constructor curve(t:int)
    {
        length = t;
    }
}

curve1 : curve[]..[];
[Associative]
{
    curve1 = curve.curve(1..10);
    curve1.Color = curve1.Length > 6 ? 1 : 2;
}
";
            string error = "value assigned by a conditional on an array throws error '%set_Color()' is invoked on invalid object ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(0);
            
        }
    }
}


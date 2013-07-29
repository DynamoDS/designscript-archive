using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Associative
{
    public class Class
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testCasePath = "..\\..\\..\\Scripts\\TD\\Associative\\Class\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T001_Associative_Class_Property_Int()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T001_Associative_Class_Property_Int.ds");

            thisTest.Verify("xPoint", 1);
            thisTest.Verify("yPoint", 2);
            thisTest.Verify("zPoint", 3);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T002_Associative_Class_Property_Double()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T002_Associative_Class_Property_Double.ds");

            thisTest.Verify("xPoint", 1.1);
            thisTest.Verify("yPoint", 2);
            thisTest.Verify("zPoint", 3);

        }


        [Test]
        [Category ("SmokeTest")]
 public void T003_Associative_Class_Property_Bool()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T003_Associative_Class_Property_Bool.ds");

            thisTest.Verify("propPoint1", true);
            thisTest.Verify("propPoint2", false);
        }



        [Test]
        [Category ("SmokeTest")]
 public void T004_Associative_Class_Property_DefaultInitialization()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T004_Associative_Class_Property_DefaultInitialization.ds");
            object defaultVar = null;
            thisTest.Verify("defaultVar", defaultVar);
            thisTest.Verify("defaultInt", 0);
            thisTest.Verify("defaultDouble", 0.0);
            thisTest.Verify("defaultBool", false);


            //"1453912"//
        }


        [Test]
        [Category ("SmokeTest")]
 public void T005_Associative_Class_Property_Get_InternalClassFunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T005_Associative_Class_Property_Get_InternalClassFunction.ds");
            thisTest.Verify("val", 4.7);
            //"1453886"//
        }


        [Test]
        [Category ("SmokeTest")]
 public void T006_Associative_Class_Property_UseInsideInternalClassFunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T006_Associative_Class_Property_UseInsideInternalClassFunction.ds");
            thisTest.Verify("val", 4.7);
        }



        [Test]
        [Category ("SmokeTest")]
 public void T007_Associative_Class_Property_CallFromFunctionOutsideClass()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T007_Associative_Class_Property_CallFromFunctionOutsideClass.ds");

            thisTest.Verify("myPointValue", 211.3);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T008_Associative_Class_Property_CallFromAnotherExternalClass()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T008_Associative_Class_Property_CallFromAnotherExternalClass.ds");

            thisTest.Verify("xPosition", 1.3);
            thisTest.Verify("yPosition", 20.5);
            thisTest.Verify("zPosition", 300.8);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T009_Associative_Class_Property_AssignInDifferentNamedConstructors()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T009_Associative_Class_Property_AssignInDifferentNamedConstructors.ds");

            thisTest.Verify("xPt1", 10.1);
            thisTest.Verify("yPt1", 200.2);
            thisTest.Verify("zPt1", 0.0);
                                            
            thisTest.Verify("xPt2", 10.1);
            thisTest.Verify("yPt2", 0.0);
            thisTest.Verify("zPt2", 3000.3);
                                            
            thisTest.Verify("xPt3", 0.0);
            thisTest.Verify("yPt3", 200.2);
            thisTest.Verify("zPt3", 3000.3);

          
        }



        [Test]
        [Category ("SmokeTest")]
 public void T010_Associative_Class_Constructor_Overloads()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T010_Associative_Class_Constructor_Overloads.ds");
   
            thisTest.Verify("zPt1", 0.1);
            thisTest.Verify("zPt2", 3000.1);
            thisTest.Verify("zPt3", 3000.1);

        }


        [Test]
        [Category ("SmokeTest")]
 public void T011_Associative_Class_Property_ExtendedClass()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T011_Associative_Class_Property_ExtendedClass.ds");


            thisTest.Verify("xPt1", 10.1);
            thisTest.Verify("yPt1", 20.2);
            thisTest.Verify("zPt1", 300.3);


        }


        [Test]
        [Category ("SmokeTest")]
 public void T012_Associative_Class_Property_Var()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T012_Associative_Class_Property_Var.ds");


            thisTest.Verify("xPoint", 1);
            thisTest.Verify("yPoint", 2.0);
            thisTest.Verify("zPoint", true);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T013_Associative_Class_Property_GetFromAnotherConstructorInSameClass()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T013_Associative_Class_Property_GetFromAnotherConstructorInSameClass.ds");


            thisTest.Verify("xPoint", 2);
            thisTest.Verify("yPoint", 3);
            thisTest.Verify("zPoint", 4);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T014_Associative_Class_Property_GetUsingMultipleReferencingWithSameName()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T014_Associative_Class_Property_GetUsingMultipleReferencingWithSameName.ds");

            thisTest.Verify("test1", 1);
            thisTest.Verify("test2", 2);
            thisTest.Verify("test3", 3);
            thisTest.Verify("test4", 4);


        }


        [Test]
        [Category ("SmokeTest")]
 public void T015_Associative_Class_Property_SetInExternalFunction()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T015_Associative_Class_Property_SetInExternalFunction.ds");

            thisTest.Verify("testX1", 10);
            thisTest.Verify("testY1", 20);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T016_Associative_Class_Property_SetInClassMethod()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T016_Associative_Class_Property_SetInClassMethod.ds");

            thisTest.Verify("testX1", 10);
            thisTest.Verify("testY1", 20);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T017_Associative_Class_Property_SetInExternalClassMethod()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T017_Associative_Class_Property_SetInExternalClassMethod.ds");

            thisTest.Verify("testX1", 10);
            thisTest.Verify("testY1", 20);
        }



        [Test]
        [Category ("SmokeTest")]
 public void T018_Associative_Class_Constructor_WithSameNameAndArgument_Negative()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T018_Associative_Class_Constructor_WithSameNameAndArgument_Negative.ds");
        }

        [Test]
        [Category ("SmokeTest")]
 public void T019_Associative_Class_Constructor_Overloads_WithSameNameAndDifferentArgumentNumber()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T019_Associative_Class_Constructor_Overloads_WithSameNameAndDifferentArgumentNumber.ds");

            thisTest.Verify("x1", 11.0);
            thisTest.Verify("x2", 101.0);
            thisTest.Verify("y1", 12.0);
            thisTest.Verify("y2", 102.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T020_Associative_Class_Constructor_Overloads_WithSameNameAndDifferentArgumenType()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T020_Associative_Class_Constructor_Overloads_WithSameNameAndDifferentArgumenType.ds");

            thisTest.Verify("x1", 11.0);
            thisTest.Verify("x2", 100);
            thisTest.Verify("y1", 12.0);
            thisTest.Verify("y2", 100);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T021_Associative_Class_Constructor_UsingUserDefinedClassAsArgument()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T021_Associative_Class_Constructor_UsingUserDefinedClassAsArgument.ds");


            thisTest.Verify("x2", 21.0);

            thisTest.Verify("y2", 22.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_Associative_Class_Constructor_AssignUserDefineProperties()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T022_Associative_Class_Constructor_AssignUserDefineProperties.ds");


            thisTest.Verify("lsX", 10.2);
            thisTest.Verify("lsY", 10.1);
            thisTest.Verify("leX", -10.2);
            thisTest.Verify("leY", -10.1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T023_Associative_Class_Constructor_CallingAFunction()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T023_Associative_Class_Constructor_CallingAFunction.ds");


            thisTest.Verify("pX", 20.3);
            thisTest.Verify("pY", 0.1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T024_Associative_Class_Constructor_CallingAnImperativeFunction()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T024_Associative_Class_Constructor_CallingAnImperativeFunction.ds");
            Object v1 = null;
            
            thisTest.Verify("p1X", v1);
            thisTest.Verify("p1Y", v1);
            thisTest.Verify("p2X", v1);
            thisTest.Verify("p2Y", v1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T025_Associative_Class_Constructor_CallingAnotherConstructor()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T025_Associative_Class_Constructor_CallingAnotherConstructor.ds");


            thisTest.Verify("lsX", 1.0);
            thisTest.Verify("lsY", 2.0);
            thisTest.Verify("leX", -1.0);
            thisTest.Verify("leY", -2.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T026_Associative_Class_Constructor_BaseConstructorAssignProperties()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T026_Associative_Class_Constructor_BaseConstructorAssignProperties.ds");


            thisTest.Verify("x", 10.0);
            thisTest.Verify("y", 20.0);
            thisTest.Verify("z", 30.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T027_Associative_Class_Constructor_BaseConstructorWithSameName()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T027_Associative_Class_Constructor_BaseConstructorWithSameName.ds");

            thisTest.Verify("z", 30.3);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T028_Associative_Class_Property_DefaultAssignment()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T028_Associative_Class_Property_DefaultAssignment.ds");

            //Verification
            object xPt1 = 1.0;
            thisTest.Verify("xPt1", xPt1, 0);

            object yPt1 = 2.0;
            thisTest.Verify("yPt1", yPt1, 0);
            object zPt1 = 0.0;
            thisTest.Verify("zPt1", zPt1, 0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T029_Associative_Class_Property_AccessModifier()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Class\\T029_Associative_Class_Property_AccessModifier.ds");
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kAccessViolation);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T030_Associative_Class_Property_AccessModifier()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Class\\T030_Associative_Class_Property_AccessModifier.ds");
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kAccessViolation);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T031_Associative_Class_Property_AccessModifier()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Class\\T031_Associative_Class_Property_AccessModifier.ds");
            thisTest.Verify("x", 4);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T051_Inherit_defaultconstructor()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T051_Inherit_defaultconstructor.ds");
            thisTest.Verify("result1", 1);
            thisTest.Verify("result2", 1.0);
            thisTest.Verify("result3", 2);

        }
          [Test]
        [Category ("SmokeTest")]
 public void T052_Inherit_defaultproperty()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T052_Inherit_defaultproperty.ds");
            object a = null;
            thisTest.Verify("x1", a);
            TestFrameWork.Verify(mirror,"x2",0);
            TestFrameWork.Verify(mirror,"x3", false);
            TestFrameWork.Verify(mirror,"x4",0.0);
            thisTest.Verify("x5", a);
            a = new object[] { };
            thisTest.Verify("x6", a);
            thisTest.Verify("x8", a);
            thisTest.Verify("x9", a);
        }

          [Test]
          [Category ("SmokeTest")]
 public void T053_Inherit_changevalue()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T053_Inherit_changevalue.ds");
              //Assert.Fail("1467246 - sprint25: rev 3445 : REGRESSION : Property setter/getter not working as expected for derived classes");
              thisTest.SetErrorMessage("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
              
              object a = null;
              thisTest.Verify("x1", a);
              thisTest.Verify("x2", 1);
              thisTest.Verify("x3", true);
              thisTest.Verify("x4", 0.5);
              thisTest.Verify("x5", a);
              object [] arr=new object [] {2,2};
              thisTest.Verify("x6", arr);
              object [] arr1=new object [] {0.5,0.5};
              thisTest.Verify("x7", arr1);
              object [] arr2=new object [] {true,false};
              thisTest.Verify("x8", arr2);
              thisTest.Verify("x9", new object[] {null, null});
          }

          /*   [Test]
             public void T054_Inherit_nested()
             {
                 ExecutionMirror mirror = thisTest.RunScript(testCasePath, "T054_Inherit_nested.ds");
             }
             [Test]
             public void T055_Inherit_donotchangevalue()
             {
                 ExecutionMirror mirror = thisTest.RunScript(testCasePath, "T055_Inherit_donotchangevalue.ds");
             }
            [Test]
            
 public void T056_Inherit_private_negative()
             {
                 ExecutionMirror mirror = thisTest.RunScript(testCasePath, "T056_Inherit_private_negative.ds");
             }
           */
          [Test]
          [Category ("SmokeTest")]
 public void T056_Inherit_private()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T056_Inherit_private.ds");
              object result = null;
              thisTest.Verify("a1", 3);
              thisTest.Verify("a2", result);
              thisTest.Verify("a3", 3);
              thisTest.Verify("a4", 4);
              thisTest.Verify("a5", result);
             

          }
           [Test]
        [Category("Method Resolution")]
          public void T057_Inherit_private_modify()
             {
                 ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T057_Inherit_private_modify.ds");
                 //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed");

                 object result = null;
                 thisTest.Verify("b1", 3);
                 thisTest.Verify("b2", result);
                 thisTest.Verify("b3", 1);
                 thisTest.Verify("b4", 10);
                 thisTest.Verify("b5", 4);
                 thisTest.Verify("b6", result);

             }
         [Test]
           [Category("Method Resolution")]
           public void T058_Inherit_private_notmodify()
             {
                 ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T058_Inherit_private_notmodify.ds");
                 //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed"); 
                 object result = null;
                 thisTest.Verify("a1", 3);
                 thisTest.Verify("a2", result);
                 thisTest.Verify("a3", 3);
                 thisTest.Verify("a4", result);
                 thisTest.Verify("a5", 4);
                 thisTest.Verify("a6", result);

             }
         [Test]
         [Category("Method Resolution")]
         public void T059_Inherit_access_privatemember()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T059_Inherit_access_privatemember.ds");
             //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed");
             
             object result = null;
             thisTest.Verify("a1", 3);
             thisTest.Verify("a2", result);
             thisTest.Verify("a3", null);
             thisTest.Verify("a4", result);
             thisTest.Verify("a5", 4);
             thisTest.Verify("a6", result);

         }

          [Test]
        [Category ("SmokeTest")]
 public void T061_Inherit_Property()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T061_Inherit_Property.ds");
            thisTest.Verify("basePoint", 2);
            thisTest.Verify("callbase", 8);
            thisTest.Verify("derivedPoint2", 10);
            thisTest.Verify("xPoint1", 2);
            thisTest.Verify("xPoint2", 8);
            thisTest.Verify("xPoint3", 10);
            
        }
        [Test]
        [Category ("SmokeTest")]
 public void T062_Inherit_classAsArgument()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T062_Inherit_classAsArgument.ds");
            thisTest.Verify("xPoint1", 1);
            thisTest.Verify("yPoint1", 2);
            thisTest.Verify("zPoint1", 3);
            thisTest.Verify("xPoint2", 7);
            thisTest.Verify("yPoint2", 8);
            thisTest.Verify("zPoint2", 9);
            thisTest.Verify("xPoint3", 8);
            thisTest.Verify("yPoint3", 9);
            thisTest.Verify("zPoint3", 10);
            thisTest.Verify("xPoint4", 9);
            thisTest.Verify("yPoint4", 10);
            thisTest.Verify("zPoint4", 11);
          
        }
        [Test]
        [Category ("SmokeTest")]
 public void T063_Inherit_classAsArgument_callinfunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T063_Inherit_classAsArgument_callinfunction.ds");
            thisTest.Verify("basePoint", 2);
            thisTest.Verify("derivedPoint2", 8);
            thisTest.Verify("xPoint1", 1);
            thisTest.Verify("yPoint1", 2);
            thisTest.Verify("zPoint1", 3);
            thisTest.Verify("xPoint2", 7);
            thisTest.Verify("yPoint2", 8);
            thisTest.Verify("zPoint2", 9);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T064_Inherit_classAsArgument_callinfunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T064_Inherit_classAsArgument_callinfunction_negative.ds");
            thisTest.Verify("basePoint", 2);
            object a =null;
            thisTest.Verify("derivedPoint2",a );
            thisTest.Verify("xPoint1", 1);
            thisTest.Verify("yPoint1", 2);
            thisTest.Verify("zPoint1", 3);
            thisTest.Verify("xPoint2", 7);
            thisTest.Verify("yPoint2", 8);
            thisTest.Verify("zPoint2", 9);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T065_Inherit_constructor_withoutbase()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T065_Inherit_constructor_withoutbase.ds");
            thisTest.Verify("basePoint", 2);
            object a = null;          
            thisTest.Verify("xPoint1", 2);
            thisTest.Verify("xPoint2", a);
            thisTest.Verify("xPoint3", 7);
        }
        [Test]
        [Category ("SmokeTest")]
 public void T066_Inherit_constructor_failing_witbase()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T066_Inherit_constructor_failing_witbase.ds");
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", 10);
          
            
        }
        [Test]
        [Category ("SmokeTest")]
 public void T067_Inherit_propertynotassignedinbase()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T067_Inherit_propertynotassignedinbase.ds");
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", 10);
        
            
        }
        [Test]
        [Category ("SmokeTest")]
 public void T068_Inherit_propertyassignedinherited()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T068_Inherit_propertyassignedinherited.ds");
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", a);
            thisTest.Verify("b3", 10);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T069_Inherit_constructor_failing_both()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T069_Inherit_constructor_failing_both.ds");
            object a = null;
            thisTest.Verify("a2", a);
            thisTest.Verify("b2", a);
          

        }
        [Test]
        [Category ("SmokeTest")]
 public void T070_Inherit_constructor_failing_inherited()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T070_Inherit_constructor_failing_inherited.ds");
            object a = null;
            thisTest.Verify("a2", 10);
            thisTest.Verify("b2", a);
            thisTest.Verify("b3", 10);


        }
     
        [Test]
        [Category ("SmokeTest")]
 public void T071_Inherit_constructor_failing_inherited_sameproperty()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T071_Inherit_constructor_failing_inherited_sameproperty.ds");
            object a = null;
            thisTest.Verify("a2", 10);
            thisTest.Verify("b2", a);
            thisTest.Verify("b3", a);


        }
        [Test]
        [Category ("Type System")]
 public void T072_inherit_Class_Constructor_CallingAFunction()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T072_inherit_Class_Constructor_CallingAFunction.ds");
            thisTest.Verify("pX", 50.5);
            thisTest.Verify("pY", 50.5);
  
        }
        [Test]
        [Category ("SmokeTest")]
 public void T073inherit_Constructor_WithSameNameAndDifferentArgumenType()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T073inherit_Constructor_WithSameNameAndDifferentArgumenType.ds");
            thisTest.Verify("x1", 1.0);
            thisTest.Verify("y1", 2.0);
            thisTest.Verify("x2", true);
            thisTest.Verify("y2", false);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T074_Inherit_property_array()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T074_Inherit_property_array.ds");
            object [] a = new object[]{1,2};
            thisTest.Verify("x1",a );
        

        }
        [Test]
        [Category ("SmokeTest")]
 public void T075_Inherit_property_array_modify()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T075_Inherit_property_array_modify.ds");
            object[] a = new object[] { 3, 4 };
            thisTest.Verify("x1", a);


        }
         [Test]
        [Category ("SmokeTest")]
 public void T076_Inherit_property_array_modifyanitem()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T076_Inherit_property_array_modifyanitem.ds");
            object[] a = new object[] { -1, 2 };
            thisTest.Verify("x1", a);

        }
        [Test]
         [Category ("SmokeTest")]
 public void T077_Inherit_property_thatdoesnotexist()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T077_Inherit_property_thatdoesnotexist.ds");
            object a =  null;
            thisTest.Verify("x1", a);

        }
        [Test]
        [Category ("SmokeTest")]
 public void T078_Inherit_property_singletonconvertedtoarray()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T078_Inherit_property_singletonconvertedtoarray.ds");
            thisTest.Verify("x1", null);

        }
        
        [Test]
        [Category ("SmokeTest")]
 public void Z001_Associative_Class_Property_Regress001()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z001_Associative_Class_Property_Regress001.ds");
            thisTest.Verify("sp_x", 1.0);
        }





        [Test]
        [Category ("SmokeTest")]
 public void Z002_Associative_Class_Property_Regress_1454056()
        {
            ExecutionMirror mirror =  thisTest.RunScriptFile(testCasePath, "Z002_Associative_Class_Property_Regress_1454056.ds");
            thisTest.Verify("val", 4.7);

        }

        [Test]
        [Category ("SmokeTest")]
 public void Z003_Associative_Class_Property_Regress_1454164()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z003_Associative_Class_Property_Regress_1454164.ds");
               //No value verification needed. Only need to safeguard unexpected assert. 
        }


        [Test]
        [Category ("SmokeTest")]
 public void Z004_Associative_Class_Property_Regress_1454138()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z004_Associative_Class_Property_Regress_1454138.ds");
            thisTest.Verify("test1", 0.1);
            thisTest.Verify("test2", 1.1);
            thisTest.Verify("test3", 2.1);
            thisTest.Verify("test4", 3.1);
        }


        [Test]
        [Category ("SmokeTest")]
 public void Z005_Associative_Class_Property_Regress_1454178()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z005_Associative_Class_Property_Regress_1454178.ds");
            thisTest.Verify("sum", 4.0);
        }

         [Test]
        [Category ("SmokeTest")]
 public void Z006_Associative_Class_Property_Regress_1453886()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z006_Associative_Class_Property_Regress_1453886.ds");
            thisTest.Verify("one", 1.1);
        }


         [Test]
         [Category ("SmokeTest")]
 public void Z007_Associative_Class_Property_Regress_1454172()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z007_Associative_Class_Property_Regress_1454172.ds");
             object[] expectedA = { 1.0, 1.0, 1.0 };

             thisTest.Verify("sum", expectedA);
         }


         [Test]
         [Category ("SmokeTest")]
 public void Z008_Associative_Class_Property_Regress_1454161()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z008_Associative_Class_Property_Regress_1454161.ds");
             thisTest.Verify("x3", 1.0);
             thisTest.Verify("y3", 1.0);
             thisTest.Verify("z3", 1.0);
             thisTest.Verify("h3", 1.0);

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z009_Associative_Class_Property_Regress_1453891()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z009_Associative_Class_Property_Regress_1453891.ds");
             thisTest.Verify("l_startPoint_X", 3.1, 0);


         }

         [Test]
         [Category ("SmokeTest")]
 public void Z010_Associative_Class_Property_Regress_1454658()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z010_Associative_Class_Property_Regress_1454658.ds");
             //No specific verification needed, prevent compilation failure only.

         }


         [Test]
         [Category ("SmokeTest")]
 public void Z011_Associative_Class_Property_Regress_1454162()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z011_Associative_Class_Property_Regress_1454162.ds");
             object[] expectedResult1 = { 1.0, 1.0, 1.0, 1.0 };
             object[] expectedResult2 = { 1.0, 1.0, 1.0, 1.0 };
             thisTest.Verify("result1", expectedResult1, 0);
             thisTest.Verify("result2", expectedResult2, 0);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z012_Access_Class_Property_From_If_Block_In_Class_Method_Regress_1456397()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z012_Access_Class_Property_From_If_Block_In_Class_Method_Regress_1456397.ds");

             thisTest.Verify("b1", 12);

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z013_Defect_1457038()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z013_Defect_1457038.ds");

             thisTest.Verify("d", 4);
             thisTest.Verify("y", 3); 

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z014_Defect_1457057()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z014_Defect_1457057.ds");
             thisTest.Verify("numpts", 2, 0); 

         }

         [Test]         
 public void Z015_Defect_1457029()
         {
             //Assert.Fail("1457029 - Sprint25: Rev 3369 : Passing a collection value using index as argument to a class method is failing");
             
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z015_Defect_1457029.ds");
             thisTest.Verify("x", 1.0, 0);

         }

         [Test]
 public void Z015_Defect_1457029_2()
         {
             //Assert.Fail("1457029 - Sprint25: Rev 3369 : Passing a collection value using index as argument to a class method is failing");
             
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z015_Defect_1457029_2.ds");
             object[] ExpectedResult = { 1.0, 2.0 };
             
             thisTest.Verify("x", ExpectedResult, 0);
            

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z016_Defect_1456771()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z016_Defect_1456771.ds");           

             thisTest.Verify("t2", 3, 0);
             thisTest.Verify("t1", 7, 0);
        

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z017_Defect_1456898()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z017_Defect_1456898.ds");

             //Verification
             object VariableToCheck = 3;
             thisTest.Verify("b1", VariableToCheck);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z017_Defect_1456898_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z017_Defect_1456898_2.ds");

             //Verification           
             thisTest.Verify("a1", 0);
             thisTest.Verify("b1", 2);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z018_Defect_1456798()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z018_Defect_1456798.ds");

             //Verification            
             thisTest.Verify("b1", 3);
             thisTest.Verify("b2", 3);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z018_Defect_1456798_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z018_Defect_1456798_2.ds");

             //Verification            
             thisTest.Verify("a2", 3.5);             
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z018_Defect_1456798_3()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z018_Defect_1456798_3.ds");

             //Verification            
             thisTest.Verify("a2", 6);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z019_Defect_1457053()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z019_Defect_1457053.ds");

             //Verification  
             thisTest.Verify("b1", 6);
             thisTest.Verify("b2", 6);

             thisTest.Verify("b3", 7);
             thisTest.Verify("b4", 7);

             thisTest.Verify("b5", 9);
             thisTest.Verify("b6", 9);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z020_Defect_1457290()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z020_Defect_1457290.ds");
             Object[] a = new Object []{ 20, 20 };
             //Verification 
             thisTest.Verify("a2", a);              
             
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z020_Defect_1457290_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z020_Defect_1457290_2.ds");
             Object[] a = new Object[] { 20, 20 };
             //Verification 
             thisTest.Verify("a2", a);

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z021_Defect_1458785()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z021_Defect_1458785.ds");
             object v1 = null;

             //Verification 
             thisTest.Verify("a3", 2);
             thisTest.Verify("a2", v1);
             thisTest.Verify("a4", v1);
             thisTest.Verify("a5", 2);
             thisTest.Verify("y11", v1);
             thisTest.Verify("z11", 2);

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z021_Defect_1458785_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z021_Defect_1458785_2.ds");
             object v1 = null;

             //Verification 
             thisTest.Verify("a1", v1);
             thisTest.Verify("a2", v1);
             thisTest.Verify("a3", v1);
             thisTest.Verify("a4", 2);
             thisTest.Verify("a5", v1);
             thisTest.Verify("a6", 3);
            

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z021_Defect_1458785_3()
         {
             string code = @"def foo ( i:int[])
{
return = i;
}

x =  1;
a1 = foo(x);
a2 = 3;

";

             ExecutionMirror mirror = thisTest.RunScriptSource(code);
             

             thisTest.Verify("a1", new object[] {1});

             thisTest.Verify("a2", 3);

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z021_Defect_1458785_4()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z021_Defect_1458785_4.ds");
             object v1 = null;

             //Verification 
             thisTest.Verify("test2", v1, 0);
             thisTest.Verify("test3", 5, 0);
             thisTest.Verify("test4", v1, 0);
             thisTest.Verify("test5", 4, 0);

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z021_Defect_1458785_5()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z021_Defect_1458785_5.ds");
             object v1 = null;

             //Verification 
             thisTest.Verify("a1", 3, 0);
             thisTest.Verify("a2", v1, 0);
             thisTest.Verify("a3", 4, 0);
             thisTest.Verify("a4", v1, 0);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z021_Defect_1458785_6()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z021_Defect_1458785_6.ds");
             object v1 = null;

             //Verification 
             thisTest.Verify("a2", v1, 0);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z022_Defect_1455292()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z022_Defect_1455292.ds");
            
             //Verification 
             thisTest.Verify("dummy", 100, 0);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z022_Defect_1455292_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z022_Defect_1455292_2.ds");
             
             //Verification 
             thisTest.Verify("xx", 10, 0);
             thisTest.Verify("yy", 100, 0);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z022_Defect_1455292_3()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z022_Defect_1455292_3.ds");
             Object[] v1 = new Object[] { 2, 3, 1, 4, 1, 4 }; 
            
             //Verification 
             thisTest.Verify("t", v1, 0);          
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z023_Defect_1455131()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z023_Defect_1455131.ds");
             object v1 = null;
            
             //Verification 
             thisTest.Verify("result", 21.0, 0); 
             thisTest.Verify("result2", v1, 0); 
             thisTest.Verify("result3", 3.0, 0); 
             
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z023_Defect_1455131_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z023_Defect_1455131_2.ds");
             Object[] v1 = new Object[] { 10.0, null, 2.0 };

             //Verification 
             thisTest.Verify("t", v1);   
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z024_Defect_1461133()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z024_Defect_1461133.ds");
             });

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z024_Defect_1461133_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z024_Defect_1461133_2.ds");             

             //Verification 
             thisTest.Verify("test", 2, 0);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z025_Defect_1459626()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z025_Defect_1459626.ds");

             //Verification 
             thisTest.Verify("t1", 10, 0);
             thisTest.Verify("t2", 20, 0);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z026_Defect_1458563()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z026_Defect_1458563.ds");
             //Verification 
             thisTest.Verify("t1", null);
           
         }

         [Test]
         [Category("Update")]
 public void Z026_Defect_1458563_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z026_Defect_1458563_2.ds");
             Object[] v1 = new Object[] { 1, 2 };
             Object v2 = null;

             //Assert.Fail("1463700 - Sprint 20 : rev 2147 : Update is not happening when a collection property of a class instance is updated using a class method");
             //Verification 
             thisTest.Verify("p1", 1);
             thisTest.Verify("p2", true);
             thisTest.Verify("p3", 1.0);
             thisTest.Verify("p4", 1);
             thisTest.Verify("p5", 2);
             thisTest.Verify("p6", 2);
             thisTest.Verify("p7", false);
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z027_Defect_1461365()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z027_Defect_1461365.ds");
             });             
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z028_Same_Name_Constructor_And_Method_Negative()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z028_Same_Name_Constructor_And_Method_Negative.ds");
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z029_Calling_Class_Constructor_From_Instance_Negative()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z029_Calling_Class_Constructor_From_Instance_Negative.ds");

             Object v2 = null;

             //Verification 
             thisTest.Verify("b1", v2);
             thisTest.Verify("c1", v2);             
           
         }

         [Test]
         [Category ("SmokeTest")]
 public void Z030_Class_Instance_Name_Same_As_Class_Negative()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z030_Class_Instance_Name_Same_As_Class_Negative.ds");
             });

         }

         [Test]
         [Category ("SmokeTest")]
 public void Z030_Class_Instance_Name_Same_As_Class_Negative_2()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Z030_Class_Instance_Name_Same_As_Class_Negative_2.ds");
             });

         }
      


    }

}

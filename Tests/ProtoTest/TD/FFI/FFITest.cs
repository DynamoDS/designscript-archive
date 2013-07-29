using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.FFI
{
    class FFITest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string FFIPath = "..\\..\\..\\Scripts\\TD\\FFI\\";

        [SetUp]
        public void Setup()
        {
        }


       
       
 public void T001_FFI_MathLibrary_Sqrt_Trigonomatry()
        {
           
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T001_FFI_MathLibrary_Sqrt_Trigonomatry.ds");

            thisTest.Verify("sqrt_10", 3.162278, 1);
            thisTest.Verify("log_100", 4.605170, 1);
            thisTest.Verify("angle", 30.0, 1);
            thisTest.Verify("sin_30", 0.5, 1);
            thisTest.Verify("cos_30", 0.866025, 1);
            thisTest.Verify("tan_30", 0.577350, 1);
            thisTest.Verify("asin_30", 30.0, 1);
            thisTest.Verify("acos_30", 30.0, 1);
            thisTest.Verify("atan_30", 30.0, 1);

        }

       
 public void T002_FFI_Matrix_Simple()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T002_FFI_Matrix_Simple.ds");

            thisTest.Verify("dummy1", 1.0, 1);
            thisTest.Verify("dummy2", 1.0, 1);
            thisTest.Verify("dummy3", 1.0, 1);
            thisTest.Verify("dummy4", 1.0, 1);

            thisTest.Verify("val_00", 1.0, 1);
            thisTest.Verify("val_11", 1.0, 1);
            thisTest.Verify("val_22", 1.0, 1);
            thisTest.Verify("val_33", 1.0, 1);


        }

        [Test]
        [Category ("SmokeTest")]
 public void T003_FFI_Tuple4_XYZH_Simple()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T003_FFI_Tuple4_XYZH_Simple.ds");

            thisTest.Verify("resultX", -10.0);
            thisTest.Verify("resultY", -20.0);
            thisTest.Verify("resultZ", -30.0);
            thisTest.Verify("resultH", -40.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T004_FFI_Tuple4_XYZ_Simple_WithGetMethods()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T004_FFI_Tuple4_XYZ_Simple_WithGetMethods.ds");

            thisTest.Verify("resultX", -10.0);
            thisTest.Verify("resultY", -20.0);
            thisTest.Verify("resultZ", -30.0);
            thisTest.Verify("resultH", 1.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T005_FFI_Tuple4_ByCoordinate3_Simple()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T005_FFI_Tuple4_ByCoordinate3_Simple.ds");

            object[] expectedA = { 10.0, 11.0, 12.0 };
            object[] expectedB = { 10.0, 11.0, 12.0, 1.0 };

            thisTest.Verify("result3", expectedA);
            thisTest.Verify("result4", expectedB);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T006_FFI_Tuple4_ByCoordinate4_Simple()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T006_FFI_Tuple4_ByCoordinate4_Simple.ds");

            object[] expectedA = { 10.0, 11.0, 12.0 };
            object[] expectedB = { 10.0, 11.0, 12.0, 13.0 };

            thisTest.Verify("result3", expectedA);
            thisTest.Verify("result4", expectedB);

        }


        [Test]
        [Category ("SmokeTest")]
 public void T007_FFI_Tuple4_Multiply_Simple()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T007_FFI_Tuple4_Multiply_Simple.ds");
            thisTest.Verify("multiply", 400.0);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T008_FFI_Transform_ByDate_Simple()
        {
            
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T008_FFI_Transform_ByDate_Simple.ds");
            thisTest.Verify("c0_X", 1.0);
            thisTest.Verify("c0_Y", 0.0);
            thisTest.Verify("c0_Z", 0.0);
            thisTest.Verify("c0_H", 0.0);

            thisTest.Verify("c1_X", 0.0);
            thisTest.Verify("c1_Y", 1.0);
            thisTest.Verify("c1_Z", 0.0);
            thisTest.Verify("c1_H", 0.0);

            thisTest.Verify("c2_X", 0.0);
            thisTest.Verify("c2_Y", 0.0);
            thisTest.Verify("c2_Z", 1.0);
            thisTest.Verify("c2_H", 0.0);

            thisTest.Verify("c3_X", 0.0);
            thisTest.Verify("c3_Y", 0.0);
            thisTest.Verify("c3_Z", 0.0);
            thisTest.Verify("c3_H", 1.0);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T009_FFI_Transform_ByTuples_Simple()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T009_FFI_Transform_ByTuples_Simple.ds");
            thisTest.Verify("c0_X", 1.0);
            thisTest.Verify("c0_Y", 0.0);
            thisTest.Verify("c0_Z", 0.0);
            thisTest.Verify("c0_H", 0.0);

            thisTest.Verify("c1_X", 0.0);
            thisTest.Verify("c1_Y", 1.0);
            thisTest.Verify("c1_Z", 0.0);
            thisTest.Verify("c1_H", 0.0);

            thisTest.Verify("c2_X", 0.0);
            thisTest.Verify("c2_Y", 0.0);
            thisTest.Verify("c2_Z", 1.0);
            thisTest.Verify("c2_H", 0.0);

            thisTest.Verify("c3_X", 0.0);
            thisTest.Verify("c3_Y", 0.0);
            thisTest.Verify("c3_Z", 0.0);
            thisTest.Verify("c3_H", 1.0);
        }



        [Test]
        [Category ("SmokeTest")]   
 public void T010_FFI_Transform_ApplyTransform()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T010_FFI_Transform_ApplyTransform.ds");
            thisTest.Verify("x", 0.2);
            thisTest.Verify("y", 4.0);
            thisTest.Verify("z", 8.0);
            thisTest.Verify("h", 2.0);

           
        }
        [Test]
        [Category ("SmokeTest")]      
 public void T011_FFI_Transform_NativeMultiply()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T011_FFI_Transform_NativeMultiply.ds");
            thisTest.Verify("r0X", 0.0);
            thisTest.Verify("r0Y", 6.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 4.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 2.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 4.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 1.0);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 0.0);
            thisTest.Verify("r3H", 4.0);
        }

        [Test]
        [Category ("SmokeTest")]       
 public void T012_FFI_Transform_NativePreMultiply()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T012_FFI_Transform_NativePreMultiply.ds");
            thisTest.Verify("r0X", 0.0);
            thisTest.Verify("r0Y", 6.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 4.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 2.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 4.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 4.5);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 6.0);
            thisTest.Verify("r3H", 6.0);
        }


        [Test]
        [Category ("SmokeTest")]        
 public void T013_FFI_Transform_TransformVector()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T013_FFI_Transform_TransformVector.ds");
            thisTest.Verify("resultx", 10.0);
            thisTest.Verify("resulty", 40.0);
            thisTest.Verify("resultz", 90.0);




        }

        [Test]
        [Category ("SmokeTest")]     
 public void T014_FFI_Transform_TransformPoint()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T014_FFI_Transform_TransformPoint.ds");
            thisTest.Verify("resultx", 10.0);
            thisTest.Verify("resulty", 40.0);
            thisTest.Verify("resultz", 90.0);



        }


        [Test]
        [Category ("SmokeTest")]
 public void T015_FFI_Transform_Identity()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T015_FFI_Transform_Identity.ds");
            thisTest.Verify("r0X", 1.0);
            thisTest.Verify("r0Y", 0.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 1.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 0.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 1.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 0.0);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 0.0);
            thisTest.Verify("r3H", 1.0);
        }


        [Test]
        [Category ("SmokeTest")]
 public void T016_FFI_Transform_GetTuples()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T016_FFI_Transform_GetTuples.ds");
            thisTest.Verify("r0X", 1.0);
            thisTest.Verify("r0Y", 0.0);
            thisTest.Verify("r0Z", 0.0);
            thisTest.Verify("r0H", 0.0);
            thisTest.Verify("r1X", 0.0);
            thisTest.Verify("r1Y", 1.0);
            thisTest.Verify("r1Z", 0.0);
            thisTest.Verify("r1H", 0.0);
            thisTest.Verify("r2X", 0.0);
            thisTest.Verify("r2Y", 0.0);
            thisTest.Verify("r2Z", 1.0);
            thisTest.Verify("r2H", 0.0);
            thisTest.Verify("r3X", 0.0);
            thisTest.Verify("r3Y", 0.0);
            thisTest.Verify("r3Z", 0.0);
            thisTest.Verify("r3H", 1.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T017_FFI_Transform_GetData()
        {
            object[] result0 = { 1.0, 0.0, 0.0, 0.0 };
            object[] result1 = { 0.0, 1.0, 0.0, 0.0 }; 
            object[] result2 = { 0.0, 0.0, 1.0, 0.0 };
            object[] result3 = { 0.0, 0.0, 0.0, 1.0 };
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T017_FFI_Transform_GetData.ds");
            thisTest.Verify("result0", result0);
            thisTest.Verify("result1", result1);
            thisTest.Verify("result2", result2);
            thisTest.Verify("result3", result3);
 
        }
       
 public void Z001_FFI_Regress_1455587()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "Z001_FFI_Regress_1455587.ds");
            thisTest.Verify("sum_1_to_10", 55.0);

        }

      
 public void T018_FFI_Math_Trigonometric_Hyperbolic()
        {
            const double deg_to_rad = Math.PI / 180;
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T018_FFI_Math_Trigonometric_Hyperbolic.ds");
            thisTest.Verify("angle", 30.0, 1);
            thisTest.Verify("sec_30", 1 / Math.Sin(deg_to_rad * 30), 1);
            thisTest.Verify("csc_30", 1 / Math.Cos(deg_to_rad * 30), 1);
            thisTest.Verify("cot_30", 1 / Math.Tan(deg_to_rad * 30), 1);
            thisTest.Verify("asec_30", 30.0, 1);
            thisTest.Verify("acsc_30", 30.0, 1);
            thisTest.Verify("acot_30", 30.0, 1);
            thisTest.Verify("sinh_1", Math.Sinh(1), 1);
            thisTest.Verify("cosh_1", Math.Cosh(1), 1);
            thisTest.Verify("tanh_1", Math.Tanh(1), 1);
            thisTest.Verify("sech_1", 1 / Math.Sinh(1), 1);
            thisTest.Verify("csch_1", 1 / Math.Cosh(1), 1);
            thisTest.Verify("coth_1", 1 / Math.Tanh(1), 1);
        }

      
 public void T019_FFI_Math_Others()
        {
            double val = 8.8234893405;
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T019_FFI_Math_Others.ds");            
            thisTest.Verify("ceil_5d5", 6.0, 1);
            thisTest.Verify("floor_5d5", 5.0, 1);
            thisTest.Verify("abs_5", 5.0, 1);
            thisTest.Verify("abs_neg5", 5.0, 1);
            thisTest.Verify("pow_3p4", 81.0, 1);
            thisTest.Verify("exp_3", Math.Exp(3), 1);
            thisTest.Verify("fact_10", 3628800, 1);
            thisTest.Verify("sum_1t100s11", 505.0, 1);
            thisTest.Verify("sum_1t100sn11", 606.0, 1);
            thisTest.Verify("val", val, 1);
            thisTest.Verify("sign_pos", 1, 1);
            thisTest.Verify("sign_zero", 0, 1);
            //thisTest.Verify("sign_neg", -1, 1);
            thisTest.Verify("log10_val", Math.Log10(val), 1);
            thisTest.Verify("logbase_val_5", Math.Log(val, 5), 1);
            thisTest.Verify("max_10", 10.0, 1);
            thisTest.Verify("min_Neg2", -2.0, 1);
            thisTest.Verify("round_val_5", 8.82349, 1);
            thisTest.Verify("round_negval_5", -8.82349, 1);
        }
        
        [Test]
        //public void T020_Sample_Test_1458422_Regress()
        
 public void T020_Sample_Test()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T020_Vector_ByCoordinates.ds");
            Assert.Fail("1467174 - sprint24 : rev 3150 : warning:Function 'get_X' not Found");

            thisTest.Verify("vec_X", 3.0, 0);
            thisTest.Verify("vec_Y", 4.0, 0);
            thisTest.Verify("vec_Z", 0.0, 0);

        }
        [Test]       
 public void T021_Vector_ByCoordinates_1458422_Regress()
        {
            //Assert.Fail("1463747 - Sprint 20 : rev 2147 : FFI issue : exception is thrown when same geometry is assigned to same variable more than once "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T021_Vector_ByCoordinates.ds");
            Assert.Fail("1467174 - sprint24 : rev 3150 : warning:Function 'get_X' not Found");

            thisTest.Verify("vec_X", 0.6, 0);
            thisTest.Verify("vec_Y", 0.8, 0);
            thisTest.Verify("vec_Z", 0.0, 0);
            thisTest.Verify("vec_Normalised", true, 0);

            thisTest.Verify("vec2_X", 1.0, 0); //vec2 is initialized as {3.0, 4.0, 0.0} but reset as {1.0, 2.0, 0.0}
            thisTest.Verify("vec2_Y", 2.0, 0);
            thisTest.Verify("vec2_Z", 0.0, 0);
            thisTest.Verify("vec_len", 2.23606797749979, 0);

            List<Object> vec4 = new List<Object> { 3.0, 0.0, 0.0 };
            List<Object> vec5 = new List<Object> { 3.0, 4.0, 5.0 };
            Assert.IsTrue(mirror.CompareArrays("vec4_coord", vec4, typeof(System.Double))); //updated to vec4=  Vector.ByCoordinates(3.0,0.0,0.0);

            Assert.IsTrue(mirror.CompareArrays("vec5_coord", vec5, typeof(System.Double))); //updated to vec5 =  Vector.ByCoordinates(3.0,4.0,5.0);

            thisTest.Verify("is_same", true, 0);
            thisTest.Verify("is_same2", false, 0);
            thisTest.Verify("is_parallel1", true, 0);
            thisTest.Verify("is_parallel2", true, 0);
            thisTest.Verify("is_parallel3", false, 0);
            thisTest.Verify("is_perp1", true, 0);
            thisTest.Verify("is_perp2", false, 0);
            thisTest.Verify("is_perp2", false, 0);
            thisTest.Verify("dotProduct", 5.0, 0);
            thisTest.Verify("cross_X", 0.0, 0);
            thisTest.Verify("cross_Y", 0.0, 0);
            thisTest.Verify("cross_Z", 1.0, 0);
            thisTest.Verify("newVec_X", 6.0, 0);
            thisTest.Verify("newVec_Y", 8.0, 0);
            thisTest.Verify("newVec_Z", 10.0, 0);
            thisTest.Verify("vec1", null, 0);
            thisTest.Verify("coord_Vec", null, 0); //ComputeGlobalCoords is not defined on Vector class.




        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_Array_Marshal()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(FFIPath, "T022_Array_Marshal.ds");

            thisTest.Verify("sum_1_10", 55.0, 0);
            object[] Expectedresult = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
            object[] Expectedresult2 = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0 };
            thisTest.Verify("twice_arr", Expectedresult2, 0);

        }
    }
}

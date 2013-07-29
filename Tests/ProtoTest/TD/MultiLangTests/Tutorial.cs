using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    class Tutorial
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testCasePath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Tutorial\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category("Replication")]
        public void T00001_Language_001_Variable_Expression_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00001_Language_001_Variable_Expression_1.ds");
            Object[] v1 = new Object[] { 1, 3, 5, 7, 9 };          
            thisTest.Verify("A", v1, 0);
        }

        [Test]
        public void T00002_Language_001a_array_test_4()
        {
            string errmsg = "";// "1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements ";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "T00002_Language_001a_array_test_4.ds", errmsg);
            
            Object[] v1 = new Object[] { 1, 2.600, 4 };            
            thisTest.Verify("collection", v1, 0);

            thisTest.Verify("d", 2.7, 0);

            /*thisTest.Verify("t1", 1, 0);
            thisTest.Verify("t2", 2.6, 0);
            thisTest.Verify("t3", 4, 0);*/
        }

        [Test]
        [Category("Replication")]
        public void T00003_Language_001b_replication_expressions()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00003_Language_001b_replication_expressions.ds");
            
            Object[] v1 = new Object[] { 1, 5, 9 };
            Object[] v2 = new Object[] { new Object[] { 1, 0, -1 }, new Object[] { 6, 5, 4  }, new Object[] { 11, 10, 9 } };
            thisTest.Verify("zipped_sum", v1, 0);
            thisTest.Verify("cartesian_sum", v2, 0);

            /*thisTest.Verify("t1", 1, 0);
            thisTest.Verify("t2", 5, 0);
            thisTest.Verify("t3", 9, 0);

            thisTest.Verify("t4", 1, 0);
            thisTest.Verify("t5", 6, 0);
            thisTest.Verify("t5", 11, 0);*/
        }

        [Test]
        [Category("Replication")]
        public void T00004_Geometry_002_line_by_points_replication_simple()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00004_Geometry_002_line_by_points_replication_simple.ds");
           
            //Object[] v1 = new Object[] { 1, 2, 3, 4, 5 };
            //thisTest.Verify("line_0_StartPoint_X", v1, 0);

            thisTest.Verify("t1", 1.0, 0);
            thisTest.Verify("t2", 2.0, 0);
            thisTest.Verify("t3", 3.0, 0);
            thisTest.Verify("t4", 4.0, 0);
            thisTest.Verify("t5", 5.0, 0);
        }

        [Test]
        [Category("Replication")]
        public void T00005_Geometry_002_line_by_points_replication_simple_correction()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00005_Geometry_002_line_by_points_replication_simple_correction.ds");
            Object[] v1 = new Object[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
               
            thisTest.Verify("line_0_StartPoint_X", v1, 0);
            thisTest.Verify("t1", 1.0, 0);
            thisTest.Verify("t2", 2.0, 0);
            thisTest.Verify("t3", 3.0, 0);
            thisTest.Verify("t4", 4.0, 0);
            thisTest.Verify("t5", 5.0, 0);
        }

        [Test]
        [Category("Replication")]
        public void T00006_Geometry_003_line_by_points_replication_array()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");
            string errmsg = "DNL-1467298 rev 3769 : replication guides with partial array indexing is not supported by parser";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "T00006_Geometry_003_line_by_points_replication_array.ds",errmsg);
           
            thisTest.Verify("line_0_StartPoint_X", 2.0, 0);
          
        }
            
        [Test]
        public void T00007_Geometry_004_circle_all_combinations()
        {
            //Assert.Fail("1456568 - Sprint 16 : Rev 982 : Replication does not work on operators ");
            string errmsg = "";//DNL-1467282 Replication guides not working in constructor of class";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "T00007_Geometry_004_circle_all_combinations.ds", errmsg);
            
            //Object[] v1 = new Object[] { new Object[] { 10.000, 10.000, 10.000, 10.000 }, new Object[] { -5.000, -5.000, -5.000, -5.000 }, new Object[] { -5.000, -5.000, -5.000, -5.000 }, new Object[] { 10.000, 10.000, 10.000, 10.000 } };
            //thisTest.Verify("lines_StartPoint_X", v1, 0);

            thisTest.Verify("t1", 10.0, 0);
            thisTest.Verify("t2", -5.0, 0);
            thisTest.Verify("t3", -5.0, 0);
            thisTest.Verify("t4", 10.0, 0);
        }

        
        [Test]
        public void T00008_Geometry_005_circle_adjacent_pairs_externalised()
        {
            //Assert.Fail("1456568 - Sprint 16 : Rev 982 : Replication does not work on operators "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00008_Geometry_005_circle_adjacent_pairs_externalised.ds");
            
            //Object[] v1 = new Object[] { 10.000, 10.000, -10.000 };
            //thisTest.Verify("lines_StartPoint_X", v1, 0);

            thisTest.Verify("t1", 10.0, 0);
            thisTest.Verify("t2", 10.0, 0);
            thisTest.Verify("t3", -10.0, 0);
        }
           
        [Test]
        [Category("Replication")]
        public void T00009_Geometry_006_circle_all_unique_combinations()
        {
            //Assert.Fail("1456568 - Sprint 16 : Rev 982 : Replication does not work on operators "); 
            //Assert.Fail("1467075 - Sprint23 : rev 2660 : replication with nested array is not working as expected");
            //Assert.Fail("1467174 sprint24 : rev 3150 : warning:Function 'get_X' not Found");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00009_Geometry_006_circle_all_unique_combinations.ds");

            thisTest.Verify("t1", 10.0, 0);
            thisTest.Verify("t2", 12.0, 0);
        }

        [Test]
        public void T00010_Geometry_007_specialPoint_2()
        {
            Assert.Fail("1460783 - Sprint 18 : Rev 1661 : Forward referencing is not being allowed in class property ( related to Update issue ) ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00010_Geometry_007_specialPoint_2.ds");
            
            thisTest.Verify("testLine_SP_X", 0, 0);
            thisTest.Verify("aRadius", 0.95, 0);
            thisTest.Verify("aTheta", 55.0, 0);
            thisTest.Verify("aY", 0.778, 0);
            thisTest.Verify("aX", 0.545, 0);

        }

        [Test]
        [Category("Replication")]
        public void T00011_Geometry_008_trim_then_tube_4()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00011_Geometry_008_trim_then_tube_4.ds");
            
            thisTest.Verify("t1", 8.0, 0);
            thisTest.Verify("t2", 8.0, 0);
            thisTest.Verify("t3", 4.0, 0);
            thisTest.Verify("t4", 6.4, 0);
            thisTest.Verify("t5", 8.0, 0);

        }
        
        [Test]
        public void T00012_Geometry_008a_alternative_method_invocations_1()
        {
            Assert.Fail("1463622 - Sprint 20 : rev 2130 : Usage of Pattern Matching syntax is throwing compiler error ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00012_Geometry_008a_alternative_method_invocations_1.ds");
            
            thisTest.Verify("x1", 0.8, 0);
            thisTest.Verify("x2", 5.6, 0);
            thisTest.Verify("x3", 9.6, 0);
            thisTest.Verify("x4", 17, 0);
            thisTest.Verify("x5", 15.4, 0);
            thisTest.Verify("x6", 27, 0);
            thisTest.Verify("x7", 0.8, 0);
            thisTest.Verify("x8", 11.9, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T00013_Geometry_009_nested_user_defined_feature_2b()
        {
            //Assert.Fail("1456115 - Sprint16: Rev 889 : Replication over a collection is not working as expected ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00013_Geometry_009_nested_user_defined_feature_2b.ds");
            
            thisTest.Verify("x1", 5.0, 0);
            thisTest.Verify("x2", 2.5, 0);
            thisTest.Verify("x3", 1.25, 0);
        }
        
        [Test]
        [Category("Replication")]
        public void T00014_Geometry_010_nested_user_defined_feature_rand_2()
        {
            //Assert.Fail("1460965 - Sprint 18 : Rev 1700 : Design Issue : Accessing properties from collection of class instances using the dot operator should yield a collection of the class properties");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00014_Geometry_010_nested_user_defined_feature_rand_2.ds");
            
            thisTest.Verify("x1", 5.0, 0);
            thisTest.Verify("x2", 2.5, 0);
            thisTest.Verify("x3", 12.5, 0);
        }

        [Test]
        [Category("Feature")]
        public void T00015_Geometry_011_nested_user_defined_feature_with_partial_class_1()
        {
            Assert.Fail("1463623 - Sprint 20 : rev 2130 : partial class is not supported");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00015_Geometry_011_nested_user_defined_feature_with_partial_class_1.ds");
           
            thisTest.Verify("x1", 15.0, 0);
            thisTest.Verify("x2", 5.0, 0);
            thisTest.Verify("x3", 25.0, 0);
            thisTest.Verify("x4", 18.0, 0);
            thisTest.Verify("x5", 5.0, 0);
            thisTest.Verify("x6", 12.5, 0);           
        }

        [Test]
        [Category("Feature")]
        public void T00016_Geometry_012_centroid_1()
        {
            Assert.Fail("1463622 - Sprint 20 : rev 2130 : Usage of Pattern Matching syntax is throwing compiler error "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00016_Geometry_012_centroid_1.ds");
            thisTest.Verify("x1", 50.0, 0);
        }

        [Test]
        [Category("Replication")]
        public void T00017_Geometry_015_Happy_Xmas_2()
        {
            Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T00017_Geometry_015_Happy_Xmas_2.ds");
            
            Object[] v1 = new Object[] { 10.200, -2.556 };
            thisTest.Verify("x1", v1, 0);            
        }
    }
}

using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    class UseCaseTesting
    {
        readonly TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\UseCaseTesting\\";

        [SetUp]
        public void SetUp()
        {
        }


        [Test]
        [Category ("SmokeTest")]
 public void T001_implicit_programming_Robert()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T001_implicit_programming_Robert.ds");
                thisTest.Verify("a", 13);
                thisTest.Verify("b", 26);
                thisTest.Verify("c", 22);
            });

        }

        [Test]
  
 public void T001_implicit_programming_Robert_2()
        {
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T001_implicit_programming_Robert_2.ds");
            thisTest.Verify("a", 13);
            thisTest.Verify("b", 13.0);
            thisTest.Verify("c", 26);
            
        }

        [Test]
        [Category("Replication")]
        public void T002_limits_to_replication_1_Robert()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T002_limits_to_replication_1_Robert.ds");

            Object[] v1 = new Object[] { 1, 1, 1, 0, 0, 0 };
            thisTest.Verify("b", v1);
        }

        [Test]
        public void T003_modifying_members_of_a_collection_abstract_1_Robert()
        {

            Assert.Fail("1463663 - Sprint 20 : rev 2130 : Modifier stack syntax is not matching the specification ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T003_modifying_members_of_a_collection_abstract_1_Robert.ds");
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T004_simple_order_1_Robert()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T004_simple_order_1_Robert.ds");
            thisTest.Verify("a1", 10);
            thisTest.Verify("b1", 20);
            thisTest.Verify("a2", 30);
            thisTest.Verify("b2", 50);
            thisTest.Verify("b", 52);
            thisTest.Verify("a", 82);
        }

        [Test]
        public void T005_modifiers_with_right_assignments_Robert()
        {
            Assert.Fail("1465319 - sprint 21 : [Design Issue] : rev 2301 : update issue with modifier stack ");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T005_modifiers_with_right_assignments_Robert.ds");
            // Verification to be done later, when design issues in the code are sorted out            
        }

        [Test]
        [Category ("SmokeTest")]
 public void T006_grouped_1_Robert()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T006_grouped_1_Robert.ds");
            thisTest.Verify("a", 82);
            thisTest.Verify("b", 52);
            thisTest.Verify("a2", 30);
            thisTest.Verify("b1", 20);
            thisTest.Verify("a1", 10);

        }

        [Test]
        public void T007_surface_trimmed_with_modifier_and_named_states_Robert()
        {
            Assert.Fail("1463663 - Sprint 20 : rev 2130 : Modifier stack syntax is not matching the specification "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T007_surface_trimmed_with_modifier_and_named_states_Robert.ds");
            // Verification to be done later, when design issues in the code are sorted out
        }

        [Test]
        [Category ("SmokeTest")]
 public void T008_long_hand_surface_trim_Robert()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T008_long_hand_surface_trim_Robert.ds");

            thisTest.Verify("test", 4.0);
        }

        [Test]
        public void T009_modifier_test_1_Robert()
        {
            Assert.Fail("1463663 - Sprint 20 : rev 2130 : Modifier stack syntax is not matching the specification "); 
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T009_modifier_test_1_Robert.ds");

            //thisTest.Verify("test", 4.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T010_imperative_if_inside_for_loop_1_Robert()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T010_imperative_if_inside_for_loop_1_Robert.ds");
            thisTest.Verify("x", 18);

        }

        [Test]       
 public void T011_Cyclic_Dependency_From_Geometry()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T011_Cyclic_Dependency_From_Geometry.ds");
            //Assert.Fail("1467186 - sprint24 : rev 3172 : Cyclic dependency detected in update cases");
            thisTest.Verify("test", new Object[] { -30.0, 1.0, 5.0 });

        }

        [Test]
        public void T012_property_test_on_collections_2_Robert()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T012_property_test_on_collections_2_Robert.ds");
            //Assert.Fail("1467186 - sprint24 : rev 3172 : Cyclic dependency detected in update cases");
            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", new Object[] { 1.0, 1.0 } );

        }
        
        [Test]
        public void T013_nested_programming_blocks_1_Robert()
        {
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testPath, "T013_nested_programming_blocks_1_Robert.ds", errmsg);
           
            thisTest.Verify("i", 8);
            thisTest.Verify("totalLength", 12.0 ); 

        }

        [Test]
        public void T014_Robert_2012_09_14_MultipleNestedLanguage()
        {

            string errmsg = "";

            string code =
    @"

            
         def foo  ()
    {   
        t = [Imperative]
        {
              t1 = [Associative]
             {                    
                    t2 = 6;   
                    return = t2; 
            }     
           return = t1;                
        }
        return = t;   
    }
    def foo2  ()
    {   
        t = [Associative]
        {
              t1 = [Imperative]
             {                    
                    t2 = 6;   
                    return = t2; 
            }     
           return = t1;                
        }
        return = t;   
    }
    p1 = foo(); // expected 6, got null
    p2 = foo2();// expected 6, got 6
";


            thisTest.RunScriptSource(code, "");
            


            thisTest.Verify("p1", 6);
            thisTest.Verify("p2", 6);

            //thisTest.Verify("totalLength", 2.0 ); // this needs to be verified after the defect is fixed


        }
    }
}

using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    public class TestImport
    {
        public TestFrameWork thisTest = new TestFrameWork();

        string testCasePath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Import\\";
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        //This served as a sample test case include functionality
        [Category ("SmokeTest")]
 public void T001_BasicImport_CurrentDirectory()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T001_BasicImport_CurrentDirectory.ds");


            object[] expectedC = { 2.2, 4.4 };

            thisTest.Verify("c", expectedC);

        }

        [Test]
        
 public void T002_BasicImport_AbsoluteDirectory()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T002_BasicImport_AbsoluteDirectory.ds");


            object[] expectedC = { 2.2, 4.4 };

            thisTest.Verify("c", expectedC);

        }

        [Test]

        [Category ("SmokeTest")]
 public void T003_BasicImport_ParentPath()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath + "ExtraFolderToTestRelativePath\\", "T003_BasicImport_ParentPath.ds");


            object[] expectedC = { 2.2, 4.4 };

            thisTest.Verify("c", expectedC);

        }

        [Test]

        [Category ("SmokeTest")]
 public void T004_BasicImport_CurrentDirectoryWithDotAndSlash()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T004_BasicImport_CurrentDirectoryWithDotAndSlash.ds");


            object[] expectedC = { 2.2, 4.4 };

            thisTest.Verify("c", expectedC);

        }

        [Test]

        [Category ("SmokeTest")]
 public void T005_BasicImport_RelativePath()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T005_BasicImport_RelativePath.ds");


            object[] expectedC = { 2.2, 4.4 };

            thisTest.Verify("c", expectedC);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T006_BasicImport_TestFunction()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T006_BasicImport_TestFunction.ds");


            object expectedD = 0.500000;

            thisTest.Verify("d", expectedD);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T007_BasicImport_TestClassConstructorAndProperties()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T007_BasicImport_TestClassConstructorAndProperties.ds");


            object myPointX = 10.1;
            object myPointY = 20.2;
            object myPointZ = 30.3;

            thisTest.Verify("myPointX", myPointX);
            thisTest.Verify("myPointY", myPointY);
            thisTest.Verify("myPointZ", myPointZ);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T008_BasicImport_TestClassConstructorAndProperties_UserDefinedClass()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T008_BasicImport_TestClassConstructorAndProperties_UserDefinedClass.ds");




            object startPtX = 10.1;
            object startPtY = 20.2;
            object startPtZ = 30.3;
            object endPtX = 110.1;
            object endPtY = 120.2;
            object endPtZ = 130.3;

            thisTest.Verify("startPtX", startPtX);
            thisTest.Verify("startPtY", startPtY);
            thisTest.Verify("startPtZ", startPtZ);
            thisTest.Verify("endPtX", endPtX);
            thisTest.Verify("endPtY", endPtY);
            thisTest.Verify("endPtZ", endPtZ);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T009_BasicImport_TestClassInstanceMethod()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T009_BasicImport_TestClassInstanceMethod.ds");


            object[] midValue = { 5.05, 10.1, 15.15 };

            thisTest.Verify("midValue", midValue);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T010_BaseImportWithVariableClassInstance_top()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T010_BaseImportWithVariableClassInstance_top.ds");
            object a = 5;
            object b = 10;
            object c = 15;
            object myPointX = 10.1;
            object[] arr = { 20.2, 40.4, 60.6 };

            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("myPointX", myPointX);
            thisTest.Verify("arr", arr);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T012_BaseImportImperative()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T012_BaseImportImperative.ds");
            
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);           
            thisTest.Verify("c", 3);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T013_BaseImportImperative_Bottom()
        {

            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T013_BaseImportImperative_Bottom.ds");
            });
        }

        [Test]
        [Category ("SmokeTest")]
 public void T014_BasicImport_BeforeImperative()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T014_BasicImport_BeforeImperative.ds");
            object[] arr = {20.2, 40.4, 60.6};

            thisTest.Verify("arr", arr);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T015_BasicImport_Middle()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T015_BasicImport_Middle.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T016_BaseImportAssociative()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T016_BaseImportAssociative.ds");
          
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 20);
           
        }

        [Test]
        [Category ("SmokeTest")]
 public void T017_BaseImportWithVariableClassInstance_Associativity()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T017_BaseImportWithVariableClassInstance_Associativity.ds");
         

            thisTest.Verify("a", 10);
            thisTest.Verify("b", 20);
            thisTest.Verify("c", 30);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T018_MultipleImport()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T018_MultipleImport.ds");
            object z = 30.3;
            object[] arr = { 20.2, 40.4, 60.6 };

            thisTest.Verify("z", z);
            thisTest.Verify("arr", arr);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T019_MultipleImport_ClashFunctionClassRedifinition()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T019_MultipleImport_ClashFunctionClassRedifinition.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T020_MultipleImport_WithSameFunctionName()
        {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T020_MultipleImport_WithSameFunctionName.ds");
             thisTest.Verify("b", 6, 0);           

        }

        [Test]
        [Category ("SmokeTest")]
 public void T021_Defect_1457354()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>            {

                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T021_Defect_1457354.ds");
            });            

        }

        [Test]
        [Category ("SmokeTest")]
 public void T021_Defect_1457354_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T021_Defect_1457354_2.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T021_Defect_1457354_3()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T021_Defect_1457354_3.ds");
            });

        }

        [Test]
        [Category ("SmokeTest")]
 public void T022_Defect_1457740()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T022_Defect_1457740.ds");
            thisTest.Verify("b", 6, 0);

        }
    }
}

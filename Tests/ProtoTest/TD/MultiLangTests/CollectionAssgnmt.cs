using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    public class CollectionAssgnmt
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string filePath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Collection\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void Collection_Assignment_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Collection_Assignment_1.ds");

          
            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 2, 2 };

            thisTest.Verify("c", -2, 0);
            thisTest.Verify("d", expectedResult2, 0);
            thisTest.Verify("e", expectedResult3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Collection_Assignment_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Collection_Assignment_2.ds");

            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 3, -4 };

            thisTest.Verify("c", expectedResult2, 0);
            thisTest.Verify("d", expectedResult3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Collection_Assignment_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Collection_Assignment_3.ds");

            object[] expectedResult2 = { 2, 0 };
            object[] expectedResult3 = { 4, -6 };

            thisTest.Verify("c", expectedResult2, 0);
            thisTest.Verify("d", expectedResult3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Collection_Assignment_4()
        {  
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Collection_Assignment_4.ds");

            object[] expectedResult = { 2,3,4 };

            thisTest.Verify("c", expectedResult, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Collection_Assignment_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Collection_Assignment_5.ds");

            object[] expectedResult1 = { 1, 2, 2 };
            object[] expectedResult2 = { 1,2,3 };
            object[] expectedResult3 = { 0,1,2 };
            object[] expectedResult4 = { -2, 1, 2 };

            thisTest.Verify("b", expectedResult2);
            thisTest.Verify("d", expectedResult1);
            thisTest.Verify("e", expectedResult4);
            thisTest.Verify("c", expectedResult3);
        }




    }
}

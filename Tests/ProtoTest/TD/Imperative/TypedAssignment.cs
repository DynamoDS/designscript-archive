using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Imperative
{
    class TypedAssignment
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\TypedAssignment\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T01_TestVariousTypes()
        {
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_TestVariousTypes.ds");

            thisTest.Verify("i1", 5);
            thisTest.Verify("d1", 5.2);
            thisTest.Verify("isTrue1", true);
            thisTest.Verify("isFalse1", false);
            thisTest.Verify("x1", 3);
            thisTest.Verify("b1", 5);
            thisTest.Verify("x11", 2);
            thisTest.Verify("y1", 2.0);

            thisTest.Verify("i2", 5);
            thisTest.Verify("d2", 5.2);
            thisTest.Verify("isTrue2", true);
            thisTest.Verify("isFalse2", false);
            thisTest.Verify("x2", 3);
            thisTest.Verify("b2", 5);
            thisTest.Verify("x12", 2);
            thisTest.Verify("y2", 2.0);


        }

    }
}

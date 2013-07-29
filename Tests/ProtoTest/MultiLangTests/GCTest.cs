using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;

namespace ProtoTest.MultiLangTests
{
    class GCTest
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();

        string testCasePath = "..\\..\\..\\Scripts\\MultiLangTests\\GCTest\\";

        [SetUp]
        public void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }

        [Test]
        public void T01_TestGCArray()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T01_TestGCArray.ds");
            thisTest.Verify("v1", 4);
            thisTest.Verify("v2", 5);
            thisTest.Verify("v3", 6);
        }
        [Test]
        public void T02_TestGCEndofIfBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T02_TestGCEndofIfBlk.ds");
            thisTest.Verify("v", 3);
        }
        [Test]
        public void T03_TestGCEndofLangBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T03_TestGCEndofLangBlk.ds");
            thisTest.Verify("v1", 2);
            thisTest.Verify("v2", 3);
        }
        [Test]
        public void T04_TestGCReturnFromLangBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T04_TestGCReturnFromLangBlk.ds");
            thisTest.Verify("v1", 1);
            thisTest.Verify("v2", 2);
        }
        [Test]
        public void T05_TestGCReturnFromFunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T05_TestGCReturnFromFunction.ds");
            thisTest.Verify("v1", 3);
            thisTest.Verify("v2", 4);
        }
        [Test]
        public void T06_TestGCEndofWhileBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T06_TestGCEndofWhileBlk.ds");
            thisTest.Verify("v1", 4);
            thisTest.Verify("v2", 4);
            thisTest.Verify("v3", 7);
        }
        [Test]
        public void T07_TestGCEndofForBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T07_TestGCEndofForBlk.ds");
            thisTest.Verify("v1", 4);
            thisTest.Verify("v2", 4);
            thisTest.Verify("v3", 7);
        }
        [Test]
        public void T08_TestGCArray02()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T08_TestGCArray02.ds");
            thisTest.Verify("v1", 1);
            thisTest.Verify("v2", 4);
            thisTest.Verify("v3", 4);
            thisTest.Verify("v4", 4);
            thisTest.Verify("v5", 5);
            thisTest.Verify("v6", 6);
            thisTest.Verify("v7", 7);
        }
        [Test]
        public void T09_TestGCPassingArguments()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T09_TestGCPassingArguments.ds");
            thisTest.Verify("v1", 1);
            thisTest.Verify("v2", 1);
            thisTest.Verify("v3", 2);
            thisTest.Verify("v4", 5);
            thisTest.Verify("v5", 6);
            thisTest.Verify("v6", 9);
        }
        [Test]
        public void T10_TestGCReturnArguments()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T10_TestGCReturnArguments.ds");
            thisTest.Verify("v1", 3);
            thisTest.Verify("v2", 4);
        }

        [Test]
        public void T11_TestGCLangBlkInFunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T11_TestGCLangBlkInFunction.ds");
            thisTest.Verify("v1", 1);
        }

        [Test]
        public void T12_TestGCIfElseInFunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T12_TestGCIfElseInFunction.ds");
            thisTest.Verify("v1", 5);
            thisTest.Verify("v2", 7);
        }

        [Test]
        public void T13_GCTestComplexCase()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T13_GCTestComplexCase.ds");
            thisTest.Verify("v1", 1);
        }
    }
}

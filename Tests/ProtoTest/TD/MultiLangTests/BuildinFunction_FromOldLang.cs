using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoCore.Lang.Replication;
namespace ProtoTest.TD.MultiLangTests
{
    class BuildinFunction_FromOldLang
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\BuildinFunctionFromOldLang\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void T80580_BuiltinFunc_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T80580_BuiltinFunc_1.ds");
            Object[] v1 = new Object[] { 1, 2, 3, 4, 0 };
            Object[] v2 = new Object[] { 3, 4 };
            Object[] v3 = new Object[] { -4, 5 };
            Object[] v4 = new Object[] { v2, v3 };


            thisTest.Verify("t1", v1);
            thisTest.Verify("t2",3);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", 2);
            thisTest.Verify("t5", 2);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
            thisTest.Verify("t8", "{true,{false,false},true}");
            thisTest.Verify("t9", v4);

            
        }
    }
}

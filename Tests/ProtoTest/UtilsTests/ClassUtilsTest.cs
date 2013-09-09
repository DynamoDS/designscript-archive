using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
using ProtoFFI;

namespace ProtoTest.UtilsTests
{
    [TestFixture]
    public class ClassUtilsTest
    {
        public ProtoCore.Core core;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Setup");
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            //DLLFFIHandler.Env = ProtoFFI.CPPModuleHelper.GetEnv();
            //DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper());

        }

        [Test]
        public void GetUpcastChainTest()
        {
            String code =
@"class A {}
class B extends A {}
class C extends B {}


";

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);

            int idA = core.DSExecutable.classTable.IndexOf("A");
            int idB = core.DSExecutable.classTable.IndexOf("B");
            int idC = core.DSExecutable.classTable.IndexOf("C");

            ClassNode cnA = core.DSExecutable.classTable.ClassNodes[idA];
            ClassNode cnB = core.DSExecutable.classTable.ClassNodes[idB];
            ClassNode cnC = core.DSExecutable.classTable.ClassNodes[idC];

            List<int> idsA = ClassUtils.GetClassUpcastChain(cnA, core);
            List<int> idsB = ClassUtils.GetClassUpcastChain(cnB, core);
            List<int> idsC = ClassUtils.GetClassUpcastChain(cnC, core);

            Assert.IsTrue(idsA.Count == 2);
            Assert.IsTrue(idsA.Contains(idA));
            

            Assert.IsTrue(idsB.Count == 3);
            Assert.IsTrue(idsB.Contains(idA));
            Assert.IsTrue(idsB.Contains(idB));

            Assert.IsTrue(idsC.Count == 4);
            Assert.IsTrue(idsC.Contains(idA));
            Assert.IsTrue(idsC.Contains(idB));
            Assert.IsTrue(idsC.Contains(idC));


        }

    }
}

using NUnit.Framework;

namespace ProtoTest
{
    [TestFixture]
    public class ProtoScriptTest
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void BasicInfrastructureTest()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            ProtoLanguage.CompileStateTracker compileState = null;

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();


            fsr.Execute(
@"
[Imperative]
{
	x = 987654321;
	[Associative]
	{
		 px = 1234321;
	}
}
", core, out compileState);
        }


    }

    [TestFixture]
    public class MultiLangNegitiveTests
    {

        //Negitive Tests with distortions of the Language def block
        [Test]
        public void ParserFailTest1()
        {
            ProtoLanguage.CompileStateTracker compileState = null;
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();

            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                fsr.Execute(
    @"
[imperative
{
    a = 3
}
", core, out compileState);
            });
        }

        [Test]
        public void ParserFailTest2()
        {
            ProtoLanguage.CompileStateTracker compileState = null;
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();


            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                fsr.Execute(
    @"
[
{
    a = 3
}
", core, out compileState);
            });
        }

        [Test]
        public void ParserFailTest3()
        {
            ProtoLanguage.CompileStateTracker compileState = null;
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();


            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {

                fsr.Execute(
    @"
[associative]
{
	a = 1;
	
}
", core, out compileState);
            });
        }



    }
}

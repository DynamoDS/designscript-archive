using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScript.Editor.CodeGen;

namespace DesignScript.Editor.Core.UnitTest
{
    class FunctionToolTipsApiTest
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void TestBasicIdentifierList()
        {
            string code = @"
class A
{
    public x : var;
    public static y : var;
    private z : var;
    protected static w : var;

    public constructor A()
    {
        
    }
    public def foo : int()
    {
        return = 10;
    }
}

class B
{
    public a : A;
    public static b : A;
    constructor B()
    {
        
    }
}
b = B.B();
";
            AutoCompletionHelper.Reset();
            AutoCompletionHelper.Compile(code, null);
            List<AutoCompletionHelper.MethodSignature> sigs = AutoCompletionHelper.GetMethodParameterList(30, 10, "b.a.foo", null);
            List<AutoCompletionHelper.MethodSignature> sigs2 = AutoCompletionHelper.GetMethodParameterList(30, 10, "B.b.foo", null);
            Assert.IsTrue(sigs.Count == 1);
            Assert.IsTrue(sigs[0].MethodName == "A.foo");
            Assert.IsTrue(sigs2.Count == 1);
            Assert.IsTrue(sigs2[0].MethodName == "A.foo");
        }

        [Test]
        public void TestFunctionInsideClass()
        {
            string code = @"
class A
{
	x : var;
	constructor A()
	{
		x = 10;
	}
	
	def foo : int(a : int, b : int)
	{
		return = a + b;
	}
	
	def bar : int(a : int, b : int)
	{
	
	}
}";

            AutoCompletionHelper.Reset();
            AutoCompletionHelper.Compile(code, null);
            List<AutoCompletionHelper.MethodSignature> sigs = AutoCompletionHelper.GetMethodParameterList(17, 10, "foo", null);
            List<AutoCompletionHelper.MethodSignature> sigs2 = AutoCompletionHelper.GetMethodParameterList(17, 10, "this.foo", null);
            Assert.IsTrue(sigs.Count == 1);
            Assert.IsTrue(sigs[0].MethodName == "A.foo");
            Assert.IsTrue(sigs2.Count == 1);
            Assert.IsTrue(sigs2[0].MethodName == "A.foo");
        }
    }
}

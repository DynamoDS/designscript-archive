using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;

namespace ProtoTest.MultiLangTests
{
    class FunctionPointerTest
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();

        string testCasePath = "..\\..\\..\\Scripts\\MultiLangTests\\FunctionPointerTest\\";

        [SetUp]
        public void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }

        [Test]
        public void T01_BasicGlobalFunction()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T01_BasicGlobalFunction.ds");
            object b = 3;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T02_GlobalFunctionWithDefaultArg()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T02_GlobalFunctionWithDefaultArg.ds");
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T03_GlobalFunctionInAssocBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T03_GlobalFunctionInAssocBlk.ds");
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T04_GlobalFunctionInImperBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T04_GlobalFunctionInImperBlk.ds");
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T05_ClassMemerVarAsFunctionPointer()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T05_ClassMemerVarAsFunctionPointer.ds");
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T05_ClassMemerVarAsFunctionPointerDefaultArg()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T05_ClassMemerVarAsFunctionPointer.ds");
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T06_ClassMemerVarAsFunctionPointerAssocBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T06_ClassMemerVarAsFunctionPointerAssocBlk.ds");
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T07_ClassMemerVarAsFunctionPointerImperBlk()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T07_ClassMemerVarAsFunctionPointerImperBlk.ds");
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T08_FunctionPointerUpdateTest()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T08_FunctionPointerUpdateTest.ds");
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T09_NegativeTest_Non_FunctionPointer()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T09_NegativeTest_Non_FunctionPointer.ds");
            object b = null;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T10_NegativeTest_UsingFunctionNameAsVarName_Global()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T10_NegativeTest_UsingFunctionNameAsVarName_Global.ds");
            });
        }

        [Test]
        public void T11_NegativeTest_UsingFunctionNameAsVarName_Global_ImperBlk()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T11_NegativeTest_UsingFunctionNameAsVarName_Global_ImperBlk.ds");
            });
        }

        [Test]
        public void T12_NegativeTest_UsingGlobalFunctionNameAsMemVarName_Class()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T12_NegativeTest_UsingGlobalFunctionNameAsMemVarName_Class.ds");
            });
        }

        [Test]
        public void T13_NegativeTest_UsingMemFunctionNameAsMemVarName_Class()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T13_NegativeTest_UsingMemFunctionNameAsMemVarName_Class.ds");
            });
        }

        [Test]
        [Category("Negative")]
        public void T14_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T14_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr.ds");
            });
        }

        [Test]
        public void T15_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr_Global_ImperBlk()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T15_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr_Global_ImperBlk.ds");
            });
        }

        [Test]
        public void T16_NegativeTest_UsingMemFunctionAsFunctionPtr()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T16_NegativeTest_UsingMemFunctionAsFunctionPtr.ds");
            object x = 2;
            object y = null;
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }

        [Test]
        public void T17_PassFunctionPointerAsArg()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T17_PassFunctionPointerAsArg.ds");
            object a = 2;
            object c = 3;
            thisTest.Verify("a", a);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T18_FunctionPointerAsReturnVal()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T18_FunctionPointerAsReturnVal.ds");
            object b = 2;
            object c = 3;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T19_NegativeTest_PassingFunctionPtrAsArg_CSFFI()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T19_NegativeTest_PassingFunctionPtrAsArg_CSFFI.ds");
            object a = null;
            thisTest.Verify("a", a);
        }

        [Test]
        public void T20_FunctionPtrUpdateOnMemVar_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T20_FunctionPtrUpdateOnMemVar_1.ds");
            object b = 3;
            thisTest.Verify("b", b);
        }

        [Test]
        [Category("Method Resolution")]
        public void T21_FunctionPtrUpdateOnMemVar_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "T21_FunctionPtrUpdateOnMemVar_2.ds");
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T22_FunctionPointer_Update()
        {
            string code = @"
class A
{
x;
}

def foo(x)
{
    return = 2 * x;
}

def bar(x, f)
{
    return = f(x);
}

x = 100;
a = A.A();
a.x = x;
x = bar(x, foo);
t = a.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 200);
        }

        [Test]
        public void T22_FunctionPointerArray()
        {
            string code = @"
def foo(x)
{
    return = 2 * x;
}

fs = {foo, foo};
r = fs[0](100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T23_FunctionPointerAsReturnValue()
        {
            string code = @"
def foo(x)
{
    return = 2 * x;
}


def bar(i)
{
    return = foo;
}


fs = bar(0..1);
f = fs[0];
r = f(100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T24_FunctionPointerAsReturnValue2()
        {
            string code = @"
def foo(x)
{
    return = 2 * x;
}


def bar:function[]()
{
    return = {foo, foo};
}


fs = bar();
f = fs[0];
r = f(100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T25_FunctionPointerTypeConversion()
        {
            string code = @"
def foo:int(x)
{
    return = 2 * x;
}


def bar:var[]()
{
    return = {foo, foo};
}


fs = bar();
f = fs[0];
r = f(100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T26_NestedFunctionPointer()
        {
            string code = @"
def foo(x)
{
    return = 2 * x;
}

def bar(x)
{
    return = 3 * x;
}

def ding(x, f1:var, f2:var)
{
    return = f1(f2(x));
}

x = 1;
r = ding(x, foo, bar);
x = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 12);
        }

        [Test]
        public void T27_FunctionPointerDefaultParameter()
        {
            string code = @"
def foo(x, y = 10, z = 100)
{
    return = x + y + z;
}

def bar(x, f)
{
    return = f(x);
}

r = bar(1, foo);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 111);
        }

        [Test]
        public void T28_FunctionPointerInInlineCond()
        {
            string code = @"
def foo(x, y = 10, z = 100)
{
    return = x + y + z;
}

def bar(x, y = 2, z = 3)
{
    return = x * y * z;
}

def ding(i, f, b)
{
    return = (i > 0) ? f(i) : b(i);
}

r1 = ding(1, foo, bar);
r2 = ding(-1, foo, bar);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 111);
            thisTest.Verify("r2", -6);
        }

        [Test]
        public void T29_FunctionPointerInInlineCond()
        {
            string code = @"
def foo(x, y = 10, z = 100)
{
    return = x + y + z;
}

def ding(i, f)
{
    return = (i > 0) ? f(i) : f;
}

r1 = ding(1, foo);
r2 = ding(-1, 100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 111);
            thisTest.Verify("r2", 100);
        }

        [Test]
        public void T30_TypeConversion()
        {
            string code = @"
def foo()
{
    return = null;
}

t1:int = foo;
t2:int[] = foo;
t3:char = foo;
t4:string = foo;
t5:bool = foo; 
t6:function = foo;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", null);
            thisTest.Verify("t2", new object[] {null});
            thisTest.Verify("t3", null);
            thisTest.Verify("t4", null);
            thisTest.Verify("t5", null);
        }

        [Test]
        public void T31_UsedAsMemberVariable()
        {
            string code = @"
class A
{
    f;
    x;
    constructor A(_x, _f)
    {
        x = _x;
        f = _f;
    }

    def update()
    {
        x = f(x);
        return = null;
    }
}

def foo(x)
{
    return = 2 * x;
}

def bar(x)
{
    return = 3 * x;
}

a = A.A(2, foo);
r = a.update1();
a.f = bar;
r = a.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 12);
        }
    }
}

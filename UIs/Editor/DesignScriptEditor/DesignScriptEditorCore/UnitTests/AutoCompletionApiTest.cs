using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScript.Editor.CodeGen;

namespace DesignScript.Editor.Core.UnitTest
{
    class AutoCompletionApiTest
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void TestInsideGlobalScope()
        {
            string code = @"
class Point
{
    x : var;
    y : var;
    z : var;
    
    constructor Point()
    {
        x = 1;
        y = 2;
        z = 3;
    }
    
    def foo : sum()
    {
        return = x + y + z;
    }
}

p = Point.Point();  // This is line 20
                    // This is line 21
";

            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(22, 1, code, "p", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list2 = AutoCompletionHelper.GetList(22, 1, code, "Point", null);
            Assert.AreEqual(4, list1.Count); // x, y, z, sum
            Assert.AreEqual(1, list2.Count); // Point
            Assert.IsTrue((list2[0].Key & AutoCompletionHelper.MemberType.Constructor) != 0);
        }

        [Test]
        public void TestInsideClassMemberFunctionScopeAndThisKeyword()
        {
            string code = @"
class Point
{
    private x : var;
    protected static w : var;
    y : var;
    z : var;
    
    constructor Point()
    {
        x = 1;
        y = 2;
        z = 3;
    }
    
    def foo : sum()
    {
        p = Point.Point();
        // This is line 18
        return = x + y + z;
    }
}

p = Point.Point();
";

            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(18, 1, code, "Point", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list2 = AutoCompletionHelper.GetList(18, 1, code, "p", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list3 = AutoCompletionHelper.GetList(18, 1, code, "this", null);
            Assert.AreEqual(2, list1.Count); // w, Point
            Assert.AreEqual(4, list2.Count); // x, y, z, foo
            Assert.AreEqual(4, list3.Count); // x, y, z, foo
        }
        [Test]
        public void TestAutoCompleteInternalMethods()
        {
            string code = @"
class A
{
    constructor A(i)
    {
    }
    
    def foo()
    {
    }
}

p = A.A({ 1, 2, 3 });
q = A.A();
";

            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(14, 1, code, "p", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list2 = AutoCompletionHelper.GetList(15, 1, code, "q", null);
            Assert.AreEqual(1, list1.Count); // foo
            Assert.AreEqual(1, list2.Count); // foo
        }
        [Test]
        public void TestRankedAndMixedTypeArray01()
        {
            string code = @"
class A
{
    constructor A(i){        
    }
    def foo()
    {
    }
}

class B
{
    constructor B()
    {
    }
    def faa()
    {
    }
    def foo()
    {
    }
    static def bar()
    {
    }
}

arr = { A.A(), 1, { 2, B.B() }, 5 };//line 26

";
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(28, 1, code, "arr", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list2 = AutoCompletionHelper.GetList(28, 1, code, "arr[0]", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list3 = AutoCompletionHelper.GetList(28, 1, code, "arr[2][1]", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list4 = AutoCompletionHelper.GetList(28, 1, code, "B", null);
            Assert.AreEqual(1, list1.Count); // foo
            Assert.AreEqual(1, list2.Count); // foo
            Assert.AreEqual(2, list3.Count); // faa,foo
            Assert.AreEqual(2, list4.Count); // B,bar
        }

        [Test]
        public void TestRankedAndMixedTypeArray02()
        {
            string code = @"
class ClassOne
{
    constructor ByValues(values : int[])
    {
    }
    
    def DoubleTheValue : double(value : double)
    {
        return = value * 2.0;
    }
}

class ClassTwo
{
    luckValue : int;
    constructor ByLuck(values : int[])
    {
        luckValue = values[0];
    }
    
    def GetLuckLevel : int()
    {
        return = luckValue;
    }
}

first = ClassOne.ByValues({ 1, 2 }); // Line #28
second = ClassTwo.ByLuck({ 1, 2 });  // Line #29

abracadabra = 3;

third = { first, second, { second, first } }; // Line #33


";
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> completionList = null;
            completionList = AutoCompletionHelper.GetList(30, 1, code, "first", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count);
            Assert.AreEqual("DoubleTheValue", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(30, 1, code, "second", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(2, completionList.Count);
            Assert.AreEqual("luckValue", completionList[0].Value);
            Assert.AreEqual("GetLuckLevel", completionList[1].Value);

            completionList = AutoCompletionHelper.GetList(35, 1, code, "third", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count); // Type defaulted to "first" ("ClassOne").
            Assert.AreEqual("DoubleTheValue", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(35, 1, code, "third[0]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count); // Type defaulted to "first" ("ClassOne").
            Assert.AreEqual("DoubleTheValue", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(35, 1, code, "third[1]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(2, completionList.Count);
            Assert.AreEqual("luckValue", completionList[0].Value);
            Assert.AreEqual("GetLuckLevel", completionList[1].Value);

            completionList = AutoCompletionHelper.GetList(35, 1, code, "third[100]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count); // Out-of-bound defaulted to "first" ("ClassOne").
            Assert.AreEqual("DoubleTheValue", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(35, 1, code, "third[2][0]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(2, completionList.Count); // Nested "ClassTwo" type.
            Assert.AreEqual("luckValue", completionList[0].Value);
            Assert.AreEqual("GetLuckLevel", completionList[1].Value);

            completionList = AutoCompletionHelper.GetList(35, 1, code, "third[2][1]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count); // Nested "ClassOne" type.
            Assert.AreEqual("DoubleTheValue", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(35, 1, code, "third[2][100]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count); // Nested out-of-bound defaulted to "ClassOne" type.
            Assert.AreEqual("DoubleTheValue", completionList[0].Value);
        }

        [Test]
        public void TestRankedAndMixedTypeArray03()
        {
            string code = @"
class Aaah
{
    constructor Oooh()
    {
    }
    
    def Yeeeah : int()
    {
        return = 23;
    }
}

class Beee
{
    constructor Booo()
    {
        a = { 1.0, Aaah.Oooh() };
        
        // Line #20
    }
}

c = { 1.0, Aaah.Oooh() };

// Line #26

";
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> completionList = null;
            completionList = AutoCompletionHelper.GetList(20, 1, code, "a", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(0, completionList.Count);

            completionList = AutoCompletionHelper.GetList(20, 1, code, "a[0]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(0, completionList.Count);

            completionList = AutoCompletionHelper.GetList(20, 1, code, "a[1]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count);
            Assert.AreEqual("Yeeeah", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(20, 1, code, "a[2]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(0, completionList.Count);

            completionList = AutoCompletionHelper.GetList(26, 1, code, "c", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(0, completionList.Count);

            completionList = AutoCompletionHelper.GetList(26, 1, code, "c[0]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(0, completionList.Count);

            completionList = AutoCompletionHelper.GetList(26, 1, code, "c[1]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count);
            Assert.AreEqual("Yeeeah", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(26, 1, code, "c[2]", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(0, completionList.Count);
        }

        [Test]
        public void TestRangeExpression()
        {
            string code = @"
class Range
{
    constructor Expression(a : int, b : int)
    {
        return = a + b;
    }
    
    def DoNothing()
    {
    }
}

a = Range.Expression(1..5..1, 2..5..1);
b = Range.Expression(1..5..1, 2..5);




// Line #20";

            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> completionList = null;
            completionList = AutoCompletionHelper.GetList(20, 1, code, "a", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count);
            Assert.AreEqual("DoNothing", completionList[0].Value);

            completionList = AutoCompletionHelper.GetList(20, 1, code, "b", null);

            Assert.AreNotEqual(null, completionList);
            Assert.AreEqual(1, completionList.Count);
            Assert.AreEqual("DoNothing", completionList[0].Value);
        }

        [Test]
        public void TestNameLoopupInLanguageBlock()
        {
            string code = @"
class Point
{
    private x : var;
    protected static w : var;
    y : var;
    z : var;
    
    constructor Point()
    {
        x = 1;
        y = 2;
        z = 3;
    }
    
    def foo : sum()
    {
        p = Point.Point();
        // This is line 18
        return = x + y + z;
    }
}

p = Point.Point();

[Imperative]
{
    // This is line 27
}

def foo : int()
{
    p1 = Point.Point();
    [Imperative]
    {
        // This is line 35
    }
    
    return = 10;
}

";
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(18, 1, code, "p", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list2 = AutoCompletionHelper.GetList(27, 1, code, "p", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list3 = AutoCompletionHelper.GetList(35, 1, code, "p1", null);
            Assert.AreEqual(4, list1.Count); // x, y, z, sum
            Assert.AreEqual(3, list2.Count); // y, z, sum
            Assert.AreEqual(3, list3.Count); // y, z, sum
        }

        [Test]
        public void TestClassInheritance()
        {
            string code = @"
class Base
{
    public x : var;
    protected y : var;
    private z : var;
    
    public def foo1 : int()
    {
        return = 10;
    }
    
    protected def foo2 : int()
    {
        return = 10;
    }
    
    private def foo3 : int()
    {
        return = 10;
    }
    
    public static def foo4 : int()
    {
        return = 10;
    }
    
    protected static def foo5 : int()
    {
        return = 10;
    }
    
    private static def foo6 : int()
    {
        return = 11;
    }
    
    public constructor Base()
    {
        // This is line 39
    }
}

class Derived extends Base
{
    public w : var;
    
    public constructor Derived()
    {
        // This is line 49
    }
    
    public def bar1 : int()
    {
        return = 101;
    }
    
    protected def bar2 : int()
    {
        return = 102;
    }
    
    private def bar3 : int()
    {
        return = 103;
    }
    
    public static def bar4 : int()
    {
        return = 104;
    }
    
    protected static def bar5 : int()
    {
        return = 105;
    }
    
    private static def bar6 : int()
    {
        return = 106;
    }
}
";
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(39, 1, code, "this", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list2 = AutoCompletionHelper.GetList(39, 1, code, "Base", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list3 = AutoCompletionHelper.GetList(49, 1, code, "this", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list4 = AutoCompletionHelper.GetList(49, 1, code, "Derived", null);
            Assert.AreEqual(6, list1.Count); // x, y, z, foo1, foo2, foo3
            Assert.AreEqual(4, list2.Count); // foo4, foo5, foo6, Base
            Assert.AreEqual(8, list3.Count); // x, y, w, foo1, foo2, bar1, bar2, bar3
            Assert.AreEqual(6, list4.Count); // foo4, foo5, bar4, bar5, bar6, Derived
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
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(25, 10, code, "this.a", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list2 = AutoCompletionHelper.GetList(25, 10, code, "B.b", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list3 = AutoCompletionHelper.GetList(29, 10, code, "b.a", null);
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list4 = AutoCompletionHelper.GetList(29, 10, code, "B.b", null);

            Assert.IsTrue(list1.Count == 2); // Only public 'x' and 'foo'.
            Assert.IsTrue(list2.Count == 2); // Only public 'x' and 'foo'.
            Assert.IsTrue(list3.Count == 2); // Only public 'x' and 'foo'.
            Assert.IsTrue(list4.Count == 2); // Only public 'x' and 'foo'.
        }

        [Test]
        public void TestDoubleDotCrash001()
        {
            string code = @"
class Sample
{
    x : var;
    constructor Sample()
    {
        x = 10;
    }
}

// S = Sample..

";

            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(11, 5, code, "Sample", null);
            Assert.IsTrue(list1.Count == 1 && list1[0].Value == "Sample");
        }

        [Test]
        public void TestDoubleDotCrash002()
        {
            string code = @"a = 2..";
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list1 = AutoCompletionHelper.GetList(1, 6, code, ".", null);
            Assert.IsTrue(list1.Count == 0);
        }

        [Test]
        public void TestInnerBlockCrash()
        {
            string code =
                "[Imperative]\n" +
                "{\n" +
                "    if (0 < 10)\n" +
                "    {\n" +
                "        a = foo\n" +
                "    }\n" +
                "\n" +
                "    return = 0;\n" +
                "}\n";

            AutoCompletionHelper.Reset();
            AutoCompletionHelper.Compile(code, null);

            List<AutoCompletionHelper.MethodSignature> signatures = null;
            signatures = AutoCompletionHelper.GetMethodParameterList(4, 15, "foo", null);
            Assert.AreNotEqual(null, signatures);
            Assert.AreEqual(0, signatures.Count); // "foo" is undefined!
        }
    }
}

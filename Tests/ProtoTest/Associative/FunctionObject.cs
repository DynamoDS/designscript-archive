﻿using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    public class FunctionObjectTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestApply01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, 42);
fo = _SingleFunctionObject(add, 2, {1}, {null, 42});
r = Apply(fo, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 45);
        }

        [Test]
        public void TestApply02()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y,z) { return = x + y + z;}

// foo1 = add(?, 42, ?);
fo1 = _SingleFunctionObject(add, 3, {1}, {null, 42, null});

// foo2 = add(100, 42, ?);
fo2 = Apply(fo1, 100);

r = Apply(fo2, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 145);
        }

        [Test]
        public void TestApply03()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, {100, 200});
fo = _SingleFunctionObject(add, 2, {1}, {null, {100, 200}});
r = Apply(fo, {1, 2});
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 101, 202 });
        }

        [Test]
        public void TestApply04()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, {100, 200});
fo = _SingleFunctionObject(add, 2, {1}, {null, {100, 200}});
r = Apply(fo, 1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 101, 201 });
        }

        [Test]
        public void TestApply05()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, {100, 200});
fo = _SingleFunctionObject(add, 2, {1}, {null, {100, 200}});
r = Apply(fo, {1});
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 101 });
        }

        [Test]
        public void TestApply06()
        {
            // Return a function object from function
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

def getFunctionObject()
{
    return = _SingleFunctionObject(add, 2, {1}, {null, 100});
}

fo = getFunctionObject();

r = Apply(fo, 42);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 142);
        }


        [Test]
        public void TestApply07()
        {
            // Return a function object from function
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x,y) { return = x * y;}
def getFunctionObject(f:function)
{
    return = _SingleFunctionObject(f, 2, {1}, {null, 100});
}

fo1 = getFunctionObject(add);
r1 = Apply(fo1, 42);
fo2 = getFunctionObject(mul);
r2 = Apply(fo2, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 142);
            thisTest.Verify("r2", 300);
        }

        [Test]
        public void TestApply08()
        {
            // Return a function object from function
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x,y) { return = x * y;}
def getFunctionObject(f:function)
{
    return = _SingleFunctionObject(f, 2, {1}, {null, 100});
}

fo = getFunctionObject({add, mul});
r = Apply(fo, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {103, 300});
        }

        [Test]
        public void TestCompose01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x, y) { return = x * y;}

fo1 = _SingleFunctionObject(add, 2, {1}, {null, 100});
fo2 = _SingleFunctionObject(mul, 2, {0}, {3, null});
fo3 = _ComposedFunctionObject({fo1, fo2});

// r = 2 * 3 + 100
r = Apply(fo3, 2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 106);
        }

        [Test]
        public void TestCompose02()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x, y) { return = x * y;}

fo1 = _SingleFunctionObject(add, 2, {1}, {null, 100});
fo2 = _ComposedFunctionObject({fo1, fo1});

// r = 42 + 100 + 100
r = Apply(fo2, 42);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 242);
        }

        [Test]
        public void TestCompose03()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x, y) { return = x + y; }

def mul(x, y) { return = x * y; }

fo1 = _SingleFunctionObject(add, 2, { 1 }, { null, 3});
fo2 = _SingleFunctionObject(mul, 2, { 0 }, { 5, null });

r1 = Apply(fo1, 7);     // 3 + 7
r2 = Apply(fo2, 11);    // 5 * 11

comp1 = Compose({ fo1, fo2 }); 
r3 = Apply(comp1, 11);  // (5 * 11) + 3

comp2 = Compose({ fo2, fo1 });
r4 = Apply(comp2, 7);         // 5 * (3 + 7)

comp3 = Compose({ comp1, fo1, fo2 });
r5 = Apply(comp3, 9);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 10);
            thisTest.Verify("r2", 55);
            thisTest.Verify("r3", 58);
            thisTest.Verify("r4", 50);
            thisTest.Verify("r5", 243);
        }
    }
}

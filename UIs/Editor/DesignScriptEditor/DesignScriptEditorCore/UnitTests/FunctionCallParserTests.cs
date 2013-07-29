using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    using FunctionCallParser;
    using System.IO;

    class FunctionCallParserTests
    {
        [SetUp]
        public void setup()
        {
        }

        [Test]
        public void BasicCallOneArgument()
        {
            string content = @"foo(12.34);";
            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("foo", root.Identifier);
            Assert.AreEqual(4, root.StartPoint.X);
            Assert.AreEqual(0, root.StartPoint.Y);
            Assert.AreEqual(9, root.EndPoint.X);
            Assert.AreEqual(0, root.EndPoint.Y);

            FunctionCallPart part = root.GetIntersectionPart(6, 0);
            Assert.AreNotEqual(null, part);
            Assert.AreEqual(4, part.StartPoint.X);
            Assert.AreEqual(0, part.StartPoint.Y);
            Assert.AreEqual(9, part.EndPoint.X);
            Assert.AreEqual(0, part.EndPoint.Y);
        }

        [Test]
        public void BasicCallTwoArguments()
        {
            string content = @"foo(12.34, -56.78);";
            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("foo", root.Identifier);
            Assert.AreEqual(4, root.StartPoint.X);
            Assert.AreEqual(0, root.StartPoint.Y);
            Assert.AreEqual(17, root.EndPoint.X);
            Assert.AreEqual(0, root.EndPoint.Y);

            FunctionCallPart secondPart = root.GetIntersectionPart(15, 0);
            Assert.AreNotEqual(null, secondPart);
            Assert.AreNotEqual(null, secondPart.ParentPart);
            Assert.AreEqual("foo", secondPart.ParentPart.Identifier);
            Assert.AreEqual(10, secondPart.StartPoint.X);
            Assert.AreEqual(0, secondPart.StartPoint.Y);
            Assert.AreEqual(17, secondPart.EndPoint.X);
            Assert.AreEqual(0, secondPart.EndPoint.Y);
        }

        [Test]
        public void BasicCallTwoNestingLevels()
        {
            string content = @"foo(12.34, bar(-87.65, 43.21), -56.78);";
            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("foo", root.Identifier);
            Assert.AreEqual(4, root.StartPoint.X);
            Assert.AreEqual(0, root.StartPoint.Y);
            Assert.AreEqual(37, root.EndPoint.X);
            Assert.AreEqual(0, root.EndPoint.Y);

            // This is a position right after "43" (second arg of "bar").
            FunctionCallPart innerPart = root.GetIntersectionPart(25, 0);
            Assert.AreNotEqual(null, innerPart);
            Assert.AreNotEqual(null, innerPart.ParentPart);
            Assert.AreNotEqual(null, innerPart.ParentPart.ParentPart);
            Assert.AreEqual("bar", innerPart.ParentPart.Identifier);
            Assert.AreEqual("foo", innerPart.ParentPart.ParentPart.Identifier);

            Assert.AreEqual(22, innerPart.StartPoint.X);
            Assert.AreEqual(0, innerPart.StartPoint.Y);
            Assert.AreEqual(28, innerPart.EndPoint.X);
            Assert.AreEqual(0, innerPart.EndPoint.Y);

            innerPart = root.GetIntersectionPart(34, 0);
            Assert.AreNotEqual(null, innerPart);
            Assert.AreNotEqual(null, innerPart.ParentPart);
            Assert.AreEqual(null, innerPart.ParentPart.ParentPart);
            Assert.AreEqual("foo", innerPart.ParentPart.Identifier);

            // Parameter "-56.78" of function "foo".
            Assert.AreEqual(30, innerPart.StartPoint.X);
            Assert.AreEqual(0, innerPart.StartPoint.Y);
            Assert.AreEqual(37, innerPart.EndPoint.X);
            Assert.AreEqual(0, innerPart.EndPoint.Y);
        }

        [Test]
        public void MultilineIntersection()
        {
            string content = @"
foo(12.34,
    bar(-87.65, 43.21),
    bleh(abc, def, ghi),
    -56.78);";

            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("foo", root.Identifier);

            // Before the first line.
            FunctionCallPart intersection = root.GetIntersectionPart(8, 0);
            Assert.AreEqual(null, intersection);

            // Beyond the last line.
            intersection = root.GetIntersectionPart(8, 5);
            Assert.AreEqual(null, intersection);

            // On the first line, before open bracket.
            intersection = root.GetIntersectionPart(3, 1);
            Assert.AreEqual(null, intersection);

            // On the last line, after the close bracket.
            intersection = root.GetIntersectionPart(11, 4);
            Assert.AreEqual(null, intersection);

            // On the first line, after the open bracket.
            intersection = root.GetIntersectionPart(32, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.Identifier);

            // On the last line, before the close bracket.
            intersection = root.GetIntersectionPart(0, 4);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("-56.78", intersection.Identifier);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);

            // In a middle line, before "bar" function.
            intersection = root.GetIntersectionPart(0, 2);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.Identifier);

            // In a middle line, after the "bar" function.
            intersection = root.GetIntersectionPart(32, 2);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bleh", intersection.Identifier);

            // In a middle line, within the "bar" function.
            intersection = root.GetIntersectionPart(12, 2);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("-87.65", intersection.Identifier);
            Assert.AreEqual("bar", intersection.ParentPart.Identifier);
        }

        [Test]
        public void MultilineNestedIncomplete01()
        {
            string content = @"
    foo     (       bar     (           ";

            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("foo", root.Identifier);

            // Before the first open bracket.
            FunctionCallPart intersection = root.GetIntersectionPart(12, 1);
            Assert.AreEqual(null, intersection);

            // Right after the first open bracket.
            intersection = root.GetIntersectionPart(13, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.Identifier); // Argument 0 of "foo"
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right before the second open bracket.
            intersection = root.GetIntersectionPart(28, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.Identifier); // Argument 0 of "foo"
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the second open bracket.
            intersection = root.GetIntersectionPart(29, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("", intersection.Identifier); // Argument 0 of "bar"
            Assert.AreEqual("bar", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // At EOL after the second open bracket.
            intersection = root.GetIntersectionPart(40, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("", intersection.Identifier); // Argument 0 of "bar"
            Assert.AreEqual("bar", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));
        }

        [Test]
        public void MultilineNestedIncomplete02()
        {
            string content = @"
    foo     (       12.34,      bar     (   56.78,      ";

            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("foo", root.Identifier);

            // Before the first open bracket.
            FunctionCallPart intersection = root.GetIntersectionPart(12, 1);
            Assert.AreEqual(null, intersection);

            // Right after the first open bracket.
            intersection = root.GetIntersectionPart(13, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("12.34", intersection.Identifier); // Argument 0 of "foo"
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right before the first comma.
            intersection = root.GetIntersectionPart(25, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier); // Argument 0 of "foo"
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the first comma.
            intersection = root.GetIntersectionPart(26, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier); // Argument 1 of "foo"
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right before the second open bracket.
            intersection = root.GetIntersectionPart(40, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.Identifier); // Argument 1 of "foo"
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the second open bracket.
            intersection = root.GetIntersectionPart(41, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.ParentPart.Identifier); // Argument 0 of "bar"
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right before the second comma.
            intersection = root.GetIntersectionPart(49, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.ParentPart.Identifier); // Argument 0 of "bar"
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the second comma.
            intersection = root.GetIntersectionPart(50, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.ParentPart.Identifier); // Argument 1 of "bar"
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // At EOL after the second comma.
            intersection = root.GetIntersectionPart(56, 1);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("bar", intersection.ParentPart.Identifier); // Argument 1 of "bar"
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));
        }

        [Test]
        public void ArrayIndexedFunctionCall01()
        {
            string content = @"first[3][-2.3].second[4][5][6].foo(12, false);";

            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("first[3][-2.3].second[4][5][6].foo", root.Identifier);

            // Before the first open bracket.
            FunctionCallPart intersection = root.GetIntersectionPart(34, 0);
            Assert.AreEqual(null, intersection);

            // After the first open bracket.
            intersection = root.GetIntersectionPart(35, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual(root, intersection.ParentPart);
            Assert.AreEqual(0, root.GetArgumentIndex(intersection));

            // Right before the comma.
            intersection = root.GetIntersectionPart(37, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual(root, intersection.ParentPart);
            Assert.AreEqual(0, root.GetArgumentIndex(intersection));

            // Right after the comma.
            intersection = root.GetIntersectionPart(38, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual(root, intersection.ParentPart);
            Assert.AreEqual(1, root.GetArgumentIndex(intersection));

            // Right before the closing bracket.
            intersection = root.GetIntersectionPart(44, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual(root, intersection.ParentPart);
            Assert.AreEqual(1, root.GetArgumentIndex(intersection));

            // After the closing bracket.
            intersection = root.GetIntersectionPart(46, 0);
            Assert.AreEqual(null, intersection);
        }

        [Test]
        public void FunctionsWithinArrayExpression()
        {
            string content = @"foo( 789, { 12, First.ByLuck(34, 56), Second.ByChance( }, -321 );";

            Parser parser = CreateParserFromText(content);
            parser.Parse();

            FunctionCallPart root = parser.RootFunctionCallPart;
            Assert.AreEqual("foo", root.Identifier);

            // Right after "foo" before the open bracket.
            FunctionCallPart intersection = root.GetIntersectionPart(3, 0);
            Assert.AreEqual(null, intersection);

            // Right after the open bracket of "foo".
            intersection = root.GetIntersectionPart(4, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("789", intersection.Identifier);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the first comma.
            intersection = root.GetIntersectionPart(9, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after "ByLuck" before the open bracket.
            intersection = root.GetIntersectionPart(28, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the open bracket of "ByLuck" call.
            intersection = root.GetIntersectionPart(29, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("First.ByLuck", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right before the close bracket of "ByLuck" call.
            intersection = root.GetIntersectionPart(35, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("First.ByLuck", intersection.ParentPart.Identifier);
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the close bracket of "ByLuck" call.
            intersection = root.GetIntersectionPart(36, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right before the open bracket of "ByChance" call.
            intersection = root.GetIntersectionPart(53, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the open bracket of "ByChance" call.
            intersection = root.GetIntersectionPart(54, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("Second.ByChance", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right before the close curly bracket.
            intersection = root.GetIntersectionPart(55, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("Second.ByChance", intersection.ParentPart.Identifier);
            Assert.AreEqual(0, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the close curly bracket.
            intersection = root.GetIntersectionPart(56, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(1, intersection.ParentPart.GetArgumentIndex(intersection));

            // Right after the comma after close curly bracket.
            intersection = root.GetIntersectionPart(57, 0);
            Assert.AreNotEqual(null, intersection);
            Assert.AreEqual("-321", intersection.Identifier);
            Assert.AreNotEqual(null, intersection.ParentPart);
            Assert.AreEqual("foo", intersection.ParentPart.Identifier);
            Assert.AreEqual(2, intersection.ParentPart.GetArgumentIndex(intersection));
        }

        private Parser CreateParserFromText(string content)
        {
            MemoryStream inputStream = new MemoryStream(
                Encoding.Default.GetBytes(content));

            return (new Parser(new Scanner(inputStream)));
        }
    }
}

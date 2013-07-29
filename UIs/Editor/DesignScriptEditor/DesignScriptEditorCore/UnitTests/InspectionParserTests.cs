using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class InspectionParserTests
    {
        public InspectionParser parser;

        [SetUp]
        public void Setup()
        {
            //Nothing is coming
        }

        [Test]
        //Single dimensional arrays
        public void TestInspectionParser01()
        {
            string variable = "a[0]";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "a");
            Assert.AreEqual(parsedResult[1], "[0]");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Multidimensional arrays
        public void TestInspectionParser02()
        {
            string variable = "array[0][1][2]";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "array");
            Assert.AreEqual(parsedResult[1], "[0]");
            Assert.AreEqual(parsedResult[2], "[1]");
            Assert.AreEqual(parsedResult[3], "[2]");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Class property
        public void TestInspectionParser03()
        {
            string variable = "class.property";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "class");
            Assert.AreEqual(parsedResult[1], ".property");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Sub-property access in Class
        public void TestInspectionParser04()
        {
            string variable = "class.property.a.var";
            parser = new InspectionParser(variable); 
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "class");
            Assert.AreEqual(parsedResult[1], ".property");
            Assert.AreEqual(parsedResult[2], ".a");
            Assert.AreEqual(parsedResult[3], ".var");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Array element is a class
        public void TestInspectionParser05()
        {
            string variable = "array[3].property";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "array");
            Assert.AreEqual(parsedResult[1], "[3]");
            Assert.AreEqual(parsedResult[2], ".property");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Class property is an array
        public void TestInspectionParser06()
        {
            string variable = "class.property[3]";
            parser = new InspectionParser(variable); 
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "class");
            Assert.AreEqual(parsedResult[1], ".property");
            Assert.AreEqual(parsedResult[2], "[3]");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Multidimensional array has a property that is an array
        public void TestInspectionParser07()
        {
            string variable = "array[3][2].property[1]";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "array");
            Assert.AreEqual(parsedResult[1], "[3]");
            Assert.AreEqual(parsedResult[2], "[2]");
            Assert.AreEqual(parsedResult[3], ".property");
            Assert.AreEqual(parsedResult[4], "[1]");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Class-class-array-class-multidimensional array-class-array
        public void TestInspectionParser08()
        {
            string variable = "classA.propertyA[3].propertyB[1][0].propertyC[1]";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.AreEqual(parsedResult[0], "classA");
            Assert.AreEqual(parsedResult[1], ".propertyA");
            Assert.AreEqual(parsedResult[2], "[3]");
            Assert.AreEqual(parsedResult[3], ".propertyB");
            Assert.AreEqual(parsedResult[4], "[1]");
            Assert.AreEqual(parsedResult[5], "[0]");
            Assert.AreEqual(parsedResult[6], ".propertyC");
            Assert.AreEqual(parsedResult[7], "[1]");
            Assert.IsTrue(parser.IsValid == true);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Invalid variable sent for parsing
        public void TestInspectionParser09()
        {
            string variable = ".variable.check";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.IsTrue(parser.IsValid == false);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Invalid variable sent for parsing
        public void TestInspectionParser10()
        {
            string variable = "[2].check";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.IsTrue(parser.IsValid == false);
            Assert.IsTrue(parser.NeedsParsing() == true);
        }

        [Test]
        //Variable does not need parsing
        public void TestInspectionParser11()
        {
            string variable = "variable";
            parser = new InspectionParser(variable);
            List<string> parsedResult = parser.GetParsedCommands();

            Assert.IsTrue(parser.NeedsParsing() == false);
        }
    }
}

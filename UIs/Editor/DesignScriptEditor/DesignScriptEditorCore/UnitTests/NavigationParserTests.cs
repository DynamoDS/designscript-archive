using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    public class NavigationParserTests
    {
        public NavigationParser parser;

        [SetUp]

        public void setup()
        {
            parser = new NavigationParser();
        }

        [Test]
        public void TestNavigationParser01()
        {
            string variable = "int a = 3";
            List<string> parsedResults = parser.Tokenize(variable);

            Assert.AreEqual(parsedResults[0], "int");
            Assert.AreEqual(parsedResults[1], " ");
            Assert.AreEqual(parsedResults[2], "a");
            Assert.AreEqual(parsedResults[3], " ");
            Assert.AreEqual(parsedResults[4], "=");
            Assert.AreEqual(parsedResults[5], " ");
            Assert.AreEqual(parsedResults[6], "3");
        }

        [Test]
        public void TestNavigationParser02()
        {
            string variable = "    ";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "    ");
        }

        //This is a test for the tab button
        [Test]
        public void TestNavigationParser03()
        {
            string variable = "\t";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "\t");
        }

        [Test]
        public void TestNavigationParser04()
        {
            string variable = "\t  ";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "\t  ");
        }

        [Test]
        public void TestNavigationParser05()
        {
            string variable = "\n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "\n");
        }

        [Test]
        public void TestNavigationParser06()
        {
            string variable = "  \n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "  ");
            Assert.AreEqual(parsedResults[1], "\n");
        }

        [Test]
        public void TestNavigationParser07()
        {
            string variable = "123123 12312345 234234 34234 \n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "123123");
            Assert.AreEqual(parsedResults[1], " ");
            Assert.AreEqual(parsedResults[2], "12312345");
            Assert.AreEqual(parsedResults[3], " ");
            Assert.AreEqual(parsedResults[4], "234234");
            Assert.AreEqual(parsedResults[5], " ");
            Assert.AreEqual(parsedResults[6], "34234");
            Assert.AreEqual(parsedResults[7], " ");
            Assert.AreEqual(parsedResults[8], "\n");
        }

        [Test]
        public void TestNavigationParser08()
        {
            string variable = "123123\t12312345\t234234\t34234\t\n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "123123");
            Assert.AreEqual(parsedResults[1], "\t");
            Assert.AreEqual(parsedResults[2], "12312345");
            Assert.AreEqual(parsedResults[3], "\t");
            Assert.AreEqual(parsedResults[4], "234234");
            Assert.AreEqual(parsedResults[5], "\t");
            Assert.AreEqual(parsedResults[6], "34234");
            Assert.AreEqual(parsedResults[7], "\t");
            Assert.AreEqual(parsedResults[8], "\n");
        }


        [Test]
        public void TestNavigationParser09()
        {
            string variable = "$%\n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "$");
            Assert.AreEqual(parsedResults[1], "%");
            Assert.AreEqual(parsedResults[2], "\n");
        }

        [Test]
        public void TestNavigationParser10()
        {
            string variable = "$%aabb\n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "$");
            Assert.AreEqual(parsedResults[1], "%");
            Assert.AreEqual(parsedResults[2], "aabb");
            Assert.AreEqual(parsedResults[3], "\n");
        }

        [Test]
        public void TestNavigationParser11()
        {
            string variable = "THE CAT";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "THE");
            Assert.AreEqual(parsedResults[1], " ");
            Assert.AreEqual(parsedResults[2], "CAT");
        }


        [Test]
        public void TestNavigationParser12()
        {
            string variable = "\asasddd.asdas.asdas";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "\a");
            Assert.AreEqual(parsedResults[1], "sasddd");
            Assert.AreEqual(parsedResults[2], ".");
            Assert.AreEqual(parsedResults[3], "asdas");
            Assert.AreEqual(parsedResults[4], ".");
            Assert.AreEqual(parsedResults[5], "asdas");
        }

        [Test]
        public void TestNavigationParser13()
        {
            string variable = "123123.123123.1232345.5343\n\n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "123123.123123");
            Assert.AreEqual(parsedResults[1], ".");
            Assert.AreEqual(parsedResults[2], "1232345.5343");
            Assert.AreEqual(parsedResults[3], "\n");
            Assert.AreEqual(parsedResults[4], "\n");
        }

        [Test]
        //Reminder for 23423.234234.23423423
        public void TestNavigationParser14()
        {
            string variable = "123123,123123,1232345,5343\n\n";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "123123");
            Assert.AreEqual(parsedResults[1], ",");
            Assert.AreEqual(parsedResults[2], "123123");
            Assert.AreEqual(parsedResults[3], ",");
            Assert.AreEqual(parsedResults[4], "1232345");
            Assert.AreEqual(parsedResults[5], ",");
            Assert.AreEqual(parsedResults[6], "5343");
            Assert.AreEqual(parsedResults[7], "\n");
            Assert.AreEqual(parsedResults[8], "\n");
        }

        [Test]
        public void TestNavigationParser15()
        {
            string variable = "9833^^**$$12123";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "9833");
            Assert.AreEqual(parsedResults[1], "^");
            Assert.AreEqual(parsedResults[2], "^");
            Assert.AreEqual(parsedResults[3], "*");
            Assert.AreEqual(parsedResults[4], "*");
            Assert.AreEqual(parsedResults[5], "$");
            Assert.AreEqual(parsedResults[6], "$");
            Assert.AreEqual(parsedResults[7], "12123");
        }

        [Test]
        //Confusion
        public void TestNavigationParser16()
        {
            string variable = "@_a";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "@");
            Assert.AreEqual(parsedResults[1], "_a");
        }

        [Test]
        public void TestNavigationParser17()
        {
            string variable = "THE 22343 CLEVER 34235 FOX";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "THE");
            Assert.AreEqual(parsedResults[1], " ");
            Assert.AreEqual(parsedResults[2], "22343");
            Assert.AreEqual(parsedResults[3], " ");
            Assert.AreEqual(parsedResults[4], "CLEVER");
            Assert.AreEqual(parsedResults[5], " ");
            Assert.AreEqual(parsedResults[6], "34235");
            Assert.AreEqual(parsedResults[7], " ");
            Assert.AreEqual(parsedResults[8], "FOX");
        }

        [Test]
        //Reminder for #
        public void TestNavigationParser18()
        {
            string variable = "9999999999999999999999999999999999 GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG 999999999999999999999999 ######";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "9999999999999999999999999999999999");
            Assert.AreEqual(parsedResults[1], " ");
            Assert.AreEqual(parsedResults[2], "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG");
            Assert.AreEqual(parsedResults[3], " ");
            Assert.AreEqual(parsedResults[4], "999999999999999999999999");
            Assert.AreEqual(parsedResults[5], " ");
            Assert.AreEqual(parsedResults[6], "#");
        }

        [Test]
        public void TestNavigationParser19()
        {
            string variable = "<a>b";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "<");
            Assert.AreEqual(parsedResults[1], "a");
            Assert.AreEqual(parsedResults[2], ">");
            Assert.AreEqual(parsedResults[3], "b");
        }

        [Test]
        public void TestNavigationParser20()
        {//Accepts white line separately
            string variable = "123123 234234 23423423";
            List<string> parsedResults = parser.Tokenize(variable);
            Assert.AreEqual(parsedResults[0], "123123");
            Assert.AreEqual(parsedResults[1], " ");
            Assert.AreEqual(parsedResults[2], "234234");
            Assert.AreEqual(parsedResults[3], " ");
            Assert.AreEqual(parsedResults[4], "23423423");
            //Assert.AreEqual(parsedResults[3], "b");
        }
    }
}

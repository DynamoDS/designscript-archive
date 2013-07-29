using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    using DesignScript.Parser;
    using System.IO;

    class NodeProcessorImperativeTest
    {
        NodeProcessor nodeProcessor = null;
        List<Node> nodeList = null;
        CodeFragment[] fragmentArray = null;
        DesignScript.Parser.Parser parser;
        ProtoCore.Core core;

        [SetUp]
        public void Setup()
        {
            nodeList = new List<Node>();
            core = new ProtoCore.Core(new ProtoCore.Options());
        }

        public Stream ConvertToStream(string textContent)
        {
            char[] charArray = textContent.ToCharArray();
            List<char> textStream = new List<char>();
            MemoryStream stream;

            foreach(char character in charArray)
            {
                textStream.Add(character);
            }
           

            return stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(new string(textStream.ToArray())));
        }

        [Test]
        public void TestImperativeBinaryExpressionNode()
        {
            string content = "[Imperative]x = 0;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 7);

            Assert.AreEqual(fragmentArray[0].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[0].Text, "[");
            Assert.AreEqual(fragmentArray[0].Line, 0);
            Assert.AreEqual(fragmentArray[0].ColStart, 0);
            Assert.AreEqual(fragmentArray[0].ColEnd, 0);

            Assert.AreEqual(fragmentArray[1].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[1].Text, "Imperative");
            Assert.AreEqual(fragmentArray[1].Line, 0);
            Assert.AreEqual(fragmentArray[1].ColStart, 1);
            Assert.AreEqual(fragmentArray[1].ColEnd, 10);

            Assert.AreEqual(fragmentArray[2].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[2].Text, "]");
            Assert.AreEqual(fragmentArray[2].Line, 0);
            Assert.AreEqual(fragmentArray[2].ColStart, 11);
            Assert.AreEqual(fragmentArray[2].ColEnd, 11);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, ";");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 17);
            Assert.AreEqual(fragmentArray[3].ColEnd, 17);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[4].Text, "=");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "x");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 12);
            Assert.AreEqual(fragmentArray[5].ColEnd, 12);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[6].Text, "0");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 16);
            Assert.AreEqual(fragmentArray[6].ColEnd, 16);
        }

        [Test]
        public void TestImperativeIntNode()
        {
            string content = "[Imperative] x = -2;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 8);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[6].Text, "-");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 17);
            Assert.AreEqual(fragmentArray[6].ColEnd, 17);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[7].Text, "2");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 18);
            Assert.AreEqual(fragmentArray[7].ColEnd, 18);

        }

        [Test]
        public void TestImperativeDoubleNode()
        {
            string content = "[Imperative] x = 2.1;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 7);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[6].Text, "2.1");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 17);
            Assert.AreEqual(fragmentArray[6].ColEnd, 19);
        }

        [Test]
        public void TestImperativeBooleanNode()
        {
            string content = "[Imperative] x = true;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 7);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[6].Text, "true");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 17);
            Assert.AreEqual(fragmentArray[6].ColEnd, 20);

        }

        [Test]
        public void TestImperativeCharacterNode()
        {
            string content = "[Imperative] x = a;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 7);
            //Should be text not local
            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[6].Text, "a");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 17);
            Assert.AreEqual(fragmentArray[6].ColEnd, 17);

        }

        [Test]
        public void TestImperativeStringNode()
        {
            string content = "[Imperative] x = test;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 7);
            //Why local again should be text!
            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[6].Text, "test");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 17);
            Assert.AreEqual(fragmentArray[6].ColEnd, 20);

        }

        [Test]
        public void TestImperativeArrayNode()
        {
            //list[1]
            string content = "[Imperative] list[1] = 0;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length , 10);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, "[");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 17);
            Assert.AreEqual(fragmentArray[5].ColEnd, 17);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "]");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 19);
            Assert.AreEqual(fragmentArray[6].ColEnd, 19);
            
            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[7].Text, "list");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 13);
            Assert.AreEqual(fragmentArray[7].ColEnd, 16);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[8].Text, "1");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 18);
            Assert.AreEqual(fragmentArray[8].ColEnd, 18);

        }

        [Test]
        public void TestImperativeTextNode()
        {
            //[Imperative]
            //{
            //  a = 'x';
            //  b = "This is an Imperative Block";
            //}
            string content = "[Imperative]{ a = 'x'; b = \"This is an Imperative Block\"; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 13);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Text);
            Assert.AreEqual(fragmentArray[8].Text, "'x'");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 18);
            Assert.AreEqual(fragmentArray[8].ColEnd, 20);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Text);
            Assert.AreEqual(fragmentArray[12].Text, "\"This is an Imperative Block\"");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 27);
            Assert.AreEqual(fragmentArray[12].ColEnd, 55);
        }

        [Test]
        public void TestImperativeExpressionListNode()
        {
            //somelist[] = {11,102,1003,1004};
            string content = "[Imperative] somelist[] = {11,102,1003,1004};";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 17);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, "{");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 26);
            Assert.AreEqual(fragmentArray[8].ColEnd, 26);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[9].Text, "11");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 27);
            Assert.AreEqual(fragmentArray[9].ColEnd, 28);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[10].Text, "102");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 30);
            Assert.AreEqual(fragmentArray[10].ColEnd, 32);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[11].Text, "1003");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 34);
            Assert.AreEqual(fragmentArray[11].ColEnd, 37);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[12].Text, "1004");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 39);
            Assert.AreEqual(fragmentArray[12].ColEnd, 42);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, ",");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 29);
            Assert.AreEqual(fragmentArray[13].ColEnd, 29);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, ",");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 33);
            Assert.AreEqual(fragmentArray[14].ColEnd, 33);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[15].Text, ",");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 38);
            Assert.AreEqual(fragmentArray[15].ColEnd, 38);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[16].Text, "}");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 43);
            Assert.AreEqual(fragmentArray[16].ColEnd, 43);
        }

        [Test]
        public void TestImperativeRangeExpNode()
        {
            //a = 1..100..#1;
            string content = "[Imperative] a = 1..100..#1;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 12);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[6].Text, "1");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 17);
            Assert.AreEqual(fragmentArray[6].ColEnd, 17);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[7].Text, "100");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 20);
            Assert.AreEqual(fragmentArray[7].ColEnd, 22);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[8].Text, "1");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 26);
            Assert.AreEqual(fragmentArray[8].ColEnd, 26);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, "..");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 18);
            Assert.AreEqual(fragmentArray[9].ColEnd, 19);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "..");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 23);
            Assert.AreEqual(fragmentArray[10].ColEnd, 24);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, "#");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 25);
            Assert.AreEqual(fragmentArray[11].ColEnd, 25);
        }

        [Test]
        public void TestImperativeIdentifierNode()
        {
            //a;
            string content = "[Imperative] a";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 4);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[3].Text, "a");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 13);
            Assert.AreEqual(fragmentArray[3].ColEnd, 13);

        }

        [Test]
        public void TestImperativeFunctionCallNode()
        {
            string content = "[Imperative] aVar = func(a, b, c);" ;
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 14);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[6].Text, "func");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 20);
            Assert.AreEqual(fragmentArray[6].ColEnd, 23);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "(");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 24);
            Assert.AreEqual(fragmentArray[7].ColEnd, 24);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[8].Text, "a");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 25);
            Assert.AreEqual(fragmentArray[8].ColEnd, 25);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[9].Text, "b");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 28);
            Assert.AreEqual(fragmentArray[9].ColEnd, 28);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[10].Text, "c");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 31);
            Assert.AreEqual(fragmentArray[10].ColEnd, 31);
        }

        [Test]
        public void TestImperativeFunctionDefinitionNode()
        {
            //def func : int (a : int, b : double, c : int){result = a + b * c;}
            string content = "[Imperative] def func : int (a : int, b : double, c : int){result = a + b * c;}";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 30);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, "{");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 58);
            Assert.AreEqual(fragmentArray[3].ColEnd, 58);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[4].Text, "def");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 13);// Expected 15 check!! It is 13
            Assert.AreEqual(fragmentArray[4].ColEnd, 15);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[5].Text, "func");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 17);
            Assert.AreEqual(fragmentArray[5].ColEnd, 20);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "(");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 28);
            Assert.AreEqual(fragmentArray[6].ColEnd, 28);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, ":");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 22);
            Assert.AreEqual(fragmentArray[7].ColEnd, 22);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[8].Text, "int");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 24);
            Assert.AreEqual(fragmentArray[8].ColEnd, 26);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, "}");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 78);
            Assert.AreEqual(fragmentArray[9].ColEnd, 78);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, ")");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 57);
            Assert.AreEqual(fragmentArray[10].ColEnd, 57);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, ",");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 36);
            Assert.AreEqual(fragmentArray[11].ColEnd, 36);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, ",");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 48);
            Assert.AreEqual(fragmentArray[12].ColEnd, 48);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, ";");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 77);
            Assert.AreEqual(fragmentArray[13].ColEnd, 77);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, "=");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 66);
            Assert.AreEqual(fragmentArray[14].ColEnd, 66);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[15].Text, "result");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 59);//Another one should be 64!!
            Assert.AreEqual(fragmentArray[15].ColEnd, 64);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[16].Text, "+");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 70);
            Assert.AreEqual(fragmentArray[16].ColEnd, 70);

            Assert.AreEqual(fragmentArray[17].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[17].Text, "a");
            Assert.AreEqual(fragmentArray[17].Line, 0);
            Assert.AreEqual(fragmentArray[17].ColStart, 68);
            Assert.AreEqual(fragmentArray[17].ColEnd, 68);

            Assert.AreEqual(fragmentArray[18].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[18].Text, "*");
            Assert.AreEqual(fragmentArray[18].Line, 0);
            Assert.AreEqual(fragmentArray[18].ColStart, 74);
            Assert.AreEqual(fragmentArray[18].ColEnd, 74);

            Assert.AreEqual(fragmentArray[19].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[19].Text, "b");
            Assert.AreEqual(fragmentArray[19].Line, 0);
            Assert.AreEqual(fragmentArray[19].ColStart, 72);
            Assert.AreEqual(fragmentArray[19].ColEnd, 72);

            Assert.AreEqual(fragmentArray[20].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[20].Text, "c");
            Assert.AreEqual(fragmentArray[20].Line, 0);
            Assert.AreEqual(fragmentArray[20].ColStart, 76);
            Assert.AreEqual(fragmentArray[20].ColEnd, 76);

            Assert.AreEqual(fragmentArray[23].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[23].Text, "int");
            Assert.AreEqual(fragmentArray[23].Line, 0);
            Assert.AreEqual(fragmentArray[23].ColStart, 33);
            Assert.AreEqual(fragmentArray[23].ColEnd, 35);

            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[22].Text, ":");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 31);
            Assert.AreEqual(fragmentArray[22].ColEnd, 31);

            Assert.AreEqual(fragmentArray[21].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[21].Text, "a");
            Assert.AreEqual(fragmentArray[21].Line, 0);
            Assert.AreEqual(fragmentArray[21].ColStart, 29);
            Assert.AreEqual(fragmentArray[21].ColEnd, 29);

            Assert.AreEqual(fragmentArray[26].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[26].Text, "double");
            Assert.AreEqual(fragmentArray[26].Line, 0);
            Assert.AreEqual(fragmentArray[26].ColStart, 42);
            Assert.AreEqual(fragmentArray[26].ColEnd, 47);

            Assert.AreEqual(fragmentArray[25].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[25].Text, ":");
            Assert.AreEqual(fragmentArray[25].Line, 0);
            Assert.AreEqual(fragmentArray[25].ColStart, 40);
            Assert.AreEqual(fragmentArray[25].ColEnd, 40);

            Assert.AreEqual(fragmentArray[24].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[24].Text, "b");
            Assert.AreEqual(fragmentArray[24].Line, 0);
            Assert.AreEqual(fragmentArray[24].ColStart, 38);
            Assert.AreEqual(fragmentArray[24].ColEnd, 38);

            Assert.AreEqual(fragmentArray[29].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[29].Text, "int");
            Assert.AreEqual(fragmentArray[29].Line, 0);
            Assert.AreEqual(fragmentArray[29].ColStart, 54);
            Assert.AreEqual(fragmentArray[29].ColEnd, 56);

            Assert.AreEqual(fragmentArray[28].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[28].Text, ":");
            Assert.AreEqual(fragmentArray[28].Line, 0);
            Assert.AreEqual(fragmentArray[28].ColStart, 52);
            Assert.AreEqual(fragmentArray[28].ColEnd, 52);

            Assert.AreEqual(fragmentArray[27].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[27].Text, "c");
            Assert.AreEqual(fragmentArray[27].Line, 0);
            Assert.AreEqual(fragmentArray[27].ColStart, 50);
            Assert.AreEqual(fragmentArray[27].ColEnd, 50);
           
        }

        [Test]
        public void TestImperativeIfElseNode()
        {
            string content = "[Imperative] if ( a == b){a = 0;} else{a = a + b;}";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 24);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, "==");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 20);
            Assert.AreEqual(fragmentArray[3].ColEnd, 21);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "a");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 18);
            Assert.AreEqual(fragmentArray[4].ColEnd, 18);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "b");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 23);
            Assert.AreEqual(fragmentArray[5].ColEnd, 23);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[6].Text, "if");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 13);
            Assert.AreEqual(fragmentArray[6].ColEnd, 14);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "{");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 25);
            Assert.AreEqual(fragmentArray[7].ColEnd, 25);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, "(");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 16);
            Assert.AreEqual(fragmentArray[8].ColEnd, 16);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, ")");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 24);
            Assert.AreEqual(fragmentArray[9].ColEnd, 24);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "}");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 32);
            Assert.AreEqual(fragmentArray[10].ColEnd, 32);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, "{");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 38);
            Assert.AreEqual(fragmentArray[11].ColEnd, 38);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[12].Text, "else");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 34);//Expected 37!!
            Assert.AreEqual(fragmentArray[12].ColEnd, 37);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, "}");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 49);
            Assert.AreEqual(fragmentArray[13].ColEnd, 49);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, ";");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 31);
            Assert.AreEqual(fragmentArray[14].ColEnd, 31);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[15].Text, "=");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 28);
            Assert.AreEqual(fragmentArray[15].ColEnd, 28);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[16].Text, "a");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 26);
            Assert.AreEqual(fragmentArray[16].ColEnd, 26);

            Assert.AreEqual(fragmentArray[17].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[17].Text, "0");
            Assert.AreEqual(fragmentArray[17].Line, 0);
            Assert.AreEqual(fragmentArray[17].ColStart, 30);
            Assert.AreEqual(fragmentArray[17].ColEnd, 30); 

            Assert.AreEqual(fragmentArray[18].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[18].Text, ";");
            Assert.AreEqual(fragmentArray[18].Line, 0);
            Assert.AreEqual(fragmentArray[18].ColStart, 48);
            Assert.AreEqual(fragmentArray[18].ColEnd, 48);

            Assert.AreEqual(fragmentArray[19].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[19].Text, "=");
            Assert.AreEqual(fragmentArray[19].Line, 0);
            Assert.AreEqual(fragmentArray[19].ColStart, 41);
            Assert.AreEqual(fragmentArray[19].ColEnd, 41);

            Assert.AreEqual(fragmentArray[20].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[20].Text, "a");
            Assert.AreEqual(fragmentArray[20].Line, 0);
            Assert.AreEqual(fragmentArray[20].ColStart, 39);
            Assert.AreEqual(fragmentArray[20].ColEnd, 39);

            Assert.AreEqual(fragmentArray[21].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[21].Text, "+");
            Assert.AreEqual(fragmentArray[21].Line, 0);
            Assert.AreEqual(fragmentArray[21].ColStart, 45);
            Assert.AreEqual(fragmentArray[21].ColEnd, 45);

            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[22].Text, "a");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 43);
            Assert.AreEqual(fragmentArray[22].ColEnd, 43);

            Assert.AreEqual(fragmentArray[23].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[23].Text, "b");
            Assert.AreEqual(fragmentArray[23].Line, 0);
            Assert.AreEqual(fragmentArray[23].ColStart, 47);
            Assert.AreEqual(fragmentArray[23].ColEnd, 47);
        }

        [Test]
        public void TestImperativeWhileStmtNode()
        {
            string content = "[Imperative] while( y < ySize ){y = y + 1;}";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 17);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, "<");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 22);
            Assert.AreEqual(fragmentArray[3].ColEnd, 22);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "y");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 20);
            Assert.AreEqual(fragmentArray[4].ColEnd, 20);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "ySize");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 24);//Again expected 28!!
            Assert.AreEqual(fragmentArray[5].ColEnd, 28);


            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[6].Text, "while");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 13);//Expected 17 
            Assert.AreEqual(fragmentArray[6].ColEnd, 17);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "{");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 31);
            Assert.AreEqual(fragmentArray[7].ColEnd, 31);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, "(");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 18);
            Assert.AreEqual(fragmentArray[8].ColEnd, 18);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, ")");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 30);
            Assert.AreEqual(fragmentArray[9].ColEnd, 30);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "}");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 42);
            Assert.AreEqual(fragmentArray[10].ColEnd, 42);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, ";");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 41);
            Assert.AreEqual(fragmentArray[11].ColEnd, 41);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, "=");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 34);
            Assert.AreEqual(fragmentArray[12].ColEnd, 34);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[13].Text, "y");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 32);
            Assert.AreEqual(fragmentArray[13].ColEnd, 32);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, "+");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 38);
            Assert.AreEqual(fragmentArray[14].ColEnd, 38);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[15].Text, "y");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 36);
            Assert.AreEqual(fragmentArray[15].ColEnd, 36);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[16].Text, "1");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 40);
            Assert.AreEqual(fragmentArray[16].ColEnd, 40);
        }

        [Test]
        public void TestImperativeUnaryExpressionNode()
        {
            string content = "[Imperative]{ a = true; b = !a; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 14);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "=");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 26);
            Assert.AreEqual(fragmentArray[10].ColEnd, 26);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[11].Text, "b");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 24);
            Assert.AreEqual(fragmentArray[11].ColEnd, 24);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, "!");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 28);
            Assert.AreEqual(fragmentArray[12].ColEnd, 28);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[13].Text, "a");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 29);
            Assert.AreEqual(fragmentArray[13].ColEnd, 29);
        }

        [Test]
        public void TestImperativeForLoopNode()
        {
            string content = "[Imperative] for (val in a){x = x + val;}";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 17);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[3].Text, "val");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 18);
            Assert.AreEqual(fragmentArray[3].ColEnd, 20);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "a");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 25);
            Assert.AreEqual(fragmentArray[4].ColEnd, 25);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[5].Text, "for");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 13);
            Assert.AreEqual(fragmentArray[5].ColEnd, 15);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[6].Text, "in");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 22);
            Assert.AreEqual(fragmentArray[6].ColEnd, 23);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "{");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 27);
            Assert.AreEqual(fragmentArray[7].ColEnd, 27);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, "(");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 17);
            Assert.AreEqual(fragmentArray[8].ColEnd, 17);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, ")");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 26);
            Assert.AreEqual(fragmentArray[9].ColEnd, 26);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "}");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 40);
            Assert.AreEqual(fragmentArray[10].ColEnd, 40);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, ";");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 39);
            Assert.AreEqual(fragmentArray[11].ColEnd, 39);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, "=");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 30);
            Assert.AreEqual(fragmentArray[12].ColEnd, 30);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[13].Text, "x");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 28);
            Assert.AreEqual(fragmentArray[13].ColEnd, 28);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, "+");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 34);
            Assert.AreEqual(fragmentArray[14].ColEnd, 34);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[15].Text, "x");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 32);
            Assert.AreEqual(fragmentArray[15].ColEnd, 32);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[16].Text, "val");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 36);//Expected 38
            Assert.AreEqual(fragmentArray[16].ColEnd, 38);
        }
        
        [Test]
        public void TestImperativeVariableDeclaration()
        {
            string content = "[Imperative]{ a : pointer = 2 ; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 11);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, "=");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 26);
            Assert.AreEqual(fragmentArray[9].ColEnd, 26);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.UserType);
            Assert.AreEqual(fragmentArray[8].Text, "pointer");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 18);
            Assert.AreEqual(fragmentArray[8].ColEnd, 24);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, ":");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 16);
            Assert.AreEqual(fragmentArray[7].ColEnd, 16);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[6].Text, "a");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 14);
            Assert.AreEqual(fragmentArray[6].ColEnd, 14);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[5].Text, "2");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 28);
            Assert.AreEqual(fragmentArray[5].ColEnd, 28);
        }

        [Test]
        public void TestImperativeNegativeUnaryExpr()
        {
            //[Imperative]{
            //      a = 10; 
            //      b = -a;
            //}
            string content = "[Imperative]{ a = 10; b = -a; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 14);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "=");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 24);
            Assert.AreEqual(fragmentArray[10].ColEnd, 24);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[11].Text, "b");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 22);
            Assert.AreEqual(fragmentArray[11].ColEnd, 22);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, "-");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 26);
            Assert.AreEqual(fragmentArray[12].ColEnd, 26);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[13].Text, "a");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 27);
            Assert.AreEqual(fragmentArray[13].ColEnd, 27);
        }

        [Test]
        public void TestImperativeInlineConditionExpr()
        {
            //[Imperative]
            //{
            //  a = 3.141593;
            //  b = 100 ;
            //	i = 2 ;
            //	j : double = (i == 2) ? a : b;
            //}
            string content = "[Imperative]{ a = 3.141593 ; b = 100 ; i = 2 ; j : int = (i == 2) ? a : b; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 31);

            Assert.AreEqual(fragmentArray[29].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[29].Text, "=");
            Assert.AreEqual(fragmentArray[29].Line, 0);
            Assert.AreEqual(fragmentArray[29].ColStart, 55);
            Assert.AreEqual(fragmentArray[29].ColEnd, 55);

            Assert.AreEqual(fragmentArray[28].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[28].Text, "int");
            Assert.AreEqual(fragmentArray[28].Line, 0);
            Assert.AreEqual(fragmentArray[28].ColStart, 51);
            Assert.AreEqual(fragmentArray[28].ColEnd, 53);

            Assert.AreEqual(fragmentArray[27].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[27].Text, ":");
            Assert.AreEqual(fragmentArray[27].Line, 0);
            Assert.AreEqual(fragmentArray[27].ColStart, 49);
            Assert.AreEqual(fragmentArray[27].ColEnd, 49);

            Assert.AreEqual(fragmentArray[26].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[26].Text, "j");
            Assert.AreEqual(fragmentArray[26].Line, 0);
            Assert.AreEqual(fragmentArray[26].ColStart, 47);
            Assert.AreEqual(fragmentArray[26].ColEnd, 47);

            Assert.AreEqual(fragmentArray[17].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[17].Text, "(");
            Assert.AreEqual(fragmentArray[17].Line, 0);
            Assert.AreEqual(fragmentArray[17].ColStart, 57);
            Assert.AreEqual(fragmentArray[17].ColEnd, 57);

            Assert.AreEqual(fragmentArray[18].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[18].Text, ")");
            Assert.AreEqual(fragmentArray[18].Line, 0);
            Assert.AreEqual(fragmentArray[18].ColStart, 64);
            Assert.AreEqual(fragmentArray[18].ColEnd, 64);

            Assert.AreEqual(fragmentArray[19].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[19].Text, "==");
            Assert.AreEqual(fragmentArray[19].Line, 0);
            Assert.AreEqual(fragmentArray[19].ColStart, 60);
            Assert.AreEqual(fragmentArray[19].ColEnd, 61);

            Assert.AreEqual(fragmentArray[20].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[20].Text, "i");
            Assert.AreEqual(fragmentArray[20].Line, 0);
            Assert.AreEqual(fragmentArray[20].ColStart, 58);
            Assert.AreEqual(fragmentArray[20].ColEnd, 58);

            Assert.AreEqual(fragmentArray[21].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[21].Text, "2");
            Assert.AreEqual(fragmentArray[21].Line, 0);
            Assert.AreEqual(fragmentArray[21].ColStart, 63);
            Assert.AreEqual(fragmentArray[21].ColEnd, 63);

            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[22].Text, "a");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 68);
            Assert.AreEqual(fragmentArray[22].ColEnd, 68);

            Assert.AreEqual(fragmentArray[23].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[23].Text, "b");
            Assert.AreEqual(fragmentArray[23].Line, 0);
            Assert.AreEqual(fragmentArray[23].ColStart, 72);
            Assert.AreEqual(fragmentArray[23].ColEnd, 72);

            Assert.AreEqual(fragmentArray[24].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[24].Text, "?");
            Assert.AreEqual(fragmentArray[24].Line, 0);
            Assert.AreEqual(fragmentArray[24].ColStart, 66);
            Assert.AreEqual(fragmentArray[24].ColEnd, 66);

            Assert.AreEqual(fragmentArray[25].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[25].Text, ":");
            Assert.AreEqual(fragmentArray[25].Line, 0);
            Assert.AreEqual(fragmentArray[25].ColStart, 70);
            Assert.AreEqual(fragmentArray[25].ColEnd, 70);
        }

        [Test]
        public void TestImperativeArrayExprList()
        {
            //[Imperative]
            //{
            //  Hello = { pt1, pt2 }.Translate();
            //}
            string content = "[Imperative]{ Hello = { pt1, pt2 }.Translate(); }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 17);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "=");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 20);
            Assert.AreEqual(fragmentArray[6].ColEnd, 20);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[7].Text, "Hello");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 14);
            Assert.AreEqual(fragmentArray[7].ColEnd, 18);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, ".");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 34);
            Assert.AreEqual(fragmentArray[8].ColEnd, 34);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, "{");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 22);
            Assert.AreEqual(fragmentArray[9].ColEnd, 22);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[10].Text, "pt1");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 24);
            Assert.AreEqual(fragmentArray[10].ColEnd, 26);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[11].Text, "pt2");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 29);
            Assert.AreEqual(fragmentArray[11].ColEnd, 31);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, ",");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 27);
            Assert.AreEqual(fragmentArray[12].ColEnd, 27);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, "}");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 33);
            Assert.AreEqual(fragmentArray[13].ColEnd, 33);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[14].Text, "Translate");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 35);
            Assert.AreEqual(fragmentArray[14].ColEnd, 43);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[15].Text, "(");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 44);
            Assert.AreEqual(fragmentArray[15].ColEnd, 44);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[16].Text, ")");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 45);
            Assert.AreEqual(fragmentArray[16].ColEnd, 45);
        }
    }
}

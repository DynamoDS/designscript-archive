using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    using DesignScript.Parser;
    using System.IO;

    class NodeProcessorAssociativeTest
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

            foreach (char character in charArray)
            {
                textStream.Add(character);
            }


            return stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(new string(textStream.ToArray())));
        }

        [Test]
        public void TestAssociativeBinaryExpressionNode()
        {
            string content = "[Associative]x = 0;";
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
            Assert.AreEqual(fragmentArray[1].Text, "Associative");
            Assert.AreEqual(fragmentArray[1].Line, 0);
            Assert.AreEqual(fragmentArray[1].ColStart, 1);
            Assert.AreEqual(fragmentArray[1].ColEnd, 11);

            Assert.AreEqual(fragmentArray[2].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[2].Text, "]");
            Assert.AreEqual(fragmentArray[2].Line, 0);
            Assert.AreEqual(fragmentArray[2].ColStart, 12);
            Assert.AreEqual(fragmentArray[2].ColEnd, 12);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, ";");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 18);
            Assert.AreEqual(fragmentArray[3].ColEnd, 18);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "x");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 13);
            Assert.AreEqual(fragmentArray[4].ColEnd, 13);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[5].Text, "0");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 17);
            Assert.AreEqual(fragmentArray[5].ColEnd, 17);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "=");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 15);
            Assert.AreEqual(fragmentArray[6].ColEnd, 15);

            
        }

        [Test]
        public void TestAssociativeIntNode()
        {
            string content = "[Associative] x = -2;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 8);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "x");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[6].Text, "2");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 19);
            Assert.AreEqual(fragmentArray[6].ColEnd, 19);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[5].Text, "-");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 18);
            Assert.AreEqual(fragmentArray[5].ColEnd, 18);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "=");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 16);
            Assert.AreEqual(fragmentArray[7].ColEnd, 16);
        }

        [Test]
        public void TestAssociativeDoubleNode()
        {
            string content = "[Associative] x = 2.1;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 7);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "x");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[5].Text, "2.1");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 18);
            Assert.AreEqual(fragmentArray[5].ColEnd, 20);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "=");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 16);
            Assert.AreEqual(fragmentArray[6].ColEnd, 16);
        }

        [Test]
        public void TestAssociativeBooleanNode()
        {
            string content = "[Associative] x = true;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 7);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);                       
            Assert.AreEqual(fragmentArray[4].Text, "x");                                              
            Assert.AreEqual(fragmentArray[4].Line, 0);                                                   
            Assert.AreEqual(fragmentArray[4].ColStart, 14);                                              
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);                                                
                                                                                                         
            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Keyword);                       
            Assert.AreEqual(fragmentArray[5].Text, "true");                                              
            Assert.AreEqual(fragmentArray[5].Line, 0);                                                   
            Assert.AreEqual(fragmentArray[5].ColStart, 18);                                              
            Assert.AreEqual(fragmentArray[5].ColEnd, 21);                                                
                                                                                                         
            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);                       
            Assert.AreEqual(fragmentArray[6].Text, "=");                                              
            Assert.AreEqual(fragmentArray[6].Line, 0);                                                   
            Assert.AreEqual(fragmentArray[6].ColStart, 16);
            Assert.AreEqual(fragmentArray[6].ColEnd, 16);

        }

        [Test]
        public void TestAssociativeCharacterNode()
        {
            string content = "[Associative] x = a;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.IsTrue(fragmentArray.Length == 7);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "x");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);
            //Should be text not local
            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "a");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 18);
            Assert.AreEqual(fragmentArray[5].ColEnd, 18);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "=");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 16);
            Assert.AreEqual(fragmentArray[6].ColEnd, 16);

        }

        [Test]
        public void TestAssociativeStringNode()
        {
            string content = "[Associative] x = test;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 7);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "x");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "test");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 18);
            Assert.AreEqual(fragmentArray[5].ColEnd, 21);
            
            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "=");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 16);
            Assert.AreEqual(fragmentArray[6].ColEnd, 16);

        }

        [Test]
        public void TestAssociativeArrayNode()
        {
            //list[1]
            string content = "[Associative] list[1] = 0;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 10);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[4].Text, "1");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 19);
            Assert.AreEqual(fragmentArray[4].ColEnd, 19);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, "[");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 18);
            Assert.AreEqual(fragmentArray[5].ColEnd, 18);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "]");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 20);
            Assert.AreEqual(fragmentArray[6].ColEnd, 20);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[7].Text, "list");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 14);
            Assert.AreEqual(fragmentArray[7].ColEnd, 17);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[8].Text, "0");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 24);
            Assert.AreEqual(fragmentArray[8].ColEnd, 24);

            
        }

        [Test]
        public void TestAssociativeTextNode()
        {
            //[Associative]
            //{
            //    c = 'h';
            //    d = "This is an Assoc Block";
            //}
            string content = "[Associative]{ c = 'h'; d = \"This is an Assoc Block\"; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 13);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Text);
            Assert.AreEqual(fragmentArray[7].Text, "'h'");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 19);
            Assert.AreEqual(fragmentArray[7].ColEnd, 21);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Text);
            Assert.AreEqual(fragmentArray[11].Text, "\"This is an Assoc Block\"");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 28);
            Assert.AreEqual(fragmentArray[11].ColEnd, 51);
        }

        [Test]
        public void TestAssociativeExpressionListNode()
        {
            //somelist[] = {11,102,1003,1004};
            string content = "[Associative] somelist[] = {11,102,1003,1004};";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 17);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, ",");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 30);
            Assert.AreEqual(fragmentArray[7].ColEnd, 30);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, ",");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 34);
            Assert.AreEqual(fragmentArray[8].ColEnd, 34);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, ",");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 39);
            Assert.AreEqual(fragmentArray[9].ColEnd, 39);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[10].Text, "11");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 28);
            Assert.AreEqual(fragmentArray[10].ColEnd, 29);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[11].Text, "102");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 31);
            Assert.AreEqual(fragmentArray[11].ColEnd, 33);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[12].Text, "1003");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 35);
            Assert.AreEqual(fragmentArray[12].ColEnd, 38);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[13].Text, "1004");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 40);
            Assert.AreEqual(fragmentArray[13].ColEnd, 43);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, "{");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 27);
            Assert.AreEqual(fragmentArray[14].ColEnd, 27);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[15].Text, "}");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 44);
            Assert.AreEqual(fragmentArray[15].ColEnd, 44);
        }

        [Test]
        public void TestAssociativeRangeExpNode()
        {
            //a = 1..100..#1;
            string content = "[Associative] a = 1..100..#1;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 12);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, "..");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 19);
            Assert.AreEqual(fragmentArray[5].ColEnd, 20);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "..");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 24);
            Assert.AreEqual(fragmentArray[6].ColEnd, 25);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "#");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 26);
            Assert.AreEqual(fragmentArray[7].ColEnd, 26);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[8].Text, "100");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 21);
            Assert.AreEqual(fragmentArray[8].ColEnd, 23);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[9].Text, "1");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 27);
            Assert.AreEqual(fragmentArray[9].ColEnd, 27);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[10].Text, "1");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 18);
            Assert.AreEqual(fragmentArray[10].ColEnd, 18);

            
        }

        [Test]
        public void TestAssociativeIdentifierNode()
        {
            //a;
            string content = "[Associative] a";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 4);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[3].Text, "a");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 14);
            Assert.AreEqual(fragmentArray[3].ColEnd, 14);

        }

        [Test]
        public void TestAssociativeFunctionCallNode()
        {
            string content = "[Associative] aVar = func(a, b, c);";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 14);
            
            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[5].Text, "func");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 21);
            Assert.AreEqual(fragmentArray[5].ColEnd, 24);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[6].Text, "a");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 26);
            Assert.AreEqual(fragmentArray[6].ColEnd, 26);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[7].Text, "b");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 29);
            Assert.AreEqual(fragmentArray[7].ColEnd, 29);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[8].Text, "c");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 32);
            Assert.AreEqual(fragmentArray[8].ColEnd, 32);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, ",");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 27);
            Assert.AreEqual(fragmentArray[9].ColEnd, 27);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, ",");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 30);
            Assert.AreEqual(fragmentArray[10].ColEnd, 30);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, "(");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 25);
            Assert.AreEqual(fragmentArray[11].ColEnd, 25);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, ")");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 33);
            Assert.AreEqual(fragmentArray[12].ColEnd, 33);
        }

        [Test]
        public void TestAssociativeFunctionDefinitionNode()
        {
            //def func : int (a : int, b : double, c : int){result = a + b * c;}
            string content = "[Associative] def func : int (a : int, b : double, c : int){result = a + b * c;}";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 30);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[3].Text, "def");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 14);// Expected 15 check!! It is 13
            Assert.AreEqual(fragmentArray[3].ColEnd, 16);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[4].Text, "func");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 18);
            Assert.AreEqual(fragmentArray[4].ColEnd, 21);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, "}");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 79);
            Assert.AreEqual(fragmentArray[5].ColEnd, 79);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "{");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 59);
            Assert.AreEqual(fragmentArray[6].ColEnd, 59);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, ";");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 78);
            Assert.AreEqual(fragmentArray[7].ColEnd, 78);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[8].Text, "result");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 60);//Another one should be 64!!
            Assert.AreEqual(fragmentArray[8].ColEnd, 65);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[9].Text, "a");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 69);
            Assert.AreEqual(fragmentArray[9].ColEnd, 69);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[10].Text, "b");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 73);
            Assert.AreEqual(fragmentArray[10].ColEnd, 73);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[11].Text, "c");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 77);
            Assert.AreEqual(fragmentArray[11].ColEnd, 77);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, "*");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 75);
            Assert.AreEqual(fragmentArray[12].ColEnd, 75);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, "+");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 71);
            Assert.AreEqual(fragmentArray[13].ColEnd, 71);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, "=");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 67);
            Assert.AreEqual(fragmentArray[14].ColEnd, 67);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[15].Text, "a");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 30);
            Assert.AreEqual(fragmentArray[15].ColEnd, 30);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[16].Text, ":");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 32);
            Assert.AreEqual(fragmentArray[16].ColEnd, 32);

            Assert.AreEqual(fragmentArray[17].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[17].Text, "int");
            Assert.AreEqual(fragmentArray[17].Line, 0);
            Assert.AreEqual(fragmentArray[17].ColStart, 34);
            Assert.AreEqual(fragmentArray[17].ColEnd, 36);

            Assert.AreEqual(fragmentArray[18].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[18].Text, "b");
            Assert.AreEqual(fragmentArray[18].Line, 0);
            Assert.AreEqual(fragmentArray[18].ColStart, 39);
            Assert.AreEqual(fragmentArray[18].ColEnd, 39);

            Assert.AreEqual(fragmentArray[19].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[19].Text, ":");
            Assert.AreEqual(fragmentArray[19].Line, 0);
            Assert.AreEqual(fragmentArray[19].ColStart, 41);
            Assert.AreEqual(fragmentArray[19].ColEnd, 41);

            Assert.AreEqual(fragmentArray[20].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[20].Text, "double");
            Assert.AreEqual(fragmentArray[20].Line, 0);
            Assert.AreEqual(fragmentArray[20].ColStart, 43);
            Assert.AreEqual(fragmentArray[20].ColEnd, 48);

            Assert.AreEqual(fragmentArray[21].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[21].Text, "c");
            Assert.AreEqual(fragmentArray[21].Line, 0);
            Assert.AreEqual(fragmentArray[21].ColStart, 51);
            Assert.AreEqual(fragmentArray[21].ColEnd, 51);

            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[22].Text, ":");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 53);
            Assert.AreEqual(fragmentArray[22].ColEnd, 53);

            Assert.AreEqual(fragmentArray[23].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[23].Text, "int");
            Assert.AreEqual(fragmentArray[23].Line, 0);
            Assert.AreEqual(fragmentArray[23].ColStart, 55);
            Assert.AreEqual(fragmentArray[23].ColEnd, 57);

            Assert.AreEqual(fragmentArray[24].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[24].Text, ")");
            Assert.AreEqual(fragmentArray[24].Line, 0);
            Assert.AreEqual(fragmentArray[24].ColStart, 58);
            Assert.AreEqual(fragmentArray[24].ColEnd, 58);

            Assert.AreEqual(fragmentArray[25].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[25].Text, "(");
            Assert.AreEqual(fragmentArray[25].Line, 0);
            Assert.AreEqual(fragmentArray[25].ColStart, 29);
            Assert.AreEqual(fragmentArray[25].ColEnd, 29);

            Assert.AreEqual(fragmentArray[26].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[26].Text, ",");
            Assert.AreEqual(fragmentArray[26].Line, 0);
            Assert.AreEqual(fragmentArray[26].ColStart, 37);
            Assert.AreEqual(fragmentArray[26].ColEnd, 37);

            Assert.AreEqual(fragmentArray[27].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[27].Text, ",");
            Assert.AreEqual(fragmentArray[27].Line, 0);
            Assert.AreEqual(fragmentArray[27].ColStart, 49);
            Assert.AreEqual(fragmentArray[27].ColEnd, 49);

            Assert.AreEqual(fragmentArray[28].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[28].Text, ":");
            Assert.AreEqual(fragmentArray[28].Line, 0);
            Assert.AreEqual(fragmentArray[28].ColStart, 23);
            Assert.AreEqual(fragmentArray[28].ColEnd, 23);

            Assert.AreEqual(fragmentArray[29].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[29].Text, "int");
            Assert.AreEqual(fragmentArray[29].Line, 0);
            Assert.AreEqual(fragmentArray[29].ColStart, 25);
            Assert.AreEqual(fragmentArray[29].ColEnd, 27);

        }

        [Test]
        public void TestAssociativeUnaryExpressionNode()
        {
            string content = "[Associative] b=~a;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 8);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, ";");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 18);
            Assert.AreEqual(fragmentArray[3].ColEnd, 18);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "b");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "a");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 17);
            Assert.AreEqual(fragmentArray[5].ColEnd, 17);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "~");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 16);
            Assert.AreEqual(fragmentArray[6].ColEnd, 16);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "=");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 15);
            Assert.AreEqual(fragmentArray[7].ColEnd, 15);

        }

        [Test]
        public void TestAssociativeForLoopNode()
        {
            string content = "[Associative] for (val in a){x = x + val;}";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();

            // We no longer support 'for' loop in Associative block.
            Assert.AreNotEqual(0, parser.errors.count);

            /*
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 17);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, "}");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 41);
            Assert.AreEqual(fragmentArray[3].ColEnd, 41);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[4].Text, ")");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 27);
            Assert.AreEqual(fragmentArray[4].ColEnd, 27);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[5].Text, "for");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 14);
            Assert.AreEqual(fragmentArray[5].ColEnd, 16);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[6].Text, "in");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 23);
            Assert.AreEqual(fragmentArray[6].ColEnd, 24);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, "{");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 28);
            Assert.AreEqual(fragmentArray[7].ColEnd, 28);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, "(");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 18);
            Assert.AreEqual(fragmentArray[8].ColEnd, 18);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, ";");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 40);
            Assert.AreEqual(fragmentArray[9].ColEnd, 40);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[10].Text, "x");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 29);
            Assert.AreEqual(fragmentArray[10].ColEnd, 29);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[11].Text, "x");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 33);
            Assert.AreEqual(fragmentArray[11].ColEnd, 33);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[12].Text, "val");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 37);//Expected 38
            Assert.AreEqual(fragmentArray[12].ColEnd, 39);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, "+");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 35);
            Assert.AreEqual(fragmentArray[13].ColEnd, 35);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, "=");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 31);
            Assert.AreEqual(fragmentArray[14].ColEnd, 31);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[15].Text, "a");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 26);
            Assert.AreEqual(fragmentArray[15].ColEnd, 26);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[16].Text, "val");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 19);
            Assert.AreEqual(fragmentArray[16].ColEnd, 21);
            */
        }

        [Test]
        public void TestAssociativeImportNode()
        {
            string content = "import (filename); ";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 5);

            Assert.AreEqual(fragmentArray[0].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[0].Text, "import");
            Assert.AreEqual(fragmentArray[0].Line, 0);
            Assert.AreEqual(fragmentArray[0].ColStart, 0);
            Assert.AreEqual(fragmentArray[0].ColEnd, 5);

            Assert.AreEqual(fragmentArray[1].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[1].Text, "(");
            Assert.AreEqual(fragmentArray[1].Line, 0);
            Assert.AreEqual(fragmentArray[1].ColStart, 7);
            Assert.AreEqual(fragmentArray[1].ColEnd, 7);

            Assert.AreEqual(fragmentArray[2].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[2].Text, ")");
            Assert.AreEqual(fragmentArray[2].Line, 0);
            Assert.AreEqual(fragmentArray[2].ColStart, 16);
            Assert.AreEqual(fragmentArray[2].ColEnd, 16);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[3].Text, "filename");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 8);
            Assert.AreEqual(fragmentArray[3].ColEnd, 15);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[4].Text, ";");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 17);
            Assert.AreEqual(fragmentArray[4].ColEnd, 17);
            
        }
        
        [Test]
        public void TestAssociativeImportNegativeNode()
        {
            string content = "import (filename01); a = 1; import(filename02); b = 2; ";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 18);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[3].Text, "filename01");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 8);
            Assert.AreEqual(fragmentArray[3].ColEnd, 17);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[4].Text, ";");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 19);
            Assert.AreEqual(fragmentArray[4].ColEnd, 19);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, ";");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 26);
            Assert.AreEqual(fragmentArray[5].ColEnd, 26);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[6].Text, "a");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 21);
            Assert.AreEqual(fragmentArray[6].ColEnd, 21);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[7].Text, "1");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 25);
            Assert.AreEqual(fragmentArray[7].ColEnd, 25);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, "=");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 23);
            Assert.AreEqual(fragmentArray[8].ColEnd, 23);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[9].Text, "");
            Assert.AreEqual(fragmentArray[9].Line, -2);
            Assert.AreEqual(fragmentArray[9].ColStart, -2);
            Assert.AreEqual(fragmentArray[9].ColEnd, -3);

        }

        [Test]
        public void TestAssociativeNullNode()
        {
            string content = "[Associative] a=null;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 7);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, ";");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 20);
            Assert.AreEqual(fragmentArray[3].ColEnd, 20);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "a");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[5].Text, "null");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 16);
            Assert.AreEqual(fragmentArray[5].ColEnd, 19);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, "=");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 15);
            Assert.AreEqual(fragmentArray[6].ColEnd, 15);
        }

        [Test]
        public void TestAssociativeParenExpressionNode()
        {
            string content = "[Associative] a=(b+c);";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 11);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, "(");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 16);
            Assert.AreEqual(fragmentArray[5].ColEnd, 16);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, ")");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 20);
            Assert.AreEqual(fragmentArray[6].ColEnd, 20);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[7].Text, "b");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 17);
            Assert.AreEqual(fragmentArray[7].ColEnd, 17);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[8].Text, "c");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 19);
            Assert.AreEqual(fragmentArray[8].ColEnd, 19);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[9].Text, "+");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 18);
            Assert.AreEqual(fragmentArray[9].ColEnd, 18);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "=");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 15);
            Assert.AreEqual(fragmentArray[10].ColEnd, 15);
        }

        [Test]
        public void TestAssociativeIdentifierListNode()
        {
            string content = "[Associative] d.b.c;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 9);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[4].Text, "d");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 14);
            Assert.AreEqual(fragmentArray[4].ColEnd, 14);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "b");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 16);
            Assert.AreEqual(fragmentArray[5].ColEnd, 16);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[6].Text, ".");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 15);
            Assert.AreEqual(fragmentArray[6].ColEnd, 15);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[7].Text, "c");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 18);
            Assert.AreEqual(fragmentArray[7].ColEnd, 18);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, ".");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 17);
            Assert.AreEqual(fragmentArray[8].ColEnd, 17);

        }

        [Test]
        public void TestAssociativeCommentNode()
        {
            string content = "//test";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 1);

            Assert.AreEqual(fragmentArray[0].CodeType, CodeFragment.Type.Comment);
            Assert.AreEqual(fragmentArray[0].Text, "//test");
            Assert.AreEqual(fragmentArray[0].Line, 0);
            Assert.AreEqual(fragmentArray[0].ColStart, 0);
            Assert.AreEqual(fragmentArray[0].ColEnd, 5);
        }

        [Test]
        public void TestAssociativeBaseConstructor()
        {
            //class Derived extends Base {
	        //  constructor DerivedConstr(derivedValue : int) : base.BaseConstructor(derivedValue + 2) { }
            //}
            string content = "class Derived extends Base { constructor DerivedConstr(derivedValue : int) : base.BaseConstructor(derivedValue + 2) { } }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 24);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, ".");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 81);
            Assert.AreEqual(fragmentArray[8].ColEnd, 81);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[10].Text, "base");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 77);
            Assert.AreEqual(fragmentArray[10].ColEnd, 80);
                                         
            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[11].Text, "BaseConstructor");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 82);
            Assert.AreEqual(fragmentArray[11].ColEnd, 96);
                                          
            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[12].Text, "derivedValue");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 98);
            Assert.AreEqual(fragmentArray[12].ColEnd, 109);
                                          
            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[13].Text, "2");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 113);
            Assert.AreEqual(fragmentArray[13].ColEnd, 113);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, "+");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 111);
            Assert.AreEqual(fragmentArray[14].ColEnd, 111);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[15].Text, "(");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 97);
            Assert.AreEqual(fragmentArray[15].ColEnd, 97);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[16].Text, ")");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 114);
            Assert.AreEqual(fragmentArray[16].ColEnd, 114);

            Assert.AreEqual(fragmentArray[17].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[17].Text, "}");
            Assert.AreEqual(fragmentArray[17].Line, 0);
            Assert.AreEqual(fragmentArray[17].ColStart, 118);
            Assert.AreEqual(fragmentArray[17].ColEnd, 118);

            Assert.AreEqual(fragmentArray[18].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[18].Text, "{");
            Assert.AreEqual(fragmentArray[18].Line, 0);
            Assert.AreEqual(fragmentArray[18].ColStart, 116);
            Assert.AreEqual(fragmentArray[18].ColEnd, 116);
        }

        [Test]
        public void TestAssociativeVariableDeclaration()
        {
            //a :int = 100;
            string content = "a :int = 100;";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 6);

            Assert.AreEqual(fragmentArray[4].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[4].Text, "int");
            Assert.AreEqual(fragmentArray[4].Line, 0);
            Assert.AreEqual(fragmentArray[4].ColStart, 3);
            Assert.AreEqual(fragmentArray[4].ColEnd, 5);

            Assert.AreEqual(fragmentArray[3].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[3].Text, ":");
            Assert.AreEqual(fragmentArray[3].Line, 0);
            Assert.AreEqual(fragmentArray[3].ColStart, 2);
            Assert.AreEqual(fragmentArray[3].ColEnd, 2);

            Assert.AreEqual(fragmentArray[2].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[2].Text, "a");
            Assert.AreEqual(fragmentArray[2].Line, 0);
            Assert.AreEqual(fragmentArray[2].ColStart, 0);
            Assert.AreEqual(fragmentArray[2].ColEnd, 0);

            Assert.AreEqual(fragmentArray[1].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[1].Text, "100");
            Assert.AreEqual(fragmentArray[1].Line, 0);
            Assert.AreEqual(fragmentArray[1].ColStart, 9);
            Assert.AreEqual(fragmentArray[1].ColEnd, 11);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, "=");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 7);
            Assert.AreEqual(fragmentArray[5].ColEnd, 7);
        }

        [Test]
        public void TestAssociativeModifierStack()
        {
            //a = {
            //    1 => a1;
            //    +1 => a2;
            //}
            string content = "[Associative]{ a = { 1 => a1; +1 => a2; } }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 25);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[5].Text, "a");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 15);
            Assert.AreEqual(fragmentArray[5].ColEnd, 15);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[14].Text, "a");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 15);
            Assert.AreEqual(fragmentArray[14].ColEnd, 15);

            Assert.AreEqual(fragmentArray[17].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[17].Text, "a");
            Assert.AreEqual(fragmentArray[17].Line, 0);
            Assert.AreEqual(fragmentArray[17].ColStart, 15);
            Assert.AreEqual(fragmentArray[17].ColEnd, 15);

            Assert.AreEqual(fragmentArray[18].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[18].Text, "a");
            Assert.AreEqual(fragmentArray[18].Line, 0);
            Assert.AreEqual(fragmentArray[18].ColStart, 15);
            Assert.AreEqual(fragmentArray[18].ColEnd, 15);

            Assert.AreEqual(fragmentArray[20].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[20].Text, "1");
            Assert.AreEqual(fragmentArray[20].Line, 0);
            Assert.AreEqual(fragmentArray[20].ColStart, 31);
            Assert.AreEqual(fragmentArray[20].ColEnd, 31);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[16].Text, "=");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 17);
            Assert.AreEqual(fragmentArray[16].ColEnd, 17);

            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[22].Text, "=");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 17);
            Assert.AreEqual(fragmentArray[22].ColEnd, 17);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[8].Text, "=>");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 23);
            Assert.AreEqual(fragmentArray[8].ColEnd, 24);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[9].Text, "a1");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 26);
            Assert.AreEqual(fragmentArray[9].ColEnd, 27);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "=>");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 33);
            Assert.AreEqual(fragmentArray[10].ColEnd, 34);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[11].Text, "a2");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 36);
            Assert.AreEqual(fragmentArray[11].ColEnd, 37);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[15].Text, "1");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 21);
            Assert.AreEqual(fragmentArray[15].ColEnd, 21);

            Assert.AreEqual(fragmentArray[21].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[21].Text, "+");
            Assert.AreEqual(fragmentArray[21].Line, 0);
            Assert.AreEqual(fragmentArray[21].ColStart, 30);
            Assert.AreEqual(fragmentArray[21].ColEnd, 30);

        }
        
        [Test]
        public void TestAssociativeNegativeUnaryExpr()
        {
            //[Associative]{
            //      a = 10; 
            //      b = -a;
            //}
            string content = "[Associative]{ a = 10; b = -a; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 14);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[10].Text, "b");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 23);
            Assert.AreEqual(fragmentArray[10].ColEnd, 23);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[11].Text, "a");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 28);
            Assert.AreEqual(fragmentArray[11].ColEnd, 28);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, "-");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 27);
            Assert.AreEqual(fragmentArray[12].ColEnd, 27);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, "=");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 25);
            Assert.AreEqual(fragmentArray[13].ColEnd, 25);

        }

        [Test]
        public void TestAssociativeInlineConditionExpr()
        {
            //[Associative]
            //{
            //  a = 3.141593;
            //  b = 100 ;
            //	i = 2 ;
            //	j : double = (i == 2) ? a : b;
            //}
            string content = "[Associative]{ a = 3.141593 ; b = 100 ; i = 2 ; j : double = (i == 2) ? a : b; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 31);

            Assert.AreEqual(fragmentArray[29].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[29].Text, "double");
            Assert.AreEqual(fragmentArray[29].Line, 0);
            Assert.AreEqual(fragmentArray[29].ColStart, 52);
            Assert.AreEqual(fragmentArray[29].ColEnd, 57);

            Assert.AreEqual(fragmentArray[28].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[28].Text, ":");
            Assert.AreEqual(fragmentArray[28].Line, 0);
            Assert.AreEqual(fragmentArray[28].ColStart, 50);
            Assert.AreEqual(fragmentArray[28].ColEnd, 50);

            Assert.AreEqual(fragmentArray[27].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[27].Text, "j");
            Assert.AreEqual(fragmentArray[27].Line, 0);
            Assert.AreEqual(fragmentArray[27].ColStart, 48);
            Assert.AreEqual(fragmentArray[27].ColEnd, 48);

            Assert.AreEqual(fragmentArray[18].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[18].Text, "(");
            Assert.AreEqual(fragmentArray[18].Line, 0);
            Assert.AreEqual(fragmentArray[18].ColStart, 61);
            Assert.AreEqual(fragmentArray[18].ColEnd, 61);

            Assert.AreEqual(fragmentArray[19].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[19].Text, ")");
            Assert.AreEqual(fragmentArray[19].Line, 0);
            Assert.AreEqual(fragmentArray[19].ColStart, 68);
            Assert.AreEqual(fragmentArray[19].ColEnd, 68);

            Assert.AreEqual(fragmentArray[20].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[20].Text, "i");
            Assert.AreEqual(fragmentArray[20].Line, 0);
            Assert.AreEqual(fragmentArray[20].ColStart, 62);
            Assert.AreEqual(fragmentArray[20].ColEnd, 62);

            Assert.AreEqual(fragmentArray[21].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[21].Text, "2");
            Assert.AreEqual(fragmentArray[21].Line, 0);
            Assert.AreEqual(fragmentArray[21].ColStart, 67);
            Assert.AreEqual(fragmentArray[21].ColEnd, 67);

            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[22].Text, "==");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 64);
            Assert.AreEqual(fragmentArray[22].ColEnd, 65);

            Assert.AreEqual(fragmentArray[23].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[23].Text, "a");
            Assert.AreEqual(fragmentArray[23].Line, 0);
            Assert.AreEqual(fragmentArray[23].ColStart, 72);
            Assert.AreEqual(fragmentArray[23].ColEnd, 72);

            Assert.AreEqual(fragmentArray[24].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[24].Text, "b");
            Assert.AreEqual(fragmentArray[24].Line, 0);
            Assert.AreEqual(fragmentArray[24].ColStart, 76);
            Assert.AreEqual(fragmentArray[24].ColEnd, 76);

            Assert.AreEqual(fragmentArray[25].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[25].Text, "?");
            Assert.AreEqual(fragmentArray[25].Line, 0);
            Assert.AreEqual(fragmentArray[25].ColStart, 70);
            Assert.AreEqual(fragmentArray[25].ColEnd, 70);

            Assert.AreEqual(fragmentArray[26].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[26].Text, ":");
            Assert.AreEqual(fragmentArray[26].Line, 0);
            Assert.AreEqual(fragmentArray[26].ColStart, 74);
            Assert.AreEqual(fragmentArray[26].ColEnd, 74);
        }

        [Test]
        public void TestAssociativeReplicationGuides()
        {
            //[Associative]
            //{
            //  a = { 1, 2 };
            //  b = { 2, 3 };
            //  c = a<1> + b<2>;
            //}
            string content = "[Associative]{ a = { 1, 2 }; b = { 2, 3 }; c = a<1> + b<2>; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 33);


            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[22].Text, "c");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 43);
            Assert.AreEqual(fragmentArray[22].ColEnd, 43);

            Assert.AreEqual(fragmentArray[23].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[23].Text, "<");
            Assert.AreEqual(fragmentArray[23].Line, 0);
            Assert.AreEqual(fragmentArray[23].ColStart, 48);
            Assert.AreEqual(fragmentArray[23].ColEnd, 48);

            Assert.AreEqual(fragmentArray[24].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[24].Text, "1");
            Assert.AreEqual(fragmentArray[24].Line, 0);
            Assert.AreEqual(fragmentArray[24].ColStart, 49);
            Assert.AreEqual(fragmentArray[24].ColEnd, 49);

            Assert.AreEqual(fragmentArray[25].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[25].Text, ">");
            Assert.AreEqual(fragmentArray[25].Line, 0);
            Assert.AreEqual(fragmentArray[25].ColStart, 50);
            Assert.AreEqual(fragmentArray[25].ColEnd, 50); 
            
            Assert.AreEqual(fragmentArray[26].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[26].Text, "a");
            Assert.AreEqual(fragmentArray[26].Line, 0);
            Assert.AreEqual(fragmentArray[26].ColStart, 47);
            Assert.AreEqual(fragmentArray[26].ColEnd, 47);

            Assert.AreEqual(fragmentArray[27].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[27].Text, "<");
            Assert.AreEqual(fragmentArray[27].Line, 0);
            Assert.AreEqual(fragmentArray[27].ColStart, 55);
            Assert.AreEqual(fragmentArray[27].ColEnd, 55);

            Assert.AreEqual(fragmentArray[28].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[28].Text, "2");
            Assert.AreEqual(fragmentArray[28].Line, 0);
            Assert.AreEqual(fragmentArray[28].ColStart, 56);
            Assert.AreEqual(fragmentArray[28].ColEnd, 56);

            Assert.AreEqual(fragmentArray[29].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[29].Text, ">");
            Assert.AreEqual(fragmentArray[29].Line, 0);
            Assert.AreEqual(fragmentArray[29].ColStart, 57);
            Assert.AreEqual(fragmentArray[29].ColEnd, 57);

            Assert.AreEqual(fragmentArray[30].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[30].Text, "b");
            Assert.AreEqual(fragmentArray[30].Line, 0);
            Assert.AreEqual(fragmentArray[30].ColStart, 54);
            Assert.AreEqual(fragmentArray[30].ColEnd, 54);

            Assert.AreEqual(fragmentArray[31].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[31].Text, "+");
            Assert.AreEqual(fragmentArray[31].Line, 0);
            Assert.AreEqual(fragmentArray[31].ColStart, 52);
            Assert.AreEqual(fragmentArray[31].ColEnd, 52);

            Assert.AreEqual(fragmentArray[32].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[32].Text, "=");
            Assert.AreEqual(fragmentArray[32].Line, 0);
            Assert.AreEqual(fragmentArray[32].ColStart, 45);
            Assert.AreEqual(fragmentArray[32].ColEnd, 45);
        }

        [Test]
        public void TestAssociativeReplicationGuidesWithRangeExpr()
        {
            //[Associative]
            //{
            //  x = 1..4;
            //  y = 5..8;
            //  d = (1..4) < 1 >..y < 2 > ..2;
            //}
            string content = "[Associative]{ x = 1..4; y = 5..8; d = (1..4)<1>..y<2>..2; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 35);

            Assert.AreEqual(fragmentArray[19].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[19].Text, "..");
            Assert.AreEqual(fragmentArray[19].Line, 0);
            Assert.AreEqual(fragmentArray[19].ColStart, 48);
            Assert.AreEqual(fragmentArray[19].ColEnd, 49);

            Assert.AreEqual(fragmentArray[20].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[20].Text, "..");
            Assert.AreEqual(fragmentArray[20].Line, 0);
            Assert.AreEqual(fragmentArray[20].ColStart, 54);
            Assert.AreEqual(fragmentArray[20].ColEnd, 55);

            Assert.AreEqual(fragmentArray[21].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[21].Text, "<");
            Assert.AreEqual(fragmentArray[21].Line, 0);
            Assert.AreEqual(fragmentArray[21].ColStart, 51);
            Assert.AreEqual(fragmentArray[21].ColEnd, 51);

            Assert.AreEqual(fragmentArray[22].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[22].Text, "2");
            Assert.AreEqual(fragmentArray[22].Line, 0);
            Assert.AreEqual(fragmentArray[22].ColStart, 52);
            Assert.AreEqual(fragmentArray[22].ColEnd, 52);

            Assert.AreEqual(fragmentArray[23].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[23].Text, ">");
            Assert.AreEqual(fragmentArray[23].Line, 0);
            Assert.AreEqual(fragmentArray[23].ColStart, 53);
            Assert.AreEqual(fragmentArray[23].ColEnd, 53);

            Assert.AreEqual(fragmentArray[24].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[24].Text, "y");
            Assert.AreEqual(fragmentArray[24].Line, 0);
            Assert.AreEqual(fragmentArray[24].ColStart, 50);
            Assert.AreEqual(fragmentArray[24].ColEnd, 50);

            Assert.AreEqual(fragmentArray[25].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[25].Text, "2");
            Assert.AreEqual(fragmentArray[25].Line, 0);
            Assert.AreEqual(fragmentArray[25].ColStart, 56);
            Assert.AreEqual(fragmentArray[25].ColEnd, 56);

            Assert.AreEqual(fragmentArray[26].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[26].Text, "<");
            Assert.AreEqual(fragmentArray[26].Line, 0);
            Assert.AreEqual(fragmentArray[26].ColStart, 45);
            Assert.AreEqual(fragmentArray[26].ColEnd, 45);

            Assert.AreEqual(fragmentArray[27].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[27].Text, "1");
            Assert.AreEqual(fragmentArray[27].Line, 0);
            Assert.AreEqual(fragmentArray[27].ColStart, 46);
            Assert.AreEqual(fragmentArray[27].ColEnd, 46);

            Assert.AreEqual(fragmentArray[28].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[28].Text, ">");
            Assert.AreEqual(fragmentArray[28].Line, 0);
            Assert.AreEqual(fragmentArray[28].ColStart, 47);
            Assert.AreEqual(fragmentArray[28].ColEnd, 47);

            Assert.AreEqual(fragmentArray[29].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[29].Text, "(");
            Assert.AreEqual(fragmentArray[29].Line, 0);
            Assert.AreEqual(fragmentArray[29].ColStart, 39);
            Assert.AreEqual(fragmentArray[29].ColEnd, 39);

            Assert.AreEqual(fragmentArray[30].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[30].Text, ")");
            Assert.AreEqual(fragmentArray[30].Line, 0);
            Assert.AreEqual(fragmentArray[30].ColStart, 44);
            Assert.AreEqual(fragmentArray[30].ColEnd, 44);

            Assert.AreEqual(fragmentArray[31].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[31].Text, "..");
            Assert.AreEqual(fragmentArray[31].Line, 0);
            Assert.AreEqual(fragmentArray[31].ColStart, 41);
            Assert.AreEqual(fragmentArray[31].ColEnd, 42);

            Assert.AreEqual(fragmentArray[32].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[32].Text, "4");
            Assert.AreEqual(fragmentArray[32].Line, 0);
            Assert.AreEqual(fragmentArray[32].ColStart, 43);
            Assert.AreEqual(fragmentArray[32].ColEnd, 43);

            Assert.AreEqual(fragmentArray[33].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[33].Text, "1");
            Assert.AreEqual(fragmentArray[33].Line, 0);
            Assert.AreEqual(fragmentArray[33].ColStart, 40);
            Assert.AreEqual(fragmentArray[33].ColEnd, 40);
        }
        
        [Test]
        public void TestAssociativeFunctionDeclarationAsBinaryExpr()
        {
            //[Associative]
            //{
            //  def fooX(x) = 2 * x;
            //}
            string content = "[Associative]{ def fooX(x) = 2 * x; }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 16);

            Assert.AreEqual(fragmentArray[5].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[5].Text, ";");
            Assert.AreEqual(fragmentArray[5].Line, 0);
            Assert.AreEqual(fragmentArray[5].ColStart, 34);
            Assert.AreEqual(fragmentArray[5].ColEnd, 34);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Keyword);
            Assert.AreEqual(fragmentArray[6].Text, "def");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 15);
            Assert.AreEqual(fragmentArray[6].ColEnd, 17);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[7].Text, "fooX");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 19);
            Assert.AreEqual(fragmentArray[7].ColEnd, 22);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Number);
            Assert.AreEqual(fragmentArray[9].Text, "2");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 29);
            Assert.AreEqual(fragmentArray[9].ColEnd, 29);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[10].Text, "x");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 33);
            Assert.AreEqual(fragmentArray[10].ColEnd, 33);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, "*");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 31);
            Assert.AreEqual(fragmentArray[11].ColEnd, 31);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[12].Text, "=");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 27);
            Assert.AreEqual(fragmentArray[12].ColEnd, 27);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[13].Text, "x");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 24);
            Assert.AreEqual(fragmentArray[13].ColEnd, 24);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, ")");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 25);
            Assert.AreEqual(fragmentArray[14].ColEnd, 25);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[15].Text, "(");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 23);
            Assert.AreEqual(fragmentArray[15].ColEnd, 23);
        }

        [Test]
        public void TestAssociativeArrayExprList()
        {
            //[Associative]
            //{
            //  Hello = { pt1, pt2 }.Translate();
            //}
            string content = "[Associative]{ Hello = { pt1, pt2 }.Translate(); }";
            Scanner scanner = new Scanner(ConvertToStream(content));
            parser = new DesignScript.Parser.Parser(scanner, core);
            parser.Parse();
            nodeProcessor = new NodeProcessor(parser.root.Body);
            nodeProcessor.GenerateFragments(out fragmentArray);

            Assert.AreEqual(fragmentArray.Length, 17);

            Assert.AreEqual(fragmentArray[6].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[6].Text, "Hello");
            Assert.AreEqual(fragmentArray[6].Line, 0);
            Assert.AreEqual(fragmentArray[6].ColStart, 15);
            Assert.AreEqual(fragmentArray[6].ColEnd, 19);

            Assert.AreEqual(fragmentArray[7].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[7].Text, ",");
            Assert.AreEqual(fragmentArray[7].Line, 0);
            Assert.AreEqual(fragmentArray[7].ColStart, 28);
            Assert.AreEqual(fragmentArray[7].ColEnd, 28);

            Assert.AreEqual(fragmentArray[8].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[8].Text, "pt1");
            Assert.AreEqual(fragmentArray[8].Line, 0);
            Assert.AreEqual(fragmentArray[8].ColStart, 25);
            Assert.AreEqual(fragmentArray[8].ColEnd, 27);

            Assert.AreEqual(fragmentArray[9].CodeType, CodeFragment.Type.Local);
            Assert.AreEqual(fragmentArray[9].Text, "pt2");
            Assert.AreEqual(fragmentArray[9].Line, 0);
            Assert.AreEqual(fragmentArray[9].ColStart, 30);
            Assert.AreEqual(fragmentArray[9].ColEnd, 32);

            Assert.AreEqual(fragmentArray[10].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[10].Text, "{");
            Assert.AreEqual(fragmentArray[10].Line, 0);
            Assert.AreEqual(fragmentArray[10].ColStart, 23);
            Assert.AreEqual(fragmentArray[10].ColEnd, 23);

            Assert.AreEqual(fragmentArray[11].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[11].Text, "}");
            Assert.AreEqual(fragmentArray[11].Line, 0);
            Assert.AreEqual(fragmentArray[11].ColStart, 34);
            Assert.AreEqual(fragmentArray[11].ColEnd, 34);

            Assert.AreEqual(fragmentArray[12].CodeType, CodeFragment.Type.Function);
            Assert.AreEqual(fragmentArray[12].Text, "Translate");
            Assert.AreEqual(fragmentArray[12].Line, 0);
            Assert.AreEqual(fragmentArray[12].ColStart, 36);
            Assert.AreEqual(fragmentArray[12].ColEnd, 44);

            Assert.AreEqual(fragmentArray[13].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[13].Text, "(");
            Assert.AreEqual(fragmentArray[13].Line, 0);
            Assert.AreEqual(fragmentArray[13].ColStart, 45);
            Assert.AreEqual(fragmentArray[13].ColEnd, 45);

            Assert.AreEqual(fragmentArray[14].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[14].Text, ")");
            Assert.AreEqual(fragmentArray[14].Line, 0);
            Assert.AreEqual(fragmentArray[14].ColStart, 46);
            Assert.AreEqual(fragmentArray[14].ColEnd, 46);

            Assert.AreEqual(fragmentArray[15].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[15].Text, ".");
            Assert.AreEqual(fragmentArray[15].Line, 0);
            Assert.AreEqual(fragmentArray[15].ColStart, 35);
            Assert.AreEqual(fragmentArray[15].ColEnd, 35);

            Assert.AreEqual(fragmentArray[16].CodeType, CodeFragment.Type.Punctuation);
            Assert.AreEqual(fragmentArray[16].Text, "=");
            Assert.AreEqual(fragmentArray[16].Line, 0);
            Assert.AreEqual(fragmentArray[16].ColStart, 21);
            Assert.AreEqual(fragmentArray[16].ColEnd, 21);
        }
    }
}

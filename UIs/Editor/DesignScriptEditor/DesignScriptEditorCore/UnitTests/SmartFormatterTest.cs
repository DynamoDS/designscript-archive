using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DesignScript.Editor.Core.UnitTest
{
    class SmartFormatterTest
    {
        [SetUp]
        public void Setup()
        {
            TextEditorCore.CreateTemporary();
        }

        [TearDown]
        public void TearDown()
        {
            TextEditorCore.InvalidateInstance();
        }

        [Test]
        public void TestExpansion()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("[Imperative]                                                \n");
            lineContents.Add("{                                                           \n");
            lineContents.Add("a=b>c?b:c;                                                  \n");
            lineContents.Add("a=b<c?c:b;                                                  \n");
            lineContents.Add("a=b&&c||d;                                                  \n");
            lineContents.Add("a=b&&(c||d);                                                \n");
            lineContents.Add("a=(b&&c)||d;                                                \n");
            lineContents.Add("a=(!a&&!b)||(c&&d);                                         \n");
            lineContents.Add("a=0xff00&0x0100|0x0200;                                     \n");
            lineContents.Add("a=b+c-d*e/f;                                                \n");
            lineContents.Add("a=(b+c)-(d*e)/f;                                            \n");
            lineContents.Add("a=b[c+d];                                                   \n");
            lineContents.Add("a=foo(b[c+d]);                                              \n");
            lineContents.Add("                                                            \n");
            lineContents.Add("def foo:double(a:int,b:double,c:double){return=a+b*c*2.0;}  \n");
            lineContents.Add("                                                            \n");
            lineContents.Add("def bar:double(a:int,b:double,c:double)                     \n");
            lineContents.Add("{                                                           \n");
            lineContents.Add("return=a+b*c*2.0;                                           \n");
            lineContents.Add("}                                                           \n");
            lineContents.Add("}                                                           \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("[Imperative]", lines[0]);
            Assert.AreEqual("{", lines[1]);
            Assert.AreEqual("    a = b > c ? b : c;", lines[2]);
            Assert.AreEqual("    a = b < c ? c : b;", lines[3]);
            Assert.AreEqual("    a = b && c || d;", lines[4]);
            Assert.AreEqual("    a = b && (c || d);", lines[5]);
            Assert.AreEqual("    a = (b && c) || d;", lines[6]);
            Assert.AreEqual("    a = (!a && !b) || (c && d);", lines[7]);
            Assert.AreEqual("    a = 0xff00 & 0x0100 | 0x0200;", lines[8]);
            Assert.AreEqual("    a = b + c - d * e / f;", lines[9]);
            Assert.AreEqual("    a = (b + c) - (d * e) / f;", lines[10]);
            Assert.AreEqual("    a = b[c + d];", lines[11]);
            Assert.AreEqual("    a = foo(b[c + d]);", lines[12]);
            Assert.AreEqual("    ", lines[13]);
            Assert.AreEqual("    def foo : double(a : int, b : double, c : double) { return = a + b * c * 2.0; }", lines[14]);
            Assert.AreEqual("    ", lines[15]);
            Assert.AreEqual("    def bar : double(a : int, b : double, c : double)", lines[16]);
            Assert.AreEqual("    {", lines[17]);
            Assert.AreEqual("        return = a + b * c * 2.0;", lines[18]);
            Assert.AreEqual("    }", lines[19]);
            Assert.AreEqual("}", lines[20]);
        }

        [Test]
        public void TestContraction()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("   [     Imperative    ]                                                 \n");
            lineContents.Add("   {                                                                     \n");
            lineContents.Add("      a   =   b   >   c   ?   b   :   c   ;                              \n");
            lineContents.Add("      a   =   b   <   c   ?   c   :   b   ;                              \n");
            lineContents.Add("      a   =   b   &&   c   ||   d   ;                                    \n");
            lineContents.Add("      a   =   b   &&   (   c   ||   d   )   ;                            \n");
            lineContents.Add("      a   =   (   b   &&   c   )   ||   d   ;                            \n");
            lineContents.Add("      a   =   (   !   a   &&   !   b   )   ||   (   c   &&   d   )   ;   \n");
            lineContents.Add("      a   =   0xff00    &    0x0100    |    0x0200  ;                    \n");
            lineContents.Add("      a   =   b   +   c   -   d   *   e   /   f   ;                      \n");
            lineContents.Add("      a   =   (   b   +   c   )   -   (   d   *   e   )   /   f   ;      \n");
            lineContents.Add("      a   =   b    [   c   +   d   ]   ;                                 \n");
            lineContents.Add("      a   =   foo    (   b   [   c   +   d   ]   )   ;                   \n");
            lineContents.Add("                                                                         \n");
            lineContents.Add("      def   foo   :   double   (   a   :   int   ,   b   :   double   ,   c   :   double   )   {   return   =   a   +   b   *   c   *   2.0   ;   }   \n");
            lineContents.Add("                                                                                                                                                      \n");
            lineContents.Add("      def   bar   :   double   (   a   :   int   ,   b   :   double   ,   c   :   double   )                                                          \n");
            lineContents.Add("      {                                                                  \n");
            lineContents.Add("                return    =   a   +    b   *   c   *   2.0    ;          \n");
            lineContents.Add("      }                                                                  \n");
            lineContents.Add("   }                                                                     \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("[Imperative]", lines[0]);
            Assert.AreEqual("{", lines[1]);
            Assert.AreEqual("    a = b > c ? b : c;", lines[2]);
            Assert.AreEqual("    a = b < c ? c : b;", lines[3]);
            Assert.AreEqual("    a = b && c || d;", lines[4]);
            Assert.AreEqual("    a = b && (c || d);", lines[5]);
            Assert.AreEqual("    a = (b && c) || d;", lines[6]);
            Assert.AreEqual("    a = (!a && !b) || (c && d);", lines[7]);
            Assert.AreEqual("    a = 0xff00 & 0x0100 | 0x0200;", lines[8]);
            Assert.AreEqual("    a = b + c - d * e / f;", lines[9]);
            Assert.AreEqual("    a = (b + c) - (d * e) / f;", lines[10]);
            Assert.AreEqual("    a = b[c + d];", lines[11]);
            Assert.AreEqual("    a = foo(b[c + d]);", lines[12]);
            Assert.AreEqual("    ", lines[13]);
            Assert.AreEqual("    def foo : double(a : int, b : double, c : double) { return = a + b * c * 2.0; }", lines[14]);
            Assert.AreEqual("    ", lines[15]);
            Assert.AreEqual("    def bar : double(a : int, b : double, c : double)", lines[16]);
            Assert.AreEqual("    {", lines[17]);
            Assert.AreEqual("        return = a + b * c * 2.0;", lines[18]);
            Assert.AreEqual("    }", lines[19]);
            Assert.AreEqual("}", lines[20]);
        }

        [Test]
        public void TestScopeIndentation01()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("[Imperative]                                          \n");
            lineContents.Add("{                                                     \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("        def maxValue : int(a : int, b : int, c : int) \n");
            lineContents.Add("        {                                             \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("if (a > b)                                            \n");
            lineContents.Add("{                                                     \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("        if (a > c)                                    \n");
            lineContents.Add("        {                                             \n");
            lineContents.Add("return a;                                             \n");
            lineContents.Add("        }                                             \n");
            lineContents.Add("        else                                          \n");
            lineContents.Add("        {                                             \n");
            lineContents.Add("return c;                                             \n");
            lineContents.Add("        }                                             \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("}                                                     \n");
            lineContents.Add("else                                                  \n");
            lineContents.Add("{                                                     \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("        if (b > c)                                    \n");
            lineContents.Add("        {                                             \n");
            lineContents.Add("return b;                                             \n");
            lineContents.Add("        }                                             \n");
            lineContents.Add("        else                                          \n");
            lineContents.Add("        {                                             \n");
            lineContents.Add("return c;                                             \n");
            lineContents.Add("        }                                             \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("}                                                     \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("        }                                             \n");
            lineContents.Add("                                                      \n");
            lineContents.Add("}                                                     \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("[Imperative]", lines[0]);
            Assert.AreEqual("{", lines[1]);
            Assert.AreEqual("    ", lines[2]);
            Assert.AreEqual("    def maxValue : int(a : int, b : int, c : int)", lines[3]);
            Assert.AreEqual("    {", lines[4]);
            Assert.AreEqual("        ", lines[5]);
            Assert.AreEqual("        if (a > b)", lines[6]);
            Assert.AreEqual("        {", lines[7]);
            Assert.AreEqual("            ", lines[8]);
            Assert.AreEqual("            if (a > c)", lines[9]);
            Assert.AreEqual("            {", lines[10]);
            Assert.AreEqual("                return a;", lines[11]);
            Assert.AreEqual("            }", lines[12]);
            Assert.AreEqual("            else", lines[13]);
            Assert.AreEqual("            {", lines[14]);
            Assert.AreEqual("                return c;", lines[15]);
            Assert.AreEqual("            }", lines[16]);
            Assert.AreEqual("            ", lines[17]);
            Assert.AreEqual("        }", lines[18]);
            Assert.AreEqual("        else", lines[19]);
            Assert.AreEqual("        {", lines[20]);
            Assert.AreEqual("            ", lines[21]);
            Assert.AreEqual("            if (b > c)", lines[22]);
            Assert.AreEqual("            {", lines[23]);
            Assert.AreEqual("                return b;", lines[24]);
            Assert.AreEqual("            }", lines[25]);
            Assert.AreEqual("            else", lines[26]);
            Assert.AreEqual("            {", lines[27]);
            Assert.AreEqual("                return c;", lines[28]);
            Assert.AreEqual("            }", lines[29]);
            Assert.AreEqual("            ", lines[30]);
            Assert.AreEqual("        }", lines[31]);
            Assert.AreEqual("        ", lines[32]);
            Assert.AreEqual("    }", lines[33]);
            Assert.AreEqual("    ", lines[34]);
            Assert.AreEqual("}", lines[35]);
        }

        [Test]
        public void TestScopeIndentation02()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("[Imperative]                                                       \n");
            lineContents.Add("{                                                                  \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("        def maxValue:int(a:int,b:int,c:int,d:int)                  \n");
            lineContents.Add("        {                                                          \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("if(a>b)                                                            \n");
            lineContents.Add("{                                                                  \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("        if(a>c)                                                    \n");
            lineContents.Add("        {                                                          \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("if(a>d)                                                            \n");
            lineContents.Add("        return a;                                                  \n");
            lineContents.Add("else                                                               \n");
            lineContents.Add("        return d;                                                  \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("        }                                                          \n");
            lineContents.Add("        else                                                       \n");
            lineContents.Add("        {                                                          \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("if(c>d)                                                            \n");
            lineContents.Add("        return c;                                                  \n");
            lineContents.Add("else                                                               \n");
            lineContents.Add("        return d;                                                  \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("        }                                                          \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("}else{                                                             \n");
            lineContents.Add("        if(b>c) {                                                  \n");
            lineContents.Add("if(b>d)                                                            \n");
            lineContents.Add("        return b;                                                  \n");
            lineContents.Add("else                                                               \n");
            lineContents.Add("        return d;                                                  \n");
            lineContents.Add("        }else{                                                     \n");
            lineContents.Add("if(c>d)                                                            \n");
            lineContents.Add("        return c;                                                  \n");
            lineContents.Add("else                                                               \n");
            lineContents.Add("        return d;                                                  \n");
            lineContents.Add("        }                                                          \n");
            lineContents.Add("}                                                                  \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("        }                                                          \n");
            lineContents.Add("                                                                   \n");
            lineContents.Add("}                                                                  \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("[Imperative]", lines[0]);
            Assert.AreEqual("{", lines[1]);
            Assert.AreEqual("    ", lines[2]);
            Assert.AreEqual("    def maxValue : int(a : int, b : int, c : int, d : int)", lines[3]);
            Assert.AreEqual("    {", lines[4]);
            Assert.AreEqual("        ", lines[5]);
            Assert.AreEqual("        if (a > b)", lines[6]);
            Assert.AreEqual("        {", lines[7]);
            Assert.AreEqual("            ", lines[8]);
            Assert.AreEqual("            if (a > c)", lines[9]);
            Assert.AreEqual("            {", lines[10]);
            Assert.AreEqual("                ", lines[11]);
            Assert.AreEqual("                if (a > d)", lines[12]);
            Assert.AreEqual("                    return a;", lines[13]);
            Assert.AreEqual("                else", lines[14]);
            Assert.AreEqual("                    return d;", lines[15]);
            Assert.AreEqual("                ", lines[16]);
            Assert.AreEqual("            }", lines[17]);
            Assert.AreEqual("            else", lines[18]);
            Assert.AreEqual("            {", lines[19]);
            Assert.AreEqual("                ", lines[20]);
            Assert.AreEqual("                if (c > d)", lines[21]);
            Assert.AreEqual("                    return c;", lines[22]);
            Assert.AreEqual("                else", lines[23]);
            Assert.AreEqual("                    return d;", lines[24]);
            Assert.AreEqual("                ", lines[25]);
            Assert.AreEqual("            }", lines[26]);
            Assert.AreEqual("            ", lines[27]);
            Assert.AreEqual("        } else {", lines[28]);
            Assert.AreEqual("            if (b > c) {", lines[29]);
            Assert.AreEqual("                if (b > d)", lines[30]);
            Assert.AreEqual("                    return b;", lines[31]);
            Assert.AreEqual("                else", lines[32]);
            Assert.AreEqual("                    return d;", lines[33]);
            Assert.AreEqual("            } else {", lines[34]);
            Assert.AreEqual("                if (c > d)", lines[35]);
            Assert.AreEqual("                    return c;", lines[36]);
            Assert.AreEqual("                else", lines[37]);
            Assert.AreEqual("                    return d;", lines[38]);
            Assert.AreEqual("            }", lines[39]);
            Assert.AreEqual("        }", lines[40]);
            Assert.AreEqual("        ", lines[41]);
            Assert.AreEqual("    }", lines[42]);
            Assert.AreEqual("    ", lines[43]);
            Assert.AreEqual("}", lines[44]);
        }

        [Test]
        public void TestScopeIndentation03()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("[Imperative]                           \n");
            lineContents.Add("{                                      \n");
            lineContents.Add("    def normalized:int(a:double)       \n");
            lineContents.Add("    {                                  \n");
            lineContents.Add("if(a<0.0)                              \n");
            lineContents.Add("return-1;                              \n");
            lineContents.Add("else if(a==0.0)                        \n");
            lineContents.Add("return 0;                              \n");
            lineContents.Add("else                                   \n");
            lineContents.Add("return 1;                              \n");
            lineContents.Add("    }                                  \n");
            lineContents.Add("}                                      \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents, 0, 0);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("[Imperative]\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 1, 1);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("{\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 2, 2);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("    def normalized : int(a : double)\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 5, 5);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("            return - 1;\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 7, 7);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("            return 0;\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 9, 9);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("            return 1;\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 4, 4);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("        if (a < 0.0)\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 6, 6);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("        else if (a == 0.0)\n", SmartFormatter.Instance.FormattedOutput);

            formatted = SmartFormatter.Instance.Format(lineContents, 8, 8);
            Assert.AreEqual(true, formatted);
            Assert.AreEqual("        else\n", SmartFormatter.Instance.FormattedOutput);
        }

        [Test, RequiresSTA]
        public void TestCommentedLines()
        {
            string fileContent =
                "// This case tests the ability to format embedded comments\n" +
                "// properly. These include both multi-line and single-line\n" +
                "// comments syntax.                                       \n" +
                "                                                          \n" +
                "// This line by itself should not be modified.            \n" +
                "                                                          \n" +
                "a=2;// This line should be formatted (not indented).      \n" +
                "                                                          \n" +
                "/* This is the beginning of a multi-line comment block    \n" +
                "                                                          \n" +
                "    This is a line within a multi-line comment block.     \n" +
                "    It is indented now but should not be indented further.\n" +
                "                                                          \n" +
                "This marks the end of a multi-line comment block */       \n" +
                "                                                          \n" +
                "    [Imperative] // This should be un-indented.           \n" +
                "{                                                         \n" +
                "    /*  This  is another multi-line                       \n" +
                "        comment block, only that it                       \n" +
                "        is  indented  for the scope */                    \n" +
                "                                                          \n" +
                "                   //   This should be indented.          \n" +
                "                                                          \n" +
                "b=3;// Formatting should resume on this line.             \n" +
                "         c     =    4    ; // And this line, too.         \n" +
                "}                                                         \n" +
                "                                                          \n" +
                "   //   Spaces in comments  shouldn't   be  normalized!   \n";

            Solution testSolution = Solution.CreateTemporary();
            testSolution.AddNewScript(fileContent);
            testSolution.ActivateScript(0);

            TextEditorCore textCore = TextEditorCore.Instance;
            textCore.ChangeScript(0);
            textCore.ParseScriptImmediate();

            SmartFormatter.Instance.AlternateEditorCore = textCore;
            bool formatted = textCore.FormatDocument();
            Assert.AreEqual(true, formatted);
            SmartFormatter.Instance.AlternateEditorCore = null;

            Assert.AreEqual("// This case tests the ability to format embedded comments\n", textCore.GetLine(0));
            Assert.AreEqual("// properly. These include both multi-line and single-line\n", textCore.GetLine(1));
            Assert.AreEqual("// comments syntax.                                       \n", textCore.GetLine(2));
            Assert.AreEqual("\n", textCore.GetLine(3));
            Assert.AreEqual("// This line by itself should not be modified.            \n", textCore.GetLine(4));
            Assert.AreEqual("\n", textCore.GetLine(5));
            Assert.AreEqual("a = 2; // This line should be formatted (not indented).      \n", textCore.GetLine(6));
            Assert.AreEqual("\n", textCore.GetLine(7));
            Assert.AreEqual("/* This is the beginning of a multi-line comment block    \n", textCore.GetLine(8));
            Assert.AreEqual("\n", textCore.GetLine(9));
            Assert.AreEqual("    This is a line within a multi-line comment block.     \n", textCore.GetLine(10));
            Assert.AreEqual("    It is indented now but should not be indented further.\n", textCore.GetLine(11));
            Assert.AreEqual("\n", textCore.GetLine(12));
            Assert.AreEqual("This marks the end of a multi-line comment block */       \n", textCore.GetLine(13));
            Assert.AreEqual("\n", textCore.GetLine(14));
            Assert.AreEqual("[Imperative] // This should be un-indented.           \n", textCore.GetLine(15));
            Assert.AreEqual("{\n", textCore.GetLine(16));
            Assert.AreEqual("    /*  This  is another multi-line                       \n", textCore.GetLine(17));
            Assert.AreEqual("        comment block, only that it                       \n", textCore.GetLine(18));
            Assert.AreEqual("        is  indented  for the scope */                    \n", textCore.GetLine(19));
            Assert.AreEqual("    \n", textCore.GetLine(20));
            Assert.AreEqual("    //   This should be indented.          \n", textCore.GetLine(21));
            Assert.AreEqual("    \n", textCore.GetLine(22));
            Assert.AreEqual("    b = 3; // Formatting should resume on this line.             \n", textCore.GetLine(23));
            Assert.AreEqual("    c = 4; // And this line, too.         \n", textCore.GetLine(24));
            Assert.AreEqual("}\n", textCore.GetLine(25));
            Assert.AreEqual("\n", textCore.GetLine(26));
            Assert.AreEqual("//   Spaces in comments  shouldn't   be  normalized!   \n", textCore.GetLine(27));
        }

        [Test]
        public void TestNegativeNumber1()
        {
            List<string> lineContents = new List<string>();

            // Expansion tests...
            lineContents.Add("a=b-foo(-2)-3;        \n");
            lineContents.Add("a=b+2-3;              \n");
            lineContents.Add("a=-b;                 \n");
            lineContents.Add("a=-2;                 \n");
            lineContents.Add("a=b>-2?-3:-4;         \n");
            lineContents.Add("a=-2+b[c-3]*b[-2];    \n");

            // Contraction tests...
            lineContents.Add("a   =  b  -  foo  (  -   2   )  -   3  ;                  \n");
            lineContents.Add("a   =  b  +  2    -   3    ;                              \n");
            lineContents.Add("a   =  -     b   ;                                        \n");
            lineContents.Add("a   =     -  2   ;                                        \n");
            lineContents.Add("a   =     b  >   -   2   ?   -   3  :   -   4    ;        \n");
            lineContents.Add("a   =  -  2  +   b  [  c  -  3  ]  *  b  [  -  2  ]  ;    \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("a = b - foo(-2) - 3;", lines[0]);
            Assert.AreEqual("a = b + 2 - 3;", lines[1]);
            Assert.AreEqual("a = -b;", lines[2]);
            Assert.AreEqual("a = -2;", lines[3]);
            Assert.AreEqual("a = b > -2 ? -3 : -4;", lines[4]);
            Assert.AreEqual("a = -2 + b[c - 3] * b[-2];", lines[5]);

            Assert.AreEqual("a = b - foo(-2) - 3;", lines[6]);
            Assert.AreEqual("a = b + 2 - 3;", lines[7]);
            Assert.AreEqual("a = -b;", lines[8]);
            Assert.AreEqual("a = -2;", lines[9]);
            Assert.AreEqual("a = b > -2 ? -3 : -4;", lines[10]);
            Assert.AreEqual("a = -2 + b[c - 3] * b[-2];", lines[11]);
        }

        [Test]
        public void TestNegativeNumber2()
        {
            List<string> lineContents = new List<string>();

            // Expansion tests...
            lineContents.Add("a=b-foo(1,2,-2)-3;        \n");

            // Contraction tests...
            lineContents.Add("a   =  b  -  foo  (  1  ,  2  ,  -   2   )  -   3  ;                  \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("a = b - foo(1, 2, -2) - 3;", lines[0]);
            Assert.AreEqual("a = b - foo(1, 2, -2) - 3;", lines[1]);
        }

        [Test]
        public void TestModifierStackSyntaxOld()
        {
            List<string> lineContents = new List<string>();

            // Expansion tests...
            lineContents.Add("a={b=>a,c=>a,d=>a};\n");

            // Contraction tests...
            lineContents.Add("a   =   {   b   =   >   a   ,   c   =   >   a   ,   d   =   >   a   }   ;   \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("a = { b => a, c => a, d => a };", lines[0]);
            Assert.AreEqual("a = { b => a, c => a, d => a };", lines[1]);
        }

        [Test]
        public void TestModifierStackSyntaxNew()
        {
            List<string> lineContents = new List<string>();

            // Expansion tests...
            lineContents.Add("a={b=>a;c=>a;d=>a};\n");

            // Contraction tests...
            lineContents.Add("a   =   {   b   =   >   a   ;   c   =   >   a   ;   d   =   >   a   }   ;   \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("a = { b => a; c => a; d => a };", lines[0]);
            Assert.AreEqual("a = { b => a; c => a; d => a };", lines[1]);
        }

        [Test]
        public void TestStringLiterals()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("import(\"Z:\\path\\module\\a.dll\");import(\"Z:\\path\\module\\a.dll\");\n");
            lineContents.Add("result=print(\"a=b>c?\"+\"d-e\"+\":\"+\"f+g;\")+print(\"i\"+\"=\"+\"j+k\"+\";\");\n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("import(\"Z:\\path\\module\\a.dll\"); import(\"Z:\\path\\module\\a.dll\");", lines[0]);
            Assert.AreEqual("result = print(\"a=b>c?\" + \"d-e\" + \":\" + \"f+g;\") + print(\"i\" + \"=\" + \"j+k\" + \";\");", lines[1]);
        }

        [Test]
        public void TestStringLiteralsCrash()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("import(\"");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(false, formatted); // Nothing to format in this case.

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("import(\"", lines[0]);
        }

        [Test]
        public void TestReplicationGuide01()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("resOne=one<c.d>+two<a.b><2>;\n");
            lineContents.Add("resTwo    =    one    <    c    .    d    >    +    two    <    a    .    b    >    <    2    >    ;\n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("resOne = one<c.d> + two<a.b><2>;", lines[0]);
            Assert.AreEqual("resTwo = one<c.d> + two<a.b><2>;", lines[1]);
        }

        [Test]
        public void TestReplicationGuide02()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("resOne=foo(a<1>,b<1>);\n");
            lineContents.Add("resTwo   =   foo   (   a   <   1   >   ,   b   <   1   >   )   ;\n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("resOne = foo(a<1>, b<1>);", lines[0]);
            Assert.AreEqual("resTwo = foo(a<1>, b<1>);", lines[1]);
        }

        [Test]
        public void TestLineBreakAfterBracket01()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("line=Line.ByStartPointEndPoint(\n");
            lineContents.Add("Point.ByCoordinates(0,0,0),\n");
            lineContents.Add("Point.ByCoordinates(9,9,9)\n");
            lineContents.Add(");\n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("line = Line.ByStartPointEndPoint(", lines[0]);
            Assert.AreEqual("    Point.ByCoordinates(0, 0, 0),", lines[1]);
            Assert.AreEqual("    Point.ByCoordinates(9, 9, 9)", lines[2]);
            Assert.AreEqual("    );", lines[3]);
        }

        [Test]
        public void TestLineBreakAfterBracket02()
        {
            List<string> lineContents = new List<string>();
            lineContents.Add("element=arr[\n");
            lineContents.Add("GetMiddleIndex()\n");
            lineContents.Add("];\n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("element = arr[", lines[0]);
            Assert.AreEqual("    GetMiddleIndex()", lines[1]);
            Assert.AreEqual("    ];", lines[2]);
        }

        [Test]
        public void TestFloatWithExponent()
        {
            List<string> lineContents = new List<string>();

            // Test space expansion.
            lineContents.Add("val1=-3.14159265358979323e-01;\n");
            lineContents.Add("val2=3.14159265358979323e-02;\n");
            lineContents.Add("val3=-3.14159265358979323e+03;\n");
            lineContents.Add("val4=3.14159265358979323e+04;\n");

            // Test space contraction.
            lineContents.Add("val5    =    -    3.14159265358979323e    -    05    ;    \n");
            lineContents.Add("val6    =    3.14159265358979323e    -    06    ;    \n");
            lineContents.Add("val7    =    -    3.14159265358979323e    +    07    ;    \n");
            lineContents.Add("val8    =    3.14159265358979323e    +    08    ;    \n");

            bool formatted = SmartFormatter.Instance.Format(lineContents);
            Assert.AreEqual(true, formatted);

            string[] lines = SmartFormatter.Instance.FormattedOutput.Split('\n');
            Assert.AreEqual("val1 = -3.14159265358979323e-01;", lines[0]);
            Assert.AreEqual("val2 = 3.14159265358979323e-02;", lines[1]);
            Assert.AreEqual("val3 = -3.14159265358979323e+03;", lines[2]);
            Assert.AreEqual("val4 = 3.14159265358979323e+04;", lines[3]);
            Assert.AreEqual("val5 = -3.14159265358979323e-05;", lines[4]);
            Assert.AreEqual("val6 = 3.14159265358979323e-06;", lines[5]);
            Assert.AreEqual("val7 = -3.14159265358979323e+07;", lines[6]);
            Assert.AreEqual("val8 = 3.14159265358979323e+08;", lines[7]);
        }
    }
}

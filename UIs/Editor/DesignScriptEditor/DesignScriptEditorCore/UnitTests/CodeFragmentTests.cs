using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScript.Parser;
using System.IO;

namespace DesignScript.Editor.Core.UnitTest
{
    class CodeFragmentTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSingleImportStatement()
        {
            string scriptContent = @"import";
            CodeFragmentManager manager = CreateFromContent(scriptContent);
            Assert.AreNotEqual(null, manager);

            CodeFragment fragment = null;
            int length = manager.GetFragment(0, 0, out fragment);
            Assert.AreEqual(6, length);
            Assert.AreNotEqual(null, fragment);
            Assert.AreEqual(CodeFragment.Type.Keyword, fragment.CodeType);
            Assert.AreEqual("import", fragment.Text);

            CodeFragment nextFragment = null;
            length = manager.GetNextFragment(fragment, out nextFragment);
            Assert.AreEqual(0, length);
            Assert.AreEqual(null, nextFragment);
        }

        private CodeFragmentManager CreateFromContent(string scriptContent)
        {
            MemoryStream inputStream = new MemoryStream(
                Encoding.Default.GetBytes(scriptContent));

            IParsedScript parsedScript = InterfaceFactory.CreateParsedScript();
            if (parsedScript.ParseStream(inputStream))
                return (new CodeFragmentManager(parsedScript));

            return null;
        }
    }
}

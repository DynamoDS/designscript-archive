using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class FileStorageTests
    {
        [SetUp]
        public void SetupTest()
        {
            ICoreComponent coreComponent = ClassFactory.CreateCoreComponent(null);
            coreComponent.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            ClassFactory.DestroyCoreComponent();
        }

        [Test]
        public void TestLoadingNonExistingFile()
        {
            IStorage storage = new FileStorage();
            string filePath = Path.GetTempPath() + "test.bin";

            Assert.Throws<FileNotFoundException>(() =>
            {
                ((FileStorage)storage).Load(filePath);
            });
        }

        [Test]
        public void TestLoadingSavingFile00()
        {
            try
            {
                IStorage s1 = new FileStorage();
                IStorage s2 = new FileStorage();

                string str = "World Hello";
                byte[] bytes = Encoding.ASCII.GetBytes(str);
                ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', ' ', ' ', ' ');

                s1.WriteBytes(signature, bytes);
                s1.WriteInteger(signature, 24);
                s1.WriteUnsignedInteger(signature, 2147483648);
                s1.WriteLong(signature, 3000000000);
                s1.WriteUnsignedLong(signature, 9223372036854775808);
                s1.WriteBoolean(signature, true);
                s1.WriteDouble(signature, 1.7);
                s1.WriteString(signature, "Hello World");

                string filePath = Path.GetTempPath() + "test.bin";
                ((FileStorage)s1).Save(filePath);
                ((FileStorage)s2).Load(filePath);

                byte[] a = s2.ReadBytes(signature);
                int b = s2.ReadInteger(signature);
                uint c = s2.ReadUnsignedInteger(signature);
                long d = s2.ReadLong(signature);
                ulong e = s2.ReadUnsignedLong(signature);
                bool f = s2.ReadBoolean(signature);
                double g = s2.ReadDouble(signature);
                string h = s2.ReadString(signature);

                Assert.AreEqual(bytes, a);
                Assert.AreEqual(24, b);
                Assert.AreEqual(2147483648, c);
                Assert.AreEqual(3000000000, d);
                Assert.AreEqual(9223372036854775808, e);
                Assert.AreEqual(true, f);
                Assert.AreEqual(1.7, g);
                Assert.AreEqual("Hello World", h);
            }
            finally
            {
                File.Delete(Path.GetTempPath() + "test.bin");
            }
        }

        [Test]
        public void TestLoadingSavingFile01()
        {
            // IDE-1066
            // Create a code block node 
            // Save the file
            // Load the file
            //
            string filePath = Path.GetTempPath() + "codeblock.bin";

            try
            {
                string commands = @"
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15310.0|d:15127.0|s:
                    BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000001|s:a = 1;|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

                GraphController graphController01 = new GraphController(null);
                bool result = graphController01.RunCommands(commands);
                Assert.AreEqual(true, result);

                graphController01.DoSaveGraph(filePath);
                GraphController graphController02 = new GraphController(null, filePath);

                Assert.AreEqual(1, graphController02.GetVisualNodes().Count);
                Assert.AreEqual(NodeType.CodeBlock, graphController02.GetVisualNodes()[0].VisualType);
            }
            finally
            {
                File.Delete(filePath);
            }
            
        }

        [Test]
        public void TestLoadingSavingFile02()
        {
            // IDE-1066
            // Create mutiple code block (total of 6 of different content)
            // Save the file
            // Load the file
            //
            string filePath = Path.GetTempPath() + "MutipleCodeblock.bin";

            try
            {
                string commands = @"
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15443.0|d:15122.0|s:
                    BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000001|s:a = 1;|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15692.0|d:15240.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15695.0|d:15242.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15432.0|d:15193.0|s:
                    BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000002|s:b + c|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15449.0|d:15254.0|s:
                    BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000003|s:1+4|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15613.0|d:15287.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15613.0|d:15290.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15425.0|d:15351.0|s:
                    BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000004|s:d = 12;\ne = f + g;\n\n5 + 4;|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15586.0|d:15251.0|s:
                    BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000005|s:h|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15610.0|d:15157.0|s:
                    BeginNodeEdit|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000006|s:j = Math.Cos(360);|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

                GraphController graphController01 = new GraphController(null);
                bool result = graphController01.RunCommands(commands);
                Assert.AreEqual(true, result);

                graphController01.DoSaveGraph(filePath);
                GraphController graphController02 = new GraphController(null, filePath);

                Assert.AreEqual(6, graphController02.GetVisualNodes().Count);
            }
            finally
            {
                File.Delete(filePath);
            }

        }

        [Test]
        public void TestLoadingSavingFile03()
        {
            // IDE-1066
            // Create mutiple code block with implicit and explicit connection
            // Save the file
            // Load the file
            //
            string filePath = Path.GetTempPath() + "MultipleCodeblockConnection.bin";

            try
            {
                string commands = @"
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15330.0|d:15157.0|s:
                    BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15541.0|d:15170.0|s:
                    BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000002|s:b = a;|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15431.0|d:15272.0|s:
                    BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000003|s:100|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateFunctionNode|d:15691.0|d:15216.0|s:|s:+|s:double,double
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15579.0|d:15172.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15645.0|d:15206.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15461.0|d:15275.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15643.0|d:15234.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None";

                GraphController graphController01 = new GraphController(null);
                bool result = graphController01.RunCommands(commands);
                Assert.AreEqual(true, result);

                graphController01.DoSaveGraph(filePath);
                GraphController graphController02 = new GraphController(null, filePath);

                Assert.AreEqual(4, graphController02.GetVisualNodes().Count);
                Assert.AreEqual(NodeType.CodeBlock, graphController02.GetVisualNodes()[0].VisualType);
                Assert.AreEqual(NodeType.CodeBlock, graphController02.GetVisualNodes()[1].VisualType);
                Assert.AreEqual(NodeType.CodeBlock, graphController02.GetVisualNodes()[2].VisualType);
                Assert.AreEqual(NodeType.Function, graphController02.GetVisualNodes()[3].VisualType);
                Assert.IsNotNull(graphController02.GetVisualEdge(0x60000001));
                Assert.IsNotNull(graphController02.GetVisualEdge(0x60000002));
                Assert.IsNotNull(graphController02.GetVisualEdge(0x60000003));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public void TestSaveAndLoad()
        {
            string filePath = Path.GetTempPath() + "test.bin";

            try
            {
                string commands = @"
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15310.0|d:15127.0|s:
                    BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000001|s:a = 1;|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateDriverNode|d:15387.0|d:15195.0
                    CreateFunctionNode|d:15444.0|d:15256.0|s:|s:+|s:double,double
                    CreateIdentifierNode|d:15533.0|d:15329.0";

                GraphController graphController01 = new GraphController(null);
                bool result = graphController01.RunCommands(commands);
                Assert.AreEqual(true, result);

                
                graphController01.DoSaveGraph(filePath);
                GraphController graphController02 = new GraphController(null, filePath);

                IVisualNode node00 = graphController02.GetVisualNodes()[0];
                Assert.AreEqual(15310.0, node00.X);
                Assert.AreEqual(15127.0, node00.Y);

                IVisualNode node01 = graphController02.GetVisualNodes()[1];
                Assert.AreEqual(15387.0, node01.X);
                Assert.AreEqual(15195.0, node01.Y);

                IVisualNode node02 = graphController02.GetVisualNodes()[2];
                Assert.AreEqual(15444.0, node02.X);
                Assert.AreEqual(15256.0, node02.Y);

                IVisualNode node03 = graphController02.GetVisualNodes()[3];
                Assert.AreEqual(15533.0, node03.X);
                Assert.AreEqual(15329.0, node03.Y);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public void TestLoadingSavedFile00()
        {
            // IDE-1213
            // create two codeblock nodes and identifier and connect them
            // save and load it 
            //
            IStorage s2 = new FileStorage();
            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "save.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(4, graphController.GetVisualNodes().Count);
            IVisualNode node11 = graphController.GetVisualNodes()[0];
            Assert.AreEqual(NodeType.CodeBlock, node11.VisualType);
            IVisualNode node12 = graphController.GetVisualNodes()[1];
            Assert.AreEqual(NodeType.CodeBlock, node12.VisualType);
            IVisualNode node13 = graphController.GetVisualNodes()[2];
            Assert.AreEqual(NodeType.Identifier, node13.VisualType);
            IVisualNode node14 = graphController.GetVisualNodes()[3];
            Assert.AreEqual(NodeType.Identifier, node14.VisualType);
            IVisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            IVisualEdge edge02 = graphController.GetVisualEdge(0x60000002);
        }
        [Test]
        public void TestLoadingSavedFile01()
        {
            // create two codeblock nodes and add them and connect to the identifer
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "twoslots.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(4, graphController.GetVisualNodes().Count);
            IVisualNode node11 = graphController.GetVisualNodes()[0];
            Assert.AreEqual(NodeType.CodeBlock, node11.VisualType);
            IVisualNode node12 = graphController.GetVisualNodes()[1];
            Assert.AreEqual(NodeType.CodeBlock, node12.VisualType);
            IVisualNode node13 = graphController.GetVisualNodes()[2];
            Assert.AreEqual(NodeType.Function, node13.VisualType);
            IVisualNode node14 = graphController.GetVisualNodes()[3];
            Assert.AreEqual(NodeType.Identifier, node14.VisualType);
            IVisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            IVisualEdge edge02 = graphController.GetVisualEdge(0x60000002);
            IVisualEdge edge03 = graphController.GetVisualEdge(0x60000003);
        }
        [Test]
        public void TestLoadingSavedFile02()
        {
            // create three codeblock nodes and add them and connect to the functiona and the to the identifier
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "threeslots.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(5, graphController.GetVisualNodes().Count);
            IVisualNode node11 = graphController.GetVisualNodes()[0];
            Assert.AreEqual(NodeType.CodeBlock, node11.VisualType);
            IVisualNode node12 = graphController.GetVisualNodes()[1];
            Assert.AreEqual(NodeType.CodeBlock, node12.VisualType);
            IVisualNode node13 = graphController.GetVisualNodes()[2];
            Assert.AreEqual(NodeType.CodeBlock, node13.VisualType);
            IVisualNode node14 = graphController.GetVisualNodes()[3];
            Assert.AreEqual(NodeType.Function, node14.VisualType);
            IVisualNode node15 = graphController.GetVisualNodes()[4];
            Assert.AreEqual(NodeType.Identifier, node15.VisualType);
            IVisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            IVisualEdge edge02 = graphController.GetVisualEdge(0x60000002);
            IVisualEdge edge03 = graphController.GetVisualEdge(0x60000003);
            IVisualEdge edge04 = graphController.GetVisualEdge(0x60000004);
        }
        [Test]
        public void TestLoadingSavedFile03()
        {
            // create a codeblock and connect it to a driver
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "driver.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(2, graphController.GetVisualNodes().Count);
            IVisualNode node11 = graphController.GetVisualNodes()[0];
            Assert.AreEqual(NodeType.Driver, node11.VisualType);
            IVisualNode node12 = graphController.GetVisualNodes()[1];
            Assert.AreEqual(NodeType.Identifier, node12.VisualType);
            IVisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
        }

        [Test]
        public void TestLoadingSavedFile04()
        {
            // create three codeblock nodes and connect it to a function node with three slots
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "threeslots.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(5, graphController.GetVisualNodes().Count);
            IVisualNode node11 = graphController.GetVisualNodes()[0];
            Assert.AreEqual(NodeType.CodeBlock, node11.VisualType);
            IVisualNode node12 = graphController.GetVisualNodes()[1];
            Assert.AreEqual(NodeType.CodeBlock, node12.VisualType);
            IVisualNode node13 = graphController.GetVisualNodes()[2];
            Assert.AreEqual(NodeType.CodeBlock, node13.VisualType);
            IVisualNode node14 = graphController.GetVisualNodes()[3];
            Assert.AreEqual(NodeType.Function, node14.VisualType);
            IVisualNode node15 = graphController.GetVisualNodes()[4];
            Assert.AreEqual(NodeType.Identifier, node15.VisualType);
            IVisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            IVisualEdge edge02 = graphController.GetVisualEdge(0x60000002);
            IVisualEdge edge03 = graphController.GetVisualEdge(0x60000003);
            IVisualEdge edge04 = graphController.GetVisualEdge(0x60000004);
            IVisualEdge edge05 = graphController.GetVisualEdge(0x60000005);
        }

        [Test]
        public void TestLoadingSavedFile05()
        {
            // create five codeblock nodes and connect it to a function node with five slots
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "fiveslots.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(6, graphController.GetVisualNodes().Count);
            IVisualNode node11 = graphController.GetVisualNodes()[0];
            Assert.AreEqual(NodeType.CodeBlock, node11.VisualType);
            IVisualNode node12 = graphController.GetVisualNodes()[1];
            Assert.AreEqual(NodeType.CodeBlock, node12.VisualType);
            IVisualNode node13 = graphController.GetVisualNodes()[2];
            Assert.AreEqual(NodeType.CodeBlock, node13.VisualType);
            IVisualNode node14 = graphController.GetVisualNodes()[3];
            Assert.AreEqual(NodeType.CodeBlock, node14.VisualType);
            IVisualNode node15 = graphController.GetVisualNodes()[4];
            Assert.AreEqual(NodeType.Function, node15.VisualType);
            IVisualNode node16 = graphController.GetVisualNodes()[5];
            Assert.AreEqual(NodeType.CodeBlock, node16.VisualType);
            IVisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            IVisualEdge edge02 = graphController.GetVisualEdge(0x60000002);
            IVisualEdge edge03 = graphController.GetVisualEdge(0x60000003);
            IVisualEdge edge04 = graphController.GetVisualEdge(0x60000004);
            IVisualEdge edge05 = graphController.GetVisualEdge(0x60000005);
        }

        [Test]
        public void TestLoadingSavedFile06()
        {
            // create six codeblock nodes and connect it to a function node with six slots
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "sixslots.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(7, graphController.GetVisualNodes().Count);
            IVisualNode node11 = graphController.GetVisualNodes()[0];
            Assert.AreEqual(NodeType.CodeBlock, node11.VisualType);
            IVisualNode node12 = graphController.GetVisualNodes()[1];
            Assert.AreEqual(NodeType.CodeBlock, node12.VisualType);
            IVisualNode node13 = graphController.GetVisualNodes()[2];
            Assert.AreEqual(NodeType.CodeBlock, node13.VisualType);
            IVisualNode node14 = graphController.GetVisualNodes()[3];
            Assert.AreEqual(NodeType.CodeBlock, node14.VisualType);
            IVisualNode node15 = graphController.GetVisualNodes()[4];
            Assert.AreEqual(NodeType.CodeBlock, node15.VisualType);
            IVisualNode node16 = graphController.GetVisualNodes()[5];
            Assert.AreEqual(NodeType.CodeBlock, node16.VisualType);
            IVisualNode node17 = graphController.GetVisualNodes()[6];
            Assert.AreEqual(NodeType.Function, node17.VisualType);
            IVisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            IVisualEdge edge02 = graphController.GetVisualEdge(0x60000002);
            IVisualEdge edge03 = graphController.GetVisualEdge(0x60000003);
            IVisualEdge edge04 = graphController.GetVisualEdge(0x60000004);
            IVisualEdge edge05 = graphController.GetVisualEdge(0x60000005);
            IVisualEdge edge06 = graphController.GetVisualEdge(0x60000006);
        }

        [Test]
        public void TestLoadingSavedFile07()
        {
            // Create a save file at temp path contain 4 nodes and 3 edges
            // Load the file on a new graph controller instance
            // Check and add another node
            // Overwritten the same file
            // Load the file again in a new graph controller instance
            // Check the number of node
            // Delete temp path
            //
            try
            {
                string commands = @"
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:10341.0|d:10228.0|s:
                    BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000001|s:200|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateIdentifierNode|d:10534.0|d:10264.0
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10376.0|d:10242.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10537.0|d:10278.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:10352.0|d:10329.0|s:
                    BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000003|s:100|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateFunctionNode|d:10526.0|d:10354.0|s:|s:+|s:double,double
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10379.0|d:10240.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10526.0|d:10365.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10387.0|d:10342.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10529.0|d:10385.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None";

                GraphController graphController01 = new GraphController(null);
                bool result = graphController01.RunCommands(commands);
                Assert.AreEqual(true, result);

                string filePath = Path.GetTempPath() + "test.bin";
                graphController01.DoSaveGraph(filePath);
                GraphController graphController02 = new GraphController(null, filePath);

                Assert.AreEqual(4, graphController02.GetVisualNodes().Count);
                IVisualNode node01 = graphController02.GetVisualNodes()[0];
                Assert.AreEqual(NodeType.CodeBlock, node01.VisualType);
                IVisualNode node02 = graphController02.GetVisualNodes()[1];
                Assert.AreEqual(NodeType.Identifier, node02.VisualType);
                IVisualNode node03 = graphController02.GetVisualNodes()[2];
                Assert.AreEqual(NodeType.CodeBlock, node03.VisualType);
                IVisualNode node04 = graphController02.GetVisualNodes()[3];
                Assert.AreEqual(NodeType.Function, node04.VisualType);
                IVisualEdge edge01 = graphController02.GetVisualEdge(0x60000001);
                Assert.IsNotNull(edge01);
                IVisualEdge edge02 = graphController02.GetVisualEdge(0x60000002);
                Assert.IsNotNull(edge02);
                IVisualEdge edge03 = graphController02.GetVisualEdge(0x60000003);
                Assert.IsNotNull(edge03);

                IVisualNode node05 = new CodeBlockNode(graphController02, "a = 1;");
                graphController02.DoSaveGraph(filePath);
                GraphController graphController03 = new GraphController(null, filePath);
                Assert.AreEqual(5, graphController03.GetVisualNodes().Count);
            }
            finally
            {
                string filePath = Path.GetTempPath() + "test.bin";
                File.Delete(filePath);
            }
        }

        [Test]
        public void Defect_IDE_1533()
        {
            // Create a simple file with CBN a = 1;
            // Save it as : DesignScriptStudio.Tests\\UnitTests\\testfiles\\Defect_IDE_1533.bin
            // Now try to open the file with the file name only ("Defect_IDE_1533.bin"); and verify

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15417.0|d:15222.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController graphController01 = new GraphController(null);
            bool result = graphController01.RunCommands(commands);
            Assert.AreEqual(true, result);

            string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
            string filePath = testPath + "Defect_IDE_1533.bin";
            graphController01.DoSaveGraph(filePath);
            GraphController graphController02 = new GraphController(null, filePath); 
            
            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(15417, node.X);
            Assert.AreEqual(15222, node.Y);
            Assert.AreEqual(false, node.Selected);
        }

        [Test]
        public void Defect_IDE_1556()
        {
            // Load the LinesToGrid.bin file. It should give exception, but no crash
            
            Assert.Throws<InvalidDataException>(() =>
            {
                IStorage s2 = new FileStorage();
                string testPath = "..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\";
                string filePath = testPath + "LinesToGrid.bin";
                ((FileStorage)s2).Load(filePath);

                GraphController graphController = new GraphController(null, filePath);

                Assert.AreEqual(0, graphController.GetVisualNodes().Count);
            });
           
        }

        [Test]
        public void Defect_IDE_1705()
        {
            // create six codeblock nodes and connect it to a function node with six slots
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_1705.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(3, graphController.GetVisualNodes().Count);

            //IVisualNode node01 = graphController.GetVisualNode(0x10000002);// Code Block Node
            //Assert.AreEqual("1", node01.PreviewValue);

        }

        [Test]
        public void Defect_IDE_1803()
        {
            // create six codeblock nodes and connect it to a function node with six slots
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_1803_CreateCuboid.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(2, graphController.GetVisualNodes().Count);

            //IVisualNode node01 = graphController.GetVisualNode(0x10000002);// Code Block Node
            //Assert.AreEqual("1", node01.PreviewValue);

        }

        [Test]
        public void Defect_IDE_1744()
        {
            // create six codeblock nodes and connect it to a function node with six slots
            // save and load it 
            //IDE-1213
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "AdditionOfNumber_FromExternalDS.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(2, graphController.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1796()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1796
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "grid_working.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(16, graphController.GetVisualNodes().Count);

        }
        [Test]
        public void Defect_IDE_2053()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2053
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "class.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(1, graphController.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1946_1()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1946
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_IDE_1946_1.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(11, graphController.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1946_2()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1946
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_IDE_1946_2.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(16, graphController.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_2023()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2023
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_IDE_2023.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            Assert.AreEqual(4, graphController.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_2241()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2241
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_IDE_2241.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);

            string commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x1000000f|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x1000000f|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
DeleteComponents
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x1000000e
CreateSubRadialMenu|i:1002
CreateSubRadialMenu|i:1002
SelectMenuItem|i:1002|d:16850.50647433|d:15341.08333333|u:0x1000000e|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000010|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000010
CreateSubRadialMenu|i:1001
CreateSubRadialMenu|i:1001
SelectMenuItem|i:1001|d:16917.173141|d:15304.41666667|u:0x10000010|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000011|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000011|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000011|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:16930.50647433|d:15309.41666667
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000011|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:17103.83980766|d:15289.41666667
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000011|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result); 
            
            Assert.AreEqual(11, graphController.GetVisualNodes().Count);
         }


        [Test]
        public void Defect_IDE_2087()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2241
            try
            {
                string commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15330.0|d:15123.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:1|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15342.0|d:15206.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:a|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15433.0|d:15128.0|s:
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:b = a|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15348.0|d:15124.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15326.0|d:15206.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
CreateFunctionNode|d:15593.0|d:15192.0|s:Special Nodes|s:+|s:double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15347.0|d:15124.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None|d:15550.0|d:15190.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15473.0|d:15130.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15538.0|d:15209.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None";

                GraphController graphController01 = new GraphController(null);
                bool result = graphController01.RunCommands(commands);
                Assert.AreEqual(true, result);

                string filePath = Path.GetTempPath() + "Defect_IDE_2087.bin";
                graphController01.DoSaveGraph(filePath);
                GraphController graphController02 = new GraphController(null, filePath);

                // there should not be any errors 
                ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
                Assert.AreEqual(0, buildStatus.ErrorCount);
                Assert.AreEqual(0, buildStatus.WarningCount);

                Assert.AreEqual(4, graphController02.GetVisualNodes().Count);

                IVisualNode node01 = graphController02.GetVisualNodes()[0];
                Assert.AreEqual(NodeType.CodeBlock, node01.VisualType);
                IVisualNode node02 = graphController02.GetVisualNodes()[1];
                Assert.AreEqual(NodeType.CodeBlock, node02.VisualType);
                IVisualNode node03 = graphController02.GetVisualNodes()[2];
                Assert.AreEqual(NodeType.CodeBlock, node03.VisualType);
                IVisualNode node04 = graphController02.GetVisualNodes()[3];
                Assert.AreEqual(NodeType.Function, node04.VisualType);              

            }
            finally
            {
                string filePath = Path.GetTempPath() + "Defect_IDE_2087.bin";
                File.Delete(filePath);
            }
        }

        [Test]
        public void Defect_IDE_2225()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2225
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_IDE_2225.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);
            Assert.AreEqual(6, graphController.GetVisualNodes().Count);

            // there should not be any errors 
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            IVisualNode node = graphController.GetVisualNode(0x10000003);
            // there should be no error/warning from this node
            Assert.AreEqual(String.Empty, node.ErrorMessage);
         
        }

        [Test]
        public void Defect_IDE_2041()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2041
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_IDE_2041.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController graphController = new GraphController(null, filePath);
            Assert.AreEqual(8, graphController.GetVisualNodes().Count);

            // there should not be any errors 
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            IVisualNode node = graphController.GetVisualNode(0x10000012);
            // there should be no error/warning from this node
            Assert.AreEqual(String.Empty, node.ErrorMessage);

        }
    }
}

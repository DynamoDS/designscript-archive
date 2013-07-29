using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;
using System.Threading;
using GraphToDSCompiler;

namespace DesignScriptStudio.Tests.UnitTests
{
    class CodeBlockNodeTests
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
        public void TestGetLineNumberFromInputSlotIndex()
        {
            IGraphController graphController = new GraphController(null);
            Dictionary<int, List<VariableLine>> input = new Dictionary<int, List<VariableLine>>();
            Dictionary<int, List<VariableLine>> inputNull = null;
            List<VariableLine> variableLines = new List<VariableLine>();
            variableLines.Add(new VariableLine("a", 1));
            variableLines.Add(new VariableLine("b", 2));
            input.Add(0, variableLines);
            int slotIndex = 1;

            Assert.AreEqual(2, CodeBlockNode.GetLineNumberFromInputSlotIndex(input, slotIndex));

            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual(2, CodeBlockNode.GetLineNumberFromInputSlotIndex(inputNull, slotIndex));
            });
        }

        [Test]
        public void TestGetInputSlotIndexFromLineNumber()
        {
            IGraphController graphController = new GraphController(null);
            Dictionary<int, List<VariableLine>> input = new Dictionary<int, List<VariableLine>>();
            Dictionary<int, List<VariableLine>> inputNull = null;
            List<VariableLine> variableLines = new List<VariableLine>();
            variableLines.Add(new VariableLine("a", 1));
            variableLines.Add(new VariableLine("b", 2));
            input.Add(0, variableLines);
            int lineNumber = 2;

            Assert.AreEqual(1, CodeBlockNode.GetInputSlotIndexFromLineNumber(input, lineNumber));

            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual(1, CodeBlockNode.GetInputSlotIndexFromLineNumber(inputNull, lineNumber));
            });
        }

        [Test]
        public void TestGetInputSlotIndicesByName()
        {
            IGraphController graphController = new GraphController(null);
            Dictionary<int, List<VariableLine>> input = new Dictionary<int, List<VariableLine>>();
            Dictionary<int, List<VariableLine>> inputNull = null;
            List<VariableLine> variableLines = new List<VariableLine>();
            variableLines.Add(new VariableLine("a", 0));
            variableLines.Add(new VariableLine("b", 1));
            input.Add(0, variableLines);
            string variable = "b";
            List<int> expected = new List<int>();
            expected.Add(1);

            Assert.AreEqual(expected, CodeBlockNode.GetInputSlotIndicesByName(input, variable));
            Assert.Throws<NullReferenceException>(() =>
            {
                Assert.AreEqual(expected, CodeBlockNode.GetInputSlotIndicesByName(inputNull, variable));
            });
        }

        [Test]
        public void TestGetInputSlotNameFromInputSlotIndex()
        {
            IGraphController graphController = new GraphController(null);
            Dictionary<int, List<VariableLine>> input = new Dictionary<int, List<VariableLine>>();
            Dictionary<int, List<VariableLine>> inputNull = null;
            List<VariableLine> variableLines = new List<VariableLine>();
            variableLines.Add(new VariableLine("a", 0));
            variableLines.Add(new VariableLine("b", 1));
            input.Add(0, variableLines);
            int slotIndex = 1;

            Assert.AreEqual("b", CodeBlockNode.GetInputSlotNameFromInputSlotIndex(input, 1));

            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual("b", CodeBlockNode.GetInputSlotNameFromInputSlotIndex(inputNull, slotIndex));
            });
        }

        [Test]
        public void TestGetReferencedVariables()
        {
            IGraphController graphController = new GraphController(null);
            Dictionary<int, List<VariableLine>> input = new Dictionary<int, List<VariableLine>>();
            Dictionary<int, List<VariableLine>> inputNull = null;
            List<VariableLine> variableLines = new List<VariableLine>();
            variableLines.Add(new VariableLine("a", 0));
            variableLines.Add(new VariableLine("b", 1));
            variableLines.Add(new VariableLine("_temp_123", 2));
            input.Add(0, variableLines);
            List<string> referencedVariables = new List<string>();
            referencedVariables.Add("a");
            referencedVariables.Add("b");

            Assert.AreEqual(referencedVariables, CodeBlockNode.GetReferencedVariables(input, false));
            referencedVariables.Add("_temp_123");
            Assert.AreEqual(referencedVariables, CodeBlockNode.GetReferencedVariables(input, true));

            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual(referencedVariables, CodeBlockNode.GetReferencedVariables(inputNull, true));
            });
        }

        [Test]
        public void TestGetAssignmentStatements()
        {
            // Create code block node with the following content. Note that blank
            // lines are intentional so that statement indices are not mixed with 
            // line indices (if we do, then these blank statements will cause issues 
            // and we will catch those issues).
            // 
            //      |                   |
            //      |    1 + 2;         |
            //      |                   |
            //      |    a = 1 + 2;     |
            //      |                   |
            //      |    b;             |
            //      |                   |
            //      |    c = b;         |
            //      |                   |
            //      |    d + e;         |
            //      |                   |
            //      |    f = d + e;     |
            //      |                   |
            // 
            string commands = @"
                CreateCodeBlockNode|d:15414.0|d:15141.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:\n1 + 2;\n\na = 1 + 2;\n\nb;\n\nc = b;\n\nd + e;\n\nf = d + e;\n|b:True";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            VisualNode node = controller.GetVisualNode(0x10000001) as VisualNode;
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node

            List<AssignmentStatement> assignments = new List<AssignmentStatement>();
            node.GetAssignmentStatements(assignments);

            Assert.AreEqual(6, assignments.Count); // Should have six assignments.

            // First assignment has no assignee (since it is a temp) and no referenced variables.
            Assert.AreEqual("", assignments[0].Assignee);
            Assert.AreEqual(0, assignments[0].References.Count);

            // Assignee = 'a', References = { }
            Assert.AreEqual("a", assignments[1].Assignee);
            Assert.AreEqual(0, assignments[1].References.Count);

            // Assignee = '', References = { b }
            Assert.AreEqual("", assignments[2].Assignee);
            Assert.AreEqual(1, assignments[2].References.Count);
            Assert.AreEqual("b", assignments[2].References[0]);

            // Assignee = 'c', References = { } ('b' is referenced once above).
            Assert.AreEqual("c", assignments[3].Assignee);
            Assert.AreEqual(0, assignments[3].References.Count);

            // Assignee = '', References = { d, e }
            Assert.AreEqual("", assignments[4].Assignee);
            Assert.AreEqual(2, assignments[4].References.Count);
            Assert.AreEqual("d", assignments[4].References[0]);
            Assert.AreEqual("e", assignments[4].References[1]);

            // Assignee = 'f', References = { } ('d', 'e' are both referenced before).
            Assert.AreEqual("f", assignments[5].Assignee);
            Assert.AreEqual(0, assignments[5].References.Count);
        }

        [Test]
        public void TestGetLineNumberFromOutputSlotIndex()
        {
            IGraphController graphController = new GraphController(null);
            List<VariableLine> outputLines = new List<VariableLine>();
            List<VariableLine> outputLinesNull = null;
            outputLines.Add(new VariableLine("a", 0));
            outputLines.Add(new VariableLine("b", 1));

            Assert.AreEqual(1, CodeBlockNode.GetLineNumberFromOutputSlotIndex(outputLines, 1));
            Assert.AreEqual(0, CodeBlockNode.GetLineNumberFromOutputSlotIndex(outputLines, 0));
            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual(1, CodeBlockNode.GetLineNumberFromOutputSlotIndex(outputLinesNull, 1));
            });
        }

        [Test]
        public void TestGetOutputSlotIndexFromLineNumber()
        {
            IGraphController graphController = new GraphController(null);
            List<VariableLine> outputLines = new List<VariableLine>();
            List<VariableLine> outputLinesNull = null;
            outputLines.Add(new VariableLine("a", 0));
            outputLines.Add(new VariableLine("b", 1));

            Assert.AreEqual(0, CodeBlockNode.GetOutputSlotIndexFromLineNumber(outputLines, 0));
            Assert.AreEqual(1, CodeBlockNode.GetOutputSlotIndexFromLineNumber(outputLines, 1));
            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual(1, CodeBlockNode.GetOutputSlotIndexFromLineNumber(outputLinesNull, 1));
            });
        }

        [Test]
        public void TestGetOutputSlotIndicesByName()
        {
            IGraphController graphController = new GraphController(null);
            List<VariableLine> outputLines = new List<VariableLine>();
            List<VariableLine> outputLinesNull = null;
            outputLines.Add(new VariableLine("a", 0));
            outputLines.Add(new VariableLine("b", 1));
            List<int> expected = new List<int>();
            expected.Add(1);

            Assert.AreEqual(expected, CodeBlockNode.GetOutputSlotIndicesByName(outputLines, "b"));
            Assert.Throws<NullReferenceException>(() =>
            {
                Assert.AreEqual(expected, CodeBlockNode.GetOutputSlotIndicesByName(outputLinesNull, "b"));
            });
        }

        [Test]
        public void TestGetOutputSlotNameFromOutputSlotIndex()
        {
            IGraphController graphController = new GraphController(null);
            List<VariableLine> outputLines = new List<VariableLine>();
            List<VariableLine> outputLinesNull = null;
            outputLines.Add(new VariableLine("a", 0));
            outputLines.Add(new VariableLine("b", 1));

            Assert.AreEqual("a", CodeBlockNode.GetOutputSlotNameFromOutputSlotIndex(outputLines, 0));
            Assert.AreEqual("b", CodeBlockNode.GetOutputSlotNameFromOutputSlotIndex(outputLines, 1));

            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual("a", CodeBlockNode.GetOutputSlotNameFromOutputSlotIndex(outputLinesNull, 0));
            });
        }

        [Test]
        public void TestGetDefinedVariables()
        {
            IGraphController graphController = new GraphController(null);
            List<VariableLine> outputLines = new List<VariableLine>();
            List<VariableLine> outputLinesNull = null;
            outputLines.Add(new VariableLine("a", 0));
            outputLines.Add(new VariableLine("b", 1));
            outputLines.Add(new VariableLine("_temp_123", 2));
            List<string> expected = new List<string>();
            expected.Add("a");
            expected.Add("b");

            Assert.AreEqual(expected, CodeBlockNode.GetDefinedVariables(outputLines, false));

            expected.Add("_temp_123");
            Assert.AreEqual(expected, CodeBlockNode.GetDefinedVariables(outputLines, true));

            Assert.Throws<ArgumentNullException>(() =>
            {
                Assert.AreEqual(expected, CodeBlockNode.GetDefinedVariables(outputLinesNull, true));
            });
        }

        [Test]
        public void TestConstructorFromSnapshotNode()
        {
            IGraphController graphController = new GraphController(null);
            uint id = 0x10000001;
            string content = "a = b;\nc = d;";
            GraphToDSCompiler.SnapshotNode snapshotNode = new GraphToDSCompiler.SnapshotNode(id, GraphToDSCompiler.SnapshotNodeType.CodeBlock, content);
            CodeBlockNode node = new CodeBlockNode(graphController, snapshotNode);

            Assert.IsNotNull(node);
            Assert.AreEqual(id, node.NodeId);
            Assert.AreEqual(content, node.Text);

            uint[] outputSlots = node.GetOutputSlots();
            uint[] inputSlots = node.GetInputSlots();
            Assert.AreEqual(2, outputSlots.Length);
            Assert.AreEqual(2, inputSlots.Length);
        }

        [Test]
        public void TestDeserilaizeOperationException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();
            IVisualNode node = new CodeBlockNode(graphController, "Double Click and Type");

            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', ' ', ' ', ' ');
            storage.WriteUnsignedInteger(signature, 21);
            storage.Seek(0, SeekOrigin.Begin);

            bool result = node.Deserialize(storage);
            Assert.AreEqual(result, false);
        }

        [Test]
        public void TestDeserializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            IVisualNode node = new CodeBlockNode(graphController, "Double Click and Type");

            Assert.Throws<ArgumentNullException>(() =>
            {
                node.Deserialize(storage);
            });
        }

        [Test]
        public void TestSerializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            IVisualNode node = new CodeBlockNode(graphController, "Double Click and Type");

            Assert.Throws<ArgumentNullException>(() =>
            {
                node.Serialize(storage);
            });
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();

            IVisualNode node1 = new CodeBlockNode(graphController, "Double Click and Type");
            IVisualNode node2 = new CodeBlockNode(graphController, "Double Click and Type");

            node1.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            node2.Deserialize(storage);

            Assert.AreEqual(NodeType.CodeBlock, node2.VisualType);
            Assert.AreEqual(node1.NodeId, node2.NodeId);
            Assert.AreEqual(true, ((CodeBlockNode)node2).Dirty);
            Assert.AreEqual(((CodeBlockNode)node1).Text, ((CodeBlockNode)node2).Text);
            Assert.AreEqual(((CodeBlockNode)node1).Caption, ((CodeBlockNode)node2).Caption);
            Assert.AreEqual(node1.X, node2.X);
            Assert.AreEqual(node1.Y, node2.Y);
        }

        [Test]
        public void TestCodeBlockNode00()
        {
            // Create a single code block node by typing on the canvas
            //
            string commands = @"
                CreateCodeBlockNode|d:545.0|d:206.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(1, controller.GetSlots().Count); // Check if slot count is only 1
        }

        [Test]
        public void TestCodeBlockNode01()
        {
            // Create 2 code block node by typing on the canvas
            //
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:472.0|d:165.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:543.0|d:265.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:200|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);

            Assert.AreEqual(2, controller.GetVisualNodes().Count); //Check if node count is 2           
            Assert.NotNull(new object[] { node01, node02 }); // Check if node is created
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType); // Check if node type is code block node
            Assert.AreEqual(NodeType.CodeBlock, node02.VisualType); // Check if node type is code block node
            Assert.AreEqual("100;", ((CodeBlockNode)node01).Text); // Check if text is 100
            Assert.AreEqual("200;", ((CodeBlockNode)node02).Text);// Check if text is 200
            Assert.AreEqual(2, controller.GetSlots().Count); // Check if slot count is 2
            Assert.AreEqual(null, node01.GetInputSlots()); // Check if input slot count is 0
            Assert.AreEqual(null, node02.GetInputSlots()); // Check if input slot count is 0 
        }

        [Test]
        public void TestCodeBlockNodeWith1000Characaters()
        {
            // Create a single code block node and type in 1000 characters
            //
            string commands = @"
                CreateCodeBlockNode|d:330.0|d:213.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:397.0|d:235.0
                EndNodeEdit|u:0x10000001|s:1234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123451234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789|b:True";

            //@TODO(Joy): Cap maximum display width
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(1001, ((VisualNode)node).Text.Count()); // Check if Text contain 1000 characters

            // The compiler will return an error due to the literal value being too 
            // large to parse, and thus the code block will not have any output slot.
            // 
            // "Error: The value is too big or too small to be converted to an integer"
            // 
            Assert.AreEqual(0, controller.GetSlots().Count);
        }

        [Test]
        public void TestCodeBlockNodeWithMixCharacters()
        {
            // Create a single code block node and type in alpha-numeric characters
            //
            string commands = @"
                CreateCodeBlockNode|d:426.0|d:227.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:494.0|d:249.0
                EndNodeEdit|u:0x10000001|s:sas121!@@#$$$|b:True";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual("sas121!@@#$$$;", ((VisualNode)node).Text); // Check if Text contain the alpha-numeric characters
            Assert.AreEqual(0, controller.GetSlots().Count); // Check if slot count is only 1
        }

        [Test]
        public void CreateCodeBlockWithMultipleLines()
        {
            // Create Code Block with mutiple lines
            //
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10432.0|d:10142.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;b = 2;c = a + b;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNodes().ElementAt(0);
            Assert.AreEqual(3, node.GetOutputSlots().Count());
        }

        [Test]
        public void TestOnDragingInvalidOutputConnectionFromCodeBlockCrash()
        {
            // Drag  and drop code block node from library.
            // Try to drag the output connection line from Code Block node.
            // DS Studio is crashing while there is no possible output connection from default Code Block node.
            // Defect Logged: http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1047

            string commands = @"
                CreateCodeBlockNode|d:404.0|d:117.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:531.0|d:129.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:726.0|d:266.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestEarlierTypedValuesRemainAsItIsInNewTextoxAsWell()
        {
            // Launch DS studio either through exe or using VS.
            // Click in canvas and start typing and create one code block node. 
            // Click in canvas again and start typing.
            // Earlier typed values are getting displayed in new text box.
            // Defect Logged: http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-966

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:524.0|d:239.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:123|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:566.0|d:357.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:456|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:693.0|d:246.0|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:789|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);
            IVisualNode node03 = controller.GetVisualNode(0x10000003);

            Assert.AreEqual("123;", ((VisualNode)node01).Text); // Check if Text contain the alpha-numeric characters
            Assert.AreEqual("456;", ((VisualNode)node02).Text); // Check if Text contain the alpha-numeric characters
            Assert.AreEqual("789;", ((VisualNode)node03).Text); // Check if Text contain the alpha-numeric characters
        }

        [Test]
        public void TestCodeBlockToCodeBlockConnection00()
        {
            // Create code block node with value of 100
            // Create code block node with value of 120
            // Try connecting them. It should not be able to connect.
            //
            string commands = @"
                CreateCodeBlockNode|d:389.0|d:164.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:456.0|d:250.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:120|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:424.0|d:176.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:459.0|d:262.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            VisualEdge edge = controller.GetVisualEdge(0x60000001);
            Assert.IsNull(edge);
        }

        [Test]
        public void CursorPositionShouldNotBeAtTheEndOfCodeBlockNodeWhenUserStartEditingIt()
        {
            //Create Code Block node and type multiple lines to create a code block node.
            //Now double click on the code block node by focusing cursor on the second line of code block node.
            //Once in edit mode, you will notice the cursor position at the end of code block node, which is not much helpful when code block node contains several lines and user wants to edit on particular line.
            //Defect Logged: http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1014, http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1084

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:425.0|d:180.75|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            string text = ((CodeBlockNode)node01).Text;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:abc=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            string text01 = ((CodeBlockNode)node01).Text;

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);
            Assert.AreEqual(text, ((CodeBlockNode)node01).Text); // Check if Text a=1;

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);
            Assert.AreEqual(text01, ((CodeBlockNode)node01).Text); // Check if Text is abc=1;
        }

        [Test]
        public void CodeBlockNodeWithNegativeDoubleValues()
        {
            //Create Code Block node with -3.3 is crashing.
            //
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:498.0|d:290.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:-3.3|b:True";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Ignore]
        public void L04TestLiteral_Delete_Createnew_click()
        {
            // Create Single literal node

            // @TODO(Monika): Please re-record this test case, due to the changes in 
            // mouse-down/begin-drag/end-drag/mouse-up sequence, these recorded commands are no longer valid.
            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:365.0|d:296.0|s:100
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                ClearSelection
                BeginDrag|d:419.0|d:320.0";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            /*IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node);// check if node create 
            Assert.AreEqual(NodeType.Literal, node.VisualType);// check if literal node
            Assert.AreEqual(1, controller.GetSlots().Count);//output slot. */
        }

        [Ignore]
        public void L06TestLiteral_move()
        {
            // Create Single literal node

            // @TODO(Monika): Please re-record this test case, due to the changes in 
            // mouse-down/begin-drag/end-drag/mouse-up sequence, these recorded commands are no longer valid.
            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:375.0|d:228.0|s:100
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|d:399.0|d:244.0
                EndDrag|d:535.0|d:235.0";

            GraphController controller = new GraphController(null);
            try
            {
                bool result = controller.RunCommands(commands);
                Assert.AreEqual(true, result);
            }
            catch
            {
                Console.WriteLine("defect id - ");
            }

            /*IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node);// check if node create 
            Assert.AreEqual(NodeType.Literal, node.VisualType);// check if literal node
            Assert.AreEqual(1, controller.GetSlots().Count);//output slot. */
        }

        [Test]
        public void T001_CreateCodeBlockUsingDoubleClickAndClickOnCanvasToFinishEditing()
        {
            // 1. Double click on canvas and write multi line code ( a = 1; b = a + 1; )
            // 2. Click on canvas to finish editing
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:560.0|d:240.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;\nb = a+2;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                ";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(560, node.X);
            Assert.AreEqual(240, node.Y);
            Assert.AreEqual(false, node.Selected);
            Assert.AreEqual("a = 1;\nb = a + 2;", node.Text);
        }

        [Test]
        public void T002_CreateCodeBlockUsingDoubleClickAndShiftEnterToFinishEditing()
        {
            // 1. Double click on canvas and write multi line code ( a = 1; b = a + 1; )
            // 2. Press Shift+Enter to finish editing 
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:531.0|d:189.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;\nb = a + 1;|b:True";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(531, node.X);
            Assert.AreEqual(189, node.Y);
            Assert.AreEqual(false, node.Selected);

            Assert.AreEqual("a = 1;\nb = a + 1;", node.Text);

        }

        [Test]
        public void Defect_CreateCodeBlockNodeWithoutSemicolon()
        {
            // double click in canvas and start typing. 
            // Type for ex a=1 and click in canvas to create code block.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10493.91641687|d:9682.42373976|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1|b:True";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual("a = 1;", ((CodeBlockNode)node01).Text); // Check if text is "a = 1"
        }

        [Test]
        public void Defect_IDE_1027()
        {
            // Crash if there is no semicolon at the end of line in code block.
            // double click n canvas and start typing. 
            // Type for ex 12345 and click in canvas to create code block.
            // After above step DS will crash because there is no semicolon at the end of line.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1027

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10342.0|d:10188.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:12345|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node

            Assert.AreEqual("12345;", ((CodeBlockNode)node01).Text); // Check if text is "12345"
        }

        [Test]
        public void Defect_IDE_1026()
        {
            // Extra line is getting added at the end of code block after finishing edit.
            // double click n canvas and start typing. 
            // Type for ex a=1; and click in canvas to create code block.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1026

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10449.0|d:10190.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node

            Assert.AreEqual("a = 1;", ((CodeBlockNode)node01).Text); // Check if text is "a = 1;"
        }

        [Test]
        public void Defect_IDE_1180()
        {
            // [CRASH] if first line is blank then dragging out connection line from second line results in crash.
            // Double click and then press enter to make first line of Code Block node blank, then on second line type a=1; and complete the node creation.
            // Now try to drag the output connection from second line.
            // After above operation it results in crash with below call stack.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1180

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10336.33333333|d:10220.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:\na=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10380.66666667|d:10233.33333333
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10496.0|d:10289.66666667
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node

            Assert.AreEqual("a = 1;", ((CodeBlockNode)node01).Text); // Check if text is "\na = 1;"
        }

        [Test]
        public void Defect_IDE_1221()
        {
            // [CRASH] while creating a Code Block node without value and first line blank.
            // Double click and and press enter to make first line blank and then click out side to complete the creation of Code Block node.
            // After above step DSS crashed. see the exception below.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1221

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10459.0|d:10239.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:\n|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node

        }

        [Test]
        public void Defect_IDE_1082()
        {
            // Create Code Block by double clicking in canvas with some text in it.
            // Now double click on Code Block for editing and remove all characters from Code Block and then click outside.
            // Above action will revert the deletion of characters and will end the editing command.
            // Now again double click in canvas to create new Code Block
            // At this stage user is not able to create new Code Block. (But if he clicks again in previous Code Block and without doing anything if he ends the editing then user is able to create new Code Block.)
            // http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1082

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10428.0|d:10185.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:abc|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10521.0|d:10197.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// CodeBlock Node

            Assert.IsNull(node01); // Check if node is null
            Assert.AreEqual("1;", ((CodeBlockNode)node02).Text); // Check if text is 1

        }

        [Test]
        public void T003_Defect_IDE_1215_ExtraSpaceAfterSemicolonShouldNotAddExtraSemicolon()
        {
            //1. Double click on canvas to create a code block node
            //2. Add test 'a= 1;' and then add an extra space after the semicolon : no extra semicolons should be added because of the extra space
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10504.0|d:10191.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1; |b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10504, node.X);
            Assert.AreEqual(10191, node.Y);
            Assert.AreEqual(false, node.Selected);
            Assert.AreEqual("a = 1;", node.Text);


        }

        [Test]
        public void T004_Defect_IDE_1215_ExtraSpacesAfterSemicolonShouldNotAddExtraSemicolon()
        {
            //1. Double click on canvas to create a code block node
            //2. Add test 'a= 1; b = a + 1' and then add 2 extra spaces after the last semicolon : no extra semicolons should be added because of the extra spaces
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10383.0|d:10169.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;\nb = a + 1  |b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10383, node.X);
            Assert.AreEqual(10169, node.Y);
            Assert.AreEqual(false, node.Selected);
            Assert.AreEqual("a = 1;\nb = a + 1;", node.Text);

        }

        [Test]
        public void Defect_IDE_1184()
        {
            // "output slot location is incorrect when first line is blank and Code Block contains numeric value."
            // Double click and press enter to make first line of Code Block blank and type 1 on second line and then again press enter and then click outside to create Code Block node.
            // After above step you will notice that numeric value is at second line but the output slot representation (three dots) is aligned to first line. (see attached image.)

            string commands = @"
                            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                            CreateCodeBlockNode|d:10363.0|d:10178.0|s:
                            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                            EndNodeEdit|u:0x10000001|s:\n1|b:True
                            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                            CreateIdentifierNode|d:10575.0|d:10240.0
                            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10385.0|d:10208.0
                            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10579.0|d:10255.0
                            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001); // Code Block Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002); // Identifier Node

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output 0x10000001
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000002); // Input 0x10000002
            Assert.IsNotNull(inputSlot);

            // Output slot 0x30000001 should connect to input slot 0x30000003.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            //Assert.AreEqual(0x30000002, connecting[0]);
            Assert.IsTrue(connecting.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count); //Check if node count is 2           
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType); // Check if node type is code block node
            Assert.AreEqual(NodeType.Identifier, node02.VisualType); // Check if node type is identifier node
        }

        [Test]
        public void Defect_IDE_1028()
        {
            // "Hit test is not done for the new code block with multiple output slots."
            // Create code block with multiple lines.
            // Check the output connections.
            // there is only one output connection is coming even there are multiple output slots.

            string commands = @"
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10396.0|d:10349.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1;\nb=2;\nc=a+b;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10588.0|d:10330.0
                CreateIdentifierNode|d:10579.5|d:10396.0
                CreateIdentifierNode|d:10590.5|d:10485.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10463.5|d:10361.5
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10590.5|d:10345.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10461.5|d:10378.5
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10585.5|d:10410.5
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:10462.0|d:10393.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10594.5|d:10499.5
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001); // Code Block Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002); // Identifier Node
            IVisualNode node03 = controller.GetVisualNode(0x10000003); // Identifier Node
            IVisualNode node04 = controller.GetVisualNode(0x10000004); // Identifier Node

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output 0x10000001
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000004); // Input 0x10000002
            Assert.IsNotNull(inputSlot);

            // Output slot 0x30000001 should connect to input slot 0x30000003.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000004));

            ISlot outputSlot01 = controller.GetSlot(0x30000002); // Output 0x10000001
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000006); // Input 0x10000002
            Assert.IsNotNull(inputSlot01);

            // Output slot 0x30000001 should connect to input slot 0x30000003.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000006));

            ISlot outputSlot02 = controller.GetSlot(0x30000003); // Output 0x10000001
            Assert.IsNotNull(outputSlot02);
            ISlot inputSlot02 = controller.GetSlot(0x30000008); // Input 0x10000002
            Assert.IsNotNull(inputSlot02);

            // Output slot 0x30000001 should connect to input slot 0x30000003.
            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting02);
            //Assert.AreEqual(0x30000002, connecting[0]);
            Assert.IsTrue(connecting02.Contains((uint)0x30000008));

            Assert.AreEqual(4, controller.GetVisualNodes().Count); //Check if node count is 2           
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType); // Check if node type is code block node
            Assert.AreEqual(NodeType.Identifier, node02.VisualType); // Check if node type is identifier node
            Assert.AreEqual(NodeType.Identifier, node03.VisualType); // Check if node type is identifier node
            Assert.AreEqual(NodeType.Identifier, node04.VisualType); // Check if node type is identifier node
        }

        [Test]
        public void Defect_IDE_1240()
        {
            // " Hit test is not done for the new code block with multiple output slots."
            // Create code block with multiple lines.
            // Check the output connections.
            // there is only one output connection is coming even there are multiple output slots.

            string commands = @"
                CreateCodeBlockNode|d:10395.0|d:10436.0|s:Double Click and type
                CreateDriverNode|d:10394.0|d:10515.33333333
                CreateIdentifierNode|d:10581.0|d:10356.0
                CreateFunctionNode|d:10609.66666667|d:10469.33333333|s:|s:Range|s:double,double,double
                CreateFunctionNode|d:10329.66666667|d:10353.33333333|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,North|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,North|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,South|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,South|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,North|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,North|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,South|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,South|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,North|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,North|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10773.0|d:10474.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10773.0|d:10474.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001); // Code Block Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002); // Driver Node
            IVisualNode node03 = controller.GetVisualNode(0x10000003); // Identifier Node
            IVisualNode node04 = controller.GetVisualNode(0x10000004); // Function Node
            IVisualNode node05 = controller.GetVisualNode(0x10000005); // Function Ndoe

            Assert.AreEqual(5, controller.GetVisualNodes().Count); //Check if node count is 2           
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType); // Check if node type is code block node
            Assert.AreEqual(NodeType.Driver, node02.VisualType); // Check if node type is driver node
            Assert.AreEqual(NodeType.Identifier, node03.VisualType); // Check if node type is identifier node
            Assert.AreEqual(NodeType.Function, node04.VisualType); // Check if node type is function node
            Assert.AreEqual(NodeType.Function, node05.VisualType); // Check if node type is function node
        }

        [Test]
        public void EmptyCodeBlockNodeTest_DoubleClickingInCanvas()
        {
            // Double click in canvas and start Code Block node.
            // Click outside Code Block node without typing anything.
            // Above action should remove the Code Block from canvas.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10496.0|d:10162.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count); //Check that there should not be any node in the canvas.           
        }

        [Test]
        public void EmptyCodeBlockNodeTest_DragAndDropFromLibrary()
        {
            // Drag and Drop Code Block node from Library.
            // Double click inside Code Block node to enable editing but do not type anything
            // Click outside of Code Block node.
            // Above action should remove the Code Block from canvas.

            string commands = @"
                CreateCodeBlockNode|d:10387.0|d:10185.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:Your code goes here|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count); //Check that there should not be any node in the canvas.           
        }

        [Test]
        public void EscKeyBehaviorTest_PressEscAfterEditingExistingCodeBlockNode()
        {
            // Double click in canvas and start Code Block node and type "a = 1;"
            // Double click inside Code Block node to enable editing and type bc after a so the new text will be "abc = 1;" (But dont comit the changes)
            // Press Esc key from key board.
            // Above action should set the Code Block node back to "a = 1;" and should not accept the changes those have been made while editing.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10422.0|d:10184.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            string text = ((CodeBlockNode)node01).Text;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:|b:False
                ClearSelection";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(1, controller.GetVisualNodes().Count); //Check that there should be one code Block node in canvas.
            Assert.AreEqual(text, ((CodeBlockNode)node01).Text);
        }

        [Test]
        public void T006_TestCodeBlockOutputNode_1()
        {
            // 1. double click on the canvas and create a code block node
            // 2. Add code : "a = 1;" => verify only one output slot is created
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10357.0|d:10143.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(1, node.GetOutputSlots().Count()); // Check if CBN output slot count is only 1
            Assert.AreEqual("a = 1;", node.Text); // Check CBN text
            Assert.AreEqual(10357, node.X);// Check the position of the node
            Assert.AreEqual(10143, node.Y);
        }

        [Test]
        public void T007_TestCodeBlockOutputNode_1()
        {
            // 1. drag a CBN from the library to the canvas 
            // 2. create a driver node a = 0;
            // 3. edit the CBN to have value "b = a" => verify only one output slot  is created            
            string commands = @"
                CreateDriverNode|d:10472.0|d:10168.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10551.0|d:10185.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10551.0|d:10185.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10659.0|d:10281.0|s:Double Click and type
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:Double Click and type|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10704.0|d:10297.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10704.0|d:10297.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b = a;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000002);
            Assert.AreEqual(10659, node.X);// verify position
            Assert.AreEqual(10281, node.Y);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(3, controller.GetSlots().Count); // Check total no of slots is 3
            Assert.AreEqual(1, node.GetOutputSlots().Count()); // Check if CBN output slot count is only 1
            Assert.AreEqual("b = a;", node.Text); // Check if slot count is only 1
        }

        [Test]
        public void T008_TestCodeBlockOutputNode_3()
        {
            // 1. create a driver node b = 1;
            // 2. drag a CBN from the library to the canvas 
            // 3. edit the CBN to have value "a = b; c = a; d = {0,1}" => verify 3 output slot  is created            

            string commands = @"
                CreateDriverNode|d:10581.0|d:10178.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10654.0|d:10190.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10654.0|d:10190.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:b|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10652.0|d:10193.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10652.0|d:10193.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10727.0|d:10278.0|s:Double Click and type
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:Double Click and type|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10807.0|d:10293.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10807.0|d:10293.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:a = b;\nc = a +b;\nd = {0,1};|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(10727, node.X);
            Assert.AreEqual(10278, node.Y);
            Assert.AreEqual(3, node.GetOutputSlots().Count()); // Check if CBN output slot count is 3
            Assert.AreEqual("a = b;\nc = a + b;\nd = { 0, 1 };", node.Text); // Check CBN text
            Assert.AreEqual(5, controller.GetSlots().Count); // Check total no of slots is 5
        }

        [Test]
        public void Defect_IDE_1329()
        {
            // Double clicking on node should start editing, even if user is already in edit mode of other node.
            // Create Driver Node.
            // Double click on caption to edit ( keep in edit mode)
            // Double click in canvas and start Code Block node.
            // Above action should start Code Block node and finish editing of Driver Node. 

            string commands = @"
                CreateDriverNode|d:10664.0|d:10375.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:Var1|b:True";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Driver Node
            string caption = ((DriverNode)node01).Caption; // Catch the default cation name;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10697.85714286|d:10470.0952381|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:1|b:True";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            IVisualNode node02 = controller.GetVisualNode(0x10000002);// Code Block Node

            Assert.AreEqual(caption, ((DriverNode)node01).Caption);
            Assert.AreEqual(2, controller.GetVisualNodes().Count); // Check if there are two nodes.
        }
        [Test]
        public void Defect_IDE_1020()
        {
            // Double Clicking inside (third slot) Range node starting code block node..
            // Create Range node.
            // Double click inside Range node (In third slot, left bottom corner) which will activate code block.

            string commands = @"
                CreateFunctionNode|d:10940.0952381|d:10572.57142857|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Range Node

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual("a", ((FunctionNode)node01).Text); // Check the name after change
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1051()
        {
            // Syntax error in CBN should not crash DSS. A warning message should be displayed instead.
            // Double click in canvas and type 1 and the press enter and type 2. Click anywhere in canvas to create Code Block.
            // DS Studio is crashing while Creating a Code Block with multiple lines with numeric values.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10443.21428571|d:10081.57619048|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1\n2|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Code Block Node

            Assert.AreEqual("1\n2;", ((CodeBlockNode)node01).Text); // Check the value of Code Block node.
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1244()
        {
            // Code Block with only spaces should be treated as empty Code Block..
            // Double click and start Code Block.
            // Add some spaces (only one should be enough)
            // Click out side to commit the Code Block creation.
            // After above step it will Create a Code Block with semicolon, which is not correct. Code Block with empty spaces should be treated as Empty code Block and the behavior should be same as empty Code Block. 

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10136.33333333|d:10179.26666667|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:     |b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1364()
        {
            // Connection is getting lost once variable is redefined and then renamed.
            // Create a Code Block with "a" and another with "b=1".
            // connect Code Block with b=1 to a.
            // Now create another Code Block with "a=1".
            // After above step there will be a error message for the Code Block with a=1.
            // Now change the a=1 to aa=1.
            // After above step you will notice that the connection between Code Block "b=1" and "a" is lost.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:9456.1|d:10145.66666667|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:9353.52857143|d:10152.23809524|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:9399.81428571|d:10167.0952381
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:9451.24285714|d:10161.95238095
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:9391.24285714|d:10239.66666667|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:a=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:aa = 1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000001));

            Assert.AreEqual(3, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1365()
        {
            // Error message is not coming once user redefined the variable after first error message.
            // Create Code Block with "a = 10".
            // Now create Code Block with "a = 20".
            // After above step error message will pop up for second Code Block.
            // Now edit the second Code Block with something line "a a = 20"
            // there is a space between both "a", so DSS will again pop up an error message.
            // Now again edit the Code Block to "a = 20".
            // After above step there is no output slot for "a = 20" and there is no error message for redefining the variable.

            string commands = @"
                CreateCodeBlockNode|d:15413.0|d:15107.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a=10|b:True
                CreateCodeBlockNode|d:15513.0|d:15148.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:a=20;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:a a = 20;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:a = 20;|b:True";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(2, controller.GetVisualNodes().Count);

            TestHelpers testHelper = new TestHelpers(controller);
            testHelper.EnsureNodeSlotInfo(0x10000001, null, new uint[] { 0x30000001 });
            testHelper.EnsureNodeSlotInfo(0x10000002, null, new uint[] { 0x30000003 });
            testHelper.EnsureSlotInfo(0x30000001, SlotType.Output, true, new uint[] { 0x10000001 }, null);
            testHelper.EnsureSlotInfo(0x30000003, SlotType.Output, true, new uint[] { 0x10000002 }, null);

            Assert.IsNullOrEmpty(controller.GetVisualNode(0x10000001).ErrorMessage);
            Assert.IsNotNullOrEmpty(controller.GetVisualNode(0x10000002).ErrorMessage);
        }

        [Test]
        public void Defect_IDE_1306()
        {
            // Error message is not coming once user redefined the variable after first error message.
            // Drag and Drop two CBN from Library and commit the second CBN by clicking in Canvas.
            // Now try to edit first CBN, DSS will crash on this step.(even after moving first code block node text will remain as it is on old location.)

            string commands = @"
                CreateCodeBlockNode|d:9997.33333333|d:10318.66666667|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:Your code goes here|b:True
                CreateCodeBlockNode|d:9974.33333333|d:10386.66666667|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }
        [Ignore]
        public void Test_CatchError()
        {
            // to demonstrate how to catch error mesg 

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10387.0|d:10189.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1\nb = 2;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
            IVisualNode Node = controller.GetVisualNodes()[0];
            Assert.AreEqual("';' is expected.", Node.ErrorMessage);
        }

        [Test]
        public void Defect_IDE_1493()
        {
            // IDE-1493 Output slot location mismatched for CBN with multi-lines

            // Create code block node with the following content to verify both 
            // the input and output line information:
            // 
            //      |    1 + 4;                  |
            //      |                            |
            //      |    a+ b;                   |
            //      |                            |
            //      |    { 1, 2, 3, 4, 5, 6 };   |
            //      |                            |
            //      |    1..20..2;               |
            // 
            string commands = @"
                CreateCodeBlockNode|d:15429.0|d:15131.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:1+4;\n\na+b;\n\n{1,2,3,4,5,6};\n\n1..20..2;|b:True";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            TestHelpers helper = new TestHelpers(controller);

            Dictionary<int, List<VariableLine>> inputLines = new Dictionary<int, List<VariableLine>>();
            inputLines.Add(1, new List<VariableLine>()); // Second statement.
            inputLines[1].Add(new VariableLine("a", 2)); // 'a' on the third line.
            inputLines[1].Add(new VariableLine("b", 2)); // 'b' on the third line.
            helper.EnsureInputLines(0x10000001, inputLines);

            List<VariableLine> outputLines = new List<VariableLine>();
            outputLines.Add(new VariableLine(0)); // Temporary on line 0.
            outputLines.Add(new VariableLine(2)); // Temporary on line 2.
            outputLines.Add(new VariableLine(4)); // Temporary on line 4.
            outputLines.Add(new VariableLine(6)); // Temporary on line 6.
            helper.EnsureOutputLines(0x10000001, outputLines);
        }

        [Test]
        public void Defect_IDE_1557()
        {
            // Create Two CBN and try to connect one to another (though it is not possible to connect two CBN.)
            // After above step you will notice that, the second CBN where you dropped connection line is been selected event there is no connection.

            string commands = @"
                CreateCodeBlockNode|d:29529.0|d:29710.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:29717.0|d:29780.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:29552.0|d:29721.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:29728.0|d:29794.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// CodeBlock Node

            Assert.IsFalse(((VisualNode)node01).Selected);
            Assert.IsFalse(((VisualNode)node02).Selected);

        }

        [Test]
        public void Defect_IDE_1592()
        {
            // [CRASH] while dragging a connection line from input slot of CBN just after creation.
            // Create a CBN with any value and just after committing the CB, drag a connection line from input slot and move it upward.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15607.0|d:15472.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15604.14285714|d:15475.76190476
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15606.42857143|d:15434.61904762
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1589()
        {
            // IDE-1589 Input slot connection is not persists if variable name changed.
            // Create a CBN with any value and just after committing the CB, drag a connection line from input slot and move it upward.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15544.0|d:15409.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15391.42857143|d:15401.85714286|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15406.35714286|d:15404.5
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15532.85714286|d:15417.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000001));
            Assert.AreEqual(2, controller.GetVisualNodes().Count);

            // Now converting normal value to simple expression.
            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:1+3;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            ISlot outputSlot01 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000001));
            Assert.AreEqual(2, controller.GetVisualNodes().Count);

            // Now assigning variable "b" to the expression.
            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b= 1 + 3;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            ISlot outputSlot02 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot02);
            ISlot inputSlot02 = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot02);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting02);
            Assert.IsTrue(connecting02.Contains((uint)0x30000001));
            Assert.AreEqual(2, controller.GetVisualNodes().Count);

            // Now converting expression to Range expression.
            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:1..10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            ISlot outputSlot03 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot03);
            ISlot inputSlot03 = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot03);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting03 = outputSlot03.ConnectingSlots;
            Assert.IsNotNull(connecting03);
            Assert.IsTrue(connecting03.Contains((uint)0x30000001));
            Assert.AreEqual(2, controller.GetVisualNodes().Count);

            // Now modifying Range expression.
            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:1..10..#4;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result04 = controller.RunCommands(commands);
            Assert.AreEqual(true, result04);

            ISlot outputSlot04 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot04);
            ISlot inputSlot04 = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot04);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting04 = outputSlot04.ConnectingSlots;
            Assert.IsNotNull(connecting04);
            Assert.IsTrue(connecting04.Contains((uint)0x30000001));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

            // Now converting single line CBN to multiline CBN
            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:1..10..#4;\nb=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result05 = controller.RunCommands(commands);
            Assert.AreEqual(true, result05);

            ISlot outputSlot05 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot05);
            ISlot inputSlot05 = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot05);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting05 = outputSlot05.ConnectingSlots;
            Assert.IsNotNull(connecting05);
            Assert.IsTrue(connecting05.Contains((uint)0x30000001));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1589_1()
        {
            // IDE-1589 Input slot connection is not persists if variable name changed.
            // Create a CBN with any value and just after committing the CB, drag a connection line from input slot and move it upward.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15712.0|d:15425.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15844.0|d:15445.4|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15729.6|d:15427.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15828.0|d:15447.8
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000002); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot);
            // Output slot of second CoedBlock Node should connect to input slot first Code Block node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1620()
        {
            // Create a CBN with below code -empty line between two statements.
            // a+b;  c;
            // a+b;  b=a;
            string commands = @"
                CreateCodeBlockNode|d:15356.0|d:15188.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a=1;\n\nb=2;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15452.0|d:15261.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:a=1;\n\nc;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

        }
        [Ignore]
        public void T44_Preview_1566()
        {
            // Error nodes must not have preview

            string commands = @"
                CreateCodeBlockNode|d:15400.0|d:15244.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a=1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Code Block Node
            Assert.AreEqual("1", node01.PreviewValue);
        }

        [Test]
        public void Defect_IDE_1755_1()
        {
            // Create a CBN with value 
            // a=10;
            // a=20;
            // Verify that there should be only one output slot for second line.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15457.0|d:15205.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=10;\na=20;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output of first line
            Assert.AreEqual(false, outputSlot01.States.HasFlag(SlotStates.Visible));
            ISlot outputSlot02 = controller.GetSlot(0x30000002); // Output of second line
            Assert.AreEqual(true, outputSlot02.States.HasFlag(SlotStates.Visible));
        }

        [Test]
        public void Defect_IDE_1755_2()
        {
            // Create a CBN with value 
            // a=10;
            // a=20;
            // Now delete the above created CBN
            // Verify that application should not crash.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15397.0|d:15226.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=10;\na=20;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1756()
        {
            // Create a CBN with value 
            // a=b;
            // c=b+d;
            // Now create another CBN with value : b=10;
            // Verify that application should not crash and there should be an implicit connection between above two CBN.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15352.0|d:15187.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=b;\nc=b+d;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15222.0|d:15189.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b=10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            Assert.AreEqual(5, controller.GetSlots().Count);

            //Only one implicit connection is formed.
            Assert.IsNotNull(controller.GetVisualEdge(0x60000001));
            Assert.IsNull(controller.GetVisualEdge(0x60000002));

            //Implicit connection should be formed between the first slot of the first code block node and output slot of second code block
            ISlot outputSlot = controller.GetSlot(0x30000005);
            ISlot inputSlot = controller.GetSlot(0x30000001);
            Assert.AreEqual(1, outputSlot.ConnectingSlots.Count());
            Assert.Contains(outputSlot.SlotId, inputSlot.ConnectingSlots);
            Assert.Contains(inputSlot.SlotId, outputSlot.ConnectingSlots);
        }

        [Test]
        public void Defect_IDE_1780()
        {
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15382.0|d:15121.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:floorHeight = 5;\nceilingHeight = 10;\n\nnumColsX = 5;\nnumColsY = 6;\nxSize = 10;\nySize = 15;\n\ncolRadius = 0.4;\nbeamRadius = 0.2;\n\nxHeight = 3;\nyHeight = 5;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15594.0|d:15254.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15595.0|d:15254.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15784.0|d:15131.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:ceilingPoints = { };\nxArray = (Math.Sin((0..180..#numColsX)) * xHeight);\nyArray = (Math.Sin((0..180..#numColsY)) * yHeight);\nzArray = xArray<1> + yArray<2> + ceilingHeight;\n\nceilingPoints = Point.ByCoordinates((0..xSize..#numColsX)<1>, (0..ySize..#numColsY)<2>, zArray<1><2>);\ncolumns = Cone.ByStartPointEndPointRadius(Point.ByCoordinates({ 0, xSize }<1>, { 0, ySize }<2>, floorHeight), Point.ByCoordinates({ 0, xSize }<1>, { 0, ySize }<2>, ceilingHeight), colRadius, colRadius).SetColor(Color.Red);\nbeamsY = Cone.ByStartPointEndPointRadius(transposedCeilingPoints[0..(Count(transposedCeilingPoints) - 2)], transposedCeilingPoints[1..(Count(transposedCeilingPoints) - 1)], beamRadius, beamRadius).SetColor(Color.Cyan);|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            Assert.AreEqual(28, controller.GetSlots().Count);

            //There should be a total of 10 implicit connection
            Assert.IsNotNull(controller.GetVisualEdge(0x60000001));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000002));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000003));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000004));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000005));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000006));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000007));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000008));
            Assert.IsNotNull(controller.GetVisualEdge(0x60000009));
            Assert.IsNotNull(controller.GetVisualEdge(0x6000000a));
            Assert.IsNull(controller.GetVisualEdge(0x6000000b));
        }

        [Test]
        public void ImplicitConnectionTest_1()
        {
            // Create a CBN : c=2;
            // Create CBN: a= b+c;
            // Create CBN: b = d*2+c;
            // Create CBN : d = e/2;
            // Create Driver Node which should look like after edit as e=2;

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15490.0|d:15343.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:c=2|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15606.66666667|d:15388.33333333|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:a=b+c|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15522.66666667|d:15460.33333333|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:b=d*2+c|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15459.33333333|d:15535.66666667|s:
                BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000004|s:d=e/2|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateDriverNode|d:15378.66666667|d:15463.66666667
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000005|s:e|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000005|s:4|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000003); // Input of second Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of first CoedBlock Node should have implicit connection to second Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000003));

            ISlot outputSlot02 = controller.GetSlot(0x30000001); // Output of first CodeBlock ndoe. (c=2)
            Assert.IsNotNull(outputSlot02);
            ISlot inputSlot02 = controller.GetSlot(0x30000006); // Input of third Code Block node. (b=d*2+c)
            Assert.IsNotNull(inputSlot02);
            // Output slot of first CoedBlock Node should have implicit connection to third Code Block node.
            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting02);
            Assert.IsTrue(connecting02.Contains((uint)0x30000006));

            ISlot outputSlot03 = controller.GetSlot(0x30000007); // Output of third CodeBlock ndoe. (b=d*2+c)
            Assert.IsNotNull(outputSlot03);
            ISlot inputSlot03 = controller.GetSlot(0x30000002); // Input of second Code Block node. (a=b+c)
            Assert.IsNotNull(inputSlot03);
            // Output slot of third CoedBlock Node should have implicit connection to second Code Block node.
            uint[] connecting03 = outputSlot03.ConnectingSlots;
            Assert.IsNotNull(connecting03);
            Assert.IsTrue(connecting03.Contains((uint)0x30000002));

            ISlot outputSlot04 = controller.GetSlot(0x30000009); // Output of fourth CodeBlock ndoe. (d=e/2)
            Assert.IsNotNull(outputSlot04);
            ISlot inputSlot04 = controller.GetSlot(0x30000005); // Input of second Code Block node. (b=d*2+c)
            Assert.IsNotNull(inputSlot04);
            // Output slot of fourth CoedBlock Node should have implicit connection to third Code Block node.
            uint[] connecting04 = outputSlot04.ConnectingSlots;
            Assert.IsNotNull(connecting04);
            Assert.IsTrue(connecting04.Contains((uint)0x30000005));

            ISlot outputSlot05 = controller.GetSlot(0x3000000a); // Output of Driver Node (e=4)
            Assert.IsNotNull(outputSlot05);
            ISlot inputSlot05 = controller.GetSlot(0x30000008); // Input of fourth Code Block node. (d = e/2)
            Assert.IsNotNull(inputSlot05);
            // Output slot of Drive node should have implicit connection to fourth Code Block node.
            uint[] connecting05 = outputSlot05.ConnectingSlots;
            Assert.IsNotNull(connecting05);
            Assert.IsTrue(connecting05.Contains((uint)0x30000008));

            Assert.AreEqual(5, controller.GetVisualNodes().Count);

        }

        [Test]
        public void ImplicitConnectionTest_2()
        {
            // Create a CBN with value 
            // a = 10;
            // a;
            // Verify that there is an implicit connection between above two CBN.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15419.0|d:15232.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=2;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15547.0|d:15327.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002); // Input of second Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of first CoedBlock Node should have implicit connection to second Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
        }

        [Test]
        public void ImplicitConnectionTest_3()
        {
            // Create a CBN : p1= Point.ByCoordinates(0,0,0);
            // Create a CBN : x= p1.X;
            // Verify that there is an implicit connection between above two CBN.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15405.0|d:15254.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:p1= Point.ByCoordinates(0,0,0);|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15533.9047619|d:15364.14285714|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:x= p1.X;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002); // Input of second Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of first CoedBlock Node should have implicit connection to second Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1791()
        {
            // Create a CBN : 1
            // Create a CBN : a
            // Create a CBN : c= a+ b;
            // Create a CBN : b=1;
            // Connect first two CBN
            // Verify that there is an implicit conenction between second and fourth CBN.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15339.0|d:15171.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15388.0|d:15213.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15333.0|d:15284.0|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:b=1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15447.0|d:15262.0|s:
                BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000004|s:c=a+b;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15333.33333333|d:15167.33333333
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15287.33333333|d:15158.66666667
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15309.33333333|d:15163.33333333
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15375.33333333|d:15216.66666667
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000004); // Output of third CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000006); // Input of fourth Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of third CoedBlock Node should have implicit connection to fourth Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000006));

            ISlot outputSlot02 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot02);
            ISlot inputSlot02 = controller.GetSlot(0x30000005); // Input of fourth Code Block node.
            Assert.IsNotNull(inputSlot02);
            // Output slot of second CoedBlock Node should have implicit connection to fourth Code Block node.
            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting02);
            Assert.IsTrue(connecting02.Contains((uint)0x30000005));

            Assert.AreEqual(4, controller.GetVisualNodes().Count);
        }
        [Test]
        public void Defect_IDE_1750()
        {
            // Create a CBN : {1,2}

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15341.0|d:15234.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a={1,2}|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);


        }
        [Test]
        public void Defect_IDE_1750_2()
        {
            // Create a CBN : {0}

            string commands = @"
                CreateCodeBlockNode|d:15381.0|d:15243.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:{0}|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);


        }
        [Test]
        public void Defect_IDE_1750_3()
        {
            // Create a CBN : a={0,1}
            // create b= a[0]

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15365.0|d:15241.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a={0,1}|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15396.0|d:15338.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b=a[0]|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);


        }
        [Test]
        public void Defect_IDE_1750_4()
        {
            // Create a CBN : a={0,1}
            // create b= {{1,2},{2,3}}
            // create a CBN c=b, d=a

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15426.0|d:15287.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a={1,2}|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15567.0|d:15288.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b={{1,2}{2,3}}|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b = { { 1, 2 },{ 2, 3 } };|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15497.0|d:15384.0|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:c=b;\nd=a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);


        }
        [Test]
        public void Defect_IDE_1750_5()
        {
            // Create a CBN : a=b={0,1}



            string commands = @"
               MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15363.0|d:15226.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=b={1,2}|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);


        }
        [Test]
        public void Defect_IDE_1750_6()
        {
            // Create a CBN : a=b={0,1}
            // create CBN c=b


            string commands = @"
          MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15363.0|d:15226.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=b={1,2}|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15379.0|d:15302.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:c=b|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);


        }

        [Test]
        public void Defect_IDE_1769()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15364.0|d:15119.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:count = 10|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15377.0|d:15282.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15376.0|d:15282.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15403.0|d:15237.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:0..10..#count\n0\n0\n|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000002);
            Assert.AreEqual(true, node.Error);


        }

        [Test]
        public void Defect_IDE_1686()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15434.0|d:15227.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:1+4;\na=123;\n;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void ImplicitConnectionTest_Defect_1849()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15411.0|d:15185.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15525.73333333|d:15245.06666667|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b=a;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15416.4|d:15307.73333333|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:a=12;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            DeleteComponents
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000004); // Output of third CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002); // Input of second Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of third CoedBlock Node should have implicit connection to second Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void ImplicitConnectionTest_4()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15342.0|d:15198.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15462.86666667|d:15264.73333333|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b=a|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15347.53333333|d:15326.06666667|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:a=12;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:ab = 10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000004); // Output of third CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002); // Input of second Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of third CoedBlock Node should have implicit connection to second Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            Assert.AreEqual(3, controller.GetVisualNodes().Count);

        }

        [Test]
        public void ImplicitConnectionTest_Defect_1850()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15394.0|d:15199.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:10|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15484.0|d:15273.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:a|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15599.0|d:15353.0|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:b=a;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15416.2|d:15197.2
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15468.2|d:15277.2
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000004); // Input of third Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of second CoedBlock Node should have implicit connection to third Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000004));

            Assert.AreEqual(3, controller.GetVisualNodes().Count);

        }

        [Test]
        public void ImplicitConnectionTest_Defect_1852()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15335.0|d:15203.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:1|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15432.0|d:15167.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:a|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15586.0|d:15162.0|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:c=a;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15437.0|d:15294.0|s:
            BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000004|s:b|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15565.0|d:15292.0|s:
            BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000005|s:d=b;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15353.0|d:15203.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15415.0|d:15169.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ClearSelection
            SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15418.0|d:15168.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15422.0|d:15299.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000007); // Output of fourth CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000008); // Input of fifth Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of fourth CoedBlock Node should have implicit connection to fifth Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000008));

            Assert.AreEqual(5, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1792()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1792

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15517.0|d:15232.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = b > 5 ? 1 : 2;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15374.0|d:15173.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b=4|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b = 1;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b = 7;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000003); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000001); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of second CoedBlock Node should have implicit connection to first Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000001));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1794()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1794

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15350.0|d:15154.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15491.0|d:15230.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b=1;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b = a;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15388.06666667|d:15293.06666667|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:a=180;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15403.4|d:15355.06666667
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15405.4|d:15355.73333333
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:c = 10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000004); // Output of second CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000003); // Input of first Code Block node.
            Assert.IsNotNull(inputSlot01);
            // Output slot of second CoedBlock Node should have implicit connection to first Code Block node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000003));

            Assert.AreEqual(3, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1860()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1860

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15422.0|d:15216.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:x+y;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15406.0|d:15323.66666667|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:p1 = Point.ByCoordinates(0,0,0);|b:True";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1264()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1264
            string commands = @"
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15362.0|d:15130.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:a = 10;\nb = 20;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15363.0|d:15259.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15363.0|d:15258.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15536.0|d:15204.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:c = a + b;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15408.0|d:15325.0|s:
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:a = c;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000003);
            Assert.AreEqual("'a' is already defined.\n", node.ErrorMessage);


        }

        [Test]
        public void Defect_IDE_1860_1()
        {
            // Create CBN: a= Math.Sin(123);
            // Edit CBN: a=Math.Sin(1
            //           23);
            // Edit back to step one.
            // Verify that there is no crash.

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15366.0|d:15238.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=Math.Sin(123);|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = Math.Sin(12\n3);|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = Math.Sin(123);|b:True
            ";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1780_1()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1780

            string commands = @"
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15542.0|d:15306.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:floorHeight = 5;\nceilingHeight = 10;\n\nnumColsX = 5;\nnumColsY = 6;\nxSize = 10;\nySize = 15;\n\ncolRadius = 0.4;\nbeamRadius = 0.2;\n\nxHeight = 3;\nyHeight = 5;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15802.85714286|d:15369.85714286|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:ceilingPoints = { };\nxArray = (Math.Sin((0..180..#numColsX)) * xHeight);\nyArray = (Math.Sin((0..180..#numColsY)) * yHeight);\nzArray = xArray<1> + yArray<2> + ceilingHeight;\n\nceilingPoints = Point.ByCoordinates((0..xSize..#numColsX)<1>, (0..ySize..#numColsY)<2>, zArray<1><2>);\ncolumns = Cone.ByStartPointEndPointRadius(Point.ByCoordinates({ 0, xSize }<1>, { 0, ySize }<2>, floorHeight), Point.ByCoordinates({ 0, xSize }<1>, { 0, ySize }<2>, ceilingHeight), colRadius, colRadius).SetColor(Color.Red);\nbeamsY = Cone.ByStartPointEndPointRadius(transposedCeilingPoints[0..(Count(transposedCeilingPoints) - 2)], transposedCeilingPoints[1..(Count(transposedCeilingPoints) - 1)], beamRadius, beamRadius).SetColor(Color.Cyan);|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15888.57142857|d:15453.85714286
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15910.28571429|d:15409.28571429
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000003);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x3000000b);
            Assert.IsNotNull(inputSlot01);
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x3000000b));

            ISlot outputSlot02 = controller.GetSlot(0x30000009);
            Assert.IsNotNull(outputSlot02);
            ISlot inputSlot02 = controller.GetSlot(0x3000000c);
            Assert.IsNotNull(inputSlot02);
            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting02);
            Assert.IsTrue(connecting02.Contains((uint)0x3000000c));

            ISlot outputSlot03 = controller.GetSlot(0x30000004);
            Assert.IsNotNull(outputSlot03);
            ISlot inputSlot03 = controller.GetSlot(0x3000000d);
            Assert.IsNotNull(inputSlot03);
            uint[] connecting03 = outputSlot03.ConnectingSlots;
            Assert.IsNotNull(connecting03);
            Assert.IsTrue(connecting03.Contains((uint)0x3000000d));

            ISlot outputSlot04 = controller.GetSlot(0x3000000a);
            Assert.IsNotNull(outputSlot04);
            ISlot inputSlot04 = controller.GetSlot(0x3000000e);
            Assert.IsNotNull(inputSlot04);
            uint[] connecting04 = outputSlot04.ConnectingSlots;
            Assert.IsNotNull(connecting04);
            Assert.IsTrue(connecting04.Contains((uint)0x3000000e));

            ISlot outputSlot05 = controller.GetSlot(0x30000002);
            Assert.IsNotNull(outputSlot05);
            ISlot inputSlot05 = controller.GetSlot(0x3000000f);
            Assert.IsNotNull(inputSlot05);
            uint[] connecting05 = outputSlot05.ConnectingSlots;
            Assert.IsNotNull(connecting05);
            Assert.IsTrue(connecting05.Contains((uint)0x3000000f));

            ISlot outputSlot06 = controller.GetSlot(0x30000005);
            Assert.IsNotNull(outputSlot06);
            ISlot inputSlot06 = controller.GetSlot(0x30000010);
            Assert.IsNotNull(inputSlot06);
            uint[] connecting06 = outputSlot06.ConnectingSlots;
            Assert.IsNotNull(connecting06);
            Assert.IsTrue(connecting06.Contains((uint)0x30000010));

            ISlot outputSlot07 = controller.GetSlot(0x30000006);
            Assert.IsNotNull(outputSlot07);
            ISlot inputSlot07 = controller.GetSlot(0x30000011);
            Assert.IsNotNull(inputSlot07);
            uint[] connecting07 = outputSlot07.ConnectingSlots;
            Assert.IsNotNull(connecting07);
            Assert.IsTrue(connecting07.Contains((uint)0x30000011));

            ISlot outputSlot08 = controller.GetSlot(0x30000001);
            Assert.IsNotNull(outputSlot08);
            ISlot inputSlot08 = controller.GetSlot(0x30000012);
            Assert.IsNotNull(inputSlot08);
            uint[] connecting08 = outputSlot08.ConnectingSlots;
            Assert.IsNotNull(connecting08);
            Assert.IsTrue(connecting08.Contains((uint)0x30000012));

            ISlot outputSlot09 = controller.GetSlot(0x30000007);
            Assert.IsNotNull(outputSlot09);
            ISlot inputSlot09 = controller.GetSlot(0x30000013);
            Assert.IsNotNull(inputSlot09);
            uint[] connecting09 = outputSlot09.ConnectingSlots;
            Assert.IsNotNull(connecting09);
            Assert.IsTrue(connecting09.Contains((uint)0x30000013));

            ISlot outputSlot10 = controller.GetSlot(0x30000008);
            Assert.IsNotNull(outputSlot10);
            ISlot inputSlot10 = controller.GetSlot(0x30000015);
            Assert.IsNotNull(inputSlot10);
            uint[] connecting10 = outputSlot10.ConnectingSlots;
            Assert.IsNotNull(connecting10);
            Assert.IsTrue(connecting10.Contains((uint)0x30000015));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1724()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15384.0|d:15214.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=1;\n\n1..4..1-2;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = 1;\n\n-1..-4..1 - 2;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output of first line
            Assert.AreEqual(true, outputSlot01.States.HasFlag(SlotStates.Visible));

            ISlot outputSlot02 = controller.GetSlot(0x30000001); // Output slot of third line.
            Assert.AreEqual(true, outputSlot02.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1724_1()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15515.0|d:15219.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a\n+\nb;\n\nc=10;|b:True";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1894()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15357.0|d:15147.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:x+y;|b:True";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1865_1()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15401.0|d:15220.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=Math.Cos(1\n2);|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1865_2()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15477.0|d:15252.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1895()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15405.0|d:15208.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a\n=\n1;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output of first line
            Assert.AreEqual(true, outputSlot01.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1893()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15359.0|d:15186.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=10;b=20;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output of first line
            Assert.AreEqual(true, outputSlot01.States.HasFlag(SlotStates.Visible));

            ISlot outputSlot02 = controller.GetSlot(0x30000002); // Output of second line
            Assert.AreEqual(true, outputSlot02.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1762()
        {
            // Create a CBN: [ a = { 1, 2, 3 }; ]
            // Create another CBN: [ c = a[0]; ]
            // Edit the first one to [ a = { 1, 2, 3 }; a = { 10, 20, 30 }; ]
            // 
            string commands = @"
                CreateCodeBlockNode|d:15385.0|d:15099.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a={1,2,3};|b:True
                CreateCodeBlockNode|d:15539.0|d:15162.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:c=a[0];|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = { 1, 2, 3 };\na={10,20,30};|b:True";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output slot of first line should not be visible
            Assert.AreEqual(false, outputSlot01.States.HasFlag(SlotStates.Visible));

            ISlot outputSlot02 = controller.GetSlot(0x30000004); // Output slot of second line should be visible
            Assert.AreEqual(true, outputSlot02.States.HasFlag(SlotStates.Visible));

            ISlot inputSlot01 = controller.GetSlot(0x30000002); // Input slot of second line
            Assert.IsNotNull(inputSlot01);

            // Checking whenter second slot is connected to second CBN. (Implicit connection test)
            uint[] connecting01 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1790()
        {

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15459.0|d:15247.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=Point.ByCoordinates(3,4,0);\na= a.Translate(10,0,0);|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001); // Output slot of first line should not be visible
            Assert.AreEqual(false, outputSlot01.States.HasFlag(SlotStates.Visible));

            ISlot outputSlot02 = controller.GetSlot(0x30000002); // Output slot of second line should be visible
            Assert.AreEqual(true, outputSlot02.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1317()
        {

            string commands = @"
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15314.0|d:15190.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15314.0|d:15189.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15314.0|d:15189.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:234\n|b:True
CreateFunctionNode|d:15520.0|d:15143.0|s:ProtoGeometry.dll|s:Arc.ByCenterPointRadiusAngle|s:Point,double,double,double,Vector
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15343.0|d:15193.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15448.0|d:15128.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, node.Selected);

            node = (VisualNode)controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, node.Selected);
        }

        [Test]
        public void Defect_IDE_1919()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1919

            string commands = @"
            CreateFunctionNode|d:15577.0|d:15237.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15417.0|d:15255.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:0|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15469.0|d:15355.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15468.0|d:15356.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15434.0|d:15251.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15528.0|d:15219.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15429.0|d:15257.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15525.0|d:15235.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15434.0|d:15252.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15535.0|d:15259.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15941.0|d:15182.0|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:arrayOfPoints = {start, start.Translate(10,0,0)};|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:start|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000004); // Output slot Point node.
            Assert.AreEqual(true, outputSlot01.States.HasFlag(SlotStates.Visible));

            ISlot inputSlot01 = controller.GetSlot(0x30000006); // input slot of second CBN
            Assert.AreEqual(false, inputSlot01.States.HasFlag(SlotStates.Visible));

            // Checking whenter Point node is connected to second CBN. (Implicit connection test)
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000006));

            Assert.AreEqual(3, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1910()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1910

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15354.0|d:15185.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:1|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15367.0|d:15260.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:2|b:True
            CreateFunctionNode|d:15558.0|d:15192.0|s:Special Nodes|s:+|s:double,double
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15367.0|d:15186.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None|d:15525.0|d:15183.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15383.0|d:15261.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15517.0|d:15206.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15506.0|d:15304.0|s:
            BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000004|s:a=Var3+4;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15552.0|d:15412.0|s:
            BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000005|s:b=a*Var3;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000005);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000006);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(false, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000006));

            ISlot outputSlot02 = controller.GetSlot(0x30000005);
            Assert.IsNotNull(outputSlot02);
            ISlot inputSlot02 = controller.GetSlot(0x30000009);
            Assert.IsNotNull(inputSlot02);
            Assert.AreEqual(false, inputSlot02.States.HasFlag(SlotStates.Visible));

            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting02);
            Assert.IsTrue(connecting02.Contains((uint)0x30000009));

            ISlot outputSlot03 = controller.GetSlot(0x30000007);
            Assert.IsNotNull(outputSlot03);
            ISlot inputSlot03 = controller.GetSlot(0x30000008);
            Assert.IsNotNull(inputSlot03);
            Assert.AreEqual(false, inputSlot03.States.HasFlag(SlotStates.Visible));

            uint[] connecting03 = outputSlot03.ConnectingSlots;
            Assert.IsNotNull(connecting03);
            Assert.IsTrue(connecting03.Contains((uint)0x30000008));

            Assert.AreEqual(5, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1350()
        {

            // verify no crash : as per comment in defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1350
            string commands = @"
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15366.0|d:15167.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:def func ( a)\n{\n    return = a+1;\n}\nb = func(1);|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:def func(a)\n{\nreturn = a + 1;\n}\nb  func(1);|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, node.Error);


        }

        [Test]
        public void Defect_IDE_1930()
        {
            // steps as per log file in defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1930 : verify no crash
            string commands = @"
CreateFunctionNode|d:15574.0|d:15118.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15471.0|d:15094.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:0|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15487.0|d:15097.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15531.0|d:15102.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
CreateDriverNode|d:15431.0|d:15157.0
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:32|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateIdentifierNode|d:15438.0|d:15197.0
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15363.0|d:15197.0|s:
BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000005|s:4|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15378.0|d:15198.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15423.0|d:15193.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15458.0|d:15201.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15539.0|d:15139.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15466.0|d:15159.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15537.0|d:15119.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual("Point", node.Caption);
            Assert.AreEqual(false, node.Error);

            node = (VisualNode)controller.GetVisualNode(0x10000005);
            Assert.AreEqual(false, node.Error);

            VisualEdge edge;

            edge = (VisualEdge)controller.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge);
            edge = (VisualEdge)controller.GetVisualEdge(0x60000002);
            Assert.IsNotNull(edge);
            edge = (VisualEdge)controller.GetVisualEdge(0x60000003);
            Assert.IsNotNull(edge);
            edge = (VisualEdge)controller.GetVisualEdge(0x60000004);
            Assert.IsNotNull(edge);
        }

        [Test]
        public void Defect_IDE_1870()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1870

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15346.0|d:15170.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:0;\n10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateFunctionNode|d:15455.85714286|d:15123.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
            CreateFunctionNode|d:15447.85714286|d:15272.71428571|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15365.57142857|d:15169.85714286
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15419.85714286|d:15107.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15365.57142857|d:15171.57142857
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15415.85714286|d:15125.85714286
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15369.0|d:15168.14285714
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15413.57142857|d:15147.57142857
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15367.28571429|d:15172.14285714
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15406.71428571|d:15271.57142857
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15367.85714286|d:15169.28571429
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15402.71428571|d:15290.42857143
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15367.85714286|d:15187.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None|d:15419.28571429|d:15262.42857143
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000002
            CreateSubRadialMenu|i:2001
            CreateSubRadialMenu|i:2001
            SelectMenuItem|i:2001|d:15543.28571429|d:15080.57142857|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15480.42857143|d:15294.85714286
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15531.85714286|d:15111.42857143
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15649.0|d:15209.14285714|s:
            BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000005|s:b=Var4>7?1:2;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x3000000d);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x3000000e);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(false, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x3000000e));


            Assert.AreEqual(5, controller.GetVisualNodes().Count);

        }
        [Test]
        public void Defect_IDE_1981()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1981

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15419.0|d:15198.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15399.0|d:15411.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15399.0|d:15410.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateFunctionNode|d:15610.0|d:15206.0|s:Special Nodes|s:+|s:double,double
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15442.0|d:15200.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15556.0|d:15200.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15442.0|d:15197.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15560.0|d:15220.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:20;\n10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000005);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(true, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            ISlot outputSlot02 = controller.GetSlot(0x30000005);
            Assert.IsNotNull(outputSlot02);
            ISlot inputSlot02 = controller.GetSlot(0x30000003);
            Assert.IsNotNull(inputSlot02);
            Assert.AreEqual(true, inputSlot02.States.HasFlag(SlotStates.Visible));

            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNotNull(connecting02);
            Assert.IsTrue(connecting02.Contains((uint)0x30000003));


            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }
        [Test]
        public void Defect_IDE_1885()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1885

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15490.0|d:15239.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:Your code goes here|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15401.0|d:15243.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:Your code goes here|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15440.0|d:15251.0|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:a=.01|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000003);
            Assert.IsNotNull(node);

        }

        [Test]
        public void Defect_IDE_1738()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1738

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15390.0|d:15208.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=1..10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15454.0|d:15280.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15458.0|d:15284.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15422.9|d:15272.6|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:a[0] = 9;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000002);
            Assert.AreEqual("'a' is already defined.\n", node.ErrorMessage);

        }

        [Test]
        public void Defect_IDE_2065()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2065

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15438.0|d:15195.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:\na=12+\nb;\n\nv.x.y=8+\n\nn+\nm;\n\nh=1+2;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(1, controller.GetVisualNodes().Count); // Check there should be one node only.
        }

        [Test]
        public void Defect_IDE_1984()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1984

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15445.0|d:15262.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=1;\nb=2;\nb=3;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateIdentifierNode|d:15489.85714286|d:15333.85714286
            CreateIdentifierNode|d:15527.57142857|d:15301.85714286
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15525.85714286|d:15306.14285714
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15579.0|d:15270.14285714
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15495.57142857|d:15337.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15571.57142857|d:15337.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000002
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15483.57142857|d:15261.14285714
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15551.57142857|d:15261.14285714
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15482.42857143|d:15294.28571429
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15544.14285714|d:15334.85714286
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = 1;\n\nb = 2;\nb = 3;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(3, controller.GetVisualNodes().Count); // Check if tehre are three nodes.

        }

        [Test]
        public void Defect_IDE_2165()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2165

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15419.0|d:15233.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:{1,2,3};|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(1, controller.GetVisualNodes().Count); // Check if tehre are three nodes.
            Assert.IsEmpty(node.ErrorMessage);


        }

        [Test]
        public void Defect_IDE_2178()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2165

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15424.0|d:15210.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a;b;c;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(1, controller.GetVisualNodes().Count); // Check if tehre are three nodes.

            // Input putput slots of first line
            ISlot inputSlot01 = controller.GetSlot(0x30000001);
            Assert.AreEqual(true, inputSlot01.States.HasFlag(SlotStates.Visible));
            ISlot outputSlot01 = controller.GetSlot(0x30000004);
            Assert.AreEqual(true, outputSlot01.States.HasFlag(SlotStates.Visible));

            // Input putput slots of second line
            ISlot inputSlot02 = controller.GetSlot(0x30000002);
            Assert.AreEqual(true, inputSlot02.States.HasFlag(SlotStates.Visible));
            ISlot outputSlot02 = controller.GetSlot(0x30000005);
            Assert.AreEqual(true, outputSlot02.States.HasFlag(SlotStates.Visible));

            // Input putput slots of third line
            ISlot inputSlot03 = controller.GetSlot(0x30000003);
            Assert.AreEqual(true, inputSlot03.States.HasFlag(SlotStates.Visible));
            ISlot outputSlot03 = controller.GetSlot(0x30000006);
            Assert.AreEqual(true, outputSlot03.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void CBNTestWithLastLineEmpty()
        {
            // Create CBN : a; and second line empty

            string commands = @"
            CreateCodeBlockNode|d:15475.0|d:15175.0|s:Your code goes here
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            EndNodeEdit|u:0x10000001|s:a;\n|b:True
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.AreEqual(2, controller.GetSlots().Count); // Check if slot count is only 1
            Assert.AreEqual("a;", ((CodeBlockNode)node).Text); // Check if text is 100
        }

        [Test]
        public void Defect_IDE_1826()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1826

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15612.0|d:15149.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=b;\nc=d+b;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15494.48571429|d:15139.42857143|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b=10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = c;\nc = d + b;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000005);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000001);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(false, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000001));

            ISlot inputSlot02 = controller.GetSlot(0x30000002);
            Assert.AreEqual(false, inputSlot02.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1591()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1591

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15438.0|d:15114.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15480.0|d:15259.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:a=1;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b = 1;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000003);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000001);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(true, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsFalse(connecting01.Contains((uint)0x30000001));

            ISlot outputSlot02 = controller.GetSlot(0x30000002);
            Assert.AreEqual(true, outputSlot02.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1827()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1827

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15410.0|d:15230.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a=b;\nc=d+b;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15311.17142857|d:15228.42857143|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b=10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15316.31428571|d:15229.57142857
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15316.31428571|d:15229.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:d = 10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15373.45714286|d:15333.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15373.45714286|d:15332.42857143
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000005);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(false, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            ISlot outputSlot02 = controller.GetSlot(0x30000001);
            Assert.AreEqual(false, outputSlot02.States.HasFlag(SlotStates.Visible));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_2191()
        {
            // steps as per defect : 
            //1. Create a CBN and paste the following code:
            /*p11 = Point.ByCoordinates((1..5)<1>, (1..5)<2>, 2.5).SetVisibility(false);
              p21 = Point.ByCoordinates((1..5)<1>, 2.5, (1..5)<2>).SetVisibility(false);
              surf2 = BSplineSurface.ByControlVertices(p21, 2, 2).SetVisibility(false);;
              surf1 = {
              BSplineSurface.ByControlVertices(p11, 2, 2).SetVisibility(false) => a1;
              a1.PointAtParameter(0.5, 0.5) => a2;
              a1.Trim(surf2, a2, false).SetColor(Color.Cyan); 
            };*/
            //2. Verify no crash -> only modifier block not supported error
            string commands = @"
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15387.0|d:15152.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:p11 = Point.ByCoordinates((1..5)<1>, (1..5)<2>, 2.5).SetVisibility(false);\np21 = Point.ByCoordinates((1..5)<1>, 2.5, (1..5)<2>).SetVisibility(false);\nsurf2 = BSplineSurface.ByControlVertices(p21, 2, 2).SetVisibility(false);;\nsurf1 = {\n        BSplineSurface.ByControlVertices(p11, 2, 2).SetVisibility(false) => a1;\n        a1.PointAtParameter(0.5, 0.5) => a2;\n        a1.Trim(surf2, a2, false).SetColor(Color.Cyan); \n};|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual("Modifier Blocks are not supported currently", node.ErrorMessage);

        }

        [Test]
        public void Defect_IDE_2075()
        {

            string commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15402.0|d:15167.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:1;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15411.0|d:15277.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:2;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15567.0|d:15203.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15567.0|d:15202.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15567.0|d:15202.0|s:
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:a;\nb|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x80000002
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15419.0|d:15172.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15553.0|d:15207.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15427.0|d:15280.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15558.0|d:15221.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15334.0|d:15117.0
SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15611.0|d:15343.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
ConvertSelectionToCode";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);






        }

        [Test]
        public void Defect_IDE_2182()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2182

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15465.0|d:15203.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a;\nb;\nc;\n123|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x80000002
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:#123;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_2297()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2297

            string commands = @"
            CreateFunctionNode|d:15566.0|d:15223.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15379.0|d:15223.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:0..10|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15412.0|d:15225.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None|d:15541.0|d:15202.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15414.0|d:15222.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None|d:15539.0|d:15222.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15414.0|d:15224.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,ReplicationGuide|i:2|e:System.Windows.Input.ModifierKeys,None|d:15558.0|d:15248.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,ReplicationGuide|i:2|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000001
            SelectMenuItem|i:1|d:15463.0|d:15184.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            SelectMenuItem|i:2|d:15459.0|d:15200.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1997()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1997

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15541.0|d:15148.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:10|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15630.0|d:15176.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:b|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000002
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15559.0|d:15146.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15625.0|d:15183.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15571.0|d:15248.42857143|s:
            BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000003|s:a=10;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:a;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);


            ISlot outputSlot01 = controller.GetSlot(0x30000004);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(false, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));


            ISlot outputSlot02 = controller.GetSlot(0x30000001);
            Assert.IsNotNull(outputSlot02);
            uint[] connecting02 = outputSlot02.ConnectingSlots;
            Assert.IsNull(connecting02);

            Assert.AreEqual(3, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_1984_1()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1984 (scenario mentioned by Ritesh)

            string commands = @"
            CreateCodeBlockNode|d:15412.0|d:15181.0|s:Your code goes here
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            EndNodeEdit|u:0x10000001|s:a={1,2,3};|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateIdentifierNode|d:15530.7|d:15185.03333333
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15431.2|d:15178.03333333
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15513.7|d:15187.03333333
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = { 1, 2, 3 };\na[0]=4;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x80000002
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:a = { 1, 2, 3 };\na[0] = b;|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000004);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(true, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_2284()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2284

            string commands = @"
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15533.0|d:15180.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:1|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15569.0|d:15249.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:a|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15548.0|d:15184.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15560.0|d:15257.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            UndoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            UndoOperation
            RedoOperation
            UndoOperation
            RedoOperation
            UndoOperation
            RedoOperation
            UndoOperation
            RedoOperation
            UndoOperation
            RedoOperation
            UndoOperation
            RedoOperation
            UndoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            RedoOperation
            RedoOperation
            RedoOperation
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot01 = controller.GetSlot(0x30000001);
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000002);
            Assert.IsNotNull(inputSlot01);
            Assert.AreEqual(true, inputSlot01.States.HasFlag(SlotStates.Visible));

            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000002));

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

        }

        [Test]
        public void Defect_IDE_2249()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2249
            // create 4 CBN nodes like this : 
            // 1. CBN : //comment;
            // 2. CBN : /* comment\ncomment2\ncomment3 */
            // 3. CBN : /* comment\n* ccc\ncomment */
            // 4. CBN : // comment1\na = 1;\n/* comment2\ncc\n*/\nb = a + 1\n//comment3;
            //=> and verify the final CBN values


            string commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15350.0|d:15171.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s://comment|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15349.0|d:15222.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:/*comment\ncomment2\ncomment3*/|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15549.0|d:15206.0|s:
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:/*comment\n*ccc\ncomment*/|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15484.0|d:15320.0|s:
BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000004|s:// comment1\na= 1;\n/*comment2\ncc\n*/\nb=a+1;\n//comment3;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(4, controller.GetVisualNodes().Count);
            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual("//comment;", node.Text);

            node = controller.GetVisualNode(0x10000002);
            Assert.AreEqual("/* comment\ncomment2\ncomment3 */", node.Text);

            node = controller.GetVisualNode(0x10000003);
            Assert.AreEqual("/* comment\n* ccc\ncomment */", node.Text);

            node = controller.GetVisualNode(0x10000004);
            Assert.AreEqual("// comment1\na = 1;\n/* comment2\ncc\n*/\nb = a + 1;", node.Text);
        }

        [Test]
        public void Defect_IDE_2239()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2239

            string commands = @"CreateFunctionNode|d:15582.0|d:15164.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                                CreateCodeBlockNode|d:15361.0|d:15180.0|s:
                                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                                EndNodeEdit|u:0x10000002|s:0|b:True
                                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);


            Assert.AreEqual(2, controller.GetVisualNodes().Count); // Check if tehre are two nodes.

            IVisualNode node = controller.GetVisualNode(0x10000002);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType); // Check if node type is code block node
            Assert.IsNullOrEmpty(node.ErrorMessage);

            node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType); // Check if node type is code block node
            Assert.IsNullOrEmpty(node.ErrorMessage);
        }

        [Test]
        public void Defect_IDE_2063()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2063
            // 1. CBN : 1
            // 2. CBN : a
            // 3. connect 1 to a
            // 4. CBN : 2
            // 5. CBN : b
            // 6. connet 2 to b
            // Select 1, a, edge between 1&a, edge between 2&b => CTRL_SHIFT+C => Verify edge between 2&b is still there

            string commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15368.0|d:15168.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:1|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15594.0|d:15170.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:a|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15406.0|d:15235.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15235.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15385.0|d:15168.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15590.0|d:15176.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15129.0|d:15258.0|s:
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:2|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15752.0|d:15268.0|s:
BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000004|s:b|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15282.38666667|d:15395.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15282.38666667|d:15391.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15146.38666667|d:15262.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15742.38666667|d:15270.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15321.38666667|d:15130.0
SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15667.38666667|d:15305.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
ConvertSelectionToCode";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);


            Assert.AreEqual(3, controller.GetVisualNodes().Count); // Check if tehre are two nodes.

            VisualEdge node = controller.GetVisualEdge(0x60000002);
            Assert.NotNull(node); // Check if edge is still there  
            Assert.AreEqual(EdgeType.ExplicitConnection, node.EdgeType);
            Assert.AreEqual(true, node.Selected);
        }

        [Test]
        public void Defect_IDE_2288()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2288
            string commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15401.0|d:15135.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:1|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15401.0|d:15202.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:2|b:True
CreateFunctionNode|d:15513.0|d:15159.0|s:Special Nodes|s:+|s:double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15416.0|d:15134.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15462.0|d:15151.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15416.0|d:15208.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15465.0|d:15168.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15370.0|d:15084.0
SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15607.0|d:15273.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
ConvertSelectionToCode
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);


            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            VisualNode node = controller.GetVisualNode(0x10000004);
            Assert.NotNull(node);
            Assert.AreEqual("Var4 = 1;\nVar4_1 = 2;\nVar3 = (Var4 + Var4_1);", node.Text);


        }


        [Test]
        public void Defect_IDE_2319()
        {
            // Steps are metioned in the defect. http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2319
            string commands = @"
            CreateFunctionNode|d:15428.0|d:15265.5|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,North|u:0x10000001
            CreateSubRadialMenu|i:3003
            CreateSubRadialMenu|i:3003
            CreateSubRadialMenu|i:3004
            SelectMenuItem|i:3004|d:15359.36939087|d:15261.7|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,North
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            ";


            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignScriptStudio.Graph.Core;
using GraphToDSCompiler;
using NUnit.Framework;

namespace DesignScriptStudio.Tests.UnitTests
{
    class TestHelpers
    {
        private GraphController controller = null;

        internal TestHelpers(GraphController controller)
        {
            this.controller = controller;
        }

        internal void EnsureNodeSlotInfo(uint nodeId, uint[] inputSlots, uint[] outputSlots)
        {
            VisualNode node = controller.GetVisualNode(nodeId);
            Assert.AreNotEqual(null, node);

            // Ensure input slot information is as expected.
            if (null == inputSlots || (inputSlots.Length <= 0))
                Assert.AreEqual(null, node.GetInputSlots());
            else
            {
                uint[] actualInputs = node.GetInputSlots();
                Assert.AreNotEqual(null, actualInputs);
                Assert.AreEqual(inputSlots.Length, actualInputs.Length);

                int index = 0;
                foreach (uint slot in inputSlots)
                {
                    // Ensure we have the expected input slots.
                    Assert.AreEqual(slot, actualInputs[index++]);
                }
            }


            // Ensure output slot information is as expected.
            if (null == outputSlots || (outputSlots.Length <= 0))
                Assert.AreEqual(null, node.GetOutputSlots());
            else
            {
                uint[] actualOutputs = node.GetOutputSlots();
                Assert.AreNotEqual(null, actualOutputs);
                Assert.AreEqual(outputSlots.Length, actualOutputs.Length);

                int index = 0;
                foreach (uint slot in outputSlots)
                {
                    // Ensure we have the expected output slots.
                    Assert.AreEqual(slot, actualOutputs[index++]);
                }
            }
        }

        internal void EnsureSlotInfo(uint slotId, SlotType slotType, bool visible, uint[] owners, uint[] connectingSlots)
        {
            Slot slot = controller.GetSlot(slotId) as Slot;
            Assert.AreNotEqual(null, slot); // Ensure we have the slot we need.
            Assert.AreEqual(slotType, slot.SlotType); // Ensure the slot is the right type.
            Assert.AreEqual(visible, slot.States.HasFlag(SlotStates.Visible)); // Ensure its visibility.

            // Owner list cannot be null or empty.
            int index = 0;
            uint[] actualOwners = slot.Owners;
            Assert.AreNotEqual(null, actualOwners);
            Assert.AreEqual(owners.Length, actualOwners.Length);

            foreach (uint owner in owners)
            {
                // Ensure we have the same owner at this index.
                Assert.AreEqual(owner, actualOwners[index++]);
            }

            // Connecting slots can be empty and null.
            if (null != connectingSlots && (connectingSlots.Length > 0))
            {
                uint[] actualConnectingSlots = slot.ConnectingSlots;
                Assert.AreNotEqual(null, actualConnectingSlots);
                Assert.AreEqual(connectingSlots.Length, actualConnectingSlots.Length);

                index = 0;
                foreach (uint connectingSlot in connectingSlots)
                {
                    // Ensure we have the same connecting slot at this index.
                    Assert.AreEqual(connectingSlot, actualConnectingSlots[index++]);
                }
            }
            else
            {
                // Make sure there's no connecting slot.
                Assert.AreEqual(null, slot.ConnectingSlots);
            }
        }

        internal void EnsureEdgeInfo(uint edgeId, uint startSlotId, uint endSlotId, EdgeType edgeType)
        {
            VisualEdge edge = controller.GetVisualEdge(edgeId);
            Assert.AreNotEqual(null, edge); // Make sure the edge exists.
            Assert.AreEqual(startSlotId, edge.StartSlotId); // Connecting from...
            Assert.AreEqual(endSlotId, edge.EndSlotId); // Connecting to...
            Assert.AreEqual(edgeType, edge.EdgeType); // Explicit or implicit?
        }

        internal void EnsureInputLines(uint codeBlockNodeId, Dictionary<int, List<VariableLine>> expectedInputs)
        {
            // Ensure we do have a valid node corresponding to the ID.
            VisualNode node = controller.GetVisualNode(codeBlockNodeId);
            Assert.AreNotEqual(null, node);

            // Make sure it is of type code block node.
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);
            CodeBlockNode codeBlockNode = node as CodeBlockNode;
            Assert.AreNotEqual(null, codeBlockNode);

            // The caller expects no output lines to be generated.
            if (null == expectedInputs || (expectedInputs.Count <= 0))
            {
                Assert.AreEqual(null, codeBlockNode.InputLines);
                return;
            }

            Dictionary<int, List<VariableLine>> actualInputs = codeBlockNode.InputLines;
            Assert.AreNotEqual(null, actualInputs);
            Assert.AreEqual(expectedInputs.Count, actualInputs.Count);

            foreach (KeyValuePair<int, List<VariableLine>> expectedInput in expectedInputs)
            {
                int statementIndex = expectedInput.Key; // Statement index.
                Assert.AreEqual(true, actualInputs.Keys.Contains(statementIndex));

                // Ensure that we have the right number of actual input lines.
                List<VariableLine> actualInput = actualInputs[statementIndex];
                Assert.AreNotEqual(null, actualInput);
                Assert.AreEqual(expectedInput.Value.Count, actualInput.Count);

                // Ensure that each of the input lines match up to the expectation.
                int lineCount = expectedInput.Value.Count;
                for (int index = 0; index < lineCount; ++index)
                {
                    VariableLine expected = expectedInput.Value[index];
                    VariableLine actual = actualInput[index];
                    Assert.AreEqual(expected.line, actual.line);

                    // If the expected variable isn't a temporary.
                    if (!string.IsNullOrEmpty(expected.variable))
                        Assert.AreEqual(expected.variable, actual.variable);
                }
            }
        }

        internal void EnsureOutputLines(uint codeBlockNodeId, List<VariableLine> expectedLines)
        {
            // Ensure we do have a valid node corresponding to the ID.
            VisualNode node = controller.GetVisualNode(codeBlockNodeId);
            Assert.AreNotEqual(null, node);

            // Make sure it is of type code block node.
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);
            CodeBlockNode codeBlockNode = node as CodeBlockNode;
            Assert.AreNotEqual(null, codeBlockNode);

            // The caller expects no output lines to be generated.
            if (null == expectedLines || (expectedLines.Count <= 0))
            {
                Assert.AreEqual(null, codeBlockNode.OutputLines);
                return;
            }

            List<VariableLine> actualLines = codeBlockNode.OutputLines;
            Assert.AreNotEqual(null, actualLines);
            Assert.AreEqual(expectedLines.Count, actualLines.Count);

            // Ensure that each of the output lines match up to the expectation.
            int lineCount = expectedLines.Count;
            for (int index = 0; index < lineCount; ++index)
            {
                VariableLine expected = expectedLines[index];
                VariableLine actual = actualLines[index];
                Assert.AreEqual(expected.line, actual.line);

                // If the expected variable isn't a temporary.
                if (!string.IsNullOrEmpty(expected.variable))
                    Assert.AreEqual(expected.variable, actual.variable);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    partial class GraphControllerTests
    {
        [Test]
        public void TestRemoveSlotOperationException()
        {
            GraphController graphController = new GraphController(null);

            Assert.Throws<InvalidOperationException>(() =>
            {
                graphController.RemoveSlot(0x60000001);
            });
        }

        [Test]
        public void TestRemoveVisualNodeOperationException()
        {
            GraphController graphController = new GraphController(null);

            Assert.Throws<InvalidOperationException>(() =>
            {
                graphController.RemoveVisualNode(0x10000001);
            });
        }

        [Test]
        public void TestRemoveNodeVisualDrawingOperationException()
        {
            GraphController graphController = new GraphController(null);

            Assert.Throws<InvalidOperationException>(() =>
            {
                graphController.RemoveDrawingVisual(0x10000001);
            });
        }

        [Test]
        public void TestSynchronizeToLifeRunnerOperationException()
        {
            GraphController graphController= new GraphController(null);

            Assert.Throws<InvalidOperationException>(() =>
            {
                DeltaNodes deltaNodes = new DeltaNodes();
                graphController.SynchronizeToLiveRunner(deltaNodes);
            });
        }
    }
}

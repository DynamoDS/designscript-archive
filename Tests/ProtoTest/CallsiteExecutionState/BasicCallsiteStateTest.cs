using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;

namespace ProtoTest.CallsiteState
{

    public class BasicTests
    {
        public TestFrameWork thisTest = new TestFrameWork();

        private ProtoCore.Core SetupTestCore(string csStateName)
        {
            ProtoCore.CallsiteExecutionState.filename = csStateName;
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
            ProtoCore.Core testCore = new ProtoCore.Core(new ProtoCore.Options());
            testCore.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(testCore));
            testCore.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(testCore));

            // this setting is to fix the random failure of replication test case
            testCore.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            testCore.Options.Verbose = true;
            testCore.EnableCallsiteExecutionState = true;

            return testCore;
        }

        private void RemoveTestCallsiteStateFile(string csStateName)
        {
            // TODO implement the path
            File.Delete(csStateName);
        }

        /// <summary>
        /// TestGetEntries will test only for the number of VMstate entries serialized in the current run
        /// Entries in the VMState must equal the number of lines of executable code with at least one function call
        /// </summary>
        /// 
        [Test]
        public void TestGetEntries_GlobalStatements01()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
p = Point.ByCoordinates(1, 1, 1);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements01");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ProtoLanguage.CompileStateTracker compileState = null;
            ExecutionMirror mirror = runner.Execute(code, core, out compileState);

            ProtoCore.CallsiteExecutionState vmstate = core.csExecutionState;
            int entries = vmstate.GetVMStateCount();
            Assert.IsTrue(entries == 1);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetEntries_GlobalStatements02()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
x = 1;
p = Point.ByCoordinates(x, 1, 1);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements02");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ProtoLanguage.CompileStateTracker compileState = null;
            ExecutionMirror mirror = runner.Execute(code, core, out compileState);

            ProtoCore.CallsiteExecutionState vmstate = core.csExecutionState;
            int entries = vmstate.GetVMStateCount();
            Assert.IsTrue(entries == 1);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetEntries_GlobalStatements03()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
x = 1;
y = 1;
z = 1;
p1 = Point.ByCoordinates(x, y, z);
p2 = Point.ByCoordinates(x, y, z);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements03");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ProtoLanguage.CompileStateTracker compileState = null;
            ExecutionMirror mirror = runner.Execute(code, core, out compileState);

            ProtoCore.CallsiteExecutionState vmstate = core.csExecutionState;
            int entries = vmstate.GetVMStateCount();
            Assert.IsTrue(entries == 2);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetEntries_GlobalStatements04()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
x = {1,2,3};
p = Point.ByCoordinates(x, 1, 1);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements04");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ProtoLanguage.CompileStateTracker compileState = null;
            ExecutionMirror mirror = runner.Execute(code, core, out compileState);

            ProtoCore.CallsiteExecutionState vmstate = core.csExecutionState;
            int entries = vmstate.GetVMStateCount();
            Assert.IsTrue(entries == 1);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }
    }
}

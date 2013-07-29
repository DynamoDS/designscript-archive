using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Automation
{
    public class AutomationResult
    {
        public string testName { get; set; }
        public List<AssertionResult> asserts { get; set; }

        public AutomationResult()
        {
        }

        public AutomationResult(string name, List<AssertionResult> Asserts)
        {
            testName = name;
            this.asserts = Asserts;
        }
    }
}

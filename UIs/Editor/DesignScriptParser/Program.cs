using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore;

namespace DesignScript.Parser
{
    class TestParser
    {
        static void Main()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            string filePath = @"C:\DSCode\autodeskresearch\branches\UIResearch\DesignScriptParser\DSParser\DSParser\scripts\test1.ds";
            DesignScript.Parser.Scanner s = new DesignScript.Parser.Scanner(filePath);
            DesignScript.Parser.Parser p = new DesignScript.Parser.Parser(s, core);
            p.Parse();
            DesignScript.Parser.Node c = p.root;
        }
    }
}

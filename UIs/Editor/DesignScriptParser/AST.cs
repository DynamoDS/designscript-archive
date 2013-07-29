using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Parser
{
    public abstract class Node
    {
        private static int sID = 0;

        public int Line { get; set; }
        public int Col { get; set; }
        public int ID
        {
            get;
            private set;
        }
        public Node()
        {
            ID = ++sID;
        }

        public string Name
        {
            get;
            set;
        }
    }

    public class CommentNode : Node
    {
        public enum CommentType { Inline, Block }
        public CommentType Type { get; private set; }
        public string Value { get; private set; }
        public CommentNode(int col, int line, string value, CommentType type)
        {
            Col = col;
            Line = line;
            Value = value;
            Type = type;
        }
    }
}

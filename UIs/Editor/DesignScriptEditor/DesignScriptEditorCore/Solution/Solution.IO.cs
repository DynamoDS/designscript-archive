using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Core
{
    using System.Xml.Serialization;
    using System.IO;
    using DesignScript.Parser;

    [XmlType("Script")]
    public class ScriptItem
    {
        [XmlAttribute]
        public bool EntryPoint { get; set; }
        [XmlAttribute]
        public string RelativePath { get; set; }
    }

    [XmlType("Reference")]
    public class ReferenceItem
    {
    }

    public class InlineMessageItem
    {
        public string FilePath { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public enum OutputMessageType { Info, Warning, Error, PossibleError, PossibleWarning, Invalid }
        public OutputMessageType Type { get; private set; }
        public void SetType(ProtoCore.OutputMessage.MessageType type)
        {
            switch (type)
            {
                case ProtoCore.OutputMessage.MessageType.Error:
                    Type = OutputMessageType.Error;
                    break;
                case ProtoCore.OutputMessage.MessageType.Warning:
                    Type = OutputMessageType.Warning;
                    break;
                case ProtoCore.OutputMessage.MessageType.Info:
                    Type = OutputMessageType.Info;
                    break;
                default: Type = OutputMessageType.Invalid;
                    break;
            }

        }
        public void SetType(OutputMessageType type)
        {
            switch (type)
            {
                case OutputMessageType.PossibleError:
                    Type = OutputMessageType.PossibleError;
                    break;
                case OutputMessageType.PossibleWarning:
                    Type = OutputMessageType.PossibleWarning;
                    break;
                case OutputMessageType.Error:
                    Type = OutputMessageType.Error;
                    break;
                case OutputMessageType.Warning:
                    Type = OutputMessageType.Warning;
                    break;
                default: Type = OutputMessageType.Invalid;
                    break;
            }
        }
    }

    [XmlType("Breakpoint")]
    public class BreakpointItem
    {
        [XmlIgnore]
        public string AbsolutePath { get; set; }

        [XmlAttribute]
        public string RelativePath { get; set; }
        [XmlAttribute]
        public int Line { get; set; }
        [XmlAttribute]
        public int Column { get; set; }
    }

    [XmlType("Expression")]
    public class ExpressionItem
    {
        [XmlAttribute]
        public string Content { get; set; }
    }

    [XmlRootAttribute("Solution", IsNullable = false)]
    public class SolutionData
    {
        private List<ScriptItem> scripts = null;
        private List<BreakpointItem> breakpoints = null;
        private List<ExpressionItem> watchExpressions = null;
        private List<InlineMessageItem> inlineMessages = null;

        #region Public Class Operational Methods

        // For use by deserializer.
        public SolutionData()
        {
        }

        // For use by Solution.
        internal SolutionData(Solution solution)
        {
            scripts = new List<ScriptItem>();
            breakpoints = new List<BreakpointItem>();
            watchExpressions = new List<ExpressionItem>();
            inlineMessages = new List<InlineMessageItem>();
        }

        #endregion

        #region Public Class Properties

        [XmlArrayAttribute("Scripts")]
        public List<ScriptItem> Scripts
        {
            get { return scripts; }
            set
            {
                if (null != scripts)
                {
                    string message = "Only XmlSerializer can set 'Scripts'!";
                    throw new InvalidOperationException(message);
                }

                scripts = value;
            }
        }

        [XmlArrayAttribute("Breakpoints")]
        public List<BreakpointItem> Breakpoints
        {
            get { return breakpoints; }
            set
            {
                if (null != breakpoints)
                {
                    string message = "Only XmlSerializer can set 'Breakpoints'!";
                    throw new InvalidOperationException(message);
                }

                breakpoints = value;
            }
        }

        [XmlArrayAttribute("WatchExpressions")]
        public List<ExpressionItem> WatchExpressions
        {
            get { return watchExpressions; }
            set
            {
                if (null != watchExpressions)
                {
                    string message = "Only XmlSerializer can set 'WatchExpressions'!";
                    throw new InvalidOperationException(message);
                }

                watchExpressions = value;
            }
        }

        [XmlIgnore]
        public List<InlineMessageItem> InlineMessages
        {
            get
            {
                // Since "inlineMessages" does not get serialized to a solution file, 
                // it will not be recreated by the de-serializer when a solution file 
                // is opened. Therefore, we need it to be created if it has not been.
                if (null == inlineMessages)
                    inlineMessages = new List<InlineMessageItem>();

                return inlineMessages;
            }
        }

        #endregion
    }
}

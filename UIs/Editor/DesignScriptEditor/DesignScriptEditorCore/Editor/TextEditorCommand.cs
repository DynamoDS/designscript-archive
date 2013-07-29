﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DesignScript.Editor.Core
{
    public class CommandAssert
    {
        [XmlIgnore]
        public bool Passed { get; set; }

        [XmlAttribute]
        public int AssertNumber { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }

        private static int assertCounter = 1;

        public CommandAssert()
        {
            Passed = true;
        }

        public CommandAssert(string name, string value)
        {
            AssertNumber = assertCounter++;
            PropertyName = name;
            PropertyValue = value;
        }
    }

    public class TextEditorCommand
    {
        #region Public Class Configurations

        public enum Modifier : uint
        {
            None = 0x00000000,
            Shift = 0x00000001,
            Control = 0x00000002,
            Alt = 0x00000004,
            LeftButtonDown = 0x00000008,
            MiddleButtonDown = 0x00000010,
            RightButtonDown = 0x00000020
        }

        public enum Method
        {
            Invalid,
            CreateNewScript,
            LoadScriptFromFile,
            SaveScriptToFile,
            CloseScript,
            ChangeScript,
            SetCursorPosition,
            SetMouseDownPosition,
            SetMouseUpPosition,
            SetMouseMovePosition,
            ClearDragDropState,
            SelectFragment,
            SelectLines,
            CommentLines,
            DeleteCurrentLine,
            InsertText,
            SetPageSize,
            DoNavigation,           // Arrows, page up/down, etc.
            DoControlCharacter,     // Delete, backspace.
            DoCopyText,
            DoPasteText,
            SelectAllText,
            SelectPartial,
            MoveSelectedText,
            UndoEditing,
            RedoEditing,
            Run,
            Step,
            Stop,
            ToggleBreakpoint,
            FormatDocument,
            FindReplace,
        }

        #endregion

        #region Private Class Data Members

        private List<object> arguments = null;
        private uint modifiers = ((uint)Modifier.None);

        #endregion

        #region Public Class Operational Methods

        public TextEditorCommand()
        {
        }

        public TextEditorCommand(Method methodName)
        {
            this.MethodName = methodName;
        }

        public void AppendArgument(object argument)
        {
            if (null == this.arguments)
                this.arguments = new List<object>();

            this.arguments.Add(argument);
        }

        public void AppendAsserts(List<CommandAssert> asserts)
        {
            if (null == this.Asserts)
                this.Asserts = new List<CommandAssert>();
            if (asserts != null)
                this.Asserts.AddRange(asserts);
        }

        #endregion

        #region Public Class Properties

        public bool IsControlKeyDown
        {
            get { return ((modifiers & ((uint)Modifier.Control)) != 0); }
        }

        public bool IsShiftKeyDown
        {
            get { return ((modifiers & ((uint)Modifier.Shift)) != 0); }
        }

        public bool IsAltKeyDown
        {
            get { return ((modifiers & ((uint)Modifier.Alt)) != 0); }
        }

        public bool IsLeftButtonDown
        {
            get { return ((modifiers & ((uint)Modifier.LeftButtonDown)) != 0); }
        }

        public bool IsMiddleButtonDown
        {
            get { return ((modifiers & ((uint)Modifier.MiddleButtonDown)) != 0); }
        }

        public bool IsRightButtonDown
        {
            get { return ((modifiers & ((uint)Modifier.RightButtonDown)) != 0); }
        }

        [XmlAttribute]
        public int CommandNumber { get; set; }

        [XmlAttribute]
        public long IntervalTime { get; set; }

        [XmlElement]
        public uint Modifiers
        {
            get { return modifiers; }
            set { modifiers = value; }
        }

        [XmlElement]
        public Method MethodName { get; set; }

        [XmlArray]
        public object[] Arguments
        {
            get
            {
                if (this.arguments != null)
                    return this.arguments.ToArray();
                else
                    return null;
            }

            set
            {
                if (null != this.arguments)
                    throw new InvalidOperationException();

                this.arguments = new List<object>();
                this.arguments.AddRange(value);
            }
        }

        [XmlArray]
        public List<CommandAssert> Asserts { get; set; }

        #endregion
    }

    [Serializable()]
    [XmlRootAttribute("Commands")]
    public class Commands
    {
        [XmlAttribute("EnableSmartFormatting")]
        public bool EnableSmartFormatting { get; set; }

        [XmlElement("TextEditorCommand")]
        public List<TextEditorCommand> TextEditorCommands { get; set; }
    }
}

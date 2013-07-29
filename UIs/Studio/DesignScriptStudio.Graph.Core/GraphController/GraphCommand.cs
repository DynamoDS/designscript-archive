using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Input;

namespace DesignScriptStudio.Graph.Core
{
    class GraphCommand
    {
        internal enum Name
        {
            None,
            CreateIdentifierNode,
            CreateDriverNode,
            CreateCodeBlockNode,
            CreateFunctionNode,
            CreateRenderNode,
            CreatePropertyNode,
            SaveGraph,
            MouseDown,
            MouseUp,
            BeginDrag,
            EndDrag,
            BeginNodeEdit,
            EndNodeEdit,
            BeginHighFrequencyUpdate,
            EndHighFrequencyUpdate,
            SelectComponent,
            ClearSelection,
            UndoOperation,
            RedoOperation,
            DeleteComponents,
            AddReplicationGuide,
            RemoveReplicationGuide,
            EditReplicationGuide,
            SetReplicationGuide,
            SelectMenuItem,
            CreateRadialMenu,
            CreateSubRadialMenu,
            ImportScript,
            ConvertSelectionToCode,
            TogglePreview
        }

        const char textDelimiter = '|';
        List<object> arguments = null;

        #region Public Class Operational Methods

        internal GraphCommand(Name commandName)
        {
            this.CommandName = commandName;
        }

        internal static GraphCommand FromString(string content)
        {
            if (String.IsNullOrEmpty(content))
                throw new ArgumentException("Invalid argument", "content");

            content = content.Trim();
            if (String.IsNullOrEmpty(content))
                throw new ArgumentException("Invalid argument", "content");

            char[] delimiter = new char[] { textDelimiter };
            string[] inputs = content.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            if (null == inputs || (inputs.Length <= 0))
                return null;

            Type enumType = typeof(GraphCommand.Name);
            object commandName = Enum.Parse(enumType, inputs[0]);
            GraphCommand command = new GraphCommand((Name)commandName);

            int inputIndex = 0;
            foreach (string input in inputs)
            {
                // Skip past the command name.
                if (inputIndex++ < 1)
                    continue;

                if (String.IsNullOrEmpty(input))
                    throw new ArgumentException("Invalid argument", "content");
                if ((input.Length < 2) || (input[1] != ':'))
                    throw new ArgumentException("Invalid argument", "content");

                int intValue = 0;
                bool boolValue = false;
                double doubleValue = 0.0;

                string value = input.Substring(2); // Skip past the "x:" portion.

                switch (input[0])
                {
                    case 'b':
                        if (!Boolean.TryParse(value, out boolValue))
                            throw new InvalidCastException("Invalid 'bool' value");
                        command.AppendArgument(boolValue);
                        break;
                    case 'd':
                        if (!Double.TryParse(value, out doubleValue))
                            throw new InvalidCastException("Invalid 'double' value");
                        command.AppendArgument(doubleValue);
                        break;
                    case 'e':
                        int commaIndex = value.IndexOf(',');
                        if (-1 == commaIndex)
                            throw new ArgumentException("Invalid argument", "content");

                        string fullTypeName = value.Substring(0, commaIndex);
                        string enumValue = value.Substring(commaIndex + 1); // Skip ','.
                        System.Type type = GraphCommand.GetEnumType(fullTypeName);
                        command.AppendArgument(Enum.Parse(type, enumValue));
                        break;
                    case 'i':
                        if (!Int32.TryParse(value, out intValue))
                            throw new InvalidCastException("Invalid 'int' value");
                        command.AppendArgument(intValue);
                        break;
                    case 's':
                        string transformed = value.Replace("\\n", "\n");
                        command.AppendArgument(transformed);
                        break;
                    case 'u':
                        if (value.StartsWith("0x") || value.StartsWith("0X"))
                            value = value.Substring(2);

                        uint unsignedValue = UInt32.Parse(value, NumberStyles.HexNumber);
                        command.AppendArgument(unsignedValue);
                        break;
                }
            }

            return command;
        }

        internal void AppendArgument(object argument)
        {
            if (null == argument)
                throw new ArgumentNullException("argument");

            if (null == arguments)
                arguments = new List<object>();

            arguments.Add(argument);
        }

        internal object GetArgument(int index)
        {
            if (null == arguments)
                throw new InvalidOperationException();

            // Relying on indexer to throw out-of-range exception.
            return arguments[index];
        }

        #endregion

        #region Overridable Methods

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(CommandName.ToString());

            if (null != arguments && (arguments.Count > 0))
            {
                foreach (object argument in arguments)
                {
                    builder.Append(textDelimiter.ToString());
                    switch (argument.GetType().ToString())
                    {
                        case "System.Boolean":
                            builder.Append("b:" + argument.ToString());
                            break;
                        case "System.Double":
                            double value = ((double)argument);
                            builder.Append("d:" + value.ToString("0.0#######"));
                            break;
                        case "System.Int32":
                            builder.Append("i:" + argument.ToString());
                            break;
                        case "System.String":
                            string transformed = argument as string;
                            // @TODO(Zx): We should never have to worry about '\r'.
                            transformed = transformed.Replace("\r\n", "\\n");
                            transformed = transformed.Replace("\n\r", "\\n");
                            transformed = transformed.Replace("\n", "\\n");
                            builder.Append("s:" + transformed);
                            break;
                        case "System.UInt32":
                            uint unsignedValue = ((uint)argument);
                            builder.Append("u:0x" + unsignedValue.ToString("x8"));
                            break;

                        default:
                            {
                                if (argument is System.Enum)
                                {
                                    builder.Append(string.Format("e:{0},{1}",
                                        argument.GetType().FullName, argument.ToString()));
                                    break;
                                }

                                throw new InvalidOperationException("Unhandled argument type");
                            }
                    }
                }
            }

            return builder.ToString();
        }

        #endregion

        #region Internal Class Properties

        internal Name CommandName { get; private set; }

        #endregion

        #region Private Class Helper Methods

        private static System.Type GetEnumType(string fullEnumName)
        {
            string[] assemblyNames = 
            {
                "",
                ", WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                ", PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
            };

            foreach (string assemblyName in assemblyNames)
            {
                string fullyQualifiedEnumName = fullEnumName + assemblyName;
                System.Type type = Type.GetType(fullyQualifiedEnumName);
                if (null != type)
                    return type;
            }

            return null;
        }

        #endregion
    }
}

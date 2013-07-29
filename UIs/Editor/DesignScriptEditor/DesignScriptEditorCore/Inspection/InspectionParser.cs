using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore;

namespace DesignScript.Editor.Core
{
    public class InspectionParser
    {
        bool needsParsing;
        List<string> commands;

        public bool IsValid { get; private set; }

        public InspectionParser(string variable)
        {
            commands = new List<string>();
            Parse(variable);
        }

        public bool NeedsParsing()
        {
            return needsParsing;
        }

        private void Parse(string variable)
        {
            if (variable.Contains("[") || variable.Contains(".")|| variable.Contains(","))
            {
                needsParsing = true;
            }
            else
            {
                needsParsing = false;
                return;
            }

            string prevVariable = string.Empty;
            int arrayIndex = 0;
            string classProperty = string.Empty;
            int limitValue = 0;
            bool valid = true;

            bool isArray = false;
            bool isClass = false;
            bool isLimit = false;

            foreach (char c in variable)
            {
                if (c == '[')
                {
                    bool classProp = false;
                    if (isClass)
                    {
                        commands.Add("." + classProperty);
                        classProperty = string.Empty;
                        isClass = false;
                        classProp = true;
                    }
                    isArray = true;
                    if (prevVariable != string.Empty && !classProp)
                        commands.Add(prevVariable);
                    prevVariable = string.Empty;
                    continue;
                }
                else if (c == ']')
                {
                    isArray = false;
                    commands.Add("[" + arrayIndex + "]");
                    arrayIndex = 0;
                    prevVariable = string.Empty;
                    continue;
                }
                else if (c == '.')
                {
                    bool classProp = false;
                    if (isClass)
                    {
                        commands.Add("." + classProperty);
                        classProperty = string.Empty;
                        isClass = false;
                        classProp = true;
                    }
                    if (prevVariable != string.Empty && !classProp)
                        commands.Add(prevVariable);
                    isClass = true;
                    prevVariable = string.Empty;
                    continue;
                }
                else if (c == ',')
                {
                    bool classProp = false;
                    if (isClass)
                    {
                        commands.Add("." + classProperty);
                        classProperty = string.Empty;
                        isClass = false;
                        classProp = true;
                    }
                    if (prevVariable != string.Empty && !classProp)
                        commands.Add(prevVariable);
                    isLimit = true;
                    prevVariable = string.Empty;
                    continue;
                }

                else if (isArray == true)
                {
                    if (Char.IsDigit(c))
                    {
                        arrayIndex = (arrayIndex * 10) + Convert.ToInt32(Char.GetNumericValue(c));
                    }
                    else
                    {
                        valid = false;
                    }
                    continue;
                }
                else if (isClass == true)
                {
                    classProperty += c;
                }
                else if (isLimit == true)
                {
                    if (Char.IsDigit(c))
                    {
                        limitValue = (limitValue * 10) + Convert.ToInt32(Char.GetNumericValue(c));
                    }
                    else
                    {
                        valid = false;
                    }
                    continue;
                }

                prevVariable += c;
            }

            if (isArray)
            {
                valid = false;
            }

            if (isClass)
            {
                commands.Add("." + classProperty);
                if (classProperty == string.Empty)
                    valid = false;
                classProperty = string.Empty;
                isClass = false;
            }
            else if (isLimit)
            {
                commands.Add("," + limitValue);
                limitValue = 0;
                isLimit = false;
            }

            if (!valid || Char.IsDigit(commands[0][0]) || commands[0].Contains("[") || commands[0].Contains(".") || commands[0].Contains(","))
            {
                IsValid = false;
            }
            else
            {
                IsValid = true;
            }
        }

        public List<string> GetParsedCommands()
        {
            return commands;
        }
    }
}

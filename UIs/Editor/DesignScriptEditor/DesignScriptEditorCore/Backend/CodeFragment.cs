using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Core
{
    using DesignScript.Parser;
    using System.Collections;
    using DesignScript.Parser.Associative;
    using System.Windows.Media;

    class LineComparer : IComparer<CodeFragment>
    {
        public int Compare(CodeFragment x, CodeFragment y)
        {
            return (x.Line - y.Line);
        }
    }

    // @TODO Ajit to make this a struct
    public class CodeFragment : IComparable
    {
        public enum Type
        {
            None,
            Keyword,
            Function,
            Comment,
            UserType,
            Argument,
            Local,
            Punctuation,
            Text,
            Number
        };

        internal static System.Windows.Media.Color[] colorTable = 
        {
            Colors.Red,                  // None,
            Colors.Blue,                 // Keyword,
            Colors.Black,                // Function,
            Colors.Green,                // Comment,
            Color.FromRgb(43, 145, 175), // UserType,
            Colors.Black,                // Argument,
            Colors.Black,                // Local,
            Colors.Black,                // Punctuation,
            Colors.Brown,                // Text,
            Colors.Brown                 // Number
        };

        #region Public Class Properties

        public string Text { get; private set; }
        public int Line { get; private set; }
        public int ColStart { get; private set; }
        public int ColEnd { get; private set; }
        public Type CodeType { get; private set; }

        #endregion

        public CodeFragment(string stringValue, Type codeType, int line, int colstart)
        {
            Text = stringValue;
            this.CodeType = codeType;
            Line = line;
            ColStart = colstart;
            ColEnd = colstart + stringValue.Length - 1;
        }

        public CodeFragment(int line, int colstart, int colend)
        {
            CodeType = Type.None;
            Line = line;
            ColStart = colstart;
            ColEnd = colend;
        }

        public static Color GetFragmentColor(Type fragmentType)
        {
            int index = ((int)fragmentType);
            if (index < 0 || (index >= CodeFragment.colorTable.Length))
                return Colors.Red;

            return CodeFragment.colorTable[index];
        }

        public void SetText(string newText)
        {
            Text = newText;
            ColEnd = ColStart + newText.Length - 1;
        }

        public void Offset(int offSet)
        {
            ColStart = ColStart + offSet;
            ColEnd = ColEnd + offSet;
        }

        public int CompareTo(object obj)
        {
            CodeFragment codeFragment = ((CodeFragment)obj);
            int fragmentLine = codeFragment.Line;
            if (Line != fragmentLine)
                return Line - fragmentLine;
            else
            {
                if (ColEnd < codeFragment.ColStart)
                    return -1;
                else if (ColStart > codeFragment.ColEnd)
                    return 1;
                else
                    return 0;
            }

            throw new NotImplementedException();
        }
    }

    internal class CodeFragmentManager
    {
        private IParsedScript parsedScript = null;
        private CodeFragment[] fragmentArray = null;
        private CodeFragment fragmentPointer = null;

        internal CodeFragmentManager(IParsedScript iParsedScript)
        {
            // TODO: Complete member initialization
            this.parsedScript = iParsedScript;

            CodeBlockNode parsedResults = iParsedScript.GetParsedResults();
            if (null != parsedResults)
            {
                NodeProcessor nodeProcessor = new NodeProcessor(parsedResults.Body);
                nodeProcessor.GenerateFragments(out fragmentArray);
                Array.Sort(fragmentArray);
            }
            else
            {
                // Create a dummy fragment just for the heck of it.
                fragmentArray = new CodeFragment[] { new CodeFragment(-1, -1, -1) };
            }
        }

        internal IParsedScript GetParsedScript()
        {
            return this.parsedScript;
        }

        internal int GetFragment(int column, int line, out CodeFragment fragment)
        {
            fragment = null;

            int index = Array.BinarySearch(fragmentArray, new CodeFragment(line, column, column));
            if (index < 0)
                return 0;

            fragment = fragmentArray[index];
            return fragmentArray[index].Text.Length;
        }

        internal bool GetLineFragments(int line, out List<CodeFragment> fragments)
        {
            LineComparer comparer = new LineComparer();
            int index = Array.BinarySearch<CodeFragment>(
                fragmentArray, new CodeFragment(line, 0, 0), comparer);

            fragments = null;
            if (index < 0) // No matching code fragment found!
                return false;

            while (index > 0)
            {
                // Search backward for the start fragment.
                if (fragmentArray[index - 1].Line != line)
                    break;

                index = index - 1;
            }

            int length = fragmentArray.Length;
            fragments = new List<CodeFragment>();
            while (index < length && (fragmentArray[index].Line == line))
                fragments.Add(fragmentArray[index++]);

            return true;
        }

        internal bool GetFragmentRegion(int column, int line, CharPosition start, CharPosition end)
        {
            CodeFragment target = new CodeFragment(line, column, column);
            int index = Array.BinarySearch(fragmentArray, target);
            if (index < 0 || (index >= fragmentArray.Length))
                return false;

            // If this is a comment fragment, then we only select the whole comment
            // line (from start to end) when user clicks on the first forward slash.
            // Otherwise returns "false" so the caller can handle its own word 
            // selection logics (e.g. single-word selection).
            // 
            CodeFragment fragment = fragmentArray[index];
            if (fragment.CodeType == CodeFragment.Type.Comment)
            {
                if (column != fragment.ColStart)
                    return false;
            }

            start.SetCharacterPosition(fragment.ColStart, fragment.Line);
            end.SetCharacterPosition(fragment.ColEnd + 1, fragment.Line);
            return true;
        }

        internal int GetPreviousFragment(int column, int line, out CodeFragment prevFragment)
        {
            prevFragment = null;
            CodeFragment target = new CodeFragment(line, column, column);
            int index = Array.BinarySearch(fragmentArray, target);
            if (index < 0 || (index >= fragmentArray.Length))
                return 0;

            index = index - 1;
            if (index < 0)
                return 0;

            prevFragment = fragmentArray[index];
            return fragmentArray[index].Text.Length;
        }

        internal int GetNextFragment(CodeFragment curFragemnt, out CodeFragment nextFragment)
        {
            nextFragment = null;
            CodeFragment target = curFragemnt;
            int index = Array.BinarySearch(fragmentArray, target);
            if (index < 0 || (index >= fragmentArray.Length))
                return 0;

            index = index + 1;
            if (index >= fragmentArray.Length)
                return 0;

           nextFragment = fragmentArray[index];
            return fragmentArray[index].Text.Length;
        }

        internal int GetFragmentForInspection(int column, int line, out CodeFragment forkFragment)
        {
            List<CodeFragment> fragments = null;
            CodeFragment currFragment = null;
            forkFragment = null;
            GetFragment(column, line, out currFragment);
            if (null != currFragment)
            {
                fragments = new List<CodeFragment>();
                fragments.Add(currFragment);
                CodeFragment tempFragment = null;
                CodeFragment tempPrevFragment = null;
                GetPreviousFragment(currFragment.ColStart, currFragment.Line, out tempPrevFragment);
                bool bExpectDot = true;
                while (null != tempPrevFragment)
                {
                    if (bExpectDot && tempPrevFragment.Text.Equals("."))
                    {
                        fragments.Add(tempPrevFragment);
                        tempFragment = tempPrevFragment;
                        GetPreviousFragment(tempFragment.ColStart, tempFragment.Line, out tempPrevFragment);
                    }
                    else if (!bExpectDot && tempPrevFragment.CodeType == CodeFragment.Type.Local)
                    {
                        fragments.Add(tempPrevFragment);
                        tempFragment = tempPrevFragment;
                        GetPreviousFragment(tempFragment.ColStart, tempFragment.Line, out tempPrevFragment);
                    }
                    else
                    {
                        break;
                    }
                    bExpectDot = !bExpectDot;
                }
                fragments.Reverse();
                string inspectionText = string.Empty;
                foreach (CodeFragment fragment in fragments)
                {
                    inspectionText += fragment.Text;
                }
                tempFragment = new CodeFragment(inspectionText, currFragment.CodeType, currFragment.Line, currFragment.ColStart);
                forkFragment = tempFragment;
                return forkFragment.Text.Length;
            }

            return 0;
        }

        internal bool SetFragmentText(int column, int line, string text)
        {
            CodeFragment oldCodeFragment;
            if (GetFragment(column, line, out oldCodeFragment) == 0)
                return false;
            else
            {
                List<CodeFragment> lineFragments = null;
                if (GetLineFragments(line, out lineFragments) == false)
                    return false;

                int offset = 0;
                bool foundTargetted = false;
                foreach (CodeFragment fragment in lineFragments)
                {
                    if (false == foundTargetted)
                    {
                        if (column >= fragment.ColStart && (column <= fragment.ColEnd))
                        {
                            foundTargetted = true;
                            offset = text.Length - fragment.Text.Length;
                            fragment.SetText(text);
                        }
                    }
                    else
                    {
                        fragment.Offset(offset);
                    }
                }
            }

            return true;
        }

        internal int GenerateHighlightArray(CodeFragment fragment)
        {
            if (fragment == null)
            {
                fragmentPointer = null;
                return 0;
            }
            else if (fragmentPointer == fragment)
                return 3;
            else if (fragmentPointer == null || fragmentPointer != fragment)
                fragmentPointer = fragment;
            if (fragmentPointer.CodeType == CodeFragment.Type.Local)
                return 1;
            else if (fragmentPointer.CodeType == CodeFragment.Type.Punctuation &&
                (fragmentPointer.Text == "{" || fragmentPointer.Text == "}" || fragmentPointer.Text == "(" || fragmentPointer.Text == ")"))
                return 2;
            else
                return 0;
        }

        internal CodeFragment[] GetAllMatchingFragments()
        {
            return Array.FindAll(fragmentArray, delegate(CodeFragment fragment)
            {
                return fragmentPointer.Text == fragment.Text;
            });
        }

        internal CodeFragment[] GetPairedFragments()
        {
            CodeFragment[] pairedFragments = null;

            string symbol = fragmentPointer.Text;
            string endSymbol = String.Empty;

            if (symbol == "{")
                endSymbol = "}";
            else if (symbol == "}")
                endSymbol = "{";
            else if (symbol == "(")
                endSymbol = ")";
            else if (symbol == ")")
                endSymbol = "(";

            int index = Array.BinarySearch(fragmentArray, fragmentPointer);
            int pairCount = 1;

            if (symbol == "{" || symbol == "(")
            {
                for (int arrIndex = index + 1; arrIndex < fragmentArray.Length; arrIndex++)
                {
                    if (fragmentArray[arrIndex].Text == symbol)
                        pairCount += 1;
                    else if (fragmentArray[arrIndex].Text == endSymbol)
                        pairCount -= 1;
                    if (pairCount == 0)
                    {
                        pairedFragments = new CodeFragment[2];
                        pairedFragments[0] = fragmentPointer;
                        pairedFragments[1] = fragmentArray[arrIndex];
                        break;
                    }
                }
            }
            else if (symbol == "}" || symbol == ")")
            {
                for (int arrIndex = index - 1; arrIndex >= 0; arrIndex--)
                {
                    if (fragmentArray[arrIndex].Text == symbol)
                        pairCount += 1;
                    else if (fragmentArray[arrIndex].Text == endSymbol)
                        pairCount -= 1;
                    if (pairCount == 0)
                    {
                        pairedFragments = new CodeFragment[2];
                        pairedFragments[0] = fragmentPointer;
                        pairedFragments[1] = fragmentArray[arrIndex];
                        break;
                    }
                }
            }

            return pairedFragments;
        }
    }
}

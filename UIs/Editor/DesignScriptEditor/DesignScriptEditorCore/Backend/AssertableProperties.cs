using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.CodeModel;
using DesignScript.Parser;
using System.IO;

namespace DesignScript.Editor.Core
{
    internal class AssertableProperties : IAssertableProperties
    {
        private ITextEditorCore textEditorCore = null;

        #region Public Operational Methods

        internal AssertableProperties(ITextEditorCore textEditorCore)
        {
            this.textEditorCore = textEditorCore;
        }

        #endregion

        #region IAssertableProperties Members

        public int LineCount
        {
            get
            {
                ITextBuffer textBuffer = textEditorCore.CurrentTextBuffer;
                return (null == textBuffer ? 0 : textBuffer.GetLineCount());
            }
        }

        public int BreakpointCount
        {
            get { return Solution.Current.BreakpointCount; }
        }

        public string SelectionText
        {
            get { return textEditorCore.SelectionText; }
        }

        public string ActiveScriptName
        {
            get
            {
                IScriptObject activeScript = Solution.Current.ActiveScript;
                if (null != activeScript)
                {
                    IParsedScript script = activeScript.GetParsedScript();
                    if (null != script)
                    {
                        string scriptPath = script.GetScriptPath();
                        if (string.IsNullOrEmpty(scriptPath) == false)
                        {
                            FileInfo fileInfo = new FileInfo(scriptPath);
                            return fileInfo.Name;
                        }
                    }
                }

                return string.Empty;
            }
        }

        public int CurrentSearchIndex
        {
            get
            {
                return textEditorCore.CurrentTextBuffer.CurrentSearchIndex;
            }
        }

        public string SearchTerm
        {
            get
            {
                if (textEditorCore.CurrentTextBuffer.TextToSearch != null)
                    return textEditorCore.CurrentTextBuffer.TextToSearch;
                else
                    return "invalid";
            }
        }

        public int MatchCount
        {
            get
            {
                if (textEditorCore.CurrentTextBuffer.SearchResult != null)
                    return textEditorCore.CurrentTextBuffer.SearchResult.Count;
                else
                    return 0;
            }
        }

        public int SearchOption
        {
            get
            {
                return textEditorCore.CurrentTextBuffer.SearchOption;
            }
        }

        public System.Drawing.Point CursorPosition
        {
            get { return textEditorCore.CursorPosition; }
        }

        public System.Drawing.Point SelectionStart
        {
            get { return textEditorCore.SelectionStart; }
        }

        public System.Drawing.Point SelectionEnd
        {
            get { return textEditorCore.SelectionEnd; }
        }

        public System.Drawing.Point ExecutionCursorStart
        {
            get { return GetExecutionCursor(true); }
        }

        public System.Drawing.Point ExecutionCursorEnd
        {
            get { return GetExecutionCursor(false); }
        }

        #endregion

        #region Private Class Helper Methods

        System.Drawing.Point GetExecutionCursor(bool start)
        {
            System.Drawing.Point cursor = new System.Drawing.Point();
            IExecutionSession session = Solution.Current.ExecutionSession;
            if (null != session)
            {
                CodeRange codeRange = new CodeRange();
                if (session.GetExecutionCursor(ref codeRange))
                {
                    if (false != start)
                    {
                        cursor.X = codeRange.StartInclusive.CharNo - 1;
                        cursor.Y = codeRange.StartInclusive.LineNo - 1;
                    }
                    else
                    {
                        cursor.X = codeRange.EndExclusive.CharNo - 1;
                        cursor.Y = codeRange.EndExclusive.LineNo - 1;
                    }
                }
            }

            return cursor;
        }

        #endregion
    }
}

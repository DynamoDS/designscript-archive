using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Core
{
    class ScriptState
    {
        internal ScriptState(ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
            this.cursorPosition = new CharPosition(textBuffer);
            this.selectionStart = new CharPosition(textBuffer);
            this.selectionEnd = new CharPosition(textBuffer);

            // @TODO(Ben): Can we do without the following three lines?
            this.cursorPosition.SetCharacterPosition(new System.Drawing.Point(0, 0));
            this.selectionStart.SetCharacterPosition(new System.Drawing.Point(0, 0));
            this.selectionEnd.SetCharacterPosition(new System.Drawing.Point(0, 0));
        }

        public int visualOffset = 0;
        public double horizontalScrollPosition = 0;
        public double verticalScrollPosition = 0;
        public ITextBuffer textBuffer = null;
        public CharPosition selectionEnd = null;
        public CharPosition selectionStart = null;
        public CharPosition cursorPosition = null;
        public System.Drawing.Point mouseDownPosition;
    }
}

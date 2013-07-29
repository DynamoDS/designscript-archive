using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore;
using DesignScript.Editor;

namespace DesignScript.Editor.Core
{
    class EditorOutputStream : IOutputStream
    {
        private List<ProtoCore.OutputMessage> outputMessages = null;

        public EditorOutputStream()
        {
        }

        public void Write(ProtoCore.OutputMessage message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Line " + message.Line);
            sb.Append(", Col " + message.Column);
            sb.Append(", Path " + message.FilePath);
            sb.Append(", Type " + message.Type);
            sb.Append(", Message " + message.Message);

            Logger.LogInfo("Editor-Output-Stream", sb.ToString());

            if (null == outputMessages)
                outputMessages = new List<ProtoCore.OutputMessage>();

            outputMessages.Add(message);

            if (message.Type == ProtoCore.OutputMessage.MessageType.Warning)
                message.Continue = true;
        }

        public List<ProtoCore.OutputMessage> GetMessages()
        {
            if (null == outputMessages)
            {
                outputMessages = new List<ProtoCore.OutputMessage>();
            }

            return outputMessages;
        }

        public void Clear()
        {
            if (null == outputMessages)
            {
                outputMessages = new List<ProtoCore.OutputMessage>();
            }
            else
            {
                outputMessages.Clear();
            }
        }
    }
}

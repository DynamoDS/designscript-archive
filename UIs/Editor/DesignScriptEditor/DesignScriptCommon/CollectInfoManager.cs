using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesignScript.Editor.Common
{
    using DesignScript.Editor.Core;

    public class CollectInfoManager
    {
        ILoggerWrapper logger = null;
        public bool CollectFeedback { get; private set; }

        public CollectInfoManager(ILoggerWrapper logger)
        {
            this.logger = logger;
            this.CollectFeedback = false;
            string filepath = DesignScript.Editor.Configurations.GetSettingsFilePath();

            if (!File.Exists(filepath))
            {
                SetCollectInfoOption(ShowCollectInfoDialog());
            }
            else
            {
                ITextEditorSettings editorSettings = EditorSettingsData.Deserialize(filepath);
                EnableDisableLogger(editorSettings.CollectFeedback);
            }
        }

        public void SetCollectInfoOption(bool collectFeedback)
        {
            this.CollectFeedback = collectFeedback;

            string filepath = DesignScript.Editor.Configurations.GetSettingsFilePath();
            ITextEditorSettings editorSettings = EditorSettingsData.Deserialize(filepath);

            EnableDisableLogger(collectFeedback);
            editorSettings.CollectFeedback = collectFeedback;
            EditorSettingsData.Serialize(filepath, editorSettings);
        }

        public bool GetCollectInfoOption()
        {
            string filepath = DesignScript.Editor.Configurations.GetSettingsFilePath();
            ITextEditorSettings editorSettings = EditorSettingsData.Deserialize(filepath);
            return editorSettings.CollectFeedback;
        }

        private bool ShowCollectInfoDialog()
        {
            CollectInfoDialog collectInfoDialog = new CollectInfoDialog();
            if (null != Application.Current)
                collectInfoDialog.Owner = Application.Current.MainWindow;

            collectInfoDialog.ShowDialog();
            return collectInfoDialog.CollectFeedback;
        }

        private void EnableDisableLogger(bool collectFeedback)
        {
            string optInOut = collectFeedback ? "Opt In" : "Opt Out";
            logger.FORCE_Log("CollectFeedbackOptIn", optInOut);
            logger.EnableLogging(collectFeedback);
        }
    }
}

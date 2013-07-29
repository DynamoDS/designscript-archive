using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;

namespace DesignScript.Instrumentation
{
    public abstract class ParallelGateway<type>
    {
        #region Private Class Data Members

        private ConcurrentQueue<type> pendingWorkItems = null;
        private BackgroundWorker backgroundWorker = null;

        #endregion

        #region Public Class Properties

        public bool EnableDiagnosticsOutput { get; set; }

        #endregion

        #region Protected Overridable Methods

        protected abstract bool HandleWorkTask(type workItem);
        protected abstract void OutputDiagnostics(type workItem);

        #endregion

        #region Private Class Helper Methods

        protected void CancelWorkInternal()
        {
            lock (typeof(BackgroundWorker))
            {
                if (null != backgroundWorker)
                {
                    if (false != backgroundWorker.IsBusy)
                        backgroundWorker.CancelAsync();
                }
            }
        }

        protected void AppendWorkItem(type workItem)
        {
            if (false != EnableDiagnosticsOutput)
                OutputDiagnostics(workItem);

            if (null == pendingWorkItems)
                pendingWorkItems = new ConcurrentQueue<type>();

            pendingWorkItems.Enqueue(workItem);
            if (null == backgroundWorker)
            {
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.WorkerSupportsCancellation = true;
                backgroundWorker.DoWork += new DoWorkEventHandler(ProcessWorkItems);
            }

            if (backgroundWorker.IsBusy == false)
                backgroundWorker.RunWorkerAsync();
        }

        private void ProcessWorkItems(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (null == worker)
                throw new InvalidOperationException("Expected a 'BackgroundWorker'!");

            while (true)
            {
                // Worker is requested to cancel its operations.
                if (worker.CancellationPending != false)
                    break;

                type nextWorkItem = default(type);
                if (pendingWorkItems.TryDequeue(out nextWorkItem) == false)
                {
                    Thread.Sleep(1000); // No work to do, go to sleep.
                    continue;
                }

                if (HandleWorkTask(nextWorkItem) == false)
                    break; // We are requested to stop.
            }
        }

        #endregion
    }
}

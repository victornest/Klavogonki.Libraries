using System;

namespace Klavogonki
{
    public class ProgressCounter
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        public int TotalProgress { get; set; }

        private double CurrentProgress { get; set; }

        private double BeforeOperationProgress { get; set; }

        private double TotalOperationProgress { get; set;}

        public void ResetProgress()
        {
            TotalProgress = 0;
            CurrentProgress = 0;
            BeforeOperationProgress = 0;
            TotalOperationProgress = 0;
        }

        public void AddProgress(double value)
        {
            CurrentProgress += value;
            BeforeOperationProgress = CurrentProgress;
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(new Progress((int)CurrentProgress, TotalProgress)));
        }

        public void SetTotalOperationProgress(double progress)
        {
            TotalOperationProgress = progress;
            BeforeOperationProgress = CurrentProgress;
        }

        public void AddOperationProgress(object sender, EventArgs<Progress> e)
        {
            CurrentProgress = BeforeOperationProgress + e.Value.Percent / 100.0 * TotalOperationProgress;
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(new Progress((int)CurrentProgress, TotalProgress)));
        }
    }
}

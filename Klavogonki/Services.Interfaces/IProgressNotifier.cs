using System;

namespace Klavogonki
{
    public interface IProgressNotifier
    {
        event EventHandler<EventArgs<Progress>> ProgressChanged;
    }
}
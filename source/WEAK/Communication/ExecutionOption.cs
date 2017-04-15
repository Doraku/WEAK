using System;

namespace WEAK.Communication
{
    /// <summary>
    /// Specifies how the callback should be executed.
    /// </summary>
    [Flags]
    public enum ExecutionOption
    {
        /// <summary>
        /// No options.
        /// </summary>
        None = 0,
        /// <summary>
        /// No strong reference should be kept on the target of the callback.
        /// </summary>
        WeakReference = 1,
        /// <summary>
        /// Specifies that the callback should be executed asynchronously.
        /// </summary>
        Async = 2,
        /// <summary>
        /// Specifies that the callback should be executed in a long running Task.
        /// </summary>
        LongRunning = 6,
        /// <summary>
        /// Specifies that the callback should be executed on the context.
        /// </summary>
        Context = 8
    }
}

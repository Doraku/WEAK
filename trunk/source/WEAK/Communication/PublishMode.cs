namespace WEAK.Communication
{
    /// <summary>
    /// Specifies how the callback should be executed.
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        /// Specifies that the callback should be executed on the publishing Thread.
        /// </summary>
        Direct = 0,
        /// <summary>
        /// Specifies that the callback should be executed in a Thread pool.
        /// </summary>
        Async = 1,
        /// <summary>
        /// Specifies that the callback should be executed in a long running Task.
        /// </summary>
        LongRunning = 2,
        /// <summary>
        /// Specifies that the callback should be executed on the context.
        /// </summary>
        Context = 3,
        /// <summary>
        /// Specifies that the callback should be executed asynchronously on the context.
        /// </summary>
        ContextAsync = 4
    }
}

namespace WEAK.Communication
{
    /// <summary>
    /// Specifies how the argument should be published.
    /// </summary>
    public enum PublishingMode
    {
        /// <summary>
        /// Specifies that the argument should be published on the executing Thread.
        /// </summary>
        Direct = 0,
        /// <summary>
        /// Specifies that the argument should be published in a Thread pool.
        /// </summary>
        Async = 1,
        /// <summary>
        /// Specifies that the argument should be published in a long running Task.
        /// </summary>
        LongRunning = 2,
        /// <summary>
        /// Specifies that the argument should be published on the context.
        /// </summary>
        Context = 3,
        /// <summary>
        /// Specifies that the argument should be published asynchronously on the context.
        /// </summary>
        ContextAsync = 4
    }
}

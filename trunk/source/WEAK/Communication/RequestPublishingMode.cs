namespace WEAK.Communication
{
    /// <summary>
    /// Specifies how the request should be published.
    /// </summary>
    public enum RequestPublishingMode
    {
        /// <summary>
        /// Specifies that the request should be published on the executing Thread.
        /// </summary>
        Direct = 1,
        /// <summary>
        /// Specifies that the request should be published in a Thread pool.
        /// </summary>
        Async = 2,
        /// <summary>
        /// Specifies that the request should be published in a long running Task.
        /// </summary>
        LongRunning = 3,
        /// <summary>
        /// Specifies that the request should be published on the context.
        /// </summary>
        Context = 4
    }
}

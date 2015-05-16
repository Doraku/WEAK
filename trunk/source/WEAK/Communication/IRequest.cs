namespace WEAK.Communication
{
    /// <summary>
    /// Defines a property to express the mode of publication of a request.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Gets the PublishingMode.
        /// </summary>
        RequestPublishingMode PulishingMode { get; }
    }
}

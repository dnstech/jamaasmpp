
namespace JamaaTech.Smpp.Net.Client
{
    interface IShortMessage
    {
        /// <summary>
        /// Gets the <see cref="IShortMessage"/> source address
        /// </summary>
        string SourceAddress { get; }

        /// <summary>
        /// Gets the <see cref="IShortMessage"/> destination address
        /// </summary>
        string DestinationAddress { get; }

        /// <summary>
        /// Tells if a delivery notification is required
        /// </summary>
        bool RegisterDeliveryNotification { get; }

        /// <summary>
        /// Gets the index of this message segment in a group of contatenated message segements
        /// </summary>
        int SegmentID { get; }

        /// <summary>
        /// Gets the message count
        /// </summary>
        int MessageCount { get; }

        /// <summary>
        /// Gets the message
        /// </summary>
        string Text { get; }
    }
}

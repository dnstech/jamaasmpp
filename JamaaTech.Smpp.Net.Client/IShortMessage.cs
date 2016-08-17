
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
        /// Total number of fragments within the concatenated short message
        /// </summary>
        int TotalSegments { get; }

        /// <summary>
        /// Sequence number of a particular message within the concatenated short message. Starting value is 1, then increments by 1.
        /// </summary>
        int SegmentSequenceNumber { get; }

        /// <summary>
        /// Originator generated reference number so that a segmented short message may be reassembled into a single original message
        /// </summary>
        int MultiSegmentMessageReferenceNumber { get; }

        /// <summary>
        /// Message text contents
        /// </summary>
        string Text { get; }

        /// <summary>
        /// SMSC (SMPP Server) message id of the submitted message
        /// </summary>
        string SmppMessageId { get; set; }

        string SmppErrorCode { get; set; }

        string SmppFinalDate { get; set; }

        string SmppMessageState { get; set; }                
    }
}
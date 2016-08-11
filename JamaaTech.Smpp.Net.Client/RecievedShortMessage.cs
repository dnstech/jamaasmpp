
namespace JamaaTech.Smpp.Net.Client
{
    using JamaaTech.Smpp.Net.Lib;

    public class RecievedShortMessage : ShortMessage
    {
        internal RecievedShortMessage(string sourceAddress, string destinatinAddress, string text, bool deliveryNotification = false)
            : base(sourceAddress, destinatinAddress, text, deliveryNotification)
        {
        }

        public int Sequence { get; private set; }

        internal RecievedShortMessage(string sourceAddress, string destinationAddress, int multiSegmentMessageReferenceNumber, int segmentSequenceNumber, int totalSegments, string text, bool deliveryNotification = false)
            : base(sourceAddress, destinationAddress, multiSegmentMessageReferenceNumber, segmentSequenceNumber, totalSegments, text, DataCoding.SMSCDefault, deliveryNotification)
        {
            this.Sequence = segmentSequenceNumber;
        }
    }
}
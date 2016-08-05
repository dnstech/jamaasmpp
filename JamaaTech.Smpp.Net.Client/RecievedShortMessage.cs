
namespace JamaaTech.Smpp.Net.Client
{
    public class RecievedShortMessage : ShortMessage
    {
        internal RecievedShortMessage(string sourceAddress, string destinatinAddress, string text, string messageId, bool deliveryNotification = false)
            : base(sourceAddress, destinatinAddress, text, deliveryNotification)
        {
            this.MessageId = messageId;
        }

        public int Sequence { get; private set; }

        internal RecievedShortMessage(int segmentId, int maxCount, int sequence, string sourceAddress, string destinatinAddress, string text, string messageId, bool deliveryNotification = false)
            : base(segmentId, maxCount, sourceAddress, destinatinAddress, text, deliveryNotification)
        {
            Sequence = sequence;
            this.MessageId = messageId;
        }
    }
}

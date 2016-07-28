
namespace JamaaTech.Smpp.Net.Client
{
    public class RecievedShortMessage : ShortMessage
    {
        internal RecievedShortMessage(string sourceAddress, string destinatinAddress, string text, bool deliveryNotification = false)
            : base(sourceAddress, destinatinAddress, text, deliveryNotification)
        {
        }

        public int Sequence { get; private set; }

        internal RecievedShortMessage(int segmentId, int maxCount, int sequence, string sourceAddress, string destinatinAddress, string text, bool deliveryNotification = false)
            : base(segmentId, maxCount, sourceAddress, destinatinAddress, text, deliveryNotification)
        {
            Sequence = sequence;
        }
    }
}

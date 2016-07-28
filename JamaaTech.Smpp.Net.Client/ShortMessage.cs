/************************************************************************
 * Copyright (C) 2008 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Client Library.
 *
 * Jamaa SMPP Client Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Client Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// Defines a base class for diffent types of messages that can be used with <see cref="SmppClient"/>
    /// </summary>
    public class ShortMessage : IShortMessage
    {
        public ShortMessage(string sourceAddress, string destinatinAddress, string text, bool deliveryNotification = false)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinatinAddress;
            Text = text;
            RegisterDeliveryNotification = deliveryNotification;
        }

        public ShortMessage(int segmentId, int messageCount, string sourceAddress, string destinatinAddress, string text, bool deliveryNotification = false)
            : this(sourceAddress, destinatinAddress, text, deliveryNotification)
        {
            SegmentID = segmentId;
            MessageCount = messageCount;
        }
        
        public string SourceAddress { get; protected set; }

        public string DestinationAddress { get; protected set; }

        public int SegmentID { get; protected set; }

        public int MessageCount { get; protected set; }

        public string Text { get; protected set; }

        public bool RegisterDeliveryNotification { get; protected set; }
    }
}

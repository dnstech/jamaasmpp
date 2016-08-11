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
    using JamaaTech.Smpp.Net.Lib;

    /// <summary>
    /// Defines a base class for diffent types of messages that can be used with <see cref="SmppClient"/>
    /// </summary>
    public class ShortMessage : IShortMessage
    {
        public ShortMessage(string sourceAddress, string destinationAddress, string text, bool deliveryNotification = false)
        {
            this.Text = text;
            this.SourceAddress = sourceAddress;
            this.DestinationAddress = destinationAddress;            
            this.RegisterDeliveryNotification = deliveryNotification;
        }

        public ShortMessage(string sourceAddress, string destinationAddress, int multiSegmentMessageReferenceNumber, int segmentSequenceNumber, int totalSegments, string text, DataCoding dataCoding, bool deliveryNotification = false)
            : this(sourceAddress, destinationAddress, text, deliveryNotification)
        {
            this.Text = text;
            this.DataCoding = dataCoding;
            this.SourceAddress = sourceAddress;
            this.DestinationAddress = destinationAddress;            
            this.RegisterDeliveryNotification = deliveryNotification;
            this.MultiSegmentMessageReferenceNumber = multiSegmentMessageReferenceNumber;
            this.SegmentSequenceNumber = segmentSequenceNumber;
            this.TotalSegments = totalSegments;
        }

        public DataCoding DataCoding { get; protected set; }
        
        public string SourceAddress { get; protected set; }

        public string DestinationAddress { get; protected set; }

        public bool RegisterDeliveryNotification { get; protected set; }

        public int MultiSegmentMessageReferenceNumber { get; protected set; }

        public int SegmentSequenceNumber { get; protected set; }

        public int TotalSegments { get; protected set; }

        public string Text { get; protected set; }

        public string SmppMessageId { get; set; }

        public string SmppErrorCode { get; set; }

        public string SmppFinalDate { get; set; }

        public string SmppMessageState { get; set; }
    }
}
/////************************************************************************
//// * Copyright (C) 2008 Jamaa Technologies
//// *
//// * This file is part of Jamaa SMPP Client Library.
//// *
//// * Jamaa SMPP Client Library is free software. You can redistribute it and/or modify
//// * it under the terms of the Microsoft Reciprocal License (Ms-RL)
//// *
//// * You should have received a copy of the Microsoft Reciprocal License
//// * along with Jamaa SMPP Client Library; See License.txt for more details.
//// *
//// * Author: Benedict J. Tesha
//// * benedict.tesha@jamaatech.com, www.jamaatech.com
//// *
//// ************************************************************************/

////using System;

////namespace JamaaTech.Smpp.Net.Client
////{
////    using JamaaTech.Smpp.Net.Lib;

////    public class TextMessage : ShortMessage
////    {
////        public string Text { get; protected set; }

////        public TextMessage(string sourceAddress, string destinationAddress, string text, bool deliveryNotification = false)
////            : base(sourceAddress, destinationAddress, deliveryNotification)
////        {
////            this.Text = text;
////            this.SourceAddress = sourceAddress;
////            this.DestinationAddress = destinationAddress;            
////            this.RegisterDeliveryNotification = deliveryNotification;
////        }

////        /// <summary>
////        /// Initializes a new instance of <see cref="TextMessage"/>
////        /// </summary>
////        public TextMessage(string sourceAddress, string destinatinAddress, int multiSegmentMessageReferenceNumber, int segmentSequenceNumber, int totalSegments, string text, DataCoding dataCoding = DataCoding.SMSCDefault, bool deliveryNotification = false)
////            : base(sourceAddress, destinatinAddress, multiSegmentMessageReferenceNumber, segmentSequenceNumber, totalSegments, dataCoding, deliveryNotification)
////        {
////            this.SourceAddress = sourceAddress;
////            this.DestinationAddress = destinatinAddress;
////            this.RegisterDeliveryNotification = deliveryNotification;
////            this.MultiSegmentMessageReferenceNumber = multiSegmentMessageReferenceNumber;
////            this.SegmentSequenceNumber = segmentSequenceNumber;
////            this.TotalSegments = totalSegments;
////            this.Text = text;
////        }
////    }
////}
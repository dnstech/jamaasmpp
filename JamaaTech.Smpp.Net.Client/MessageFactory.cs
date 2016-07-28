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

using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;

namespace JamaaTech.Smpp.Net.Client
{
    /// <summary>
    /// A factory class for constructing messages from PDUs
    /// </summary>
    public static class MessageFactory
    {
        /// <summary>
        /// Creates a <see cref="RecievedShortMessage"/> from a received <see cref="SingleDestinationPDU"/>
        /// </summary>
        /// <param name="pdu">The PDU from which a <see cref="RecievedShortMessage"/> is constructed</param>
        /// <returns>A <see cref="RecievedShortMessage"/> represening a text message extracted from the received PDU</returns>
        public static RecievedShortMessage CreateMessage(SingleDestinationPDU pdu)
        {
            //This version supports only text messages
            Udh udh = null;
            string message = string.Empty;
            pdu.GetMessageText(out message, out udh);
            //Check if the udh field is present
            //if (udh != null) { sms = new TextMessage(udh.SegmentID, udh.MessageCount, udh.MessageSequence); }
            if (udh != null)
            {
                return new RecievedShortMessage(udh.SegmentID, udh.MessageCount, udh.MessageSequence, pdu.SourceAddress.Address, pdu.DestinationAddress.Address, message, pdu.RegisteredDelivery == RegisteredDelivery.DeliveryReceipt);
            }

            return new RecievedShortMessage(pdu.SourceAddress.Address, pdu.DestinationAddress.Address, message, pdu.RegisteredDelivery == RegisteredDelivery.DeliveryReceipt);
        }
    }
}

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

using System;
using System.Collections.Generic;
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Client
{
    public class TextMessage : ShortMessage
    {
        private DataCoding coding = DataCoding.SMSCDefault;
        private DataCodingLengthDefinition length = new DataCodingLengthDefinition { SizeWithoutUdh = 160, SizeWithUdh = 153 };

        public TextMessage(string sourceAddress, string destinatinAddress, string text, bool deliveryNotification = false)
            : base(sourceAddress, destinatinAddress, text, deliveryNotification)
        {
        }


        internal IEnumerable<SendSmPDU> GetMessagePDUs(DataCoding defaultEncoding)
        {
            return GetPDUs(defaultEncoding);
        }

        protected IEnumerable<SendSmPDU> GetPDUs(DataCoding defaultEncoding)
        {
            coding = SniffEncoding();
            length = EncodingLenghts[coding];

            SegmentID = new Random().Next(1000, 9999); // generate random ID

            IList<string> subMessages = GetSubMessages();
            MessageCount = subMessages.Count;

            if (MessageCount > 1)
            {
                for (int sequence = 1; sequence <= MessageCount; sequence++)
                {
                    PDUHeader header = new PDUHeader(CommandType.SubmitSm, (uint)sequence);
                    var udh = new Udh(SegmentID, MessageCount, sequence);
                    SubmitSm sm = new SubmitSm(header);
                    sm.SourceAddress.Address = SourceAddress;
                    sm.DestinationAddress.Address = DestinationAddress;
                    sm.RegisteredDelivery = RegisterDeliveryNotification ? RegisteredDelivery.DeliveryReceipt : RegisteredDelivery.None;
                    sm.DataCoding = coding;

                    ByteBuffer buffer = new ByteBuffer();
                    buffer.Append(udh.GetBytes());
                    buffer.Append(SMPPEncodingUtil.GetBytesFromString(subMessages[sequence - 1], coding));

                    sm.SetMessageBytes(buffer.ToBytes());
                    if (udh != null) { sm.EsmClass = sm.EsmClass | EsmClass.UdhiIndicator; }
                    yield return sm;
                }
            }
            else
            {
                SubmitSm sm = new SubmitSm();
                sm.SourceAddress.Address = SourceAddress;
                sm.DestinationAddress.Address = DestinationAddress;
                sm.RegisteredDelivery = RegisterDeliveryNotification ? RegisteredDelivery.DeliveryReceipt : RegisteredDelivery.None;
                sm.DataCoding = coding;
                sm.SetMessageBytes(SMPPEncodingUtil.GetBytesFromString(Text, coding));

                yield return sm;
            }
        }

        private IList<string> GetSubMessages()
        {
            List<string> list = new List<string>();

            if(SMPPEncodingUtil.GetBytesFromString(Text, coding).Length <= length.SizeWithoutUdh)
            {
                list.Add(Text);
            }
            else
            {
                AddSubMessages(Text, list);
            }

            return list;
        }

        private void AddSubMessages(string remaining, IList<string> list)
        {
            if(SMPPEncodingUtil.GetBytesFromString(remaining, coding).Length <= length.SizeWithUdh)
            {
                list.Add(remaining);
            }
            else
            {
                int max = Math.Min(remaining.Length, length.SizeWithUdh);
                string partOfMessage = remaining.Substring(0, max);
                while (SMPPEncodingUtil.GetBytesFromString(partOfMessage, coding).Length > length.SizeWithUdh)
                {
                    max = max - 1;
                    partOfMessage = remaining.Substring(0, max);
                }
                list.Add(partOfMessage);
                remaining = remaining.Remove(0, max);
                AddSubMessages(remaining, list);
            }
        }

        private DataCoding SniffEncoding()
        {
            int max = 0;
            char character;
            foreach(char c in Text)
            {
                int code = (int)c;
                if(code > max)
                {
                    max = code;
                    character = c;
                }
            }

            DataCoding messageEncoding = DataCoding.SMSCDefault;

            if(max <= 127)
            {
                messageEncoding = DataCoding.ASCII;
            }
            else if(max <= 255)
            {
                messageEncoding = DataCoding.Latin1;
            }
            else
            {
                messageEncoding = DataCoding.UCS2;
            }

            return messageEncoding;
        }

        private class DataCodingLengthDefinition
        {
            public int SizeWithoutUdh { get; set; }
            public int SizeWithUdh { get; set; }
        }

        private static Dictionary<DataCoding, DataCodingLengthDefinition> EncodingLenghts = new Dictionary<DataCoding, DataCodingLengthDefinition>
        {
            { DataCoding.SMSCDefault, new DataCodingLengthDefinition { SizeWithoutUdh = 160, SizeWithUdh = 153 } },
            { DataCoding.Latin1, new DataCodingLengthDefinition { SizeWithoutUdh = 140, SizeWithUdh = 134 } },
            { DataCoding.ASCII, new DataCodingLengthDefinition { SizeWithoutUdh = 160, SizeWithUdh = 153 } },
            { DataCoding.UCS2, new DataCodingLengthDefinition { SizeWithoutUdh = 70, SizeWithUdh = 67 } },
        };

        public override string ToString()
        {
            return Text == null ? "" : Text;
        }
    }
}

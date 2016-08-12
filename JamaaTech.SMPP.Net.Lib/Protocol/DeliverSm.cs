/************************************************************************
 * Copyright (C) 2007 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Library.
 *
 * Jamaa SMPP Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

using System;
using JamaaTech.Smpp.Net.Portable;
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;

namespace JamaaTech.Smpp.Net.Lib.Protocol
{
    public class DeliverSm : SingleDestinationPDU
    {
        #region Variables
        private byte vProtocolId;
        private PriorityFlag vPriorityFlag;
        private string vScheduleDeliveryTime;
        private string vValidityPeriod;
        private bool vReplaceIfPresent;
        private byte vSmDefalutMessageId;
        private string vMessageId;
        private byte[] vMessageBytes;
        private MessageState vMessageState;
        #endregion

        #region Properties
        public override SmppEntityType AllowedSource
        {
            // IL: expected to see SMSC based on spec http://docs.nimta.com/SMPP_v3_4_Issue1_2.pdf
            // get { return SmppEntityType.ESME; }   
            get { return SmppEntityType.SMSC; }   
        }

        public override SmppSessionState AllowedSession
        {
            get { return SmppSessionState.Transmitter; }
        }

        public byte ProtocolID
        {
            get { return vProtocolId; }
            set { vProtocolId = value; }
        }

        public PriorityFlag PriorityFlag
        {
            get { return vPriorityFlag; }
            set { vPriorityFlag = value; }
        }

        public string ScheduleDeliveryTime
        {
            get { return vScheduleDeliveryTime; }
            set { vScheduleDeliveryTime = value; }
        }

        public string ValidityPeriod
        {
            get { return vValidityPeriod; }
            set { vValidityPeriod = value; }
        }

        public bool ReplaceIfPresent
        {
            get { return vReplaceIfPresent; }
            set { vReplaceIfPresent = value; }
        }

        public byte SmDefaultMessageId
        {
            get { return vSmDefalutMessageId; }
            set { vSmDefalutMessageId = value; }
        }

        public string MessageId
        {
            get { return vMessageId; }
            set{ vMessageId = value; }
        }

        public MessageState MessageState
        {
            get { return vMessageState; }
            set { vMessageState = value; }
        }

        #endregion

        #region Constructors
        public DeliverSm(PDUHeader header)
            : base(header)
        {
            vServiceType = Protocol.ServiceType.DEFAULT;
            vProtocolId = 0;
            vPriorityFlag = PriorityFlag.Level0;
            vScheduleDeliveryTime = "";
            vValidityPeriod = "";
            vRegisteredDelivery = RegisteredDelivery.None;
            vReplaceIfPresent = false;
            vDataCoding = DataCoding.ASCII;
            vSmDefalutMessageId = 0;
            vMessageState = MessageState.Unknown;
        }

        public DeliverSm()
            : this(new PDUHeader(CommandType.SubmitSm))
        { }
        #endregion

        #region Methods
        public override ResponsePDU CreateDefaultResponse()
        {
            PDUHeader header = new PDUHeader(CommandType.DeliverSmResp, vHeader.SequenceNumber);
            return new GenericNack(header);
        }

        protected override byte[] GetBodyData()
        {
            ByteBuffer buffer = new ByteBuffer(256);
            buffer.Append(EncodeCString(vServiceType));
            buffer.Append(vSourceAddress.GetBytes());
            buffer.Append(vDestinationAddress.GetBytes());
            buffer.Append((byte)vEsmClass);
            buffer.Append(vProtocolId);
            buffer.Append((byte)vPriorityFlag);
            buffer.Append(EncodeCString(vScheduleDeliveryTime));
            buffer.Append(EncodeCString(vValidityPeriod));
            buffer.Append((byte)vRegisteredDelivery);
            buffer.Append(vReplaceIfPresent ? (byte)1 : (byte)0);
            buffer.Append((byte)vDataCoding);
            buffer.Append(vSmDefalutMessageId);

            // IL: is this going to work?
            // buffer.Append((byte)vMessageState);

            //Check if vMessageBytes is not null
            if (vMessageBytes == null)
            {
                //Check whether optional field is used
                if (vTlv.GetTlvByTag(Tag.message_payload) == null)
                {
                    //Create an empty message
                    vMessageBytes = new byte[] { 0x00 };
                }
            }
            if (vMessageBytes == null) { buffer.Append(0); }
            else
            {
                buffer.Append((byte)vMessageBytes.Length);
                buffer.Append(vMessageBytes);
            }
            return buffer.ToBytes();
        }

        protected override void Parse(ByteBuffer buffer)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            vServiceType = DecodeCString(buffer);
            vSourceAddress = SmppAddress.Parse(buffer);
            vDestinationAddress = SmppAddress.Parse(buffer);
            vEsmClass = (EsmClass)GetByte(buffer);
            vProtocolId = GetByte(buffer);
            vPriorityFlag = (PriorityFlag)GetByte(buffer);
            vScheduleDeliveryTime = DecodeCString(buffer);
            vValidityPeriod = DecodeCString(buffer);
            vRegisteredDelivery = (RegisteredDelivery)GetByte(buffer);
            vReplaceIfPresent = GetByte(buffer) == 0 ? false : true;
            vDataCoding = (DataCoding)GetByte(buffer);
            vSmDefalutMessageId = GetByte(buffer);

            int length = GetByte(buffer);
            if (length == 0) { vMessageBytes = null; }
            else
            {
                if (length > buffer.Length)
                {
                    throw new NotEnoughBytesException("Pdu encoutered less bytes than indicated by message length");
                }
                vMessageBytes = buffer.Remove(length);
            }

            if (buffer.Length > 0) { vTlv = TlvCollection.Parse(buffer); }
            var messageIdTag = vTlv.GetTlvByTag(Tag.receipted_message_id);
            if (messageIdTag != null)
            {
                var bytesValue = vTlv.GetTlvByTag(Tag.receipted_message_id).RawValue;
                this.MessageId = SmppEncodingUtil.GetStringFromBytes(bytesValue);
            }

            var messageStateTag = vTlv.GetTlvByTag(Tag.message_state);
            if (messageStateTag != null)
            {
                var bytesValue = vTlv.GetTlvByTag(Tag.message_state).RawValue;
                this.MessageState = (MessageState)GetByte(new ByteBuffer(bytesValue));
            } 
        }

        public override byte[] GetMessageBytes()
        {
            if (vMessageBytes != null) { return vMessageBytes; }
            //Otherwise, check if the 'message_payload' field is used
            Tlv.Tlv tlv = vTlv.GetTlvByTag(Tag.message_payload);
            if (tlv == null) { return null; }
            return tlv.RawValue;
        }

        public override void SetMessageBytes(byte[] message)
        {
            if (message != null && message.Length > 254)
            { throw new ArgumentException("Message length cannot be greater than 254 bytes"); }
            vMessageBytes = message;
        }
        #endregion
    }
}

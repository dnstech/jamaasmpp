/************************************************************************
 * Copyright (C) 2008 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Client Library.
 *
 * Jamaa SMPP Client Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Recisprocal License
 * along with Jamaa SMPP Client Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.coms
 *
 ************************************************************************/

using System;
using System.Threading;
using System.Diagnostics;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Client
{
    public class SmppClient : ISmppClient
    {
        private const int MinimumReconnectInterval = 5000;
        private SmppClientSession transieverSession;
        private SmppClientSession recieverSession;
        private Exception lastException;
        private object connectionSyncRoot;
        private Timer timer;
        private int timeout;
        private int autoReconnectDelay;
        private SendMessageCallBack sendMessageCallback;
        private bool started;
        private static TraceSwitch traceSwitch = new TraceSwitch("SmppClientSwitch", "SmppClient trace switch");

        public event EventHandler<MessageEventArgs> MessageReceived;

        public event EventHandler<MessageEventArgs> MessageDelivered;

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        public event EventHandler<MessageEventArgs> MessageSent;

        public event EventHandler<StateChangedEventArgs> StateChanged;

        public SmppClient(SmppConnectionProperties connectionProperties)
        {
            if(connectionProperties == null)
            {
                // IL: replaced nameof(connectionProperties)
                throw new ArgumentNullException(typeof(SmppConnectionProperties).Name, "Connection properties must be set");
            }

            ConnectionProperties = connectionProperties;
            connectionSyncRoot = new object();
            autoReconnectDelay = 10000;
            timeout = 5000;
            ConnectionState = SmppConnectionState.Closed;
            KeepAliveInterval = 30000;

            timer = new Timer(AutoReconnectTimerEventHandler, null, Timeout.Infinite, AutoReconnectDelay);
            sendMessageCallback += SendMessage;
        }
        
        public int AutoReconnectDelay
        {
            get { return autoReconnectDelay; }
            set
            {
                timer.Change(Timeout.Infinite, autoReconnectDelay);
                autoReconnectDelay = value;
            }
        }

        public SmppConnectionState ConnectionState { get; private set; }

        public int KeepAliveInterval { get; set; }

        public SmppConnectionProperties ConnectionProperties { get; private set; }

        public int ConnectionTimeout { get; set; }

        public bool Started { get; private set; }

        internal SendSmPDU GetMessagePdu(ShortMessage message)
        {
            if (message == null)
            {
                throw new SmppClientException("Each message should have at least a single segment");
            }

            var sm = new SubmitSm();
            sm.SourceAddress.Address = message.SourceAddress;
            sm.DestinationAddress.Address = message.DestinationAddress;
            sm.RegisteredDelivery = message.RegisterDeliveryNotification ? RegisteredDelivery.DeliveryReceipt : RegisteredDelivery.None;
            sm.DataCoding = message.DataCoding;
            sm.SetMessageBytes(SMPPEncodingUtil.GetBytesFromString(message.Text, sm.DataCoding));

            return sm;
        }

        public bool QueryMessage(ShortMessage message)
        {
            if (message == null)
            {
                return false;
            }

            if (this.ConnectionState != SmppConnectionState.Connected)
            {
                throw new SmppClientException("Quering sent messages failed because the SmppClient is not connected");
            }

            var querySm = new QuerySm { MessageID = message.SmppMessageId };
            ResponsePDU response = this.transieverSession.SendPdu(querySm);
            if (response.Header.ErrorCode != SmppErrorCode.ESME_ROK)
            {
                throw new SmppException(response.Header.ErrorCode);
            }

            var querySmResponse = response as QuerySmResp;
            if (querySmResponse != null)
            {                
                message.SmppFinalDate = querySmResponse.FinalDate;
                message.SmppErrorCode = querySmResponse.ErrorCode.ToString();
                message.SmppMessageState = querySmResponse.MessageState.ToString();
            }

            return true;
        }

        public void SendMessage(ShortMessage message, int timeOut)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (this.ConnectionState != SmppConnectionState.Connected)
            {
                throw new SmppClientException("Sending message operation failed because the SmppClient is not connected");
            }

            var sm = new SubmitSm();
            sm.SourceAddress.Address = message.SourceAddress;
            sm.DestinationAddress.Address = message.DestinationAddress;
            sm.RegisteredDelivery = message.RegisterDeliveryNotification ? RegisteredDelivery.DeliveryReceipt : RegisteredDelivery.None;
            sm.DataCoding = message.DataCoding;
            sm.SetMessageBytes(SMPPEncodingUtil.GetBytesFromString(message.Text, sm.DataCoding));


            var response = this.transieverSession.SendPdu(sm, timeOut);
            if (response.Header.ErrorCode != SmppErrorCode.ESME_ROK)
            {
                throw new SmppException(response.Header.ErrorCode);
            }

            message.SmppMessageId = GetMessageIdFromResponse(response);

            this.RaiseMessageSentEvent(message);
            
        }

        public void SendMessage(ShortMessage message)
        {
            SendMessage(message, transieverSession.DefaultResponseTimeout);
        }

        public IAsyncResult BeginSendMessage(ShortMessage message, int timeout, AsyncCallback callback, object state)
        {
            return sendMessageCallback.BeginInvoke(message, timeout, callback, state);
        }

        public IAsyncResult BeginSendMessage(ShortMessage message, AsyncCallback callback, object state)
        {
            int timeout = 0;
            timeout = transieverSession.DefaultResponseTimeout;
            return BeginSendMessage(message, timeout, callback, state);
        }

        public void EndSendMessage(IAsyncResult result)
        {
            sendMessageCallback.EndInvoke(result);
        }
       
        public void Start()
        {
            Start(0);
        }

        public void Start(int connectDelay)
        {
            if (connectDelay < 0)
            {
                connectDelay = 0;
            }

            started = true;
            timer.Change(connectDelay, AutoReconnectDelay);
            RaiseStateChangedEvent(started);
        }

        public void Connect()
        {
            Open(timeout);
        }

        public void Connect(int timeout)
        {
            Open(timeout);
        }
        
        public void Stop()
        {
            if (!started) { return; }
            started = false;
            StopTimer();
            RaiseStateChangedEvent(started);
        }

        public void Disconnect()
        {
            CloseSession();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private void Open(int timeOut)
        {
            try
            {
                if (Monitor.TryEnter(connectionSyncRoot))
                {
                    if (ConnectionState != SmppConnectionState.Closed)
                    {
                        lastException = new InvalidOperationException("You cannot open while the instance is already connected");
                        throw lastException;
                    }

                    SessionBindInfo bindInfo = null;
                    bool useSepConn = false;
                    lock (ConnectionProperties.SyncRoot)
                    {
                        bindInfo = ConnectionProperties.GetBindInfo();
                        useSepConn = ConnectionProperties.InterfaceVersion == InterfaceVersion.v33;
                    }

                    try
                    {
                        OpenSession(bindInfo, useSepConn, timeOut);
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        throw;
                    }
                    lastException = null;
                }
                else
                {
                    //Another thread is already in either a connecting or reconnecting state
                    //Wait until the thread finishes
                    Monitor.Enter(connectionSyncRoot);
                    //Now, the thread has finished connecting,
                    //Check on the result if the thread encountered any problem during connection
                    if (lastException != null) { throw lastException; }
                }
            }
            finally
            {
                Monitor.Exit(connectionSyncRoot);
            }
        }

        private void OpenSession(SessionBindInfo bindInfo, bool useSeparateConnections, int timeOut)
        {
            ChangeState(SmppConnectionState.Connecting);

            if (useSeparateConnections)
            {
                //Create two separate sessions for sending and receiving
                try
                {
                    bindInfo.AllowReceive = true;
                    bindInfo.AllowTransmit = false;
                    recieverSession = SmppClientSession.Bind(bindInfo, timeOut);
                    InitializeSession(recieverSession);
                }
                catch
                {
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }
                //--
                try
                {
                    bindInfo.AllowReceive = false;
                    bindInfo.AllowTransmit = true;
                    transieverSession = SmppClientSession.Bind(bindInfo, timeOut);
                    InitializeSession(transieverSession);
                }
                catch
                {
                    try { recieverSession.EndSession(); }
                    catch {/*Silent catch*/}
                    recieverSession = null;
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }
                ChangeState(SmppConnectionState.Connected);
            }
            else
            {
                //Use a single session for both sending and receiving
                bindInfo.AllowTransmit = true;
                bindInfo.AllowReceive = true;
                try
                {
                    SmppClientSession session = SmppClientSession.Bind(bindInfo, timeOut);
                    transieverSession = session;
                    recieverSession = session;
                    InitializeSession(session);
                    ChangeState(SmppConnectionState.Connected);
                }
                catch (SmppException ex)
                {
                    if (ex.ErrorCode == SmppErrorCode.ESME_RINVCMDID)
                    {
                        //If SMSC returns ESME_RINVCMDID (Invalid command id)
                        //the SMSC might not be supporting the BindTransceiver PDU
                        //Therefore, we can try to use bind with separate connections
                        OpenSession(bindInfo, true, timeOut);
                    }
                    else
                    {
                        ChangeState(SmppConnectionState.Closed);
                        //Start background timer
                        StartTimer();
                        throw;
                    }
                }
                catch
                {
                    ChangeState(SmppConnectionState.Closed);
                    StartTimer();
                    throw;
                }
            }
        }

        private void CloseSession()
        {
            if (ConnectionState == SmppConnectionState.Closed)
            {
                return;
            }

            ChangeState(SmppConnectionState.Closed);

            if (transieverSession != null)
            {
                transieverSession.EndSession();
                transieverSession = null;
            }

            if (recieverSession != null)
            {
                recieverSession.EndSession();
                recieverSession = null;
            }
        }

        private void InitializeSession(SmppClientSession session)
        {
            session.EnquireLinkInterval = KeepAliveInterval;
            session.PduReceived += PduReceivedEventHander;
            session.SessionClosed += SessionClosedEventHandler;
        }

        private void ChangeState(SmppConnectionState newState)
        {
            SmppConnectionState oldState = SmppConnectionState.Closed;
            oldState = ConnectionState;
            ConnectionState = newState;
            ConnectionProperties.SmscID = newState == SmppConnectionState.Connected ? transieverSession.SmscID : "";
            RaiseConnectionStateChangeEvent(newState, oldState);
        }

        private void RaiseMessageReceivedEvent(ShortMessage message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageEventArgs(message));
            }
        }

        private void RaiseMessageDeliveredEvent(ShortMessage message)
        {
            if (MessageDelivered != null)
            {
                MessageDelivered(this, new MessageEventArgs(message));
            }
        }

        private void RaiseMessageSentEvent(ShortMessage message)
        {
            if (MessageSent != null)
            {
                MessageSent(this,new MessageEventArgs(message));
            }
        }

        private void RaiseConnectionStateChangeEvent(SmppConnectionState newState, SmppConnectionState oldState)
        {
            if (ConnectionStateChanged == null)
            {
                return;
            }

            ConnectionStateChangedEventArgs e = new ConnectionStateChangedEventArgs(newState,oldState, AutoReconnectDelay);
            ConnectionStateChanged(this, e);

            if (e.ReconnectInteval < MinimumReconnectInterval)
            {
                e.ReconnectInteval = MinimumReconnectInterval;
            }

            Interlocked.Exchange(ref autoReconnectDelay, e.ReconnectInteval);
        }

        private void RaiseStateChangedEvent(bool started)
        {
            if (StateChanged == null)
            {
                return;
            }

            StateChangedEventArgs e = new StateChangedEventArgs(started);
            StateChanged(this, e);
        }

        //// message_id should be implemented in the following packet types according to the spec - http://docs.nimta.com/SMPP_v3_4_Issue1_2.pdf:
        //// => submit_sm_resp	    (from SMSC to ESME)				            [OK]
        //// => submit_multi_resp	(from SMSC to ESME)				            [Missing]
        //// => deliver_sm		    (from SMSC to ESME)				            [OK]	
        //// => deliver_sm_resp	    (from ESME to SMSC)				            [Missing]
        //// => data_sm		        (from SMSC to ESME) / (from ESME to SMSC)	[Missing]
        //// => data_sm_resp        (from SMSC to ESME) / (from ESME to SMSC)	[OK]
        //// => query_sm            (from ESME to SMSC)				            [OK]
        //// => query_sm_resp       (from SMSC to ESME)				            [OK]
        //// => cancel_sm		    (from ESME to SMSC)				            [OK]
        //// => replace_sm		    (from ESME to SMSC)				            [OK]

        //// message_state should be implemented in the following packet types according to the spec - http://docs.nimta.com/SMPP_v3_4_Issue1_2.pdf:
        //// => deliver_sm 		    (from SMSC to ESME)				            [Missing] – attempted to fix
        //// => data_sm		        (from SMSC to ESME) / (from ESME to SMSC)	[Missing]
        //// => query_sm_resp	    (from SMSC to ESME)				            [OK]


        private static string GetMessageIdFromResponse(ResponsePDU response)
        {
            if (response == null)
            {
                return string.Empty;
            }

            var submitSmResp = response as SubmitSmResp;
            if (submitSmResp != null)
            {
                return submitSmResp.MessageID;
            }

            var dataSmResp = response as DataSmResp;
            if (dataSmResp != null)
            {
                return dataSmResp.MessageID;
            }

            var querySmResp = response as QuerySmResp;
            if (querySmResp != null)
            {
                return querySmResp.MessageID;
            }

            return string.Empty;

            // Not supported ATM
            ////var deliverSmResp = response as DeliverSmResp;
            ////if (deliverSmResp != null)
            ////{
            ////    return deliverSmResp.MessageId;
            ////}           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetMessageIdFromRequest(RequestPDU request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            // From SMSC (server) to ESME (terminal)
            var deliverSm = request as DeliverSm;
            if (deliverSm != null)
            {
                return deliverSm.MessageId;
            }

            // SmOperationPDU includes: QuerySm, CancelSm, ReplaceSm
            var smOperationPdu = request as SmOperationPDU;
            if (smOperationPdu != null)
            {
                return smOperationPdu.MessageID;
            }

            return string.Empty;
        }

        private static string GetMessageStateFromRequest(RequestPDU request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            // From SMSC (server) to ESME (terminal)
            var deliverSm = request as DeliverSm;
            if (deliverSm != null)
            {
                return deliverSm.MessageState.ToString();
            }

            // TODO: Add support for other packages - see the commented section above
            return string.Empty;
        }

        private static string GetMessageStateFromRequest(ResponsePDU response)
        {
            if (response == null)
            {
                return string.Empty;
            }

            // From SMSC (server) to ESME (terminal)
            var querySmResp = response as QuerySmResp;
            if (querySmResp != null)
            {
                return querySmResp.MessageState.ToString();
            }

            // TODO: Add support for other packages - see the commented section above
            return string.Empty;
        }

        private void PduReceivedEventHander(object sender, PduReceivedEventArgs e)
        {
            //This handler is interested in SingleDestinationPDU only
            SingleDestinationPDU pdu = e.Request as SingleDestinationPDU;
            if (pdu == null)
            {
                return;
            }

            ShortMessage message;
            try
            {
                message = MessageFactory.CreateMessage(pdu);
                message.SmppMessageId = GetMessageIdFromRequest(e.Request);;
                message.SmppMessageState = GetMessageStateFromRequest(e.Request);
            }
            catch (SmppException smppEx)
            {
                if (traceSwitch.TraceError)
                {
                    string traceMessage = string.Format("200019:SMPP message decoding failure - {0} - {1} {2};",
                                                        smppEx.ErrorCode, 
                                                        new ByteBuffer(pdu.GetBytes()).DumpString(), 
                                                        smppEx.Message);
                    Trace.WriteLine(traceMessage);
                }
                //Notify the SMSC that we encountered an error while processing the message
                e.Response = pdu.CreateDefaultResponse();
                e.Response.Header.ErrorCode = smppEx.ErrorCode;
                return;
            }
            catch(Exception ex)
            {
                if (traceSwitch.TraceError)
                {
                    string traceMessage = string.Format("200019:SMPP message decoding failure - {0} {1};",
                                                        new ByteBuffer(pdu.GetBytes()).DumpString(), 
                                                        ex.Message);
                    Trace.WriteLine(traceMessage);
                }
                //Let the receiver know that this message was rejected
                e.Response = pdu.CreateDefaultResponse();
                e.Response.Header.ErrorCode = SmppErrorCode.ESME_RX_P_APPN; //ESME Receiver Reject Message
                return;
            }

            //If we have just a normal message
            if ((((byte)pdu.EsmClass) | 0xc3) == 0xc3)
            {
                RaiseMessageReceivedEvent(message);
            }

            // TODO: Investigate why pdu is always set to Default instead of EsmClass.DeliveryReceipt
            //Or if we have received a delivery receipt
            // else if ((pdu.EsmClass & EsmClass.DeliveryReceipt) == EsmClass.DeliveryReceipt)
            else
            {
                RaiseMessageDeliveredEvent(message);
            }
        }

        private void SessionClosedEventHandler(object sender, SmppSessionClosedEventArgs e)
        {
            if (e.Reason != SmppSessionCloseReason.EndSessionCalled)
            {
                StartTimer();
            }
            CloseSession();
        }

        private void StartTimer()
        {
            timer.Change(AutoReconnectDelay, AutoReconnectDelay);
        }

        private void StopTimer()
        {
            timer.Change(Timeout.Infinite, AutoReconnectDelay);
        }

        private void AutoReconnectTimerEventHandler(object state)
        {
            //Do not reconnect if AutoReconnectDalay < 0 or if SmppClient is shutdown
            if (AutoReconnectDelay <= 0 || !Started)
            {
                return;
            }

            //Stop the timer from raising subsequent events before
            //the current thread exists
            StopTimer();

            int timeOut = 0;
            timeOut = timeout;
            try { Open(timeOut); }
            catch (Exception) {/*Do nothing*/}

            if (ConnectionState == SmppConnectionState.Closed)
            { StartTimer(); }
            else
            { StopTimer(); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResorces)
        {
            try
            {
                Stop();
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            catch { /*Sielent catch*/ }
        }
    }
}
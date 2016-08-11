using System;

namespace JamaaTech.Smpp.Net.Client
{
    public interface ISmppClient : IDisposable
    {
        /// <summary>
        /// Occurs when a message is received
        /// </summary>
        event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Occurs when a message delivery notification is received
        /// </summary>
        event EventHandler<MessageEventArgs> MessageDelivered;

        /// <summary>
        /// Occurs when connection state changes
        /// </summary>
        event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Occurs when a message is successfully sent
        /// </summary>
        event EventHandler<MessageEventArgs> MessageSent;

        /// <summary>
        /// Occurs when <see cref="ISmppClient"/> is started or shut down
        /// </summary>
        event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets or sets a value indicating the time in miliseconds to wait before attemping to reconnect after a connection is lost
        /// </summary>
        int AutoReconnectDelay { get; set; }

        /// <summary>
        /// Indicates the current state of <see cref="ISmppClient"/>
        /// </summary>
        SmppConnectionState ConnectionState { get; }

        /// <summary>
        /// Gets or sets a value that indicates the time in miliseconds in which Enquire Link PDUs are periodically sent
        /// </summary>
        int KeepAliveInterval { get; set; }

        /// <summary>
        /// Gets an instance of <see cref="SmppConnectionProperties"/> that represents connection properties for this <see cref="ISmppClient"/>
        /// </summary>
        SmppConnectionProperties ConnectionProperties { get; }

        /// <summary>
        /// Gets or sets a value that speficies the amount of time after which a synchronous <see cref="ISmppClient.SendMessage"/> call will timeout
        /// </summary>
        int ConnectionTimeout { get; set; }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> value indicating if an instance of <see cref="ISmppClient"/> is started
        /// </summary>
        bool Started { get; }

        /// <summary>
        /// Immediately attempts to reestablish a lost connection without waiting for <see cref="ISmppClient"/> to automatically reconnect
        /// </summary>
        void Connect();

        /// <summary>
        /// Immediately attempts to reestablish a lost connection without waiting for <see cref="ISmppClient"/> to automatically reconnect
        /// </summary>
        /// <param name="timeout">A time in miliseconds after which a connection operation times out</param>
        void Connect(int timeout);

        /// <summary>
        /// Disconnects <see cref="ISmppClient"/> from the remote SMPP server 
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Starts <see cref="ISmppClient"/> and immediately connects to a remote SMPP server
        /// </summary>
        void Start();

        /// <summary>
        /// Starts <see cref="ISmppClient"/> and waits for a specified amount of time before establishing connection
        /// </summary>
        /// <param name="connectDelay">A value in miliseconds to wait before establishing connection</param>
        void Start(int delay);

        /// <summary>
        /// Stop <see cref="ISmppClient"/>
        /// </summary>
        void Stop();

        /// <summary>
        /// Restarts <see cref="ISmppClient"/>
        /// </summary>
        void Restart();

        /// <summary>
        /// Sends message to a remote SMMP server
        /// </summary>
        /// <param name="message">A message to send</param>
        /// <param name="timeOut">A value in miliseconds after which the send operation times out</param>
        void SendMessage(ShortMessage message, int timeOut);

        /// <summary>
        /// Sends message to a remote SMPP server
        /// </summary>
        /// <param name="message">A message to send</param>
        void SendMessage(ShortMessage message);

        /// <summary>
        /// Sends message asynchronously to a remote SMPP server
        /// </summary>
        /// <param name="message">A message to send</param>
        /// <param name="timeout">A value in miliseconds after which the send operation times out</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate</param>
        /// <param name="state">An object that contains state information for this request</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send message operation</returns>
        IAsyncResult BeginSendMessage(ShortMessage message, int timeout, AsyncCallback callback, object state);

        /// <summary>
        /// Sends message asynchronously to a remote SMPP server
        /// </summary>
        /// <param name="message">A message to send</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate</param>
        /// <param name="state">An object that contains state information for this request</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send message operation</returns>
        IAsyncResult BeginSendMessage(ShortMessage message, AsyncCallback callback, object state);

        /// <summary>
        /// Ends a pending asynchronous send message operation
        /// </summary>
        /// <param name="result">An <see cref="IAsyncResult"/> that stores state information for this asynchronous operation</param>
        void EndSendMessage(IAsyncResult result);

        bool QueryMessage(ShortMessage message);
    }
}
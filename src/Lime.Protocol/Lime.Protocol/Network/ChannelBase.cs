﻿using Lime.Protocol.Resources;
using Lime.Protocol.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lime.Protocol.Network
{
    /// <summary>
    /// Base class for the protocol
    /// communication channels
    /// </summary>
    public abstract class ChannelBase : IChannel, IDisposable
    {
        #region Fields
        
		private readonly TimeSpan _sendTimeout;
		private readonly bool _fillEnvelopeRecipients;
		private readonly bool _autoReplyPings;


		private readonly IAsyncQueue<Message> _messageBuffer;
		private readonly IAsyncQueue<Command> _commandBuffer;
		private readonly IAsyncQueue<Notification> _notificationBuffer;
		private readonly IAsyncQueue<Session> _sessionBuffer;		

        private Task _consumeTransportTask;

        protected readonly CancellationTokenSource _channelCancellationTokenSource;                
        private bool _isDisposing;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of ChannelBase
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="sendTimeout"></param>
        /// <param name="buffersLimit"></param>
        /// <param name="fillEnvelopeRecipients">Indicates if the from and to properties of sent and received envelopes should be filled with the session information if not defined.</param>
        /// <param name="autoReplyPings">Indicates if the channel should reply automatically to ping request commands. In this case, the ping command are not returned by the ReceiveCommandAsync method.</param>
        public ChannelBase(ITransport transport, TimeSpan sendTimeout, int buffersLimit, bool fillEnvelopeRecipients, bool autoReplyPings)
        {
            if (transport == null)
            {
                throw new ArgumentNullException("transport");
            }

            this.Transport = transport;
            this.Transport.Closing += Transport_Closing;

            _sendTimeout = sendTimeout;
            _fillEnvelopeRecipients = fillEnvelopeRecipients;
            _autoReplyPings = autoReplyPings;

            _channelCancellationTokenSource = new CancellationTokenSource();

            this.State = SessionState.New;

			_messageBuffer = new AsyncQueue<Message> (buffersLimit, buffersLimit);
			_commandBuffer = new AsyncQueue<Command> (buffersLimit, buffersLimit);
			_notificationBuffer = new AsyncQueue<Notification> (buffersLimit, buffersLimit);
			_sessionBuffer = new AsyncQueue<Session> (1, 1);
        }

        ~ChannelBase()
        {
            Dispose(false);
        }

        #endregion

        #region IChannel Members

        /// <summary>
        /// The current session transport
        /// </summary>
        public ITransport Transport { get; private set; }

        /// <summary>
        /// Remote node identifier
        /// </summary>
        public Node RemoteNode { get; protected set; }

        /// <summary>
        /// Remote node identifier
        /// </summary>
        public Node LocalNode { get; protected set; }

        /// <summary>
        /// The session Id
        /// </summary>
        public Guid SessionId { get; protected set; }

        private SessionState _state;

        /// <summary>
        /// Current session state
        /// </summary>
        public SessionState State 
        {
            get { return _state; }
            protected set
            {                
                _state = value;

                if (_state == SessionState.Established)
                {
                    _consumeTransportTask = this.ConsumeTransportAsync();
                }
            }
        }

        #endregion

        #region IMessageChannel Members

        /// <summary>
        /// Sends a message to the
        /// remote node
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">message</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual Task SendMessageAsync(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (this.State != SessionState.Established)
            {
                throw new InvalidOperationException(string.Format("Cannot send a message in the '{0}' session state", this.State));
            }
            
            return this.SendAsync(message);
        }

        /// <summary>
        /// Receives a message
        /// from the remote node.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            if (this.State != SessionState.Established)
            {
                throw new InvalidOperationException(string.Format("Cannot receive a message in the '{0}' session state", this.State));
            }

            if (_consumeTransportTask.IsFaulted)
            {
                throw _consumeTransportTask.Exception.InnerException;
            }

            var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                _channelCancellationTokenSource.Token,
                cancellationToken);

			return _messageBuffer.ReceiveAsync(combinedCancellationTokenSource.Token);
        }      

        #endregion

        #region ICommandChannel Members

        /// <summary>
        /// Sends a command envelope to
        /// the remote node
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">message</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual Task SendCommandAsync(Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (this.State != SessionState.Established)
            {
                throw new InvalidOperationException(string.Format("Cannot send a command in the '{0}' session state", this.State));
            }

            return this.SendAsync(command);
        }

        /// <summary>
        /// Receives a command
        /// from the remote node.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Task<Command> ReceiveCommandAsync(CancellationToken cancellationToken)
        {
            if (this.State != SessionState.Established)
            {
                throw new InvalidOperationException(string.Format("Cannot receive a command in the '{0}' session state", this.State));
            }

            if (_consumeTransportTask.IsFaulted)
            {
                throw _consumeTransportTask.Exception.InnerException;
            }

            var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                _channelCancellationTokenSource.Token,
                cancellationToken);				

            return _commandBuffer.ReceiveAsync(combinedCancellationTokenSource.Token);
        }

        #endregion

        #region INotificationChannel Members

        /// <summary>
        /// Sends a notification to the
        /// remote node
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">notification</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual Task SendNotificationAsync(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException("notification");
            }

            if (this.State != SessionState.Established)
            {
                throw new InvalidOperationException(string.Format("Cannot send a notification in the '{0}' session state", this.State));
            }

            return this.SendAsync(notification);
        }

        /// <summary>
        /// Receives a notification
        /// from the remote node.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Task<Notification> ReceiveNotificationAsync(CancellationToken cancellationToken)
        {
            if (this.State != SessionState.Established)
            {
                throw new InvalidOperationException(string.Format("Cannot receive a notification in the '{0}' session state", this.State));
            }

            if (_consumeTransportTask.IsFaulted)
            {
                throw _consumeTransportTask.Exception.InnerException;
            }

            var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                _channelCancellationTokenSource.Token,
                cancellationToken);

            return _notificationBuffer.ReceiveAsync(combinedCancellationTokenSource.Token);
        }

        #endregion

        #region ISessionChannel Members

        /// <summary>
        /// Sends a session change message to 
        /// the remote node. 
        /// Avoid to use this method directly. Instead,
        /// use the Server or Client channel methods.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">session</exception>
        public virtual Task SendSessionAsync(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            return this.SendAsync(session);
        }

        /// <summary>
        /// Receives a session
        /// from the remote node.
        /// Avoid to use this method directly. Instead,
        /// use the Server or Client channel methods.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual async Task<Session> ReceiveSessionAsync(CancellationToken cancellationToken)
        {
            if (this.State == SessionState.Finished)
            {
                throw new InvalidOperationException(string.Format("Cannot receive a session in the '{0}' session state", this.State));
            }
       
            if (this.State == SessionState.Established)
            {
                var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    _channelCancellationTokenSource.Token,
                    cancellationToken);     

                if (_consumeTransportTask.IsFaulted)
                {
                    throw _consumeTransportTask.Exception.InnerException;
                }

                return await _sessionBuffer.ReceiveAsync(combinedCancellationTokenSource.Token).ConfigureAwait(false);
            }
            else
            {
                var result = await this.ReceiveAsync(cancellationToken).ConfigureAwait(false);

                if (!(result is Session))
                {
                    await this.Transport.CloseAsync(_channelCancellationTokenSource.Token).ConfigureAwait(false);
                    throw new InvalidOperationException("An unexpected envelope type was received from the transport.");
                }

                return (Session)result;
            }
        }

        #endregion

        #region Private Methods

        private async Task ConsumeTransportAsync()
        {
            try
            {
                while (!_channelCancellationTokenSource.IsCancellationRequested &&
                        this.State == SessionState.Established)
                {
                    Exception exception = null;

                    try
                    {
                        var envelope = await this.ReceiveAsync(_channelCancellationTokenSource.Token).ConfigureAwait(false);

                        if (_autoReplyPings &&
                            envelope is Command &&
                            this.IsPingRequestCommand((Command)envelope))
                        {
                            var pingCommandResponse = new Command()
                            {
                                Id = envelope.Id,
                                To = envelope.From,
                                Status = CommandStatus.Success,
                                Method = CommandMethod.Get,
                                Resource = new Ping()
                            };

                            await SendCommandAsync(pingCommandResponse).ConfigureAwait(false);
                        }
                        else
                        {
                            await this.PostEnvelopeToBufferAsync(envelope);
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (ObjectDisposedException)
                    {
                        if (!_isDisposing)
                        {
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    if (exception != null)
                    {
                        var cts = new CancellationTokenSource(_sendTimeout);
                        await this.Transport.CloseAsync(cts.Token).ConfigureAwait(false);
                        throw exception;
                    }
                }
            }
            finally
            {
                if (!_channelCancellationTokenSource.IsCancellationRequested)
                {
                    _channelCancellationTokenSource.Cancel();
                }
            }
        }
        
        /// <summary>
        /// Indicates if a command is
        /// a ping request
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private bool IsPingRequestCommand(Command command)
        {
            if (command.Method == CommandMethod.Get &&
                command.Status == CommandStatus.Pending &&
                command.Uri != null)
            {
                if (command.Uri.IsRelative)
                {
                    return command.Uri.Path.Equals(UriTemplates.PING, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return command.Uri.ToUri().LocalPath.Equals(UriTemplates.PING, StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        /// <summary>
        /// Cancels the token that is associated to 
        /// the channel send and receive tasks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Transport_Closing(object sender, DeferralEventArgs e)
        {
            using (e.GetDeferral())
            {
                _channelCancellationTokenSource.Cancel();
            }            
        }

        /// <summary>
        /// Fills the buffer with the received envelope
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        private async Task PostEnvelopeToBufferAsync(Envelope envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope", "An empty envelope was received from the transport");
            }

            if (envelope is Notification)
            {
                await this.PostNotificationToBufferAsync((Notification)envelope).ConfigureAwait(false);
            }
            else if (envelope is Message)
            {
                await this.PostMessageToBufferAsync((Message)envelope).ConfigureAwait(false);
            }
            else if (envelope is Command)
            {
                await this.PostCommandToBufferAsync((Command)envelope).ConfigureAwait(false);
            }
            else if (envelope is Session)
            {
                await this.PostSessionToBufferAsync((Session)envelope).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException("Invalid or unknown envelope received by the transport.");
            }
        }

        /// <summary>
        /// Fills the buffer with the received envelope
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Task PostMessageToBufferAsync(Message message)
        {
            if (!_messageBuffer.Post(message))
            {
                throw new InvalidOperationException("Message buffer limit reached");
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Fills the buffer with the received envelope
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private Task PostCommandToBufferAsync(Command command)
        {
            if (!_commandBuffer.Post(command))
            {
                throw new InvalidOperationException("Command buffer limit reached");
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Fills the buffer with the received envelope
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        private Task PostNotificationToBufferAsync(Notification notification)
        {
            if (!_notificationBuffer.Post(notification))
            {
                throw new InvalidOperationException("Notification buffer limit reached");
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Fills the buffer with the received envelope
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private Task PostSessionToBufferAsync(Session session)
        {
            if (!_sessionBuffer.Post(session))
            {
                throw new InvalidOperationException("Session buffer limit reached");
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Sends the envelope to the transport
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        private Task SendAsync(Envelope envelope)
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource(_sendTimeout);

            if (_fillEnvelopeRecipients)
            {
                FillEnvelope(envelope, true);
            }

            return this.Transport.SendAsync(
                envelope,
                CancellationTokenSource.CreateLinkedTokenSource(
                    _channelCancellationTokenSource.Token,
                    timeoutCancellationTokenSource.Token).Token);
        }

        /// <summary>
        /// Receives an envelope from the transport
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        private async Task<Envelope> ReceiveAsync(CancellationToken cancellationToken)
        {
            var envelope = await this.Transport.ReceiveAsync(cancellationToken);

            if (_fillEnvelopeRecipients)
            {
                FillEnvelope(envelope, false);
            }

            return envelope;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Fills the envelope recipients
        /// using the session information
        /// </summary>
        /// <param name="envelope"></param>
        protected virtual void FillEnvelope(Envelope envelope, bool isSending)
        {
            if (!isSending)
            {
                // Receiving
                Node from = this.RemoteNode;
                Node to = this.LocalNode;

                if (from != null)
                {
                    if (envelope.From == null)
                    {
                        envelope.From = from.Copy();
                    }
                    else if (string.IsNullOrEmpty(envelope.From.Domain))
                    {
                        envelope.From.Domain = from.Domain;
                    }
                }

                if (to != null)
                {
                    if (envelope.To == null)
                    {
                        envelope.To = to.Copy();
                    }
                    else if (string.IsNullOrEmpty(envelope.To.Domain))
                    {
                        envelope.To.Domain = to.Domain;
                    }
                }
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isDisposing = disposing;

                if (!_channelCancellationTokenSource.IsCancellationRequested)
                {
                    _channelCancellationTokenSource.Cancel();
                }

                _channelCancellationTokenSource.Dispose();
                this.Transport.DisposeIfDisposable();
            }
        }

        #endregion

    }
}

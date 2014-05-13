using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lime.Protocol.Network
{
    /// <summary>
    /// Base class for transport
    /// implementation
    /// </summary>
    public abstract class TransportBase : ITransport
    {
        #region Constants
        public const int DEFAULT_BUFFER_SIZE = 8192;
        #endregion

        #region Private fields

        private bool _closingInvoked;
        private bool _closedInvoked;

        #endregion

        #region Constructor

        public TransportBase(int buffersize = DEFAULT_BUFFER_SIZE)
        {
            _buffer = new byte[buffersize];
            _bufferCurPos = 0;
        }
        #endregion

        #region ITransport Members

        /// <summary>
        /// Sends an envelope to 
        /// the connected node
        /// </summary>
        /// <param name="envelope">Envelope to be transported</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task SendAsync(Envelope envelope, CancellationToken cancellationToken);

        /// <summary>
        /// Receives an envelope 
        /// from the remote node.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public abstract Task<Envelope> ReceiveAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Opens the transport connection with
        /// the specified Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public abstract Task OpenAsync(Uri uri, CancellationToken cancellationToken);

        /// <summary>
        /// Closes the connection
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async virtual Task CloseAsync(CancellationToken cancellationToken)
        {
            await this.OnClosingAsync().ConfigureAwait(false);
            await this.PerformCloseAsync(cancellationToken).ConfigureAwait(false);
            this.OnClosed();
        }

        /// <summary>
        /// Enumerates the supported compression
        /// options for the transport
        /// </summary>
        /// <returns></returns>
        public virtual SessionCompression[] GetSupportedCompression()
        {
            return new SessionCompression[] { SessionCompression.None };
        }

        /// <summary>
        /// Gets the current transport 
        /// compression option
        /// </summary>
        public virtual SessionCompression Compression { get; protected set; }

        /// <summary>
        /// Defines the compression mode
        /// for the transport
        /// </summary>
        /// <param name="compression">The compression mode</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual Task SetCompressionAsync(SessionCompression compression, CancellationToken cancellationToken)
        {
            if (compression != SessionCompression.None)
            {
                throw new NotSupportedException();
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Enumerates the supported encryption
        /// options for the transport
        /// </summary>
        /// <returns></returns>
        public virtual SessionEncryption[] GetSupportedEncryption()
        {
            return new SessionEncryption[] { SessionEncryption.None };
        }

        /// <summary>
        /// Gets the current transport 
        /// encryption option
        /// </summary>
        public virtual SessionEncryption Encryption { get; protected set; }

        /// <summary>
        /// Defines the encryption mode
        /// for the transport
        /// </summary>
        /// <param name="encryption"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual Task SetEncryptionAsync(SessionEncryption encryption, CancellationToken cancellationToken)
        {
            if (encryption != SessionEncryption.None)
            {
                throw new NotSupportedException();
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Occurs when the channel is about
        /// to be closed
        /// </summary>
        public event EventHandler<DeferralEventArgs> Closing;

        /// <summary>
        /// Occurs after the connection was closed
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Closes the transport
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task PerformCloseAsync(CancellationToken cancellationToken);
        
        #endregion


        /// <summary>
        /// Raises the Closing event with
        /// a deferral to wait the event handlers
        /// to complete the execution.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnClosingAsync()
        {
            if (!_closingInvoked)
            {
                _closingInvoked = true;

                var e = new DeferralEventArgs();
                this.Closing.RaiseEvent(this, e);
                return e.WaitForDeferralsAsync();
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Raises the Closed event.
        /// </summary>
        protected virtual void OnClosed()
        {
            if (!_closedInvoked)
            {
                _closedInvoked = true;
                this.Closed.RaiseEvent(this, EventArgs.Empty);
            }
        }


        #region Private methods

        protected static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        protected static X509Certificate SelectLocalCertificate(
            object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssuers)
        {
            if (localCertificates.Count > 0)
            {
                return localCertificates[0];
            }
            return null;
        }

        #region Buffer fields

        protected byte[] _buffer;
        protected int _bufferCurPos;

        protected int _jsonStartPos;
        protected int _jsonCurPos;
        protected int _jsonStackedBrackets;
        protected bool _jsonStarted = false;

        #endregion

        /// <summary>
        /// Try to extract a JSON document
        /// from the buffer, based on the 
        /// brackets count.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        protected bool TryExtractJsonFromBuffer(out byte[] json)
        {
            if (_bufferCurPos > _buffer.Length)
            {
                throw new ArgumentException("Buffer current pos or length value is invalid");
            }

            json = null;
            int jsonLenght = 0;

            for (int i = _jsonCurPos; i < _bufferCurPos; i++)
            {
                _jsonCurPos = i + 1;

                if (_buffer[i] == '{')
                {
                    _jsonStackedBrackets++;
                    if (!_jsonStarted)
                    {
                        _jsonStartPos = i;
                        _jsonStarted = true;
                    }
                }
                else if (_buffer[i] == '}')
                {
                    _jsonStackedBrackets--;
                }

                if (_jsonStarted &&
                    _jsonStackedBrackets == 0)
                {
                    jsonLenght = i - _jsonStartPos + 1;
                    break;
                }
            }

            if (jsonLenght > 1)
            {
                json = new byte[jsonLenght];
                Array.Copy(_buffer, _jsonStartPos, json, 0, jsonLenght);

                // Shifts the buffer to the left
                _bufferCurPos -= (jsonLenght + _jsonStartPos);
                Array.Copy(_buffer, jsonLenght + _jsonStartPos, _buffer, 0, _bufferCurPos);

                _jsonCurPos = 0;
                _jsonStartPos = 0;
                _jsonStarted = false;

                return true;
            }

            return false;
        }

        #endregion        

    }
}

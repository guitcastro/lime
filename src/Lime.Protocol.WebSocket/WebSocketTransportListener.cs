using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Lime.Protocol.Server;
using vtortola.WebSockets;

namespace Lime.Protocol.WebSocket
{
    public class WebSocketTransportListener : ITransportListener
    {
        private readonly X509Certificate2 _sslCertificate;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly ITraceWriter _traceWriter;
        private WebSocketListener _webSocketListener;
        
        public WebSocketTransportListener(Uri listenerUri, X509Certificate2 sslCertificate, IEnvelopeSerializer envelopeSerializer, ITraceWriter traceWriter = null)
        {
            this.ListenerUris = new Uri[] { listenerUri };

            if (sslCertificate != null)
            {
                if (!sslCertificate.HasPrivateKey)
                {
                    throw new ArgumentException("The certificate must have a private key");
                }

                try
                {
                    // Checks if the private key is available for the current user
                    var key = sslCertificate.PrivateKey;
                }
                catch (CryptographicException ex)
                {
                    throw new SecurityException("The current user doesn't have access to the certificate private key. Use WinHttpCertCfg.exe to assign the necessary permissions.", ex);
                }
            }

            _envelopeSerializer = envelopeSerializer;
            _traceWriter = traceWriter;
        }

        public Uri[] ListenerUris { get; private set; }
        public async Task StartAsync()
        {

            if (_webSocketListener != null)
            {
                throw new InvalidOperationException("The listener is already active");
            }

            IPEndPoint listenerEndPoint;

            if (this.ListenerUris[0].IsLoopback)
            {
                listenerEndPoint = new IPEndPoint(IPAddress.Any, this.ListenerUris[0].Port);
            }
            else
            {
                var dnsEntry = await Dns.GetHostEntryAsync(this.ListenerUris[0].Host);

                if (dnsEntry.AddressList.Any(a => a.AddressFamily == AddressFamily.InterNetwork))
                {
                    listenerEndPoint = new IPEndPoint(dnsEntry.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork), this.ListenerUris[0].Port);
                }
                else
                {
                    throw new ArgumentException("Could not resolve the IPAddress of the hostname");
                }
            }
            _webSocketListener = new WebSocketListener(listenerEndPoint,new WebSocketListenerOptions()
            {
                
#if DEBUG
                PingTimeout = TimeSpan.FromMinutes(5),
                WebSocketReceiveTimeout = TimeSpan.FromMinutes(5),
                WebSocketSendTimeout = TimeSpan.FromMinutes(5)
#endif
                
            });
            var rfc6455 = new vtortola.WebSockets.Rfc6455.WebSocketFactoryRfc6455(_webSocketListener);
            _webSocketListener.Standards.RegisterStandard(rfc6455);
            _webSocketListener.Start();
        }

        public async Task<ITransport> AcceptTransportAsync(CancellationToken cancellationToken)
        {
            if (!_webSocketListener.IsStarted)
            {
                throw new InvalidOperationException("The listener was not started. Calls StartAsync first.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var webSocketClient = await _webSocketListener
                .AcceptWebSocketAsync(cancellationToken)
                .ConfigureAwait(false);

            return new WebSocketTransport(
                new WebSocketClientAdapter(webSocketClient), 
                _envelopeSerializer,_traceWriter);            
        }

        public Task StopAsync()
        {
            _webSocketListener.Stop();
            _webSocketListener = null;
            return Task.FromResult<object>(null);
        }
    }
}

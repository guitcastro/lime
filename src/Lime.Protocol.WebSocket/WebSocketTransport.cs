using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using vtortola.WebSockets;

namespace Lime.Protocol.WebSocket
{
    internal class WebSocketTransport : TransportBase, ITransport
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly ITraceWriter _traceWriter;
        private SemaphoreSlim _receiveSemaphore;
        private SemaphoreSlim _sendSemaphore;


        public WebSocketTransport(IWebSocketClient webSocketClient, IEnvelopeSerializer envelopeSerializer,
            ITraceWriter traceWriter)
        {
            _webSocketClient = webSocketClient;
            _envelopeSerializer = envelopeSerializer;
            _traceWriter = traceWriter;

            _receiveSemaphore = new SemaphoreSlim(1);
            _sendSemaphore = new SemaphoreSlim(1);
        }

        public override async Task SendAsync(Envelope envelope, CancellationToken cancellationToken)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }

            var envelopeJson = _envelopeSerializer.Serialize(envelope);


            if (_traceWriter != null &&
                _traceWriter.IsEnabled)
            {
                await _traceWriter.TraceAsync(envelopeJson, DataOperation.Send).ConfigureAwait(false);
            }

            var jsonBytes = Encoding.UTF8.GetBytes(envelopeJson);


            await _sendSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                using (var writeStream = _webSocketClient.GetWriteStream())
                {
                    await writeStream.WriteAsync(jsonBytes, 0, jsonBytes.Length, cancellationToken).ConfigureAwait(false);
                    await writeStream.CloseAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _sendSemaphore.Release();
            }


        }

        public override async Task<Envelope> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (!_webSocketClient.Connected)
            {
                throw new Exception("Invalid connection state");
            }
            Envelope envelope = null;

            await _receiveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (
                    var messageReader =
                        await _webSocketClient.GetReadStreamAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (envelope == null)
                    {
                        if (messageReader == null)
                            continue; // disconnection

                        switch (messageReader.MessageType)
                        {
                            case WebSocketMessageType.Text:
                            case WebSocketMessageType.Binary:
                                _bufferCurPos +=
                                    await
                                        messageReader.ReadAsync(_buffer, _bufferCurPos, _buffer.Length - _bufferCurPos,
                                            cancellationToken);
                                break;
                        }
                        if (_bufferCurPos >= _buffer.Length)
                        {
                            await base.CloseAsync(CancellationToken.None).ConfigureAwait(false);
                            throw new InvalidOperationException("Maximum buffer size reached");
                        }
                        byte[] json;
                        if (this.TryExtractJsonFromBuffer(out json))
                        {
                            var jsonString = Encoding.UTF8.GetString(json);

                            if (_traceWriter != null &&
                                _traceWriter.IsEnabled)
                            {
                                await _traceWriter.TraceAsync(jsonString, DataOperation.Receive).ConfigureAwait(false);
                            }

                            envelope = _envelopeSerializer.Deserialize(jsonString);
                        }

                    }
                }
            }
            finally
            {
                _receiveSemaphore.Release();
            }

            return envelope;
        }

        public override async Task OpenAsync(Uri uri, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_webSocketClient.Connected)
            {
                throw new Exception("Invalid connection state");

            }
        }

        protected override Task PerformCloseAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _webSocketClient.Close();

            return Task.FromResult<object>(null);
        }
    }
}

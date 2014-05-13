using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Microsoft.AspNet.SignalR.Client;

namespace Lime.Protocol.SignalR
{
    public class SignalRTransport : TransportBase, ITransport, IDisposable
    {
        private Connection _connection;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly ITraceWriter _traceWriter;
        private readonly BufferBlock<string> _envelopeBuffer;

        public SignalRTransport(IEnvelopeSerializer envelopeSerializer, ITraceWriter traceWriter)
        {
            _envelopeSerializer = envelopeSerializer;
            _traceWriter = traceWriter;
            _envelopeBuffer = new BufferBlock<string>();
        }

        public override async Task SendAsync(Envelope envelope, CancellationToken cancellationToken)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }

            if (_connection == null || _connection.State != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Invalid stream state. Call OpenAsync first.");
            }

            var envelopeJson = _envelopeSerializer.Serialize(envelope);

            if (_traceWriter != null &&
                _traceWriter.IsEnabled)
            {
                await _traceWriter.TraceAsync(envelopeJson, DataOperation.Send).ConfigureAwait(false);
            }

            await _connection.Send(envelopeJson).ConfigureAwait(false);
        }

        public override async Task<Envelope> ReceiveAsync(CancellationToken cancellationToken)
        {
            var data = await _envelopeBuffer.ReceiveAsync(cancellationToken);
            return _envelopeSerializer.Deserialize(data);
        }

        public override async Task OpenAsync(Uri uri, CancellationToken cancellationToken)
        {
            _connection = new Connection(uri.AbsoluteUri);
            _connection.Received += data => _envelopeBuffer.SendAsync(data, cancellationToken);
            await _connection.Start();

        }

        protected override async Task PerformCloseAsync(CancellationToken cancellationToken)
        {
            _connection.Stop();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
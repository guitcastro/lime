using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;

namespace Lime.Protocol.WebSocket
{
    internal class WebSocketClientAdapter : IWebSocketClient
    {
        private readonly vtortola.WebSockets.WebSocket _webSocket;
        private WebSocketMessageWriteStream _writerStream;

        internal WebSocketClientAdapter(vtortola.WebSockets.WebSocket webSocket)
        {
            if (webSocket == null)
            {
                throw new ArgumentNullException("webSocket");
            }

            _webSocket = webSocket;
        }

        public Task<WebSocketMessageReadStream> GetReadStreamAsync(CancellationToken cancellationToken)
        {
            return _webSocket.ReadMessageAsync(cancellationToken);
        }

        public WebSocketMessageWriteStream GetWriteStream()
        {
            _writerStream = _webSocket.CreateMessageWriter(WebSocketMessageType.Text);
            return _writerStream;
        }

        public bool Connected
        {
            get { return _webSocket.IsConnected; }
        }

        public void Close()
        {
            _webSocket.Close();
        }

        public void FlushWriteStream()
        {
            if (_writerStream != null)
            {
                _writerStream.Flush();
            }
        }
    }
}

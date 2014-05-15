using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using vtortola.WebSockets;

namespace Lime.Protocol.WebSocket
{
    public interface IWebSocketClient
    {
        /// <summary>
        /// Returns the stream used to retrieve data.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<WebSocketMessageReadStream> GetReadStreamAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns the stream used to send data.
        /// </summary>
        /// <returns></returns>
        WebSocketMessageWriteStream GetWriteStream();

        /// <summary>
        /// Gets a value indicating whether the underlying System.Net.Sockets.Socket
        /// for a System.Net.Sockets.TcpClient is connected to a remote host.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        bool Connected { get; }

        /// <summary>
        /// Disposes this System.Net.Sockets.TcpClient instance and requests that the
        /// underlying TCP connection be closed.
        /// </summary>
        void Close();
    }
}
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Shouldly;
using vtortola.WebSockets;

namespace Lime.Protocol.WebSocket.UnitTests
{
    [TestClass]
    public class WebSocketClientAdapterTests
    {
        private readonly vtortola.WebSockets.WebSocket _webSocket = Substitute.For<vtortola.WebSockets.WebSocket>();


        [TestMethod]
        public void ShouldNotInstantiateANewClientWithNullWebsocket()
        {
            try
            {
                var adapter = new WebSocketClientAdapter(null);
                Assert.Fail();
            }
            catch (ArgumentException exception)
            {
                exception.ParamName.ShouldBe("webSocket");
            }
        }

        [TestMethod]
        public void ShouldBeAbleTocreateANewInstanceWithAWebSocket()
        {
                var adapter = new WebSocketClientAdapter(_webSocket);
                adapter.ShouldNotBe(null);
        }

        [TestMethod]
        public void AdapterConnectedShouldReflectWebSocketStatus()
        {
            var adapter = new WebSocketClientAdapter(_webSocket);
            _webSocket.IsConnected.Returns(false);
            adapter.Connected.ShouldBe(false);
            _webSocket.IsConnected.Returns(true);
            adapter.Connected.ShouldBe(true);
        }

        [TestMethod]
        public void AdapterCloseShouldCloseTheWebSocket()
        {
            var adapter = new WebSocketClientAdapter(_webSocket);
            adapter.Close();
            _webSocket.Received(1).Close();
        }

        [TestMethod]
        public void AdapterGetWriteStreamShouldCreateAWriteStream()
        {
            var adapter = new WebSocketClientAdapter(_webSocket);
            var writer = Substitute.For<WebSocketMessageWriteStream>();
            _webSocket.CreateMessageWriter(Arg.Any<WebSocketMessageType>()).Returns(writer);
            var stream = adapter.GetWriteStream();
            _webSocket.Received(1).CreateMessageWriter(Arg.Any<WebSocketMessageType>());
            stream.ShouldBe(writer);
        }

        [TestMethod]
        public void AdapterGetReadStreamAsyncShouldCallWebSocketReadStreamAsync()
        {
            var adapter = new WebSocketClientAdapter(_webSocket);
            adapter.GetReadStreamAsync(new CancellationToken());
            _webSocket.Received(1).ReadMessageAsync(Arg.Any<CancellationToken>());
        }

    }
}

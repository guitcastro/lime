using System;
using System.Threading;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Shouldly;
using vtortola.WebSockets;

namespace Lime.Protocol.WebSocket.UnitTests
{
    [TestClass]
    public class WebSocketTransportTests
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly ITraceWriter _traceWriter;
        private readonly WebSocketMessageWriteStream _webSocketMessageWriteStream;

        public WebSocketTransportTests()
        {
            _webSocketClient = Substitute.For<IWebSocketClient>();
            _webSocketMessageWriteStream = Substitute.For<WebSocketMessageWriteStream>();
            _webSocketClient.GetWriteStream().Returns(_webSocketMessageWriteStream);
            _envelopeSerializer = Substitute.For<IEnvelopeSerializer>();
            _traceWriter = Substitute.For<ITraceWriter>();
        }

        [TestMethod]
        public void ICanCreateAWebSocketTransportThatImplementITransport()
        {
            var transport = new WebSocketTransport(_webSocketClient, _envelopeSerializer, _traceWriter);
            transport.ShouldNotBe(null);
            transport.ShouldBeAssignableTo<ITransport>();
        }
        [TestMethod]
        public void CannotCreateATransportWithoutWebSocketClient()
        {
            try
            {
                var transport = new WebSocketTransport(null, _envelopeSerializer, _traceWriter);
                Assert.Fail();
            }
            catch (ArgumentException exception)
            {
                exception.ParamName.ShouldBe("webSocketClient");
            }
        }

        [TestMethod]
        public void CannotCreateATransportWithoutEnvelopSerializer()
        {
            try
            {
                var transport = new WebSocketTransport(_webSocketClient, null, _traceWriter);
                Assert.Fail();
            }
            catch (ArgumentException exception)
            {
                exception.ParamName.ShouldBe("envelopeSerializer");
            }
        }

        [TestMethod]
        public void CanCreateATransportWithoutTraceWritter()
        {
            var transport = new WebSocketTransport(_webSocketClient, _envelopeSerializer, null);
            transport.ShouldNotBe(null);
            transport.ShouldBeAssignableTo<ITransport>();
        }

        [TestMethod]
        public void CantSendAsyncWithNullEnvelope()
        {
            var transport = new WebSocketTransport(_webSocketClient, _envelopeSerializer, _traceWriter);
            try
            {
                transport.SendAsync(null, new CancellationToken()).Wait();
                Assert.Fail();
            }
            catch (AggregateException aggregateException)
            {
                var innerException = aggregateException.InnerException as ArgumentNullException;
                innerException.ShouldNotBe(null);
                innerException.ParamName.ShouldBe("envelope");
            }
        }

        [TestMethod]
        public void SendAcyncShouldFollowTheWebsocketFlow()
        {
            var transport = new WebSocketTransport(_webSocketClient, _envelopeSerializer, _traceWriter);
            transport.SendAsync(Substitute.For<Envelope>(), new CancellationToken()).Wait();
            _webSocketClient.Received(1).GetWriteStream();
            _webSocketMessageWriteStream.Received(1).WriteAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
            _webSocketMessageWriteStream.Received(1).CloseAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public void ReceiveAcyncShouldThrowAExceptionWhenClientIsNotConnected()
        {
            _webSocketClient.Connected.Returns(false);
            var transport = new WebSocketTransport(_webSocketClient, _envelopeSerializer, _traceWriter);
                try
                {
                    transport.ReceiveAsync(new CancellationToken()).Wait();
                    Assert.Fail();
                }
                catch (AggregateException aggregateException)
                {
                    aggregateException.InnerException.ShouldNotBe(null);
                }
        }



    }
}

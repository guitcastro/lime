using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Network;
using Lime.Protocol.Server;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;

namespace Lime.Protocol.SignalR
{
    public class SignalRTransportListener : ITransportListener
    {
        
        public Uri[] ListenerUris { get; private set; }

        public Task StartAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ITransport> AcceptTransportAsync(CancellationToken cancellationToken)
        {
            return null;
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}

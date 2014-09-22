﻿using Lime.Protocol.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lime.Protocol.Client
{
    /// <summary>
    /// Helper extensions for the 
    /// IClientChannel interface
    /// </summary>
    public static class IClientChannelExtensions
    {
        /// <summary>
        /// Performs the session negotiation and authentication
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="compressionSelector"></param>
        /// <param name="encryptionSelector"></param>
        /// <param name="identity"></param>
        /// <param name="authenticator"></param>
        /// <param name="instance"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async static Task<Session> EstablishSessionAsync(this IClientChannel channel, Func<SessionCompression[], SessionCompression> compressionSelector,
            Func<SessionEncryption[], SessionEncryption> encryptionSelector, Identity identity, Func<AuthenticationScheme[], Authentication, Authentication> authenticator, 
            string instance, CancellationToken cancellationToken)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            if (authenticator == null)
            {
                throw new ArgumentNullException("authenticator");
            }

            var receivedSession = await channel.StartNewSessionAsync(cancellationToken).ConfigureAwait(false);

            // Session negotiation
            if (receivedSession.State == SessionState.Negotiating)
            {
                if (compressionSelector == null)
                {
                    throw new ArgumentNullException("compressionSelector");
                }

                if (encryptionSelector == null)
                {
                    throw new ArgumentNullException("encryptionSelector");
                }

                // Select options
                receivedSession = await channel.NegotiateSessionAsync(
                    compressionSelector(receivedSession.CompressionOptions),
                    encryptionSelector(receivedSession.EncryptionOptions),
                    cancellationToken).ConfigureAwait(false);

                if (receivedSession.State == SessionState.Negotiating)
                {
                    // Configure transport
                    if (receivedSession.Compression != channel.Transport.Compression)
                    {
                        await channel.Transport.SetCompressionAsync(receivedSession.Compression.Value, cancellationToken).ConfigureAwait(false);
                    }

                    if (receivedSession.Encryption != channel.Transport.Encryption)
                    {
                        await channel.Transport.SetEncryptionAsync(receivedSession.Encryption.Value, cancellationToken).ConfigureAwait(false);
                    }
                }

                // Await for authentication options
                receivedSession = await channel.ReceiveAuthenticatingSessionAsync(cancellationToken).ConfigureAwait(false);
            }

            // Session authentication
            if (receivedSession.State == SessionState.Authenticating)
            {
                do
                {
                    Authentication roundtrip = null;

                    receivedSession = await channel.AuthenticateSessionAsync(
                        identity,
                        authenticator(receivedSession.SchemeOptions, roundtrip),
                        instance,
                        cancellationToken).ConfigureAwait(false);

                    roundtrip = receivedSession.Authentication;

                } while (receivedSession.State == SessionState.Authenticating);
            }

            return receivedSession;
        }
    }
}

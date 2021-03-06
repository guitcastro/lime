﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lime.Protocol.Resources
{
    /// <summary>
    /// Represents the availability status of a node in a network. 
    /// A node can only receive envelopes from another nodes in the network 
    /// if it sets its presence to an available status (except from the server, 
    /// who always knows if a node is available or node, since this information 
    /// is enforced by the existing session). 
    /// In a new session, the node starts with an unavailable status.
    /// </summary>
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    [DebuggerDisplay("Status = {Status}, RoutingRule = {RoutingRule}")]
    public partial class Presence : Document
    {
        public const string MIME_TYPE = "application/vnd.lime.presence+json";

        public const string STATUS_KEY = "status";
        public const string MESSAGE_KEY = "message";
        public const string ROUTING_RULE_KEY = "routingRule";
        public const string LAST_SEEN_KEY = "lastSeen";
        public const string PRIORITY_KEY = "priority";
        public const string INSTANCES_KEY = "instances";

        public Presence()
            : base(MediaType.Parse(MIME_TYPE))
        {

        }

        /// <summary>
        /// The node presence status.
        /// </summary>
        [DataMember(Name = STATUS_KEY)]
        public PresenceStatus? Status { get; set; }

        /// <summary>
        /// A status message associated 
        /// to the presence status.
        /// </summary>
        [DataMember(Name = MESSAGE_KEY)]
        public string Message { get; set; }

        /// <summary>
        /// Rule to the server route envelopes 
        /// addressed to the identity. 
        /// </summary>
        [DataMember(Name = ROUTING_RULE_KEY)]
        public RoutingRule? RoutingRule { get; set; }

        [DataMember(Name = LAST_SEEN_KEY)]
        public DateTimeOffset? LastSeen { get; set; }

        /// <summary>
        /// The value of the priority for 
        /// the identityByPriority routing rule.
        /// </summary>
        [DataMember(Name = PRIORITY_KEY, EmitDefaultValue = false)]
        public int Priority { get; set; }

        /// <summary>
        /// Present instances for
        /// a identity.
        /// </summary>
        [DataMember(Name = INSTANCES_KEY)]
        public string[] Instances { get; set; }
    }

    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public enum RoutingRule
    {
        /// <summary>
        /// Only deliver envelopes addressed 
        /// to the current session instance (name@domain/instance).
        /// </summary>
        [EnumMember(Value = "instance")]
        Instance,
        /// <summary>
        /// Deliver envelopes addressed to the current session instance 
        /// (name@domain/instance) and envelopes addressed to the 
        /// identity (name@domain)
        /// </summary>
        [EnumMember(Value = "identity")]
        Identity,
        /// <summary>
        /// Deliver envelopes addressed to the current session 
        /// instance (name@domain/instance) and envelopes addressed 
        /// to the identity (name@domain) if the distance from the 
        /// origitator is the smallest among the available 
        /// nodes of the identity with this setting.
        /// </summary>
        [EnumMember(Value = "identityByDistance")]
        IdentityByDistance,
        /// <summary>
        /// Deliver envelopes addressed to the current session 
        /// instance (name@domain/instance) and envelopes addressed 
        /// to the identity (name@domain) if the value of the 
        /// priority property is the largest among the available 
        /// nodes of the identity with this setting.
        /// </summary>
        [EnumMember(Value = "identityByPriority")]
        IdentityByPriority,

        /// <summary>
        /// Deliver any envelopes addressed to the identity name@domain, 
        /// including the envelopes addressed to any specific instance.
        /// </summary>
        [EnumMember(Value = "promiscuous")]
        Promiscuous,

        /// <summary>
        /// This rule is intended to be used only for external domain authorities
        /// (gateways) and sub-domain authorities (applications), in order to 
        /// deliver envelopes addressed to their domain using the smallest distance 
        /// from the origitator among the available connected nodes for these authorities.
        /// </summary>
        [EnumMember(Value = "domainByDistance")]
        DomainByDistance
    }

    /// <summary>
    /// Possible presence status values
    /// </summary>
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public enum PresenceStatus
    {
        /// <summary>
        /// The node is not available for messaging and 
        /// SHOULD not receive any envelope by any node, 
        /// except by the connected server.
        /// </summary>
        [EnumMember]
        Unavailable,
        /// <summary>
        /// The node is available for messaging 
        /// and envelopes can be routed to the node 
        /// according to the defined routing rule.
        /// </summary>
        [EnumMember]
        Available,
        /// <summary>
        /// The node is available but the senders should notice 
        /// that it is busy and doesn't want to the disturbed 
        /// or it is on heavy load and don't want to receive 
        /// any envelope.
        /// </summary>
        [EnumMember]
        Busy,
        /// <summary>
        /// The node is available but the senders should notice
        /// that it may not be reading or processing 
        /// the received envelopes.
        /// </summary>
        [EnumMember]
        Away
    }
}

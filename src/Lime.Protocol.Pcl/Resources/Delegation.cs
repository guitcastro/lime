﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lime.Protocol.Resources
{
    /// <summary>
    /// Represents a delegation to send envelopes on behalf of another 
    /// identity of the same network. The delegation can be constrained to 
    /// specific envelope types and/or destinations. 
    /// It is associated to the issuer's session and can 
    /// be revoked through a delete command.
    /// </summary>
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public partial class Delegation : Document
    {
        public const string MIME_TYPE = "application/vnd.lime.delegation+json";

        public const string TARGET_KEY = "target";
        public const string DESTINATIONS_KEY = "destinations";
        public const string COMMANDS_KEY = "commands";
        public const string MESSAGES_KEY = "messages";

        public Delegation()
            : base(MediaType.Parse(MIME_TYPE))
        {

        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = TARGET_KEY)]
        public Node Target { get; set; }

        /// <summary>
        /// Array of destinations that the delegated 
        /// identity can originate envelopes on behalf. 
        /// </summary>
        [DataMember(Name = DESTINATIONS_KEY)]
        public Identity[] Destinations { get; set; }

        /// <summary>
        /// Command definitions for delegation. 
        /// </summary>
        [DataMember(Name = COMMANDS_KEY)]
        public DelegationCommand[] Commands { get; set; }

        /// <summary>
        /// Message definitions for delegation. 
        /// </summary>
        [DataMember(Name = MESSAGES_KEY)]
        public MediaType[] Messages { get; set; }

    }

    [DataContract]
    public partial class DelegationCommand 
    {
        public const string TYPE_KEY = "type";
        public const string METHODS_KEY = "methods";

        [DataMember(Name = TYPE_KEY)]
        public MediaType Type { get; set; }

        [DataMember(Name = METHODS_KEY)]
        public CommandMethod Methods { get; set; }
    }
}

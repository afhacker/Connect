﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Connect.Protobuf.Models
{
    public class VersionRequestMessageArgs : MessageArgsBase
    {
        public VersionRequestMessageArgs() : base((int)ProtoOAPayloadType.PROTO_OA_VERSION_REQ)
        {
        }
    }
}

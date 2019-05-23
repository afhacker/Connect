﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Connect.Protobuf.Models
{
    public class TrendbarsRequestMessageArgs : MessageArgsBase
    {
        public TrendbarsRequestMessageArgs() : base((int)ProtoOAPayloadType.PROTO_OA_GET_TRENDBARS_REQ)
        {
        }

        public long AccountId { get; set; }

        public long SymbolId { get; set; }

        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }

        public ProtoOATrendbarPeriod Period { get; set; }
    }
}

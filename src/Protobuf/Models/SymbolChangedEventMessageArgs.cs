﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Connect.Protobuf.Models
{
    public class SymbolChangedEventMessageArgs : MessageArgsBase
    {
        public SymbolChangedEventMessageArgs() : base((int)ProtoOAPayloadType.PROTO_OA_SYMBOL_CHANGED_EVENT)
        {
        }

        public long AccountId { get; set; }

        public long SymbolId { get; set; }

        public int Index { get; set; }
    }
}

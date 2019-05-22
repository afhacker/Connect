﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Connect.Protobuf.Models
{
    public class StopLimitOrderRequestMessageArgs : PendingOrderRequestMessageArgs
    {
        public StopLimitOrderRequestMessageArgs()
        {
            OrderType = ProtoOAOrderType.STOP_LIMIT;
        }

        public int SlippageInPoints { get; set; }
    }
}

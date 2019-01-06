﻿using Newtonsoft.Json;
using System;
using Connect.Common;

namespace Connect.RESTful
{
    public class CashFlow
    {
        #region Properties

        [JsonProperty("cashflowId")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string TypeText { get; set; }

        [JsonProperty("delta")]
        public long Amount { get; set; }

        [JsonProperty("balance")]
        public long Balance { get; set; }

        [JsonProperty("balanceVersion")]
        public long BalanceVersion { get; set; }

        [JsonProperty("changeTimestamp")]
        public long ChangeTimestamp { get; set; }

        public DateTimeOffset ChangeTime => DateTimeOffset.FromUnixTimeMilliseconds(ChangeTimestamp);

        public CashFlowType Type => Utility.ParseEnum(TypeText, CashFlowType.None);

        #endregion Properties

        #region Methods

        public static bool operator !=(CashFlow obj1, CashFlow obj2)
        {
            if (object.ReferenceEquals(obj1, null))
            {
                return !object.ReferenceEquals(obj2, null);
            }

            return !obj1.Equals(obj2);
        }

        public static bool operator ==(CashFlow obj1, CashFlow obj2)
        {
            if (object.ReferenceEquals(obj1, null))
            {
                return object.ReferenceEquals(obj2, null);
            }

            return obj1.Equals(obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CashFlow))
            {
                return false;
            }

            return Equals((CashFlow)obj);
        }

        public bool Equals(CashFlow other) => other?.Id == Id;

        public override int GetHashCode()
        {
            int hash = 17;

            hash = (hash * 31) + Id.GetHashCode();

            return hash;
        }

        #endregion Methods
    }
}
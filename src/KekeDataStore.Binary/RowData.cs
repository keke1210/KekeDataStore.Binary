﻿using System.Collections.Generic;

namespace KekeDataStore.Binary
{
    public interface IRowData<T> 
    {
        string ClientId { get; set; }
        Dictionary<string, T> DataCollection { get; set; }
    }

    public class RowData<T> : IRowData<T> where T : IBaseEntity
    {
        /// <summary>
        /// Identifier for the client user
        /// </summary>
        public string ClientId { get; set; }
        public Dictionary<string, T> DataCollection { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace CommonFiles
{
    /// <summary>
    /// Response, which contains key and key for our client
    /// </summary>
    public class KeyResponse
    {
        /// <summary>
        /// new Id for client - app
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Key to sign data by
        /// </summary>
        public IEnumerable<byte> Key { get; set; }
    }
}
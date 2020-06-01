using System;
using System.Globalization;

namespace KekeDataStore.Binary
{
    internal class KekeDataStoreException : Exception
    {
        public KekeDataStoreException()
        { }

        public KekeDataStoreException(string message) 
            : base(message)
        { }

        public KekeDataStoreException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        { }
    }
}

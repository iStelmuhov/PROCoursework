using System;
using System.Collections.Generic;

namespace ServiceAssembly
{
    class ClientTimeComparer : IEqualityComparer<Client>
    {
        public bool Equals(Client x, Client y)
        {
            return !DateTime.Equals(x, y);
        }

        public int GetHashCode(Client obj)
        {
            return obj.GetHashCode();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
    public class BaseEqualityComparer : IEqualityComparer<Base>
    {
        public bool Equals(Base x, Base y)
        {
            return x.GetKey().Equals(y.GetKey());
        }

        public int GetHashCode(Base obj)
        {
            return obj.GetKey().GetHashCode();
        }
    }
}
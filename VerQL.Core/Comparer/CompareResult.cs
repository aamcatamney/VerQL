using System.Collections.Generic;

namespace VerQL.Core.Comparer
{
    public class CompareResult<T>
    {
        public List<T> Additional { get; set; } = new List<T>();
        public List<T> Missing { get; set; } = new List<T>();
        public List<T> Different { get; set; } = new List<T>();
        public List<T> Same { get; set; } = new List<T>();
    }
}
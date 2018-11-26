using System.Collections.Generic;

namespace VerQL.Core.Models
{
    public class Database
    {
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<Procedure> Procedures { get; set; } = new List<Procedure>();
        public List<Function> Functions { get; set; } = new List<Function>();
        public List<View> Views { get; set; } = new List<View>();
    }
}
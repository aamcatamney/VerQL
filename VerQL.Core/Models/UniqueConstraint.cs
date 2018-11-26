using System.Collections.Generic;

namespace VerQL.Core.Models
{
    public class UniqueConstraint
    {
        public string Name { get; set; }
        public bool Clustered { get; set; } = false;
        public List<UniqueColumn> Columns { get; set; } = new List<UniqueColumn>();
    }
}
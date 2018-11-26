using System.Collections.Generic;

namespace VerQL.Core.Models
{
    public class Table
    {
        public string Schema { get; set; } = "dbo";
        public string Name { get; set; }
        public List<Column> Columns { get; set; } = new List<Column>();
        public List<ForeignKeyConstraint> ForeignKeys { get; set; } = new List<ForeignKeyConstraint>();
        public PrimaryKeyConstraint PrimaryKeyConstraint { get; set; }
        public List<UniqueConstraint> UniqueConstraints { get; set; } = new List<UniqueConstraint>();
    }
}
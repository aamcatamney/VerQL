using System.Collections.Generic;

namespace VerQL.Core.Models
{
  public class ForeignKeyConstraint : TableBase
  {
    public string Name { get; set; }
    public string ReferenceSchema { get; set; } = "dbo";
    public string ReferenceTable { get; set; }
    public List<string> Columns { get; set; } = new List<string>();
    public List<string> ReferenceColumns { get; set; } = new List<string>();
    public eCascadeAction OnDelete { get; set; } = eCascadeAction.NO_ACTION;
    public eCascadeAction OnUpdate { get; set; } = eCascadeAction.NO_ACTION;
  }
}
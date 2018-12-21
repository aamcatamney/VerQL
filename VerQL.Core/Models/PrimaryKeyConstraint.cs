using System.Collections.Generic;

namespace VerQL.Core.Models
{
  public class PrimaryKeyConstraint : TableBase
  {
    public string Name { get; set; }
    public bool Clustered { get; set; } = true;
    public List<PrimaryKeyColumn> Columns { get; set; } = new List<PrimaryKeyColumn>();
    public int FillFactor { get; set; } = 0;
  }
}
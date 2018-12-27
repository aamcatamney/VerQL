using System.Collections.Generic;

namespace VerQL.Core.Models
{
  public class Index : TableBase
  {
    public string Name { get; set; }
    public bool IsUnique { get; set; }
    public List<IndexColumn> Columns { get; set; } = new List<IndexColumn>();
    public List<string> IncludedColumns { get; set; } = new List<string>();
  }
}
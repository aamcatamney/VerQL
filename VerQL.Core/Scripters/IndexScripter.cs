using System.Collections.Generic;
using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
  public class IndexScripter
  {
    public string ScriptIndexCreate(Index index)
    {
      var sb = new StringBuilder();
      sb.Append($"CREATE INDEX [{index.Name}] ON [{index.TableSchema}].[{index.TableName}] (");

      var cols = new List<string>();
      foreach (var col in index.Columns)
      {
        var c = $"[{col.Name}] ";
        if (col.Asc)
        {
          c += "ASC";
        }
        else
        {
          c += "DESC";
        }
        cols.Add(c);
      }
      sb.Append(string.Join(", ", cols));
      sb.Append(")");

      if (index.IncludedColumns.Any())
      {
        sb.Append($" INCLUDE ({string.Join(", ", index.IncludedColumns.Select(c => $"[{c}]"))})");
      }

      return sb.ToString().Trim();
    }
  }
}
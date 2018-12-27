using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
  public class IndexEqualityComparer : IEqualityComparer<Index>
  {
    public bool Equals(Index x, Index y)
    {
      return (x.Name ?? "").Equals(y.Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableSchema ?? "").Equals(y.TableSchema ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableName ?? "").Equals(y.TableName ?? "", StringComparison.OrdinalIgnoreCase) &&
             x.IsUnique == y.IsUnique &&
             ColumnsMatch(x, y);
    }

    private string GetColHashCode(Index uc)
    {
      return $"({string.Join("|", uc.Columns.Select(c => $"{c.Name}[{c.Asc}]"))}|{string.Join("|", uc.IncludedColumns)})";
    }

    private bool ColumnsMatch(Index x, Index y)
    {
      var xs = GetColHashCode(x);
      var ys = GetColHashCode(y);
      return xs.Equals(ys, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Index obj)
    {
      return $"{obj.TableSchema}|{obj.TableName}|{obj.Name}|{obj.IsUnique}|{GetColHashCode(obj)}".GetHashCode();
    }
  }
}
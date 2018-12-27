using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
  public class UniqueConstraintEqualityComparer : IEqualityComparer<UniqueConstraint>
  {
    public bool Equals(UniqueConstraint x, UniqueConstraint y)
    {
      return (x.Name ?? "").Equals(y.Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableSchema ?? "").Equals(y.TableSchema ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableName ?? "").Equals(y.TableName ?? "", StringComparison.OrdinalIgnoreCase) &&
             x.Clustered == y.Clustered &&
             ColumnsMatch(x, y);
    }

    private string GetColHashCode(UniqueConstraint uc)
    {
      return string.Join("|", uc.Columns.Select(c => $"{c.Name}[{c.Asc}]"));
    }

    private bool ColumnsMatch(UniqueConstraint x, UniqueConstraint y)
    {
      var xs = GetColHashCode(x);
      var ys = GetColHashCode(y);
      return xs.Equals(ys, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(UniqueConstraint obj)
    {
      return $"{obj.TableSchema}|{obj.TableName}|{obj.Name}|{obj.Clustered}|{GetColHashCode(obj)}".GetHashCode();
    }
  }
}
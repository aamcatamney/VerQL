using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
  public class PrimaryKeyConstraintEqualityComparer : IEqualityComparer<PrimaryKeyConstraint>
  {
    public bool Equals(PrimaryKeyConstraint x, PrimaryKeyConstraint y)
    {
      return (x.Name ?? "").Equals(y.Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableSchema ?? "").Equals(y.TableSchema ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableName ?? "").Equals(y.TableName ?? "", StringComparison.OrdinalIgnoreCase) &&
             x.Clustered == y.Clustered &&
             x.FillFactor == y.FillFactor &&
             ColumnsMatch(x, y);
    }

    private string GetColHashCode(PrimaryKeyConstraint uc)
    {
      return string.Join("|", uc.Columns.Select(c => $"{c.Name}[{c.Asc}]"));
    }

    private bool ColumnsMatch(PrimaryKeyConstraint x, PrimaryKeyConstraint y)
    {
      var xs = GetColHashCode(x);
      var ys = GetColHashCode(y);
      return xs.Equals(ys, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(PrimaryKeyConstraint obj)
    {
      return $"{obj.TableSchema}|{obj.TableName}|{obj.Name}|{obj.Clustered}|{obj.FillFactor}|{GetColHashCode(obj)}".GetHashCode();
    }
  }
}
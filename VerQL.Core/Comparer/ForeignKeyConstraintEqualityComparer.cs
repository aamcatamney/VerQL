using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
  public class ForeignKeyConstraintEqualityComparer : IEqualityComparer<ForeignKeyConstraint>
  {
    public bool Equals(ForeignKeyConstraint x, ForeignKeyConstraint y)
    {
      return (x.Name ?? "").Equals(y.Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableSchema ?? "").Equals(y.TableSchema ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.TableName ?? "").Equals(y.TableName ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.ReferenceTable ?? "").Equals(y.ReferenceTable ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.ReferenceSchema ?? "").Equals(y.ReferenceSchema ?? "", StringComparison.OrdinalIgnoreCase) &&
             x.OnDelete == y.OnDelete &&
             x.OnUpdate == y.OnUpdate &&
             ColumnsMatch(x, y);
    }

    private string GetColHashCode(ForeignKeyConstraint uc)
    {
      return $"({string.Join("|", uc.Columns)})({string.Join("|", uc.ReferenceColumns)})";
    }

    private bool ColumnsMatch(ForeignKeyConstraint x, ForeignKeyConstraint y)
    {
      var xs = GetColHashCode(x);
      var ys = GetColHashCode(y);
      return xs.Equals(ys, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(ForeignKeyConstraint obj)
    {
      return $"{obj.TableSchema}|{obj.TableName}|{obj.Name}|{obj.ReferenceSchema}|{obj.ReferenceTable}|{obj.OnDelete}|{obj.OnUpdate}|{GetColHashCode(obj)}".GetHashCode();
    }
  }
}
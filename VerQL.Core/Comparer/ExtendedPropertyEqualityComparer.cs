using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
  public class ExtendedPropertyEqualityComparer : IEqualityComparer<ExtendedProperty>
  {
    public bool Equals(ExtendedProperty x, ExtendedProperty y)
    {
      return (x.Name ?? "").Equals(y.Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Value ?? "").Equals(y.Value ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level0Name ?? "").Equals(y.Level0Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level0Type ?? "").Equals(y.Level0Type ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level1Name ?? "").Equals(y.Level1Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level1Type ?? "").Equals(y.Level1Type ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level2Name ?? "").Equals(y.Level2Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level2Type ?? "").Equals(y.Level2Type ?? "", StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(ExtendedProperty obj)
    {
      return $"{obj.Name}|{obj.Value}|{obj.Level0Name}|{obj.Level0Type}|{obj.Level1Name}|{obj.Level1Type}|{obj.Level2Name}|{obj.Level2Type}".GetHashCode();
    }
  }
}
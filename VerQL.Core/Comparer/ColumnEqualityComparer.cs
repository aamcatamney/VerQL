using System;
using System.Collections;
using System.Collections.Generic;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
    public class ColumnEqualityComparer : IEqualityComparer<Column>
    {
        public bool Equals(Column x, Column y)
        {
            return (x.Name ?? "").Equals(y.Name ?? "", StringComparison.OrdinalIgnoreCase) &&
                   (x.Type ?? "").Equals(y.Type ?? "", StringComparison.OrdinalIgnoreCase) &&
                    x.MaxLength.Equals(y.MaxLength) &&
                    x.IsNullable.Equals(y.IsNullable) &&
                    x.IsComputed.Equals(y.IsComputed) &&
                    (x.ComputedText ?? "").Equals(y.ComputedText ?? "", StringComparison.OrdinalIgnoreCase) &&
                    x.IsPrimaryKey.Equals(y.IsPrimaryKey) &&
                    x.IsUnique.Equals(y.IsUnique) &&
                    x.IsIdentity.Equals(y.IsIdentity) &&
                    x.IsUserDefined.Equals(y.IsUserDefined) &&
                    x.SeedValue.Equals(y.SeedValue) &&
                    x.IncrementValue.Equals(y.IncrementValue) &&
                    x.HasDefault.Equals(y.HasDefault) &&
                    (x.DefaultName ?? "").Equals(y.DefaultName ?? "", StringComparison.OrdinalIgnoreCase) &&
                    DefaultTextEqual(x, y);
        }

        private bool DefaultTextEqual(Column x, Column y)
        {
            var xs = x.DefaultText ?? string.Empty;
            var ys = y.DefaultText ?? string.Empty;
            if (x.IsNumericType())
            {
                if (!xs.StartsWith("((")) xs = $"(({xs}";
                if (!xs.EndsWith("))")) xs = $"{xs}))";
            }
            else
            {
                if (!xs.StartsWith("(")) xs = $"({xs}";
                if (!xs.EndsWith(")")) xs = $"{xs})";
            }
            if (y.IsNumericType())
            {
                if (!ys.StartsWith("((")) ys = $"(({ys}";
                if (!ys.EndsWith("))")) ys = $"{ys}))";
            }
            else
            {
                if (!ys.StartsWith("(")) ys = $"({ys}";
                if (!ys.EndsWith(")")) ys = $"{ys})";
            }
            return xs.Equals(ys, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Column obj)
        {
            return $"{obj.Name}|{obj.Type}|{obj.MaxLength}|{obj.IsNullable}|{obj.IsComputed}|{obj.ComputedText}|{obj.IsPrimaryKey}|{obj.IsUnique}|{obj.IsIdentity}|{obj.IsUserDefined}|{obj.SeedValue}|{obj.IncrementValue}|{obj.HasDefault}|{obj.DefaultText}|{obj.DefaultText}".GetHashCode();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
  public class ColumnScripter
  {
    public string ScriptCreate(Column column, bool StructureOnly = false)
    {
      var sb = new StringBuilder();

      sb.Append($"[{column.Name}] ");

      if (column.IsComputed)
      {
        sb.Append($"AS {column.ComputedText} ");
      }
      else
      {
        sb.Append($"{column.Type} ");
      }

      if (column.MaxLength != 0)
      {
        if (column.MaxLength == -1)
        {
          sb.Append("(MAX) ");
        }
        else
        {
          sb.Append($"({column.MaxLength}) ");
        }
      }

      if (column.IsIdentity && !StructureOnly)
      {
        sb.Append("IDENTITY");
        if (column.SeedValue != 1 || column.IncrementValue != 1)
        {
          sb.Append($"({column.SeedValue}, {column.IncrementValue})");
        }
        sb.Append(" ");
      }

      if (column.IsPrimaryKey)
      {
        sb.Append("PRIMARY KEY ");
      }

      if (column.IsUnique && !StructureOnly)
      {
        sb.Append("UNIQUE ");
      }

      if (!column.IsComputed)
      {
        if (!column.IsNullable)
        {
          sb.Append("NOT ");
        }

        sb.Append("NULL ");
      }

      if (column.HasDefault && !StructureOnly)
      {
        if (!string.IsNullOrEmpty(column.DefaultName))
        {
          sb.Append($"CONSTRAINT [{column.DefaultName}] ");
        }
        sb.Append($"DEFAULT {column.DefaultText}");
      }

      return sb.ToString().Trim();
    }

    public List<string> ScriptAlter(Table table, Column left, Column right)
    {
      var alts = new List<string>();
      if (!left.Type.Equals(right.Type, StringComparison.OrdinalIgnoreCase)
          || left.MaxLength != right.MaxLength || left.IsNullable != right.IsNullable
          || left.IsPrimaryKey != right.IsPrimaryKey)
      {
        alts.Add($"ALTER TABLE [{table.Schema}].[{table.Name}] ALTER COLUMN {ScriptCreate(right, true)}");
      }

      // Unique
      if (!left.IsUnique && right.IsUnique)
      {
        alts.Add($"ALTER TABLE [{table.Schema}].[{table.Name}] ADD UNIQUE ([{right.Name}])");
      }
      return alts;
    }

    public string CheckExists(Table table, Column col)
    {
      return $"if exists (select * from sys.columns where object_id = OBJECT_ID('[{table.Schema}].[{table.Name}]', 'U') and name = '{col.Name}')";
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
  public class TableScripter
  {
    public string ScriptCreate(Table table, PrimaryKeyConstraint primaryKeyConstraint, List<Column> columns, List<UniqueConstraint> uniqueConstraints)
    {
      var sb = new StringBuilder();
      sb.AppendLine($"CREATE TABLE [{table.Schema}].[{table.Name}] (");

      sb.Append(string.Join(",\n", columns.Select(c => new ColumnScripter().ScriptCreate(c))));

      if (primaryKeyConstraint != null)
      {
        sb.AppendLine(",");
        sb.Append(new ConstraintScripter().ScriptPrimaryKeyCreate(primaryKeyConstraint));
      }

      foreach (var u in uniqueConstraints)
      {
        sb.AppendLine(",");
        sb.Append(new ConstraintScripter().ScriptUniqueCreate(u));
      }

      sb.AppendLine(");");
      return sb.ToString();
    }

    public string ScriptAddMissing(Table table, List<Column> columns)
    {
      var sb = new StringBuilder();
      foreach (var c in columns)
      {
        sb.AppendLine(new ColumnScripter().CheckExists(table, c));
        sb.AppendLine($"ALTER TABLE [{table.Schema}].[{table.Name}] ADD {new ColumnScripter().ScriptCreate(c)};");
      }
      return sb.ToString();
    }

    public string ScriptDropColumns(Table table, List<Column> columns)
    {
      var sb = new StringBuilder();
      foreach (var c in columns)
      {
        sb.AppendLine(new ColumnScripter().CheckExists(table, c));
        sb.AppendLine($"ALTER TABLE [{table.Schema}].[{table.Name}] DROP COLUMN [{c.Name}];");
      }
      return sb.ToString();
    }

    public string ScriptAlter(Table table, List<Tuple<Column, Column>> columns)
    {
      var sb = new StringBuilder();
      foreach (var c in columns)
      {
        foreach (var line in new ColumnScripter().ScriptAlter(table, c.Item1, c.Item2))
        {
          sb.AppendLine(line);
        }
      }
      return sb.ToString();
    }
  }
}
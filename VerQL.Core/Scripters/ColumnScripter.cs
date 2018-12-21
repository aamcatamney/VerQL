using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
  public class ColumnScripter
  {
    public string ScriptCreate(Column column)
    {
      var sb = new StringBuilder();

      sb.Append($"[{column.Name}] {column.Type} ");

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

      if (column.IsIdentity)
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

      if (column.IsUnique)
      {
        sb.Append("UNIQUE ");
      }

      if (!column.IsNullable)
      {
        sb.Append("NOT ");
      }

      sb.Append("NULL ");

      if (column.HasDefault)
      {
        if (!string.IsNullOrEmpty(column.DefaultName))
        {
          sb.Append($"CONSTRAINT [{column.DefaultName}] ");
        }
        sb.Append($"DEFAULT {column.DefaultText}");
      }

      return sb.ToString().Trim();
    }
  }
}
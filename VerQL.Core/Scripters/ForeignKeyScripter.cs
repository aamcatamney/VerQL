using System.Collections.Generic;
using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
  public class ForeignKeyScripter
  {
    public string ScriptForeignKeyCreate(ForeignKeyConstraint foreignKeyConstraint)
    {
      var sb = new StringBuilder();
      sb.AppendLine($"ALTER TABLE [{foreignKeyConstraint.TableSchema}].[{foreignKeyConstraint.TableName}]");
      sb.Append($"ADD CONSTRAINT [{foreignKeyConstraint.Name}] FOREIGN KEY (");
      sb.Append(string.Join(",", foreignKeyConstraint.Columns.Select(c => $"[{c}]")));
      sb.AppendLine(")");

      sb.Append($"REFERENCES [{foreignKeyConstraint.ReferenceSchema}].[{foreignKeyConstraint.ReferenceTable}] (");
      sb.Append(string.Join(",", foreignKeyConstraint.ReferenceColumns.Select(c => $"[{c}]")));
      sb.AppendLine(")");

      if (foreignKeyConstraint.OnDelete == eCascadeAction.CASCADE)
      {
        sb.AppendLine("ON DELETE CASCADE");
      }
      else if (foreignKeyConstraint.OnDelete == eCascadeAction.SET_DEFAULT)
      {
        sb.AppendLine("ON DELETE SET DEFAULT");
      }
      else if (foreignKeyConstraint.OnDelete == eCascadeAction.SET_NULL)
      {
        sb.AppendLine("ON DELETE SET NULL");
      }

      if (foreignKeyConstraint.OnUpdate == eCascadeAction.CASCADE)
      {
        sb.AppendLine("ON UPDATE CASCADE");
      }
      else if (foreignKeyConstraint.OnUpdate == eCascadeAction.SET_DEFAULT)
      {
        sb.AppendLine("ON UPDATE SET DEFAULT");
      }
      else if (foreignKeyConstraint.OnUpdate == eCascadeAction.SET_NULL)
      {
        sb.AppendLine("ON UPDATE SET NULL");
      }

      return sb.ToString().Trim();
    }
  }
}
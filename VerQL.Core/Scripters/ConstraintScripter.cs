using System.Collections.Generic;
using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class ConstraintScripter
    {
        public string ScriptPrimaryKeyCreate(PrimaryKeyConstraint primaryKeyConstraint)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(primaryKeyConstraint.Name))
            {
                sb.Append($"PRIMARY KEY ");
            }
            else
            {
                sb.Append($"CONSTRAINT [{primaryKeyConstraint.Name}] PRIMARY KEY ");
            }

            if (!primaryKeyConstraint.Clustered)
            {
                sb.Append("NONCLUSTERED ");
            }
            else
            {
                sb.Append("CLUSTERED ");
            }

            sb.Append("(");
            var cols = new List<string>();
            foreach (var col in primaryKeyConstraint.Columns)
            {
                var c = $"[{col.Name}] ";
                if (col.Asc)
                {
                    c += "ASC";
                }
                else
                {
                    c += "DESC";
                }
                cols.Add(c);
            }
            sb.Append(string.Join(", ", cols));
            sb.Append(") ");

            if (primaryKeyConstraint.FillFactor > 0)
            {
                sb.Append($"WITH (FILLFACTOR = {primaryKeyConstraint.FillFactor}) ");
            }

            return sb.ToString().Trim();
        }

        public string ScriptUniqueCreate(UniqueConstraint uniqueConstraint)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(uniqueConstraint.Name))
            {
                sb.Append($"UNIQUE ");
            }
            else
            {
                sb.Append($"CONSTRAINT [{uniqueConstraint.Name}] UNIQUE ");
            }

            if (uniqueConstraint.Clustered)
            {
                sb.Append("CLUSTERED ");
            }

            sb.Append("(");
            var cols = new List<string>();
            foreach (var col in uniqueConstraint.Columns)
            {
                var c = $"[{col.Name}] ";
                if (col.Asc)
                {
                    c += "ASC";
                }
                else
                {
                    c += "DESC";
                }
                cols.Add(c);
            }
            sb.Append(string.Join(", ", cols));
            sb.Append(")");

            return sb.ToString();
        }
    }
}
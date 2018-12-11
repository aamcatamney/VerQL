using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class TableScripter
    {
        public string ScriptCreate(Table table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{table.Schema}].[{table.Name}] (");

            sb.Append(string.Join(",\n", table.Columns.Select(c => new ColumnScripter().ScriptCreate(c))));

            if (table.PrimaryKeyConstraint != null)
            {
                sb.AppendLine(",");
                sb.Append(new ConstraintScripter().ScriptPrimaryKeyCreate(table.PrimaryKeyConstraint));
            }

            foreach (var u in table.UniqueConstraints)
            {
                sb.AppendLine(",");
                sb.Append(new ConstraintScripter().ScriptUniqueCreate(u));
            }

            sb.AppendLine(");");
            return sb.ToString();
        }
    }
}
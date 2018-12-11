using System.Linq;
using System.Text;
using VerQL.Core.Models;

namespace VerQL.Core.Scripters
{
    public class SchemaScripter
    {
        public string ScriptCreate(Schema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE SCHEMA [{schema.Name}]");
            return sb.ToString();
        }
    }
}
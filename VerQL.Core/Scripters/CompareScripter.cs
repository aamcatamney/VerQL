using System.Collections.Generic;
using System.Text;
using VerQL.Core.Comparer;

namespace VerQL.Core.Scripters
{
    public class CompareScripter
    {
        public string ScriptCompareAsFile(CompareResponse compareResponse)
        {
            var sb = new StringBuilder();

            foreach (var s in ScriptCompareAsStatments(compareResponse))
            {
                sb.AppendLine(s);
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public List<string> ScriptCompareAsStatments(CompareResponse compareResponse)
        {
            var statments = new List<string>();

            foreach (var s in compareResponse.Schemas.Missing)
            {
                statments.Add(new SchemaScripter().ScriptCreate(s));
            }

            foreach (var ut in compareResponse.UserTypes.Missing)
            {
                statments.Add(new UserTypeScripter().ScriptCreate(ut));
            }

            foreach (var tbl in compareResponse.Tables.Missing)
            {
                statments.Add(new TableScripter().ScriptCreate(tbl));
            }

            foreach (var v in compareResponse.Views.Missing)
            {
                statments.Add(new ViewScripter().ScriptCreate(v));
            }

            foreach (var f in compareResponse.Functions.Missing)
            {
                statments.Add(new FunctionScripter().ScriptCreate(f));
            }

            foreach (var sp in compareResponse.Procedures.Missing)
            {
                statments.Add(new ProcedureScripter().ScriptCreate(sp));
            }

            return statments;
        }
    }
}
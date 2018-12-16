using System.Collections.Generic;
using System.Linq;
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
            var stage1 = new List<string>();
            var stage2 = new List<string>();
            var stage3 = new List<string>();

            foreach (var s in compareResponse.Schemas.Missing)
            {
                stage1.Add(new SchemaScripter().ScriptCreate(s));
            }

            foreach (var ut in compareResponse.UserTypes.Missing)
            {
                stage1.Add(new UserTypeScripter().ScriptCreate(ut));
            }

            foreach (var tbl in compareResponse.Tables.Missing)
            {
                stage1.Add(new TableScripter().ScriptCreate(tbl));
                foreach (var fk in tbl.ForeignKeys)
                {
                    stage2.Add(new ForeignKeyScripter().ScriptForeignKeyCreate(tbl, fk));
                }
            }

            foreach (var t in compareResponse.Triggers.Missing)
            {
                stage2.Add(new DefinitionBasedScripter().ScriptCreate(t));
            }

            foreach (var v in compareResponse.Views.Missing)
            {
                stage3.Add(new DefinitionBasedScripter().ScriptCreate(v));
            }

            foreach (var f in compareResponse.Functions.Missing)
            {
                stage3.Add(new DefinitionBasedScripter().ScriptCreate(f));
            }

            foreach (var sp in compareResponse.Procedures.Missing)
            {
                stage3.Add(new DefinitionBasedScripter().ScriptCreate(sp));
            }

            return (new[] { stage1, stage2, stage3 }).SelectMany(s => s).ToList();
        }
    }
}
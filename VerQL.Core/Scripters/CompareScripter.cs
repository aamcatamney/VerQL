using System.Collections.Generic;
using System.Linq;
using System.Text;
using VerQL.Core.Comparer;
using VerQL.Core.Utils;

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
        var cols = compareResponse.Columns.Missing.FilterByTable(tbl).ToList();
        var pk = compareResponse.PrimaryKeyConstraints.Missing.FilterByTable(tbl).FirstOrDefault();
        var uq = compareResponse.UniqueConstraints.Missing.FilterByTable(tbl).ToList();
        stage1.Add(new TableScripter().ScriptCreate(tbl, pk, cols, uq));
      }

      foreach (var tbl in compareResponse.Tables.Same)
      {
        var missingCols = compareResponse.Columns.Missing.FilterByTable(tbl).ToList();
        var diffCols = compareResponse.Columns.Different.FilterByTable(tbl).ToList();
        //var pk = compareResponse.PrimaryKeyConstraints.Missing.FilterByTable(tbl).FirstOrDefault();
        //var uq = compareResponse.UniqueConstraints.Missing.FilterByTable(tbl).ToList();
        if (missingCols.Any())
        {
          stage1.Add(new TableScripter().ScriptAddMissing(tbl, missingCols));
        }
        if (diffCols.Any())
        {
          stage1.Add(new TableScripter().ScriptAlter(tbl, diffCols));
        }
      }

      foreach (var fk in compareResponse.ForeignKeyConstraints.Missing)
      {
        stage2.Add(new ForeignKeyScripter().ScriptForeignKeyCreate(fk));
      }

      foreach (var t in compareResponse.Triggers.Missing)
      {
        stage2.Add(new DefinitionBasedScripter().ScriptCreate(t));
      }

      foreach (var t in compareResponse.Triggers.Different)
      {
        stage2.Add(new DefinitionBasedScripter().ScriptAlter(t.Item2));
      }

      foreach (var v in compareResponse.Views.Missing)
      {
        stage3.Add(new DefinitionBasedScripter().ScriptCreate(v));
      }

      foreach (var v in compareResponse.Views.Different)
      {
        stage3.Add(new DefinitionBasedScripter().ScriptAlter(v.Item2));
      }

      foreach (var f in compareResponse.Functions.Missing)
      {
        stage3.Add(new DefinitionBasedScripter().ScriptCreate(f));
      }

      foreach (var f in compareResponse.Functions.Different)
      {
        stage3.Add(new DefinitionBasedScripter().ScriptAlter(f.Item2));
      }

      foreach (var sp in compareResponse.Procedures.Missing)
      {
        stage3.Add(new DefinitionBasedScripter().ScriptCreate(sp));
      }

      foreach (var sp in compareResponse.Procedures.Different)
      {
        stage3.Add(new DefinitionBasedScripter().ScriptAlter(sp.Item2));
      }

      return (new[] { stage1, stage2, stage3 }).SelectMany(s => s).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }
  }
}
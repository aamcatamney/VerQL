using System.Collections.Generic;
using System.Linq;
using System.Text;
using VerQL.Core.Comparer;
using VerQL.Core.Utils;

namespace VerQL.Core.Scripters
{
  public class CompareScripter
  {
    private readonly Dictionary<string, string> vars;
    private readonly ScriptingOptions options;
    private List<string> Stage1 = new List<string>();
    private List<string> Stage2 = new List<string>();
    private List<string> Stage3 = new List<string>();

    public CompareScripter(ScriptingOptions options, Dictionary<string, string> vars)
    {
      this.options = options;
      this.vars = vars;
    }

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

    private void ScriptMissing(CompareResponse compareResponse)
    {
      foreach (var s in compareResponse.Schemas.Missing)
      {
        Stage1.Add(new SchemaScripter().ScriptCreate(s));
      }

      foreach (var ut in compareResponse.UserTypes.Missing)
      {
        Stage1.Add(new UserTypeScripter().ScriptCreate(ut));
      }

      foreach (var tbl in compareResponse.Tables.Missing)
      {
        var cols = compareResponse.Columns.Missing.FilterByTable(tbl).ToList();
        var pk = compareResponse.PrimaryKeyConstraints.Missing.FilterByTable(tbl).FirstOrDefault();
        var uq = compareResponse.UniqueConstraints.Missing.FilterByTable(tbl).ToList();
        var indexs = compareResponse.Indexs.Missing.FilterByTable(tbl).ToList();
        Stage1.Add(new TableScripter().ScriptCreate(tbl, pk, cols, uq));
        foreach (var i in compareResponse.Indexs.Missing.FilterByTable(tbl))
        {
          Stage2.Add(new IndexScripter().ScriptIndexCreate(i));
        }
      }

      foreach (var tbl in compareResponse.Tables.Same)
      {
        var missingCols = compareResponse.Columns.Missing.FilterByTable(tbl).ToList();
        //var pk = compareResponse.PrimaryKeyConstraints.Missing.FilterByTable(tbl).FirstOrDefault();
        //var uq = compareResponse.UniqueConstraints.Missing.FilterByTable(tbl).ToList();
        if (missingCols.Any())
        {
          Stage1.Add(new TableScripter().ScriptAddMissing(tbl, missingCols));
        }
        foreach (var i in compareResponse.Indexs.Missing.FilterByTable(tbl))
        {
          Stage2.Add(new IndexScripter().ScriptIndexCreate(i));
        }
      }

      foreach (var fk in compareResponse.ForeignKeyConstraints.Missing)
      {
        Stage2.Add(new ForeignKeyScripter().ScriptForeignKeyCreate(fk));
      }

      foreach (var t in compareResponse.Triggers.Missing)
      {
        Stage2.Add(new DefinitionBasedScripter(vars).ScriptCreate(t));
      }

      foreach (var v in compareResponse.Views.Missing)
      {
        Stage3.Add(new DefinitionBasedScripter(vars).ScriptCreate(v));
      }

      foreach (var f in compareResponse.Functions.Missing)
      {
        Stage3.Add(new DefinitionBasedScripter(vars).ScriptCreate(f));
      }

      foreach (var sp in compareResponse.Procedures.Missing)
      {
        Stage3.Add(new DefinitionBasedScripter(vars).ScriptCreate(sp));
      }

      foreach (var ep in compareResponse.ExtendedProperties.Missing)
      {
        Stage3.Add(new ExtendedPropertyScripter().ScriptCreate(ep));
      }
    }

    private void ScriptDifferent(CompareResponse compareResponse)
    {
      foreach (var tbl in compareResponse.Tables.Same)
      {
        var diffCols = compareResponse.Columns.Different.FilterByTable(tbl).ToList();
        //var pk = compareResponse.PrimaryKeyConstraints.Missing.FilterByTable(tbl).FirstOrDefault();
        //var uq = compareResponse.UniqueConstraints.Missing.FilterByTable(tbl).ToList();
        if (diffCols.Any())
        {
          Stage1.Add(new TableScripter().ScriptAlter(tbl, diffCols));
        }

        foreach (var i in compareResponse.Indexs.Different.FilterByTable(tbl))
        {
          Stage2.Add(new IndexScripter().ScriptDrop(i.Item2));
          Stage2.Add(new IndexScripter().ScriptIndexCreate(i.Item2));
        }
      }

      foreach (var t in compareResponse.Triggers.Different)
      {
        Stage2.Add(new DefinitionBasedScripter(vars).ScriptAlter(t.Item2));
      }

      foreach (var v in compareResponse.Views.Different)
      {
        Stage3.Add(new DefinitionBasedScripter(vars).ScriptAlter(v.Item2));
      }

      foreach (var f in compareResponse.Functions.Different)
      {
        Stage3.Add(new DefinitionBasedScripter(vars).ScriptAlter(f.Item2));
      }

      foreach (var sp in compareResponse.Procedures.Different)
      {
        Stage3.Add(new DefinitionBasedScripter(vars).ScriptAlter(sp.Item2));
      }

      foreach (var ep in compareResponse.ExtendedProperties.Different)
      {
        Stage3.Add(new ExtendedPropertyScripter().ScriptDrop(ep.Item2));
        Stage3.Add(new ExtendedPropertyScripter().ScriptCreate(ep.Item2));
      }
    }

    private void ScriptAdditional(CompareResponse compareResponse)
    {
      foreach (var ep in compareResponse.ExtendedProperties.Additional)
      {
        Stage3.Add(new ExtendedPropertyScripter().ScriptDrop(ep));
      }

      if (options.DropProceduresNotInSource)
      {
        foreach (var sp in compareResponse.Procedures.Additional)
        {
          Stage3.Add(new DefinitionBasedScripter().ScriptDrop(sp));
        }
      }

      if (options.DropViewsNotInSource)
      {
        foreach (var v in compareResponse.Views.Additional)
        {
          Stage3.Add(new DefinitionBasedScripter().ScriptDrop(v));
        }
      }

      foreach (var f in compareResponse.Functions.Additional)
      {
        Stage3.Add(new DefinitionBasedScripter().ScriptDrop(f));
      }

      foreach (var t in compareResponse.Triggers.Additional)
      {
        Stage3.Add(new DefinitionBasedScripter().ScriptDrop(t));
      }

      foreach (var tbl in compareResponse.Tables.Same)
      {
        if (!options.BlockOnPossibleDataLoss)
        {
          var addCols = compareResponse.Columns.Additional.FilterByTable(tbl).ToList();
          //var pk = compareResponse.PrimaryKeyConstraints.Missing.FilterByTable(tbl).FirstOrDefault();
          //var uq = compareResponse.UniqueConstraints.Missing.FilterByTable(tbl).ToList();
          if (addCols.Any())
          {
            Stage1.Add(new TableScripter().ScriptDropColumns(tbl, addCols));
          }
        }

        if (options.DropIndexesNotInSource)
        {
          foreach (var i in compareResponse.Indexs.Additional.FilterByTable(tbl))
          {
            Stage2.Add(new IndexScripter().ScriptDrop(i));
          }
        }
      }

      foreach (var tbl in compareResponse.Tables.Additional)
      {

      }

      foreach (var ut in compareResponse.UserTypes.Additional)
      {

      }

      foreach (var s in compareResponse.Schemas.Additional)
      {

      }
    }

    public List<string> ScriptCompareAsStatments(CompareResponse compareResponse)
    {
      ScriptMissing(compareResponse);
      ScriptDifferent(compareResponse);
      ScriptAdditional(compareResponse);
      return (new[] { Stage1, Stage2, Stage3 }).SelectMany(s => s).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }
  }
}
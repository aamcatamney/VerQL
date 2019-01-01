using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Loaders
{
  public class DirectoryLoader : ILoader
  {
    private const string ReplaceString = @"\s\s+(?=([^']*'[^']*')*[^']*$)";
    private const string DefaultConstraint = @"(constraint).+(\[?\w\]?).+(?=default)";
    private string _path;
    public DirectoryLoader(string Path)
    {
      this._path = Path;
    }
    public LoaderResponse Load()
    {
      var resp = new LoaderResponse();

      if (string.IsNullOrEmpty(this._path))
      {
        resp.Errors.Add("Path must not be null or empty");
        return resp;
      }

      if (!Directory.Exists(this._path))
      {
        resp.Errors.Add("Directory does not exist");
        return resp;
      }

      var sqlStatments = new List<string>();
      foreach (var p in Directory.GetFiles(_path, "*.sql", SearchOption.AllDirectories))
      {
        foreach (var s in ParseFile(p))
        {
          if (!string.IsNullOrEmpty(s))
          {
            sqlStatments.Add(s);
          }
        }
      }
      resp.Database = ProcessStatments(sqlStatments);

      resp.Warnings.AddRange(CheckProcedures(resp.Database));

      return resp;
    }

    private List<string> CheckProcedures(Database database)
    {
      var warnings = new List<string>();
      var procs = new List<Procedure>();
      foreach (var proc in database.Procedures.GroupBy(p => p.GetKey().ToLower()))
      {
        if (proc.Count() > 1)
        {
          warnings.Add($"Procedure: '{proc.Key}' is duplicated {proc.Count()} times");
        }
        procs.Add(proc.First());
      }
      database.Procedures = procs;
      return warnings;
    }

    protected List<string> ParseFile(string path)
    {
      var contents = File.ReadAllText(path);
      var starts = Regex.Matches(contents, "^(?!--)(EXECUTE |CREATE )(?=([^']*'[^']*')*[^']*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
      var statments = new List<string>();
      for (int i = 0; i < starts.Count; i++)
      {

        var s = starts[i];
        var index = contents.Length;
        if ((i + 1) < starts.Count)
        {
          index = starts[i + 1].Index;
        }
        var actual = contents.Substring(s.Index, index - s.Index);
        statments.Add(actual);
      }
      return statments;
    }

    protected Database ProcessStatments(List<string> sqlStatments)
    {
      var db = new Database();
      var regex = new Regex(ReplaceString);
      foreach (var s in sqlStatments)
      {
        var spaceRemoved = regex.Replace(s, " ");
        if (spaceRemoved.Trim().StartsWith("create table", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessTable(s);
          if (t != null)
          {
            db.Tables.Add(t.Table);
            db.Columns.AddRange(t.Columns);
            db.ForeignKeyConstraints.AddRange(t.ForeignKeyConstraints);
            if (t.PrimaryKeyConstraint != null) db.PrimaryKeyConstraints.Add(t.PrimaryKeyConstraint);
            db.UniqueConstraints.AddRange(t.UniqueConstraints);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("create procedure", StringComparison.OrdinalIgnoreCase))
        {
          var p = ProcessDefinitionBased<Procedure>(new Procedure(), "procedure", s);
          if (p != null)
          {
            db.Procedures.Add(p);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("create function", StringComparison.OrdinalIgnoreCase))
        {
          var f = ProcessDefinitionBased<Function>(new Function(), "function", s);
          if (f != null)
          {
            db.Functions.Add(f);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("create view", StringComparison.OrdinalIgnoreCase))
        {
          var v = ProcessDefinitionBased<View>(new View(), "view", s);
          if (v != null)
          {
            db.Views.Add(v);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("create trigger", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessDefinitionBased<Trigger>(new Trigger(), "trigger", s);
          if (t != null)
          {
            db.Triggers.Add(t);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("create type", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessUserType(s);
          if (t != null)
          {
            db.UserTypes.Add(t);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("create schema", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessSchema(s);
          if (t != null)
          {
            db.Schemas.Add(t);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("create nonclustered index", StringComparison.OrdinalIgnoreCase) || spaceRemoved.Trim().StartsWith("create index", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessIndex(s);
          if (t != null)
          {
            db.Indexs.Add(t);
          }
        }
        else if (spaceRemoved.Trim().StartsWith("execute sp_addextendedproperty", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessExtendedProperty(s);
          if (t != null)
          {
            db.ExtendedProperties.Add(t);
          }
        }
      }
      return db;
    }

    protected Schema ProcessSchema(string sql)
    {
      var t = sql;
      t = t.Substring(t.IndexOf("schema", StringComparison.OrdinalIgnoreCase) + 6).Trim();
      var split = t.Split(null).ToList();
      var s = new Schema();
      s.Name = split[0];
      s.Name = s.Name.RemoveSquareBrackets();
      split.RemoveAt(0);
      t = String.Join(" ", split).Trim();
      if (t.IndexOf("AUTHORIZATION", StringComparison.OrdinalIgnoreCase) > -1)
      {
        t = t.Substring(t.IndexOf("AUTHORIZATION", StringComparison.OrdinalIgnoreCase) + 13).Trim();
        if (t.EndsWith(";")) t = t.Substring(0, t.Length - 1);
        s.Authorization = t.RemoveSquareBrackets().Trim();
      }
      return s;
    }

    protected UserType ProcessUserType(string sql)
    {
      var ut = new UserType();
      var t = sql;
      t = t.Substring(t.IndexOf("type", StringComparison.OrdinalIgnoreCase) + 4).Trim();
      var split = t.Split(null).Where(s => !string.IsNullOrEmpty(s)).ToList();
      ut.Name = split[0];
      split.RemoveAt(0);
      if (ut.Name.Contains("."))
      {
        ut.Schema = ut.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        ut.Name = ut.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      ut.Name = ut.Name.RemoveSquareBrackets();
      ut.Schema = ut.Schema.RemoveSquareBrackets();

      t = String.Join(" ", split).Trim();
      if (t.EndsWith(";")) t = t.Substring(0, t.Length - 1);

      var c = ProcessColumn(string.Empty, string.Empty, t);
      ut.Type = c.Type;
      ut.MaxLength = c.MaxLength;
      ut.IsNullable = c.IsNullable;
      return ut;
    }

    protected T ProcessDefinitionBased<T>(T def, string type, string sql) where T : DefinitionBased
    {
      def.Name = sql.Substring(sql.IndexOf(type, StringComparison.OrdinalIgnoreCase) + type.Length + 1).Trim().Split(null).FirstOrDefault();
      if (def.Name.Contains("."))
      {
        def.Schema = def.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        def.Name = def.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      def.Name = def.Name.RemoveSquareBrackets();
      def.Schema = def.Schema.RemoveSquareBrackets();
      def.Definition = sql.Trim();
      if (def.Definition.EndsWith("GO", StringComparison.OrdinalIgnoreCase))
      {
        def.Definition = def.Definition.Substring(0, def.Definition.Length - 2).Trim();
      }
      return def;
    }

    protected ProcessTableResponse ProcessTable(string sql)
    {
      var resp = new ProcessTableResponse();
      var text = sql.Trim();
      if (text.EndsWith("GO", StringComparison.OrdinalIgnoreCase))
      {
        text = text.Substring(0, text.Length - 2).Trim();
      }
      var tbl = new Table();
      // Name & Schema
      tbl.Name = sql.Substring(sql.IndexOf("table", StringComparison.OrdinalIgnoreCase) + 5);
      tbl.Name = tbl.Name.Substring(0, tbl.Name.IndexOf("(")).Trim();
      if (tbl.Name.Contains("."))
      {
        tbl.Schema = tbl.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        tbl.Name = tbl.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      tbl.Name = tbl.Name.RemoveSquareBrackets();
      tbl.Schema = tbl.Schema.RemoveSquareBrackets();

      text = text.Substring(text.IndexOf("(") + 1);
      if (text.EndsWith(")")) text = text.Substring(0, text.Length - 1);
      else if (text.EndsWith(");")) text = text.Substring(0, text.Length - 2);

      // cols
      var lines = new List<string>();
      var split = text.TrueSplit();
      foreach (var s in split)
      {
        var t = s.Trim();
        if (t.StartsWith(",")) t = t.Substring(1);
        if (t.EndsWith(",")) t = t.Substring(0, t.Length - 1);
        t = t.Trim();
        if (!t.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase) && t.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > 0 && !Regex.IsMatch(t, DefaultConstraint, RegexOptions.IgnoreCase))
        {
          var rsplit = Regex.Split(s, "CONSTRAINT", RegexOptions.IgnoreCase).ToList();
          lines.Add(rsplit[0]);
          rsplit.RemoveAt(0);
          lines.AddRange(rsplit.Select(r => $"CONSTRAINT {r}"));
        }
        else
        {
          lines.Add(s);
        }
      }

      foreach (var cc in lines)
      {
        var t = cc.Trim();
        if (t.StartsWith(",")) t = t.Substring(1);
        if (t.EndsWith(",")) t = t.Substring(0, t.Length - 1);
        t = t.Trim();

        if (t.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase) || t.StartsWith("UNIQUE", StringComparison.OrdinalIgnoreCase))
        {
          if (t.IndexOf("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) > -1)
          {
            resp.PrimaryKeyConstraint = ProcessPrimaryKey(tbl.Schema, tbl.Name, t);
          }
          else if (t.IndexOf("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) > -1)
          {
            resp.ForeignKeyConstraints.Add(ProcessForeignKey(tbl.Schema, tbl.Name, t));
          }
          // Needs better unique find
          else if (t.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) > -1)
          {
            resp.UniqueConstraints.Add(ProcessUniqueConstraint(tbl.Schema, tbl.Name, t));
          }
          // Needs better check find
          else if (t.IndexOf("CHECK", StringComparison.OrdinalIgnoreCase) > -1)
          {

          }
        }
        else if (t.StartsWith("PRIMARY KEY", StringComparison.OrdinalIgnoreCase))
        {
          resp.PrimaryKeyConstraint = ProcessPrimaryKey(tbl.Schema, tbl.Name, t);
        }
        else
        {
          resp.Columns.Add(ProcessColumn(tbl.Schema, tbl.Name, t));
        }
      }

      // Validate PK
      if (resp.PrimaryKeyConstraint != null && string.IsNullOrEmpty(resp.PrimaryKeyConstraint.Name))
      {
        foreach (var pcol in resp.PrimaryKeyConstraint.Columns)
        {
          var col = resp.Columns.FirstOrDefault(c => c.Name.Equals(pcol.Name, StringComparison.OrdinalIgnoreCase));
          if (col != null)
          {
            col.IsPrimaryKey = true;
          }
        }
        resp.PrimaryKeyConstraint = null;
      }

      resp.Table = tbl;
      return resp;
    }

    protected ForeignKeyConstraint ProcessForeignKey(string TableSchema, string TableName, string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      var fk = new ForeignKeyConstraint();
      fk.TableSchema = TableSchema;
      fk.TableName = TableName;

      if (t.IndexOf("ON DELETE NO ACTION") > -1)
      {
        t = Regex.Replace(t, "ON DELETE NO ACTION", "", RegexOptions.IgnoreCase);
      }
      else if (t.IndexOf("ON DELETE CASCADE") > -1)
      {
        fk.OnDelete = eCascadeAction.CASCADE;
        t = Regex.Replace(t, "ON DELETE CASCADE", "", RegexOptions.IgnoreCase);
      }
      else if (t.IndexOf("ON DELETE SET DEFAULT") > -1)
      {
        fk.OnDelete = eCascadeAction.SET_DEFAULT;
        t = Regex.Replace(t, "ON DELETE SET DEFAULT", "", RegexOptions.IgnoreCase);
      }
      else if (t.IndexOf("ON DELETE SET NULL") > -1)
      {
        fk.OnDelete = eCascadeAction.SET_NULL;
        t = Regex.Replace(t, "ON DELETE SET NULL", "", RegexOptions.IgnoreCase);
      }

      if (t.IndexOf("ON UPDATE NO ACTION") > -1)
      {
        t = Regex.Replace(t, "ON UPDATE NO ACTION", "", RegexOptions.IgnoreCase);
      }
      else if (t.IndexOf("ON UPDATE CASCADE") > -1)
      {
        fk.OnUpdate = eCascadeAction.CASCADE;
        t = Regex.Replace(t, "ON UPDATE CASCADE", "", RegexOptions.IgnoreCase);
      }
      else if (t.IndexOf("ON UPDATE SET DEFAULT") > -1)
      {
        fk.OnUpdate = eCascadeAction.SET_DEFAULT;
        t = Regex.Replace(t, "ON UPDATE SET DEFAULT", "", RegexOptions.IgnoreCase);
      }
      else if (t.IndexOf("ON UPDATE SET NULL") > -1)
      {
        fk.OnUpdate = eCascadeAction.SET_NULL;
        t = Regex.Replace(t, "ON UPDATE SET NULL", "", RegexOptions.IgnoreCase);
      }

      t = Regex.Replace(t, "FOREIGN KEY", "", RegexOptions.IgnoreCase);

      if (t.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > -1)
      {
        t = Regex.Replace(t, "CONSTRAINT", "", RegexOptions.IgnoreCase).Trim();
        var split = t.Split(null).ToList();
        fk.Name = split[0].RemoveSquareBrackets();
        split.RemoveAt(0);
        t = string.Join(" ", split).Trim();
      }

      foreach (var c in t.Split(new[] { "REFERENCES" }, StringSplitOptions.None).First().RemoveBrackets().TrueSplit())
      {
        fk.Columns.Add(c.RemoveSquareBrackets().Trim());
      }
      t = t.Substring(t.IndexOf("REFERENCES", StringComparison.OrdinalIgnoreCase));
      t = Regex.Replace(t, "REFERENCES", "", RegexOptions.IgnoreCase).Trim();

      fk.ReferenceTable = t;
      fk.ReferenceTable = fk.ReferenceTable.Substring(0, fk.ReferenceTable.IndexOf("(")).Trim();
      if (fk.ReferenceTable.Contains("."))
      {
        fk.ReferenceSchema = fk.ReferenceTable.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        fk.ReferenceTable = fk.ReferenceTable.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      fk.ReferenceTable = fk.ReferenceTable.RemoveSquareBrackets();
      fk.ReferenceSchema = fk.ReferenceSchema.RemoveSquareBrackets();

      t = t.Substring(t.IndexOf("("));
      if (t.Contains(")")) t = t.Substring(0, t.LastIndexOf(")"));

      foreach (var c in t.RemoveBrackets().TrueSplit())
      {
        fk.ReferenceColumns.Add(c.RemoveSquareBrackets().Trim());
      }

      return fk;
    }

    protected PrimaryKeyConstraint ProcessPrimaryKey(string TableSchema, string TableName, string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      var pk = new PrimaryKeyConstraint();
      pk.TableSchema = TableSchema;
      pk.TableName = TableName;

      t = Regex.Replace(t, "PRIMARY KEY", "", RegexOptions.IgnoreCase);

      if (t.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > -1)
      {
        t = Regex.Replace(t, "CONSTRAINT", "", RegexOptions.IgnoreCase).Trim();
        var split = t.Split(null).ToList();
        pk.Name = split[0].RemoveSquareBrackets();
        split.RemoveAt(0);
        t = string.Join(" ", split).Trim();
      }

      pk.Clustered = t.IndexOf("NONCLUSTERED", StringComparison.OrdinalIgnoreCase) == -1;
      t = Regex.Replace(t, "NONCLUSTERED", "", RegexOptions.IgnoreCase);
      t = Regex.Replace(t, "CLUSTERED", "", RegexOptions.IgnoreCase);

      if (t.IndexOf("WITH", StringComparison.OrdinalIgnoreCase) > -1)
      {
        var with = t.Substring(t.IndexOf("WITH", StringComparison.OrdinalIgnoreCase) + 4);
        var ws = with.Trim().RemoveBrackets().TrueSplit(true, '=').Select(s => s.Trim()).ToList();
        ws.RemoveAt(0);
        pk.FillFactor = int.Parse(ws[0]);

        t = t.Substring(0, t.IndexOf("WITH", StringComparison.OrdinalIgnoreCase));
      }

      foreach (var c in t.RemoveBrackets().TrueSplit())
      {
        var col = new PrimaryKeyColumn();
        var colText = c.RemoveBrackets().Trim();
        col.Asc = !colText.EndsWith("DESC", StringComparison.OrdinalIgnoreCase);
        if (colText.EndsWith("DESC", StringComparison.OrdinalIgnoreCase)) colText = colText.Substring(0, colText.Length - 4);
        else if (colText.EndsWith("ASC", StringComparison.OrdinalIgnoreCase)) colText = colText.Substring(0, colText.Length - 3);
        col.Name = colText.RemoveSquareBrackets().Trim();
        pk.Columns.Add(col);
      }

      return pk;
    }

    protected UniqueConstraint ProcessUniqueConstraint(string TableSchema, string TableName, string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      var uc = new UniqueConstraint();
      uc.TableSchema = TableSchema;
      uc.TableName = TableName;

      if (t.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) > -1)
      {
        t = t.Substring(t.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) + 10).Trim();
      }
      var split = t.Split(null).ToList();

      // Have name
      if (!new[] { "NONCLUSTERED", "CLUSTERED", "UNIQUE" }.Any(s => split[0].RemoveSquareBrackets().StartsWith(s, StringComparison.OrdinalIgnoreCase)))
      {
        uc.Name = split[0].RemoveSquareBrackets();
        split.RemoveAt(0);
      }

      // Remove UNIQUE
      split.RemoveAll(s => s.Equals("UNIQUE", StringComparison.OrdinalIgnoreCase));
      split = split.Select(s => s.StartsWith("UNIQUE", StringComparison.OrdinalIgnoreCase) ? s.Substring(6) : s).ToList();

      t = string.Join(" ", split).Trim();

      t = Regex.Replace(t, "NONCLUSTERED", "", RegexOptions.IgnoreCase);
      uc.Clustered = t.IndexOf("CLUSTERED", StringComparison.OrdinalIgnoreCase) > -1;
      t = Regex.Replace(t, "CLUSTERED", "", RegexOptions.IgnoreCase);

      foreach (var c in t.RemoveBrackets().TrueSplit())
      {
        var col = new UniqueColumn();
        var colText = c.RemoveBrackets().Trim();
        col.Asc = !colText.EndsWith("DESC", StringComparison.OrdinalIgnoreCase);
        if (colText.EndsWith("DESC", StringComparison.OrdinalIgnoreCase)) colText = colText.Substring(0, colText.Length - 4);
        else if (colText.EndsWith("ASC", StringComparison.OrdinalIgnoreCase)) colText = colText.Substring(0, colText.Length - 3);
        col.Name = colText.RemoveSquareBrackets().Trim();
        uc.Columns.Add(col);
      }

      return uc;
    }

    protected Index ProcessIndex(string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ").Trim();
      var i = new Index();

      t = t.Replace(";", string.Empty);
      if (t.EndsWith("GO", StringComparison.OrdinalIgnoreCase))
      {
        t = t.Substring(0, t.Length - 2).Trim();
      }

      if (t.IndexOf("INDEX", StringComparison.OrdinalIgnoreCase) > -1)
      {
        t = t.Substring(t.IndexOf("INDEX", StringComparison.OrdinalIgnoreCase) + 5).Trim();
      }
      var split = t.Split(null).ToList();

      // name
      i.Name = split[0].RemoveSquareBrackets();
      split.RemoveAt(0);

      // Remove UNIQUE
      if (split.Any(s => s.Equals("UNIQUE", StringComparison.OrdinalIgnoreCase)))
      {
        i.IsUnique = true;
        split.RemoveAll(s => s.Equals("UNIQUE", StringComparison.OrdinalIgnoreCase));
        split = split.Select(s => s.StartsWith("UNIQUE", StringComparison.OrdinalIgnoreCase) ? s.Substring(6) : s).ToList();
      }
      t = string.Join(" ", split).Trim();

      t = Regex.Replace(t, "NONCLUSTERED", "", RegexOptions.IgnoreCase);
      t = Regex.Replace(t, "CLUSTERED", "", RegexOptions.IgnoreCase);

      // Tbl
      t = t.Substring(t.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase) + 4).Trim();
      split = t.Split('(').ToList();
      i.TableName = split[0];
      if (i.TableName.Contains("."))
      {
        i.TableSchema = i.TableName.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        i.TableName = i.TableName.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      i.TableName = i.TableName.Trim().RemoveSquareBrackets().Trim();
      i.TableSchema = i.TableSchema.Trim().RemoveSquareBrackets().Trim();
      split.RemoveAt(0);
      t = string.Join(" ", split).Trim();

      var cols = t;
      var icols = "";

      if (t.IndexOf("INCLUDE", StringComparison.OrdinalIgnoreCase) > -1)
      {
        cols = t.Substring(0, t.IndexOf("INCLUDE", StringComparison.OrdinalIgnoreCase));
        icols = t.Substring(t.IndexOf("INCLUDE", StringComparison.OrdinalIgnoreCase) + 7);
      }

      cols = cols.Trim().RemoveBrackets().Trim();
      icols = icols.Trim().RemoveBrackets().Trim();

      foreach (var c in cols.TrueSplit(false))
      {
        var col = new IndexColumn();
        var colText = c.Trim().RemoveBrackets().Trim();
        col.Asc = !colText.EndsWith("DESC", StringComparison.OrdinalIgnoreCase);
        if (colText.EndsWith("DESC", StringComparison.OrdinalIgnoreCase)) colText = colText.Substring(0, colText.Length - 4);
        else if (colText.EndsWith("ASC", StringComparison.OrdinalIgnoreCase)) colText = colText.Substring(0, colText.Length - 3);
        col.Name = colText.Trim().RemoveSquareBrackets().Trim();
        i.Columns.Add(col);
      }

      foreach (var c in icols.TrueSplit(false))
      {
        i.IncludedColumns.Add(c.Trim().RemoveBrackets().Trim().RemoveSquareBrackets().Trim());
      }

      return i;
    }

    protected ExtendedProperty ProcessExtendedProperty(string text)
    {
      var split = text.TrimGoAndSemi().TrueSplit(false).Select(t => t.Trim()).Select(s => s.Split(new[] { '@' }).LastOrDefault()).ToList();
      var ep = new ExtendedProperty();
      foreach (var s in split)
      {
        var key = s.Split(new[] { '=' }).First().RemoveNAndQuotes();
        var value = s.Split(new[] { '=' }).Last().RemoveNAndQuotes();

        if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
        {
          ep.Name = value;
        }
        else if (key.Equals("value", StringComparison.OrdinalIgnoreCase))
        {
          ep.Value = value;
        }
        else if (key.Equals("level0type", StringComparison.OrdinalIgnoreCase))
        {
          ep.Level0Type = value;
        }
        else if (key.Equals("level0name", StringComparison.OrdinalIgnoreCase))
        {
          ep.Level0Name = value;
        }
        else if (key.Equals("level1type", StringComparison.OrdinalIgnoreCase))
        {
          ep.Level1Type = value;
        }
        else if (key.Equals("level1name", StringComparison.OrdinalIgnoreCase))
        {
          ep.Level1Name = value;
        }
        else if (key.Equals("level2type", StringComparison.OrdinalIgnoreCase))
        {
          ep.Level2Type = value;
        }
        else if (key.Equals("level2name", StringComparison.OrdinalIgnoreCase))
        {
          ep.Level2Name = value;
        }
      }
      return ep;
    }

    protected Column ProcessColumn(string TableSchema, string TableName, string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      // Is null
      var col = new Column();
      col.TableSchema = TableSchema;
      col.TableName = TableName;
      col.IsNullable = t.IndexOf("NOT NULL", StringComparison.OrdinalIgnoreCase) == -1;
      t = Regex.Replace(t, "NOT NULL", "", RegexOptions.IgnoreCase);
      t = Regex.Replace(t, "NULL", "", RegexOptions.IgnoreCase);

      // Is Primary Key
      col.IsPrimaryKey = t.IndexOf("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) > -1;
      t = Regex.Replace(t, "PRIMARY KEY", "", RegexOptions.IgnoreCase);

      // Is Unique
      col.IsUnique = t.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) > -1;
      t = Regex.Replace(t, "UNIQUE", "", RegexOptions.IgnoreCase);

      //Name
      var split = t.Split(null).ToList();
      col.Name = split[0].RemoveSquareBrackets();
      split.RemoveAt(0);

      // Type
      split = string.Join(" ", split).Trim().Split(null).ToList();
      col.Type = split[0].Trim().RemoveSquareBrackets();
      col.IsUserDefined = col.Type.Contains(".");

      // Is Computed
      if (col.Type.Equals("AS", StringComparison.OrdinalIgnoreCase) || col.Type.StartsWith("AS(", StringComparison.OrdinalIgnoreCase))
      {
        split.RemoveAt(0);
        col.IsComputed = true;
        col.IsNullable = false;
        col.ComputedText = string.Join(" ", split).Trim();
        if (col.Type.StartsWith("AS(", StringComparison.OrdinalIgnoreCase)) col.ComputedText = "(" + col.ComputedText;
        col.Type = null;
      }
      else
      {
        var length = "";
        if (col.Type.Contains("("))
        {
          length = col.Type.Split(new[] { "(" }, StringSplitOptions.None)[1].RemoveBrackets().Trim();
          col.Type = col.Type.Split(new[] { "(" }, StringSplitOptions.None)[0].Trim();
        }
        else if (split.Count > 1 && split[1].Contains("("))
        {
          length = split[1].RemoveBrackets().Trim();
        }

        if (length.Equals("MAX", StringComparison.OrdinalIgnoreCase))
        {
          col.MaxLength = -1;
        }
        else if (!string.IsNullOrEmpty(length))
        {
          col.MaxLength = int.Parse(length);
        }
        split.RemoveAt(0);
      }
      t = string.Join(" ", split).Trim();

      // Identity
      if (t.ToUpper().Contains("IDENTITY"))
      {
        var ii = t.IndexOf("IDENTITY", StringComparison.OrdinalIgnoreCase) + 8;
        var ivalues = t.Substring(ii).Trim();
        if (ivalues.StartsWith("("))
        {
          var values = ivalues.Substring(1).Split(new[] { ")" }, StringSplitOptions.None)[0].Split(new[] { "," }, StringSplitOptions.None);
          col.SeedValue = int.Parse(values[0]);
          col.IncrementValue = int.Parse(values[1]);
        }
        col.IsIdentity = true;
      }

      // Default
      if (t.ToUpper().Contains("DEFAULT"))
      {
        var dcn = Regex.Match(t, DefaultConstraint, RegexOptions.IgnoreCase);
        if (dcn.Success)
        {
          col.DefaultName = t.Substring(dcn.Index + 11, dcn.Length - 11).Trim().RemoveSquareBrackets();
        }
        var di = t.IndexOf("DEFAULT", StringComparison.OrdinalIgnoreCase) + 8;
        col.DefaultText = t.Substring(di).Trim();
        col.HasDefault = true;
      }

      col.RemoveDefault();
      return col;
    }

    protected class ProcessTableResponse
    {
      public Table Table { get; set; }
      public List<Column> Columns { get; set; } = new List<Column>();
      public List<ForeignKeyConstraint> ForeignKeyConstraints { get; set; } = new List<ForeignKeyConstraint>();
      public PrimaryKeyConstraint PrimaryKeyConstraint { get; set; }
      public List<UniqueConstraint> UniqueConstraints { get; set; } = new List<UniqueConstraint>();
    }
  }
}
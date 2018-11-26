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

      return resp;
    }

    protected List<string> ParseFile(string path)
    {
      var contents = File.ReadAllText(path);
      var starts = Regex.Matches(contents, "^(?!--)(EXECUTE|CREATE)(?=([^']*'[^']*')*[^']*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
      foreach (var s in sqlStatments)
      {
        if (s.Trim().StartsWith("create table", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessTable(s);
          if (t != null)
          {
            db.Tables.Add(t);
          }
        }
        else if (s.Trim().StartsWith("create procedure", StringComparison.OrdinalIgnoreCase))
        {
          var p = ProcessProcedure(s);
          if (p != null)
          {
            db.Procedures.Add(p);
          }
        }
        else if (s.Trim().StartsWith("create function", StringComparison.OrdinalIgnoreCase))
        {
          var f = ProcessFunction(s);
          if (f != null)
          {
            db.Functions.Add(f);
          }
        }
        else if (s.Trim().StartsWith("create view", StringComparison.OrdinalIgnoreCase))
        {
          var v = ProcessView(s);
          if (v != null)
          {
            db.Views.Add(v);
          }
        }
        else if (s.Trim().StartsWith("create trigger", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessTrigger(s);
          if (t != null)
          {
            db.Triggers.Add(t);
          }
        }
        else if (s.Trim().StartsWith("create type", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessUserType(s);
          if (t != null)
          {
            db.UserTypes.Add(t);
          }
        }
        else if (s.Trim().StartsWith("create schema", StringComparison.OrdinalIgnoreCase))
        {
          var t = ProcessSchema(s);
          if (t != null)
          {
            db.Schemas.Add(t);
          }
        }
      }
      return db;
    }

    protected Schema ProcessSchema(string sql)
    {
      var t = sql;
      t = t.Substring(t.IndexOf("create schema", StringComparison.OrdinalIgnoreCase) + 13).Trim();
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
      ut.Name = sql.Substring(sql.IndexOf("create type", StringComparison.OrdinalIgnoreCase) + 11).Trim().Split(null).FirstOrDefault();
      if (ut.Name.Contains("."))
      {
        ut.Schema = ut.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        ut.Name = ut.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      ut.Name = ut.Name.RemoveSquareBrackets();
      ut.Schema = ut.Schema.RemoveSquareBrackets();
      ut.Definition = sql;
      return ut;
    }

    protected Trigger ProcessTrigger(string sql)
    {
      var tri = new Trigger();
      tri.Name = sql.Substring(sql.IndexOf("create trigger", StringComparison.OrdinalIgnoreCase) + 14).Trim().Split(null).FirstOrDefault();
      if (tri.Name.Contains("."))
      {
        tri.Schema = tri.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        tri.Name = tri.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      tri.Name = tri.Name.RemoveSquareBrackets();
      tri.Schema = tri.Schema.RemoveSquareBrackets();
      tri.Definition = sql;
      return tri;
    }

    protected Procedure ProcessProcedure(string sql)
    {
      var proc = new Procedure();
      proc.Name = sql.Substring(sql.IndexOf("create procedure", StringComparison.OrdinalIgnoreCase) + 16).Trim().Split(null).FirstOrDefault();
      if (proc.Name.Contains("."))
      {
        proc.Schema = proc.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        proc.Name = proc.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      proc.Name = proc.Name.RemoveSquareBrackets();
      proc.Schema = proc.Schema.RemoveSquareBrackets();
      proc.Definition = sql;
      return proc;
    }

    protected Function ProcessFunction(string sql)
    {
      var fun = new Function();
      fun.Name = sql.Substring(sql.IndexOf("create function", StringComparison.OrdinalIgnoreCase) + 15).Trim().Split(null).FirstOrDefault();
      if (fun.Name.Contains("."))
      {
        fun.Schema = fun.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        fun.Name = fun.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      fun.Name = fun.Name.RemoveSquareBrackets();
      fun.Schema = fun.Schema.RemoveSquareBrackets();
      fun.Definition = sql;
      return fun;
    }

    protected View ProcessView(string sql)
    {
      var view = new View();
      view.Name = sql.Substring(sql.IndexOf("create view", StringComparison.OrdinalIgnoreCase) + 11).Trim().Split(null).FirstOrDefault();
      if (view.Name.Contains("."))
      {
        view.Schema = view.Name.Split(new[] { "." }, StringSplitOptions.None).FirstOrDefault();
        view.Name = view.Name.Split(new[] { "." }, StringSplitOptions.None).LastOrDefault();
      }
      view.Name = view.Name.RemoveSquareBrackets();
      view.Schema = view.Schema.RemoveSquareBrackets();
      view.Definition = sql;
      return view;
    }

    protected Table ProcessTable(string sql)
    {
      var text = sql.Trim();
      if (text.EndsWith("GO", StringComparison.OrdinalIgnoreCase))
      {
        text = text.Substring(0, text.Length - 2).Trim();
      }
      var tbl = new Table();
      // Name & Schema
      tbl.Name = sql.Substring(sql.IndexOf("create table", StringComparison.OrdinalIgnoreCase) + 12);
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
      foreach (var cc in text.TrueSplit())
      {
        var t = cc.Trim();
        if (t.StartsWith(",")) t = t.Substring(1);
        if (t.EndsWith(",")) t = t.Substring(0, t.Length - 1);
        t = t.Trim();

        if (t.StartsWith("CONSTRAINT", StringComparison.OrdinalIgnoreCase))
        {
          if (t.IndexOf("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) > -1)
          {
            tbl.PrimaryKeyConstraint = ProcessPrimaryKey(t);
          }
          else if (t.IndexOf("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) > -1)
          {
            tbl.ForeignKeys.Add(ProcessForeignKey(t));
          }
          // Needs better unique find
          else if (t.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) > -1)
          {
            tbl.UniqueConstraints.Add(ProcessUniqueConstraint(t));
          }
          // Needs better check find
          else if (t.IndexOf("CHECK", StringComparison.OrdinalIgnoreCase) > -1)
          {

          }
        }
        else if (t.StartsWith("PRIMARY KEY", StringComparison.OrdinalIgnoreCase))
        {
          tbl.PrimaryKeyConstraint = ProcessPrimaryKey(t);
        }
        else
        {
          tbl.Columns.Add(ProcessColumn(t));
        }
      }
      return tbl;
    }

    protected ForeignKeyConstraint ProcessForeignKey(string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      var fk = new ForeignKeyConstraint();

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

      foreach (var c in t.RemoveBrackets().TrueSplit())
      {
        fk.ReferenceColumns.Add(c.RemoveSquareBrackets().Trim());
      }

      return fk;
    }

    protected PrimaryKeyConstraint ProcessPrimaryKey(string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      var pk = new PrimaryKeyConstraint();

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

      foreach (var c in t.TrueSplit())
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

    protected UniqueConstraint ProcessUniqueConstraint(string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      var uc = new UniqueConstraint();

      t = t.Substring(t.IndexOf("CONSTRAINT", StringComparison.OrdinalIgnoreCase) + 10).Trim();
      var split = t.Split(null).ToList();
      uc.Name = split[0].RemoveSquareBrackets();
      split.RemoveAt(0);
      t = string.Join(" ", split).Trim();

      uc.Clustered = t.IndexOf("NONCLUSTERED", StringComparison.OrdinalIgnoreCase) == -1;
      t = Regex.Replace(t, "NONCLUSTERED", "", RegexOptions.IgnoreCase);
      t = Regex.Replace(t, "CLUSTERED", "", RegexOptions.IgnoreCase);

      foreach (var c in t.TrueSplit())
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

    protected Column ProcessColumn(string text)
    {
      var t = new Regex(@"\s\s+").Replace(text, " ");
      // Is null
      var col = new Column();
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
        var di = t.IndexOf("DEFAULT", StringComparison.OrdinalIgnoreCase) + 8;
        col.DefaultText = t.Substring(di).Trim();
        col.HasDefault = true;
      }

      return col;
    }
  }
}
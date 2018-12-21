using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Loaders
{
  public class DatabaseLoader : ILoader
  {
    private string _connString;
    public DatabaseLoader(string ConnString)
    {
      this._connString = ConnString;
    }
    public LoaderResponse Load()
    {
      var resp = new LoaderResponse();

      if (string.IsNullOrEmpty(this._connString))
      {
        resp.Errors.Add("Connection string must not be null or empty");
        return resp;
      }

      resp.Database = GetDatabaseSchema();

      return resp;
    }

    protected Database GetDatabaseSchema()
    {
      Database db = null;
      var sql = string.Empty;
      var assembly = typeof(DatabaseLoader).GetTypeInfo().Assembly;
      var test = assembly.GetManifestResourceNames();
      var resourceStream = assembly.GetManifestResourceStream("VerQL.Core.SQL.GetDatabaseSchema.sql");
      using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
      {
        sql = reader.ReadToEnd();
      }
      using (var conn = new SqlConnection(this._connString))
      {
        conn.Open();
        using (var multi = conn.QueryMultiple(sql))
        {
          db = new Database();
          db.Schemas = multi.Read<Schema>().ToList();
          db.UserTypes = multi.Read<UserType>().ToList();
          db.Tables = multi.Read<Table>().ToList();
          db.Columns = multi.Read<Column>().ToList();
          db.Columns.RemoveDefaults();
          db.ForeignKeyConstraints = multi.Read<ForeignKeyConstraint>().ToList();
          MapForeignKeyColumns(db.ForeignKeyConstraints, multi.Read<FBForeignKeyColumn>());

          var pkc = multi.Read<DBPrimaryKeyConstraint>().ToList();
          MapPrimaryKeyColumns(pkc, multi.Read<DBPrimaryKeyColumn>());
          db.PrimaryKeyConstraints = pkc.Where(c => !c.SystemNamed).Select(c => (PrimaryKeyConstraint)c).ToList();
          MapColumnPrimaryKeys(db.Columns, pkc.Where(c => c.SystemNamed));

          var uq = multi.Read<DBUniqueConstraint>().ToList();
          MapUniqueKeyColumns(uq, multi.Read<DBUniqueColumn>());
          MapColumnUniqueConstraints(db.Columns, uq.Where(c => c.SystemNamed));
          db.UniqueConstraints = uq.Where(c => !c.SystemNamed).Select(c => (UniqueConstraint)c).ToList();

          db.Procedures = multi.Read<Procedure>().ToList();
          TrimDefinitions(db.Procedures);
          db.Views = multi.Read<View>().ToList();
          TrimDefinitions(db.Views);
          db.Functions = multi.Read<Function>().ToList();
          TrimDefinitions(db.Functions);
          db.Triggers = multi.Read<Trigger>().ToList();
          TrimDefinitions(db.Triggers);
        }
      }
      return db;
    }

    private void MapColumnPrimaryKeys(IEnumerable<Column> columns, IEnumerable<DBPrimaryKeyConstraint> primaryKeys)
    {
      foreach (var pk in primaryKeys)
      {
        var col = columns.FirstOrDefault(c => pk.TableName.Equals(c.TableName, StringComparison.OrdinalIgnoreCase) &&
          pk.TableSchema.Equals(c.TableSchema, StringComparison.OrdinalIgnoreCase)
          && pk.Columns.Any(pc => pc.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)));
        if (col != null)
        {
          col.IsPrimaryKey = true;
        }
      }
    }

    private void MapColumnUniqueConstraints(IEnumerable<Column> columns, IEnumerable<DBUniqueConstraint> uniqueConstraints)
    {
      foreach (var pk in uniqueConstraints)
      {
        var col = columns.FirstOrDefault(c => pk.TableName.Equals(c.TableName, StringComparison.OrdinalIgnoreCase) &&
          pk.TableSchema.Equals(c.TableSchema, StringComparison.OrdinalIgnoreCase)
          && pk.Columns.Any(pc => pc.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)));
        if (col != null)
        {
          col.IsUnique = true;
        }
      }
    }

    private void TrimDefinitions(IEnumerable<DefinitionBased> definitions)
    {
      if (definitions != null)
      {
        foreach (var d in definitions)
        {
          d.Definition = d.Definition.Trim();
        }
      }
    }

    private void MapForeignKeyColumns(IEnumerable<ForeignKeyConstraint> fks, IEnumerable<FBForeignKeyColumn> cols)
    {
      foreach (var fk in fks)
      {
        fk.Columns = cols.Where(c => c.Schema == fk.TableSchema && c.Table == fk.TableName && fk.Name == c.FK).Select(c => c.Column).ToList();
        fk.ReferenceColumns = cols.Where(c => c.Schema == fk.TableSchema && c.Table == fk.TableName && fk.Name == c.FK).Select(c => c.ReferenceColumn).ToList();
      }
    }

    private void MapPrimaryKeyColumns(IEnumerable<PrimaryKeyConstraint> pkc, IEnumerable<DBPrimaryKeyColumn> cols)
    {
      foreach (var pk in pkc)
      {
        pk.Columns = cols.Where(c => c.Schema == pk.TableSchema && c.Table == pk.TableName && pk.Name == c.PK).Select(c => (PrimaryKeyColumn)c).ToList();
      }
    }

    private void MapUniqueKeyColumns(IEnumerable<UniqueConstraint> uqs, IEnumerable<DBUniqueColumn> cols)
    {
      foreach (var uq in uqs)
      {
        uq.Columns = cols.Where(c => c.Schema == uq.TableSchema && c.Table == uq.TableName && uq.Name == c.UQ).Select(c => (UniqueColumn)c).ToList();
      }
    }

    protected class FBForeignKeyColumn
    {
      public string Schema { get; set; }
      public string Table { get; set; }
      public string FK { get; set; }
      public string Column { get; set; }
      public string ReferenceColumn { get; set; }
    }

    protected class DBPrimaryKeyConstraint : PrimaryKeyConstraint
    {
      public bool SystemNamed { get; set; }
    }

    protected class DBPrimaryKeyColumn : PrimaryKeyColumn
    {
      public string Schema { get; set; }
      public string Table { get; set; }
      public string PK { get; set; }
    }

    protected class DBUniqueConstraint : UniqueConstraint
    {
      public bool SystemNamed { get; set; }
    }

    protected class DBUniqueColumn : UniqueColumn
    {
      public string Schema { get; set; }
      public string Table { get; set; }
      public string UQ { get; set; }
      public bool SystemNamed { get; set; }
    }
  }
}
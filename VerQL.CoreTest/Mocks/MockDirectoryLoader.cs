using System.Collections.Generic;
using VerQL.Core.Loaders;
using VerQL.Core.Models;

namespace VerQL.CoreTest
{
  public class MockDirectoryLoader : DirectoryLoader
  {
    public MockDirectoryLoader() : base("FAKEPATH") { }

    public TestProcessTableResponse TestProcessTable(string sql)
    {
      var resp = new TestProcessTableResponse();
      var result = ProcessTable(sql);
      resp.Table = result.Table;
      resp.Columns = result.Columns;
      resp.ForeignKeyConstraints = result.ForeignKeyConstraints;
      resp.PrimaryKeyConstraint = result.PrimaryKeyConstraint;
      resp.UniqueConstraints = result.UniqueConstraints;
      return resp;
    }

    public PrimaryKeyConstraint TestProcessPrimaryKey(string TableSchema, string TableName, string sql)
    {
      return ProcessPrimaryKey(TableSchema, TableName, sql);
    }

    public Procedure TestProcessProcedure(string sql)
    {
      return ProcessDefinitionBased<Procedure>(new Procedure(), "procedure", sql);
    }

    public Function TestProcessFunction(string sql)
    {
      return ProcessDefinitionBased<Function>(new Function(), "function", sql);
    }

    public class TestProcessTableResponse
    {
      public Table Table { get; set; }
      public List<Column> Columns { get; set; } = new List<Column>();
      public List<ForeignKeyConstraint> ForeignKeyConstraints { get; set; } = new List<ForeignKeyConstraint>();
      public PrimaryKeyConstraint PrimaryKeyConstraint { get; set; }
      public List<UniqueConstraint> UniqueConstraints { get; set; } = new List<UniqueConstraint>();
    }
  }
}
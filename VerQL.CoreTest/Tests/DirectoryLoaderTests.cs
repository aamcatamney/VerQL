using System;
using Xunit;

namespace VerQL.CoreTest
{
  public class DirectoryLoaderTests
  {
    [Theory]
    [InlineData("create table [dbo].[myTable] ()")]
    [InlineData("create table [dbo].myTable ()")]
    [InlineData("create table dbo.[myTable] ()")]
    [InlineData("create table dbo.myTable ()")]
    [InlineData("create table myTable ()")]
    [InlineData("CREATE TABLE myTable ()")]
    [InlineData("CREATE table myTable ()")]
    [InlineData("create TABLE myTable ()")]
    [InlineData("create TABLE myTable ();")]
    public void ProcessTable_NameAndSchemas(string sql)
    {
      var t = new MockDirectoryLoader().TestProcessTable(sql);
      Assert.NotNull(t);
      Assert.Equal<string>("myTable", t.Table.Name);
      Assert.Equal<string>("dbo", t.Table.Schema);
      Assert.Empty(t.Columns);
      Assert.Null(t.PrimaryKeyConstraint);
    }

    [Theory]
    [InlineData("PRIMARY KEY CLUSTERED ([MyCol] ASC)")]
    [InlineData("PRIMARY KEY CLUSTERED ([MyCol])")]
    [InlineData("PRIMARY KEY ([MyCol])")]
    [InlineData("PRIMARY KEY (MyCol)")]
    public void TestProcessPrimaryKey_NoName(string sql)
    {
      var p = new MockDirectoryLoader().TestProcessPrimaryKey("", "", sql);
      Assert.NotNull(p);
      Assert.Null(p.Name);
      Assert.True(p.Clustered);
      Assert.Equal<int>(1, p.Columns.Count);
      Assert.Equal<string>("MyCol", p.Columns[0].Name);
      Assert.True(p.Columns[0].Asc);
    }

    [Theory]
    [InlineData("PRIMARY KEY NONCLUSTERED ([MyCol] DESC)")]
    [InlineData("PRIMARY KEY NONCLUSTERED (MyCol DESC)")]
    public void TestProcessPrimaryKey_NoName_Desc_NonClustered(string sql)
    {
      var p = new MockDirectoryLoader().TestProcessPrimaryKey("", "", sql);
      Assert.NotNull(p);
      Assert.Null(p.Name);
      Assert.False(p.Clustered);
      Assert.Equal<int>(1, p.Columns.Count);
      Assert.Equal<string>("MyCol", p.Columns[0].Name);
      Assert.False(p.Columns[0].Asc);
    }

    [Theory]
    [InlineData("create procedure dbo.myProc @id int as select * from myTable where id = @id")]
    [InlineData("create procedure myProc @id int as select * from myTable where id = @id")]
    [InlineData("create procedure [myProc] @id int as select * from myTable where id = @id")]
    [InlineData("create procedure [dbo].[myProc] @id int as select * from myTable where id = @id")]
    [InlineData("create procedure dbo.[myProc] @id int as select * from myTable where id = @id")]
    [InlineData("create procedure [dbo].myProc @id int as select * from myTable where id = @id")]
    public void TestProcessProcedure(string sql)
    {
      var p = new MockDirectoryLoader().TestProcessProcedure(sql);
      Assert.NotNull(p);
      Assert.NotNull(p.Name);
      Assert.Equal<string>("myProc", p.Name);
      Assert.NotNull(p.Schema);
      Assert.Equal<string>("dbo", p.Schema);
      Assert.NotNull(p.Definition);
      Assert.Equal<string>(sql, p.Definition);
    }

    [Theory]
    [InlineData("create function dbo.myFunc (@ItemList nvarchar(max)) select @ItemList")]
    [InlineData("create function myFunc (@ItemList nvarchar(max)) select @ItemList")]
    [InlineData("create function [myFunc] (@ItemList nvarchar(max)) select @ItemList")]
    [InlineData("create function [dbo].[myFunc] (@ItemList nvarchar(max)) select @ItemList")]
    [InlineData("create function dbo.[myFunc] (@ItemList nvarchar(max)) select @ItemList")]
    [InlineData("create function [dbo].myFunc (@ItemList nvarchar(max)) select @ItemList")]
    [InlineData("create   function [dbo].myFunc (@ItemList nvarchar(max)) select @ItemList")]
    public void TestProcessFunction(string sql)
    {
      var p = new MockDirectoryLoader().TestProcessFunction(sql);
      Assert.NotNull(p);
      Assert.NotNull(p.Name);
      Assert.Equal<string>("myFunc", p.Name);
      Assert.NotNull(p.Schema);
      Assert.Equal<string>("dbo", p.Schema);
      Assert.NotNull(p.Definition);
      Assert.Equal<string>(sql, p.Definition);
    }

    [Theory]
    [InlineData("create table [dbo].[myTable] ([mycol] nvarchar(200) constraint [df_myTable_mycol] default ('') not null)")]
    [InlineData("create table [dbo].[myTable] ([mycol] nvarchar(200) not null constraint [df_myTable_mycol] default (''))")]
    public void ProcessTable_ColumnsDefaults(string sql)
    {
      var t = new MockDirectoryLoader().TestProcessTable(sql);
      Assert.NotNull(t);
      Assert.Equal<string>("myTable", t.Table.Name);
      Assert.Equal<string>("dbo", t.Table.Schema);
      Assert.NotEmpty(t.Columns);
      Assert.Equal<int>(1, t.Columns.Count);
      Assert.Equal<string>("mycol", t.Columns[0].Name);
      Assert.Equal<string>("nvarchar", t.Columns[0].Type);
      Assert.Equal<int>(200, t.Columns[0].MaxLength);
      Assert.Equal<bool>(true, t.Columns[0].HasDefault);
      Assert.Equal<string>("('')", t.Columns[0].DefaultText);
      Assert.Equal<string>("df_myTable_mycol", t.Columns[0].DefaultName);
      Assert.Equal<bool>(false, t.Columns[0].IsNullable);
      Assert.Empty(t.UniqueConstraints);
      Assert.Empty(t.ForeignKeyConstraints);
      Assert.Null(t.PrimaryKeyConstraint);
    }
  }
}

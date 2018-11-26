﻿using System;
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
            Assert.Equal<string>("myTable", t.Name);
            Assert.Equal<string>("dbo", t.Schema);
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
            var p = new MockDirectoryLoader().TestProcessPrimaryKey(sql);
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
            var p = new MockDirectoryLoader().TestProcessPrimaryKey(sql);
            Assert.NotNull(p);
            Assert.Null(p.Name);
            Assert.False(p.Clustered);
            Assert.Equal<int>(1, p.Columns.Count);
            Assert.Equal<string>("MyCol", p.Columns[0].Name);
            Assert.False(p.Columns[0].Asc);
        }
    }
}

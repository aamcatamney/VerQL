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
                    MapTableColumns(db.Tables, multi.Read<DBColumn>());
                    var fks = multi.Read<DBForeignKeyConstraint>();
                    MapForeignKeyColumns(fks, multi.Read<FBForeignKeyColumn>());
                    MapTableForeignKeys(db.Tables, fks);
                    var pks = multi.Read<DBPrimaryKeyConstraint>();
                    MapPrimaryKeyColumns(pks, multi.Read<DBPrimaryKeyColumn>());
                    MapTablePrimaryKeys(db.Tables, pks);
                    var uqs = multi.Read<DBUniqueConstraint>();
                    MapUniqueKeyColumns(uqs, multi.Read<DBUniqueColumn>());
                    MapTableUniqueKeys(db.Tables, uqs);
                    db.Procedures = multi.Read<Procedure>().ToList();
                    db.Views = multi.Read<View>().ToList();
                    db.Functions = multi.Read<Function>().ToList();
                    db.Triggers = multi.Read<Trigger>().ToList();
                }
            }
            return db;
        }

        private void MapTableColumns(IEnumerable<Table> tables, IEnumerable<DBColumn> cols)
        {
            foreach (var t in tables)
            {
                t.Columns = cols.Where(c => c.Schema == t.Schema && c.Table == t.Name).Select(c => (Column)c).ToList();
            }
        }

        private void MapTableForeignKeys(IEnumerable<Table> tables, IEnumerable<DBForeignKeyConstraint> foreignKeys)
        {
            foreach (var t in tables)
            {
                t.ForeignKeys = foreignKeys.Where(c => c.Schema == t.Schema && c.Table == t.Name).Select(c => (ForeignKeyConstraint)c).ToList();
            }
        }

        private void MapTablePrimaryKeys(IEnumerable<Table> tables, IEnumerable<DBPrimaryKeyConstraint> primaryKeys)
        {
            foreach (var t in tables)
            {
                var p = primaryKeys.FirstOrDefault(c => c.Schema == t.Schema && c.Table == t.Name);
                if (p != null)
                {
                    t.PrimaryKeyConstraint = (PrimaryKeyConstraint)p;
                }
            }
        }

        private void MapTableUniqueKeys(IEnumerable<Table> tables, IEnumerable<DBUniqueConstraint> uniqueKeys)
        {
            foreach (var t in tables)
            {
                t.UniqueConstraints = uniqueKeys.Where(c => c.Schema == t.Schema && c.Table == t.Name).Select(u => (UniqueConstraint)u).ToList();
            }
        }

        private void MapForeignKeyColumns(IEnumerable<DBForeignKeyConstraint> fks, IEnumerable<FBForeignKeyColumn> cols)
        {
            foreach (var fk in fks)
            {
                fk.Columns = cols.Where(c => c.Schema == fk.Schema && c.Table == fk.Table && fk.Name == c.FK).Select(c => c.Column).ToList();
                fk.ReferenceColumns = cols.Where(c => c.Schema == fk.Schema && c.Table == fk.Table && fk.Name == c.FK).Select(c => c.ReferenceColumn).ToList();
            }
        }

        private void MapPrimaryKeyColumns(IEnumerable<DBPrimaryKeyConstraint> pkc, IEnumerable<DBPrimaryKeyColumn> cols)
        {
            foreach (var pk in pkc)
            {
                pk.Columns = cols.Where(c => c.Schema == pk.Schema && c.Table == pk.Table && pk.Name == c.PK).Select(c => (PrimaryKeyColumn)c).ToList();
            }
        }

        private void MapUniqueKeyColumns(IEnumerable<DBUniqueConstraint> uqs, IEnumerable<DBUniqueColumn> cols)
        {
            foreach (var uq in uqs)
            {
                uq.Columns = cols.Where(c => c.Schema == uq.Schema && c.Table == uq.Table && uq.Name == c.UQ).Select(c => (UniqueColumn)c).ToList();
            }
        }

        protected class DBColumn : Column
        {
            public string Schema { get; set; }
            public string Table { get; set; }
        }

        protected class DBForeignKeyConstraint : ForeignKeyConstraint
        {
            public string Schema { get; set; }
            public string Table { get; set; }
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
            public string Schema { get; set; }
            public string Table { get; set; }
        }

        protected class DBPrimaryKeyColumn : PrimaryKeyColumn
        {
            public string Schema { get; set; }
            public string Table { get; set; }
            public string PK { get; set; }
        }

        protected class DBUniqueConstraint : UniqueConstraint
        {
            public string Schema { get; set; }
            public string Table { get; set; }
        }

        protected class DBUniqueColumn : UniqueColumn
        {
            public string Schema { get; set; }
            public string Table { get; set; }
            public string UQ { get; set; }
        }
    }
}
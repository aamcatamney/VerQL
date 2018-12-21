using System;
using System.Collections.Generic;
using System.Linq;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
  public class DatabaseComparer
  {
    private BaseEqualityComparer BEC = new BaseEqualityComparer();
    private ColumnEqualityComparer CEC = new ColumnEqualityComparer();
    public CompareResponse Compare(Database left, Database right)
    {
      var resp = new CompareResponse();
      resp.Procedures = CompareDefinition(left.Procedures, right.Procedures);
      resp.Views = CompareDefinition(left.Views, right.Views);
      resp.Functions = CompareDefinition(left.Functions, right.Functions);
      resp.Triggers = CompareDefinition(left.Triggers, right.Triggers);
      resp.Schemas = CompareSchema(left.Schemas, right.Schemas);
      resp.UserTypes = CompareUserTypes(left.UserTypes, right.UserTypes);
      resp.Tables = CompareTables(left.Tables, right.Tables);
      resp.Columns = CompareColumns(left.Columns, right.Columns);
      return resp;
    }

    protected CompareResult<Table> CompareTables(List<Table> left, List<Table> right)
    {
      var resp = new CompareResult<Table>();
      var cols = new Dictionary<Table, CompareResult<Column>>();
      foreach (Table l in GetBaseIntersect(left, right))
      {
        resp.Same.Add(l);
      }
      resp.Missing = GetBaseMissing(left, right).ToList();
      resp.Additional = GetBaseMissing(right, left).ToList();
      return resp;
    }

    protected CompareResult<Column> CompareColumns(IEnumerable<Column> left, IEnumerable<Column> right)
    {
      var resp = new CompareResult<Column>();
      foreach (var l in GetTableIntersect(left, right))
      {
        var r = right.First(x => x.GetTableKey().Equals(l.GetTableKey(), StringComparison.OrdinalIgnoreCase) && x.Name.Equals(l.Name, StringComparison.OrdinalIgnoreCase));
        if (CEC.Equals(l, r)) resp.Same.Add(l);
        else resp.Different.Add(l);
      }
      resp.Additional = GetTableMissing(right, left).ToList();
      resp.Missing = GetTableMissing(left, right).ToList();
      return resp;
    }

    protected CompareResult<Schema> CompareSchema(List<Schema> left, List<Schema> right)
    {
      var resp = new CompareResult<Schema>();
      var rightNames = right.Select(r => r.Name).ToList();
      var leftNames = left.Select(r => r.Name).ToList();
      foreach (var ln in leftNames.Intersect(rightNames))
      {
        var l = left.First(x => x.Name.Equals(ln));
        var r = right.First(x => x.Name.Equals(l.Name));
        if (l.Authorization.Equals(r.Authorization)) resp.Same.Add(l);
        else resp.Different.Add(l);
      }
      resp.Missing = rightNames.Except(leftNames).Select(p => right.First(r => r.Name.Equals(p))).ToList();
      resp.Additional = leftNames.Except(rightNames).Select(p => left.First(r => r.Name.Equals(p))).ToList();
      return resp;
    }

    protected CompareResult<UserType> CompareUserTypes(List<UserType> left, List<UserType> right)
    {
      var resp = new CompareResult<UserType>();
      foreach (UserType l in left.Intersect(right, BEC))
      {
        var r = right.First(x => x.GetKey().Equals(l.GetKey()));
        if (l.IsNullable == r.IsNullable && l.MaxLength == r.MaxLength && l.Type.Equals(r.Type, StringComparison.OrdinalIgnoreCase)) resp.Same.Add(l);
        else resp.Different.Add(l);
      }
      resp.Missing = GetBaseMissing(left, right).ToList();
      resp.Additional = GetBaseMissing(right, left).ToList();
      return resp;
    }

    protected CompareResult<T> CompareDefinition<T>(List<T> left, List<T> right) where T : DefinitionBased
    {
      var resp = new CompareResult<T>();
      foreach (T l in GetBaseIntersect(left, right))
      {
        var r = right.First(x => x.GetKey().Equals(l.GetKey(), StringComparison.OrdinalIgnoreCase));
        if (l.Definition.Equals(r.Definition, StringComparison.OrdinalIgnoreCase)) resp.Same.Add(l);
        else resp.Different.Add(l);
      }
      resp.Missing = GetBaseMissing(left, right).ToList();
      resp.Additional = GetBaseMissing(right, left).ToList();
      return resp;
    }

    private IEnumerable<T> GetBaseMissing<T>(IEnumerable<T> left, IEnumerable<T> right) where T : Base
    {
      return right.Where(r => !left.Any(x => x.GetKey().Equals(r.GetKey(), StringComparison.OrdinalIgnoreCase)));
    }

    private IEnumerable<T> GetBaseIntersect<T>(IEnumerable<T> left, IEnumerable<T> right) where T : Base
    {
      return left.Where(r => right.Any(x => x.GetKey().Equals(r.GetKey(), StringComparison.OrdinalIgnoreCase)));
    }

    private IEnumerable<T> GetTableMissing<T>(IEnumerable<T> left, IEnumerable<T> right) where T : TableBase
    {
      return right.Where(r => !left.Any(x => x.GetTableKey().Equals(r.GetTableKey(), StringComparison.OrdinalIgnoreCase)));
    }

    private IEnumerable<T> GetTableIntersect<T>(IEnumerable<T> left, IEnumerable<T> right) where T : TableBase
    {
      return left.Where(r => right.Any(x => x.GetTableKey().Equals(r.GetTableKey(), StringComparison.OrdinalIgnoreCase)));
    }
  }
}
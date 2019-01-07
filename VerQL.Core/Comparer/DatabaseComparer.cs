using System;
using System.Collections.Generic;
using System.Linq;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
  public class DatabaseComparer
  {
    private readonly Dictionary<string, string> vars;
    private BaseEqualityComparer BEC = new BaseEqualityComparer();
    private ColumnEqualityComparer CEC = new ColumnEqualityComparer();
    private UniqueConstraintEqualityComparer UCEC = new UniqueConstraintEqualityComparer();
    private PrimaryKeyConstraintEqualityComparer PCEC = new PrimaryKeyConstraintEqualityComparer();
    private ForeignKeyConstraintEqualityComparer FCEC = new ForeignKeyConstraintEqualityComparer();
    private IndexEqualityComparer ICEC = new IndexEqualityComparer();
    private ExtendedPropertyEqualityComparer EPEC = new ExtendedPropertyEqualityComparer();

    public DatabaseComparer(Dictionary<string, string> vars = null)
    {
      this.vars = vars;
      if (this.vars == null)
      {
        this.vars = new Dictionary<string, string>();
      }
    }
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
      resp.UniqueConstraints = CompareUniqueConstraints(left.UniqueConstraints, right.UniqueConstraints);
      resp.PrimaryKeyConstraints = ComparePrimaryKeyConstraints(left.PrimaryKeyConstraints, right.PrimaryKeyConstraints);
      resp.ForeignKeyConstraints = CompareForeignKeyConstraints(left.ForeignKeyConstraints, right.ForeignKeyConstraints);
      resp.Indexs = CompareIndexs(left.Indexs, right.Indexs);
      resp.ExtendedProperties = CompareExtendedProperties(left.ExtendedProperties, right.ExtendedProperties);
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

    protected CompareResult<UniqueConstraint> CompareUniqueConstraints(IEnumerable<UniqueConstraint> left, IEnumerable<UniqueConstraint> right)
    {
      var resp = new CompareResult<UniqueConstraint>();
      resp.Additional = GetTableMissing(right, left).ToList();
      foreach (var l in GetTableIntersect(left, right))
      {
        var r = right.FirstOrDefault(x => x.GetTableKey().Equals(l.GetTableKey(), StringComparison.OrdinalIgnoreCase) && x.Name.Equals(l.Name, StringComparison.OrdinalIgnoreCase));
        if (r == null) resp.Additional.Add(l);
        else if (UCEC.Equals(l, r)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<UniqueConstraint, UniqueConstraint>(l, r));
      }
      resp.Missing = GetTableMissing(left, right).ToList();
      return resp;
    }

    protected CompareResult<PrimaryKeyConstraint> ComparePrimaryKeyConstraints(IEnumerable<PrimaryKeyConstraint> left, IEnumerable<PrimaryKeyConstraint> right)
    {
      var resp = new CompareResult<PrimaryKeyConstraint>();
      resp.Additional = GetTableMissing(right, left).ToList();
      foreach (var l in GetTableIntersect(left, right))
      {
        var r = right.FirstOrDefault(x => x.GetTableKey().Equals(l.GetTableKey(), StringComparison.OrdinalIgnoreCase) && x.Name.Equals(l.Name, StringComparison.OrdinalIgnoreCase));
        if (r == null) resp.Additional.Add(l);
        else if (PCEC.Equals(l, r)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<PrimaryKeyConstraint, PrimaryKeyConstraint>(l, r));
      }
      resp.Missing = GetTableMissing(left, right).ToList();
      return resp;
    }

    protected CompareResult<ForeignKeyConstraint> CompareForeignKeyConstraints(IEnumerable<ForeignKeyConstraint> left, IEnumerable<ForeignKeyConstraint> right)
    {
      var resp = new CompareResult<ForeignKeyConstraint>();
      resp.Additional = GetTableMissing(right, left).ToList();
      foreach (var l in GetTableIntersect(left, right))
      {
        var r = right.FirstOrDefault(x => x.GetTableKey().Equals(l.GetTableKey(), StringComparison.OrdinalIgnoreCase) && (x.Name ?? string.Empty).Equals((l.Name ?? string.Empty), StringComparison.OrdinalIgnoreCase));
        if (r == null) resp.Additional.Add(l);
        else if (FCEC.Equals(l, r)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<ForeignKeyConstraint, ForeignKeyConstraint>(l, r));
      }
      resp.Missing = GetTableMissing(left, right).ToList();
      return resp;
    }

    protected CompareResult<Index> CompareIndexs(IEnumerable<Index> left, IEnumerable<Index> right)
    {
      var resp = new CompareResult<Index>();
      resp.Additional = GetTableMissing(right, left).ToList();
      foreach (var l in GetTableIntersect(left, right))
      {
        var r = right.FirstOrDefault(x => x.GetTableKey().Equals(l.GetTableKey(), StringComparison.OrdinalIgnoreCase) && x.Name.Equals(l.Name, StringComparison.OrdinalIgnoreCase));
        if (r == null) resp.Additional.Add(l);
        else if (ICEC.Equals(l, r)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<Index, Index>(l, r));
      }
      resp.Missing = GetTableMissing(left, right).ToList();
      return resp;
    }

    protected CompareResult<Column> CompareColumns(IEnumerable<Column> left, IEnumerable<Column> right)
    {
      var resp = new CompareResult<Column>();
      resp.Additional = GetTableMissing(right, left).ToList();
      foreach (var l in GetTableIntersect(left, right))
      {
        var r = right.FirstOrDefault(x => x.GetTableKey().Equals(l.GetTableKey(), StringComparison.OrdinalIgnoreCase) && x.Name.Equals(l.Name, StringComparison.OrdinalIgnoreCase));
        if (r == null) resp.Additional.Add(l);
        else if (CEC.Equals(l, r)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<Column, Column>(l, r));
      }
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
        else resp.Different.Add(new Tuple<Schema, Schema>(l, r));
      }
      resp.Missing = rightNames.Except(leftNames).Select(p => right.First(r => r.Name.Equals(p))).ToList();
      resp.Additional = leftNames.Except(rightNames).Select(p => left.First(r => r.Name.Equals(p))).ToList();
      return resp;
    }

    protected CompareResult<ExtendedProperty> CompareExtendedProperties(IEnumerable<ExtendedProperty> left, IEnumerable<ExtendedProperty> right)
    {
      var resp = new CompareResult<ExtendedProperty>();
      resp.Additional = GetExtendedPropertyMissing(right, left).ToList();
      foreach (var l in GetExtendedPropertyIntersect(left, right))
      {
        var r = right.FirstOrDefault(x => GetExtendedPropertyMatch(x, l));
        if (r == null) resp.Missing.Add(l);
        else if (EPEC.Equals(l, r)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<ExtendedProperty, ExtendedProperty>(l, r));
      }
      resp.Missing = GetExtendedPropertyMissing(left, right).ToList();
      return resp;
    }

    protected CompareResult<UserType> CompareUserTypes(List<UserType> left, List<UserType> right)
    {
      var resp = new CompareResult<UserType>();
      resp.Additional = GetBaseMissing(right, left).ToList();
      foreach (UserType l in left.Intersect(right, BEC))
      {
        var r = right.FirstOrDefault(x => x.GetKey().Equals(l.GetKey()));
        if (r == null) resp.Missing.Add(l);
        else if (l.IsNullable == r.IsNullable && l.MaxLength == r.MaxLength && l.Type.Equals(r.Type, StringComparison.OrdinalIgnoreCase)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<UserType, UserType>(l, r));
      }
      resp.Missing = GetBaseMissing(left, right).ToList();
      return resp;
    }

    protected CompareResult<T> CompareDefinition<T>(List<T> left, List<T> right) where T : DefinitionBased
    {
      var resp = new CompareResult<T>();
      foreach (T l in GetBaseIntersect(left, right))
      {
        var r = right.First(x => x.GetKey().Equals(l.GetKey(), StringComparison.OrdinalIgnoreCase));
        if (l.ReplaceVars(vars).Equals(r.ReplaceVars(vars), StringComparison.OrdinalIgnoreCase)) resp.Same.Add(l);
        else resp.Different.Add(new Tuple<T, T>(l, r));
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

    private IEnumerable<ExtendedProperty> GetExtendedPropertyMissing(IEnumerable<ExtendedProperty> left, IEnumerable<ExtendedProperty> right)
    {
      return right.Where(r => !left.Any(x => GetExtendedPropertyMatch(r, x)));
    }

    private IEnumerable<ExtendedProperty> GetExtendedPropertyIntersect(IEnumerable<ExtendedProperty> left, IEnumerable<ExtendedProperty> right)
    {
      return left.Where(r => right.Any(x => GetExtendedPropertyMatch(r, x)));
    }

    private bool GetExtendedPropertyMatch(ExtendedProperty x, ExtendedProperty r)
    {
      return (x.Name ?? "").Equals(r.Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level0Name ?? "").Equals(r.Level0Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level0Type ?? "").Equals(r.Level0Type ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level1Name ?? "").Equals(r.Level1Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level1Type ?? "").Equals(r.Level1Type ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level2Name ?? "").Equals(r.Level2Name ?? "", StringComparison.OrdinalIgnoreCase) &&
             (x.Level2Type ?? "").Equals(r.Level2Type ?? "", StringComparison.OrdinalIgnoreCase);
    }
  }
}
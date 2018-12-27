using System.Collections.Generic;
using VerQL.Core.Models;

namespace VerQL.Core.Comparer
{
  public class CompareResponse
  {
    public CompareResult<Procedure> Procedures { get; set; } = new CompareResult<Procedure>();
    public CompareResult<View> Views { get; set; } = new CompareResult<View>();
    public CompareResult<Function> Functions { get; set; } = new CompareResult<Function>();
    public CompareResult<Schema> Schemas { get; set; } = new CompareResult<Schema>();
    public CompareResult<UserType> UserTypes { get; set; } = new CompareResult<UserType>();
    public CompareResult<Trigger> Triggers { get; set; } = new CompareResult<Trigger>();
    public CompareResult<Table> Tables { get; set; } = new CompareResult<Table>();
    public CompareResult<Column> Columns { get; set; } = new CompareResult<Column>();
    public CompareResult<PrimaryKeyConstraint> PrimaryKeyConstraints { get; set; } = new CompareResult<PrimaryKeyConstraint>();
    public CompareResult<ForeignKeyConstraint> ForeignKeyConstraints { get; set; } = new CompareResult<ForeignKeyConstraint>();
    public CompareResult<UniqueConstraint> UniqueConstraints { get; set; } = new CompareResult<UniqueConstraint>();
    public CompareResult<Index> Indexs { get; set; } = new CompareResult<Index>();
  }
}
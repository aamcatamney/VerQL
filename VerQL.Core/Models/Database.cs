using System.Collections.Generic;

namespace VerQL.Core.Models
{
  public class Database
  {
    public List<Table> Tables { get; set; } = new List<Table>();
    public List<Column> Columns { get; set; } = new List<Column>();
    public List<PrimaryKeyConstraint> PrimaryKeyConstraints { get; set; } = new List<PrimaryKeyConstraint>();
    public List<UniqueConstraint> UniqueConstraints { get; set; } = new List<UniqueConstraint>();
    public List<ForeignKeyConstraint> ForeignKeyConstraints { get; set; } = new List<ForeignKeyConstraint>();
    public List<Procedure> Procedures { get; set; } = new List<Procedure>();
    public List<Function> Functions { get; set; } = new List<Function>();
    public List<View> Views { get; set; } = new List<View>();
    public List<Trigger> Triggers { get; set; } = new List<Trigger>();
    public List<UserType> UserTypes { get; set; } = new List<UserType>();
    public List<Schema> Schemas { get; set; } = new List<Schema>();
    public List<Index> Indexs { get; set; } = new List<Index>();
  }
}
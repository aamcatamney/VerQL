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
        public Dictionary<Table, CompareResult<Column>> Columns { get; set; } = new Dictionary<Table, CompareResult<Column>>();
    }
}
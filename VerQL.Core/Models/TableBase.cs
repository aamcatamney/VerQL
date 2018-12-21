namespace VerQL.Core.Models
{
  public abstract class TableBase
  {
    public string TableSchema { get; set; } = "dbo";
    public string TableName { get; set; }
  }
}
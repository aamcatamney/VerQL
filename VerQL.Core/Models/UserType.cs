namespace VerQL.Core.Models
{
  public class UserType
  {
    public string Schema { get; set; } = "dbo";
    public string Name { get; set; }
    public string Definition { get; set; }
  }
}
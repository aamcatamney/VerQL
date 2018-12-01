namespace VerQL.Core.Models
{
    public abstract class Base
    {
        public string Schema { get; set; } = "dbo";
        public string Name { get; set; }
    }
}
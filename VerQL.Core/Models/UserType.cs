namespace VerQL.Core.Models
{
    public class UserType : Base
    {
        public string Type { get; set; }
        public int MaxLength { get; set; }
        public bool IsNullable { get; set; }
    }
}
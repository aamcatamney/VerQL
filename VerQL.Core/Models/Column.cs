namespace VerQL.Core.Models
{
    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int MaxLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsComputed { get; set; }
        public string ComputedText { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIdentity { get; set; }
        public int SeedValue { get; set; } = 1;
        public int IncrementValue { get; set; } = 1;
        public bool HasDefault { get; set; }
        public string DefaultText { get; set; }
    }
}
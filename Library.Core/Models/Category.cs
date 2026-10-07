namespace Library.Core.Models
{
    public readonly record struct CategoryId(ushort Value);

    public readonly record struct Category
    {
        public CategoryId Id { get; init; }
        public int NameOffset { get; init; }
        public int NameLength { get; init; }
        
    }
}
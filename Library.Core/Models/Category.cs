using System.Runtime.InteropServices;

namespace Library.Core.Models
{
    public readonly record struct CategoryId(ushort Value);

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Category
    {
        public CategoryId Id { get; init; }
        public int NameOffset { get; init; }
        public int NameLength { get; init; }
        
    }
}
using System.Runtime.InteropServices;

namespace Library.Core.Models
{
    public readonly record struct BookId(int Value);

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Book
    {
        public long Isbn { get; init; }
        public BookId Id { get; init; }
        public int TitleOffset { get; init; }
        public int TitleLength { get; init; }
        public int AuthorOffset { get; init; }
        public int AuthorLength { get; init; }
        public ushort Year { get; init; }
        public CategoryId CategoryId { get; init; }
    }
}

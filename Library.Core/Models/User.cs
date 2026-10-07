using System.Runtime.InteropServices;

namespace Library.Core.Models
{
    public readonly record struct UserId(int Value);

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct User
    {
        public UserId Id { get; init; }
        public int NameOffset { get; init; }
        public int NameLength { get; init; }
    }
}
